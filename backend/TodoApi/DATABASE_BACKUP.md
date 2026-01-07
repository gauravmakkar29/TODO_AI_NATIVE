# Database Backup and Recovery Procedures

## Overview

This document outlines the backup and recovery procedures for the Todo API database. The application uses PostgreSQL for production and In-Memory database for development/testing.

## Backup Procedures

### Automated Backups

#### PostgreSQL (Production)

**Option 1: Using pg_dump (Recommended)**

```bash
# Full database backup
pg_dump -h localhost -U postgres -d TodoDb -F c -f backup_$(date +%Y%m%d_%H%M%S).dump

# Backup with compression
pg_dump -h localhost -U postgres -d TodoDb -F c -Z 9 -f backup_$(date +%Y%m%d_%H%M%S).dump

# Backup specific schema only
pg_dump -h localhost -U postgres -d TodoDb -n public -F c -f backup_$(date +%Y%m%d_%H%M%S).dump
```

**Option 2: Using pg_basebackup (For Point-in-Time Recovery)**

```bash
# Full cluster backup
pg_basebackup -h localhost -U postgres -D /backup/pg_backup -Ft -z -P
```

#### Automated Backup Script

Create a cron job or scheduled task to run daily backups:

```bash
#!/bin/bash
# backup_database.sh

BACKUP_DIR="/var/backups/todoapi"
DB_NAME="TodoDb"
DB_USER="postgres"
DB_HOST="localhost"
RETENTION_DAYS=30

# Create backup directory if it doesn't exist
mkdir -p $BACKUP_DIR

# Create backup
BACKUP_FILE="$BACKUP_DIR/todoapi_backup_$(date +%Y%m%d_%H%M%S).dump"
pg_dump -h $DB_HOST -U $DB_USER -d $DB_NAME -F c -Z 9 -f $BACKUP_FILE

# Remove backups older than retention period
find $BACKUP_DIR -name "todoapi_backup_*.dump" -mtime +$RETENTION_DAYS -delete

echo "Backup completed: $BACKUP_FILE"
```

**Schedule with cron:**
```bash
# Run daily at 2 AM
0 2 * * * /path/to/backup_database.sh >> /var/log/todoapi_backup.log 2>&1
```

### Manual Backups

#### Before Major Updates

1. Stop the application (if possible)
2. Create a backup using pg_dump
3. Verify backup file integrity
4. Store backup in secure location
5. Resume application

#### Before Schema Migrations

1. Create a full database backup
2. Test migration on a copy of the database
3. Document rollback procedure
4. Execute migration during maintenance window

## Recovery Procedures

### Full Database Restore

```bash
# Stop the application first
systemctl stop todoapi

# Drop existing database (CAUTION: This deletes all data)
dropdb -h localhost -U postgres TodoDb

# Create new database
createdb -h localhost -U postgres TodoDb

# Restore from backup
pg_restore -h localhost -U postgres -d TodoDb -v backup_20240106_020000.dump

# Verify restore
psql -h localhost -U postgres -d TodoDb -c "SELECT COUNT(*) FROM \"Users\";"

# Restart application
systemctl start todoapi
```

### Partial Restore (Specific Tables)

```bash
# Restore specific table
pg_restore -h localhost -U postgres -d TodoDb -t "Todos" -v backup_20240106_020000.dump

# Restore multiple tables
pg_restore -h localhost -U postgres -d TodoDb -t "Todos" -t "Categories" -v backup_20240106_020000.dump
```

### Point-in-Time Recovery (PITR)

If continuous archiving is enabled:

```bash
# Stop PostgreSQL
systemctl stop postgresql

# Restore base backup
cp -r /backup/pg_backup/* /var/lib/postgresql/data/

# Configure recovery
echo "restore_command = 'cp /var/lib/postgresql/archive/%f %p'" >> /var/lib/postgresql/data/postgresql.conf
echo "recovery_target_time = '2024-01-06 14:30:00'" >> /var/lib/postgresql/data/postgresql.conf

# Start PostgreSQL
systemctl start postgresql
```

## Backup Verification

### Verify Backup File

```bash
# Check backup file format
pg_restore --list backup_20240106_020000.dump

# Test restore to a temporary database
createdb -h localhost -U postgres TodoDb_test
pg_restore -h localhost -U postgres -d TodoDb_test backup_20240106_020000.dump
dropdb -h localhost -U postgres TodoDb_test
```

### Automated Verification Script

```bash
#!/bin/bash
# verify_backup.sh

BACKUP_FILE=$1
TEST_DB="TodoDb_test_$(date +%s)"

if [ -z "$BACKUP_FILE" ]; then
    echo "Usage: verify_backup.sh <backup_file>"
    exit 1
fi

# Create test database
createdb -h localhost -U postgres $TEST_DB

# Attempt restore
if pg_restore -h localhost -U postgres -d $TEST_DB -v $BACKUP_FILE; then
    echo "Backup verification: SUCCESS"
    dropdb -h localhost -U postgres $TEST_DB
    exit 0
else
    echo "Backup verification: FAILED"
    dropdb -h localhost -U postgres $TEST_DB
    exit 1
fi
```

## Backup Storage

### Local Storage

- Store backups in `/var/backups/todoapi/`
- Maintain at least 30 days of daily backups
- Maintain weekly backups for 3 months
- Maintain monthly backups for 1 year

### Remote Storage (Recommended)

1. **Cloud Storage (AWS S3, Azure Blob, Google Cloud Storage)**
   ```bash
   # Upload to S3
   aws s3 cp backup_20240106_020000.dump s3://todoapi-backups/
   
   # With lifecycle policy for automatic deletion
   ```

2. **Network File System (NFS)**
   - Mount remote NFS share
   - Store backups on network storage
   - Ensure network storage is backed up separately

3. **Tape Storage**
   - For long-term archival
   - Monthly backups stored offsite

## Disaster Recovery Plan

### Recovery Time Objective (RTO): 4 hours
### Recovery Point Objective (RPO): 24 hours

### Steps

1. **Assess the Situation**
   - Identify the scope of data loss
   - Determine the most recent valid backup
   - Estimate recovery time

2. **Prepare Recovery Environment**
   - Provision new database server (if needed)
   - Install PostgreSQL
   - Configure network access

3. **Restore Database**
   - Restore from most recent backup
   - Apply any transaction logs (if available)
   - Verify data integrity

4. **Update Application**
   - Update connection strings
   - Test application connectivity
   - Verify functionality

5. **Post-Recovery**
   - Monitor application logs
   - Verify data consistency
   - Document incident
   - Review and improve procedures

## Monitoring and Alerts

### Backup Monitoring

- Monitor backup job execution
- Alert on backup failures
- Alert on backup file size anomalies
- Verify backup file integrity daily

### Example Monitoring Script

```bash
#!/bin/bash
# check_backups.sh

BACKUP_DIR="/var/backups/todoapi"
ALERT_EMAIL="admin@example.com"

# Check if latest backup exists and is recent (within 25 hours)
LATEST_BACKUP=$(ls -t $BACKUP_DIR/*.dump 2>/dev/null | head -1)

if [ -z "$LATEST_BACKUP" ]; then
    echo "ALERT: No backup files found" | mail -s "Backup Alert" $ALERT_EMAIL
    exit 1
fi

BACKUP_AGE=$(find $LATEST_BACKUP -mtime +1)

if [ -n "$BACKUP_AGE" ]; then
    echo "ALERT: Latest backup is older than 24 hours" | mail -s "Backup Alert" $ALERT_EMAIL
    exit 1
fi

# Check backup file size (should be > 0)
BACKUP_SIZE=$(stat -f%z "$LATEST_BACKUP" 2>/dev/null || stat -c%s "$LATEST_BACKUP" 2>/dev/null)

if [ "$BACKUP_SIZE" -eq 0 ]; then
    echo "ALERT: Latest backup file is empty" | mail -s "Backup Alert" $ALERT_EMAIL
    exit 1
fi

echo "Backup check: OK"
```

## Best Practices

1. **Test Restores Regularly**
   - Monthly restore tests to a test environment
   - Document any issues encountered
   - Update procedures based on findings

2. **Automate Everything**
   - Use cron jobs or task schedulers
   - Automate backup verification
   - Automate alerting

3. **Document Everything**
   - Keep detailed logs of all backup operations
   - Document recovery procedures
   - Maintain runbooks

4. **Security**
   - Encrypt backup files at rest
   - Use secure transfer methods (SFTP, SCP)
   - Restrict access to backup files
   - Rotate backup credentials regularly

5. **Version Control**
   - Keep backup scripts in version control
   - Tag releases with backup procedure versions
   - Document changes to backup procedures

## Contact Information

For backup and recovery issues, contact:
- Database Administrator: [DBA Email]
- System Administrator: [SysAdmin Email]
- On-Call Engineer: [OnCall Phone]

## Revision History

- 2024-01-06: Initial documentation created

