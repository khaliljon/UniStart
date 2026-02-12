# UniStart - Setup Guide

## Prerequisites
- .NET 8 SDK
- Node.js 18+ and npm
- PostgreSQL 17
- Visual Studio 2022 or VS Code

## Backend Setup

### 1. Database Setup
Create a PostgreSQL database:
```sql
CREATE DATABASE unistart_dev;
```

### 2. Update Connection String
Edit `appsettings.Development.json` with your PostgreSQL credentials:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=unistart_dev;Username=postgres;Password=your_password"
  }
}
```

### 3. Run Migrations
```bash
cd UniStart
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Or the app will auto-migrate on startup in Development mode.

### 4. Run the Backend
```bash
dotnet run
```

The API will be available at:
- https://localhost:7001
- http://localhost:5000

Swagger UI: https://localhost:7001/swagger

## Frontend Setup

### 1. Install Dependencies
```bash
cd client
npm install
```

### 2. Run Development Server
```bash
npm run dev
```

The React app will be available at: http://localhost:5173

## Project Structure

```
UniStart/
├── Domain/                    # Domain Layer
│   ├── Entities/             # Entity classes
│   └── Interfaces/           # Repository interfaces
├── Infrastructure/           # Infrastructure Layer
│   ├── Data/                 # DbContext
│   └── Repositories/         # Repository implementations
├── Application/              # Application Layer
│   ├── DTOs/                 # Data Transfer Objects
│   ├── Interfaces/           # Service interfaces
│   └── Services/             # Service implementations
├── Controllers/              # API Controllers
├── client/                   # React Frontend
│   ├── src/
│   │   ├── components/       # Reusable components
│   │   ├── pages/            # Page components
│   │   ├── services/         # API services
│   │   ├── store/            # Redux store
│   │   ├── hooks/            # Custom hooks
│   │   └── types/            # TypeScript types
│   └── package.json
├── Program.cs                # Application entry point
└── appsettings.json         # Configuration
```

## Architecture

### Backend (ASP.NET Core)
- **Clean Architecture** with separation of concerns
- **Entity Framework Core** with PostgreSQL
- **JWT Authentication** for secure API access
- **Repository Pattern** with Unit of Work
- **Dependency Injection** throughout

### Frontend (React 19)
- **TypeScript** for type safety
- **Redux Toolkit** for state management
- **React Router** for navigation
- **Axios** for API calls

### Adaptive Engine (per RULES.md)
- **Skill Adjustment**: +5 for correct, -3 for incorrect
- **Question Selection**:
  - skill < 40 → Easy questions
  - 40 ≤ skill < 70 → Medium questions
  - skill ≥ 70 → Hard questions

## API Endpoints

See [API_EXAMPLES.md](API_EXAMPLES.md) for detailed examples.

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/auth/register | Register new user |
| POST | /api/auth/login | Login user |
| GET | /api/exams | List all exams |
| GET | /api/exams/{id}/sections | Get exam sections |
| GET | /api/users/{id} | Get user profile |
| PUT | /api/users/{id} | Update user profile |
| POST | /api/test/next-question | Get next adaptive question |
| POST | /api/test/submit-answer | Submit answer |
| GET | /api/test/skill-profiles | Get user skills |
| GET | /api/analytics | Get user analytics |

## Testing the Application

1. Start the backend: `dotnet run`
2. Start the frontend: `cd client && npm run dev`
3. Navigate to http://localhost:5173
4. Register a new account
5. Select exam(s) and start the adaptive test
6. View your progress in Analytics

## Database Schema

See [DB_SCHEMA.md](DB_SCHEMA.md) for the complete schema.

## Domain Model

See [DOMAIN_MODEL.md](DOMAIN_MODEL.md) for entity relationships.
