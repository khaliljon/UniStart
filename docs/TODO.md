# UniStart — Sprint 3: CSCA Question Seeding Plan

## Обзор
Создание вопросов с 4 вариантами ответа для CSCA Математика (20 глав) на основе учебника.
Источники: `QuestionsCscaMath.md` (8774 строк), `AnswersCscaMath.md` (2326 строк).

---

## Фаза 1 — Обновление структуры CSCA (5 секций) ✅

- [x] Добавить 2 новые ExamSection: **Chinese Technical (中文技术)**, **Chinese Humanitarian (中文人文)**
- [x] Добавить навыки `SK_CN_TECH`, `SK_CN_HUM` в таблицу Skills
- [x] Обновить название ExamType CSCA → "CSCA (Mathematics + Physics + Chemistry + Chinese Technical + Chinese Humanitarian)"

---

## Фаза 2 — Сидинг вопросов по математике ✅

### Подход
- Вопросы уже имеющие формат A/B/C/D → прямое использование
- Fill-in-blank / расчётные → конвертация в 4-вариантный формат с правдоподобными дистракторами
- True/False → формат "Какое утверждение верно/неверно?"
- Сложность: Easy (базовые), Medium (стандартные), Hard (olympiad-уровень, proof, multi-step)

### Файлы
- [x] `Infrastructure/Data/CscaMathQuestionSeeder.cs` — создан, ~182 вопроса по 20 главам
- [x] Wired в `DatabaseSeeder.SeedAsync()` после `SeedQuestionsAsync()`

### Главы 1-5 (~60 вопросов) ✅
| Глава | Тема | ~Кол-во | Сложность |
|-------|------|---------|-----------|
| 1 | Множества (определения, операции) | 11 | Easy-Medium |
| 2 | Неравенства (линейные, квадратичные, дробные) | 12 | Easy-Hard |
| 3 | Функции (область, чётность, виды) | 15 | Easy-Hard |
| 4 | Тригонометрия (единичная окружность, графики) | 14 | Medium-Hard |
| 5 | Обратные тригонометрические функции | 6 | Easy-Medium |

### Главы 6-10 (~39 вопросов) ✅
| Глава | Тема | ~Кол-во | Сложность |
|-------|------|---------|-----------|
| 6 | Формулы сложения/удвоения | 8 | Easy-Medium |
| 7 | Последовательности (арифм., геом.) | 7 | Easy-Hard |
| 8 | Комплексные числа | 7 | Easy-Medium |
| 9 | Прямые на плоскости | 7 | Easy-Medium |
| 10 | Конические сечения (окружность, эллипс, гипербола, парабола) | 9 | Easy-Hard |

### Главы 11-15 (~40 вопросов) ✅
| Глава | Тема | ~Кол-во | Сложность |
|-------|------|---------|-----------|
| 11 | Векторы на плоскости | 9 | Easy-Medium |
| 12 | Пространственные векторы | 6 | Easy-Medium |
| 13 | Пространственные плоскости и прямые | 6 | Easy-Medium |
| 14 | Пределы последовательностей и функций | 9 | Easy-Hard |
| 15 | Производные (определение, правила) | 10 | Easy-Medium |

### Главы 16-20 (~36 вопросов) ✅
| Глава | Тема | ~Кол-во | Сложность |
|-------|------|---------|-----------|
| 16 | Применение производных | 7 | Medium |
| 17 | Перестановки и сочетания | 9 | Easy-Medium |
| 18 | Вероятность (классическая, условная) | 10 | Easy-Hard |
| 19 | Случайные величины, E(X), D(X) | 10 | Easy-Medium |
| 20 | Статистика (выборка, среднее, дисперсия) | 10 | Easy-Medium |

**Итого: ~182 вопроса** (покрыты все 20 глав; можно расширить до ~550 в будущем)

---

## Фаза 3 — Верификация ✅

- [x] `dotnet build` — 0 ошибок, 0 предупреждений
- [x] `cd client && npx tsc --noEmit` — 0 ошибок
- [ ] Запуск приложения, проверка что вопросы загружаются
- [ ] Проверка корректности ответов по AnswersCscaMath.md

---

## Будущие задачи (Спринт 4+)

- [ ] Сидинг вопросов по Физике (12 глав)
- [ ] Сидинг вопросов по Химии
- [ ] Контент для Chinese Technical (中文技术)
- [ ] Контент для Chinese Humanitarian (中文人文)
- [ ] Улучшение взаимодействия Тьютор ↔ Студент
- [ ] Новые фичи (аналитика, адаптивность)

---

## Sprint 5 — Монетизация & UX

### FIX-29 ✅ Фильтрация секций предикшена
### FIX-30 ✅ 3-дневный trial период
### FIX-31 ✅ Аналитика: free/pro split (ProGate)
### FIX-32 ✅ Работа над ошибками — лимит по тарифу
### FIX-33 ✅ Предикшен — blur для Free
### FIX-34 ✅ Профиль: убраны уроки, кнопка апгрейда → PricingModal
### FIX-35 ✅ PricingModal: цена 10 000 ₸, годовая подписка 99 990 ₸
### FIX-36 ✅ i18n: обновлены ключи (3 локали) + LandingPage

---

## FIX-37 — Бесплатный пробный мок-экзамен + интерактивный upsell

### Концепция
После регистрации пользователь получает **1 бесплатный полноценный мок-экзамен** (реальный сценарий: все секции, ограничение по времени, настоящие вопросы). После завершения показывается **интерактивный upsell-модал** с превью Pro-функций.

### Backend

#### 1. User entity — новое поле
```csharp
// Domain/Entities/User.cs
public bool FreeMockUsed { get; set; } = false;
```

#### 2. EF Migration
```
dotnet ef migrations add AddFreeMockUsed
```

#### 3. SubscriptionService — логика доступа
В `HasAccessAsync` изменить кейс `mock_exams`:
```csharp
"mock_exams" => limits.MockExamsEnabled || !user.FreeMockUsed,
```
Результат: Free-пользователь с `FreeMockUsed == false` → доступ разрешён.

#### 4. SubscriptionStatusDto — новое поле
```csharp
public record SubscriptionStatusDto(
    ...
    bool FreeMockAvailable  // !user.FreeMockUsed && !isPro
);
```

#### 5. MockExamController / MockExamService — пометка использования
При **завершении** мок-экзамена (endpoint `POST /api/mock-exams/attempts/{id}/complete`):
```csharp
if (!user.IsPro && !user.FreeMockUsed)
{
    user.FreeMockUsed = true;
    await _unitOfWork.SaveChangesAsync();
}
```

### Frontend

#### 6. MockExamPage.tsx — разрешить 1 бесплатный мок
Заменить `ProGate hasAccess={isPro}` на:
```tsx
const canAccessMock = isPro || subscriptionStatus?.freeMockAvailable;
<ProGate hasAccess={canAccessMock} featureName="Mock Exams">
```

#### 7. MockResultUpsellModal — интерактивный upsell после бесплатного мока
Новый компонент `client/src/components/MockResultUpsellModal.tsx`.
Показывается **только после завершения бесплатного мока** (не для Pro).

**Экраны (шаги):**
1. **Результаты** — «Твой балл: X/Y. Хочешь узнать, где ты потерял баллы?»
   - Кнопка: «Показать аналитику» → переход к шагу 2
2. **Превью аналитики** — Размытый radar-чарт + heatmap с текстом:
   «С Pro ты увидишь полную аналитику: сильные/слабые стороны, прогресс по темам»
   - Кнопка: «А что ещё есть?» → шаг 3
3. **Работа над ошибками** — Превью списка ошибок (1-2 видны, остальные blur):
   «Разбирай каждую ошибку с объяснением. Доступно в Pro.»
   - Кнопка: «Хочу попробовать Pro» → шаг 4
4. **CTA** — PricingModal (месяц/год) со скидкой:
   «Специальное предложение: первый месяц -50%» (5 000 ₸ вместо 10 000 ₸)

#### 8. i18n ключи
Добавить в `mockUpsell` секцию (ru/kz/en):
```
mockUpsell: {
  title: 'Мок завершён!',
  scoreText: 'Твой результат',
  wantAnalytics: 'Хочешь узнать, где потерял баллы?',
  showAnalytics: 'Показать аналитику',  
  analyticsPreview: 'С Pro — полная аналитика',
  whatElse: 'А что ещё есть?',
  mistakesPreview: 'Разбирай каждую ошибку',
  tryPro: 'Хочу попробовать Pro',
  specialOffer: 'Первый месяц -50%',
}
```

### Порядок реализации
1. [ ] Backend: добавить `FreeMockUsed` в User entity
2. [ ] Backend: миграция `AddFreeMockUsed`
3. [ ] Backend: `HasAccessAsync` — разрешить mock_exams при !FreeMockUsed
4. [ ] Backend: `SubscriptionStatusDto` + `FreeMockAvailable`
5. [ ] Backend: пометить FreeMockUsed при завершении мока
6. [ ] Frontend: MockExamPage — `canAccessMock` вместо `isPro`
7. [ ] Frontend: `MockResultUpsellModal` — 4-шаговый интерактив
8. [ ] Frontend: i18n ключи (ru/kz/en)
9. [ ] Тестирование: регистрация → мок → upsell → повторный вход → gate

---

## Pre-Launch Checklist

### FIX-43 ✅ Фаза 1 — Юридическая база
- [x] PrivacyPage.tsx — 10 секций, ЗРК «О персональных данных» №94-V
- [x] TermsPage.tsx — 12 секций (подписки, оплата KZT, возвраты 14 дней, ИС, споры)
- [x] CookieBanner.tsx — фиксированный баннер, Accept/Decline, localStorage
- [x] i18n: 95+ ключей legal секции (ru/en/kz)
- [x] Публичные маршруты /privacy, /terms (без авторизации)
- [x] Чекбокс согласия на RegisterPage (блокирует отправку)
- [x] Ссылки в футере LandingPage

### FIX-44 ✅ Фаза 2 — Безопасность
- [x] Аудит секретов: appsettings.json (плейсхолдеры), Production (env vars), .gitignore ок
- [x] CSP заголовок: default-src 'self', script/font/style-src, frame-ancestors 'none'
- [x] HSTS: max-age=31536000; includeSubDomains; preload
- [x] CORS: ограничены методы (GET/POST/PUT/DELETE/PATCH)
- [x] Rate limiting: auth 10/мин, api 120/мин, global 200/мин per IP
- [x] InputSanitizer: HTML-тег стриппинг для user-facing полей
  - AuthService (Name при регистрации, обновлении)
  - TutorService (Headline, Bio, Experience)
  - MessageService (Text, RequestMessage, LastMessagePreview)
  - AdminService (user Name, topic Name)
  - НЕ применяется к контенту вопросов (KaTeX/markdown)
- [x] Аудит зависимостей: dotnet — 0 уязвимостей, npm — 8 dev-only (eslint/vite)
- [x] HTTPS redirect уже в пайплайне
- [x] Env vars: UNISTART_JWT_SECRET, UNISTART_DB_CONNECTION

### Фаза 3 — Инфраструктура (TODO)
- [ ] Docker: Dockerfile + docker-compose.yml (PostgreSQL 17 + .NET 8 + Nginx)
- [ ] CI/CD: GitHub Actions (build → test → deploy)
- [ ] Мониторинг: health checks, Serilog Seq/файлы
- [ ] Бэкапы PostgreSQL (pg_dump cron)

### Фаза 4 — Контент & QA (TODO)
- [ ] Финальная проверка 182 вопросов CSCA Math
- [ ] Сидинг вопросов по физике (12 глав)
- [ ] Тестирование всех user flows (регистрация → экзамен → аналитика → подписка)

### Фаза 5 — Бесплатный мок-экзамен (TODO)
- [ ] См. FIX-37 выше

### Фаза 6 — Финальная проверка (TODO)
- [ ] Lighthouse: Performance ≥90, Accessibility ≥90
- [ ] Cross-browser: Chrome, Firefox, Safari, Edge
- [ ] Mobile responsive проверка
- [ ] SEO: meta tags, sitemap.xml, robots.txt

---

## Sprint 7 — Тьюторская экосистема и монетизация

> **Добавлено**: 18 марта 2026

### 1. Модель подписок — Тьюторы, Ученики, Школы

| Тариф | Цена | Период | Кто платит | Что включено |
|-------|------|--------|------------|--------------|
| **Ученик Free** | 0 ₸ | — | — | Диагностический тест, ограниченная аналитика, 1 бесплатный мок |
| **Ученик Pro** | 10 000 ₸ | месяц | Ученик | Полный доступ ко всем экзаменам, аналитика, прогнозы |
| **Ученик Pro (годовая)** | 99 990 ₸ | год | Ученик | То же, скидка ~17% |
| **Ученик при тьюторе** | 7 000 ₸ | месяц | Ученик | Полный Pro-доступ, скидка 30% за привязку к тьютору |
| **Тьютор** | 20 000 ₸ | год | Тьютор | Панель тьютора, управление учениками, создание контента |
| **Школа** | 50 000 ₸ | год | Школа | Профиль школы, все тьюторы школы, брендинг, аналитика по школе |

### 2. Привязка Тьютор ↔ Ученик

#### Механизм привязки
1. **Тьютор** генерирует уникальный инвайт-код (или ссылку) в своей панели
2. **Ученик** вводит инвайт-код при регистрации или в настройках профиля
3. Система создаёт связь `TutorStudent` (M:N через промежуточную таблицу)
4. Ученик получает скидку на подписку: 7 000 ₸/мес вместо 10 000 ₸/мес

#### Новые сущности (Domain)
```
TutorStudent {
  Id, TutorUserId, StudentUserId, 
  InviteCode, Status (Pending/Active/Revoked),
  LinkedAt, RevokedAt
}

TutorSubscription {
  Id, TutorUserId, Plan (Tutor/School),
  StartDate, EndDate, Status (Active/Expired/Cancelled),
  MaxStudents (Tutor: 50, School: unlimited)
}
```

#### Бизнес-правила
- Тьютор может привязать до **50 учеников** (базовый план)
- Ученик может быть привязан только к **1 тьютору** одновременно
- При отвязке от тьютора ученик теряет скидку (следующий платёж по полной цене)
- Тьютор видит прогресс, аналитику и результаты всех своих учеников
- Тьютор может создавать домашние задания и назначать их ученикам

### 3. Школьный профиль

#### Как выглядит профиль школы
- **Публичная страница** `/schools/:slug` (аналог профиля тьютора):
  - Название школы, логотип, описание, контакты
  - Список тьюторов школы (фото, специализация, рейтинг)
  - Список доступных экзаменов (CSCA, IELTS, SAT, etc.)
  - Отзывы учеников
  - Кнопка «Записаться» → выбор тьютора → инвайт

#### Как действуют тьюторы внутри школы
1. **Администратор школы** (owner) — создаёт профиль школы, добавляет/удаляет тьюторов
2. **Тьютор школы** — обычный тьютор, но привязан к школе:
   - Его профиль отображается на странице школы
   - Он может управлять только своими учениками
   - Школьный бренд указан в его профиле
3. **Ученик** записывается через страницу школы → привязывается к конкретному тьютору внутри школы

#### Новые сущности (Domain)
```
School {
  Id, Name, Slug, Description, LogoUrl,
  ContactEmail, ContactPhone, Website,
  InstagramUrl, TelegramUrl,
  OwnerUserId, CreatedAt, IsActive
}

SchoolTutor {
  Id, SchoolId, TutorUserId, 
  Role (Owner/Tutor), JoinedAt, IsActive
}
```

#### Роли и доступ
| Действие | Владелец школы | Тьютор школы | Обычный тьютор |
|----------|---------------|--------------|----------------|
| Создать/редактировать профиль школы | ✅ | ❌ | — |
| Добавить тьютора в школу | ✅ | ❌ | — |
| Удалить тьютора из школы | ✅ | ❌ | — |
| Видеть всех учеников школы | ✅ | ❌ | — |
| Управлять своими учениками | ✅ | ✅ | ✅ |
| Создавать контент/вопросы | ✅ | ✅ | ✅ |
| Аналитика по школе | ✅ | ❌ | — |

### 4. Генерация контента тьюторами

#### Задача
Тьюторы должны иметь возможность создавать **свои вопросы** для назначения ученикам, не влияя на общую базу вопросов платформы.

#### Подход — «Приватный пул»
- Вопросы тьютора хранятся в той же таблице `Questions`, но с полем `CreatedByTutorId`
- Вопросы без `CreatedByTutorId` (= null) → общие вопросы платформы
- Вопросы с `CreatedByTutorId` → приватные, видны только тьютору и его ученикам

#### Новые поля (Domain)
```
Question {
  ... существующие поля ...
  CreatedByTutorId: int?  — если null, это вопрос платформы
  IsPrivate: bool         — по умолчанию true для тьюторских
}
```

#### Функционал для тьютора
1. **Создание вопросов** — через панель тьютора (интерфейс аналогичен admin-панели)
   - Выбор экзамена / секции / темы
   - Текст вопроса, 4 варианта ответа, правильный, объяснение
   - Сложность (Easy/Medium/Hard)
   - Мат. клавиатура (KaTeX)
2. **Импорт вопросов** — загрузка из файла (JSON/Excel)
3. **Назначение домашнего задания**:
   - Тьютор выбирает вопросы (свои + общие) → создаёт «задание»
   - Указывает учеников и дедлайн
   - Ученики видят задание в своей панели
4. **Аналитика по заданиям** — % выполнения, средний балл, время

#### Новые сущности
```
Assignment {
  Id, TutorUserId, Title, Description,
  Deadline, CreatedAt, IsActive
}

AssignmentQuestion {
  Id, AssignmentId, QuestionId, OrderIndex
}

AssignmentStudent {
  Id, AssignmentId, StudentUserId,
  Status (Assigned/InProgress/Completed/Overdue),
  StartedAt, CompletedAt, Score
}

AssignmentAnswer {
  Id, AssignmentStudentId, QuestionId,
  SelectedOptionId, IsCorrect, AnsweredAt
}
```

### 5. Финансовая модель

#### Revenue потоки
1. **Ученики**: 10 000 ₸/мес (или 7 000 ₸ при тьюторе) × MAU
2. **Тьюторы**: 20 000 ₸/год × количество тьюторов
3. **Школы**: 50 000 ₸/год × количество школ

#### Пример (100 MAU, 10 тьюторов, 2 школы)
- Ученики без тьютора: 50 × 10 000 = 500 000 ₸/мес
- Ученики с тьютором: 50 × 7 000 = 350 000 ₸/мес
- Тьюторы: 10 × 20 000 = 200 000 ₸/год (16 667 ₸/мес)
- Школы: 2 × 50 000 = 100 000 ₸/год (8 333 ₸/мес)
- **Итого: ~875 000 ₸/мес (~$1 750)**

### 6. Этапы реализации

#### Этап 1 — Привязка тьютор ↔ ученик (MVP) ✅
- [x] Модель `TutorStudent` + миграция
- [x] API: генерация инвайт-кода, привязка по коду, отвязка
- [x] Frontend: ввод инвайт-кода в профиле ученика
- [x] Frontend: список учеников в панели тьютора (с аналитикой)
- [x] Скидка 7 000 ₸ при привязке (через `SubscriptionService`)

#### Этап 2 — Генерация контента тьюторами ✅
- [x] `Question.CreatedByTutorId` + `IsPrivate` + миграция `AddTutorQuestionContent`
- [x] API: CRUD вопросов для тьютора (`/api/tutors/questions`)
- [x] Frontend: страница создания вопросов в панели тьютора (`TutorQuestionsPage`)
- [x] Фильтрация: тьютор видит свои вопросы, поиск, фильтр по экзамену
- [x] i18n: ru/en/kz
- [x] Навигация: роут `/questions` + ссылка «Вопросы» в TutorLayout

#### Этап 3 — Домашние задания ✅
- [x] Модели `Assignment`, `AssignmentQuestion`, `AssignmentStudent`, `AssignmentAnswer`
- [x] EF Core конфигурация, DbSets, миграция `AddAssignments`
- [x] API: создание задания, назначение ученикам, получение результатов (8 эндпоинтов)
- [x] Frontend тьютора: `TutorAssignmentsPage` — список, создание, детали с прогрессом учеников
- [x] Frontend ученика: `StudentAssignmentsPage` — список заданий, прохождение по вопросам, результаты
- [x] i18n: навигационный ключ `assignments` (ru/en/kz)
- [x] Навигация: роуты `/assignments` для тьютора и ученика + ссылки в Layout

#### Этап 4 — Школьный профиль ✅
- [x] `TutorSchool` — добавлены `OwnerUserId`, `UpdatedAt`, FK → Users
- [x] Миграция `AddSchoolOwnerAndUpdatedAt`
- [x] DTOs: `CreateSchoolDto`, `UpdateSchoolDto`, `SchoolAdminDto`
- [x] API: CRUD школы + управление тьюторами (5 эндпоинтов в TutorController)
- [x] `TutorService`: `CreateSchool`, `GetMySchool`, `UpdateSchool`, `AddTutor`, `RemoveTutor` + slug-генерация (кириллица→латиница)
- [x] Frontend: типы `SchoolAdmin`, `CreateSchoolRequest`, `UpdateSchoolRequest`
- [x] Frontend: `tutorService` — 5 методов API (create, getMySchool, update, addTutor, removeTutor)
- [x] Frontend: `TutorSchoolManagePage` — создание школы / редактирование / управление преподавателями
- [x] Навигация: «Школа» в `TutorLayout`, роут `/school` в `App.tsx`
- [ ] Frontend: публичная страница школы `/schools/:slug` (уже существует базовая)
- [ ] Интеграция LinHao International School как первый пилот

#### Этап 5 — Платёжная интеграция
- [ ] Kaspi Pay / Halyk epay — подключение
- [ ] Автоматическое продление подписки
- [ ] Скидка при привязке к тьютору (автоматический расчёт)
- [ ] Чеки и история платежей

---

### Sprint 7 — Баг-фиксы (19 марта 2026)

#### FIX-55: TutorProfile не создавался при смене роли → 404 на `/api/tutors/{id}`
**Проблема**: Администратор менял роль пользователю на Tutor через `AdminService.UpdateUserAsync`, но `TutorProfile` запись НЕ создавалась. Из-за этого:
- `GET /api/tutors/{id}` → 404 (профиль не найден)
- `PUT /api/tutors/profile` → 500 (KeyNotFoundException)
- Тьютор не отображался в каталоге для студентов
- `TutorProfileEditPage` и `TutorReviewsPage` не могли загрузить данные

**Исправление**:
- [x] `AdminService.UpdateUserAsync` — при смене роли на Tutor автоматически создаёт `TutorProfile`
- [x] `TutorService.GetTutorProfileAsync` — авто-создание профиля если User.Role == Tutor, но TutorProfile отсутствует
- [x] `TutorService.UpdateMyProfileAsync` — вызывает `EnsureTutorProfileAsync` перед обновлением

---

## 🔒 АУДИТ БЕЗОПАСНОСТИ — 19 марта 2026

### Текущее состояние защиты платформы

#### ✅ Что УЖЕ реализовано (OWASP Top 10 покрытие)

| # | OWASP Категория | Статус | Реализация |
|---|-----------------|--------|------------|
| A01 | Broken Access Control | ✅ | JWT + `[Authorize(Roles = "...")]` на каждом эндпоинте, проверка `userId` из токена |
| A02 | Cryptographic Failures | ✅ | BCrypt хэширование паролей, JWT с HMAC-SHA256, HTTPS redirect |
| A03 | Injection | ✅ | EF Core параметризованные запросы (SQL injection), `InputSanitizer` (XSS), HTML tag stripping |
| A04 | Insecure Design | ✅ | Clean Architecture, валидация на уровне DTO, бизнес-логика в сервисах |
| A05 | Security Misconfiguration | ✅ | CSP, HSTS (preload), CORS ограничены, X-Content-Type-Options: nosniff |
| A06 | Vulnerable Components | ✅ | `dotnet list package --vulnerable` = 0, npm audit = 8 dev-only (eslint/vite) |
| A07 | Auth Failures | ✅ | Rate limiting (auth: 10/мин), email verification, JWT expiry, refresh tokens |
| A08 | Data Integrity | ⚠️ | Docker images without pinned digests, нет SBOM |
| A09 | Logging & Monitoring | ✅ | Serilog (file + console), audit log, request logging с UserId |
| A10 | SSRF | ✅ | Нет user-controlled URL fetching, DeepSeek API через фиксированный BaseURL |

#### ✅ Дополнительные меры безопасности

- **Rate Limiting**: auth 10/мин, api 120/мин, global 200/мин per IP
- **Input Sanitization**: `InputSanitizer.Sanitize()` на всех user-facing полях (Auth, Tutor, Message, Admin, Assignments)
- **Soft Delete**: данные не удаляются физически (аудит, восстановление)
- **Audit Log**: действия админов логируются (кто, когда, что)
- **Health Checks**: `/health/live`, `/health/ready` (PostgreSQL)
- **Security Headers**: CSP, HSTS, X-Content-Type-Options, X-Frame-Options, Referrer-Policy
- **CORS**: ограничены origins (только `https://unistart.kz` в production)

### 🔐 Разграничение доступа: Админ ↔ Тьютор ↔ Студент

#### Текущая модель (19 марта 2026)

```
Admin (полный доступ)
  ├── Управление пользователями (CRUD, смена ролей, блокировка)
  ├── Управление контентом (вопросы, экзамены, темы)
  ├── Статистика платформы и аудит
  ├── Импорт вопросов (PDF/DOCX/XLSX)
  └── Доступ к тьюторским эндпоинтам [Roles = "Tutor,Admin"]

Tutor (свой контент + привязанные ученики)
  ├── Свой профиль (редактирование, расписание, отзывы)
  ├── Привязка учеников (инвайт-код)
  ├── CRUD своих приватных вопросов (IsPrivate = true)
  ├── Создание заданий для привязанных учеников
  └── НЕТ доступа к admin-панели

Student (только своё)
  ├── Практика, моки, аналитика (ограничено подпиской)
  ├── Привязка к тьютору (ввод инвайт-кода)
  ├── Прохождение заданий от тьютора
  ├── Чат с тьютором (SignalR)
  └── НЕТ доступа к тьюторским и admin-эндпоинтам
```

#### Рекомендации для enterprise-grade безопасности

**Приоритет 1 — Критичные (до деплоя):**

| # | Мера | Статус | Описание |
|---|------|--------|----------|
| S-1 | Контент-изоляция тьютора | ⬜ | Админ видит все вопросы для модерации, но НЕ может редактировать приватные вопросы тьютора без его согласия. Логировать доступ |
| S-2 | Ownership checks на вcех endpoint'ах | ✅ | Тьютор может управлять только СВОИМИ вопросами/заданиями (проверка `CreatedByTutorId == userId`) |
| S-3 | Student scoping | ✅ | Студент видит только СВОИ задания (проверка `AssignmentStudent.StudentUserId == userId`) |
| S-4 | Password policy | ✅ | 8+ символов, uppercase, lowercase, digit, special char — backend `ValidatePasswordComplexity` + frontend regex |
| S-5 | Account lockout | ✅ | 5 неудачных попыток → 15 мин блокировки. `User.FailedLoginAttempts` + `LockoutEnd`. Сброс при успешном входе |

**Приоритет 2 — Важные (первый месяц после запуска):**

| # | Мера | Статус | Описание |
|---|------|--------|----------|
| S-6 | Промокоды тьюторов | ✅ | `TutorInviteCode` + `TutorInviteCodeUsage` entities. MaxUses, ExpiresAt, IsActive, Note. CRUD endpoints `invite-codes`. Миграция `AddSecurityAndInviteCodes` |
| S-7 | Session management | ⚠️ | JWT без server-side revocation — добавить blacklist при logout / смене пароля |
| S-8 | Data encryption at rest | ⬜ | PostgreSQL: включить TDE или pgcrypto для PII (email, имя) |
| S-9 | 2FA | ⬜ | TOTP (Google Authenticator) для тьюторов и админов |
| S-10 | API versioning header | ✅ | `x-api-version` header support |

**Приоритет 3 — Продвинутые (масштабирование):**

| # | Мера | Статус | Описание |
|---|------|--------|----------|
| S-11 | WAF | ⬜ | Cloudflare WAF или ModSecurity перед nginx |
| S-12 | DDoS protection | ⬜ | Cloudflare (бесплатный план) |
| S-13 | Penetration testing | ⬜ | Заказать у независимой компании перед публичным запуском |
| S-14 | GDPR/ЗРК compliance audit | ⬜ | Формальный аудит соответствия Закону РК о персональных данных |
| S-15 | Backup encryption | ⬜ | Шифрование pg_dump бэкапов (GPG) перед хранением |

### 🎟️ Промокоды тьюторов — Детальный план

**Текущий механизм**: Тьютор генерирует `InviteCode` (6 символов, многоразовый, бессрочный). Любой студент может ввести код и привязаться.

**Проблемы**:
- Код можно передать неограниченному числу студентов
- Нет срока действия
- Нет истории кто использовал код

**Рекомендуемая эволюция (Этап 6 Sprint 8)**:

```
TutorInviteCode {
  Id, TutorUserId,
  Code (string, unique, 8 chars),
  MaxUses (int?, null = unlimited),
  UsedCount (int, default 0),
  ExpiresAt (DateTime?),
  IsActive (bool, default true),
  Note (string?, "Для группы 11А"),
  CreatedAt
}

TutorInviteCodeUsage {
  Id, InviteCodeId, StudentUserId, UsedAt
}
```

**Функционал**:
- Тьютор создаёт коды с лимитом (например, "5 студентов") и сроком ("до 1 апреля")
- История использования: кто и когда ввёл код
- Деактивация кода (IsActive = false)
- Несколько активных кодов одновременно (для разных групп)
- Автоматическая деактивация по истечении срока

### 📊 Защита данных пользователей

**Какие PII (Personally Identifiable Information) хранятся:**

| Данные | Где хранится | Защита |
|--------|-------------|--------|
| Email | Users.Email | BCrypt hash нет (plain), но HTTPS + DB access control |
| Имя | Users.Name | Plain text, sanitized |
| Пароль | Users.PasswordHash | BCrypt (cost factor 12) ✅ |
| IP-адрес | Serilog logs | 14-day retention, файл на сервере |
| Ответы на тесты | UserAnswers | Привязаны к UserId, no external sharing |
| Чат-сообщения | Messages | Sanitized, только участники видят |

**Рекомендации по защите PII:**
1. Email — не показывать полный email другим пользователям (маскировать: `k***@gmail.com`)
2. Логи — исключить PII из логов (не логировать email в request body)
3. Экспорт данных — реализовать "Download my data" (GDPR Article 20 / ЗРК)
4. Удаление аккаунта — реализовать полное удаление (сейчас только soft delete)
5. Cookie consent — ✅ уже реализован (CookieBanner)

### 🚨 Известные ограничения (принятые риски)

| Риск | Уровень | Mitigation |
|------|---------|------------|
| JWT без server-side revocation | Средний | Короткий TTL (60 мин), refresh token rotation при реализации |
| In-memory rate limiting | Низкий | Достаточно для одного сервера. Redis при масштабировании |
| Нет 2FA | Средний | Планируется S-9. Снижен rate limiting на auth |
| Email в plain text в БД | Низкий | DB access control + SSL connection. pgcrypto при необходимости |
| Single server | Средний | Бэкапы ежедневно. Мониторинг uptime |

---

## 🏫 Sprint 7 — White Label B2B (26 марта 2026)

### Архитектура White Label

```
Пользователь → *.unistart.kz (wildcard SSL)
  → nginx (wildcard server_name)
    → React SPA (единое приложение)
      → BrandingContext определяет subdomain по window.location.hostname
        → GET /api/tutors/schools/branding?slug={subdomain}
          → CSS Variables, favicon, SEO meta, фильтрация контента
```

**Ключевой принцип**: Единое SPA, единая БД, единый API. Subdomain определяет контекст через `BrandingContext`. Нет отдельных деплоев для каждой школы.

### Модель данных White Label

```
User
  ├── SchoolId (int?, FK → TutorSchools.Id, ON DELETE SET NULL)
  │   └── Автоматически устанавливается при регистрации на subdomain
  └── Role: Student | Tutor | Admin | SchoolAdmin

TutorSchool
  ├── OwnerUserId (int?, FK → Users.Id)
  │   └── Владелец школы — единственный кто может управлять через SchoolAdminController
  ├── Slug (string, unique) → используется как subdomain
  ├── Subdomain (string?) → опциональный override
  └── Branding: NavbarTitle, PrimaryColor, LogoUrl, Favicon, Description

TutorProfile
  └── SchoolId (int?, FK → TutorSchools.Id)
      └── Привязка тьютора к школе (через owner или invite)
```

### Безопасность: Модель назначения SchoolAdmin

#### Кто может назначить SchoolAdmin?

**Только UniStart Admin** — через `AdminService.UpdateUserAsync`:
```
POST /api/admin/users/{id} { role: "SchoolAdmin" }
```

SchoolAdmin НЕ может:
- Назначить другого SchoolAdmin
- Повысить себя до Admin
- Создать новую школу напрямую (только через API тьюторов)

#### Цепочка назначения (security flow)

```
1. UniStart Admin создаёт пользователя с Role = SchoolAdmin
   ИЛИ меняет существующему пользователю роль на SchoolAdmin

2. SchoolAdmin (как Tutor) создаёт школу:
   POST /api/tutors/school → TutorService.CreateSchoolAsync
   → Устанавливает TutorSchool.OwnerUserId = текущий userId
   → Привязывает свой TutorProfile.SchoolId к школе

3. SchoolAdmin назначает тьюторов в школу:
   POST /api/tutors/school/tutors/{tutorId} → AddTutorToSchoolAsync
   → Проверка: OwnerUserId == requestUserId (ownership check)
   → Устанавливает TutorProfile.SchoolId = school.Id

4. Студенты привязываются автоматически при регистрации на subdomain:
   POST /api/auth/register { schoolSlug: "linhao" }
   → AuthService: User.SchoolId = school.Id
```

#### Разграничение доступа: Admin vs SchoolAdmin

| Действие | UniStart Admin | SchoolAdmin |
|----------|---------------|-------------|
| Управление всеми пользователями | ✅ | ❌ |
| Просмотр студентов своей школы | ✅ | ✅ (SchoolAdminController) |
| Просмотр тьюторов своей школы | ✅ | ✅ (SchoolAdminController) |
| Аналитика студентов школы | ✅ | ✅ (students/{id}/analytics) |
| Назначение ролей | ✅ | ❌ |
| Создание школы | ✅ (через API) | ✅ (только свою) |
| Редактирование школы | ✅ (через БД) | ✅ (только свою, ownership check) |
| Добавление тьютора в школу | ✅ (через БД) | ✅ (только в свою школу) |
| Удаление тьютора из школы | ✅ (через БД) | ✅ (кроме себя) |
| Управление контентом (вопросы, экзамены) | ✅ | ❌ |
| Доступ к admin-панели /admin/* | ✅ | ❌ |
| Dashboard школы /school-admin/* | ✅ | ✅ |
| Управление подписками/платежами | ✅ | ❌ |

#### Ownership checks в SchoolAdminController

Каждый endpoint проверяет `GetOwnedSchool()`:
```csharp
private async Task<TutorSchool?> GetOwnedSchool()
{
    var userId = GetUserId(); // из JWT ClaimTypes.NameIdentifier
    return await _db.TutorSchools.FirstOrDefaultAsync(s => s.OwnerUserId == userId && s.IsActive);
}
```
Если школа не найдена → 404 "You don't own a school". Нет возможности обратиться к чужой школе.

### Выполненные задачи White Label

- [x] `User.SchoolId` (int?, FK) + миграция `AddUserSchoolId`
- [x] Auto-bind при регистрации на subdomain (`AuthService` → `SchoolSlug`)
- [x] `SchoolAdminController` — dashboard, students, tutors, analytics (4 endpoint'а)
- [x] `SchoolAdminDashboardPage` — React страница с карточками и таблицей
- [x] `WhiteLabelLanding` — брендированная landing page с i18n (ru/en/kz)
- [x] `BrandingContext` — CSS variables, navbar, favicon, SEO meta
- [x] Wildcard SSL `*.unistart.kz` + nginx конфигурация
- [x] Rate limiting: auth 20/мин, `google-client-id` исключён (`[DisableRateLimiting]`)
- [x] Google OAuth: поддомены добавлены в Google Cloud Console, кнопка работает на WL
- [x] Onboarding tour: показывает имя школы вместо "UniStart" на WL (`GuidedTour`)
- [x] TutorsPage: скрывает секцию "Партнёрские школы" на WL, тьюторы фильтруются по schoolId

### Pending White Label задачи

- [ ] SchoolDetailPage: i18n (сейчас hardcoded Russian), убрать бейдж "Партнёр UniStart" на WL
- [ ] Admin-панель: вкладка управления школами (список всех школ, назначение SchoolAdmin)
- [ ] Главная UniStart: секция "Наши школы-партнёры" с ссылками на сайты школ
- [ ] SchoolAdmin invite flow: админ UniStart отправляет email-приглашение → пользователь получает роль SchoolAdmin
- [ ] COOP header: `Cross-Origin-Opener-Policy: same-origin-allow-popups` для Google OAuth popup (cosmetic)
- [ ] Аналитика по школам: сводная статистика всех школ для UniStart Admin
- [ ] Кастомный домен для школы (school.example.com → CNAME → unistart.kz, nginx proxy)
- [ ] Школьные тарифные планы: 49 990 ₸/год (единый тариф)

---

## 🎁 Sprint 8 — Реферальная программа и юридический аудит (апрель 2026)

### Анализ юридических документов

#### 1. PrivacyPolicy.md — Политика конфиденциальности
- **Основание**: Закон РК «О персональных данных» №94-V от 21.05.2013
- **Оператор**: ИП Каландаров (БИН/ИИН и адрес — шаблонные `__________________`, НУЖНО ЗАПОЛНИТЬ)
- **Собираемые данные**: имя, email, пароль (bcrypt), ответы на тесты, IP, cookies (JWT, язык, тема)
- **НЕ собираемые**: биометрия, банковские данные (обрабатывает Kaspi Pay), данные <14 лет
- **Хранение**: до удаления аккаунта; при неактивности >2 лет — уведомление + 30 дней; IP-логи до 12 мес
- **Удаление**: по запросу — 30 дней из БД, 60 дней из бэкапов, уведомление после уничтожения
- **Права пользователя**: информирование, уточнение, блокировка, уничтожение, отзыв согласия, обжалование
- **Передача**: только с согласия, для исполнения договора (платёжные системы), по закону РК

#### 2. EmployerAgreement.md — Пользовательское соглашение
- **11 разделов**: предмет, аккаунт, права/обязанности, AI IP, персональные данные, ответственность, споры
- **Подписки**: Free (ограниченный), Pro (10 000 ₸/мес, 99 990 ₸/год), с тьютором (7 000 ₸/мес)
- **Возврат**: 14 дней с даты оплаты (ЗРК «О защите прав потребителей»)
- **Удаление аккаунта**: пользователь может запросить через unistart.kz@gmail.com
- **Возраст**: 14+ (несовершеннолетние с согласия родителей)
- **AI контент**: ИС генерируемого контента принадлежит UniStart
- **Споры**: досудебная претензия 30 дней → суд по месту регистрации оператора

#### 3. ReferralAgreement.md — Партнёрский договор реферальной программы
- **Принятие оферты**: активация промокода в личном кабинете = заключение договора
- **Типы партнёров**: Тьютор, Школа, Студент
- **Условия начисления**: только НОВЫЙ пользователь + оплата Pro подписки
- **Вознаграждения**:
  - Тьютор: 500 ₸ за каждого привлечённого, вывод от 5 000 ₸
  - Школа: 500 ₸ за каждого привлечённого, вывод от 10 000 ₸
  - Студент: 5 дней Pro за каждого привлечённого (без денежных выплат!)
- **Антифрод**: запрет спама, купонных агрегаторов, платной рекламы без согласования, дубликатов аккаунтов
- **Аннулирование**: при возврате подписки, при нарушении условий, если промокод на агрегаторе
- **Срок хранения**: невостребованное вознаграждение аннулируется через 12 месяцев
- **Выплаты**: на расчётный счёт, реквизиты на unistart.kz@gmail.com, партнёр сам платит налоги
- **Расторжение**: админ — с уведомлением за 14 дней, партнёр — в любое время

---

### Юридический аудит: что реализовано vs что требуется

| Требование | Документ | Статус | Что нужно |
|------------|----------|--------|-----------|
| Согласие на обработку ПД при регистрации | Privacy §2.1.1 | ✅ | Чекбокс + ссылки /terms, /privacy |
| Ссылки на Политику в футере | Privacy §10 | ✅ | Footer на LandingPage |
| Cookie consent | Privacy §4.5 | ✅ | CookieBanner компонент |
| Удаление аккаунта по запросу | Privacy §9 | ⚠️ | Нет кнопки «Удалить аккаунт» в профиле. Только через email |
| Экспорт данных (ЗРК/GDPR) | Privacy §7 | ❌ | Нет функции «Скачать мои данные» |
| Маскировка email у других пользователей | Privacy §5.4 | ⚠️ | Email виден тьютору, админу. Маскировка не реализована |
| Возврат подписки (14 дней) | Terms §5.3 | ❌ | Нет механизма возврата (нет платёжной интеграции) |
| Ограничение возраста 14+ | Terms §4.2 | ⚠️ | Нет проверки возраста на фронте, только в соглашении |
| Реферальная программа | Referral | ❌ | Полностью отсутствует — нужна реализация |
| Промокод в личном кабинете | Referral §1 | ❌ | Нет секции реферальной программы в ProfilePage |
| Отслеживание рефералов | Referral §1.4 | ❌ | Нет сущностей и API |
| Выплаты/начисления | Referral §2 | ❌ | Нет бэкенд-логики |
| БИН/ИИН/адрес оператора | Privacy §3.1 | ⚠️ | Заглушки `__________________` — ЗАПОЛНИТЬ |

---

### Реферальная система — Архитектура и план реализации

#### Новые сущности (Domain)

```csharp
// Domain/Entities/ReferralCode.cs
public class ReferralCode
{
    public int Id { get; set; }
    public int OwnerUserId { get; set; }         // FK → Users.Id (партнёр)
    public User Owner { get; set; } = null!;
    public string Code { get; set; } = "";        // Уникальный код, 8 символов (UPPER + цифры)
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public int UsedCount { get; set; } = 0;       // Денормализация для быстрого отображения
}

// Domain/Entities/ReferralUsage.cs
public class ReferralUsage
{
    public int Id { get; set; }
    public int ReferralCodeId { get; set; }       // FK → ReferralCodes.Id
    public ReferralCode ReferralCode { get; set; } = null!;
    public int ReferredUserId { get; set; }       // FK → Users.Id (привлечённый)
    public User ReferredUser { get; set; } = null!;
    public DateTime RegisteredAt { get; set; }    // Когда зарегистрировался
    public DateTime? PaidAt { get; set; }         // Когда оплатил Pro (null = ещё не оплатил)
    public bool RewardGranted { get; set; }       // Вознаграждение начислено?
}

// Domain/Entities/ReferralReward.cs
public class ReferralReward
{
    public int Id { get; set; }
    public int OwnerUserId { get; set; }          // FK → Users.Id (получатель)
    public User Owner { get; set; } = null!;
    public int ReferralUsageId { get; set; }      // FK → ReferralUsages.Id
    public ReferralUsage Usage { get; set; } = null!;
    public string RewardType { get; set; } = "";  // "money" или "days"
    public decimal Amount { get; set; }           // 500 (₸) или 5 (дней)
    public bool IsPaidOut { get; set; }           // Выплачено?
    public DateTime GrantedAt { get; set; }
    public DateTime? PaidOutAt { get; set; }
    public DateTime ExpiresAt { get; set; }       // GrantedAt + 12 месяцев
}
```

#### Новые поля в User

```csharp
// User.cs — добавить
public string? ReferralCode { get; set; }         // Промокод, если есть (связь 1:1 с ReferralCode)
public int? ReferredByCodeId { get; set; }        // Через какой промокод зарегистрировался
```

#### API Endpoints

```
POST   /api/referral/activate          — Активировать реферальную программу (генерация кода)
GET    /api/referral/my                — Мой промокод, статистика, вознаграждения
GET    /api/referral/stats             — Детальная статистика: привлечённые, оплатившие, суммы
POST   /api/referral/withdraw          — Запрос на вывод вознаграждения
POST   /api/auth/register              — Обновить: принимать ?ref=ПРОМОКОД → ReferredByCodeId
```

#### Бизнес-логика (ReferralService.cs)

1. **Активация**: Пользователь нажимает «Активировать» → генерация `ReferralCode` (8 символов, UPPER + цифры), привязка к пользователю
2. **Регистрация по коду**: `?ref=CODE` → `AuthService.RegisterAsync` записывает `ReferredByCodeId`, создаёт `ReferralUsage` с `PaidAt = null`
3. **Оплата Pro**: `SubscriptionService` при оплате проверяет `ReferredByCodeId` → если есть и `ReferralUsage.PaidAt == null`:
   - `PaidAt = now`, `RewardGranted = true`
   - Создаёт `ReferralReward`:
     - Если владелец кода = Tutor/SchoolAdmin → `RewardType = "money"`, `Amount = 500`
     - Если владелец кода = Student → `RewardType = "days"`, `Amount = 5`
   - Инкрементирует `ReferralCode.UsedCount`
4. **Вывод**: Партнёр запрашивает вывод → admin видит заявку → manual payout (пока нет автоматики)

#### Frontend — ProfilePage: секция «Реферальная программа»

```
┌─────────────────────────────────────────────────┐
│  🎁 Реферальная программа                        │
│                                                   │
│  [Ваш промокод ещё не активирован]               │
│  [Активировать программу]                        │
│                                                   │
│  — ПОСЛЕ АКТИВАЦИИ: —                            │
│                                                   │
│  Ваш промокод: ABC12345   [📋 Копировать]        │
│  Ссылка: unistart.kz/register?ref=ABC12345       │
│                                                   │
│  📊 Статистика:                                   │
│  Привлечено: 12 | Оплатили Pro: 5                │
│  Ваше вознаграждение: 2 500 ₸ (или +25 дней Pro) │
│  К выплате: 2 500 ₸ [Вывести]                   │
│                                                   │
│  ℹ️ Условия:                                      │
│  • Тьюторы/школы: 500 ₸ за каждого               │
│  • Студенты: 5 дней Pro за каждого                │
│  • Подробнее: /referral-terms                     │
└─────────────────────────────────────────────────┘
```

#### Frontend — LandingPage: секция «Реферальная программа»

Добавить ПЕРЕД секцией «Партнёрство» (или заменить CTA внутри неё):

```
┌─────────────────────────────────────────────────┐
│  🎁 Приглашай друзей — получай бонусы            │
│                                                   │
│  Студенты: +5 дней Pro за каждого друга          │
│  Тьюторы: 500 ₸ за каждого ученика              │
│  Школы: 500 ₸ за каждого учащегося              │
│                                                   │
│  [Зарегистрироваться и получить промокод]        │
└─────────────────────────────────────────────────┘
```

#### Frontend — RegisterPage: параметр ?ref=

```
// RegisterPage.tsx — уже есть useSearchParams
const refCode = searchParams.get('ref');

// При handleSubmit → добавить referralCode в payload:
dispatch(register({ ..., referralCode: refCode || undefined }));
```

#### Frontend — Новая страница /referral-terms

Публичная страница `ReferralTermsPage.tsx` — рендер Партнёрского договора (аналогично TermsPage/PrivacyPage). Контент из ReferralAgreement.md, но в i18n формате.

---

### Этапы реализации реферальной программы

#### Этап 1 — Backend (Domain + Migration + Service)
- [ ] `Domain/Entities/ReferralCode.cs` + `ReferralUsage.cs` + `ReferralReward.cs`
- [ ] `User.ReferralCode`, `User.ReferredByCodeId` — новые поля
- [ ] EF Core конфигурация в `UniStartDbContext` + DbSets
- [ ] Миграция `AddReferralSystem`
- [ ] `Application/Interfaces/IReferralService.cs`
- [ ] `Application/Services/ReferralService.cs` — Activate, GetMyStats, RecordUsage, GrantReward
- [ ] `Application/DTOs/ReferralDtos.cs` — ReferralStatsDto, ReferralActivateDto
- [ ] `Controllers/ReferralController.cs` — 4 endpoints
- [ ] Обновить `AuthService.RegisterAsync` — обработка `referralCode` параметра
- [ ] Обновить `SubscriptionService` — при оплате Pro → начислить реферальное вознаграждение

#### Этап 2 — Frontend (Profile + Landing + Register)
- [ ] `ProfilePage.tsx` — секция «Реферальная программа» (активация, промокод, статистика)
- [ ] `services/referralService.ts` — API клиент (activate, getMyStats, withdraw)
- [ ] `LandingPage.tsx` — секция «Приглашай друзей» перед партнёрством
- [ ] `RegisterPage.tsx` — обработка `?ref=` параметра, передача в API
- [ ] `ReferralTermsPage.tsx` — публичная страница с Партнёрским договором
- [ ] `App.tsx` — роут `/referral-terms`
- [ ] i18n: секция `referral` (ru/en/kz) — ~30 ключей

#### Этап 3 — Админка
- [ ] `AdminReferralPage.tsx` — просмотр всех рефералов, заявки на вывод, ручное одобрение выплат
- [ ] `adminService.ts` — getReferralStats, approveWithdrawal
- [ ] `AdminController.cs` — endpoint'ы для управления реферальной программой

---

### Исправления, реализованные в текущем спринте (апрель 2026)

- [x] **Admin badge dismiss**: Красный бейдж в AdminLayout теперь сохраняется до первого посещения страницы (localStorage: `admin_seen_schools`, `admin_seen_verifications`). При клике на «Тьюторы» — dismiss тьюторского бейджа, при клике на «Школы» в dropdown — dismiss школьного бейджа
- [x] **User counts fix**: `AdminService.GetUserStatsAsync()` — теперь Admins = Admin + SchoolAdmin (было только Admin). Tutors = Tutor + SchoolTutor (было уже корректно)
- [x] **School soft-delete**: Работает корректно — публичный список (`TutorService.GetSchoolsAsync`) фильтрует по `IsActive`, админ-панель показывает все школы. Восстановление через `handleRestore` / `RestoreSchool` endpoint
- [x] **School branding**: `GetSchoolBrandingAsync` — убран фильтр `IsActive`, subdomain-брендинг работает для всех школ (даже отклонённых)
- [x] **Registration page**: Уже содержит чекбокс согласия + ссылки на /terms и /privacy (FIX-43)

---

### Приоритетные TODO (ближайшие действия)

#### Критичные (перед запуском реферальной программы)
1. ⬜ Заполнить БИН/ИИН и адрес в PrivacyPolicy.md (заглушки `__________________`)
2. ⬜ Конвертировать PrivacyPolicy.md и ReferralAgreement.md из CP-1251 в UTF-8
3. ⬜ Реализовать кнопку «Удалить аккаунт» в ProfilePage (Privacy §9, Terms §4.3.3)
4. ⬜ Создать страницу /referral-terms (рендер Партнёрского договора)
5. ⬜ Добавить ссылку на /referral-terms в Footer (рядом с /terms и /privacy)

#### Важные (первый месяц)
6. ⬜ Backend реферальной системы (entities, migration, service, controller)
7. ⬜ Frontend реферальной программы (profile секция, landing секция, register ?ref=)
8. ⬜ Админка реферальной программы (просмотр, одобрение выплат)
9. ⬜ Email-уведомления: «Ваш реферал оплатил Pro» / «Вознаграждение начислено»
10. ⬜ Экспорт данных «Скачать мои данные» (Privacy §7, ЗРК о ПД)

#### Желательные (расширение)
11. ⬜ Автоматические выплаты (интеграция с банковским API)
12. ⬜ Реферальный дашборд для тьюторов (в TutorLayout)
13. ⬜ A/B тестирование реферальных бонусов (500₸ vs 1000₸)
14. ⬜ Push/email-напоминания неактивным партнёрам
