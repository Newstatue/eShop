using IdentityData.Models;

namespace IdentityData.Data;

public static class Extensions
{
    public static void UseMigration(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        context.Database.Migrate();

        SeedTestUser(context);
    }

    private static void SeedTestUser(IdentityDbContext context)
    {
        const string testUserId = "3dd84bea-f505-402f-be94-bf0b1b3ac916";
        const string registerEventUid = "60f26d5f-9c6d-4f6d-88af-5f8a3c21d0e1";

        if (!context.Users.Any(u => u.KeycloakId == testUserId))
        {
            context.Users.Add(new UserEntity
            {
                KeycloakId = testUserId,
                Username = "test",
                RealmId = "4f76679d-ef29-4a59-bdf8-7d4e12fa0ff7",
                CreatedClientId = "webapp-client",
                CreatedFromIp = "172.19.0.1",
                CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(1761916116006).UtcDateTime,
                EmailVerified = false
            });
        }

        if (!context.ProcessedEvents.Any(e => e.Uid == registerEventUid))
        {
            context.ProcessedEvents.Add(new ProcessedEventEntity
            {
                Uid = registerEventUid, EventType = "access.REGISTER", ProcessedAt = DateTime.UtcNow
            });
        }

        context.SaveChanges();
    }
}
