# Анализ системы статистики по карточкам

## Как работает система статистики

### 1. Основные модели данных

#### `UserFlashcardProgress` (прогресс по одной карточке)
- **`LastReviewedAt`** - дата последнего просмотра карточки (обновляется ВСЕГДА при review)
- **`IsMastered`** - освоена ли карточка (`Repetitions >= 3 && EaseFactor >= 2.0`)
- **`Repetitions`** - количество успешных повторений (quality >= 3)
- **`EaseFactor`** - коэффициент легкости (SM-2 алгоритм)
- **`FirstReviewedAt`** - дата первого просмотра

#### `UserFlashcardSetAccess` (прогресс по набору)
- **`CardsStudiedCount`** - количество освоенных карточек (`IsMastered = true`) в наборе
- **`TotalCardsCount`** - общее количество карточек в наборе
- **`IsCompleted`** - полностью ли изучен набор (`CardsStudiedCount >= TotalCardsCount`)
- **`AccessCount`** - сколько раз открывали набор
- **`LastAccessedAt`** - последний доступ к набору

#### `ApplicationUser` (общая статистика пользователя)
- **`TotalCardsStudied`** - общее количество просмотренных карточек (`LastReviewedAt != null`)

---

## 2. Процесс обновления статистики при просмотре карточки

### Шаг 1: Вызов `ReviewFlashcard` (FlashcardsController.cs:538)
```csharp
[HttpPost("cards/review")]
public async Task<ActionResult<ReviewResultDto>> ReviewFlashcard(ReviewFlashcardDto dto)
```

### Шаг 2: Получение/создание `UserFlashcardProgress` (строки 554-568)
- Если прогресса нет → создается новая запись с `EaseFactor = 2.5`
- Если есть → используется существующая

### Шаг 3: Обновление прогресса через SM-2 (строка 571)
```csharp
_spacedRepetitionService.UpdateUserFlashcardProgress(progress, dto.Quality);
```

**Что происходит в `SpacedRepetitionService.UpdateUserFlashcardProgress`:**
1. **ВСЕГДА** обновляется `LastReviewedAt = DateTime.UtcNow` (строка 123)
2. Увеличивается `TotalReviews++` (строка 124)
3. Если `quality >= 3` → увеличивается `CorrectReviews++` (строка 130)
4. Обновляется `EaseFactor` по формуле SM-2 (строка 135)
5. Если `quality < 3` → сбрасывается прогресс (`Repetitions = 0`, `IsMastered = false`)
6. Если `quality >= 3` → увеличивается `Repetitions++` и пересчитывается `IsMastered` (строка 175)

### Шаг 4: Обновление статистики набора (строки 574-630)

**4.1. Создание/получение `UserFlashcardSetAccess`** (строки 574-603)
- Если нет → создается с `CardsStudiedCount = 0`

**4.2. Подсчет освоенных карточек в наборе** (строки 606-612)
```csharp
var masteredCardsCount = await _context.UserFlashcardProgresses
    .Where(p => p.UserId == userId 
        && p.Flashcard.FlashcardSetId == card.FlashcardSetId 
        && p.IsMastered)
    .CountAsync();

setAccess.CardsStudiedCount = masteredCardsCount;
```

**ПРОБЛЕМА 1:** Каждый раз при review делается запрос к БД для подсчета `masteredCardsCount` - это неэффективно!

**4.3. Проверка завершения набора** (строки 623-630)
- Если `masteredCardsCount >= totalCardsInSet2` → `IsCompleted = true`

### Шаг 5: Обновление общей статистики пользователя (строки 632-640)
```csharp
user.TotalCardsStudied = await _context.UserFlashcardProgresses
    .Where(p => p.UserId == userId && p.LastReviewedAt != null)
    .CountAsync();
```

**ПРОБЛЕМА 2:** Каждый раз при review делается запрос к БД для подсчета `TotalCardsStudied` - это неэффективно!

---

## 3. Как считается статистика в контроллерах

### `StudentController.GetProgress` (строки 40-130)
```csharp
// ReviewedCards - из UserFlashcardProgress
var reviewedCardsCount = await _context.UserFlashcardProgresses
    .Where(p => p.UserId == userId && p.LastReviewedAt != null)
    .CountAsync();

// MasteredCards - из UserFlashcardProgress
var masteredCardsCount = await _context.UserFlashcardProgresses
    .Where(p => p.UserId == userId && p.IsMastered)
    .CountAsync();
```

### `TeacherController.GetStudentStats` (строки 373-500)
```csharp
// ReviewedCards - из UserFlashcardProgress
var reviewedCardsCount = await _context.UserFlashcardProgresses
    .Where(p => p.UserId == studentId && p.LastReviewedAt != null)
    .CountAsync();

// MasteredCards - из UserFlashcardProgress
var masteredCardsCount = await _context.UserFlashcardProgresses
    .Where(p => p.UserId == studentId && p.IsMastered)
    .CountAsync();

// Прогресс по наборам - из UserFlashcardSetAccess + UserFlashcardProgress
var flashcardProgressBySet = await _context.UserFlashcardProgresses
    .Where(p => p.UserId == studentId)
    .GroupBy(p => p.Flashcard.FlashcardSetId)
    .Select(g => new
    {
        SetId = g.Key,
        ReviewedCards = g.Count(p => p.LastReviewedAt != null),
        MasteredCards = g.Count(p => p.IsMastered)
    })
    .ToDictionaryAsync(x => x.SetId);
```

### `AdminController.GetUsers` (строки 119-125)
```csharp
var flashcardProgressDict = allFlashcardProgresses
    .GroupBy(p => p.UserId)
    .ToDictionary(
        g => g.Key,
        g => new
        {
            ReviewedCards = g.Count(p => p.LastReviewedAt != null),
            MasteredCards = g.Count(p => p.IsMastered),
            LastCardDate = g.Where(p => p.LastReviewedAt != null)
                .Select(p => (DateTime?)p.LastReviewedAt)
                .DefaultIfEmpty()
                .Max()
        });
```

---

## 4. Выявленные проблемы

### Проблема 1: Неэффективные запросы при каждом review
**Где:** `FlashcardsController.ReviewFlashcard` (строки 606-612, 637-639)

**Что происходит:**
- При каждом review карточки делается 2 дополнительных запроса к БД:
  1. Подсчет `masteredCardsCount` для набора
  2. Подсчет `TotalCardsStudied` для пользователя

**Почему это плохо:**
- Если пользователь просматривает 20 карточек подряд → 40 лишних запросов к БД
- Медленная работа при большом количестве карточек

**Решение:**
- Обновлять `setAccess.CardsStudiedCount` инкрементально (если карточка стала освоенной → +1)
- Обновлять `user.TotalCardsStudied` инкрементально (если карточка просмотрена впервые → +1)
- Или вообще убрать эти поля и считать динамически (как в контроллерах)

### Проблема 2: Дублирование данных
**Где:** `ApplicationUser.TotalCardsStudied` и `UserFlashcardSetAccess.CardsStudiedCount`

**Что происходит:**
- Эти поля дублируют данные, которые можно получить из `UserFlashcardProgress`
- При изменении данных в `UserFlashcardProgress` нужно вручную обновлять эти поля

**Почему это плохо:**
- Риск рассинхронизации данных
- Лишние запросы к БД

**Решение:**
- Убрать эти поля или сделать их вычисляемыми (computed properties)
- Использовать только `UserFlashcardProgress` как источник истины

### Проблема 3: Неправильное название поля
**Где:** `UserFlashcardSetAccess.CardsStudiedCount`

**Что происходит:**
- Название `CardsStudiedCount` подразумевает "просмотренные карточки"
- Но на самом деле там хранится количество **освоенных** карточек (`IsMastered = true`)

**Почему это плохо:**
- Путаница в коде
- Неправильное понимание логики

**Решение:**
- Переименовать в `MasteredCardsCount` или `CardsMasteredCount`
- Или добавить отдельное поле `ReviewedCardsCount`

### Проблема 4: Отсутствие индексов
**Где:** Запросы к `UserFlashcardProgress`

**Что происходит:**
- Частые запросы по `UserId`, `FlashcardId`, `FlashcardSetId`, `IsMastered`, `LastReviewedAt`
- Без индексов эти запросы могут быть медленными

**Решение:**
- Добавить индексы на часто используемые поля

---

## 5. Рекомендации по улучшению

### Краткосрочные (быстрые исправления):
1. ✅ Исправить логику `IsMastered` (уже сделано - снижено требование до `EaseFactor >= 2.0`)
2. ⚠️ Переименовать `CardsStudiedCount` → `MasteredCardsCount`
3. ⚠️ Оптимизировать запросы в `ReviewFlashcard` (инкрементальное обновление)

### Долгосрочные (рефакторинг):
1. Убрать `ApplicationUser.TotalCardsStudied` и считать динамически
2. Убрать `UserFlashcardSetAccess.CardsStudiedCount` и считать динамически
3. Добавить индексы на `UserFlashcardProgress`
4. Использовать кэширование для часто запрашиваемой статистики

---

## 6. Текущая логика подсчета (правильная)

### ReviewedCards (просмотренные карточки):
```csharp
Count(p => p.LastReviewedAt != null)
```
- Карточка считается просмотренной, если у нее есть `LastReviewedAt`
- Обновляется **ВСЕГДА** при review (даже если ответ неправильный)

### MasteredCards (освоенные карточки):
```csharp
Count(p => p.IsMastered)
```
- Карточка считается освоенной, если `Repetitions >= 3 && EaseFactor >= 2.0`
- Обновляется только при успешных ответах (quality >= 3)

### CardsStudiedCount в UserFlashcardSetAccess:
```csharp
Count(p => p.Flashcard.FlashcardSetId == setId && p.IsMastered)
```
- Количество освоенных карточек в конкретном наборе
- Обновляется при каждом review (неэффективно!)

---

## 7. Выводы

**Что работает правильно:**
- ✅ Логика обновления `LastReviewedAt` и `IsMastered` в `SpacedRepetitionService`
- ✅ Подсчет статистики в контроллерах (из `UserFlashcardProgress`)
- ✅ Проверка завершения набора (`IsCompleted`)

**Что нужно исправить:**
- ⚠️ Неэффективные запросы в `ReviewFlashcard`
- ⚠️ Дублирование данных (`TotalCardsStudied`, `CardsStudiedCount`)
- ⚠️ Неправильное название поля (`CardsStudiedCount`)

**Главная проблема:**
Система статистики работает правильно, но **неэффективно** - при каждом review делаются лишние запросы к БД для подсчета уже известных данных.

