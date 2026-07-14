package com.evorsio.eshop.service.impl;

import cn.dev33.satoken.stp.StpUtil;
import com.baomidou.mybatisplus.core.toolkit.Wrappers;
import com.baomidou.mybatisplus.extension.service.impl.ServiceImpl;
import com.evorsio.eshop.common.BizCode;
import com.evorsio.eshop.common.BizConstants;
import com.evorsio.eshop.common.BizException;
import com.evorsio.eshop.common.BizProperties;
import com.evorsio.eshop.domain.*;
import com.evorsio.eshop.mapper.*;
import com.evorsio.eshop.service.OrderService;
import com.evorsio.eshop.util.OrderUtil;
import com.evorsio.eshop.vo.CreateOrderVo;
import com.evorsio.eshop.vo.OrderVo;
import jakarta.annotation.Resource;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.apache.rocketmq.spring.core.RocketMQTemplate;
import org.redisson.api.RLock;
import org.redisson.api.RedissonClient;
import org.springframework.context.annotation.Lazy;
import org.springframework.data.redis.core.StringRedisTemplate;
import org.springframework.data.redis.core.script.DefaultRedisScript;
import org.springframework.messaging.support.MessageBuilder;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.math.BigDecimal;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.concurrent.TimeUnit;

/**
 * @author Admin
 * @description 针对表【order】的数据库操作Service实现
 * @createDate 2026-06-16 00:58:31
 */
@Slf4j
@Service
@RequiredArgsConstructor
public class OrderServiceImpl extends ServiceImpl<OrderMapper, Order>
        implements OrderService {

    private final SkuMapper skuMapper;
    private final SpuMapper spuMapper;
    private final OrderItemMapper orderItemMapper;
    private final StockLogMapper stockLogMapper;
    private final StringRedisTemplate redisTemplate;
    private final RedissonClient redissonClient;
    private final DefaultRedisScript<List> deductStockScript;
    private final RocketMQTemplate mqTemplate;
    private final BizProperties bizProperties;

    @Lazy
    @Resource
    private OrderService self;

    @Override
    public OrderVo createOrder(CreateOrderVo vo) {
        long userId = StpUtil.getLoginIdAsLong();
        String orderNo = OrderUtil.generateOrderNo();

        // 1. 分布式锁防止重复下单
        RLock lock = redissonClient.getLock(
                BizConstants.REDIS_KEY_LOCK_ORDER_PREFIX + userId
        );
        boolean locked;
        try {
            locked = lock.tryLock(0, 10, TimeUnit.SECONDS);
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            throw new BizException(BizCode.FAIL);
        }

        if (!locked) {
            throw new BizException(BizCode.FAIL);
        }

        try {
            List<StockLog> stockLogs = new ArrayList<>();
            List<CreateOrderVo.OrderItemVo> deducted = new ArrayList<>();

            try {
                // 2. lua原子扣减
                for (CreateOrderVo.OrderItemVo item : vo.getItems()) {
                    String key = BizConstants.REDIS_KEY_SKU_STOCK_PREFIX + item.getSkuId();

                    @SuppressWarnings("unchecked")
                    List<Long> r = (List<Long>) redisTemplate.execute(
                            deductStockScript,
                            Collections.singletonList(key),
                            String.valueOf(item.getQuantity())
                    );

                    long code = r.get(0);
                    long before = r.get(1);
                    long deduct = r.get(2);
                    long after = r.get(3);

                    if (code == -1) {
                        throw new BizException(BizCode.PRODUCT_NOT_FOUND);
                    }
                    if (code == 0) {
                        throw new BizException(BizCode.FAIL);
                    }

                    deducted.add(item);

                    StockLog stockLog = new StockLog();
                    stockLog.setSkuId(item.getSkuId());
                    stockLog.setOrderNo(orderNo);
                    stockLog.setBeforeQty((int) before);
                    stockLog.setDeductQty((int) deduct);
                    stockLog.setAfterQty((int) after);
                    stockLogs.add(stockLog);

                    log.info("库存扣减 skuId={} before={} deduct={} after={}",
                            item.getSkuId(), before, deduct, after);

                }

                OrderVo result = self.doCreateOrder(vo, userId, orderNo, stockLogs);

                // 3.订单提交发送延迟消息
                mqTemplate.syncSend(
                        BizConstants.MQ_TOPIC_ORDER_CANCEL,
                        MessageBuilder.withPayload(orderNo).build(),
                        3000,
                        bizProperties.getOrder().getCancelDelayLevel()
                );

                return result;
            } catch (RuntimeException e) {
                // 4.下单失败回滚库存
                for (CreateOrderVo.OrderItemVo item : deducted) {
                    redisTemplate.opsForValue().increment(
                            BizConstants.REDIS_KEY_SKU_STOCK_PREFIX + item.getSkuId(),
                            item.getQuantity()
                    );
                }
                throw e;
            }
        } finally {
            if (lock.isHeldByCurrentThread()) {
                lock.unlock();
            }
        }
    }

    @Override
    @Transactional(rollbackFor = Exception.class)
    public OrderVo doCreateOrder(CreateOrderVo vo, Long userId, String orderNo, List<StockLog> stockLogs) {
        BigDecimal totalAmount = BigDecimal.ZERO;
        List<OrderItem> orderItems = new ArrayList<>();

        for (CreateOrderVo.OrderItemVo item : vo.getItems()) {
            Sku sku = skuMapper.selectById(item.getSkuId());
            Spu spu = spuMapper.selectById(sku.getSpuId());

            skuMapper.update(
                    null,
                    Wrappers.<Sku>lambdaUpdate()
                            .setSql("stock = stock - {0}", item.getQuantity())
                            .eq(Sku::getId, item.getSkuId())
            );

            OrderItem orderItem = new OrderItem();
            orderItem.setSkuId(sku.getId());
            orderItem.setSkuSpec(sku.getSpec());
            orderItem.setSpuName(spu.getName());
            orderItem.setPrice(sku.getPrice());
            orderItem.setQuantity(item.getQuantity());
            orderItems.add(orderItem);

            totalAmount = totalAmount.add(
                    sku.getPrice().multiply(BigDecimal.valueOf(item.getQuantity()))
            );
        }

        Order order = new Order();
        order.setOrderNo(orderNo);
        order.setUserId(userId);
        order.setTotalAmount(totalAmount);
        order.setStatus(0);
        this.save(order);

        for (OrderItem orderItem1 : orderItems) {
            orderItem1.setOrderId(order.getId());
            orderItemMapper.insert(orderItem1);
        }

        // 写库存流水日志
        stockLogs.forEach(stockLogMapper::insert);

        OrderVo result = new OrderVo();
        result.setOrderId(order.getId());
        result.setOrderNo(orderNo);
        result.setTotalAmount(totalAmount);
        return result;
    }

    @Override
    public void payOrder(String orderNo) {
        long userId = StpUtil.getLoginIdAsLong();

        boolean update = this.update(
                Wrappers.<Order>lambdaUpdate()
                        .set(Order::getStatus, 1)
                        .eq(Order::getOrderNo, orderNo)
                        .eq(Order::getUserId, userId)
                        .eq(Order::getStatus, 0)
        );

        if (!update) {
            throw new BizException(BizCode.FAIL);
        }

        log.info("订单支付成功 orderNo={}", orderNo);
    }

    @Override
    @Transactional(rollbackFor = Exception.class)
    public void cancelIfUnpaid(String orderNo) {
        boolean update = this.update(
                Wrappers.<Order>lambdaUpdate()
                        .set(Order::getStatus, 2)
                        .eq(Order::getOrderNo, orderNo)
                        .eq(Order::getStatus, 0)
        );
        if (!update) {
            log.info("订单已支付 orderNo={}", orderNo);
            return;
        }

        log.info("订单超时未支付，回滚库存 orderNo={}", orderNo);
        Order order = this.getOne(
                Wrappers.<Order>lambdaQuery()
                        .eq(Order::getOrderNo, orderNo)
        );
        List<OrderItem> items = orderItemMapper.selectList(
                Wrappers.<OrderItem>lambdaQuery()
                        .eq(OrderItem::getOrderId, order.getId())
        );

        for (OrderItem item : items) {
            skuMapper.update(null,
                    Wrappers.<Sku>lambdaUpdate()
                            .setSql("stock = stock + {0}", item.getQuantity())
                            .eq(Sku::getId, item.getSkuId())
            );

            redisTemplate.opsForValue().increment(
                    BizConstants.REDIS_KEY_SKU_STOCK_PREFIX + item.getSkuId(),
                    item.getQuantity()
            );

            log.info("回滚库存成功 skuId={} qty={}", item.getSkuId(), item.getQuantity());
        }
    }
}




