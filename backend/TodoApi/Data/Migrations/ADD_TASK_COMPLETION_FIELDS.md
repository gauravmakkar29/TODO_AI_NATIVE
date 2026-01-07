# Migration: Add Task Completion and Status Management Fields

This migration adds the following fields to support the Task Completion and Status Management feature (AIRE-19):

## New Fields Added to Todo Table

1. **Status** (int) - Enum field: 0 = Pending, 1 = Completed, 2 = Archived
2. **IsArchived** (bool) - Soft delete flag for archived tasks
3. **CompletedAt** (DateTime, nullable) - Timestamp when task was completed
4. **ArchivedAt** (DateTime, nullable) - Timestamp when task was archived

## Migration Steps

### 1. Create Migration

```bash
cd backend/TodoApi
dotnet ef migrations add AddTaskCompletionAndStatusFields --output-dir Data/Migrations
```

### 2. Review Generated Migration

The migration will be created in `Data/Migrations/` directory. Review the generated file to ensure:
- Status column is added as integer
- IsArchived column defaults to false
- CompletedAt and ArchivedAt are nullable DateTime columns
- Indexes are created for Status and IsArchived fields

### 3. Apply Migration

**For Development (In-Memory):**
- Migrations are applied automatically on startup
- No manual action needed

**For Production (PostgreSQL):**
```bash
# Apply migration
dotnet ef database update

# Or generate SQL script for manual review
dotnet ef migrations script --output migration.sql
```

### 4. Data Migration (Optional)

If you have existing data, you may want to:
- Set Status = 0 (Pending) for all existing todos where IsCompleted = false
- Set Status = 1 (Completed) for all existing todos where IsCompleted = true
- Set IsArchived = false for all existing todos

This can be done via SQL:
```sql
UPDATE "Todos" 
SET "Status" = CASE 
    WHEN "IsCompleted" = true THEN 1 
    ELSE 0 
END,
"IsArchived" = false
WHERE "Status" IS NULL;
```

## Rollback

If you need to rollback this migration:

```bash
# Rollback to previous migration
dotnet ef database update PreviousMigrationName

# Or remove the migration (if not applied)
dotnet ef migrations remove
```

## Verification

After applying the migration, verify:
1. All existing todos have Status set correctly
2. New todos default to Status = 0 (Pending)
3. Indexes are created for performance
4. CompletedAt is set when IsCompleted changes to true
5. ArchivedAt is set when IsArchived changes to true

