using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Amazon.S3;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using dotenv.net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// Load .env file only in Development environment
if (builder.Environment.IsDevelopment())
{
    DotEnv.Load();
}

string? connectionString = null;

if (builder.Environment.IsProduction())
{
    Console.WriteLine("Production environment detected. Attempting to load AZURE_POSTGRESQL_CONNECTIONSTRING from environment variables.");
    connectionString = Environment.GetEnvironmentVariable("AZURE_POSTGRESQL_CONNECTIONSTRING");

    if (string.IsNullOrEmpty(connectionString) || connectionString.StartsWith("${", StringComparison.OrdinalIgnoreCase))
    {
        Console.Error.WriteLine("FATAL ERROR: AZURE_POSTGRESQL_CONNECTIONSTRING is not configured or is a placeholder in Production environment.");
        Console.Error.WriteLine("Please ensure 'AZURE_POSTGRESQL_CONNECTIONSTRING' is correctly set in your production environment variables.");
        throw new InvalidOperationException("Database connection string is not properly configured for Production. Check console logs for details.");
    }
}
else // Non-Production environments (e.g., Development)
{
    Console.WriteLine("Non-Production environment detected. Trying AZURE_POSTGRESQL_CONNECTIONSTRING from env first, then appsettings.json DefaultConnection.");
    // Try AZURE_POSTGRESQL_CONNECTIONSTRING from environment variables first
    connectionString = Environment.GetEnvironmentVariable("AZURE_POSTGRESQL_CONNECTIONSTRING");

    // If AZURE_POSTGRESQL_CONNECTIONSTRING is not found or is a placeholder, try appsettings.json DefaultConnection
    if (string.IsNullOrEmpty(connectionString) || connectionString.StartsWith("${", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("AZURE_POSTGRESQL_CONNECTIONSTRING not found or is placeholder in env, trying DefaultConnection from appsettings.json.");
        string? defaultConnectionStringTemplate = builder.Configuration.GetSection("ConnectionStrings")["DefaultConnection"];

        if (!string.IsNullOrEmpty(defaultConnectionStringTemplate) && defaultConnectionStringTemplate.Contains("${"))
        {
            Console.WriteLine($"Connection string template from appsettings: {defaultConnectionStringTemplate}");
            // Manually resolve placeholders from environment variables
            string? dbHost = Environment.GetEnvironmentVariable("DB_HOST");
            string? dbName = Environment.GetEnvironmentVariable("DB_NAME");
            string? dbUser = Environment.GetEnvironmentVariable("DB_USER");
            string? dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

            if (!string.IsNullOrEmpty(dbHost) && !string.IsNullOrEmpty(dbName) && !string.IsNullOrEmpty(dbUser) && !string.IsNullOrEmpty(dbPassword))
            {
                connectionString = defaultConnectionStringTemplate
                    .Replace("${DB_HOST}", dbHost, StringComparison.OrdinalIgnoreCase)
                    .Replace("${DB_NAME}", dbName, StringComparison.OrdinalIgnoreCase)
                    .Replace("${DB_USER}", dbUser, StringComparison.OrdinalIgnoreCase)
                    .Replace("${DB_PASSWORD}", dbPassword, StringComparison.OrdinalIgnoreCase);
                Console.WriteLine($"Resolved connection string from template: {connectionString}");
            }
            else
            {
                Console.Error.WriteLine("One or more required environment variables (DB_HOST, DB_NAME, DB_USER, DB_PASSWORD) for DefaultConnection are missing.");
                connectionString = null; // Ensure it remains null or empty to trigger the error below
            }
        }
        else if (!string.IsNullOrEmpty(defaultConnectionStringTemplate))
        {
            // It's a non-placeholder string directly from appsettings.json
            connectionString = defaultConnectionStringTemplate;
            Console.WriteLine($"Using direct DefaultConnection from appsettings.json: {connectionString}");
        }
    }

    // Validate the final connection string for non-production
    if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("${")) // Check if any placeholder remains unresolved
    {
        Console.Error.WriteLine("FATAL ERROR: Database connection string is not configured or placeholders could not be resolved in non-Production environment.");
        Console.Error.WriteLine("Ensure AZURE_POSTGRESQL_CONNECTIONSTRING is set, or DefaultConnection in appsettings.json has its placeholders (e.g., ${DB_HOST}) resolvable from .env or environment variables.");
        throw new InvalidOperationException("Database connection string is not properly configured or placeholders unresolved. Check console logs.");
    }
}

builder.Services.AddDbContext<PoolTournamentManager.Shared.Infrastructure.Data.ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(30);
    });
});

Console.WriteLine($"Using connection string: {connectionString}");

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
builder.Services.AddScoped<PoolTournamentManager.Shared.Infrastructure.Storage.S3StorageService>();
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
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Use CORS middleware
app.UseCors("AllowLocalhost");

app.UseAuthorization();

app.MapControllers();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<PoolTournamentManager.Shared.Infrastructure.Data.ApplicationDbContext>();
        context.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw;
    }
}

app.Run();
