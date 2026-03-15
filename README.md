# UniStart — Adaptive Exam Preparation Platform

Adaptive platform for CSCA, NUET, SAT, TOEFL and IELTS exam preparation using Item Response Theory (IRT 3PL) and Computerized Adaptive Testing (CAT).

Official CSCA preparation partner: **LinHao International School**.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 8 (ASP.NET Core), EF Core, FluentValidation, Hangfire |
| Frontend | React 19, TypeScript, Redux Toolkit, Vite |
| Database | PostgreSQL 17 |
| Auth | JWT (access + refresh tokens) |
| Real-time | SignalR |
| i18n | Russian, Kazakh, English |

## Core Features

- **IRT 3PL + CAT Engine** — adaptive question selection based on student ability estimation (theta)
- **Mock Exams** — timed, section-based practice exams replicating real test format (CSCA, NUET, SAT, TOEFL, IELTS)
- **Score Prediction** — real-time exam score forecast with confidence intervals
- **Analytics** — radar charts, heatmaps, skill tracking, progress over time
- **Study Plan** — auto-generated preparation plan based on exam date, target score, and weak areas
- **Diagnostic Test** — 10-question initial placement (~5 min)
- **Review Mistakes** — detailed explanations and error analysis
- **Admin Panel** — question import (PDF/DOCX/XLSX), content management, user management
- **Tutor System** — tutor profiles, catalog, chat (SignalR)
- **Monetization** — Free / Pro tiers, monthly (10 000 ₸) and yearly (99 990 ₸) plans

## Quick Start

```bash
# Prerequisites: .NET 8 SDK, Node.js 18+, PostgreSQL 17

# 1. Clone and configure
cp appsettings.json appsettings.Development.local.json
# Edit connection string and JWT secret in the local file

# 2. Backend
dotnet restore
dotnet ef database update
dotnet run

# 3. Frontend
cd client
npm install
npm run dev
```

The app runs at `http://localhost:5173` (frontend) and `http://localhost:5196` (API).

## Project Structure

```
├── Domain/Entities/        # EF Core entities (User, Question, ExamType, etc.)
├── Application/
│   ├── DTOs/               # Request/response models
│   ├── Interfaces/         # Service contracts
│   ├── Services/           # Business logic (IRT, analytics, subscription, etc.)
│   └── Validators/         # FluentValidation rules
├── Infrastructure/         # DbContext, repositories, seeders
├── Controllers/            # API endpoints
├── client/src/
│   ├── pages/              # React pages
│   ├── components/         # Shared components
│   ├── services/           # API client services
│   ├── store/              # Redux slices
│   ├── i18n/               # Translations (ru, kz, en)
│   └── types/              # TypeScript type definitions
└── docs/                   # Architecture, API docs, TODO
```

## Security

- Secrets (`ConnectionStrings`, `JwtSettings.SecretKey`, API keys) must **never** be committed
- `appsettings.json` contains only placeholder values (`CHANGE_ME`)
- Production config uses `appsettings.Production.json` (gitignored) or environment variables
- `.gitignore` excludes: `appsettings.*.local.json`, `appsettings.Production.json`, `logs/`

## Documentation

- [API.md](API.md) — API endpoints reference
- [ARCHITECTURE.md](ARCHITECTURE.md) — system architecture
- [DB_SCHEMA.md](DB_SCHEMA.md) — database schema
- [DOMAIN_MODEL.md](DOMAIN_MODEL.md) — domain model
- [SETUP.md](SETUP.md) — detailed setup guide
- [docs/TODO.md](docs/TODO.md) — development roadmap
