# UniStart — Прогресс проекта

## Текущий статус: Онбординг флоу завершён — Мастер настройки после регистрации

---

## 🧭 Навигация — Редизайн (v2)

### Проблема

Текущее меню содержит **9 ссылок** в одной строке:

```
Exams | Test | Analytics | History | Plan | Predict | Tips | Mock | 📬
```

Это перегружает новичка, нарушает правило «7 ± 2» и не даёт понять, с чего начать. Вкладка уведомлений (📬) не относится к навигации — это настройка профиля. Mock и Test по сути одно и то же — «прохождение вопросов / экзамена».

---

### Принципы

1. **Прогрессивное раскрытие** — новый пользователь видит только главное; детали открываются по мере погружения
2. **Группировка по задачам** — пользователь думает «хочу учиться», «как мой прогресс?», «кто я?», а не «какая страница мне нужна?»
3. **Профиль = правый верхний угол** — паттерн, знакомый по YouTube, GitHub, Duolingo, Coursera и т.д.
4. **≤ 5 пунктов в основном меню** — золотой стандарт для SaaS/EdTech

---

### Новая структура

#### Основное меню (navbar) — 4 пункта

| # | Название | Иконка | Что внутри | Маршрут |
|---|----------|--------|------------|---------|
| 1 | **Главная** | 🏠 | Dashboard: выбор экзамена, рекомендации дня, streak, быстрые действия | `/` |
| 2 | **Обучение** | 📚 | Единый хаб: Practice (адаптивный тест), Mock Exam (полный формат), темы + уроки, повторение ошибок | `/learn` |
| 3 | **Прогресс** | 📊 | Аналитика, прогноз балла, история сессий, план подготовки | `/progress` |
| 4 | **План** | 📅 | Персональный план, задания на сегодня, недельный обзор | `/plan` |

#### Профиль пользователя (top-right dropdown) — аватар / имя

| Пункт | Описание |
|-------|----------|
| 👤 Профиль | Имя, email, выбранные экзамены, цель, дата экзамена |
| 🔔 Уведомления | Настройки email: streak-reminder, weekly digest, welcome |
| 🌙 / ☀️ Тема | Переключатель тёмной/светлой темы |
| 🚪 Выйти | Logout |

---

### Подробная логика каждого раздела

#### 1. 🏠 Главная (`/`)

Вместо текущего `ExamSelectionPage` — полноценный **Dashboard**, который:
- Показывает streak и статистику за сегодня (вопросов решено / лимит)
- Отображает карточки рекомендаций («Сегодня: Algebra 20 мин + Grammar Review 15 мин»)
- Quick actions: «▶ Продолжить обучение», «📝 Mock Exam», «🔄 Повторить ошибки»
- Badge выбранных экзаменов (SAT, TOEFL, NUET) с кнопкой «Изменить»
- При первом входе (после онбординга) — приветственная карточка

#### 2. 📚 Обучение (`/learn`)

Объединяет 4 текущие страницы в одну с **табами или карточками**:

| Подраздел | Текущая страница | Доступ |
|-----------|-----------------|--------|
| Практика | `TestPage` | Адаптивный тест по выбранным экзаменам |
| Mock Exam | `MockExamPage` | Полноценные тренировочные экзамены с секциями и таймером |
| Темы | `TopicsPage` + `LessonPage` | Список тем → уроки → практика по теме |
| Ошибки | `ReviewPage` | Повторение вопросов с ошибками (spaced repetition) |

> **Почему Mock внутри «Обучение»**: Mock Exam — это формат обучения, а не отдельный продукт. Duolingo объединяет «урок» и «тест» в одном месте. Khan Academy — «Practice» и «Unit test» рядом. Пользователь думает: «хочу учиться» → идёт в «Обучение» → выбирает формат.

#### 3. 📊 Прогресс (`/progress`)

Объединяет аналитику и прогнозирование в одну страницу с **табами**:

| Подраздел | Текущая страница | Описание |
|-----------|-----------------|----------|
| Обзор | `AnalyticsPage` | Radar навыков, heatmap активности, accuracy по уровням |
| Прогноз | `PredictionPage` | Прогнозируемый балл, CI, сравнение с целью, what-if |
| История | `HistoryPage` | Список сессий с фильтрами, детальный просмотр |

#### 4. 📅 План (`/plan`)

Остаётся отдельным разделом, т.к. это **ежедневный инструмент** — пользователь заходит каждый день посмотреть задания. Включает:
- Задания на сегодня (из `StudyPlanPage`)
- Недельный обзор
- Настройка цели (целевой балл, дата экзамена)

---

### Сравнение: Было → Стало

```
БЫЛО (9 пунктов):
┌───────────────────────────────────────────────────────────────────┐
│ UniStart  Exams Test Analytics History Plan Predict Tips Mock 📬  │
│                                              [🌙] User [Logout]  │
└───────────────────────────────────────────────────────────────────┘

СТАЛО (4 пункта + профиль):
┌───────────────────────────────────────────────────────────────────┐
│ UniStart   🏠 Главная   📚 Обучение   📊 Прогресс   📅 План      │
│                                              [🌙] [👤 Имя ▾]    │
└───────────────────────────────────────────────────────────────────┘
                                                    ┌─────────────┐
                                                    │ 👤 Профиль  │
                                                    │ 🔔 Уведом.  │
                                                    │ 🌙 Тема     │
                                                    │ 🚪 Выйти    │
                                                    └─────────────┘
```

---

### Маппинг маршрутов

| Старый маршрут | Новый маршрут | Примечание |
|---------------|---------------|------------|
| `/` (ExamSelection) | `/` | Dashboard вместо только выбора экзаменов |
| `/test` | `/learn` или `/learn/practice` | Таб «Практика» внутри Обучения |
| `/mock-exam` | `/learn/mock` | Таб «Mock Exam» внутри Обучения |
| `/topics` | `/learn/topics` | Таб «Темы» внутри Обучения |
| `/review` | `/learn/review` | Таб «Ошибки» внутри Обучения |
| `/analytics` | `/progress` | Таб «Обзор» внутри Прогресса |
| `/prediction` | `/progress/prediction` | Таб «Прогноз» внутри Прогресса |
| `/history` | `/progress/history` | Таб «История» внутри Прогресса |
| `/study-plan` | `/plan` | Без изменений |
| `/recommendations` | `/` (встроено в Dashboard) | Рекомендации на главной |
| `/notifications` | Dropdown профиля → `/profile/notifications` | Убрано из основного меню |
| `/diagnostic` | `/learn/diagnostic` | Вызывается из онбординга / Dashboard |

---

### Аналоги из EdTech

| Продукт | Основное меню | Профиль |
|---------|---------------|---------|
| **Duolingo** | Home, Leaderboard, Shop, Profile (4) | Аватар top-right |
| **Khan Academy** | Courses, Search, Assignments, Teachers (4) | Аватар top-right |
| **Coursera** | Home, My Learning, Explore (3) | Аватар top-right |
| **Quizlet** | Home, Library, Create (3) | Аватар top-right |
| **UniStart v2** | Главная, Обучение, Прогресс, План (4) | Аватар top-right ✅ |

---

### План реализации

#### Фаза 1 — Профиль (просто)
- [ ] Создать `ProfileDropdown` компонент (аватар + имя → выпадающее меню)
- [ ] Перенести переключатель темы в дропдаун
- [ ] Перенести «Уведомления» из navbar в дропдаун → `/profile/notifications`
- [ ] Добавить страницу `/profile` — имя, email, экзамены, подписка
- [ ] Добавить `Logout` в дропдаун, убрать отдельную кнопку

#### Фаза 2 — Объединение страниц
- [ ] Создать `LearnPage` (`/learn`) — хаб с табами/карточками: Practice, Mock, Topics, Review
- [ ] Создать `ProgressPage` (`/progress`) — хаб с табами: Overview, Prediction, History
- [ ] Создать `DashboardPage` (`/`) — заменить ExamSelectionPage на полноценный дашборд
- [ ] Встроить рекомендации (текущий `RecommendationsPage`) в Dashboard

#### Фаза 3 — Навигация
- [ ] Обновить `Layout.tsx` — 4 пункта в navbar + ProfileDropdown
- [ ] Обновить `App.tsx` — новые маршруты с вложенными роутами
- [ ] Мобильная адаптация — bottom tab bar для телефонов (4 иконки)
- [ ] Анимация переходов между разделами

---

*Последнее обновление: 27 февраля 2026*

---

## Что сделали

### Какие цели ставили
- Создать MVP+ адаптивной платформы для подготовки к SAT/TOEFL/NUET
- Реализовать базовую функциональность тестирования с адаптивным движком
- Добавить систему отслеживания прогресса и повторения ошибок
- Улучшить UX через тёмную тему, анимации и адаптивность
- Построить систему аутентификации с JWT

### Что сделали

#### Этап 1: Базовая функциональность ✅
- **Feedback после ответа** — показываем правильный/неправильный ответ с объяснением
- **Прогресс-бар** — визуализация текущего прогресса в тесте
- **Practice/Exam режимы** — свободная практика vs экзаменационный режим с таймером

#### Этап 2: Функции обучения ✅
- **API повторения ошибок** — spaced repetition для вопросов с ошибками
- **UI повторения ошибок** — интерфейс для работы над ошибками
- **Topics API** — получение тем с прогрессом и статистикой mastery
- **Topics UI** — выбор тем для практики с отображением прогресса
- **Навигация** — роуты и переходы между страницами

#### Этап 3: UI/UX улучшения ✅
- **Тёмная тема** — полная поддержка dark mode с CSS переменными
- **Toggle темы** — переключатель в header с сохранением в localStorage
- **Loading skeletons** — плейсхолдеры при загрузке данных
- **CSS анимации** — плавные переходы и микро-интеракции
- **Адаптивность** — корректное отображение на мобильных устройствах

#### Аутентификация и авторизация ✅
- **Регистрация** — `POST /api/auth/register` с валидацией, BCrypt-хэширование паролей
- **Вход** — `POST /api/auth/login` с JWT-токенами (HMAC-SHA256)
- **JWT middleware** — защита API-эндпоинтов через `[Authorize]`
- **Страницы Login/Register** — React-формы с Redux-состоянием (`authSlice`)
- **Тестовый пользователь** — автоматический seed `test@unistart.kz / test123`

#### Фильтрация по экзаменам ✅
- Topics страница показывает темы только для выбранных экзаменов
- Review Mistakes страница показывает ошибки только для выбранных экзаменов
- Визуальные badges показывают выбранные экзамены на страницах
- Редирект на выбор экзамена если экзамены не выбраны

#### Исправления багов ✅
- Прогресс-бар больше не показывает > 100% (Math.Min + Distinct по QuestionId)
- Улучшены метки статистики на странице тем
- Исправлен контраст текста в тёмной теме
- Исправлена сериализация массивов в axios для ASP.NET Core
- Исправлена ошибка FK_UserAnswers_Users (тестовый пользователь + fallback userId=1)
- Исправлен Vite proxy (http://localhost:5009)

### За счёт чего продвинулись

#### Архитектурные решения
- **Clean Architecture** — чёткое разделение на слои (Domain → Application → Infrastructure → API), что позволяет легко тестировать и модифицировать компоненты независимо
- **CQRS-подобный подход** — разделение команд (submit answer) и запросов (get questions), упрощающий масштабирование
- **Entity Framework Core** — code-first миграции для быстрой итерации схемы БД без ручного SQL

#### Технологический стек
- **.NET 8 + ASP.NET Core** — современный бэкенд с отличной производительностью и поддержкой
- **React 19 + TypeScript** — строгая типизация предотвращает ошибки на этапе компиляции
- **Redux Toolkit** — предсказуемое состояние приложения, удобный DevTools для отладки
- **PostgreSQL 17** — надёжная СУБД с поддержкой сложных запросов и индексов
- **Vite** — мгновенная пересборка при разработке (HMR < 100ms)

#### Адаптивный движок
- **v1 (Этап 1–3)**: Пороговый алгоритм — уровень < 40 → Easy, 40–70 → Medium, ≥ 70 → Hard; верный ответ +5, неверный −3
- **v2 (Этап 5)**: IRT 3PL модель — P(θ) = c + (1−c)/(1 + e^(−a(θ−b))), EAP Байесовская оценка, CAT с Fisher information, кривая забывания Эббингхауза

#### Качественная база данных
- **30 вопросов** с продуманной структурой:
  - SAT Reading & Writing: 6 вопросов (Main Idea, Grammar, Vocabulary)
  - SAT Math: 9 вопросов (Linear Equations, Geometry, Quadratic, Data Analysis)
  - TOEFL: 5 вопросов (Academic Reading, Lecture Comprehension)
  - NUET: 10 вопросов (Algebra, Problem Solving, Logical Reasoning, Argument Analysis)
- **Детальные объяснения** — каждый вопрос содержит развёрнутое объяснение правильного ответа
- **Три уровня сложности** — Easy/Medium/Hard для каждой темы

### Как поняли, что результат хороший

#### Техническая валидация
- ✅ **Компиляция без ошибок** — `dotnet build` и TypeScript компилятор проходят без warnings
- ✅ **Все API работают** — протестированы через Swagger UI и прямые запросы:
  - `GET /api/exams` — возвращает список экзаменов
  - `POST /api/test/next-question` — возвращает адаптивно подобранный вопрос
  - `POST /api/test/submit-answer` — сохраняет ответ и возвращает результат
  - `GET /api/test/topics` — возвращает темы с прогрессом
  - `GET /api/test/weak-questions` — возвращает вопросы для повторения
  - `GET /api/analytics` — возвращает аналитику пользователя
  - `POST /api/auth/register` и `POST /api/auth/login` — аутентификация
- ✅ **Фильтрация работает** — выбор SAT показывает только SAT-темы, NUET — только NUET-темы

#### Визуальная валидация
- ✅ **Светлая тема** — все элементы читаемы, контрастны, соответствуют дизайн-системе
- ✅ **Тёмная тема** — фоны, тексты, кнопки и badges корректно инвертированы
- ✅ **Мобильный вид** — интерфейс адаптируется к экранам от 320px
- ✅ **Анимации** — плавные fade-in, skeleton loading, hover эффекты работают без лагов

#### Функциональная валидация
- ✅ **Полный цикл пользователя**: регистрация → вход → выбор экзамена → выбор режима → ответы → статистика
- ✅ **Прогресс сохраняется** — mastery по темам обновляется корректно (без > 100%)
- ✅ **Режимы работают**: Practice (без таймера, с объяснениями) и Exam (с таймером, без подсказок)
- ✅ **Review Mistakes** — показывает только вопросы с ошибками для выбранных экзаменов

#### Метрики качества кода
- **0 ошибок ESLint** в TypeScript коде
- **0 warnings C#** в бэкенд коде
- **Типизация** — 100% TypeScript, no `any` types
- **Консистентность** — единый стиль именования, структура компонентов

---

## Дорожная карта развития

> **Миссия**: Образовательная платформа с адаптивным обучением и рекомендательными алгоритмами, способная подстраиваться под каждого пользователя, выстраивать персональный план подготовки и прогнозировать результат на реальном экзамене.

---

### Этап 4: Аналитика и визуализация прогресса ✅

**Цель**: Дать пользователю понятную картину его прогресса — где он сейчас, как продвигается, в чём сильные и слабые стороны.

#### 4.1 Графики и дашборд
- [x] Установить Recharts (npm)
- [x] **Линейный график прогресса** — динамика skill level по каждому навыку за время (ось X — даты, ось Y — уровень 0–100)
- [x] **Radar chart навыков** — визуальный профиль сильных/слабых сторон пользователя (Reading, Writing, Math, Critical Thinking и т.д.)
- [x] **Bar chart по темам** — % mastery по каждой теме внутри выбранного экзамена
- [x] **Heatmap активности** — карта дней занятий (аналог GitHub contributions)
- [x] Дашборд-страница `/analytics` с виджетами: серия дней, общая точность, кол-во вопросов, средний уровень

#### 4.2 История тестовых сессий
- [x] Новая сущность `TestSession` (Id, UserId, ExamTypeCode, Mode, StartedAt, CompletedAt, TotalQuestions, CorrectCount, Score)
- [x] Миграция БД
- [x] API `GET /api/test/sessions` — список сессий пользователя с пагинацией
- [x] API `GET /api/test/sessions/{id}` — детали сессии со всеми ответами
- [x] UI: страница истории тестов с фильтрами (экзамен, дата, режим)
- [x] UI: детальный просмотр сессии — каждый вопрос, ответ, правильность, объяснение

#### 4.3 Расширенная статистика
- [x] Точность по уровням сложности (Easy/Medium/Hard) — таблица и графики
- [x] Время на ответ (добавить поле `TimeSpentSeconds` в `UserAnswer`) — среднее и по темам
- [x] Тренд точности за последние N дней
- [ ] Самые сложные вопросы пользователя (с наибольшим кол-вом ошибок)

---

### Этап 5: Продвинутый адаптивный движок (v2) ✅

**Цель**: Перейти от простых правил (пороги 40/70, +5/−3) к модели, которая учитывает множество факторов и точнее определяет уровень студента.

#### 5.1 Item Response Theory (IRT)
- [x] Реализовать **модель IRT (3PL)** для калибровки вопросов:
  - Каждому вопросу назначен параметр сложности `b` (DifficultyParam)
  - Параметр различимости `a` (DiscriminationParam)
  - Параметр угадывания `c` (GuessParam = 0.25 для 4-option MCQ)
- [x] Добавить поля `DifficultyParam`, `DiscriminationParam`, `GuessParam` в сущность `Question`
- [x] Алгоритм оценки уровня: **Expected A Posteriori (EAP)** с Байесовским априори N(0, 1.5), 41 квадратурная точка
- [x] Оценка θ (theta) — латентный уровень способности студента по шкале [-4, +4]
- [x] Добавить поля `Theta` и `ThetaSE` в `UserSkillProfile`
- [x] Миграция `AddIRTAndTopicDependencies` — новые столбцы и таблица
- [x] Seed IRT-параметров для всех 30 вопросов с per-question вариацией
- [x] Полная IRT-библиотека `IrtMath.cs`: Probability, Information, EstimateAbilityEAP, ThetaToLevel, LevelToTheta

#### 5.2 Мультидименсиональное профилирование
- [x] Перейти от единого skill level к **вектору компетенций** по каждому топику (θ per skill)
- [x] Граф зависимостей между темами — сущность `TopicDependency` с весами
- [x] Seed 13 зависимостей: Linear Eq→Quadratic, Algebra→Problem Solving, Logical→Argument etc.
- [x] Учитывать **давность ответов** — кривая забывания Эббингхауза: `R(t) = e^(-t/S)`, S = stability
- [x] Confidence interval для оценки уровня — показываем "уровень 65 ± 8" (95% CI: θ ± 1.96·SE)

#### 5.3 Адаптивный выбор вопросов
- [x] **Computerized Adaptive Testing (CAT)** — выбирать вопрос с максимальной Fisher information
- [x] Ограничения выбора: не повторять вопросы, рандомизация из top-70% для exposure control
- [x] Frontend: отображение confidence interval на шкалах навыков (Analytics) и в feedback (TestPage)
- [ ] A/B тестирование: сравнить CAT vs текущий алгоритм по метрикам обучения

---

### Этап 6: Персональный план подготовки 📅 ✅

**Цель**: Автоматически генерировать и адаптировать индивидуальный план подготовки к конкретной дате экзамена.

#### 6.1 Настройка профиля цели
- [x] UI: пользователь задаёт **целевой экзамен**, **дату экзамена**, **целевой балл**
- [x] Новая сущность `StudyGoal` (UserId, ExamTypeCode, TargetDate, TargetScore, CreatedAt)
- [x] Расчёт доступного времени: `daysUntilExam`, `recommendedHoursPerDay`

#### 6.2 Генерация плана
- [x] Алгоритм распределения тем по дням на основе:
  - **Приоритет слабых зон** — наибольший вес темам с низким mastery
  - **Зависимости между темами** — сначала базовые, потом продвинутые
  - **Spaced repetition** — повторение через интервалы (1, 3, 7, 14 дней)
  - **Баланс нагрузки** — равномерное распределение по дням с учётом сложности
- [x] Новая сущность `StudyPlan` (Id, UserId, GoalId, GeneratedAt, Entries[])
- [x] `StudyPlanEntry` (PlanId, Date, TopicId, RecommendedMinutes, Type: New/Review/Practice)
- [x] API `POST /api/study-plan/generate/{goalId}` — генерация плана
- [x] API `GET /api/study-plan/today` — задания на сегодня
- [x] API `POST /api/study-plan/entries/{entryId}/complete` — отметка выполнения

#### 6.3 Динамическая адаптация плана
- [x] **Перерасчёт плана** после каждой сессии:
  - Тема освоена хорошо? → уменьшить будущие сессии для неё
  - Тема идёт плохо? → добавить дополнительные повторения
  - Пользователь пропустил день? → перераспределить нагрузку
- [x] Рекомендации: "Сегодня рекомендуем: Algebra (20 мин) + Grammar Review (15 мин)"
- [x] Недельный обзор: выполнено/запланировано, отклонение от плана, статистика

---

### Этап 7: Прогнозирование результата на экзамене 📊 ✅

**Цель**: На основе данных о подготовке предсказать, какой балл студент получит на реальном экзамене, и показать, что нужно улучшить.

#### 7.1 Модель прогнозирования
- [x] **Маппинг θ → балл экзамена**: преобразование латентного уровня IRT в шкалу реального экзамена:
  - SAT: 400–1600 (Math 200–800, EBRW 200–800)
  - TOEFL: 0–120 (Reading 0–30, Listening 0–30, Speaking 0–30, Writing 0–30)
  - NUET: собственная шкала
- [x] Учитывать распределение вопросов по секциям и их вес в финальном балле
- [x] **Confidence interval**: прогноз с 90% CI (θ ± 1.645·SE)
- [x] Регулярный пересчёт прогноза при каждом запросе

#### 7.2 UI прогноза
- [x] Виджет "Прогнозируемый балл" — большой, заметный с CI полосой
- [x] График динамики прогноза по датам (AreaChart с зоной уверенности)
- [x] Разбивка по секциям: BarChart + RadarChart + карточки с деталями
- [x] Сравнение с целевым баллом: маркер цели на шкале, отображение разрыва

#### 7.3 Рекомендации по улучшению
- [x] Автоматический анализ: топ-5 тем с наибольшим потенциалом роста (ROI)
- [x] Приоритизация по ROI: какие темы дадут наибольший прирост балла
- [x] Сценарий "Что если": выбрать тему и уровень → рассчитать прогнозируемый рост балла

---

### Этап 8: Рекомендательная система 🎯 ✅

**Цель**: Интеллектуально подсказывать студенту, что делать дальше — какую тему учить, какой тип задач практиковать, когда повторять.

#### 8.1 Контекстные рекомендации
- [x] **After-session рекомендации**: анализ ошибок по темам, похвала за сильные темы, совет по accuracy, автоматическая проверка milestones
- [x] **Ежедневные рекомендации**: кривая забывания Эббингхауза (`IrtMath.RetentionProbability`) — рекомендация повторить тему при retention < 70%
- [x] **Рекомендация режима**: "У вас скоро экзамен — попробуйте Exam Mode" + streak мотивация
- [x] **Слабые навыки**: автоматическое обнаружение навыков с уровнем < 40% → рекомендация практики
- [x] Приоритизация рекомендаций: high → medium → low, максимум 8 на день
- [x] API: `GET /api/recommendations/daily`, `GET /api/recommendations/after-session/{id}`
- [x] `RecommendationService.cs` — 350+ строк бизнес-логики
- [x] `RecommendationController.cs` — 5 эндпоинтов

#### 8.2 Collaborative Filtering (отложено — при масштабе)
- [ ] Анализ паттернов успешных студентов: "Студенты с похожим профилем, набравшие 1400+, больше всего практиковали Data Analysis"
- [ ] Рекомендация порядка изучения тем на основе успешных траекторий других пользователей
- [ ] Требование: минимум 100+ активных пользователей для статистической значимости

#### 8.3 Мотивация и вовлечение
- [x] **Streak tracking** — подсчёт серии дней из `UserAnswer.AnsweredAt`, текущий/рекордный/всего дней
- [x] **Milestones** — 14 типов: Q10/Q50/Q100/Q250/Q500/Q1000, STREAK_3/7/14/30, MASTERY_{skill}, ACC_90, SESSIONS_10
- [x] Новая сущность `UserMilestone` (Id, UserId, Code, Title, Description, Icon, AchievedAt) + миграция
- [x] API: `GET /api/recommendations/streak`, `GET /api/recommendations/milestones`, `POST /api/recommendations/check-milestones`
- [x] UI: страница `/recommendations` — streak-карточка (градиент 3+/7+ дней), вчерашняя сводка, рекомендации с приоритетами, сетка достижений
- [x] **Weekly digest** — email/push: прогресс за неделю, прогноз, рекомендации на следующую (требует email-сервис)

---

### Этап 9: Расширение контента ✅

**Цель**: Масштабировать базу вопросов и контента для полноценной подготовки.

#### 9.1 Расширение базы вопросов
- [x] **97 вопросов** (30 seed + 67 expanded) по всем 16 темам трёх экзаменов (SAT:40, TOEFL:28, NUET:29)
- [x] `QuestionExpansionSeeder` — автоматическое расширение базы при запуске (проверка ≤35 вопросов)
- [x] Админ-панель: CRUD вопросов, фильтрация по экзамену/теме/сложности, детальный просмотр
- [x] Bulk import вопросов (JSON) с валидацией каждого вопроса и отчётом об ошибках
- [x] Валидация: проверка topicId, difficulty, обязательно ≥2 варианта ответа, ровно 1 правильный
- [x] Авто-расчёт IRT параметров (b, a, c) через `IrtMath.DifficultyToParam/Discrimination`
- [x] Статистика: общее количество, по экзаменам, по сложности, по темам, покрытие тем
- [x] API: `GET /api/admin/questions` (filters), `GET /api/admin/questions/{id}`, `POST`, `PUT`, `DELETE`, `POST /import`, `GET /stats`
- [x] UI: страница `/admin` — 3 вкладки (Статистика, Вопросы, Импорт), модальный просмотр деталей

#### 9.2 Учебные материалы ✅
- [x] **Мини-уроки** по каждой теме — 19 уроков (1–2 на тему) с markdown-контентом: теория, формулы, стратегии, таблицы
- [x] **Подсказки** (hints) — пошаговые наводящие подсказки для всех 97 вопросов (по сложности Easy/Medium/Hard)
- [x] **Видео-ссылки** — YouTube видео на 10 из 16 тем (SAT Math, SAT R&W, TOEFL Reading, NUET)
- [x] Новые сущности: `TopicLesson` (Id, TopicId, Title, Content, VideoUrl, SortOrder), `Question.Hint`, `Question.VideoUrl`
- [x] Миграция `AddLearningMaterials` — TopicLessons таблица + Hint/VideoUrl столбцы
- [x] Backend: `LessonService` + `ILessonService`, `LessonController` (4 эндпоинта)
- [x] API: `GET /api/lessons` (список тем с уроками), `GET /api/lessons/topic/{id}`, `GET /api/lessons/{id}`, `GET /api/lessons/hint/{questionId}`
- [x] Frontend: кнопка "📖 Урок" на TopicsPage, полноценный reader с markdown-рендером, навигация по урокам
- [x] Frontend: кнопка "💡 Подсказка" на TestPage и TopicsPage (practice mode), lazy-load по клику
- [x] Frontend: карточка "🎬 Видео-урок" со ссылкой на YouTube
- [x] `TopicProgressDto` расширен: `LessonCount`, `HasVideoLessons`
- [x] `QuestionDto` расширен: `HasHint`
- [x] Seed: `SeedTopicLessonsAsync()` + `SeedQuestionHintsAsync()` в DatabaseSeeder

#### 9.3 Реальные форматы экзаменов ✅
- [x] **Mock Exam система** — полноценные тренировочные экзамены с секциями и таймером
- [x] **SAT Practice Test** — 3 секции (Reading & Writing 15м + Math No Calc 15м + Math Calc 15м), все 40 вопросов
- [x] **TOEFL iBT Practice Test** — 4 секции (Reading 18м + Listening 12м + Speaking 10м + Writing 10м), 28 вопросов
- [x] **NUET Practice Test** — 2 секции (Quantitative Reasoning 25м + Critical Thinking 25м), 29 вопросов
- [x] **TOEFL Reading Passages** — 3 академических текста (Cambrian Explosion, Urban Heat Islands, Psychology of Decision-Making) с привязкой вопросов к пассажам
- [x] Новые сущности: `ReadingPassage`, `MockExam`, `MockExamSection`, `MockExamAttempt`, `MockExamAnswer`
- [x] `Question.ReadingPassageId` — опциональная привязка вопроса к пассажу
- [x] Миграция `AddMockExams` — 5 новых таблиц + FK ReadingPassageId
- [x] Backend: `MockExamService` + `IMockExamService` (9 методов), `MockExamController` (9 эндпоинтов)
- [x] API: `GET /api/mock-exams`, `GET /{id}`, `POST /{id}/start`, `GET /attempts/{id}/current-section`, `GET /attempts/{id}/sections/{idx}`, `POST /attempts/{id}/answer`, `POST /attempts/{id}/complete-section`, `GET /attempts/{id}/results`, `GET /history`, `POST /attempts/{id}/abandon`
- [x] Frontend: `MockExamPage.tsx` — 6 фаз (list, detail, instructions, section, section-break, results)
- [x] UI: навигатор вопросов, split-view пассаж+вопрос для TOEFL, countdown таймер с цветовыми индикаторами
- [x] UI: результаты — общий балл, разбивка по секциям, progress bars, review с фильтрами (all/incorrect/unanswered)
- [x] UI: история попыток, best score, attempt count
- [x] Seed: `SeedReadingPassagesAsync()` + `SeedMockExamsAsync()` в DatabaseSeeder
- [x] Навигация: вкладка "Mock" в Layout, маршрут `/mock-exam` в App.tsx

#### Онбординг флоу ✅
- [x] **Многошаговый мастер настройки** — 4 этапа: Welcome → Выбор экзамена → Настройка цели → Готово!
- [x] `User.HasCompletedOnboarding` — флаг завершения онбординга в сущности User
- [x] Миграция `AddOnboarding` — новый столбец в таблице Users
- [x] `AuthResponseDto` — теперь включает `hasCompletedOnboarding`
- [x] Backend: `OnboardingService` + `IOnboardingService` (3 метода: GetStatus, GetExamTypes, CompleteOnboarding)
- [x] `OnboardingController` — 3 эндпоинта: `GET /api/onboarding/status`, `GET /api/onboarding/exam-types`, `POST /api/onboarding/complete`
- [x] Авто-создание StudyGoal + StudyPlan при завершении онбординга
- [x] `ExamTypeInfoDto` — расширенная информация об экзамене (описание, секции, шкала баллов)
- [x] Frontend: `OnboardingPage.tsx` — визард с градиентным фоном, карточки экзаменов, слайдер баллов, preset кнопки
- [x] Frontend: `onboardingService.ts` — клиент для API онбординга
- [x] Redux: `setOnboardingComplete` action в authSlice
- [x] Роутинг: `/onboarding` маршрут, автоматический redirect после регистрации/логина если не прошёл онбординг
- [x] Типы: `OnboardingStatus`, `ExamTypeInfo`, `ExamSectionInfo`, `CompleteOnboardingRequest`

---

### Бизнес-фичи и монетизация 💰

| Фича | Сложность | Статус |
|------|-----------|--------|
| Онбординг-флоу | Средняя | ✅ Готово |
| Диагностический тест | Лёгкая | ✅ Готово |
| Subscription/план | Средняя | ✅ Готово |
| Daily question limit | Лёгкая | ✅ Готово |
| Paywall UI | Средняя | ✅ Готово |
| Лендинг | Средняя | ✅ Готово |
| Email-уведомления | Сложная | ✅ Готово |

#### Диагностический мини-тест 🩺
- [x] Спец-режим `diagnostic`: 10 вопросов (по 2–3 из каждой секции выбранного экзамена)
- [x] Автоматический подбор вопросов разной сложности (Easy/Medium/Hard) для калибровки θ
- [x] После завершения — redirect на страницу результатов с начальным прогнозом балла
- [x] Запуск диагностики из онбординга (шаг "Ready") или отдельной кнопкой на главной
- [x] Обновление skill profiles на основе результатов диагностики

#### Subscription / тарифный план 💳
- [x] Поле `SubscriptionTier` в User (`Free` | `Pro`), поле `SubscriptionExpiresAt`
- [x] Миграция для новых полей
- [x] Middleware/сервис проверки тарифа: `ISubscriptionService.HasAccess(userId, feature)`
- [x] Конфигурация лимитов по тарифам (вопросы/день, mock exams, уроки, аналитика)
- [x] API: `GET /api/subscription/status`, `POST /api/subscription/upgrade` (заглушка для будущей оплаты)
- [x] FREE тариф: 15 вопросов/день, 1 урок/день, базовая аналитика, недельный прогноз
- [x] PRO тариф: безлимитные вопросы, все уроки, mock exams, полный план, real-time прогноз

#### Daily question limit 🔒
- [x] Считать `UserAnswers` за сегодня (`AnsweredAt >= today UTC`)
- [x] Блокировать после 15 вопросов для Free-пользователей
- [x] UI: счётчик "осталось X вопросов сегодня" на TestPage
- [x] UI: модальное окно при достижении лимита с CTA на апгрейд
- [x] API: `GET /api/subscription/daily-usage` — текущее использование и лимит

#### Paywall UI 🚧
- [x] Компонент `<ProGate>` — обёртка для Pro-контента (блюр + overlay + CTA)
- [x] Применить к: Mock Exams, расширенная аналитика (radar, heatmap), полный план подготовки
- [x] Тизеры: показать частичные данные + "Разблокируйте PRO для полного доступа"
- [x] Модальное окно тарифов с описанием Free vs Pro
- [x] Компонент `<UpgradeBanner>` — ненавязчивый баннер в sidebar/footer

#### Лендинг-страница 🌐
- [x] Отдельная страница `/landing` (без авторизации)
- [x] Hero-секция: заголовок, подзаголовок, CTA "Начать бесплатно"
- [x] Секция возможностей: адаптивные тесты, план, прогноз, mock exams
- [x] Секция тарифов: Free vs Pro с таблицей сравнения
- [x] Social proof: отзывы/статистика (заглушки)
- [x] Footer: контакты, FAQ
- [x] Redirect на `/register` по CTA

#### Email-уведомления 📧
- [x] Интеграция MailKit для отправки email (SMTP)
- [x] Streak-reminder: "Вы не занимались 2 дня — не потеряйте серию!"
- [x] Weekly digest: прогресс за неделю, прогноз, рекомендации
- [x] Welcome email после регистрации
- [x] Настройки уведомлений: вкл/выкл для каждого типа
- [x] Шаблоны писем (HTML) с branding UniStart

---

### Этап 10: Инфраструктура и масштабирование ⚙️

#### 10.1 Тестирование
- [ ] Unit-тесты для AdaptiveEngineService (xUnit + Moq)
- [ ] Integration-тесты для API-эндпоинтов (WebApplicationFactory)
- [ ] Frontend-тесты: Vitest + React Testing Library
- [ ] Покрытие ≥ 80% для бизнес-логики

#### 10.2 DevOps и деплой
- [ ] Dockerfile для бэкенда
- [ ] docker-compose (API + PostgreSQL + frontend build)
- [ ] GitHub Actions CI/CD: build → test → deploy
- [ ] Выбор хостинга: Azure App Service / Railway / DigitalOcean

#### 10.3 Безопасность и качество
- [ ] Rate limiting на API
- [ ] Input validation и sanitization
- [ ] Логирование (Serilog → файл/Dashboard)
- [ ] Health checks endpoint
- [ ] Мониторинг и алерты

#### 10.4 Мобильное приложение (отложено)
- [ ] React Native или PWA
- [ ] Офлайн-режим с синхронизацией
- [ ] Push-уведомления о рекомендованных занятиях

---

## Приоритеты реализации

| Приоритет | Этап | Обоснование |
|-----------|------|-------------|
| 🔴 Высокий | Этап 4 — Аналитика | Без визуализации прогресса пользователь не видит ценности. Базовый retention. |
| 🔴 Высокий | Этап 5 — Адаптивный движок v2 | Ядро продукта. IRT даёт научно обоснованную оценку уровня. |
| 🔴 Высокий | Этап 6 — Персональный план | Ключевой дифференциатор от конкурентов. Структурирует обучение. |
| ✅ Готово | Этап 7 — Прогнозирование | θ→score маппинг, CI, секции, ROI tips, what-if, история |
| ✅ Готово | Этап 8 — Рекомендации | Daily briefing, after-session, streak, 14 milestones, forgetting curve, UI |
| ✅ Готово | Этап 9 — Контент | 97 вопросов (16/16 тем), админ-панель, 19 уроков, hints, 3 mock exams, reading passages, timed sections |
| 🟢 Низкий | Этап 10 — Инфраструктура | Нужно перед продакшеном, но не блокирует разработку фич. |

---

## Валидация продукта 📋

### Проблемное интервью
Планируем провести проблемные интервью с целевой аудиторией для проверки:

**Целевая аудитория:**
- Абитуриенты, готовящиеся к SAT/TOEFL/NUET
- Родители абитуриентов
- Репетиторы и преподаватели

**Вопросы для интервью:**
1. Как вы сейчас готовитесь к экзамену?
2. Какие инструменты/ресурсы используете?
3. Что больше всего раздражает в текущих решениях?
4. Как отслеживаете свой прогресс?
5. Сколько времени тратите на подготовку?
6. Готовы ли платить за инструмент подготовки? Сколько?

**Гипотезы для проверки:**
- [ ] Студенты испытывают сложности с отслеживанием прогресса
- [ ] Существующие решения не адаптируются под уровень студента
- [ ] Режим повторения ошибок востребован
- [ ] Разделение по темам помогает в подготовке
- [x] Персональный план подготовки — ключевой запрос
- [ ] Студенты хотят знать свой прогнозируемый балл до экзамена

---

## Технический стек

| Компонент | Технология |
|-----------|------------|
| Backend | .NET 8 + ASP.NET Core |
| Frontend | React 19 + TypeScript + Vite |
| Database | PostgreSQL 17 |
| ORM | Entity Framework Core 8 |
| Auth | JWT (HMAC-SHA256) + BCrypt |
| State | Redux Toolkit |
| Styling | CSS Custom Properties (dark/light) |
| API Docs | Swagger / Swashbuckle |

---

## Структура проекта

```
UniStart/
├── Domain/
│   ├── Entities/        # User, ExamType, Question, Skill, Topic, UserAnswer, UserSkillProfile, UserMilestone, ReadingPassage, MockExam, MockExamSection, MockExamAttempt, MockExamAnswer
│   └── Interfaces/      # IRepository<T>, IUnitOfWork
├── Application/
│   ├── DTOs/            # AuthDtos, ExamDtos, QuestionDtos, SkillDtos, TestSessionDtos, PredictionDtos, RecommendationDtos, AdminDtos, LessonDtos, MockExamDtos
│   ├── Interfaces/      # IAdaptiveEngineService, IAnalyticsService, IAuthService, IExamService, IJwtService, IStudyPlanService, IScorePredictionService, IRecommendationService, IAdminService, ILessonService, IMockExamService
│   └── Services/        # AdaptiveEngineService, AnalyticsService, AuthService, ExamService, JwtService, StudyPlanService, ScorePredictionService, RecommendationService, AdminService, LessonService, MockExamService
├── Infrastructure/
│   ├── Data/            # UniStartDbContext, DatabaseSeeder, QuestionExpansionSeeder
│   └── Repositories/    # Repository<T>, UnitOfWork
├── Controllers/         # AuthController, ExamsController, TestController, AnalyticsController, UsersController, StudyPlanController, PredictionController, RecommendationController, AdminController, LessonController, MockExamController, OnboardingController
├── client/              # React 19 frontend
│   └── src/
│       ├── pages/       # Login, Register, Onboarding, ExamSelection, Test, Analytics, Review, Topics, StudyPlan, Prediction, Recommendations, Admin, MockExam
│       ├── components/  # Layout, Skeleton
│       ├── services/    # api, authService, onboardingService, examService, testService, analyticsService, studyPlanService, predictionService, recommendationService, adminService, lessonService, mockExamService
│       ├── store/       # Redux store + slices (auth, exam, test)
│       ├── hooks/       # useAppDispatch, useAppSelector, useTheme
│       └── types/       # TypeScript interfaces
└── docs/                # ARCHITECTURE.md, API.md, DB_SCHEMA.md, RULES.md, etc.
```

---

*Последнее обновление: 25 февраля 2026*
