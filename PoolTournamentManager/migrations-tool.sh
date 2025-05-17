#!/bin/bash

# Script to generate provider-specific migrations for both PostgreSQL and SQL Server

# Usage examples:
# ./migrations-tool.sh add-postgres "InitialMigration"    - Add a new PostgreSQL migration
# ./migrations-tool.sh add-sqlserver "InitialMigration" - Add a new SQL Server migration
# ./migrations-tool.sh update-postgres                    - Update PostgreSQL database
# ./migrations-tool.sh update-sqlserver                - Update SQL Server database

# Make sure the migration output directories exist
mkdir -p Migrations/PostgreSQL
mkdir -p Migrations/SqlServer

# Function to show usage
show_usage() {
    echo "Usage:"
    echo "  $0 add-postgres <migration-name>     - Add a new PostgreSQL migration"
    echo "  $0 add-sqlserver <migration-name> - Add a new SQL Server migration"
    echo "  $0 update-postgres                   - Update PostgreSQL database"
    echo "  $0 update-sqlserver               - Update SQL Server database"
}

# Handle command
case "$1" in
    add-postgres)
        if [ -z "$2" ]; then
            echo "Error: Migration name is required"
            show_usage
            exit 1
        fi
        export ASPNETCORE_ENVIRONMENT=Development
        dotnet ef migrations add "$2" --context ApplicationDbContext --output-dir Migrations/PostgreSQL --project PoolTournamentManager.csproj
        ;;
    add-sqlserver)
        if [ -z "$2" ]; then
            echo "Error: Migration name is required"
            show_usage
            exit 1
        fi
        export ASPNETCORE_ENVIRONMENT=Production
        dotnet ef migrations add "$2" --context ApplicationDbContext --output-dir Migrations/SqlServer --project PoolTournamentManager.csproj
        ;;
    update-postgres)
        export ASPNETCORE_ENVIRONMENT=Development
        dotnet ef database update --context ApplicationDbContext --project PoolTournamentManager.csproj
        ;;
    update-sqlserver)
        export ASPNETCORE_ENVIRONMENT=Production
        dotnet ef database update --context ApplicationDbContext --project PoolTournamentManager.csproj
        ;;
    *)
        show_usage
        exit 1
        ;;
esac 