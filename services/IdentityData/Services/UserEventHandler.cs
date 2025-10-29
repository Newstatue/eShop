using System.Text.Json;

using IdentityData.Data;
using IdentityData.Models;

using ServiceDefaults.Messaging.Events;

namespace IdentityData.Services;

public class UserEventHandler(ILogger<UserEventHandler> logger, IdentityDbContext dbContext) : IUserEventHandler
{
    public async Task HandleLoginAsync(KeycloakWebhookIntegrationEvent evt)
    {
        var payload = TryDeserialize(evt.RawPayload);
        if (payload is null)
        {
            logger.LogWarning("无法解析登录事件 JSON。Payload={Payload}", evt.RawPayload);
            return;
        }

        var auth = payload.AuthDetails;
        if (string.IsNullOrWhiteSpace(auth.UserId))
        {
            logger.LogWarning("登录事件缺少 userId，跳过。");
            return;
        }

        var loginTime = DateTimeOffset.FromUnixTimeMilliseconds(payload.Time).UtcDateTime;
        var user = await dbContext.Users.FindAsync(auth.UserId);
        if (user is null)
        {
            logger.LogInformation("用户 {UserId} 登录成功但尚未注册。", auth.UserId);
            return;
        }

        user.LastLoginAt = loginTime;
        user.LastLoginIp = auth.IpAddress;
        user.LastSessionId = auth.SessionId;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        logger.LogInformation("用户 {Username} 登录成功 (IP={Ip})", auth.Username, auth.IpAddress);
    }

    public async Task HandleRegisterAsync(KeycloakWebhookIntegrationEvent evt)
    {
        var payload = TryDeserialize(evt.RawPayload);
        if (payload is null)
        {
            logger.LogWarning("无法解析注册事件 JSON。Payload={Payload}", evt.RawPayload);
            return;
        }

        var auth = payload.AuthDetails;
        if (string.IsNullOrWhiteSpace(auth.UserId))
        {
            logger.LogWarning("注册事件缺少 userId，跳过。");
            return;
        }

        var uid = payload.Uid;
        if (await dbContext.ProcessedEvents.AnyAsync(e => e.Uid == uid))
        {
            logger.LogInformation("跳过重复注册事件 UID={Uid}", uid);
            return;
        }

        var existing = await dbContext.Users.FindAsync(auth.UserId);
        var username = auth.Username ?? "(unknown)";

        if (existing is null)
        {
            await dbContext.Users.AddAsync(new UserEntity
            {
                KeycloakId = auth.UserId,
                Username = username,
                RealmId = evt.RealmId,
                CreatedFromIp = auth.IpAddress,
                CreatedClientId = auth.ClientId,
                CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(payload.Time).UtcDateTime
            });
            logger.LogInformation("创建新用户 {Username} ({UserId})", username, auth.UserId);
        }
        else
        {
            existing.Username = username;
            existing.UpdatedAt = DateTime.UtcNow;
            logger.LogInformation("更新现有用户 {Username} ({UserId})", username, auth.UserId);
        }

        dbContext.ProcessedEvents.Add(new ProcessedEventEntity
        {
            Uid = uid, EventType = evt.EventType, ProcessedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private static KeycloakEventPayload? TryDeserialize(string raw)
    {
        try
        {
            return JsonSerializer.Deserialize<KeycloakEventPayload>(raw);
        }
        catch
        {
            return null;
        }
    }
}
