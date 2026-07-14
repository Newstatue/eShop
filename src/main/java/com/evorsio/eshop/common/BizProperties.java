package com.evorsio.eshop.common;

import lombok.Data;
import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.stereotype.Component;

/**
 * @author Evorsio
 * @since 2026/6/16
 */
@Data
@Component
@ConfigurationProperties(prefix = "biz")
public class BizProperties {
    private Sms sms = new Sms();
    private Order order = new Order();

    @Data
    public static class Sms {
        // 过期时间（秒）
        private long expire = 300;
        // 重发间隔（秒）
        private long interval = 60;
    }

    @Data
    public static class Order {
        private Integer cancelDelayLevel = 16;
    }
}
