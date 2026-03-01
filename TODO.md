# UniStart — Статус проекта и план запуска

> **Последнее обновление**: 1 марта 2026

---

## 📊 Текущий статус: MVP+ готов — Подготовка к production-запуску

### Масштаб проекта

| Метрика | Значение |
|---------|----------|
| **C# файлов** | 118 |
| **C# строк кода** | ~19 000 |
| **TypeScript/TSX файлов** | 58 |
| **Frontend строк кода** | ~10 800 |
| **Всего строк кода** | **~30 000** |
| **Контроллеров (API)** | 15 |
| **Сервисов (backend)** | 19 (включая 2 background) |
| **Domain-сущностей** | 22 |
| **Страниц (React)** | 26 |
| **API-эндпоинтов** | ~60+ |
| **Миграций БД** | 10 |
| **Вопросов в базе** | 97 (16 тем, 3 экзамена) |

---

## ✅ Что уже сделано (полный перечень)

### 1. Ядро платформы

#### Аутентификация и авторизация
- [x] Регистрация / вход (JWT HMAC-SHA256, BCrypt)
- [x] Middleware `[Authorize]` на всех защищённых эндпоинтах
- [x] Redux `authSlice` на фронтенде, persistent token
- [x] Тестовые пользователи: `test@unistart.kz / test123`, `admin@unistart.kz / admin123`

#### Адаптивный движок (IRT 3PL + CAT)
- [x] Модель IRT 3PL: `P(θ) = c + (1−c) / (1 + e^(−a(θ−b)))`
- [x] Байесовская оценка уровня EAP (81 квадратурная точка, prior N(0, 1.5), adaptive fallback)
- [x] CAT — выбор вопроса с максимальной Fisher Information
- [x] Exposure control (рандомизация из top-70%)
- [x] Кривая забывания Эббингхауза: `R(t) = e^(−t/S)`
- [x] Мультидименсиональный θ — отдельный уровень по каждому топику
- [x] Граф зависимостей между темами (13 связей)
- [x] Confidence interval (θ ± 1.96·SE)
- [x] Фильтрация по topicId — практика по конкретной теме из учебного плана

#### Тестирование и практика
- [x] Адаптивная практика (бесконечная сессия)
- [x] Feedback после каждого ответа (правильный/неправильный + объяснение)
- [x] Прогресс-бар и статистика в реальном времени
- [x] Подсказки (hints) для всех 97 вопросов
- [x] Возможность завершить тест в любой момент (Quit)
- [x] Диагностический мини-тест (10 вопросов для начальной калибровки θ)
- [x] Повторение ошибок (spaced repetition)
- [x] Практика по отдельным темам

#### Mock Exams (полноформатные тренировочные экзамены)
- [x] SAT Practice Test — 3 секции, 40 вопросов, 45 минут
- [x] TOEFL iBT Practice Test — 4 секции, 28 вопросов, 50 минут
- [x] NUET Practice Test — 2 секции, 29 вопросов, 50 минут
- [x] Таймер с цветовыми индикаторами (зелёный → жёлтый → красный)
- [x] TOEFL Reading Passages (3 академических текста, split-view UI)
- [x] Навигатор вопросов, перерывы между секциями
- [x] Результаты: балл, разбивка по секциям, review ответов с фильтрами
- [x] История попыток, best score

### 2. Аналитика и прогнозирование

#### Аналитика (`/progress`)
- [x] Radar chart навыков
- [x] Линейный график прогресса по дням
- [x] Bar chart mastery по темам
- [x] Heatmap активности (аналог GitHub contributions)
- [x] Статистика: streak, общая точность, кол-во вопросов, средний уровень
- [x] Точность по сложности (Easy/Medium/Hard)
- [x] Среднее время ответа по темам

#### Прогноз балла
- [x] Маппинг θ → реальная шкала экзамена (SAT 400–1600, TOEFL 0–120, NUET)
- [x] Confidence interval (90% CI: θ ± 1.645·SE)
- [x] AreaChart динамики прогноза с зоной уверенности
- [x] Разбивка по секциям (BarChart + RadarChart + карточки)
- [x] Сравнение с целевым баллом (маркер цели на шкале)
- [x] What-if сценарии: "что если улучшу тему X на Y%?"
- [x] ROI-анализ: топ-5 тем с наибольшим потенциалом роста балла

#### История сессий
- [x] Список сессий с пагинацией и фильтрами (экзамен, дата, тип)
- [x] Детальный просмотр: каждый вопрос, ответ, правильность, объяснение

### 3. Персональный план подготовки (`/plan`)

#### Генерация плана
- [x] Цель: целевой экзамен + дата экзамена + целевой балл
- [x] Алгоритм распределения: приоритет слабых зон, зависимости тем, spaced repetition
- [x] Типы задач: New / Review / Practice
- [x] Баланс нагрузки по дням с учётом сложности
- [x] Сущности: `StudyGoal`, `StudyPlan`, `StudyPlanEntry`

#### Ежедневные задания
- [x] Карточки заданий на сегодня (тема, минуты, кол-во вопросов)
- [x] Кнопка «▶ Начать» — переход к практике по теме (topicId фильтр)
- [x] **Автоматическая отметка выполнения** — auto-complete по UserAnswers за сегодня
- [x] Защита от случайного удаления цели (⋯ меню → подтверждение)
- [x] Недельная статистика: выполнено/запланировано, streak, accuracy

#### Динамическая адаптация
- [x] Перерасчёт при хорошем/плохом освоении темы
- [x] Перераспределение нагрузки при пропуске дня

### 4. Рекомендательная система

- [x] After-session рекомендации (анализ ошибок, похвала, milestone-чеки)
- [x] Ежедневные рекомендации (кривая забывания, retention < 70% → повторить)
- [x] Рекомендация режима ("скоро экзамен — попробуйте Mock Exam")
- [x] Слабые навыки: автообнаружение скиллов < 40%
- [x] 14 типов milestones (Q10–Q1000, STREAK_3–30, MASTERY, ACC_90, SESSIONS_10)
- [x] Streak tracking (текущий, рекордный, всего дней)
- [x] UI: страница рекомендаций с приоритетами и сеткой достижений

### 5. Контент и обучение

- [x] 97 вопросов (SAT: 40, TOEFL: 28, NUET: 29) по всем 16 темам
- [x] IRT-параметры (a, b, c) для каждого вопроса с per-question вариацией
- [x] 19 мини-уроков (теория, формулы, стратегии, markdown-рендеринг)
- [x] Подсказки (hints) для всех вопросов (по уровням Easy/Medium/Hard)
- [x] Видео-ссылки (YouTube) на 10 из 16 тем
- [x] `QuestionExpansionSeeder` — автоматическое расширение базы при запуске
- [x] TOEFL Reading Passages (3 академических текста с привязкой вопросов)

### 6. Админ-панель (`/admin`)

- [x] CRUD вопросов с фильтрацией (экзамен, тема, сложность)
- [x] Bulk import из JSON с валидацией каждого вопроса
- [x] Статистика: по экзаменам, сложности, темам, покрытие тем
- [x] Авто-расчёт IRT параметров при создании/импорте
- [x] Управление пользователями

### 7. Монетизация (заглушка для будущей оплаты)

- [x] `SubscriptionTier`: Free / Pro
- [x] Лимит 15 вопросов/день для Free, безлимит для Pro
- [x] `<ProGate>` компонент — блюр + overlay + CTA для Pro-контента
- [x] `<UpgradeBanner>` — ненавязчивый баннер апгрейда
- [x] Модальное окно тарифов (Free vs Pro сравнение)
- [x] Счётчик "осталось X вопросов сегодня"
- [x] API: `/api/subscription/status`, `/api/subscription/upgrade` (mock-заглушка)

### 8. Email-уведомления

- [x] MailKit интеграция (SMTP Gmail)
- [x] Welcome email после регистрации
- [x] Streak-reminder ("Вы не занимались 2 дня — не потеряйте серию!")
- [x] Weekly digest (прогресс за неделю, прогноз, рекомендации)
- [x] HTML-шаблоны с branding UniStart
- [x] Background services: `StreakReminderBackgroundService`, `WeeklyDigestBackgroundService`
- [x] Настройки уведомлений (вкл/выкл по типу)

### 9. Онбординг

- [x] Многошаговый мастер: Welcome → Выбор экзамена → Настройка цели → Готово
- [x] Авто-создание StudyGoal + StudyPlan при завершении онбординга
- [x] Redirect на диагностический тест
- [x] Карточки экзаменов, слайдер баллов, preset кнопки
- [x] Флаг `HasCompletedOnboarding` в User

### 10. UX/UI

- [x] Тёмная/светлая тема (CSS Custom Properties, toggle + localStorage)
- [x] Навигация 4 пункта: 🏠 Главная, 📚 Обучение, 📊 Прогресс, 📅 План
- [x] Профиль dropdown (правый верхний угол): профиль, уведомления, тема, выйти
- [x] Dashboard (`/`) — streak, рекомендации, быстрые действия, выбранные экзамены
- [x] Лендинг-страница (`/landing`): hero, фичи, тарифы, отзывы/social proof
- [x] Loading skeletons, CSS анимации, hover эффекты
- [x] Адаптивность (от 320px)
- [x] Recharts: корректные цвета графиков (зелёный/красный, tooltips, legends)

---

## � Известные проблемы и пробелы (аудит 27.02.2026)

### 🔴 Критические (ломают ключевые сценарии)

#### 1. ✅ `selectedExams` не сохраняется — теряется при F5
- **Где**: `examSlice.ts` — `initialState.selectedExams = []`, нет `localStorage`
- **Фикс**: `localStorage.getItem('selectedExams')` при инициализации, `localStorage.setItem()` в `toggleExamSelection`, `setSelectedExams`, `localStorage.removeItem()` в `clearSelection`

#### 2. ✅ Лимит вопросов (Free: 15/день) — только на фронтенде
- **Фикс**: `CanAnswerQuestionAsync()` вызывается в `TestController.SubmitAnswer` и `MockExamController.SubmitAnswer` → 429 при превышении лимита

#### 3. ✅ Нет защиты от повторной отправки ответа
- **Фикс**: `ProcessAnswerAsync` проверяет `!exists UserAnswer(userId, questionId, sessionId)` перед сохранением → `ArgumentException` при дубле

#### 4. ✅ Нет валидации принадлежности вопроса сессии
- **Фикс**: `ProcessAnswerAsync` при наличии `TestSessionId` валидирует: сессия существует, принадлежит пользователю, не завершена

#### 5. ✅ Mock Exam таймер — только на клиенте
- **Фикс**: серверная проверка `StartedAt + TotalTimeMinutes >= now` в `SubmitAnswerAsync`, `CompleteSectionAsync`, `GetCurrentSectionAsync`. При превышении — автоматическое завершение экзамена

### 🟠 Важные (ухудшают UX, но не ломают)

#### 6. ✅ Auto-complete плана: порог = 1 вопрос
- **Где**: `StudyPlanService.AutoCompleteTodayAsync` — `topicStats.Total >= 1`
- **Фикс**: порог = `Math.Max(1, recommendedQuestions / 2)` — реализовано

#### 7. ✅ `GetCurrentUserId` fallback на userId = 1
- **Где**: `TestController`, `StudyPlanController`, `AnalyticsController`
- **Фикс**: возвращает 401 вместо fallback — реализовано

#### 8. Race condition: auto-start + redirect при пустых экзаменах
- **Где**: `TestPage.tsx` — два `useEffect` запускаются параллельно: redirect (no exams) и auto-start (topicId)
- **Последствия**: при переходе из плана, если `selectedExams = []` (после F5), запускается API-вызов с пустым массивом экзаменов + одновременный redirect на `/`. Вопрос может прийти из чужого экзамена
- **Фикс**: проверять `selectedExams.length > 0` перед auto-start; или передавать `examTypeCode` из плана

#### 9. ✅ Прогресс-бар игнорирует topicId-фильтр
- **Где**: `AdaptiveEngineService.GetTotalQuestionsCountAsync`
- **Фикс**: `topicId` и `sectionId` передаются и фильтруются в `GetTotalQuestionsCountAsync` — реализовано

#### 10. ✅ EAP posterior collapse → θ сброс до 0
- **Где**: `IrtMath.EstimateAbilityEAP`
- **Фикс**: 81 квадратурных точек (было 41), диапазон ±5 (было ±4), priorSD = 1.5 (было 1.0), adaptive fallback при denominator < 1e-300 (retry с 161 точками и шире prior)

#### 11. ✅ Нет React Error Boundary
- **Где**: `main.tsx` → `<App />` без обёртки
- **Фикс**: `<ErrorBoundary>` обёртка с кнопкой «Обновить страницу» — реализовано

#### 12. JWT: нет refresh tokens, expiresAt не проверяется
- **Где**: backend — только access token (24ч), нет refresh. Frontend — `authSlice` не проверяет срок годности при restore
- **Последствия**: через 24ч токен протухает → пользователь выкидывается на `/login` прямо посреди работы. При восстановлении с протухшим токеном — мелькание авторизованного UI, потом выброс
- **Фикс**: проверять `expiresAt` при restore, добавить refresh token

### 🟡 Минорные (косметические, edge-cases)

#### 13. Mock Exam: нет Resume после ухода со страницы
- **Где**: `MockExamService.StartMockExamAsync` — автоматически abandon старых попыток. Нет UI для возврата к in_progress
- **Последствия**: случайный уход со страницы = потеря всего прогресса mock-экзамена

#### 14. Онбординг: wizard теряет прогресс при F5
- **Где**: `OnboardingPage.tsx` — все `useState`, ничего не в `sessionStorage`
- **Последствия**: обновление страницы на шаге 3 → возврат на шаг 1

#### 15. Dashboard: silent catch на все API-ошибки
- **Где**: `DashboardPage.tsx` — `catch {}` без UI-ошибки
- **Последствия**: при падении сервера Dashboard показывает `—` без объяснения

#### 16. Orphaned StudyPlanEntries при регенерации плана
- **Где**: `StudyPlanService.GeneratePlanAsync` — ставит `IsActive = false` на план, но не удаляет entries
- **Последствия**: каждая регенерация оставляет старые записи в БД. Со временем — раздувание таблиц

#### 17. ✅ `new Random()` → `Random.Shared`
- **Где**: `IrtMath.cs` — заменено на `Random.Shared` для thread-safety

#### 18. ✅ N+1 запросы в MockExamService
- **Где**: `GetAvailableMockExamsAsync`, `GetMockExamDetailAsync`
- **Фикс**: заменены O(N) циклов с `CountAsync()` на единый batch `GroupBy` + `ToDictionaryAsync` → 1 SQL-запрос вместо N

#### 19. Целевая дата плана — нет серверной валидации
- **Где**: `StudyPlanController` / `StudyPlanService.CreateGoalAsync` — принимает любую дату, даже прошлую
- **Последствия**: дата в прошлом → молча создаёт 7-дневный план. Frontend защищает `min={date}`, но обходится через API

#### 20. ✅ Exception details утекают в 500-ответах
- **Где**: `AuthController.cs`
- **Фикс**: Global Exception Handler (OP-4) + очистка catch-блоков — детали не утекают

---

## 🛠 Аудит админской / production-инфраструктуры (28.02.2026)

> Сравнение текущего состояния с тем, что есть в реальных production-проектах.
> Приоритеты: **P0** — без этого нельзя в прод, **P1** — нужно для стабильной работы, **P2** — масштабирование и удобство.

### ✅ Что уже есть

| Область | Статус | Где |
|---------|--------|-----|
| Админ-панель (вопросы CRUD, bulk import) | ✅ | `AdminController.cs`, `AdminQuestionsPage.tsx` |
| Управление пользователями (список, роль, подписка, удаление) | ✅ | `AdminUsersPage.tsx`, `AdminController` |
| Статистика контента (по экзаменам, темам, покрытие) | ✅ | `AdminStatsPage.tsx` |
| JWT аутентификация + `[Authorize(Roles = "Admin")]` | ✅ | `Program.cs`, все контроллеры |
| Background-сервисы (streak reminder, weekly digest) | ✅ | `StreakReminderBackgroundService.cs`, `WeeklyDigestBackgroundService.cs` |
| Базовые индексы (Email unique, Skill.Code unique) | ✅ | `UniStartDbContext.cs` |
| `CreatedAt` / `UpdatedAt` на `User`, `CreatedAt` на `Question` | ✅ | `User.cs`, `Question.cs` |
| Swagger + JWT security definition | ✅ | `Program.cs` (только dev) |
| CORS для React frontend | ✅ | `Program.cs` (только localhost) |
| Пагинация (только `/api/analytics/sessions`) | ⚠️ Частично | `AnalyticsController.cs` |
| Валидация DTO (только `RegisterDto`, `LoginDto`, `OnboardingDtos`) | ⚠️ Частично | `AuthDtos.cs` |

---

### 🔴 P0 — Безопасность (Critical, без этого нельзя в production)

#### OP-1. ✅ Секреты захардкожены в `appsettings.json` и закоммичены
- **Где**: `appsettings.json` — JWT `SecretKey`, SMTP `Password`, `ConnectionString` в открытом виде
- **Риск**: любой с доступом к репозиторию видит все credentials. Если репо станет публичным — полный компромисс
- **Фикс**:
  - [ ] `dotnet user-secrets` для development
  - [x] Переменные окружения (`UNISTART_JWT_SECRET`, `UNISTART_DB_CONNECTION`, `UNISTART_SMTP_PASSWORD`) — `Program.cs` читает из env vars с fallback на config
  - [x] `appsettings.Production.json` — создан, все секреты = placeholder
  - [x] `.gitignore` → добавлены `appsettings.*.local.json`, `appsettings.Production.json`, `logs/`
  - [ ] Опционально: Azure Key Vault / HashiCorp Vault для production

#### OP-2. ✅ Нет Rate Limiting — brute-force возможен
- **Где**: `Program.cs` — нет `AddRateLimiter()`, `/api/auth/login` и `/api/auth/register` полностью открыты
- **Риск**: автоматический перебор паролей, credential stuffing, DDoS
- **Фикс**:
  - [x] .NET 8 встроенный `RateLimiter` middleware (`AddRateLimiter` + `UseRateLimiter`) — 3 политики: "auth" (10/мин fixed), "api" (120/мин sliding), global (200/мин per IP)
  - [x] Fixed window: 10 логинов в минуту на IP для auth — `[EnableRateLimiting("auth")]` на `AuthController`
  - [x] Sliding window: 120 запросов в минуту для API
  - [x] 429 Too Many Requests с `ProblemDetails` RFC 7807 ответом

#### OP-3. ✅ Нет Security Headers
- **Где**: `Program.cs` — `UseHttpsRedirection()` есть, но нет `UseHsts()`, нет CSP, X-Frame-Options
- **Риск**: clickjacking, MIME-sniffing, отсутствие HSTS позволяет downgrade-атаки
- **Фикс**:
  - [x] `app.UseHsts()` — включён в non-dev окружении
  - [x] Inline middleware добавляет 5 security headers:
    ```
    X-Content-Type-Options: nosniff
    X-Frame-Options: DENY
    X-XSS-Protection: 0
    Referrer-Policy: strict-origin-when-cross-origin
    Permissions-Policy: camera=(), microphone=(), geolocation=()
    ```

#### OP-4. ✅ Нет Global Exception Handler — ошибки утекают и формат не единый
- **Где**: каждый контроллер делает свой `try/catch`, формат ответов разный (`{ error = "..." }`, `StatusCode(500, ...)`, необработанные 500)
- **Риск**: утечка внутренних деталей (SQL, стек), невозможно единообразно обрабатывать ошибки на фронте
- **Фикс**:
  - [x] `UseExceptionHandler` middleware с единым форматом RFC 7807 `ProblemDetails`
  - [x] Маппинг исключений → HTTP-коды:
    - `UnauthorizedAccessException` → 401
    - `ArgumentException` / `ValidationException` → 400
    - `KeyNotFoundException` → 404
    - `InvalidOperationException` → 409
    - Всё остальное → 500 (stack trace только в Development)
  - [x] `ILogger.LogError` для всех необработанных исключений (через Serilog)
  - [ ] Correlation ID (`X-Request-Id`) в каждом ответе и логе

---

### 🟠 P1 — Надёжность и наблюдаемость (нужно для стабильной работы)

#### OP-5. ✅ Нет структурированного логирования (Serilog)
- **Сейчас**: дефолтный `Microsoft.Extensions.Logging`, только console в dev. В production — ничего не записывается
- **Фикс**:
  - [x] Serilog: Console + File (rolling daily, 14 дней retention, `logs/unistart-.txt`)
  - [ ] JSON structured logs (timestamp, level, message, properties, exception) — позже при деплое
  - [x] Enrichers: `MachineName`, `ThreadId` + request-level: `RequestHost`, `UserAgent`, `UserId`
  - [x] `UseSerilogRequestLogging()` — автоматический лог HTTP-запросов с enrichment
  - [x] Минимальный уровень: Information для приложения, Warning для Microsoft/EF — настроено через `MinimumLevel.Override`

#### OP-6. ✅ Нет Health Checks
- **Сейчас**: нет `/health` эндпоинта. Docker/K8s не может проверить, жив ли сервис
- **Фикс**:
  - [x] `builder.Services.AddHealthChecks()` + `.AddNpgSql()` (PostgreSQL проверка)
  - [x] `app.MapHealthChecks("/health")` — полный JSON с деталями всех проверок
  - [x] `app.MapHealthChecks("/health/ready")` — readiness (включая БД, тег "ready")
  - [x] `app.MapHealthChecks("/health/live")` — liveness (процесс жив, без тегов)
  - [ ] `.AddSmtpCheck()` (email) — добавить позже
  - [ ] UI: `AspNetCore.HealthChecks.UI` (опционально)
  - [ ] Внешний мониторинг: UptimeRobot (бесплатно) на `/health`

#### OP-7. ✅ Нет Audit Log — действия админов не отслеживаются
- **Сейчас**: админ может удалить пользователя, изменить роль, удалить вопрос — и нет никакого следа
- **Фикс**:
  - [x] Сущность `AuditLog` (`Domain/Entities/AuditLog.cs`): Id (bigint), UserId, UserEmail, Action, EntityType, EntityId, OldValues (jsonb), NewValues (jsonb), IpAddress, Timestamp
  - [x] `IAuditService` + `AuditService` — `LogAsync()` с try/catch (никогда не ломает основную операцию)
  - [x] Вызов из `AdminController` при каждой мутации: все 7 эндпоинтов (Create/Update/Delete Question, BulkImport, Update/Delete User, Create Topic)
  - [x] API: `GET /api/admin/audit-logs?action=&entityType=&userId=&from=&to=&page=&pageSize=` с пагинацией (max 200/стр)
  - [x] 3 индекса: `IX_AuditLogs_Timestamp`, `IX_AuditLogs_Entity`, `IX_AuditLogs_UserId`
  - [x] Миграция `20260227200430_AddAuditLogAndSoftDelete`
  - [x] UI: `AdminAuditLogsPage.tsx` — таблица аудита с фильтрами, expandable rows, пагинация, цветовые action-badges

#### OP-8. ✅ Нет индексов на часто-запрашиваемых таблицах
- **Сейчас**: 4 unique-индекса. Нет индексов на самые тяжёлые запросы
- **Фикс** (миграция `20260227193556_AddPerformanceIndexes`):
  - [x] `UserAnswers(UserId, AnsweredAt)` — аналитика, background services, auto-complete
  - [x] `UserAnswers(UserId, QuestionId, TestSessionId)` — duplicate check (Fix #3)
  - [x] `Questions(TopicId)` — уже существовал как FK-индекс в InitialCreate
  - [x] `StudyPlanEntries(PlanId, Date)` — ежедневный план, auto-complete
  - [x] `TestSessions(UserId, StartedAt)` — история сессий
  - [x] `MockExamAttempts(UserId, Status)` — доступные/активные mock-экзамены

#### OP-9. ✅ Нет Soft Delete — данные теряются навсегда
- **Сейчас**: `DeleteQuestion` и `DeleteUser` делали `_db.Remove()` — hard delete без возможности восстановления
- **Фикс**:
  - [x] Интерфейс `ISoftDeletable`: `bool IsDeleted`, `DateTime? DeletedAt`, `int? DeletedBy` (`Domain/Entities/ISoftDeletable.cs`)
  - [x] Добавлено на: `User`, `Question` — оба реализуют `ISoftDeletable`
  - [x] Global query filter: `.HasQueryFilter(e => !e.IsDeleted)` на `User` и `Question` в `DbContext`
  - [x] `DeleteQuestionAsync` / `DeleteUserAsync` теперь soft-delete (устанавливают `IsDeleted = true`)
  - [x] Admin API: `POST /api/admin/questions/{id}/restore`, `POST /api/admin/users/{id}/restore` с аудит-логом
  - [x] `RestoreQuestionAsync` / `RestoreUserAsync` с `IgnoreQueryFilters()`
  - [x] Миграция `20260227200430_AddAuditLogAndSoftDelete`: +3 колонки на Users, +3 на Questions
  - [ ] Реальное физическое удаление — только через scheduled job (через 30 дней, позже)

#### OP-10. ✅ Нет кэширования — каждый запрос в БД
- **Сейчас**: никакого кэша. Статичные данные (ExamTypes, Sections, Topics, Skills) загружаются из БД при каждом запросе
- **Фикс**:
  - [x] `builder.Services.AddMemoryCache()` — зарегистрирован в DI
  - [x] `ExamService`: кэш `GetAllExamsAsync` (`exams:all`), `GetExamWithSectionsAsync` (`exams:sections:{code}`), `GetExamSectionsAsync` (`exams:exam-sections:{code}`) — TTL 15 мин
  - [x] `AdminService`: кэш `GetSectionsAsync` (`admin:sections`), `GetSkillsAsync` (`admin:skills`) — TTL 15 мин
  - [x] Инвалидация при CRUD в админке (`CreateTopicAsync` инвалидирует `admin:sections` и `admin:skills`)
  - [ ] Позже: Redis для distributed cache (если несколько инстансов)

#### OP-11. ✅ Нет валидации на админских DTO
- **Сейчас**: `CreateQuestionDto`, `UpdateQuestionDto`, `AdminUpdateUserDto`, `CreateTopicDto` — ноль валидации. Можно отправить пустой текст, отрицательные IRT-параметры, невалидную сложность
- **Фикс**:
  - [x] FluentValidation (`FluentValidation.AspNetCore` 11.3.0 NuGet)
  - [x] `AddFluentValidationAutoValidation()` + `AddValidatorsFromAssemblyContaining<>()` в `Program.cs`
  - [x] **Admin validators** (`Application/Validators/AdminValidators.cs`):
    - `CreateQuestionDtoValidator`: Text ≥10 символов ≤5000, TopicId > 0, 2–6 AnswerOptions, ровно 1 IsCorrect, Difficulty ∈ {Easy, Medium, Hard}, DifficultyParam ∈ [−3, 3], DiscriminationParam ∈ [0.1, 3], GuessParam ∈ [0, 0.5]
    - `UpdateQuestionDtoValidator`: те же правила, все поля optional
    - `BulkImportDtoValidator`: 1–500 вопросов, каждый через CreateQuestionDtoValidator
    - `AdminUpdateUserDtoValidator`: Role ∈ {Student, Tutor, Admin}, SubscriptionTier ∈ {Free, Pro}, Email формат, SubscriptionExpiresAt в будущем
    - `CreateTopicDtoValidator`: Name ≥2 символа ≤200, SectionId > 0, SkillId > 0
    - `CreateAnswerOptionDtoValidator`: Text не пустой ≤2000
  - [x] **Auth validators** (`Application/Validators/AuthValidators.cs`):
    - `RegisterDtoValidator`: Email (формат + ≤200), Name ≥2 ≤100, Password ≥6 ≤100
    - `LoginDtoValidator`: Email формат, Password не пустой
    - `UpdateUserDtoValidator`: Name ≥2, Email формат (optional)
  - [x] **Test/Domain validators** (`Application/Validators/TestValidators.cs`):
    - `StartTestSessionDtoValidator`: ExamTypeCodes 1–5, не пустые строки
    - `CreateStudyGoalDtoValidator`: ExamTypeCode, TargetDate > now, TargetScore 1–2400
    - `UpdateStudyGoalDtoValidator`, `CompleteEntryDtoValidator`, `MockExamSubmitAnswerDtoValidator`
    - `CompleteOnboardingDtoValidator`, `DiagnosticAnswerDtoValidator`, `StartDiagnosticDtoValidator`
    - `UpgradeRequestDtoValidator`: Plan ∈ {Pro, ProAnnual}

---

### 🟡 P2 — Масштабирование и удобство

#### OP-12. ✅ Hangfire — фоновые задачи с мониторингом
- **Фикс**:
  - [x] `Hangfire.AspNetCore 1.8.17` + `Hangfire.PostgreSql 1.20.10` NuGet
  - [x] `IBackgroundJobsService` + `BackgroundJobsService` — экстракция логики из старых BackgroundService
  - [x] `RecurringJob.AddOrUpdate("streak-reminder", ...)` — `0 */6 * * *` (каждые 6ч)
  - [x] `RecurringJob.AddOrUpdate("weekly-digest", ...)` — `0 8 * * 1` (понедельник 08:00 UTC)
  - [x] Dashboard: `/hangfire` с `HangfireAdminAuthFilter` (Admin в production, все в Development)
  - [x] API: `GET /api/admin/jobs/status` — статус всех рекуррентных задач
  - [x] API: `POST /api/admin/jobs/{jobId}/trigger` — ручной запуск
  - [x] Hangfire авто-retry: 10 попыток с экспоненциальным backoff (встроенный)

#### OP-13. ✅ Нет пагинации на admin-эндпоинтах
- **Сейчас**: `GET /api/admin/questions` и `GET /api/admin/users` возвращают ВСЕ записи. При 1000+ вопросах и 500+ пользователях — тормоза
- **Фикс**:
  - [x] `page` + `pageSize` параметры на все list-эндпоинты
  - [x] `PagedResult<T>` DTO: `{ items[], totalCount, page, pageSize, totalPages }`
  - [x] Default `pageSize = 50`, max `pageSize = 200`
  - [x] Frontend: пагинация в `AdminQuestionsPage` и `AdminUsersPage` (« ‹ page/total › » навигация, сброс страницы при изменении фильтров)

#### OP-14. ✅ Блокировка/деактивация пользователей
- **Сейчас**: единственный вариант — удалить пользователя (hard delete). Нет suspend/ban
- **Фикс**:
  - [x] Поля в `User`: `IsBlocked (bool)`, `BlockedAt (DateTime?)`, `BlockReason (string? MaxLength=500)`
  - [x] Middleware в `Program.cs`: проверять `IsBlocked` после `UseAuthorization()` → 403 Forbidden (RFC 7807 ProblemDetails)
  - [x] Admin API: `POST /api/admin/users/{id}/block` (с reason), `POST /api/admin/users/{id}/unblock` + audit logging
  - [x] Admin UI: кнопка «🚫 Заблокировать / ✅ Разблокировать» в карточке пользователя, 🚫 индикатор в списке, блок статус-карточка
  - [x] Защита: нельзя заблокировать Admin-пользователя
  - [x] Миграция `AddUserBlockFields`
  - [ ] Email-уведомление пользователю при блокировке (позже)

#### OP-15. ✅ Нет API versioning
- **Сейчас**: все маршруты `/api/...` без версии. Любое breaking change ломает всех клиентов
- **Фикс**:
  - [x] NuGet: `Asp.Versioning.Mvc` + `Asp.Versioning.Mvc.ApiExplorer`
  - [x] Non-breaking: Header (`x-api-version`) + Query string (`api-version`) — не ломает текущие маршруты
  - [x] `AssumeDefaultVersionWhenUnspecified = true`, `DefaultApiVersion = 1.0`, `ReportApiVersions = true`
  - [x] `[ApiVersion("1.0")]` на все 15 контроллеров
  - [x] `.AddApiExplorer(opts => { opts.GroupNameFormat = "'v'VVV"; })`

#### OP-16. ✅ Аудит-колонки не полные
- **Сейчас**: `User` ← `CreatedAt`, `UpdatedAt`. `Question` ← `CreatedAt` only. Большинство сущностей — ничего
- **Фикс**:
  - [x] Интерфейс `IAuditable`: `DateTime CreatedAt`, `DateTime? UpdatedAt` (`Domain/Entities/IAuditable.cs`)
  - [x] Применено к: `User`, `Question` (+UpdatedAt), `StudyGoal` (+UpdatedAt), `Topic` (+CreatedAt, UpdatedAt)
  - [x] `UniStartDbContext.SaveChangesAsync` override — автозаполнение:
    - `EntityState.Added` → `CreatedAt = DateTime.UtcNow`
    - `EntityState.Modified` → `UpdatedAt = DateTime.UtcNow`, защита `CreatedAt` от изменения
  - [x] Миграция `AddAuditableColumns`

#### OP-17. ✅ Нет сжатия ответов (Response Compression)
- **Сейчас**: API отдаёт JSON без сжатия. При больших ответах (список вопросов, аналитика) — лишний трафик
- **Фикс**:
  - [x] `builder.Services.AddResponseCompression(opts => { opts.EnableForHttps = true; })`
  - [x] Brotli + Gzip провайдеры зарегистрированы
  - [x] `app.UseResponseCompression()` — в pipeline перед routing

#### OP-18. ✅ Нет экспорта данных
- **Сейчас**: ни один endpoint не отдаёт CSV/Excel. Админ не может выгрузить вопросы, пользователей, ответы
- **Фикс**:
  - [x] `GET /api/admin/questions/export?examTypeCode=&difficulty=` → CSV файл (UTF-8 BOM для Excel)
  - [x] `GET /api/admin/users/export?role=` → CSV файл
  - [x] NuGet: `CsvHelper 33.1.0` + ручной `BuildCsv<T>()` с `CsvEscape()` для корректного quoting
  - [x] Frontend: `exportQuestionsCsv()` / `exportUsersCsv()` — blob download через `adminService.ts`
  - [x] 📥 CSV кнопки: `AdminQuestionsPage.tsx`, `AdminUsersPage.tsx`
  - [ ] `GET /api/admin/analytics/export?from=&to=` → UserAnswers за период (позже)

#### OP-19. ✅ CORS захардкожен на localhost
- **Сейчас**: `WithOrigins("http://localhost:3000", "http://localhost:5173")` — фронтенд на production-домене не сможет обращаться к API
- **Фикс**:
  - [x] `appsettings.json` → `"CorsOrigins": ["http://localhost:3000", "http://localhost:5173"]`
  - [x] `appsettings.Production.json` → `"CorsOrigins": ["https://unistart.kz", "https://www.unistart.kz"]`
  - [x] `builder.Configuration.GetSection("CorsOrigins").Get<string[]>()` — динамическая загрузка origins

#### OP-20. ✅ Бэкапы и восстановление
- **Фикс**:
  - [x] `scripts/backup.sh` — `pg_dump` → gzip, поддержка daily/weekly/monthly, опциональный S3 upload
  - [x] `scripts/restore.sh` — документированный процесс восстановления с верификацией
  - [x] `scripts/backup-cron.txt` — cron-конфигурация (daily 03:00, weekly Sun 04:00, monthly 1st 05:00)
  - [x] Retention: 7 daily + 4 weekly + 2 monthly
  - [ ] Тестирование восстановления 1 раз в месяц
  - [ ] Альтернатива: managed PostgreSQL с автобэкапами

#### OP-21. Нет OpenTelemetry / метрик
- **Сейчас**: ноль наблюдаемости в production. Нет метрик, нет трейсов
- **Фикс** (опционально на старте, нужно при росте):
  - [ ] `AddOpenTelemetry()` + `AddAspNetCoreInstrumentation()` + `AddHttpClientInstrumentation()`
  - [ ] Traces → Jaeger или Grafana Tempo (бесплатные self-hosted)
  - [ ] Metrics → Prometheus + Grafana
  - [ ] Или Application Insights (Azure, 5 GB/мес бесплатно)

#### OP-22. ✅ Feature Flags
- **Сейчас**: включение/выключение функций без redeployment через конфигурацию
- **Фикс**:
  - [x] `Microsoft.FeatureManagement.AspNetCore 4.0.0` NuGet
  - [x] `builder.Services.AddFeatureManagement()` в `Program.cs`
  - [x] Конфигурация в `appsettings.json` → `"FeatureManagement"`: 7 флагов (MockExams, WeeklyDigest, StreakReminder, ScorePrediction, StudyPlan, Recommendations, CsvExport)
  - [x] `[FeatureGate]` на контроллерах: MockExamController, PredictionController, RecommendationController, StudyPlanController, AdminController (экспорт)
  - [ ] Admin UI: переключатели features (позже)

#### OP-23. 🔶 Admin-панель — недостающие страницы
- **Сейчас**: Questions CRUD + Users + Import + Stats + **Audit Logs**
- **Фикс**:
  - [x] **Audit Logs** — `AdminAuditLogsPage.tsx`: таблица admin-действий с фильтрами (action, entityType, userId, даты), expandable rows (old/new values JSON), цветовые badge по типу действия, пагинация
  - [ ] **System Health** — статус БД, background jobs, SMTP, uptime, версия API
  - [ ] **User Activity** — детальный просмотр действий конкретного пользователя (сессии, ответы, даты)
  - [ ] **Announcements** — массовая рассылка уведомлений
  - [ ] **Password Reset** — сброс пароля пользователю из админки
  - [ ] **Real-time Dashboard** — активные пользователи, текущие сессии (SignalR, позже)

#### OP-24. 🔶 Тесты (started)
- **Фикс**:
  - [x] `UniStart.Tests` (xUnit 2.9 + Moq 4.20 + FluentAssertions 7.2)
  - [x] 40 unit-тестов для `IrtMath`: 3PL probability, Fisher information, EAP estimation, theta↔level, forgetting curve, item selection, posterior collapse regression
  - [ ] Unit-тесты: `AdaptiveEngineService`, `StudyPlanService`, `ScorePredictionService`
  - [ ] Integration-тесты: `WebApplicationFactory` + `TestContainers` (PostgreSQL)
  - [ ] Frontend: Vitest + React Testing Library
  - [ ] Покрытие ≥ 60% для бизнес-логики
  - [ ] CI запуск: `dotnet test` + `npx vitest run` в GitHub Actions

---

### 📋 Сводная матрица приоритетов

| Приоритет | ID | Задача | Сложность | Эффект | Статус |
|-----------|-----|--------|-----------|--------|--------|
| **P0** | OP-1 | Вынести секреты из кода | 🟢 Лёгко | 🔴 Критично | ✅ Done |
| **P0** | OP-2 | Rate Limiting | 🟢 Лёгко | 🔴 Критично | ✅ Done |
| **P0** | OP-3 | Security Headers | 🟢 Лёгко | 🔴 Критично | ✅ Done |
| **P0** | OP-4 | Global Exception Handler | 🟡 Средне | 🔴 Критично | ✅ Done |
| **P1** | OP-5 | Serilog + structured logging | 🟡 Средне | 🟠 Важно | ✅ Done |
| **P1** | OP-6 | Health Checks | 🟢 Лёгко | 🟠 Важно | ✅ Done |
| **P1** | OP-7 | Audit Log (сущность + API) | 🟡 Средне | 🟠 Важно | ✅ Done |
| **P1** | OP-8 | Индексы БД | 🟢 Лёгко | 🟠 Важно | ✅ Done |
| **P1** | OP-9 | Soft Delete | 🟡 Средне | 🟠 Важно | ✅ Done |
| **P1** | OP-10 | IMemoryCache | 🟢 Лёгко | 🟠 Важно | ✅ Done |
| **P1** | OP-11 | FluentValidation на DTO | 🟡 Средне | 🟠 Важно | ✅ Done |
| **P2** | OP-12 | Hangfire / job monitoring | 🟡 Средне | 🟡 Желательно | ✅ Done |
| **P2** | OP-13 | Пагинация admin API | 🟢 Лёгко | 🟡 Желательно | ✅ Done |
| **P2** | OP-14 | Block/Suspend users | 🟡 Средне | 🟡 Желательно | ✅ Done |
| **P2** | OP-15 | API versioning | 🟢 Лёгко | 🟡 Желательно | ✅ Done |
| **P2** | OP-16 | Аудит-колонки (IAuditable) | 🟡 Средне | 🟡 Желательно | ✅ Done |
| **P2** | OP-17 | Response Compression | 🟢 Лёгко | 🟡 Желательно | ✅ Done |
| **P2** | OP-18 | Экспорт CSV | 🟡 Средне | 🟡 Желательно | ✅ Done |
| **P2** | OP-19 | CORS из конфигурации | 🟢 Лёгко | 🟡 Желательно | ✅ Done |
| **P2** | OP-20 | Бэкапы | 🟡 Средне | 🟠 Важно | ✅ Done |
| **P2** | OP-21 | OpenTelemetry | 🔴 Сложно | 🟡 Желательно | ⬜ |
| **P2** | OP-22 | Feature Flags | 🟢 Лёгко | 🟡 Желательно | ✅ Done |
| **P2** | OP-23 | Admin-панель доп. страницы | 🔴 Сложно | 🟡 Желательно | 🔶 Partial |
| **P2** | OP-24 | Тесты (unit + integration) | 🔴 Сложно | 🟠 Важно | 🔶 Partial |

> **Прогресс**: 22/24 выполнено + 2 частично (все P0, все P1, все P2 кроме OP-21). 5/5 критических + 8 важных багов исправлены. 40 unit-тестов.
> **Осталось**: OP-21 (OpenTelemetry, опционально) + доп. тесты + доп. admin-страницы

---

## 🎓 Тьюторская система и внутренний мессенджер

> **Цель**: Студенты видят каталог тьюторов с профилями и рейтингами, могут написать тьютору через встроенный чат. Тьюторы управляют своим профилем, специализацией и расписанием. Чат — real-time (SignalR).

### Архитектура решения

```
┌─────────────────────────────────────────────────────────────┐
│  Frontend (React)                                           │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────────┐  │
│  │ /tutors      │  │ /tutors/:id  │  │ /messages         │  │
│  │  Каталог     │  │  Профиль     │  │  Чат (SignalR)    │  │
│  │  тьюторов    │  │  тьютора     │  │  real-time        │  │
│  └──────────────┘  └──────────────┘  └───────────────────┘  │
│  ┌─────────────────────────────────┐                        │
│  │ /tutor/dashboard                │                        │
│  │  Кабинет тьютора (свой профиль, │                        │
│  │  студенты, расписание, заявки)  │                        │
│  └─────────────────────────────────┘                        │
└─────────────────────────────────────────────────────────────┘
        ▼ REST + SignalR WebSocket
┌─────────────────────────────────────────────────────────────┐
│  Backend (.NET 8)                                           │
│  ┌──────────────────┐  ┌────────────────────┐               │
│  │ TutorController   │  │ MessageController  │               │
│  │  GET /tutors      │  │  GET /conversations│               │
│  │  GET /tutors/:id  │  │  GET /messages/:id │               │
│  │  PUT /tutor/profile│ │  POST /messages    │               │
│  └──────────────────┘  └────────────────────┘               │
│  ┌──────────────────┐  ┌────────────────────┐               │
│  │ ChatHub (SignalR) │  │ ITutorService      │               │
│  │  SendMessage      │  │ IMessageService    │               │
│  │  MarkAsRead       │  └────────────────────┘               │
│  │  Typing indicator │                                       │
│  └──────────────────┘                                       │
└─────────────────────────────────────────────────────────────┘
        ▼
┌─────────────────────────────────────────────────────────────┐
│  PostgreSQL — новые таблицы                                  │
│  ┌────────────────┐  ┌──────────────┐  ┌─────────────────┐  │
│  │ TutorProfiles  │  │ Conversations│  │ Messages        │  │
│  │ UserId (FK)    │  │ Id           │  │ ConversationId  │  │
│  │ Bio            │  │ StudentId    │  │ SenderId        │  │
│  │ Specializations│  │ TutorId      │  │ Text            │  │
│  │ HourlyRate     │  │ CreatedAt    │  │ SentAt          │  │
│  │ Rating         │  │ LastMessageAt│  │ ReadAt          │  │
│  │ IsAvailable    │  │ Status       │  │ Type            │  │
│  └────────────────┘  └──────────────┘  └─────────────────┘  │
│  ┌────────────────┐  ┌──────────────┐                       │
│  │ TutorReviews   │  │ TutorSchedule│                       │
│  │ TutorId        │  │ TutorId      │                       │
│  │ StudentId      │  │ DayOfWeek    │                       │
│  │ Rating (1-5)   │  │ StartTime    │                       │
│  │ Comment        │  │ EndTime      │                       │
│  └────────────────┘  └──────────────┘                       │
└─────────────────────────────────────────────────────────────┘
```

### Этап T-1: Domain-сущности и миграция 🟢

> **5 новых сущностей, 1 миграция**

- [ ] **`TutorProfile`** — расширенный профиль тьютора (1:1 к User)
  - `Id`, `UserId` (FK → User), `Bio` (text, ≤2000), `Headline` (≤200, "Сертифицированный TOEFL-тьютор")
  - `Specializations` (jsonb массив ExamTypeCode: ["SAT", "TOEFL"]), `HourlyRate` (decimal? для будущей монетизации)
  - `AverageRating` (decimal, кэш), `TotalReviews` (int, кэш), `TotalStudents` (int, кэш)
  - `IsAvailable` (bool — принимает ли новых учеников), `IsVerified` (bool — подтверждён админом)
  - `Experience` (text, ≤1000, "5 лет опыта, средний прирост учеников +150 баллов SAT")
  - `AvatarUrl` (string?), `ContactPreference` (enum: Chat / Email / Both)
  - `CreatedAt`, `UpdatedAt` (IAuditable)

- [ ] **`TutorSchedule`** — расписание доступности
  - `Id`, `TutorProfileId` (FK), `DayOfWeek` (0–6), `StartTime` (TimeOnly), `EndTime` (TimeOnly)

- [ ] **`TutorReview`** — отзывы студентов
  - `Id`, `TutorProfileId` (FK), `StudentId` (FK → User), `Rating` (1–5), `Comment` (≤1000)
  - `CreatedAt`, unique constraint: (TutorProfileId, StudentId) — один отзыв от одного студента

- [ ] **`Conversation`** — чат-диалог (1:1 студент ↔ тьютор)
  - `Id`, `StudentId` (FK → User), `TutorId` (FK → User)
  - `LastMessageAt`, `LastMessagePreview` (≤100), `UnreadCountStudent`, `UnreadCountTutor`
  - `Status` (Active / Archived), `CreatedAt`
  - Unique constraint: (StudentId, TutorId)

- [ ] **`Message`** — сообщения в чате
  - `Id` (bigint), `ConversationId` (FK), `SenderId` (FK → User)
  - `Text` (≤4000), `SentAt` (DateTime UTC), `ReadAt` (DateTime?), `IsEdited` (bool)
  - `Type` (enum: Text / System) — system = "Беседа начата", "Тьютор принял заявку"
  - Индекс: `(ConversationId, SentAt)` для пагинации

- [ ] **EF Core миграция** `AddTutoringAndMessaging`

### Этап T-2: Backend — сервисы и API 🟡

> **2 сервиса, 2 контроллера, 1 SignalR Hub**

- [ ] **`ITutorService`** + `TutorService`
  - `GetTutorsAsync(filters)` — каталог: поиск по имени, фильтр по экзамену, сортировка (рейтинг, цена, отзывы), IsAvailable only, пагинация
  - `GetTutorProfileAsync(userId)` — полный профиль + расписание + последние отзывы
  - `UpdateMyProfileAsync(userId, dto)` — тьютор редактирует свой профиль
  - `SetScheduleAsync(userId, slots[])` — установить расписание
  - `LeaveReviewAsync(studentId, tutorId, rating, comment)` — оставить отзыв (пересчёт AverageRating)
  - `GetMyStudentsAsync(tutorId)` — список студентов, с которыми есть активные беседы

- [ ] **`IMessageService`** + `MessageService`
  - `GetConversationsAsync(userId)` — все диалоги пользователя (студент видит своих тьюторов, тьютор видит своих студентов)
  - `GetMessagesAsync(conversationId, userId, page)` — история сообщений с пагинацией (новые → старые)
  - `SendMessageAsync(senderId, conversationId, text)` — отправка + обновление LastMessage + инкремент unread
  - `StartConversationAsync(studentId, tutorId)` — создать беседу (или вернуть существующую)
  - `MarkAsReadAsync(conversationId, userId)` — обнулить unread counter

- [ ] **`TutorController`** (`/api/tutors`)
  - `GET /api/tutors?search=&exam=&sort=rating&available=true&page=&pageSize=` — каталог (публичный для авторизованных)
  - `GET /api/tutors/{id}` — профиль тьютора + расписание + отзывы
  - `PUT /api/tutor/profile` — редактирование своего профиля `[Authorize(Roles = "Tutor")]`
  - `PUT /api/tutor/schedule` — обновить расписание `[Authorize(Roles = "Tutor")]`
  - `POST /api/tutors/{id}/reviews` — оставить отзыв `[Authorize(Roles = "Student")]`
  - `GET /api/tutor/students` — мои студенты `[Authorize(Roles = "Tutor")]`

- [ ] **`MessageController`** (`/api/messages`)
  - `GET /api/messages/conversations` — список диалогов текущего пользователя
  - `GET /api/messages/conversations/{id}` — сообщения диалога (пагинация, cursor-based)
  - `POST /api/messages/conversations` — начать диалог `{ tutorId }` (только студент)
  - `POST /api/messages/conversations/{id}/messages` — отправить сообщение
  - `POST /api/messages/conversations/{id}/read` — пометить прочитанным

- [ ] **`ChatHub`** (SignalR Hub) — `/hubs/chat`
  - `OnConnectedAsync` — подключение, join в группу `user_{userId}`
  - `SendMessage(conversationId, text)` — отправка через хаб → push обоим участникам
  - `MarkAsRead(conversationId)` — пометить прочитанным → push счётчик
  - `Typing(conversationId)` → "печатает..." индикатор
  - JWT-аутентификация через `?access_token=` query parameter (SignalR стандарт)

### Этап T-3: Frontend — страницы 🟡

> **4 новые страницы + 1 компонент в навигации**

- [ ] **`TutorsPage.tsx`** (`/tutors`) — Каталог тьюторов
  - Карточки тьюторов: аватар (заглушка с инициалами), имя, headline, ★ рейтинг, кол-во отзывов
  - Бэйджи специализаций: 🟦 SAT | 🟩 TOEFL | 🟨 NUET
  - Фильтры: поиск по имени, экзамен, "Только доступные", сортировка (по рейтингу/по отзывам)
  - Зелёная точка "Доступен" / серая "Не принимает"
  - Кнопка "💬 Написать" → создание беседы + redirect на /messages
  - Responsive grid: 3 col → 2 col → 1 col

- [ ] **`TutorProfilePage.tsx`** (`/tutors/:id`) — Профиль тьютора
  - Hero-секция: аватар, имя, headline, рейтинг, кол-во студентов
  - "О себе" (Bio), "Опыт" (Experience)
  - Специализации (бэйджи экзаменов)
  - Расписание (таблица по дням недели, зелёные слоты)
  - Отзывы: ★★★★☆ + комментарий + имя студента + дата
  - Кнопка "💬 Начать чат" (если студент) / "✏️ Редактировать" (если свой профиль)
  - Форма отзыва (★ рейтинг + комментарий), только если был диалог

- [ ] **`MessagesPage.tsx`** (`/messages`) — Мессенджер
  - Split-layout: список бесед (слева) + чат (справа)
  - Список бесед: аватар, имя, последнее сообщение, время, badge непрочитанных
  - Чат: пузырьки (мои справа синие, чужие слева серые), время, галочки прочтения
  - Поле ввода + кнопка отправки + Enter to send
  - "Печатает..." индикатор (SignalR)
  - Auto-scroll к последнему сообщению, lazy-load старых при scroll вверх
  - Пустое состояние: "Выберите диалог или найдите тьютора"
  - Mobile: slide-in список → чат (одна панель за раз)

- [ ] **`TutorDashboardPage.tsx`** (`/tutor/dashboard`) — Кабинет тьютора
  - Редактирование профиля: bio, headline, experience, specializations (чекбоксы), availability toggle
  - Расписание: визуальный редактор (чекбоксы по дням + time pickers)
  - Список активных студентов (из бесед): имя, последний контакт, средний прогресс
  - Статистика: ★ рейтинг, кол-во отзывов, кол-во студентов
  - Последние отзывы

- [ ] **Навигация**
  - `Layout.tsx`: добавить 📣 **Тьюторы** в navbar (между "Прогресс" и "План")
  - `ProfileDropdown.tsx`: 💬 **Сообщения** с badge непрочитанных
  - Для тьюторов: 🎓 **Мой кабинет** в dropdown

### Этап T-4: NuGet + SignalR инфраструктура 🟢

- [ ] `Microsoft.AspNetCore.SignalR` (встроенный в .NET 8)
- [ ] `Program.cs`: `builder.Services.AddSignalR()`, `app.MapHub<ChatHub>("/hubs/chat")`
- [ ] JWT auth для SignalR: `AddAuthentication().AddJwtBearer(opts => { opts.Events.OnMessageReceived = ... access_token query })` — уже есть, нужен event handler
- [ ] CORS: добавить `/hubs/*` в allowed origins
- [ ] Frontend: `@microsoft/signalr` npm package
- [ ] `chatService.ts` — класс-singleton: connect, disconnect, onMessage, sendMessage, onTyping

### Этап T-5: Seeder — тестовые данные 🟢

- [ ] 3 тьюторских аккаунта (tutor1/tutor2/tutor3@unistart.kz) с разными специализациями
- [ ] TutorProfile для каждого: bio, headline, расписание, ratings
- [ ] 5–10 тестовых отзывов
- [ ] 2–3 тестовые беседы с сообщениями

### Порядок реализации

| Шаг | Этап | Описание | Зависимости | Сложность |
|-----|------|----------|-------------|-----------|
| 1 | T-1 | Сущности + миграция | — | 🟢 Лёгко |
| 2 | T-4 | SignalR setup + chatService.ts | — | 🟢 Лёгко |
| 3 | T-2a | TutorService + TutorController | T-1 | 🟡 Средне |
| 4 | T-2b | MessageService + MessageController + ChatHub | T-1, T-4 | 🟡 Средне |
| 5 | T-3a | TutorsPage + TutorProfilePage | T-2a | 🟡 Средне |
| 6 | T-3b | MessagesPage (SignalR real-time) | T-2b | 🔴 Сложно |
| 7 | T-3c | TutorDashboardPage | T-2a | 🟡 Средне |
| 8 | T-5 | Seeder + тестовые данные | T-1 | 🟢 Лёгко |

> **Итого**: ~8 шагов, оценка трудозатрат ~4–6 часов. Результат: полноценная тьюторская маркетплейс-система с real-time чатом.

---

## 🚀 Что нужно для полноценного запуска

### Фаза 1: Критический путь (1–2 недели)

> **Без этого запускаться нельзя**

#### 1.1 Безопасность 🔒
- [ ] Вынести секреты в переменные окружения (JWT SecretKey, SMTP Password, DB ConnectionString)
- [ ] Создать `appsettings.Production.json` (без секретов, всё через env vars)
- [ ] Rate limiting на API (`AspNetCoreRateLimit` или встроенный .NET 8 `RateLimiter`)
- [ ] Input validation (FluentValidation на DTO)
- [ ] CORS — ограничить origins для production-домена
- [ ] HTTPS обязательно (Let's Encrypt)
- [ ] Сменить JWT SecretKey на криптографически стойкий (≥64 символа)
- [ ] Добавить refresh tokens (текущий JWT живёт 24ч без обновления)

#### 1.2 Контейнеризация и деплой 🐳
- [ ] Dockerfile для .NET 8 backend (`mcr.microsoft.com/dotnet/aspnet:8.0`, multi-stage build)
- [ ] Dockerfile для frontend (Vite build → nginx для раздачи статики)
- [ ] `docker-compose.yml` (API + PostgreSQL + frontend + nginx reverse proxy)
- [ ] `.env.production` с переменными окружения
- [ ] Health check endpoint (`/health`)
- [ ] Автоматические миграции при старте

#### 1.3 CI/CD 🔄
- [ ] GitHub Actions workflow: build → lint → type-check → test → docker push → deploy
- [ ] Автосборка при push в `main`
- [ ] Docker image registry (GitHub Container Registry — бесплатно)

#### 1.4 Хостинг
- [ ] Выбрать: VPS (Hetzner/DigitalOcean) или PaaS (Railway/Render) или Azure
- [ ] Настроить домен и DNS
- [ ] SSL-сертификат (Let's Encrypt автообновление через certbot/nginx)

### Фаза 2: Качество (2–3 недели)

> **Для стабильной работы и уверенности**

#### 2.1 Тестирование 🧪
- [ ] Unit-тесты: `AdaptiveEngineService`, `StudyPlanService`, `ScorePredictionService` (xUnit + Moq)
- [ ] Integration-тесты: ключевые API flows (WebApplicationFactory + TestContainers)
- [ ] Frontend-тесты: Vitest + React Testing Library (основные пользовательские сценарии)
- [ ] Покрытие ≥ 60% для бизнес-логики

#### 2.2 Логирование и мониторинг 📋
- [ ] Serilog → structured JSON logging (файл + console)
- [ ] Global exception middleware (единый формат ошибок API)
- [ ] Uptime monitoring (UptimeRobot / Healthchecks.io — бесплатно)
- [ ] Application Insights или Seq для production

#### 2.3 Производительность ⚡
- [ ] Индексы БД: `UserAnswers(UserId, AnsweredAt)`, `Questions(TopicId)`, `StudyPlanEntries(PlanId, Date)`
- [ ] Кэширование: IMemoryCache для экзаменов/тем (TTL 5–15 мин)
- [ ] Пагинация на всех list-эндпоинтах
- [ ] Gzip/Brotli compression (middleware)
- [ ] Frontend: React.lazy + Suspense для lazy-loading страниц
- [ ] Frontend: Vite chunk splitting и tree shaking

### Фаза 3: Рост (после запуска)

> **Для масштабирования и выхода в прибыль**

#### 3.1 Расширение контента 📚
- [ ] Довести базу до 300+ вопросов (минимум 15–20 на тему)
- [ ] Реальные форматы SAT Digital (из College Board practice)
- [ ] Больше Reading Passages для TOEFL
- [ ] Собственные видео-уроки или партнёрские
- [ ] Локализация: казахский язык

#### 3.2 Платёжная система 💳
- [ ] Интеграция Kaspi Pay (для KZ рынка) + Stripe (международный)
- [ ] Webhook обработка платежей → автоактивация Pro
- [ ] Пробный период (7 дней Pro бесплатно)
- [ ] Промокоды и реферальная система

#### 3.3 Маркетинг и SEO 🌐
- [ ] SSR/SSG для лендинга (Next.js или prerender)
- [ ] Open Graph + meta теги
- [ ] Google Analytics / Yandex Metrika
- [ ] Реферальная программа ("пригласи друга — получи неделю Pro")

#### 3.4 Мобильное приложение 📱
- [ ] PWA (Service Worker + manifest.json) — быстрый и дешёвый путь
- [ ] React Native (если нужен App Store / Google Play)
- [ ] Push-уведомления
- [ ] Офлайн-режим с синхронизацией

#### 3.5 Collaborative Features 👥
- [ ] Collaborative filtering (при 100+ пользователях, паттерны успешных студентов)
- [ ] Leaderboard (streak, accuracy, вопросы)
- [ ] Форум / чат поддержки

---

## 💰 Примерные расходы на содержание платформы

### Вариант 1: Минимальный запуск (до 100 пользователей)

| Статья расходов | Сервис | $/мес |
|----------------|--------|-------|
| **VPS сервер** | Hetzner CX22 (2 vCPU, 4 GB RAM, 40 GB SSD) | **$5** |
| **База данных** | PostgreSQL на том же VPS (Docker) | $0 |
| **Домен** | `.kz` (~$10/год) | **~$1** |
| **SSL** | Let's Encrypt | $0 |
| **Email** | Gmail SMTP (лимит 500 писем/день) | $0 |
| **DNS / CDN** | Cloudflare Free | $0 |
| **Мониторинг** | UptimeRobot Free | $0 |
| **CI/CD** | GitHub Actions Free (2 000 мин/мес) | $0 |
| **Бэкапы** | Hetzner snapshots | **$1** |
| | | |
| **ИТОГО** | | **~$7/мес (~3 000 ₸)** |

### Вариант 2: Рабочий (100–1 000 пользователей)

| Статья расходов | Сервис | $/мес |
|----------------|--------|-------|
| **VPS сервер** | Hetzner CX32 (4 vCPU, 8 GB RAM, 80 GB SSD) | **$13** |
| **Managed DB** | Отдельный VPS для PostgreSQL или managed | **$10** |
| **Домен** | `.kz` + `.app` | **$2** |
| **SSL** | Let's Encrypt | $0 |
| **Email** | Resend / Mailgun (10K emails бесплатно) | **$0–20** |
| **CDN** | Cloudflare Free или Pro | **$0–20** |
| **Мониторинг** | Sentry Free (5K events) + UptimeRobot | $0 |
| **CI/CD** | GitHub Actions | $0 |
| **Бэкапы** | Automated + off-site (S3/R2) | **$3** |
| **Object Storage** | Аватары, контент (если нужно) | **$3** |
| | | |
| **ИТОГО** | | **~$30–70/мес (~13 000–30 000 ₸)** |

### Вариант 3: Production (1 000–10 000 пользователей)

| Статья расходов | Сервис | $/мес |
|----------------|--------|-------|
| **Серверы** | 2× VPS (app + background worker) | **$40–80** |
| **Managed DB** | PostgreSQL managed (DO / AWS RDS) | **$25–50** |
| **Redis** | Кэш + очереди (managed или Docker) | **$10–15** |
| **Домен + DNS** | Cloudflare Pro | **$20** |
| **Email** | SendGrid / Mailgun (50K emails) | **$20–35** |
| **CDN + Storage** | Cloudflare + R2 для статики | **$5–10** |
| **Мониторинг** | Sentry Team + Grafana Cloud | **$30** |
| **CI/CD** | GitHub Actions | **$0–10** |
| **Бэкапы** | Daily + point-in-time recovery | **$10** |
| | | |
| **ИТОГО** | | **~$160–250/мес (~70 000–110 000 ₸)** |

### Разовые расходы при запуске

| Статья | Стоимость |
|--------|-----------|
| Домен `.kz` (1 год) | ~5 000 ₸ |
| Домен `.app` (1 год) | ~6 000 ₸ ($14) |
| Дизайн логотипа (фриланс) | 10 000–30 000 ₸ |
| Юридическое оформление (ИП/ТОО) | 20 000–50 000 ₸ |
| Контент: дополнительные вопросы (внешние авторы) | 50 000–150 000 ₸ |
| | |
| **ИТОГО разовые** | **~90 000–240 000 ₸** |

### 📈 Unit-экономика и breakeven

| Показатель | Значение |
|-----------|----------|
| Стоимость Pro подписки | **2 990–4 990 ₸/мес** (~$6–10) |
| Конверсия Free → Pro (EdTech среднее) | 2–5% |
| **Breakeven Вариант 1** ($7/мес) | **1–2 платящих** |
| **Breakeven Вариант 2** ($50/мес) | **5–8 платящих** |
| **Breakeven Вариант 3** ($200/мес) | **20–33 платящих** |
| Нужно Free пользователей для V2 (при 3% конверсии) | ~200 |
| Нужно Free пользователей для V3 (при 3% конверсии) | ~800 |

> **Вывод**: При конверсии 3% и цене Pro = 3 990 ₸/мес, для покрытия Варианта 2 (~30 000 ₸) нужно **~250 зарегистрированных пользователей**. Это реалистичная цель для KZ рынка SAT/TOEFL/NUET подготовки. Старт на Варианте 1 ($7/мес) окупается при **первом же платящем клиенте**.

---

## 📐 Технический стек

| Компонент | Технология | Версия |
|-----------|------------|--------|
| Backend | ASP.NET Core | .NET 8 |
| Frontend | React + TypeScript | 19 / 5.3 |
| Bundler | Vite | 5.4 |
| Database | PostgreSQL | 17 |
| ORM | Entity Framework Core | 8.0 |
| Auth | JWT (HMAC-SHA256) + BCrypt | — |
| State Management | Redux Toolkit | 2.0 |
| Charts | Recharts | 3.7 |
| Email | MailKit (SMTP) | 4.3 |
| Styling | CSS Custom Properties (dark/light) | — |
| API Docs | Swagger / Swashbuckle | 6.6 |

---

## 🏗 Архитектура (Clean Architecture)

```
UniStart/
├── Domain/               # Бизнес-сущности и интерфейсы
│   ├── Entities/         # 22 сущности (User, Question, ExamType, Topic, StudyPlan, MockExam, ...)
│   └── Interfaces/       # IRepository<T>, IUnitOfWork
│
├── Application/          # Бизнес-логика
│   ├── DTOs/             # 12 файлов DTO
│   ├── Interfaces/       # 15 интерфейсов сервисов
│   └── Services/         # 19 сервисов (IRT, CAT, план, прогноз, email, ...)
│
├── Infrastructure/       # Инфраструктура
│   ├── Data/             # DbContext, DatabaseSeeder, QuestionExpansionSeeder
│   └── Repositories/     # Generic Repository<T>, UnitOfWork
│
├── Controllers/          # 15 API-контроллеров (~60+ эндпоинтов)
├── Migrations/           # 10 EF Core миграций
│
├── client/               # React 19 SPA
│   └── src/
│       ├── pages/        # 23 страницы
│       ├── components/   # Layout, общие компоненты
│       ├── services/     # 12 API-клиентов (axios)
│       ├── store/        # Redux: authSlice, examSlice, testSlice
│       ├── hooks/        # useAppDispatch, useAppSelector, useTheme
│       └── types/        # TypeScript-интерфейсы
│
└── docs/                 # ARCHITECTURE.md, API.md, DB_SCHEMA.md, RULES.md
```

---

## 🗺 Roadmap

```
  Февраль 2026          Март 2026              Апрель 2026            Май 2026+
  ─────────────         ──────────────         ──────────────         ──────────
  ✅ MVP+ готов    →    🔒 Безопасность   →    🧪 Тесты + QA    →    🚀 Публичный
  ✅ 9 этапов            🐳 Docker              📚 300+ вопросов       запуск
  ✅ UX polish           🔄 CI/CD               💳 Kaspi Pay           📱 PWA
  ✅ Email               🌐 Деплой              📊 Analytics           👥 Маркетинг
  ✅ Subscriptions       🏥 Health checks       🌍 Локализация         🤝 Партнёры
```

---

*Создано: 27 февраля 2026*
