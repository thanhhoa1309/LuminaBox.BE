using Microsoft.EntityFrameworkCore;
using LuminaBox.Infrastructure.Persistence;

namespace LuminaBox.Api.Architecture
{
    /// <summary>
    /// Applies migrations that already exist in the Infrastructure project.
    /// It never creates migrations; those are added only via
    /// <c>dotnet ef migrations add &lt;Name&gt; --project LuminaBox.Infrastructure --startup-project LuminaBox.Api</c>.
    /// </summary>
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this WebApplication app, ILogger logger)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LuminaBoxDbContext>();

            var definedMigrations = dbContext.Database.GetMigrations().ToList();
            if (definedMigrations.Count == 0)
            {
                logger.LogInformation("No EF migrations defined yet. Skipping database migrate.");
                return;
            }

            var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
            if (pendingMigrations.Count == 0)
            {
                logger.LogInformation("Database is up to date. No pending migrations.");
                return;
            }

            logger.LogInformation(
                "Applying {Count} pending migration(s): {Migrations}",
                pendingMigrations.Count,
                string.Join(", ", pendingMigrations));

            dbContext.Database.Migrate();
        }
    }
}
