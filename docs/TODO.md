# UniStart - Статус разработки и План действий

> Последнее обновление: 12 февраля 2026

---

## ✅ Что сделано

### Backend (.NET 8 + ASP.NET Core)

#### Архитектура
- [x] Clean Architecture структура (Domain, Application, Infrastructure, Controllers)
- [x] Настройка Dependency Injection в `Program.cs`
- [x] CORS конфигурация для React frontend (localhost:5173)

#### Domain Layer
- [x] **Entities:**
  - `User` (Id, Email, PasswordHash, Name, Role, CreatedAt)
  - `ExamType` (Code, Name - SAT/TOEFL/NUET)
  - `ExamSection` (Id, ExamTypeCode, Name, MinScore, MaxScore)
  - `Skill` (Id, Code, Name, Description)
  - `Topic` (Id, SkillId, Name)
  - `Question` (Id, TopicId, Text, Difficulty, ExamSections)
  - `AnswerOption` (Id, QuestionId, Text, IsCorrect)
  - `UserAnswer` (Id, UserId, QuestionId, AnswerOptionId, AnsweredAt)
  - `UserSkillProfile` (UserId, SkillId, Level)

#### Application Layer
- [x] **DTOs:**
  - `RegisterDto`, `LoginDto`, `AuthResponseDto`
  - `ExamTypeDto`, `ExamSectionDto`
  - `QuestionDto`, `AnswerOptionDto`, `SubmitAnswerDto`
  - `UserSkillProfileDto`, `AnalyticsDto`

- [x] **Services:**
  - `AuthService` - регистрация, логин с JWT токенами
  - `AdaptiveEngineService` - адаптивный алгоритм подбора вопросов
  - `ExamService` - работа с экзаменами и секциями
  - `QuestionService` - получение вопросов по критериям
  - `UserSkillService` - профиль навыков пользователя
  - `AnalyticsService` - аналитика по тестам

- [x] **Interfaces (Repositories):**
  - `IUserRepository`
  - `IExamRepository`
  - `IQuestionRepository`
  - `IUserAnswerRepository`
  - `IUserSkillRepository`

#### Infrastructure Layer
- [x] `UniStartDbContext` - Entity Framework Core контекст
- [x] Все Repository реализации
- [x] Подключение к PostgreSQL 17 (порт 5151)
- [x] Миграция `InitialCreate` применена

#### API Controllers
- [x] `AuthController` - POST /api/auth/register, /api/auth/login
- [x] `ExamController` - GET /api/exams, GET /api/exams/{code}/sections
- [x] `TestController` - POST /api/test/start, GET /api/test/next-question, POST /api/test/submit-answer
- [x] `AnalyticsController` - GET /api/analytics/{userId}

#### Безопасность
- [x] JWT аутентификация
- [x] BCrypt хеширование паролей
- [x] HTTPS профиль (порт 7047)

---

### Frontend (React 19 + TypeScript + Vite)

#### Структура проекта
- [x] Vite конфигурация с прокси на backend
- [x] TypeScript strict mode
- [x] Tailwind CSS для стилизации

#### State Management (Redux Toolkit)
- [x] `authSlice` - состояние авторизации
- [x] `examSlice` - выбор экзаменов
- [x] `testSlice` - состояние текущего теста

#### Services (API клиенты)
- [x] `api.ts` - базовый axios instance с interceptors
- [x] `authService.ts` - методы авторизации
- [x] `examService.ts` - работа с экзаменами
- [x] `testService.ts` - адаптивное тестирование
- [x] `analyticsService.ts` - получение аналитики

#### Pages
- [x] `LoginPage` - страница входа
- [x] `RegisterPage` - страница регистрации
- [x] `ExamSelectionPage` - выбор экзамена
- [x] `TestPage` - страница тестирования
- [x] `AnalyticsPage` - страница аналитики

#### Components
- [x] Базовые UI компоненты

---

### База данных (PostgreSQL 17)
- [x] База данных `UniStart` создана
- [x] Подключение на localhost:5151
- [x] Миграции применены
- [x] Все таблицы созданы

---

### Проверено и работает
- [x] Backend запускается на https://localhost:7047
- [x] Frontend запускается на http://localhost:5173
- [x] Регистрация пользователя (API тест успешен)
- [x] JWT токен генерируется корректно

---

## 🔄 В процессе / Требует тестирования

- [ ] Полный UI flow регистрации/логина в браузере
- [ ] Проверка прокси Vite → .NET API
- [ ] Тестирование адаптивного алгоритма

---

## 📋 Что нужно сделать

### Высокий приоритет

#### 1. Seed Data (Начальные данные)
- [ ] Добавить типы экзаменов (SAT, TOEFL, NUET)
- [ ] Добавить секции для каждого экзамена
- [ ] Добавить навыки (Skills) и темы (Topics)
- [ ] Добавить тестовые вопросы с вариантами ответов
- [ ] Создать DataSeeder сервис

#### 2. Frontend доработки
- [ ] Защищённые маршруты (PrivateRoute)
- [ ] Обработка ошибок на UI
- [ ] Loading состояния для кнопок и страниц
- [ ] Toast уведомления
- [ ] Persist авторизации в localStorage

#### 3. Адаптивный движок
- [ ] Полное тестирование алгоритма
- [ ] Настройка параметров (+X/-Y для skill adjustment)
- [ ] Логирование решений алгоритма

### Средний приоритет

#### 4. Функциональность тестов
- [ ] Таймер на тест/вопрос
- [ ] Прогресс-бар теста
- [ ] Пропуск вопроса
- [ ] Завершение теста и показ результатов
- [ ] История тестирований

#### 5. Аналитика
- [ ] Графики прогресса (Chart.js / Recharts)
- [ ] Рекомендации по слабым темам
- [ ] Сравнение с целевым баллом

#### 6. Профиль пользователя
- [ ] Страница профиля
- [ ] Редактирование данных
- [ ] Выбор целевых экзаменов
- [ ] Установка целевых баллов

### Низкий приоритет

#### 7. Роли и права
- [ ] Разделение Student / Tutor / Admin
- [ ] Админ панель
- [ ] Управление контентом для Tutor

#### 8. Дополнительные фичи
- [ ] Режим практики по темам
- [ ] Флэш-карточки для vocabulary
- [ ] Объяснения к ответам
- [ ] Закладки вопросов

#### 9. DevOps
- [ ] Dockerfile для backend
- [ ] Dockerfile для frontend
- [ ] docker-compose.yml
- [ ] CI/CD через GitHub Actions
- [ ] Kubernetes манифесты

---

## 🗂️ Структура проекта

```
UniStart/
├── Domain/
│   └── Entities/           # Доменные сущности
├── Application/
│   ├── DTOs/               # Data Transfer Objects
│   ├── Interfaces/         # Интерфейсы репозиториев
│   └── Services/           # Бизнес-логика
├── Infrastructure/
│   ├── Data/               # DbContext
│   └── Repositories/       # Реализации репозиториев
├── Controllers/            # API контроллеры
├── Migrations/             # EF Core миграции
├── client/                 # React frontend
│   ├── src/
│   │   ├── pages/          # Страницы
│   │   ├── components/     # Компоненты
│   │   ├── services/       # API клиенты
│   │   ├── store/          # Redux store
│   │   └── hooks/          # Custom hooks
│   └── ...
└── docs/                   # Документация
```

---

## 🚀 Как запустить

### Backend
```bash
cd UniStart
dotnet run --launch-profile "https"
# Запустится на https://localhost:7047
```

### Frontend
```bash
cd UniStart/client
npm install
npm run dev
# Запустится на http://localhost:5173
```

### База данных
- PostgreSQL 17 на порту 5151
- База данных: UniStart
- Пользователь: postgres

---

## 📝 Примечания

- Адаптивные правила описаны в `RULES.md`
- API эндпоинты описаны в `API.md`
- Схема БД описана в `DB_SCHEMA.md`
- Примеры API запросов в `API_EXAMPLES.md`
