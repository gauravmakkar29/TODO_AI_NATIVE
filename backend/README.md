# Todo API Backend

.NET 8 Web API backend for the Todo App with authentication features and robust data persistence.

## Features

### Authentication & Security
- User Registration with password validation
- User Login with JWT tokens
- User Logout
- Password Reset functionality
- Token Refresh
- Secure password hashing with BCrypt
- Input validation and sanitization
- **Rate Limiting** - API rate limiting to prevent abuse
- **JWT-based authentication** with configurable expiration

### Data Persistence
- **PostgreSQL Support** - Production-ready database with migrations
- **In-Memory Database** - For development and testing
- **Database Migrations** - Version-controlled schema changes
- **Database Indexing** - Optimized indexes for performance
- **Transaction Support** - ACID-compliant data operations
- **Connection Resilience** - Automatic retry on failure

### Monitoring & Logging
- **Structured Logging** - Serilog with console and file output
- **Request Logging** - HTTP request/response logging
- **Performance Monitoring** - Log slow requests (>1s)

### Data Management
- **Backup & Recovery** - Comprehensive backup procedures
- **Environment Configuration** - Separate configs for dev/staging/prod
- **Data Validation** - Input validation and error handling

## Getting Started

### Prerequisites

- .NET 8 SDK
- PostgreSQL 12+ (for production) - Optional for development

### Running the Application

```bash
cd TodoApi
dotnet restore
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `http://localhost:5000/swagger`

## API Endpoints

### Authentication

- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login user
- `POST /api/auth/logout` - Logout user
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/password-reset/request` - Request password reset
- `POST /api/auth/password-reset/confirm` - Confirm password reset

## Configuration

### Database Configuration

The application supports both In-Memory (development) and PostgreSQL (production) databases.

**Development (In-Memory):**
```json
{
  "Database": {
    "UseInMemory": true
  }
}
```

**Production (PostgreSQL):**
```json
{
  "Database": {
    "UseInMemory": false,
    "Provider": "PostgreSQL"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=TodoDb;Username=postgres;Password=your_password"
  }
}
```

### JWT Settings

```json
{
  "JwtSettings": {
    "SecretKey": "YourSecretKeyHere",
    "Issuer": "TodoApi",
    "Audience": "TodoApiUsers",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Rate Limiting

```json
{
  "IpRateLimiting": {
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      },
      {
        "Endpoint": "post:/api/auth/*",
        "Period": "1m",
        "Limit": 5
      }
    ]
  }
}
```

### Logging

Logging is configured via Serilog in `appsettings.json`. Logs are written to:
- Console (development)
- File: `logs/todoapi-YYYYMMDD.log` (rolling daily, 30 days retention)

## Database Migrations

### Initial Setup

1. **Install EF Core Tools** (if not already installed):
```bash
dotnet tool install --global dotnet-ef
```

2. **Create Initial Migration**:
```bash
cd TodoApi
dotnet ef migrations add InitialCreate --output-dir Data/Migrations
```

3. **Apply Migrations**:
```bash
# Automatic (on app startup) - Recommended
# Or manually:
dotnet ef database update
```

See [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md) for detailed migration instructions.

## Database Backup & Recovery

Comprehensive backup and recovery procedures are documented in [DATABASE_BACKUP.md](DATABASE_BACKUP.md).

Key points:
- Automated daily backups recommended
- Point-in-time recovery support
- Backup verification procedures
- Disaster recovery plan

## Performance Optimizations

### Database Indexes

The following indexes are automatically created for optimal query performance:
- `Users.Email` (unique)
- `Todos.UserId`, `Todos.UserId + IsCompleted`, `Todos.UserId + Priority`, `Todos.UserId + DueDate`
- `RefreshTokens.UserId`, `RefreshTokens.Token`, `RefreshTokens.ExpiresAt`
- `TodoCategories.TodoId`, `TodoCategories.CategoryId`
- `TodoTags.TodoId`, `TodoTags.TagId`
- `FilterPresets.UserId`

### Connection Resilience

PostgreSQL connections include:
- Automatic retry on failure (3 attempts, 5-second delay)
- 30-second command timeout
- Connection pooling

## Security Features

1. **Rate Limiting**: Prevents API abuse with configurable limits
2. **JWT Authentication**: Secure token-based authentication
3. **Password Hashing**: BCrypt with salt
4. **Input Validation**: All inputs validated and sanitized
5. **CORS**: Configured for specific origins
6. **HTTPS**: Enforced in production

## Logging & Monitoring

### Structured Logging

All logs are structured and include:
- Timestamp
- Log level
- Machine name
- Thread ID
- Request context

### Request Logging

HTTP requests are automatically logged with:
- Method and path
- Response status code
- Response time
- Errors (if any)

### Performance Monitoring

Requests taking longer than 1 second are logged at Error level for monitoring.

## Development vs Production

### Development
- In-Memory database (no setup required)
- Detailed logging
- Swagger UI enabled
- Relaxed rate limiting

### Production
- PostgreSQL database
- Optimized logging
- Swagger UI disabled
- Strict rate limiting
- Connection resilience enabled

## Additional Documentation

- [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md) - Database migration procedures
- [DATABASE_BACKUP.md](DATABASE_BACKUP.md) - Backup and recovery procedures

