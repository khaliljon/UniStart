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
