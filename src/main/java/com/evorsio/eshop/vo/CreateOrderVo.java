package com.evorsio.eshop.vo;

import jakarta.validation.Valid;
import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.NotEmpty;
import jakarta.validation.constraints.NotNull;
import lombok.Data;

import java.util.List;

/**
 * @author Evorsio
 * @since 2026/6/17
 */
@Data
public class CreateOrderVo {
    @NotEmpty(message = "订单项不能为空")
    @Valid
    private List<OrderItemVo> items;

    @Data
    public static class OrderItemVo {
        @NotNull(message = "skuId不能为空")
        private Long skuId;

        @NotNull(message = "数量不能为空")
        @Min(value = 1, message = "数量至少为1")
        private Integer quantity;
    }
}
