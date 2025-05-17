using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PoolTournamentManager.Shared.Infrastructure.Data
{
    public static class MigrationManager
    {
        /// <summary>
        /// Apply migrations based on the environment and database provider
        /// </summary>
        public static IHost MigrateDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
                var environment = services.GetRequiredService<IHostEnvironment>();

                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();

                    // The context has already been configured with the correct provider
                    // in Program.cs, so we just need to run the migrations
                    logger.LogInformation("Applying migrations for {EnvironmentName} environment",
                        environment.EnvironmentName);

                    context.Database.Migrate();

                    logger.LogInformation("Database migrations applied successfully");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while applying database migrations");
                    throw;
                }
            }

            return host;
        }
    }
}