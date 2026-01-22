# Система предпочтений пользователя и рекомендаций

## Обзор

Реализована полноценная система предпочтений пользователя с onboarding опросом и настройками профиля для персонализированных рекомендаций.

## Backend (C# / .NET)

### Модель данных

**UserPreferences** (`Models/Learning/UserPreferences.cs`)
- Цели обучения (learningGoal, targetExamType, careerGoal)
- География (preferredCountry, preferredCity, willingToRelocate)
- Финансы (maxBudgetPerYear, interestedInScholarships)
- Языки (preferredLanguagesJson, englishLevel)
- Предметы (InterestedSubjects, StrongSubjects, WeakSubjects)
- Формат обучения (prefersFlashcards, prefersQuizzes, prefersExams)
- Расписание (dailyStudyTimeMinutes, preferredStudyTime, studyDays)
- Мотивация (motivationLevel, needsReminders)

### API Endpoints

**PreferencesController** (`/api/student/preferences`)

```csharp
GET    /api/student/preferences                    // Получить предпочтения
GET    /api/student/preferences/onboarding/status  // Статус onboarding
POST   /api/student/preferences                    // Создать/обновить
POST   /api/student/preferences/onboarding/complete // Завершить onboarding
POST   /api/student/preferences/onboarding/skip    // Пропустить onboarding
GET    /api/student/preferences/recommended-subjects // Рекомендуемые предметы
```

**ProfileController** (`/api/profile`)
- Теперь возвращает предпочтения вместе с профилем

### Сервисы

**UserPreferencesService**
- `GetUserPreferencesAsync()` - получение предпочтений
- `CreateOrUpdatePreferencesAsync()` - создание/обновление
- `SkipOnboardingAsync()` - пропуск с дефолтными значениями
- `HasCompletedOnboardingAsync()` - проверка статуса

**RecommendationService**
- Использует предпочтения для генерации персонализированных рекомендаций
- Учитывает: предметы, сложность, формат обучения, цели

## Frontend (React / TypeScript)

### Страницы

**OnboardingPage** (`/onboarding`)
- 7 шагов многошаговой формы
- Красивая анимация переходов
- Прогресс-бар
- Кнопка "Пропустить"
- Валидация на каждом шаге

Шаги:
1. Цели обучения (ENT, University, SelfStudy, Professional)
2. География (страна, город, готовность к переезду)
3. Финансы (бюджет, гранты)
4. Языки (предпочитаемые языки, уровень английского)
5. Предметы (интересующие, сильные, слабые)
6. Формат обучения (карточки, квизы, экзамены, время, дни)
7. Мотивация (уровень, напоминания)

**PreferencesPage** (`/profile/preferences`)
- Полное редактирование всех предпочтений
- Разделение по категориям
- Автосохранение
- Валидация

### Сервисы

**preferencesService.ts**
```typescript
getMyPreferences()              // Получить предпочтения
checkOnboardingStatus()         // Проверить статус
createOrUpdatePreferences()     // Сохранить
completeOnboarding()            // Завершить
skipOnboarding()                // Пропустить
getRecommendedSubjects()        // Рекомендации
```

### Роутинг и редиректы

1. **После регистрации** → `/onboarding`
2. **При входе в Dashboard** → проверка onboarding статуса
3. **Если onboarding не завершён** → автоматический редирект на `/onboarding`
4. **Ссылка в профиле** → `/profile/preferences`

## Как использовать

### Для нового пользователя:

1. Регистрация → автоматический редирект на onboarding
2. Пользователь заполняет опрос (или пропускает)
3. Попадает на dashboard с персонализированными рекомендациями

### Для существующего пользователя:

1. Переход в Профиль
2. Клик на "Настройки предпочтений"
3. Редактирование любых настроек
4. Сохранение → обновление рекомендаций

## Рекомендательная система

Использует предпочтения для фильтрации и ранжирования:

**Карточки (FlashcardSets)**
- Фильтр по интересующим/слабым предметам
- Приоритет для предметов из целей
- Учёт предпочитаемой сложности

**Квизы (Quizzes)**
- Фильтр по предметам
- Фильтр по сложности
- Исключение уже пройденных

**Экзамены (Exams)**
- Фильтр по целевому университету
- Фильтр по типу экзамена
- Приоритет для карьерных целей

**Университеты (Universities)**
- Фильтр по стране/городу
- Фильтр по бюджету
- Учёт грантов/стипендий
- Учёт языка обучения

## Преимущества

✅ **Персонализация** - каждый пользователь получает свои рекомендации
✅ **Гибкость** - можно пропустить или настроить позже
✅ **UX** - красивый onboarding без перегрузки
✅ **Масштабируемость** - легко добавить новые параметры
✅ **Валидация** - проверка обязательных полей

## База данных

Миграция: `AddExtendedUserPreferences`

Добавлены поля:
- PreferredCountry, PreferredCity, WillingToRelocate
- MaxBudgetPerYear, InterestedInScholarships
- PreferredLanguagesJson, EnglishLevel

## Тестирование

1. Запустите backend: `dotnet run`
2. Запустите frontend: `npm run dev`
3. Зарегистрируйте нового пользователя
4. Пройдите onboarding или пропустите
5. Проверьте рекомендации на dashboard
6. Откройте профиль → настройки предпочтений
7. Измените параметры и проверьте обновление рекомендаций

## Дальнейшие улучшения

- [ ] Email напоминания на основе расписания
- [ ] ML модель для предсказания интересов
- [ ] A/B тестирование алгоритмов рекомендаций
- [ ] Аналитика эффективности рекомендаций
- [ ] Экспорт предпочтений
