package com.evorsio.eshop;

import cn.dev33.satoken.stp.StpUtil;
import com.evorsio.eshop.common.BizConstants;
import com.evorsio.eshop.service.OrderService;
import com.evorsio.eshop.service.UserService;
import com.evorsio.eshop.vo.CreateOrderVo;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.data.redis.core.StringRedisTemplate;

import java.util.Collections;

import static org.junit.jupiter.api.Assertions.assertDoesNotThrow;
import static org.junit.jupiter.api.Assertions.assertNotNull;

@SpringBootTest
class EshopApplicationTests {

    @Test
    void contextLoads() {
    }

    @Autowired
    UserService userService;

    @Autowired
    StringRedisTemplate redisTemplate;

    // 测试发送验证码
    @Test
    void testSms(){
        String phone = "15532933883";
        userService.sendCode(phone);
        String s = redisTemplate.opsForValue().get(BizConstants.REDIS_KEY_SMS_CODE_PREFIX + phone);
        assertNotNull(s,"Redis上未找到验证码");
        System.out.println("================测试成功=================");
        System.out.println("手机号："+phone+" 验证码："+s);
        System.out.println("========================================");
    }



    @Autowired
    OrderService orderService;

    // 测试下单
    @Test
    void testOrder(){
        CreateOrderVo vo = new CreateOrderVo();
        CreateOrderVo.OrderItemVo item = new CreateOrderVo.OrderItemVo();
        item.setSkuId(1L);
        item.setQuantity(1);
        vo.setItems(Collections.singletonList(item));
        StpUtil.login(1);
        assertDoesNotThrow(() -> orderService.createOrder(vo));
        System.out.println("================测试成功=================");
    }
}
