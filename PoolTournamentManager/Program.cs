using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Amazon.S3;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using dotenv.net;

// Load environment variables from .env file
DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { ".env" }));

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

var connectionString = builder.Configuration.GetValue<string>("AZURE_POSTGRESQL_CONNECTIONSTRING")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PoolTournamentManager.Shared.Infrastructure.Data.ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));


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

app.Run();
