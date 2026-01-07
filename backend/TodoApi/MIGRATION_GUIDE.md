# Database Migration Guide

## Overview

This guide explains how to migrate from In-Memory database to PostgreSQL and manage database migrations using Entity Framework Core.

## Prerequisites

- .NET 8 SDK installed
- PostgreSQL 12+ installed and running
- PostgreSQL connection credentials

## Initial Setup

### 1. Install PostgreSQL

**Windows:**
- Download from https://www.postgresql.org/download/windows/
- Install PostgreSQL with default settings
- Note the postgres user password

**Linux (Ubuntu/Debian):**
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

**macOS:**
```bash
brew install postgresql@14
brew services start postgresql@14
```

### 2. Create Database

```bash
# Connect to PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE "TodoDb";

# Create user (optional, for production)
CREATE USER todoapi_user WITH PASSWORD 'your_secure_password';
GRANT ALL PRIVILEGES ON DATABASE "TodoDb" TO todoapi_user;

# Exit psql
\q
```

### 3. Configure Connection String

Update `appsettings.Production.json` or set environment variable:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=TodoDb;Username=postgres;Password=your_password"
  },
  "Database": {
    "UseInMemory": false,
    "Provider": "PostgreSQL"
  }
}
```

Or set environment variable:
```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Database=TodoDb;Username=postgres;Password=your_password"
```

## Creating Migrations

### Initial Migration

```bash
cd backend/TodoApi
dotnet ef migrations add InitialCreate --output-dir Data/Migrations
```

This creates the initial migration based on your `ApplicationDbContext` model.

### Subsequent Migrations

After making changes to your models:

```bash
dotnet ef migrations add AddNewFeature --output-dir Data/Migrations
```

### Review Migration

Before applying, review the generated migration file in `Data/Migrations/`:

```bash
# View migration SQL
dotnet ef migrations script --output migration.sql
```

## Applying Migrations

### Automatic (Recommended)

Migrations are automatically applied when the application starts (configured in `Program.cs`).

### Manual Application

```bash
# Apply all pending migrations
dotnet ef database update

# Apply to specific migration
dotnet ef database update InitialCreate

# Generate SQL script (without applying)
dotnet ef migrations script --output migration.sql
```

### Production Deployment

For production, it's recommended to generate SQL scripts and apply them manually:

```bash
# Generate SQL script
dotnet ef migrations script --output production_migration.sql

# Review the script
cat production_migration.sql

# Apply to production database
psql -h production_host -U postgres -d TodoDb -f production_migration.sql
```

## Rolling Back Migrations

### Remove Last Migration (Before Applying)

```bash
dotnet ef migrations remove
```

### Rollback to Specific Migration

```bash
dotnet ef database update PreviousMigrationName
```

### Rollback All Migrations

```bash
dotnet ef database update 0
```

**Warning:** This will drop all tables. Use with caution!

## Migration Best Practices

1. **Test Locally First**
   - Always test migrations on a local copy of the database
   - Verify data integrity after migration

2. **Backup Before Migration**
   - Always backup the database before applying migrations in production
   - See `DATABASE_BACKUP.md` for backup procedures

3. **Review Generated Code**
   - Check migration files before applying
   - Ensure no data loss will occur
   - Test on staging environment first

4. **Use Transactions**
   - Migrations are automatically wrapped in transactions
   - If migration fails, it will rollback automatically

5. **Idempotent Migrations**
   - Design migrations to be safe to run multiple times
   - Check if objects exist before creating them

6. **Data Migrations**
   - For data transformations, create separate migration
   - Test data migration thoroughly
   - Consider creating backup before data migration

## Troubleshooting

### Migration Conflicts

If you have migration conflicts:

```bash
# Remove conflicting migration
dotnet ef migrations remove

# Recreate migration
dotnet ef migrations add MigrationName
```

### Connection Issues

Verify connection string:
```bash
# Test connection
psql -h localhost -U postgres -d TodoDb
```

### Migration Not Found

If migration files are missing:
```bash
# List migrations
dotnet ef migrations list

# If empty, create initial migration
dotnet ef migrations add InitialCreate
```

### Database State Mismatch

If database state doesn't match migrations:

```bash
# Check migration history
dotnet ef migrations list

# Reset database (WARNING: Deletes all data)
dotnet ef database drop
dotnet ef database update
```

## Environment-Specific Configuration

### Development

Uses In-Memory database by default:
```json
{
  "Database": {
    "UseInMemory": true
  }
}
```

### Testing

Uses In-Memory database for fast tests:
```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString()));
```

### Production

Uses PostgreSQL:
```json
{
  "Database": {
    "UseInMemory": false,
    "Provider": "PostgreSQL"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod_host;Database=TodoDb;Username=user;Password=pass"
  }
}
```

## Migration Scripts

### Example: Adding Index

```csharp
// In ApplicationDbContext.OnModelCreating
modelBuilder.Entity<Todo>()
    .HasIndex(e => e.UserId);

// Create migration
dotnet ef migrations add AddTodoUserIdIndex
```

### Example: Adding Column

```csharp
// In Todo model
public string? Notes { get; set; }

// Create migration
dotnet ef migrations add AddNotesToTodo
```

### Example: Renaming Column

```csharp
// Use migration builder
migrationBuilder.RenameColumn(
    name: "OldColumnName",
    table: "Todos",
    newName: "NewColumnName");
```

## CI/CD Integration

### GitHub Actions Example

```yaml
- name: Run Migrations
  run: |
    cd backend/TodoApi
    dotnet ef database update
  env:
    ConnectionStrings__DefaultConnection: ${{ secrets.DATABASE_CONNECTION }}
```

### Azure DevOps Example

```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run Database Migrations'
  inputs:
    command: 'custom'
    custom: 'ef'
    arguments: 'database update'
    workingDirectory: 'backend/TodoApi'
```

## Additional Resources

- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)

## Support

For migration issues, contact:
- Development Team: [Team Email]
- Database Administrator: [DBA Email]

