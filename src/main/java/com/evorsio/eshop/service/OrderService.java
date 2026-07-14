package com.evorsio.eshop.service;

import com.baomidou.mybatisplus.extension.service.IService;
import com.evorsio.eshop.domain.Order;
import com.evorsio.eshop.domain.StockLog;
import com.evorsio.eshop.vo.CreateOrderVo;
import com.evorsio.eshop.vo.OrderVo;

import java.util.List;

/**
 * @author Admin
 * @description 针对表【order】的数据库操作Service
 * @createDate 2026-06-16 00:58:31
 */
public interface OrderService extends IService<Order> {

    OrderVo createOrder(CreateOrderVo vo);

    OrderVo doCreateOrder(CreateOrderVo vo, Long userId, String orderNo, List<StockLog> stockLogs);

    void payOrder(String orderNo);

    void cancelIfUnpaid(String orderNo);
}
