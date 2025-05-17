# Multi-Database Provider Migration System

This application supports both SQL Server (for Production) and PostgreSQL (for Development) database providers. To manage migrations for both providers, we've set up a special migration system.

## How It Works

- The application uses SQL Server in Production and PostgreSQL in Development
- Each database provider has its own set of migrations in separate directories
- The application automatically applies the correct migrations based on the environment
- EF Core's `MigrationsAssembly()` is used to specify the assembly containing migrations for each provider

## Directory Structure

```
Migrations/
  ├── PostgreSQL/    # PostgreSQL migrations for Development
  └── SqlServer/     # SQL Server migrations for Production
```

## Using the Migrations Tool

We've created a `migrations-tool.sh` script to simplify working with multiple migration sets:

### Create PostgreSQL Migrations (Development)

```bash
./migrations-tool.sh add-postgres "MigrationName"
```

### Create SQL Server Migrations (Production)

```bash
./migrations-tool.sh add-sqlserver "MigrationName"
```

### Update PostgreSQL Database (Development)

```bash
./migrations-tool.sh update-postgres
```

### Update SQL Server Database (Production)

```bash
./migrations-tool.sh update-sqlserver
```

## Important Notes

1. When making schema changes, you must create migrations for **both** providers
2. Each provider may generate different migrations due to database-specific features
3. Test migrations on both providers before deploying to production
4. When switching between providers locally, you may need to drop and recreate your database

## Typical Workflow

1. Make changes to your entity models
2. Create migrations for both providers:
   ```bash
   ./migrations-tool.sh add-postgres "YourChange"
   ./migrations-tool.sh add-sqlserver "YourChange"
   ```
3. Test migrations in development (PostgreSQL):
   ```bash
   ./migrations-tool.sh update-postgres
   ```
4. Before deploying, test migrations with SQL Server locally
5. Deploy to production, where SQL Server migrations will be applied 