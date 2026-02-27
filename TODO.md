# UniStart — Статус проекта и план запуска

> **Последнее обновление**: 28 февраля 2026

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
| **Страниц (React)** | 23 |
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
- [x] Байесовская оценка уровня EAP (41 квадратурная точка, prior N(0, 1.5))
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

#### 1. `selectedExams` не сохраняется — теряется при F5
- **Где**: `examSlice.ts` — `initialState.selectedExams = []`, нет `localStorage`
- **Последствия**: после обновления страницы пользователь заново выбирает экзамены. TestPage, TopicsPage, ReviewPage — всё редиректит. Dashboard показывает "Выберите экзамены" даже если пользователь их уже выбрал
- **Связанная проблема**: после онбординга выбранный экзамен не попадает в Redux → новый пользователь видит пустой Dashboard
- **Фикс**: синхронизация `selectedExams` с `localStorage` или загрузка из профиля пользователя с сервера

#### 2. Лимит вопросов (Free: 15/день) — только на фронтенде
- **Где**: `SubscriptionService.CanAnswerQuestionAsync()` существует, но **нигде не вызывается** из контроллеров
- **Последствия**: через API (cURL/Postman) можно решать неограниченное кол-во вопросов. Mock-экзамены аналогично доступны Free-пользователям через API
- **Фикс**: добавить проверку в `TestController.SubmitAnswer` и `MockExamController`

#### 3. Нет защиты от повторной отправки ответа
- **Где**: `AdaptiveEngineService.ProcessAnswerAsync` — нет проверки, что на вопрос уже отвечали
- **Последствия**: можно отправить правильный ответ на один вопрос 100 раз → искусственно завысить θ (skill level). EAP пересчитывает по **всем** UserAnswers без дедупликации
- **Фикс**: проверять `!exists UserAnswer(userId, questionId, sessionId)` перед сохранением

#### 4. Нет валидации принадлежности вопроса сессии
- **Где**: `AdaptiveEngineService.ProcessAnswerAsync` — не проверяет, что вопрос был выдан этому пользователю
- **Последствия**: можно вручную отправить ответ на любой `QuestionId` (в т.ч. из другого экзамена)
- **Фикс**: проверять, что вопрос принадлежит текущей `TestSession` пользователя

#### 5. Mock Exam таймер — только на клиенте
- **Где**: `MockExamPage.tsx` — `useState(0)` + `setInterval`, сервер не проверяет время
- **Последствия**: можно остановить таймер через DevTools или отправить ответы в любом темпе. `TimeSpentSeconds` доверяется с клиента
- **Фикс**: серверная проверка `StartedAt + TimeLimitMinutes >= now` при `SubmitAnswer`/`CompleteSection`

### 🟠 Важные (ухудшают UX, но не ломают)

#### 6. Auto-complete плана: порог = 1 вопрос
- **Где**: `StudyPlanService.AutoCompleteTodayAsync` — `topicStats.Total >= 1`
- **Последствия**: ответив на 1 вопрос по теме, задание плана на сегодня автоматически отмечается выполненным (даже если рекомендовано 10). Статистика приверженности завышается
- **Фикс**: порог = `Math.Max(1, recommendedQuestions / 2)` или процент от рекомендованного

#### 7. `GetCurrentUserId` fallback на userId = 1
- **Где**: `TestController`, `StudyPlanController`, `AnalyticsController` — `return userId > 0 ? userId : 1`
- **Последствия**: при сбое JWT middleware данные записываются в аккаунт пользователя #1
- **Фикс**: вернуть 401 вместо fallback, убрать `?? "0"` → бросить исключение

#### 8. Race condition: auto-start + redirect при пустых экзаменах
- **Где**: `TestPage.tsx` — два `useEffect` запускаются параллельно: redirect (no exams) и auto-start (topicId)
- **Последствия**: при переходе из плана, если `selectedExams = []` (после F5), запускается API-вызов с пустым массивом экзаменов + одновременный redirect на `/`. Вопрос может прийти из чужого экзамена
- **Фикс**: проверять `selectedExams.length > 0` перед auto-start; или передавать `examTypeCode` из плана

#### 9. Прогресс-бар игнорирует topicId-фильтр
- **Где**: `AdaptiveEngineService.GetTotalQuestionsCountAsync` — считает **все** вопросы по выбранным экзаменам, а не по теме
- **Последствия**: при практике по теме (5 вопросов) прогресс-бар показывает "Вопрос 4 из 97 — 3%", потом внезапно тест завершён
- **Фикс**: передать `topicId` и `sectionId` в `GetTotalQuestionsCountAsync`

#### 10. EAP posterior collapse → θ сброс до 0
- **Где**: `IrtMath.EstimateAbilityEAP` — если posterior слишком узкий для 41 квадратурной точки, fallback возвращает `(0.0, 1.5)`
- **Последствия**: сильный студент (θ → 3.5+) может внезапно увидеть сброс навыка с 85% до 50%. Редкий, но шокирующий баг
- **Фикс**: увеличить кол-во точек (81+), расширить диапазон до ±5, добавить adaptive fallback

#### 11. Нет React Error Boundary
- **Где**: `main.tsx` → `<App />` без обёртки
- **Последствия**: если компонент бросает ошибку при рендере (undefined.map, NaN в Recharts), **весь экран белеет** без возможности восстановления
- **Фикс**: `<ErrorBoundary>` с кнопкой "Обновить страницу"

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

#### 17. `new Random()` вместо `Random.Shared`
- **Где**: `IrtMath.cs` — при exposure control, несогласованность с `AdaptiveEngineService`
- **Последствия**: теоретически одинаковая последовательность при частых вызовах (маловероятно)

#### 18. N+1 запросы в MockExamService
- **Где**: `GetAvailableMockExamsAsync`, `GetMockExamDetailAsync`, `StartMockExamAsync` — цикл + запрос на каждую секцию
- **Последствия**: при 3 экзаменах × 3 секции = 9+ extra SQL-запросов вместо одного

#### 19. Целевая дата плана — нет серверной валидации
- **Где**: `StudyPlanController` / `StudyPlanService.CreateGoalAsync` — принимает любую дату, даже прошлую
- **Последствия**: дата в прошлом → молча создаёт 7-дневный план. Frontend защищает `min={date}`, но обходится через API

#### 20. Exception details утекают в 500-ответах
- **Где**: `AuthController.cs` — `details = ex.Message` в catch
- **Последствия**: внутренние ошибки (стек, SQL) видны клиенту

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

#### OP-1. Секреты захардкожены в `appsettings.json` и закоммичены
- **Где**: `appsettings.json` — JWT `SecretKey`, SMTP `Password`, `ConnectionString` в открытом виде
- **Риск**: любой с доступом к репозиторию видит все credentials. Если репо станет публичным — полный компромисс
- **Фикс**:
  - [ ] `dotnet user-secrets` для development
  - [ ] Переменные окружения (`UNISTART_JWT_SECRET`, `UNISTART_DB_CONNECTION`, `UNISTART_SMTP_PASSWORD`)
  - [ ] `appsettings.Production.json` — без секретов, всё через `Environment.GetEnvironmentVariable`
  - [ ] `.gitignore` → добавить `appsettings.*.local.json`
  - [ ] Опционально: Azure Key Vault / HashiCorp Vault для production

#### OP-2. Нет Rate Limiting — brute-force возможен
- **Где**: `Program.cs` — нет `AddRateLimiter()`, `/api/auth/login` и `/api/auth/register` полностью открыты
- **Риск**: автоматический перебор паролей, credential stuffing, DDoS
- **Фикс**:
  - [ ] .NET 8 встроенный `RateLimiter` middleware (`AddRateLimiter` + `UseRateLimiter`)
  - [ ] Fixed window: 5 логинов в минуту на IP для `/api/auth/*`
  - [ ] Sliding window: 60 запросов в минуту на пользователя для остальных API
  - [ ] 429 Too Many Requests с `Retry-After` header

#### OP-3. Нет Security Headers
- **Где**: `Program.cs` — `UseHttpsRedirection()` есть, но нет `UseHsts()`, нет CSP, X-Frame-Options
- **Риск**: clickjacking, MIME-sniffing, отсутствие HSTS позволяет downgrade-атаки
- **Фикс**:
  - [ ] `app.UseHsts()` (уже есть в шаблоне, не активирован)
  - [ ] Middleware или `NWebsec` для заголовков:
    ```
    X-Content-Type-Options: nosniff
    X-Frame-Options: DENY
    X-XSS-Protection: 0
    Referrer-Policy: strict-origin-when-cross-origin
    Content-Security-Policy: default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'
    Permissions-Policy: camera=(), microphone=(), geolocation=()
    ```

#### OP-4. Нет Global Exception Handler — ошибки утекают и формат не единый
- **Где**: каждый контроллер делает свой `try/catch`, формат ответов разный (`{ error = "..." }`, `StatusCode(500, ...)`, необработанные 500)
- **Риск**: утечка внутренних деталей (SQL, стек), невозможно единообразно обрабатывать ошибки на фронте
- **Фикс**:
  - [ ] `UseExceptionHandler` middleware с единым форматом RFC 7807 `ProblemDetails`
  - [ ] Маппинг исключений → HTTP-коды:
    - `UnauthorizedAccessException` → 401
    - `ArgumentException` / `ValidationException` → 400
    - `KeyNotFoundException` → 404
    - Всё остальное → 500 (без деталей в production)
  - [ ] `ILogger.LogError` для всех необработанных исключений
  - [ ] Correlation ID (`X-Request-Id`) в каждом ответе и логе

---

### 🟠 P1 — Надёжность и наблюдаемость (нужно для стабильной работы)

#### OP-5. Нет структурированного логирования (Serilog)
- **Сейчас**: дефолтный `Microsoft.Extensions.Logging`, только console в dev. В production — ничего не записывается
- **Фикс**:
  - [ ] Serilog → `appsettings`: Console (dev) + File (rolling daily) + Seq/ELK (production)
  - [ ] JSON structured logs (timestamp, level, message, properties, exception)
  - [ ] Enrichers: `RequestId`, `UserId`, `ClientIP`, `MachineName`
  - [ ] `UseSerilogRequestLogging()` — автоматический лог HTTP-запросов (method, path, status, elapsed)
  - [ ] Минимальный уровень: Information для приложения, Warning для Microsoft/EF

#### OP-6. Нет Health Checks
- **Сейчас**: нет `/health` эндпоинта. Docker/K8s не может проверить, жив ли сервис
- **Фикс**:
  - [ ] `builder.Services.AddHealthChecks()` + `.AddNpgSql()` (база) + `.AddSmtpCheck()` (email)
  - [ ] `app.MapHealthChecks("/health")` — простой 200/503
  - [ ] `app.MapHealthChecks("/health/ready")` — readiness (включая БД)
  - [ ] `app.MapHealthChecks("/health/live")` — liveness (процесс жив)
  - [ ] UI: `AspNetCore.HealthChecks.UI` (опционально)
  - [ ] Внешний мониторинг: UptimeRobot (бесплатно) на `/health`

#### OP-7. Нет Audit Log — действия админов не отслеживаются
- **Сейчас**: админ может удалить пользователя, изменить роль, удалить вопрос — и нет никакого следа
- **Фикс**:
  - [ ] Сущность `AuditLog`:
    ```
    Id, UserId, Action (string), EntityType, EntityId,
    OldValues (jsonb), NewValues (jsonb), IpAddress, Timestamp
    ```
  - [ ] `IAuditService.LogAsync(userId, action, entity, oldVal, newVal)`
  - [ ] Вызов из `AdminService` при каждой мутации (create/update/delete)
  - [ ] API: `GET /api/admin/audit-logs?action=&entity=&userId=&from=&to=` с пагинацией
  - [ ] UI: таблица аудита в админке с фильтрами

#### OP-8. Нет индексов на часто-запрашиваемых таблицах
- **Сейчас**: 4 unique-индекса. Нет индексов на самые тяжёлые запросы
- **Фикс** (новая миграция):
  - [ ] `UserAnswers(UserId, AnsweredAt)` — аналитика, background services, auto-complete
  - [ ] `UserAnswers(UserId, QuestionId, TestSessionId)` — duplicate check (Fix #3)
  - [ ] `Questions(TopicId)` — выборка вопросов по теме
  - [ ] `StudyPlanEntries(PlanId, Date)` — ежедневный план, auto-complete
  - [ ] `TestSessions(UserId, StartedAt)` — история сессий
  - [ ] `MockExamAttempts(UserId, Status)` — доступные/активные mock-экзамены

#### OP-9. Нет Soft Delete — данные теряются навсегда
- **Сейчас**: `DeleteQuestion` и `DeleteUser` делают `_db.Remove()` — hard delete без возможности восстановления
- **Фикс**:
  - [ ] Интерфейс `ISoftDeletable`: `bool IsDeleted`, `DateTime? DeletedAt`, `int? DeletedBy`
  - [ ] Добавить на: `User`, `Question`, `StudyPlan`, `MockExamAttempt`
  - [ ] Global query filter: `.HasQueryFilter(e => !e.IsDeleted)` в `DbContext`
  - [ ] Admin API: `POST /api/admin/questions/{id}/restore`, `POST /api/admin/users/{id}/restore`
  - [ ] Реальное физическое удаление — только через scheduled job (через 30 дней)

#### OP-10. Нет кэширования — каждый запрос в БД
- **Сейчас**: никакого кэша. Статичные данные (ExamTypes, Sections, Topics, Skills) загружаются из БД при каждом запросе
- **Фикс**:
  - [ ] `builder.Services.AddMemoryCache()` + `IMemoryCache`
  - [ ] Кэшировать (TTL 15 мин): ExamTypes, Sections, Topics, Skills, TopicDependencies
  - [ ] Инвалидация при CRUD в админке
  - [ ] Позже: Redis для distributed cache (если несколько инстансов)

#### OP-11. Нет валидации на админских DTO
- **Сейчас**: `CreateQuestionDto`, `UpdateQuestionDto`, `AdminUpdateUserDto`, `CreateTopicDto` — ноль валидации. Можно отправить пустой текст, отрицательные IRT-параметры, невалидную сложность
- **Фикс**:
  - [ ] FluentValidation (`FluentValidation.AspNetCore` NuGet)
  - [ ] Validators для каждого админского DTO:
    - `CreateQuestionValidator`: Text не пустой (≥10 символов), 2–6 AnswerOptions, ровно 1 IsCorrect, Difficulty ∈ {Easy, Medium, Hard}, DifficultyParam ∈ [−3, 3], DiscriminationParam ∈ [0.1, 3]
    - `AdminUpdateUserValidator`: Role ∈ {User, Admin}, SubscriptionTier ∈ {Free, Pro}
    - `CreateTopicValidator`: Name не пустой, SectionId существует
  - [ ] `AddFluentValidationAutoValidation()` в `Program.cs`

---

### 🟡 P2 — Масштабирование и удобство

#### OP-12. Background jobs не мониторятся и не управляются
- **Сейчас**: 2 `BackgroundService` — молча работают, при ошибке пишут `LogError` и продолжают. Нет retry, нет dead-letter, нет UI
- **Фикс**:
  - [ ] Hangfire (`Hangfire.AspNetCore` + `Hangfire.PostgreSql`):
    - Streak reminder → `RecurringJob.AddOrUpdate("streak-reminder", ...)`
    - Weekly digest → `RecurringJob.AddOrUpdate("weekly-digest", ...)`
  - [ ] Dashboard: `/hangfire` (с авторизацией `[Authorize(Roles = "Admin")]`)
  - [ ] Retry-политика: 3 попытки с экспоненциальным backoff
  - [ ] Или минимально: admin endpoint `GET /api/admin/jobs/status` с последним временем выполнения и статусами

#### OP-13. Нет пагинации на admin-эндпоинтах
- **Сейчас**: `GET /api/admin/questions` и `GET /api/admin/users` возвращают ВСЕ записи. При 1000+ вопросах и 500+ пользователях — тормоза
- **Фикс**:
  - [ ] `page` + `pageSize` параметры на все list-эндпоинты
  - [ ] `PagedResult<T>` DTO: `{ items[], totalCount, page, pageSize, totalPages }`
  - [ ] Default `pageSize = 50`, max `pageSize = 200`
  - [ ] Frontend: пагинация в `AdminQuestionsPage` и `AdminUsersPage`

#### OP-14. Блокировка/деактивация пользователей
- **Сейчас**: единственный вариант — удалить пользователя (hard delete). Нет suspend/ban
- **Фикс**:
  - [ ] Поля в `User`: `IsBlocked (bool)`, `BlockedAt (DateTime?)`, `BlockReason (string?)`
  - [ ] Middleware: проверять `IsBlocked` на каждый авторизованный запрос → 403 Forbidden
  - [ ] Admin API: `POST /api/admin/users/{id}/block`, `POST /api/admin/users/{id}/unblock`
  - [ ] Admin UI: кнопка «Заблокировать» в карточке пользователя
  - [ ] Email-уведомление пользователю при блокировке

#### OP-15. Нет API versioning
- **Сейчас**: все маршруты `/api/...` без версии. Любое breaking change ломает всех клиентов
- **Фикс**:
  - [ ] NuGet: `Asp.Versioning.Http` + `Asp.Versioning.Mvc`
  - [ ] URL-based: `/api/v1/...` (самый простой, рекомендуется для начала)
  - [ ] `[ApiVersion("1.0")]` на все существующие контроллеры
  - [ ] Swagger отдельные doc-group по версиям

#### OP-16. Аудит-колонки не полные
- **Сейчас**: `User` ← `CreatedAt`, `UpdatedAt`. `Question` ← `CreatedAt` only. Большинство сущностей — ничего
- **Фикс**:
  - [ ] Интерфейс `IAuditable`: `DateTime CreatedAt`, `DateTime? UpdatedAt`, `int? CreatedBy`, `int? UpdatedBy`
  - [ ] Применить к: `Question`, `Topic`, `ExamSection`, `StudyPlan`, `StudyPlanEntry`, `MockExamAttempt`
  - [ ] `DbContext.SaveChangesAsync` override — автозаполнение:
    ```csharp
    foreach (var entry in ChangeTracker.Entries<IAuditable>())
    {
        if (entry.State == EntityState.Added) entry.Entity.CreatedAt = DateTime.UtcNow;
        if (entry.State == EntityState.Modified) entry.Entity.UpdatedAt = DateTime.UtcNow;
    }
    ```
  - [ ] Миграция для добавления колонок

#### OP-17. Нет сжатия ответов (Response Compression)
- **Сейчас**: API отдаёт JSON без сжатия. При больших ответах (список вопросов, аналитика) — лишний трафик
- **Фикс**:
  - [ ] `builder.Services.AddResponseCompression(opts => { opts.EnableForHttps = true; })`
  - [ ] `.AddResponseCompression().AddBrotliCompression().AddGzipCompression()`
  - [ ] `app.UseResponseCompression()` перед `UseRouting`

#### OP-18. Нет экспорта данных
- **Сейчас**: ни один endpoint не отдаёт CSV/Excel. Админ не может выгрузить вопросы, пользователей, ответы
- **Фикс**:
  - [ ] `GET /api/admin/questions/export?format=csv` → CSV файл
  - [ ] `GET /api/admin/users/export?format=csv` → CSV файл
  - [ ] `GET /api/admin/analytics/export?from=&to=` → UserAnswers за период
  - [ ] NuGet: `CsvHelper` для генерации CSV
  - [ ] Frontend: кнопки «📥 Экспорт CSV» в разделах вопросов и пользователей

#### OP-19. CORS захардкожен на localhost
- **Сейчас**: `WithOrigins("http://localhost:3000", "http://localhost:5173")` — фронтенд на production-домене не сможет обращаться к API
- **Фикс**:
  - [ ] Вынести origins в конфигурацию: `appsettings.json` → `"CorsOrigins": ["http://localhost:5173"]`
  - [ ] `appsettings.Production.json` → `"CorsOrigins": ["https://unistart.kz", "https://www.unistart.kz"]`
  - [ ] `builder.Configuration.GetSection("CorsOrigins").Get<string[]>()`

#### OP-20. Нет бэкапов и стратегии восстановления
- **Сейчас**: ноль. При падении VPS или ошибке — потеря всех данных
- **Фикс**:
  - [ ] Скрипт `backup.sh`: `pg_dump` → сжатие → загрузка в S3/R2
  - [ ] Cron: ежедневный бэкап (03:00 UTC), еженедельный полный
  - [ ] Retention: 7 ежедневных + 4 еженедельных + 2 ежемесячных
  - [ ] `restore.sh` — документированный процесс восстановления
  - [ ] Тестирование восстановления 1 раз в месяц
  - [ ] Альтернатива: managed PostgreSQL (DigitalOcean/Hetzner) с автоматическими бэкапами

#### OP-21. Нет OpenTelemetry / метрик
- **Сейчас**: ноль наблюдаемости в production. Нет метрик, нет трейсов
- **Фикс** (опционально на старте, нужно при росте):
  - [ ] `AddOpenTelemetry()` + `AddAspNetCoreInstrumentation()` + `AddHttpClientInstrumentation()`
  - [ ] Traces → Jaeger или Grafana Tempo (бесплатные self-hosted)
  - [ ] Metrics → Prometheus + Grafana
  - [ ] Или Application Insights (Azure, 5 GB/мес бесплатно)

#### OP-22. Нет Feature Flags
- **Сейчас**: для включения/отключения функциональности нужен redeployment
- **Фикс** (при росте):
  - [ ] `Microsoft.FeatureManagement.AspNetCore` NuGet
  - [ ] Конфигурация в `appsettings.json`: `"FeatureFlags": { "MockExams": true, "WeeklyDigest": false }`
  - [ ] `[FeatureGate("MockExams")]` на контроллерах
  - [ ] Admin UI: переключатели features

#### OP-23. Admin-панель — недостающие страницы
- **Сейчас**: Questions CRUD + Users + Import + Stats. Нет обзора системного здоровья
- **Фикс**:
  - [ ] **Audit Logs** — страница с таблицей всех admin-действий (→ зависит от OP-7)
  - [ ] **System Health** — статус БД, background jobs, SMTP, uptime, версия API
  - [ ] **User Activity** — детальный просмотр действий конкретного пользователя (сессии, ответы, даты)
  - [ ] **Announcements** — массовая рассылка уведомлений
  - [ ] **Password Reset** — сброс пароля пользователю из админки
  - [ ] **Real-time Dashboard** — активные пользователи, текущие сессии (SignalR, позже)

#### OP-24. Нет тестов — ни одного
- **Сейчас**: 0 test-проектов, 0 unit-тестов, 0 integration-тестов
- **Фикс**:
  - [ ] `UniStart.Tests` (xUnit + Moq + FluentAssertions)
  - [ ] Unit-тесты: `IrtMath`, `AdaptiveEngineService`, `StudyPlanService`, `ScorePredictionService`
  - [ ] Integration-тесты: `WebApplicationFactory` + `TestContainers` (PostgreSQL)
  - [ ] Frontend: Vitest + React Testing Library
  - [ ] Покрытие ≥ 60% для бизнес-логики
  - [ ] CI запуск: `dotnet test` + `npx vitest run` в GitHub Actions

---

### 📋 Сводная матрица приоритетов

| Приоритет | ID | Задача | Сложность | Эффект |
|-----------|-----|--------|-----------|--------|
| **P0** | OP-1 | Вынести секреты из кода | 🟢 Лёгко | 🔴 Критично |
| **P0** | OP-2 | Rate Limiting | 🟢 Лёгко | 🔴 Критично |
| **P0** | OP-3 | Security Headers | 🟢 Лёгко | 🔴 Критично |
| **P0** | OP-4 | Global Exception Handler | 🟡 Средне | 🔴 Критично |
| **P1** | OP-5 | Serilog + structured logging | 🟡 Средне | 🟠 Важно |
| **P1** | OP-6 | Health Checks | 🟢 Лёгко | 🟠 Важно |
| **P1** | OP-7 | Audit Log (сущность + API) | 🟡 Средне | 🟠 Важно |
| **P1** | OP-8 | Индексы БД | 🟢 Лёгко | 🟠 Важно |
| **P1** | OP-9 | Soft Delete | 🟡 Средне | 🟠 Важно |
| **P1** | OP-10 | IMemoryCache | 🟢 Лёгко | 🟠 Важно |
| **P1** | OP-11 | FluentValidation на DTO | 🟡 Средне | 🟠 Важно |
| **P2** | OP-12 | Hangfire / job monitoring | 🟡 Средне | 🟡 Желательно |
| **P2** | OP-13 | Пагинация admin API | 🟢 Лёгко | 🟡 Желательно |
| **P2** | OP-14 | Block/Suspend users | 🟡 Средне | 🟡 Желательно |
| **P2** | OP-15 | API versioning | 🟢 Лёгко | 🟡 Желательно |
| **P2** | OP-16 | Аудит-колонки (IAuditable) | 🟡 Средне | 🟡 Желательно |
| **P2** | OP-17 | Response Compression | 🟢 Лёгко | 🟡 Желательно |
| **P2** | OP-18 | Экспорт CSV | 🟡 Средне | 🟡 Желательно |
| **P2** | OP-19 | CORS из конфигурации | 🟢 Лёгко | 🟡 Желательно |
| **P2** | OP-20 | Бэкапы | 🟡 Средне | 🟠 Важно |
| **P2** | OP-21 | OpenTelemetry | 🔴 Сложно | 🟡 Желательно |
| **P2** | OP-22 | Feature Flags | 🟢 Лёгко | 🟡 Желательно |
| **P2** | OP-23 | Admin-панель доп. страницы | 🔴 Сложно | 🟡 Желательно |
| **P2** | OP-24 | Тесты (unit + integration) | 🔴 Сложно | 🟠 Важно |

> **Рекомендуемый порядок**: OP-1 → OP-4 → OP-2 → OP-3 → OP-5 → OP-6 → OP-8 → OP-10 → OP-11 → OP-7 → OP-9 → OP-13 → OP-20 → OP-24 → остальное

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
