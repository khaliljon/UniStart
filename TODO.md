# UniStart — Статус проекта и план запуска

> **Последнее обновление**: 27 февраля 2026

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
