# UniStart -- Статус проекта и план запуска

> **Последнее обновление**: 2 марта 2026

---

## Текущий статус: v2 Learning -- Sprint 2 (polish) ✅ завершён

### Сводка состояния на 2 марта 2026

**Что работает стабильно:**
- Ядро платформы: IRT 3PL + CAT, адаптивная практика, mock exams (SAT, TOEFL, NUET, IELTS, CSCA)
- Аналитика, прогноз баллов, study plan, onboarding, монетизация (Free/Pro)
- Admin-панель, тьюторская система (каталог, профили, чат -- SignalR баги задокументированы)
- Production: rate limiting, security headers, Serilog, health checks, audit log, soft delete, кэш, Hangfire
- KaTeX рендеринг формул, GuidedTour, IELTS/CSCA сидер, убраны ненужные экзамены

**Критические проблемы (2 марта, Sprint 2):**
1. KaTeX иконки / символы в тёмной теме сливаются с фоном (FormulaPage, TimedDrillPage) — нужна контрастная цветовая схема.
2. Marathon drill — нет таймера обратного отсчёта, только секундомер вверх. Нужен countdown (например 3 мин) + подсчёт вопросов за время.
3. Мало контента: мало флэшкарточек, стратегий, формул. По IELTS и CSCA контент не загружается (seed есть, но БД не обновлена или фронтенд не фильтрует).
4. Mock Exams — отображаются только 3 экзамена (SAT, TOEFL, NUET). IELTS и CSCA моки не видны.
5. Установка цели — максимальный целевой балл (slider max) зафиксирован 100 для всех экзаменов. Должен быть: SAT→1600, TOEFL→120, NUET→200, IELTS→9 (шаг 0.5), CSCA→100.
6. "Что если" — в dropdown выбора темы все показывают 0%, а в результате расчёта 11% → рассинхрон masteryPercentage vs IRT currentLevel.
7. История сессий — некоторые сессии показывают 0/0 correct, examTypeName "Gaokao" — незавершённые/пустые сессии не фильтруются.

---

## ПЛАН РАБОТ -- Sprint 2 марта 2026

### FIX-1. Рендеринг теории и формул (KaTeX + Markdown)
- [x] Установить `katex` и `@types/katex` npm
- [x] Создать компонент `MathRenderer` -- парсит `$...$` inline и `$$...$$` block, рендерит через KaTeX
- [x] Обновить `renderContent()` в TopicsPage для корректного рендеринга `**bold**`, `*italic*`, `$math$`, `$$block$$`
- [x] Обновить FormulaPage -- формулы отображаются через KaTeX вместо monospace
- [x] Обновить StrategyPage -- контент через MathRenderer
- [x] Обновить FlashcardPage -- front/back карточек через MathRenderer
- [x] Обновить TimedDrillPage -- text вопросов и options через MathRenderer

### FIX-2. Убрать видеоссылки
- [x] TopicsPage: убрать блок "Видео-урок" из lesson view
- [x] Сидер: убрать videoUrl из SeedTopicLessonsAsync (не удалять поле, просто null)

### FIX-3. Убрать неактуальные экзамены из тьюторов
- [x] TutorsPage: удалить `<option value="EGE">` и `<option value="OGE">`, добавить NUET, CSCA
- [x] TutorProfileEditPage: EXAM_OPTIONS = ['SAT', 'TOEFL', 'IELTS', 'NUET', 'CSCA']
- [x] TutorDashboardPage: placeholder специализаций обновить
- [x] Seeder: обновить тьюторские специализации

### FIX-4. IELTS -- полная поддержка
- [x] ExamType: "IELTS" в SeedExamTypesAsync
- [x] Секции: Listening (0-9), Reading (0-9), Writing (0-9), Speaking (0-9)
- [x] 8 тем: Listening Comprehension, Reading Academic Texts, Writing Task 1/Task 2, Speaking Part 1/2/3, Vocabulary
- [x] Скиллы: привязка к SK_READ, SK_WRITE, SK_LISTEN, SK_SPEAK
- [x] 32 вопроса с IRT параметрами, hints, explanations
- [x] Уроки (TopicLessons) для всех 8 тем
- [x] TopicDependencies для IELTS
- [x] Mock Exam: IELTS Practice Test
- [x] Формулы: IELTS writing templates, academic vocabulary patterns
- [x] Стратегии: гайды для IELTS
- [x] Flashcard deck: IELTS Academic Vocabulary

### FIX-5. CSCA -- полная поддержка
- [x] ExamType: "CSCA" в SeedExamTypesAsync
- [x] Секции: Math Analysis (0-100), Logical Reasoning (0-100)
- [x] 6 тем: Calculus Basics, Probability & Statistics, Logical Deduction, Data Interpretation, Algebra & Functions, Spatial Reasoning
- [x] 24 вопроса с IRT параметрами, hints, explanations
- [x] Уроки (TopicLessons) для всех 6 тем
- [x] TopicDependencies для CSCA
- [x] Mock Exam: CSCA Practice Test
- [x] Формулы: CSCA math formulas
- [x] Стратегии: гайды для CSCA

### FIX-6. Post-registration guided tour
- [x] Компонент GuidedTour -- overlay с подсветкой элементов, шаги с описанием
- [x] 6 шагов: "Выбранные экзамены", "Обучение", "Практика", "Прогресс", "План", "Профиль"
- [x] Сохранение в localStorage (guidedTourCompleted)
- [x] Запускается один раз после первого входа / завершения онбординга

### FIX-7. Доработка Learning v2 компонентов
- [x] FlashcardPage: добавить возможность добавлять карточки в деку, пустое состояние
- [x] TimedDrillPage: обработка пустого состояния когда нет вопросов
- [x] MistakeJournalPage: обработка пустого examCode
- [x] FormulaPage: пустое состояние без экзамена
- [x] StrategyPage: пустое состояние без экзамена

---

## ПЛАН РАБОТ -- Sprint 2 (Polish) 2 марта 2026

### FIX-8. KaTeX / иконки в тёмной теме
- [x] Добавить CSS override для KaTeX цветов в `[data-theme="dark"]` — `.katex { color: var(--text-primary) }`
- [x] Bookmark иконки в FormulaPage — контрастный цвет для dark mode (★/☆ Unicode stars)
- [x] TimedDrillPage — проверить видимость текста ответов

### FIX-9. Marathon drill — countdown таймер
- [x] Добавить timeLimitSeconds для Marathon (180 секунд = 3 минуты)
- [x] Frontend: countdown вместо секундомера, auto-complete при timeLeft=0
- [x] Backend: не требуется — таймер на фронтенде

### FIX-10. Mock Exams — показ всех 5 экзаменов
- [x] Incremental seed: SeedIeltsCscaMockExamsAsync, SeedIeltsCscaSectionsAsync, SeedIeltsCscaTopicsAsync
- [x] Фронтенд MockExamPage не фильтрует — бэкенд теперь возвращает все 5 active mock exams

### FIX-11. Целевой балл по экзамену
- [x] GoalFormModal: EXAM_SCORE_CONFIG map с min/max/step/default по коду экзамена
- [x] SAT: 400–1600 (шаг 10), TOEFL: 0–120 (шаг 1), NUET: 0–200 (шаг 1), IELTS: 0–9 (шаг 0.5), CSCA: 0–100 (шаг 1)
- [x] Score сбрасывается на default при смене экзамена

### FIX-12. "Что если" — корректный % в dropdown
- [x] Добавлен IrtLevel в TopicProgressDto (backend: из UserSkillProfile.Theta через IrtMath.ThetaToLevel)
- [x] Frontend TopicProgress.irtLevel — dropdown показывает IRT level вместо masteryPercentage
- [x] Теперь dropdown и результат WhatIf используют одну систему уровней

### FIX-13. История сессий — фильтрация пустых
- [x] Backend: GetSessionsAsync фильтрует TotalQuestions > 0
- [x] examTypeName "Gaokao" — корректное имя для CSCA, не баг

### FIX-14. Больше контента (seed)
- [x] Incremental seed: SeedIeltsCscaFormulaCardsAsync (3 IELTS + 8 CSCA)
- [x] Incremental seed: SeedIeltsCscaFlashcardDecksAsync (IELTS 8 cards + CSCA 8 cards)
- [x] Incremental seed: SeedIeltsCscaStrategyGuidesAsync (2 IELTS + 2 CSCA)

---

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
| **Вопросов в базе** | 137 (30 тем, 5 экзаменов: SAT, TOEFL, NUET, IELTS, CSCA) |

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

## 🎓 Тьюторская система — Статус и план доработок

> **Последнее обновление**: 1 марта 2026
> **Текущий статус**: MVP реализовано, real-time чат сломан, нет enrollment flow, тьюторы видят студенческий UI

### Что уже реализовано ✅

| Компонент | Статус | Файлы |
|-----------|--------|-------|
| 5 доменных сущностей (TutorProfile, TutorScheduleSlot, TutorReview, Conversation, Message) | ✅ | `Domain/Entities/` |
| EF Core миграция `AddTutoringAndMessaging` | ✅ | `Migrations/` |
| 15 DTO (каталог, профиль, расписание, отзывы, сообщения) | ✅ | `Application/DTOs/TutorDtos.cs` |
| TutorService (каталог, профиль, расписание, отзывы) | ✅ | `Application/Services/TutorService.cs` |
| MessageService (диалоги, сообщения, unread) | ✅ | `Application/Services/MessageService.cs` |
| TutorController (6 endpoints) + MessageController (6 endpoints) | ✅ | `Controllers/` |
| ChatHub (SignalR) — подключение, группы, JWT auth | ✅ | `Hubs/ChatHub.cs` |
| Program.cs — DI, AddSignalR, MapHub, JWT OnMessageReceived | ✅ | `Program.cs` |
| TutorsPage — каталог с поиском/фильтрами/сортировкой/пагинацией | ✅ | `client/src/pages/TutorsPage.tsx` |
| TutorProfilePage — профиль, расписание, отзывы, кнопка "Написать" | ✅ | `client/src/pages/TutorProfilePage.tsx` |
| MessagesPage — split-layout мессенджер | ✅ | `client/src/pages/MessagesPage.tsx` |
| TutorDashboardPage — редактирование профиля, расписание, ученики | ✅ | `client/src/pages/TutorDashboardPage.tsx` |
| chatService.ts — SignalR singleton с auto-reconnect | ✅ | `client/src/services/chatService.ts` |
| Маршруты (/tutors, /tutors/:id, /messages, /tutor/dashboard) | ✅ | `App.tsx` |
| Навигация: 🎓 Тьюторы в navbar, 💬 Сообщения в dropdown | ✅ | `Layout.tsx`, `ProfileDropdown.tsx` |
| Seeder: 3 тьютора, профили, расписание, отзывы, тестовый диалог | ✅ | `DatabaseSeeder.cs` |

---

### 🔴 Критические баги — Real-time чат не работает

#### BUG-T1. ❌ SignalR `ReceiveMessage` — аргументы не совпадают → сообщения не доставляются
- **Где**: Hub отправляет **2 аргумента** (`conversationId`, `message`), клиент слушает **1** (`msg`)
- **Hub**: `Clients.Group(...).SendAsync("ReceiveMessage", conversationId, message)` → `ChatHub.cs:56`
- **Client**: `connection.on('ReceiveMessage', (msg: Message) => ...)` → `chatService.ts:28`
- **Результат**: Клиент получает `conversationId` (число) вместо `Message` объекта. Все поля `msg.text`, `msg.senderId` = undefined. **Real-time сообщения полностью сломаны.** REST-fallback в `MessagesPage.tsx:117-124` частично маскирует проблему.
- **Фикс**: Привести к единой сигнатуре — Hub отправляет 1 объект, клиент читает 1 объект

#### BUG-T2. ❌ Сообщения из ВСЕХ диалогов попадают в активный чат
- **Где**: `MessagesPage.tsx:59-63` — `onMessage` добавляет **каждое** входящее сообщение в `messages[]` без проверки `conversationId`
- **Результат**: Если открыт чат с Тьютором А, сообщение от Тьютора Б появляется в этом же чате
- **Фикс**: Фильтровать по `conversationId` — добавлять в messages только если совпадает с `activeId`

#### BUG-T3. ❌ `UserTyping` отправляет `userId` вместо `userName`
- **Hub**: `SendAsync("UserTyping", conversationId, userId)` → `ChatHub.cs:99` — отправляет **int**
- **Client**: ожидает `(conversationId: number, userName: string)` → `chatService.ts:36`
- **Результат**: Индикатор печати показывает "42" вместо "Елена Смирнова"
- **Фикс**: Hub должен резолвить userName из базы перед отправкой

#### BUG-T4. ❌ Typing indicator не привязан к активному диалогу
- **Где**: `MessagesPage.tsx:77` — `setTypingUser(userName)` ставится без проверки `_convId === activeId`
- **Результат**: Печатание в любом диалоге отображается в текущем открытом
- **Фикс**: Проверять `conversationId === activeId`

#### BUG-T5. ❌ `GetUnreadCount` — формат ответа не совпадает
- **Controller**: `return Ok(new { count })` → объект `{ count: N }`
- **Client**: `api.get<number>(...)` → ожидает число `N`
- **Результат**: Клиент получит `[object Object]` или `NaN` при отображении
- **Фикс**: Controller должен вернуть `Ok(count)` или клиент должен читать `.count`

---

### 🟠 Архитектурные пробелы — тьюторы = обычные студенты

#### GAP-T1. ❌ Нет тьюторского Layout — тьюторы видят студенческий UI

**Проблема**: Все не-admin пользователи проходят через `StudentRoutes()` → `<Layout />`. Тьютор видит навигацию "Главная → Обучение → Прогресс → План → Тьюторы" — студенческие страницы, которые для него бесполезны. Ссылка "🎓 Тьюторы" ведёт на каталог *других* тьюторов.

**Как на реальных платформах** (Preply, Italki, Wyzant):
- Тьютор имеет **отдельный интерфейс** с навбаром: Dashboard → Ученики → Сообщения → Расписание → Профиль
- Главная тьютора = сводка: новые заявки, непрочитанные сообщения, расписание на сегодня, рейтинг
- Нет доступа к студенческим "Обучение", "Прогресс", "План"

**Решение**: `TutorLayout` + `TutorRoutes()` в `App.tsx` (по аналогии с `AdminLayout` + `AdminRoutes()`)

#### GAP-T2. ❌ Нет enrollment flow (заявка → принятие)

**Проблема**: Сейчас любой студент может написать любому тьютору. "Ученик" — это просто пользователь с активным диалогом. Тьютор не может:
- Принять/отклонить заявку
- Заблокировать студента
- Видеть "новые заявки" отдельно

**Как на реальных платформах**:
- **Preply**: студент бронирует пробное занятие → тьютор подтверждает → становятся связаны
- **Italki**: студент отправляет запрос → тьютор принимает/отклоняет → открывается чат
- **Wyzant**: студент описывает задачу → подходящие тьюторы получают уведомление → откликаются

**Решение**:
1. Новый статус `ConversationStatus.Pending` (студент отправил заявку, тьютор ещё не ответил)
2. Тьютор видит блок "Новые заявки" на дашборде → кнопки "✅ Принять" / "❌ Отклонить"
3. При принятии — статус → `Active`, открывается чат, системное сообщение "Тьютор принял вашу заявку"
4. При отклонении — статус → `Declined`, студент видит уведомление

#### GAP-T3. ❌ Нет тьюторского Dashboard (полноценного)

**Проблема**: Текущий `TutorDashboardPage` — это форма редактирования профиля. Нет:
- Сводки (непрочитанные, заявки, расписание на сегодня)
- Быстрых действий
- Аналитики (рейтинг за неделю, кол-во новых учеников)

**Решение**: Полноценная главная страница тьютора с карточками-виджетами

#### GAP-T4. ❌ Нет online/presence индикаторов

**Проблема**: Нет видимости, онлайн ли тьютор/студент. Неизвестно, когда ждать ответа.

**Как на реальных платформах**: зелёная точка "Онлайн" / серая "Был(а) 2ч назад"

**Решение**: SignalR + Redis (или in-memory) для трекинга presence. Поле `LastSeenAt` в User.

#### GAP-T5. ❌ Нет архивации/удаления диалогов

**Проблема**: `ConversationStatus.Archived` есть в enum, но нет API/UI для архивации

#### GAP-T6. ❌ Нет верификации тьюторов через admin panel

**Проблема**: `IsVerified` есть в модели, но нет admin-эндпоинта и UI для верификации

---

### 📋 План реализации — Фаза 2 тьюторской системы

#### Этап T-6: Критические баг-фиксы мессенджера 🔴
> **Приоритет: ВЫСШИЙ — без этого чат не работает**

- [x] **T-6.1** Fix SignalR `ReceiveMessage` аргументы (Hub → 1 объект с `conversationId` внутри)
- [x] **T-6.2** Fix `MessagesPage` — фильтрация сообщений по `conversationId` перед добавлением
- [x] **T-6.3** Fix `UserTyping` — Hub резолвит `userName` из БД, клиент фильтрует по `conversationId`
- [x] **T-6.4** Fix `GetUnreadCount` — `Ok(count)` вместо `Ok(new { count })`
- [x] **T-6.5** Fix `GetMyStudentsAsync` — отдельный `StudentDto` вместо пустого `TutorCardDto`
- [x] **T-6.6** Интеграционно проверить: отправка → доставка → прочтение (e2e)

#### Этап T-7: Enrollment flow (заявка → принятие) 🟠
> **Студент кидает заявку, тьютор решает — принять или отклонить**

**Backend:**
- [x] **T-7.1** Расширить `ConversationStatus`: `Pending` → `Active` / `Declined` / `Archived`
- [x] **T-7.2** EF миграция: новые значения enum, добавить `DeclinedAt`, `DeclineReason` в `Conversation`
- [x] **T-7.3** `StartConversationAsync` → создаёт со статусом `Pending` (а не `Active`)
- [x] **T-7.4** `ITutorService.AcceptStudentAsync(tutorId, conversationId)` → статус `Active`, системное сообщение
- [x] **T-7.5** `ITutorService.DeclineStudentAsync(tutorId, conversationId, reason?)` → статус `Declined`
- [x] **T-7.6** `MessageService.SendMessageAsync` — запретить отправку в `Pending`/`Declined` диалоги (кроме системных)
- [x] **T-7.7** `ITutorService.GetPendingRequestsAsync(tutorId)` — список ожидающих заявок
- [x] **T-7.8** Endpoints: `POST /api/tutors/requests/{id}/accept`, `POST /api/tutors/requests/{id}/decline`
- [x] **T-7.9** SignalR: уведомление тьютору `NewRequest`, студенту `RequestAccepted` / `RequestDeclined`

**Frontend:**
- [x] **T-7.10** `TutorProfilePage` — при нажатии "💬 Написать" показать модалку с сообщением-заявкой (имя, цель, текст)
- [x] **T-7.11** После отправки заявки — статус "⏳ Заявка отправлена, ожидайте ответа тьютора"
- [x] **T-7.12** В списке диалогов — `Pending` диалоги отмечены бэйджем "Ожидает"
- [x] **T-7.13** В чате `Pending` — заблокировать поле ввода, показать "Тьютор ещё не принял заявку"

#### Этап T-8: Tutor Layout + персонализированные страницы 🟠
> **Тьюторы получают свой интерфейс, как Admin имеет свой**

**Инфраструктура:**
- [x] **T-8.1** `TutorLayout.tsx` — отдельный layout с навбаром: 🏠 Главная → 👥 Ученики → 💬 Сообщения → 📅 Расписание → 👤 Профиль
- [x] **T-8.2** `TutorRoutes()` в `App.tsx` — отдельный набор маршрутов для `role === 'Tutor'`
- [x] **T-8.3** Маршруты: `/` (dashboard) → `/students` → `/messages` → `/schedule` → `/profile` → `/reviews`

**Страницы:**
- [x] **T-8.4** `TutorHomePage.tsx` — сводка-dashboard тьютора:
  - Карточки: ⏳ Новые заявки (счётчик) → 💬 Непрочитанные → 📅 Занятия сегодня → ★ Рейтинг
  - Блок "Новые заявки" с кнопками ✅ Принять / ❌ Отклонить
  - Последние сообщения (preview)
  - Расписание на сегодня
- [x] **T-8.5** `TutorStudentsPage.tsx` — список учеников:
  - Карточки: аватар, имя, дата начала, последний контакт
  - Фильтр: активные / архив
  - Клик → открывает диалог в `/messages`
- [x] **T-8.6** `TutorSchedulePage.tsx` — визуальный редактор расписания:
  - Сетка 7 дней × 24 часа (выделение мышкой)
  - Перетаскивание слотов
  - Кнопка "Сохранить"
- [x] **T-8.7** `TutorProfileEditPage.tsx` — полноэкранное редактирование профиля:
  - Live-preview (как будет выглядеть карточка)
  - Drag-and-drop для аватара
  - Markdown preview для bio
- [x] **T-8.8** `TutorReviewsPage.tsx` — все отзывы о тьюторе:
  - Фильтр по рейтингу
  - Статистика: средний балл, распределение оценок (5★ = 70%, 4★ = 20%, ...)

#### Этап T-9: Online/Presence и UX-улучшения мессенджера 🟡

- [x] **T-9.1** Поле `LastSeenAt` (DateTime?) в User + миграция
- [x] **T-9.2** `ChatHub.OnConnectedAsync` → обновляет `LastSeenAt`, добавляет в in-memory set `_onlineUsers`
- [x] **T-9.3** `ChatHub.OnDisconnectedAsync` → обновляет `LastSeenAt`, удаляет из set
- [x] **T-9.4** API: `GET /api/users/{id}/presence` → `{ isOnline, lastSeenAt }`
- [x] **T-9.5** Frontend: зелёная/серая точка в карточках тьюторов и в шапке чата
- [x] **T-9.6** "Был(а) в сети 2 ч назад" / "Онлайн" текст
- [x] **T-9.7** Архивация диалогов — кнопка "📦 Архивировать" в шапке чата + вкладка "Архив" в списке
- [x] **T-9.8** Уведомление sound/badge при новом сообщении (Web Notification API, если вкладка не активна)
- [x] **T-9.9** Unread badge на иконке 💬 в навбаре (live-update через SignalR)
- [x] **T-9.10** Scroll-to-bottom кнопка при скролле вверх + lazy-load старых сообщений

#### Этап T-10: Admin-модерация тьюторов 🟡

- [x] **T-10.1** `AdminTutorsPage.tsx` — список тьюторов с действиями:
  - Верифицировать / снять верификацию
  - Заблокировать / разблокировать
  - Просмотреть профиль, отзывы, диалоги
- [x] **T-10.2** API: `POST /api/admin/tutors/{id}/verify`, `POST /api/admin/tutors/{id}/block`
- [x] **T-10.3** AuditLog записи для всех admin-действий с тьюторами
- [x] **T-10.4** Email-уведомление тьютору при верификации: "Ваш профиль подтверждён ✓"

---

### Порядок реализации (Фаза 2)

```
T-6 (баг-фиксы)  ──→  T-7 (enrollment)  ──→  T-8 (tutor UI)  ──→  T-9 (presence)  ──→  T-10 (admin)
   🔴 КРИТИЧНО          🟠 ВАЖНО               🟠 ВАЖНО            🟡 УЛУЧШЕНИЕ        🟡 УЛУЧШЕНИЕ
   ~1ч                   ~3ч                     ~4ч                  ~2ч                  ~1ч
```

| Шаг | Этап | Описание | Зависимости | Сложность |
|-----|------|----------|-------------|-----------|
| 1 | T-6 | Баг-фиксы мессенджера (5 багов) | — | 🟢 Быстро |
| 2 | T-7 | Enrollment flow (заявка → принятие/отклонение) | T-6 | 🟡 Средне |
| 3 | T-8.1–8.3 | TutorLayout + TutorRoutes | — | 🟡 Средне |
| 4 | T-8.4 | TutorHomePage (dashboard тьютора) | T-7, T-8.1 | 🟡 Средне |
| 5 | T-8.5–8.8 | Остальные тьюторские страницы | T-8.1 | 🟡 Средне |
| 6 | T-9 | Presence + UX мессенджера | T-6 | 🟡 Средне |
| 7 | T-10 | Admin-модерация тьюторов | T-8 | 🟢 Быстро |

> **Итого Фаза 2**: ~11ч. После реализации — полноценная тьюторская маркетплейс-система уровня Preply/Italki lite.

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

---

## T-12: Рекомендации, навигация и UX (аудит 02.03.2026)

### Выявленные проблемы

#### 1. Эмодзи в рекомендациях
- **Где**: `RecommendationService.cs` — поля `Icon` содержат эмодзи (🧠, ⚠️, 📝, 🌟, 💪, 🎯, 🔥, ⏱️)
- **Проблема**: Рекомендации на Dashboard отображают эмодзи вместо нейтральных иконок
- **Фикс**: Убрать эмодзи из `Icon` — поставить `null`. Frontend уже обрабатывает `r.icon || ''`

#### 2. «0 из ∞» — непонятное отображение для Pro
- **Где**: `DashboardPage.tsx` строка 84 — `usage.questionsLimit === -1 ? '∞' : usage.questionsLimit`
- **Проблема**: Pro-пользователи видят «0 из ∞» — неинформативно, выглядит как ошибка
- **Фикс**: Для Pro показывать «Безлимит» вместо «из ∞»

#### 3. Кнопки рекомендаций ведут на `/test?topicId=X` — нерабочий роут
- **Где**: `RecommendationService.cs` — все `ActionUrl` генерируются как `/test?topicId=X`, `/test?mode=exam`, `/test`
- **Проблема**: Во фронтенде нет роута `/test`. Практика живёт на `/learn?tab=practice&topicId=X`, mock — `/learn?tab=mock`. При переходе по `/test` пользователь попадает на «Практика завершена»
- **Фикс**: Заменить все `ActionUrl` в бэкенде: `/test?topicId=X` → `/learn?tab=practice&topicId=X`, `/test?mode=exam` → `/learn?tab=mock`, `/test` → `/learn`

#### 4. «Практика завершена» / «Задание выполнено» при первом входе
- **Где**: `TestPage.tsx` — состояние `testCompleted` в Redux persist'ится между навигациями
- **Проблема**: После завершения практики `testCompleted = true` остаётся в store. При следующем переходе из рекомендации/плана по topicId пользователь сразу видит «Практика завершена», не ответив ни на один вопрос
- **Фикс**: При монтировании TestPage с `topicId` — dispatch `resetTest()` перед авто-стартом, чтобы сбросить stale-состояние

#### 5. Рекомендации на странице Прогноза без ссылок
- **Где**: `PredictionPage.tsx` — компонент `ImprovementTips` (строки 420–458)
- **Проблема**: Советы «Усильте практику X» показываются как текст без кнопок/ссылок. Пользователь не может одним кликом перейти к практике по теме
- **Фикс**: Добавить `topicId` в `ImprovementTipDto` (бэкенд) и `ImprovementTip` (фронт). Отрисовать кнопку «Практика →» с навигацией на `/learn?tab=practice&topicId=X`

### Файлы затронутые

| Файл | Изменение |
|------|-----------|
| `Application/Services/RecommendationService.cs` | Убрать эмодзи из Icon, исправить ActionUrl |
| `Application/DTOs/PredictionDtos.cs` | Добавить TopicId в ImprovementTipDto |
| `Application/Services/ScorePredictionService.cs` | Передавать topic.Id в tip |
| `client/src/pages/DashboardPage.tsx` | Исправить «0 из ∞» |
| `client/src/pages/TestPage.tsx` | resetTest при монтировании с topicId |
| `client/src/pages/PredictionPage.tsx` | Кнопка «Практика →» в ImprovementTips |
| `client/src/types/index.ts` | Добавить topicId в ImprovementTip |

*Создано: 27 февраля 2026*

---

## 📚 Раздел «Обучение» v2 — Расширение контента, методик и экзаменов

> **Последнее обновление**: 2 марта 2026
> **Цель**: Превратить раздел «Обучение» из простого тренажёра вопросов в полноценную обучающую платформу с разнообразными режимами, глубокой теорией и поддержкой 5 экзаменов.

---

### 🌍 Часть A: Новые экзамены — IELTS + CSCA (Gaokao)

#### Текущее состояние

| Экзамен | Code | Вопросов | Секций | Статус |
|---------|------|----------|--------|--------|
| SAT | `SAT` | 40 | 3 (R&W, Math No Calc, Math Calc) | ✅ Готов |
| TOEFL iBT | `TOEFL` | 28 | 4 (Reading, Listening, Speaking, Writing) | ✅ Готов |
| NUET | `NUET` | 29 | 2 (Math, Critical Thinking) | ✅ Готов |
| **IELTS Academic** | `IELTS` | — | — | ⬜ Планируется |
| **CSCA (Gaokao)** | `CSCA` | — | — | ⬜ Планируется |

---

#### EX-1. IELTS Academic 🇬🇧

**Формат экзамена:**
- **Listening** (30 мин, 40 вопросов) — 4 записи с заполнением пропусков, множественный выбор, matching
- **Reading** (60 мин, 40 вопросов) — 3 академических текста, True/False/Not Given, matching headings, sentence completion
- **Writing** (60 мин, 2 задания) — Task 1: описание графика/диаграммы (150+ слов), Task 2: эссе-аргументация (250+ слов)
- **Speaking** (11–14 мин, 3 части) — Part 1: знакомство, Part 2: монолог (cue card), Part 3: дискуссия
- **Шкала**: 0–9 (band score, шаг 0.5) по каждой секции + Overall Band

**Что реализуем:**

| Компонент | Описание | Сложность |
|-----------|----------|-----------|
| ExamType `IELTS` | Code: `IELTS`, Name: `IELTS Academic` | 🟢 |
| 4 секции | Listening (0–9), Reading (0–9), Writing (0–9), Speaking (0–9) | 🟢 |
| Вопросы Reading | 30+ вопросов: True/False/Not Given, matching, fill-in-the-blank, MCQ | 🟡 |
| Вопросы Listening | 20+ вопросов: MCQ, sentence completion, matching (с аудио-контекстом описанным текстом) | 🟡 |
| Reading Passages | 3+ академических текста (по аналогии с TOEFL Reading) | 🟡 |
| Mock Exam IELTS | Полный формат: 4 секции, 80 вопросов, 2ч 45мин | 🔴 |
| θ→Band Score маппинг | `θ`: -3..+3 → Band: 1.0–9.0 (шаг 0.5) | 🟢 |
| Темы | 8–10 тем: Paraphrasing, Skimming & Scanning, Sentence Completion, Matching Headings, T/F/NG, Vocabulary in Context, Listening for Specific Info, Note Completion | 🟢 |
| Мини-уроки | По 1–2 урока на тему (стратегии IELTS Reading, Listening tips) | 🟡 |

**Задачи:**
- [ ] **EX-1.1** Seeder: ExamType `IELTS`, 4 ExamSection, 8–10 Topic, Skills, TopicDependencies
- [ ] **EX-1.2** Seeder: 30+ Reading вопросов с IRT-параметрами + 3 Reading Passages
- [ ] **EX-1.3** Seeder: 20+ Listening вопросов (текстовое описание аудио-контекста)
- [ ] **EX-1.4** Mock Exam definition: IELTS Practice Test (80 вопросов, 165 мин, 4 секции)
- [ ] **EX-1.5** ScorePredictionService: маппинг θ → IELTS Band Score (0–9, шаг 0.5)
- [ ] **EX-1.6** Frontend: карточка IELTS в онбординге, профиле, dashboard
- [ ] **EX-1.7** Мини-уроки: 10+ уроков по стратегиям IELTS (Reading skimming, T/F/NG technique, Listening note-taking)
- [ ] **EX-1.8** Уникальные типы вопросов: True/False/Not Given (новый UI-компонент вместо MCQ)

---

#### EX-2. CSCA / Gaokao (Китай) 🇨🇳

**Формат экзамена:**
- **Gaokao** — единый государственный вступительный экзамен Китая (~10 млн сдающих/год)
- **Обязательные предметы**: Chinese (语文), Math (数学), Foreign Language (外语, обычно English)
- **Для платформы** реализуем Math + English секции (наиболее релевантные для международной аудитории)
- **Math**: алгебра, геометрия, тригонометрия, статистика, последовательности, функции, производные
- **English**: reading comprehension, cloze test (fill-in-the-blank), grammar, vocabulary, writing
- **Шкала**: 0–150 по каждому предмету

**Что реализуем:**

| Компонент | Описание | Сложность |
|-----------|----------|-----------|
| ExamType `CSCA` | Code: `CSCA`, Name: `Gaokao (China College Admission)` | 🟢 |
| 2 секции | Math (0–150), English (0–150) | 🟢 |
| Вопросы Math | 25+ вопросов: алгебра, геометрия, тригонометрия, функции, производные | 🟡 |
| Вопросы English | 20+ вопросов: reading, cloze, grammar, vocabulary | 🟡 |
| Mock Exam CSCA | Math (120 мин) + English (120 мин) | 🔴 |
| θ→Score маппинг | `θ`: -3..+3 → Score: 0–150 | 🟢 |
| Темы | 10–12 тем: Algebra, Functions, Trigonometry, Sequences, Derivatives, Geometry, Probability & Statistics, Reading Comprehension, Cloze Test, Grammar & Usage, Vocabulary | 🟢 |
| Мини-уроки | Формулы + стратегии по каждой теме | 🟡 |

**Задачи:**
- [ ] **EX-2.1** Seeder: ExamType `CSCA`, 2 ExamSection, 10–12 Topic, Skills, TopicDependencies
- [ ] **EX-2.2** Seeder: 25+ Math вопросов (включая производные и тригонометрию)
- [ ] **EX-2.3** Seeder: 20+ English вопросов (cloze test — новый формат)
- [ ] **EX-2.4** Mock Exam definition: Gaokao Practice Test (45 вопросов, 240 мин, 2 секции)
- [ ] **EX-2.5** ScorePredictionService: маппинг θ → Gaokao Score (0–150)
- [ ] **EX-2.6** Frontend: карточка CSCA в онбординге, профиле, dashboard
- [ ] **EX-2.7** Мини-уроки: 12+ уроков (формулы тригонометрии, производные, грамматика Gaokao English)
- [ ] **EX-2.8** Cloze test UI: текст с пропусками, выбор слова из вариантов (новый компонент)

---

### 📖 Часть B: Улучшение теории и уроков

#### Текущее состояние теории

Сейчас: 19 мини-уроков (Markdown-текст + опциональное YouTube-видео). Формат: `TopicLesson { Title, Content (markdown), VideoUrl, SortOrder }`. Отображается в `TopicsPage.tsx` → вкладка "Урок" перед практикой. **Проблемы:**
- Контент чисто текстовый, нет интерактивных элементов
- Нет проверки понимания после прочтения урока
- Нет структурирования: формулы отдельно, стратегии отдельно
- Нет прогресса по теории (прочитал/не прочитал)

#### TH-1. Структурированные уроки с прогрессом 📖

**Идея**: Разбить каждый урок на шаги (steps). Пользователь проходит урок шаг за шагом с трекингом прогресса.

**Модель:**
```
TopicLesson (существует)
  └── LessonStep (новая)
       ├── Id, LessonId, SortOrder
       ├── StepType: "theory" | "example" | "quiz" | "summary"
       ├── Title: string
       ├── Content: string (markdown)
       ├── QuizQuestionId: int? (связь с Question для проверки)
       └── UserLessonProgress (новая)
            ├── UserId, LessonStepId, CompletedAt
```

**UX:**
1. Урок = пошаговый wizard (1/5 → 2/5 → ... → 5/5)
2. Шаг "theory" = текст + формулы (KaTeX) + картинки
3. Шаг "example" = разобранный пример с пошаговым решением (spoiler для каждого шага)
4. Шаг "quiz" = 1–2 вопроса для проверки понимания (inline, не уходя со страницы)
5. Шаг "summary" = ключевые выводы + формулы в рамке
6. Прогресс-бар урока, бейджи "📗 Прочитано" на карточке темы

**Задачи:**
- [ ] **TH-1.1** Domain: сущность `LessonStep` (Id, LessonId, StepType, Title, Content, QuizQuestionId, SortOrder)
- [ ] **TH-1.2** Domain: сущность `UserLessonProgress` (UserId, LessonStepId, CompletedAt)
- [ ] **TH-1.3** EF миграция: таблицы `LessonSteps`, `UserLessonProgress`
- [ ] **TH-1.4** LessonService: `GetLessonWithStepsAsync`, `MarkStepCompletedAsync`, `GetLessonProgressAsync`
- [ ] **TH-1.5** API: `GET /api/lessons/{id}/steps`, `POST /api/lessons/steps/{stepId}/complete`, `GET /api/lessons/{id}/progress`
- [ ] **TH-1.6** Frontend: `LessonViewer.tsx` — пошаговый wizard с прогресс-баром, навигацией "Назад/Далее"
- [ ] **TH-1.7** Frontend: inline quiz-компонент (вопрос прямо в уроке, без перехода на отдельную страницу)
- [ ] **TH-1.8** TopicsPage: бейдж "✓ Пройдено" / "3/5 шагов" на карточке темы
- [ ] **TH-1.9** Seeder: переработать 19 существующих уроков в step-формат (theory → example → quiz → summary)

---

#### TH-2. Формульный справочник (Formula Sheet) 📐

**Идея**: Отдельный раздел / вкладка "Формулы" — быстрый доступ ко всем формулам по экзамену. Как cheat sheet перед экзаменом.

**UX:**
- Группировка по темам (Algebra → Quadratic Formula, Factoring, ...; Geometry → Area, Volume, ...)
- Поиск по формулам (быстрый фильтр)
- KaTeX рендеринг формул
- Кнопка "⭐ В избранное" — пользователь собирает свой набор формул для повторения
- Печатная версия (Print CSS)

**Модель:**
```
FormulaCard
  ├── Id, TopicId, Title, Formula (KaTeX string), Description, ExamTypeCode
  └── UserFormulaBookmark
       ├── UserId, FormulaCardId, CreatedAt
```

**Задачи:**
- [ ] **TH-2.1** Domain: сущность `FormulaCard` (Id, TopicId, Title, Formula, Description, SortOrder)
- [ ] **TH-2.2** Domain: сущность `UserFormulaBookmark` (UserId, FormulaCardId, CreatedAt)
- [ ] **TH-2.3** EF миграция
- [ ] **TH-2.4** FormulaService: `GetFormulasByExamAsync`, `ToggleBookmarkAsync`, `GetBookmarkedAsync`
- [ ] **TH-2.5** API: `GET /api/formulas?examTypeCode=SAT`, `POST /api/formulas/{id}/bookmark`, `GET /api/formulas/bookmarks`
- [ ] **TH-2.6** Frontend: новая вкладка "📐 Формулы" в LearnPage (или отдельная страница)
- [ ] **TH-2.7** Frontend: карточки формул с KaTeX, поиск, фильтр по теме, кнопка закладки
- [ ] **TH-2.8** Seeder: 40+ формул для SAT Math, 20+ для NUET Math, 15+ для CSCA Math

---

#### TH-3. Flashcards — карточки для запоминания 🃏

**Идея**: Система карточек (вопрос ↔ ответ) с алгоритмом интервального повторения (SM-2). Для запоминания: формулы, словарь TOEFL/IELTS, ключевые концепции.

**Алгоритм SM-2 (SuperMemo 2):**
```
После каждого ответа (оценка 0–5):
  if quality >= 3: // правильно
    interval = previous_interval × EF
    EF = max(1.3, EF + 0.1 − (5 − quality) × (0.08 + (5 − quality) × 0.02))
  else: // забыл
    interval = 1 day, repetition = 0

NextReview = now + interval
```

**Модель:**
```
FlashcardDeck
  ├── Id, Title, Description, ExamTypeCode?, TopicId?, IsSystem (bool), CreatedByUserId?
  └── Flashcard
       ├── Id, DeckId, Front (string/markdown), Back (string/markdown), SortOrder
       └── UserFlashcardProgress
            ├── UserId, FlashcardId
            ├── EaseFactor (float, default 2.5)
            ├── Interval (int days)
            ├── Repetitions (int)
            ├── NextReviewAt (DateTime)
            ├── LastReviewedAt (DateTime?)
            └── LastQuality (int 0-5)
```

**UX:**
1. Колоды: системные (SAT Vocabulary, TOEFL Academic Words, Math Formulas, IELTS Key Phrases) + пользовательские
2. Режим изучения: показ Front → пользователь думает → кнопка "Показать ответ" → Back → оценка (Не помню / Трудно / Нормально / Легко)
3. Dashboard: "📦 12 карточек на повторение сегодня"
4. Прогресс: New / Learning / Review / Mastered
5. Аналитика: % запоминания, streak повторений

**Задачи:**
- [ ] **TH-3.1** Domain: сущности `FlashcardDeck`, `Flashcard`, `UserFlashcardProgress`
- [ ] **TH-3.2** EF миграция
- [ ] **TH-3.3** FlashcardService: SM-2 алгоритм, `GetDueCardsAsync`, `ReviewCardAsync`, `GetDeckProgressAsync`
- [ ] **TH-3.4** FlashcardService: `CreateDeckAsync`, `AddCardAsync` (пользовательские колоды)
- [ ] **TH-3.5** API: `GET /api/flashcards/decks`, `GET /api/flashcards/decks/{id}/due`, `POST /api/flashcards/review`, `POST /api/flashcards/decks` (CRUD)
- [ ] **TH-3.6** Frontend: новая вкладка "🃏 Карточки" в LearnPage
- [ ] **TH-3.7** Frontend: `FlashcardStudy.tsx` — flipcard-анимация, кнопки оценки, прогресс-бар
- [ ] **TH-3.8** Frontend: `FlashcardDecks.tsx` — список колод с прогрессом (new/learning/review/mastered)
- [ ] **TH-3.9** Frontend: создание своих колод и карточек
- [ ] **TH-3.10** Seeder: системные колоды (SAT Vocabulary 50 слов, TOEFL Academic Words 50, Math Formulas 30, IELTS Key Phrases 30)
- [ ] **TH-3.11** Рекомендация на Dashboard: "У вас 15 карточек на повторение" → ссылка

---

#### TH-4. Timed Drills — режим на скорость ⏱️

**Идея**: Короткие блиц-сессии (5–10 мин) с таймером. Тренировка скорости, имитация тайм-прессинга экзамена. Gamification-элемент.

**Режимы:**
1. **Speed Round** — 10 вопросов, 60 секунд каждый. Счётчик правильных + среднее время
2. **Marathon** — максимум вопросов за 5/10/15 минут. Leaderboard-friendly
3. **Streak Challenge** — отвечай правильно подряд. Одна ошибка = конец. Best streak counter

**Модель:**
```
TimedDrillResult
  ├── Id, UserId, DrillType ("speed" | "marathon" | "streak")
  ├── ExamTypeCode, TopicId?
  ├── QuestionsAnswered, CorrectAnswers
  ├── TotalTimeSeconds, AverageTimeSeconds
  ├── BestStreak
  ├── StartedAt, CompletedAt
```

**UX:**
1. Выбор режима → выбор темы (опционально) → старт
2. Таймер + счётчик + streak-визуализация (огонёк 🔥)
3. Результат: score, comparison с прошлыми попытками, "Побит личный рекорд!"
4. Мини-лидерборд (личный): лучшие результаты по каждому режиму

**Задачи:**
- [ ] **TH-4.1** Domain: сущность `TimedDrillResult`
- [ ] **TH-4.2** EF миграция
- [ ] **TH-4.3** TimedDrillService: `StartDrillAsync`, `GetPersonalBestsAsync`, `SaveResultAsync`
- [ ] **TH-4.4** API: `POST /api/drills/start`, `POST /api/drills/complete`, `GET /api/drills/personal-bests`
- [ ] **TH-4.5** Frontend: новая вкладка "⏱️ Блиц" в LearnPage
- [ ] **TH-4.6** Frontend: `TimedDrill.tsx` — таймер, вопрос, мгновенный feedback, streak-counter
- [ ] **TH-4.7** Frontend: экран результатов с comparison vs personal best
- [ ] **TH-4.8** Рекомендация: "Пройдите блиц по Algebra — ваш рекорд: 8/10"

---

#### TH-5. Strategy Guides — стратегии сдачи экзаменов 🎯

**Идея**: Отдельный раздел со стратегиями и тактиками конкретного экзамена. Не привязан к теме — привязан к экзамену. Важно для SAT, IELTS, TOEFL где знание стратегий даёт +50–100 баллов.

**Контент (примеры):**
- **SAT**: "Process of Elimination", "Back-solving", "Plugging In Numbers", "Reading: Main Idea First", "Time Management: 1.25 min per question"
- **TOEFL**: "Note-taking for Listening", "Template for Independent Writing", "Transition words for Speaking"
- **IELTS**: "Skimming vs Scanning", "True/False/Not Given technique", "Band 7+ Writing structure"
- **NUET**: "Критическое мышление: логические ошибки", "Скорочтение графиков"
- **CSCA**: "Gaokao Math: порядок решения", "Cloze test: контекстные подсказки"

**Модель:**
```
StrategyGuide
  ├── Id, ExamTypeCode, Title, Summary, Content (markdown)
  ├── Category: "test-taking" | "time-management" | "section-specific" | "mental"
  ├── EstimatedReadMinutes, SortOrder
  └── UserGuideProgress
       ├── UserId, GuideId, ReadAt
```

**Задачи:**
- [ ] **TH-5.1** Domain: сущности `StrategyGuide`, `UserGuideProgress`
- [ ] **TH-5.2** EF миграция
- [ ] **TH-5.3** StrategyService: `GetGuidesByExamAsync`, `MarkReadAsync`
- [ ] **TH-5.4** API: `GET /api/strategies?examTypeCode=SAT`, `POST /api/strategies/{id}/read`
- [ ] **TH-5.5** Frontend: новая вкладка "🎯 Стратегии" в LearnPage
- [ ] **TH-5.6** Frontend: `StrategyList.tsx` — карточки по категориям, бейдж "✓ Прочитано"
- [ ] **TH-5.7** Frontend: `StrategyViewer.tsx` — чтение с markdown, estimated time, прогресс-чекбокс
- [ ] **TH-5.8** Seeder: 5+ стратегий на каждый экзамен (25+ всего)
- [ ] **TH-5.9** Рекомендация: "Изучите стратегию 'Process of Elimination' для SAT" → ссылка

---

#### TH-6. Mistake Journal — дневник ошибок 📓

**Идея**: Расширение текущей вкладки "Ошибки". Сейчас — просто повторное решение неправильных вопросов. Нужно: анализ паттернов ошибок, группировка по типу ошибки, персональные заметки.

**Что добавим:**
1. **Паттерн-анализ**: "Вы часто путаете значения слов в контексте (5 из 8 ошибок в Reading)" 
2. **Тип ошибки** (тег): "Невнимательность", "Не знал материал", "Не хватило времени", "Ловушка в условии"
3. **Заметки пользователя**: к каждой ошибке можно добавить текстовую заметку ("Запомнить: area of circle = πr², NOT πd²")
4. **Фильтры**: по теме, по типу ошибки, по дате, по экзамену
5. **Экспорт**: PDF / CSV

**Модель:**
```
UserMistakeNote
  ├── Id, UserId, UserAnswerId (FK → UserAnswer)
  ├── ErrorType: "careless" | "knowledge_gap" | "time_pressure" | "trick_question" | null
  ├── NoteText: string?
  ├── CreatedAt, UpdatedAt
```

**Задачи:**
- [ ] **TH-6.1** Domain: сущность `UserMistakeNote`
- [ ] **TH-6.2** EF миграция
- [ ] **TH-6.3** MistakeService: `GetMistakesWithNotesAsync`, `AddNoteAsync`, `SetErrorTypeAsync`, `GetErrorPatternAnalysisAsync`
- [ ] **TH-6.4** API: `GET /api/mistakes?examTypeCode=&topicId=&errorType=`, `POST /api/mistakes/{userAnswerId}/note`, `PUT /api/mistakes/{userAnswerId}/error-type`
- [ ] **TH-6.5** Frontend: переделать ReviewPage → полноценный MistakeJournal с фильтрами, заметками, тег-классификацией
- [ ] **TH-6.6** Frontend: паттерн-анализ (bar chart типов ошибок, топ-3 слабых паттерна)
- [ ] **TH-6.7** Рекомендация: "У вас 70% ошибок — невнимательность. Попробуйте Timed Drill для тренировки внимания"

---

### 🗂 Часть C: Обновлённая структура раздела «Обучение»

#### Было (4 вкладки):
```
📚 Обучение
├── ▶ Практика        — Адаптивный тест (IRT + CAT)
├── 📝 Mock Exam       — Полноформатные тренировочные экзамены
├── 📗 Темы            — Уроки + практика по темам
└── ❌ Ошибки          — Повторение неправильных ответов
```

#### Станет (8 вкладок / под-группы):
```
📚 Обучение
├── 🧠 Учись
│   ├── 📖 Темы & Уроки     — Пошаговые уроки с quiz-проверкой (TH-1)
│   ├── 📐 Формулы           — Справочник формул с закладками (TH-2)
│   ├── 🎯 Стратегии         — Стратегии сдачи по каждому экзамену (TH-5)
│   └── 🃏 Карточки          — Flashcards с SM-2 spaced repetition (TH-3)
│
├── 💪 Тренируйся
│   ├── ▶ Практика           — Адаптивный тест (IRT + CAT) [существует]
│   ├── ⏱️ Блиц              — Timed drills: speed/marathon/streak (TH-4)
│   ├── 📝 Mock Exam          — Полноформатные экзамены [существует]
│   └── 📓 Дневник ошибок    — Анализ + заметки + паттерны (TH-6)
```

**Техническая реализация** — два варианта UI:

**Вариант A** (рекомендуемый): Двухуровневая навигация — 2 группы ("Учись" / "Тренируйся") с под-вкладками. Compact tabs с иконками.

**Вариант B**: Плоский список из 8 вкладок с горизонтальным скроллом (как сейчас, но больше).

**Задачи:**
- [ ] **TH-C.1** LearnPage.tsx: переделать навигацию на двухуровневую (группы + вкладки)
- [ ] **TH-C.2** Роутинг: `/learn?tab=lessons`, `/learn?tab=formulas`, `/learn?tab=strategies`, `/learn?tab=flashcards`, `/learn?tab=drills`, `/learn?tab=mistakes`
- [ ] **TH-C.3** Mobile-адаптация: collapsible группы или horizontal scroll

---

### 📋 Сводная матрица — Расширение обучения

| ID | Задача | Тип | Сложность | Зависимости | Статус |
|----|--------|-----|-----------|-------------|--------|
| **EX-1** | IELTS Academic (вопросы + mock + sections) | Экзамен | Сложно | -- | Done |
| **EX-2** | CSCA (Math Analysis + Logical Reasoning) | Экзамен | Сложно | -- | Done |
| **TH-1** | Структурированные уроки с шагами | Теория | Средне | -- | Done |
| **TH-2** | Формульный справочник + KaTeX рендеринг | Теория | Средне | -- | Done |
| **TH-3** | Flashcards + SM-2 | Методика | Средне | -- | Done |
| **TH-4** | Timed Drills (speed/marathon/streak) | Методика | Средне | -- | Done |
| **TH-5** | Strategy Guides | Контент | Лёгко | -- | Done |
| **TH-6** | Mistake Journal (дневник ошибок) | UX | Средне | -- | Done |
| **TH-C** | Обновлённая навигация LearnPage | UI | Лёгко | TH-1..TH-6 | Done |
| **FIX-1** | KaTeX рендеринг теории и формул | Рендеринг | Средне | -- | Done |
| **FIX-2** | Убрать видеоссылки | Cleanup | Лёгко | -- | Done |
| **FIX-3** | Убрать неакт. экзамены из тьюторов | Cleanup | Лёгко | -- | Done |
| **FIX-6** | Post-registration guided tour | UX | Средне | -- | Done |
| **FIX-7** | Доработка Learning v2 компонентов | Исправления | Средне | TH-1..TH-6 | Done |
| **FIX-4** | IELTS полная поддержка | Контент/Seed | Сложно | -- | Done |
| **FIX-5** | CSCA полная поддержка | Контент/Seed | Сложно | -- | Done |

### Порядок реализации

```
  Фаза 1 (контент)          Фаза 2 (методики)         Фаза 3 (экзамены)
  ─────────────────         ──────────────────         ──────────────────
  TH-1 Уроки с шагами  →   TH-3 Flashcards       →   EX-1 IELTS
  TH-2 Формулы         →   TH-4 Timed Drills     →   EX-2 CSCA
  TH-5 Стратегии       →   TH-6 Mistake Journal  →   TH-C Новая навигация
       ~3–4 дня                  ~3–4 дня                  ~5–7 дней
```

> **Итого**: ~2 недели активной работы. После реализации — полноценная EdTech платформа с 5 экзаменами, 200+ вопросами, flashcards, timed drills, стратегиями и дневником ошибок.
