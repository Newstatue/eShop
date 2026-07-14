package com.evorsio.eshop.controller;

import com.evorsio.eshop.common.R;
import com.evorsio.eshop.service.OrderService;
import com.evorsio.eshop.vo.CreateOrderVo;
import com.evorsio.eshop.vo.OrderVo;
import lombok.RequiredArgsConstructor;
import org.springframework.validation.annotation.Validated;
import org.springframework.web.bind.annotation.*;

/**
 * @author Evorsio
 * @since 2026/6/16
 */
@RestController
@RequiredArgsConstructor
@RequestMapping("/order")
public class OrderController {
    private final OrderService orderService;

    @PostMapping("/create")
    public R<OrderVo> createOrder(@Validated @RequestBody CreateOrderVo vo) {
        OrderVo o = orderService.createOrder(vo);
        return R.ok(o);
    }

    @PostMapping("/pay/{orderNo}")
    public R<Void> payOrder(@PathVariable String orderNo) {
        orderService.payOrder(orderNo);
        return R.ok();
    }
}
