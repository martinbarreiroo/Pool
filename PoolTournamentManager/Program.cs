using Microsoft.EntityFrameworkCore;
using Amazon.S3;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using dotenv.net;
using PoolTournamentManager.Shared.Infrastructure.Data;
using PoolTournamentManager.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Add Swagger with OpenAPI
builder.Services.AddSwaggerDocumentation();
builder.Services.AddEndpointsApiExplorer();

// Load .env file only in Development environment
if (builder.Environment.IsDevelopment())
{
    DotEnv.Load();
}

string connectionString;

// Use SQL Server for all environments, with different configuration sources
if (builder.Environment.IsDevelopment())
{
    Console.WriteLine("Development environment: Using SQL Server from .env");

    // Get SQL Server connection values from .env or Docker environment
    string? dbHost = Environment.GetEnvironmentVariable("SQLSERVER_SA_HOST") ?? "localhost";
    string? dbPort = Environment.GetEnvironmentVariable("SQLSERVER_PORT") ?? "1433";
    string? dbName = Environment.GetEnvironmentVariable("SQLSERVER_DATABASE") ?? "pool-tournament-manager-db";
    string? dbUser = Environment.GetEnvironmentVariable("SQLSERVER_USER") ?? "sa";
    string? dbPassword = Environment.GetEnvironmentVariable("SQLSERVER_SA_PASSWORD");

    if (string.IsNullOrEmpty(dbPassword))
    {
        Console.Error.WriteLine("FATAL ERROR: SQLSERVER_SA_PASSWORD is not set in .env file for development.");
        throw new InvalidOperationException("SQL Server password is not set in .env file.");
    }

    // Build SQL Server connection string for development
    connectionString = $"Server={dbHost},{dbPort};Database={dbName};User ID={dbUser};Password={dbPassword};TrustServerCertificate=True;";
}
// In Production or other environments, use SQL Server with Azure variables
else
{
    Console.WriteLine("Production environment: Using SQL Server from Azure variables");

    // Get SQL Server connection values from Azure environment variables
    string? dbHost = Environment.GetEnvironmentVariable("AZURE_SQL_HOST");
    string? dbName = Environment.GetEnvironmentVariable("AZURE_SQL_DATABASE");
    string? dbUser = Environment.GetEnvironmentVariable("AZURE_SQL_USERNAME");
    string? dbPassword = Environment.GetEnvironmentVariable("AZURE_SQL_PASSWORD");
    string? dbPort = Environment.GetEnvironmentVariable("AZURE_SQL_PORT") ?? "1433"; // Default SQL Server port

    if (string.IsNullOrEmpty(dbHost) ||
        string.IsNullOrEmpty(dbName) ||
        string.IsNullOrEmpty(dbUser) ||
        string.IsNullOrEmpty(dbPassword))
    {
        Console.Error.WriteLine("FATAL ERROR: One or more required Azure SQL environment variables are missing.");
        Console.Error.WriteLine("Please ensure AZURE_SQL_HOST, AZURE_SQL_DATABASE, AZURE_SQL_USERNAME, AZURE_SQL_PASSWORD are set.");
        throw new InvalidOperationException("Database connection string cannot be built due to missing environment variables.");
    }

    // Build SQL Server connection string
    connectionString = $"Server={dbHost},{dbPort};Database={dbName};User ID={dbUser};Password={dbPassword};TrustServerCertificate=True;";
}

// Register SQL Server provider for all environments
builder.Services.AddDbContext<PoolTournamentManager.Shared.Infrastructure.Data.ApplicationDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(connectionString, sqlServerOptions =>
    {
        sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        sqlServerOptions.CommandTimeout(30);
    });
}, ServiceLifetime.Scoped);

Console.WriteLine($"Using SQL Server connection: Server={connectionString.Split(';')[0].Split('=')[1]}");

// Register AWS services with environment variables taking precedence
var awsOptions = new AWSOptions();

// Get region from environment or config
string? awsRegion = Environment.GetEnvironmentVariable("AWS_REGION");
if (!string.IsNullOrEmpty(awsRegion))
{
    awsOptions.Region = Amazon.RegionEndpoint.GetBySystemName(awsRegion);
    Console.WriteLine($"Using AWS region from environment: {awsRegion}");
}
else
{
    var configRegion = builder.Configuration["AWS:Region"];
    if (!string.IsNullOrEmpty(configRegion))
    {
        awsOptions.Region = Amazon.RegionEndpoint.GetBySystemName(configRegion);
        Console.WriteLine($"Using AWS region from config: {configRegion}");
    }
}

// Get credentials from environment
string? awsAccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
string? awsSecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
if (!string.IsNullOrEmpty(awsAccessKey) && !string.IsNullOrEmpty(awsSecretKey))
{
    awsOptions.Credentials = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
    Console.WriteLine("Using AWS credentials from environment variables");
}
else
{
    Console.WriteLine("WARNING: AWS credentials not found in environment variables");
}

builder.Services.AddAWSService<IAmazonS3>(awsOptions);

// Register application services
// Register storage service with interface
builder.Services.AddScoped<PoolTournamentManager.Shared.Infrastructure.Storage.IStorageService, PoolTournamentManager.Shared.Infrastructure.Storage.S3StorageService>();
builder.Services.AddScoped<PoolTournamentManager.Features.Players.Services.PlayerService>();
builder.Services.AddScoped<PoolTournamentManager.Features.Matches.Services.MatchService>();
builder.Services.AddScoped<PoolTournamentManager.Features.Tournaments.Services.TournamentService>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",  // React default
                "http://localhost:5173",  // Vite default
                "http://localhost:5260",  // .NET app
                "http://localhost:4200"   // Angular default
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable Swagger in all environments
app.UseOpenApi();

app.UseHttpsRedirection();

// Use CORS middleware
app.UseCors("AllowLocalhost");

app.UseAuthorization();

app.MapControllers();

// Apply migrations on startup
app.MigrateDatabase();

app.Run();
