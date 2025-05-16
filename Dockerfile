# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder

WORKDIR /app

# Copy solution and project files
COPY *.sln .
COPY PoolTournamentManager/*.csproj ./PoolTournamentManager/
COPY PoolTournamentManager.Tests/*.csproj ./PoolTournamentManager.Tests/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the code
COPY . .

# Build and publish
RUN dotnet publish PoolTournamentManager/PoolTournamentManager.csproj -c Release -o /app/publish

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runner

WORKDIR /app

# Set environment to production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_URLS=http://+:80
ENV PORT=80

# Copy the published application from the builder stage
COPY --from=builder /app/publish .

# Expose the API port
EXPOSE 80

# Start the application
ENTRYPOINT ["dotnet", "PoolTournamentManager.dll"]