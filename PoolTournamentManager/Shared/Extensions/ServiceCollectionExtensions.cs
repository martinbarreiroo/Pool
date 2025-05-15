using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using PoolTournamentManager.Features.Matches.Services;
using PoolTournamentManager.Features.Players.Services;
using PoolTournamentManager.Features.Tournaments.Services;
using PoolTournamentManager.Shared.Infrastructure.Data;
using PoolTournamentManager.Shared.Infrastructure.Storage;

namespace PoolTournamentManager.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add AWS services
            services.AddAWSService<IAmazonS3>(configuration.GetAWSOptions());

            // Add database context
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Add application services
            services.AddScoped<S3StorageService>();
            services.AddScoped<PlayerService>();
            services.AddScoped<MatchService>();
            services.AddScoped<TournamentService>();

            // Add FluentValidation
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<Program>();

            return services;
        }
    }
}
