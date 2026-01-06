# Todo App - Authentication System

Full-stack application with React 18+ frontend and .NET 8 backend implementing user authentication and registration.

## Project Structure

```
.
├── backend/
│   └── TodoApi/          # .NET 8 Web API
└── frontend/              # React 18+ Application
```

## Features Implemented

### Backend (.NET 8)
- ✅ User registration with email and password
- ✅ User login with credentials
- ✅ User logout
- ✅ Password validation (min 8 characters, special chars, uppercase, lowercase, numbers)
- ✅ JWT token-based authentication
- ✅ Token refresh mechanism
- ✅ Password reset functionality
- ✅ Secure password hashing with BCrypt
- ✅ Input validation and sanitization
- ✅ CORS configuration for React app
- ✅ Swagger/OpenAPI documentation
- ✅ Todo CRUD operations (Create, Read, Update, Delete)
- ✅ Todo management with priority levels and due dates
- ✅ User-specific todo isolation

### Frontend (React 18+)
- ✅ User registration UI with form validation
- ✅ User login UI
- ✅ User logout functionality
- ✅ Password reset UI
- ✅ Protected routes
- ✅ JWT token management
- ✅ Axios interceptors for token refresh
- ✅ Responsive design
- ✅ Form validation with Zod and React Hook Form
- ✅ Todo list display with filtering
- ✅ Add/Edit/Delete todo functionality
- ✅ Mark todos as complete/incomplete
- ✅ Priority indicators and due date display
- ✅ Modern, intuitive UI for todo management

## Getting Started

### Backend Setup

1. Navigate to the backend directory:
```bash
cd backend/TodoApi
```

2. Restore packages and run:
```bash
dotnet restore
dotnet run
```

The API will be available at `http://localhost:5000` with Swagger UI at `http://localhost:5000/swagger`

### Frontend Setup

1. Navigate to the frontend directory:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Run the development server:
```bash
npm run dev
```

The frontend will be available at `http://localhost:3000`

## API Endpoints

### Authentication

- `POST /api/auth/register` - Register a new user
  - Body: `{ email, password, confirmPassword, firstName?, lastName? }`

- `POST /api/auth/login` - Login user
  - Body: `{ email, password }`

- `POST /api/auth/logout` - Logout user
  - Body: `{ refreshToken }`
  - Headers: `Authorization: Bearer {token}`

- `POST /api/auth/refresh` - Refresh access token
  - Body: `{ refreshToken }`

- `POST /api/auth/password-reset/request` - Request password reset
  - Body: `{ email }`

- `POST /api/auth/password-reset/confirm` - Confirm password reset
  - Body: `{ email, token, newPassword, confirmPassword }`

### Todo Management

- `GET /api/todo` - Get all todos for the authenticated user
  - Headers: `Authorization: Bearer {token}`
  - Returns: Array of todos

- `GET /api/todo/{id}` - Get a specific todo by ID
  - Headers: `Authorization: Bearer {token}`
  - Returns: Todo object

- `POST /api/todo` - Create a new todo
  - Headers: `Authorization: Bearer {token}`
  - Body: `{ title, description?, dueDate?, priority? }`
  - Returns: Created todo object

- `PUT /api/todo/{id}` - Update an existing todo
  - Headers: `Authorization: Bearer {token}`
  - Body: `{ title?, description?, isCompleted?, dueDate?, priority? }`
  - Returns: Updated todo object

- `DELETE /api/todo/{id}` - Delete a todo
  - Headers: `Authorization: Bearer {token}`
  - Returns: 204 No Content

## Password Requirements

- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character (@$!%*?&)

## Technology Stack

### Backend
- .NET 8
- Entity Framework Core (In-Memory Database)
- JWT Authentication
- BCrypt for password hashing
- Swagger/OpenAPI

### Frontend
- React 18+
- TypeScript
- Vite
- React Router
- Axios
- React Hook Form
- Zod for validation

## Security Features

- Password hashing with BCrypt
- JWT token-based authentication
- Token refresh mechanism
- Input validation and sanitization
- CORS configuration
- Secure HTTP headers

## Development Notes

- The backend uses an in-memory database for development. For production, update to use a persistent database.
- JWT secret key should be stored securely in production (use environment variables or Azure Key Vault).
- Email verification and password reset tokens should be sent via email in production.
- Consider implementing rate limiting for authentication endpoints.

## User Stories

### AIRE-12: Authentication System
This implementation fulfills the requirements from Jira ticket [AIRE-12](https://3pillarglobal.atlassian.net/browse/AIRE-12):

**As a user, I want to create an account and authenticate securely so that I can access my personal To Do list.**

All acceptance criteria have been implemented:
- ✅ User can register with email and password
- ✅ User can log in with credentials
- ✅ User can log out
- ✅ Password must meet security requirements
- ✅ Session management and token-based authentication
- ✅ Password reset functionality
- ✅ Email verification (optional - structure in place)

### AIRE-13: Todo Management System
This implementation fulfills the requirements from Jira ticket [AIRE-13](https://3pillarglobal.atlassian.net/browse/AIRE-13):

**As an authenticated user, I want to manage my todos (create, read, update, delete) so that I can organize my tasks effectively.**

All acceptance criteria have been implemented:
- ✅ User can create new todos with title, description, due date, and priority
- ✅ User can view all their todos in a list
- ✅ User can update existing todos (title, description, completion status, due date, priority)
- ✅ User can delete todos
- ✅ User can mark todos as complete/incomplete
- ✅ Todos are isolated per user (users can only see and manage their own todos)
- ✅ Modern, responsive UI for todo management
- ✅ Priority levels (Low, Medium, High) with visual indicators
- ✅ Due date tracking and display

