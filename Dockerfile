# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder

WORKDIR /app

# Copy csproj files and restore dependencies
COPY *.sln .
COPY PoolTournamentManager/*.csproj ./PoolTournamentManager/
COPY PoolTournamentManager.Tests/*.csproj ./PoolTournamentManager.Tests/
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build and publish the application
RUN dotnet publish PoolTournamentManager/PoolTournamentManager.csproj -c Release -o /app/publish --no-restore

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runner

WORKDIR /app

# Set environment to production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Copy the published application from the builder stage
COPY --from=builder /app/publish .

# Expose the API port
EXPOSE 80
EXPOSE 443

# Start the application
ENTRYPOINT ["dotnet", "PoolTournamentManager.dll"]