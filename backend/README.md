# Todo API Backend

.NET 8 Web API backend for the Todo App with authentication features.

## Features

- User Registration with password validation
- User Login with JWT tokens
- User Logout
- Password Reset functionality
- Token Refresh
- Secure password hashing with BCrypt
- Input validation and sanitization

## Getting Started

### Prerequisites

- .NET 8 SDK

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

JWT settings can be configured in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSecretKeyHere",
    "Issuer": "TodoApi",
    "Audience": "TodoApiUsers"
  }
}
```

## Database

Currently using In-Memory database for development. For production, update `Program.cs` to use a persistent database like SQL Server or PostgreSQL.

