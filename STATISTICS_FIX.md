# Исправление системы статистики карточек

## Дата: 12 декабря 2025

## Проблема

Пользователь обнаружил, что статистика по наборам карточек работает некорректно:
1. Завершенные наборы не подсчитывались (всегда 0)
2. Статистика набора показывала "липовые данные" (локальные вычисления вместо реальных из БД)
3. `UserFlashcardSetAccess` не создавался при review карточек, если набор не был открыт через GET `/flashcards/sets/{id}`
4. Страница статистики набора (`FlashcardStatsPage`) получала данные из неправильного эндпоинта
5. Детальная страница студента не показывала статистику по карточкам

## Корневые причины

### 1. Отсутствие создания `UserFlashcardSetAccess` при review

В `FlashcardsController.ReviewFlashcard` (строка 577) была проверка `if (setAccess != null)`, но если пользователь как-то review карточку без открытия набора (например, напрямую через API), то `setAccess` был null и статистика не обновлялась.

```csharp
// ДО ИСПРАВЛЕНИЯ:
var setAccess = await _context.UserFlashcardSetAccesses
    .FirstOrDefaultAsync(a => a.UserId == userId && a.FlashcardSetId == card.FlashcardSetId);

if (setAccess != null) // <- Проблема: если null, статистика не обновляется
{
    // ... обновление статистики
}
```

### 2. Неправильный эндпоинт в `FlashcardStatsPage`

Страница статистики набора (`FlashcardStatsPage.tsx`) использовала `flashcardService.getSet(id)` вместо `flashcardService.getSetStats(id)`, получая просто набор карточек, а не реальную статистику из БД.

```typescript
// ДО ИСПРАВЛЕНИЯ:
const data = await flashcardService.getSet(Number(id)); // Неправильно!
// Затем вычисляла статистику локально из данных карточек
```

### 3. Отсутствие метода `getSetStats` в `flashcardService`

В `flashcardService.ts` не было метода для получения статистики набора, хотя на бэкенде эндпоинт `/flashcards/sets/{id}/stats` существовал.

### 4. Устаревшие поля в `StudentDetailPage`

Страница детального просмотра студента использовала устаревшее поле `totalCardsStudied` вместо новых `completedFlashcardSets`, `reviewedCards`, `masteredCards` и не показывала детальный `FlashcardProgress`.

## Решение

### Backend

#### 1. FlashcardsController.cs - Автоматическое создание `UserFlashcardSetAccess`

**Файл:** `Controllers\FlashcardsController.cs`  
**Строки:** 570-621

**Изменение:** Добавлена логика создания `UserFlashcardSetAccess`, если его не существует, при review карточки.

```csharp
// ПОСЛЕ ИСПРАВЛЕНИЯ:
var setAccess = await _context.UserFlashcardSetAccesses
    .FirstOrDefaultAsync(a => a.UserId == userId && a.FlashcardSetId == card.FlashcardSetId);

var now = DateTime.UtcNow;

// ИСПРАВЛЕНИЕ: Создаем UserFlashcardSetAccess если его нет
if (setAccess == null)
{
    var totalCardsInSet = await _context.Flashcards
        .Where(f => f.FlashcardSetId == card.FlashcardSetId)
        .CountAsync();
    
    setAccess = new UserFlashcardSetAccess
    {
        UserId = userId,
        FlashcardSetId = card.FlashcardSetId,
        FirstAccessedAt = now,
        LastAccessedAt = now,
        AccessCount = 1,
        TotalCardsCount = totalCardsInSet,
        CardsStudiedCount = 0,
        IsCompleted = false,
        CreatedAt = now
    };
    _context.UserFlashcardSetAccesses.Add(setAccess);
    
    _logger.LogInformation("Создан UserFlashcardSetAccess для userId={UserId}, setId={SetId} при review карточки", 
        userId, card.FlashcardSetId);
}

// Обновляем количество изученных карточек
var masteredCardsCount = await _context.UserFlashcardProgresses
    .Where(p => p.UserId == userId 
        && p.Flashcard.FlashcardSetId == card.FlashcardSetId 
        && p.IsMastered)
    .CountAsync();

setAccess.CardsStudiedCount = masteredCardsCount;
setAccess.LastAccessedAt = now;
setAccess.UpdatedAt = now;

// Проверяем, полностью ли изучен набор
var totalCardsInSet2 = await _context.Flashcards
    .Where(f => f.FlashcardSetId == card.FlashcardSetId)
    .CountAsync();

setAccess.TotalCardsCount = totalCardsInSet2; // Обновляем на случай изменения количества карточек

if (masteredCardsCount >= totalCardsInSet2 && totalCardsInSet2 > 0 && !setAccess.IsCompleted)
{
    setAccess.IsCompleted = true;
    setAccess.CompletedAt = now;
    
    _logger.LogInformation("Набор карточек завершен! userId={UserId}, setId={SetId}, masteredCards={Mastered}/{Total}", 
        userId, card.FlashcardSetId, masteredCardsCount, totalCardsInSet2);
}
```

**Эффект:** 
- Теперь `UserFlashcardSetAccess` создается автоматически при первом review карточки, даже если пользователь не открывал набор явно
- Статистика набора всегда актуальна
- `IsCompleted` устанавливается автоматически, когда все карточки освоены

#### 2. StudentController.cs - Дополнена статистика по карточкам

**Файл:** `Controllers\StudentController.cs`  
**Метод:** `GetProgress`  
**Строки:** 146-173

**Изменение:** Добавлены поля `masteredCards` и `completedFlashcardSets` в ответ.

```csharp
// Статистика по карточкам (ДОПОЛНЕНО)
var reviewedCards = user.TotalCardsStudied; // Обновляется в FlashcardsController.ReviewFlashcard
var masteredCards = await _context.UserFlashcardProgresses
    .Where(p => p.UserId == userId && p.IsMastered)
    .CountAsync();
var completedFlashcardSets = await _context.UserFlashcardSetAccesses
    .Where(a => a.UserId == userId && a.IsCompleted)
    .CountAsync();

return Ok(new
{
    stats = new
    {
        totalCardsStudied = reviewedCards, // Просмотрено карточек
        masteredCards, // Освоено карточек
        completedFlashcardSets, // Завершено наборов
        totalQuizzesTaken,
        averageQuizScore = Math.Round(averageQuizScore, 1),
        totalTimeSpent,
        currentStreak,
        longestStreak,
        totalAchievements
    },
    recentActivity,
    subjectProgress
});
```

### Frontend

#### 1. flashcardService.ts - Добавлен метод `getSetStats`

**Файл:** `unistart-frontend\src\services\flashcardService.ts`  
**Строки:** 37-40

```typescript
async getSetStats(id: number): Promise<any> {
  const { data } = await api.get(`/flashcards/sets/${id}/stats`);
  return data;
},
```

#### 2. FlashcardStatsPage.tsx - Использование правильного эндпоинта

**Файл:** `unistart-frontend\src\pages\FlashcardStatsPage.tsx`

**Изменения:**
1. Добавлен интерфейс `FlashcardSetStats` для типизации данных
2. Изменен вызов API с `getSet` на `getSetStats`
3. Обновлено отображение статистики (реальные данные из БД)

```typescript
interface FlashcardSetStats {
  id: number;
  title: string;
  description: string;
  subject: string;
  isPublic: boolean;
  createdAt: string;
  updatedAt: string;
  totalCards: number;
  uniqueStudents: number; // Изучающих студентов
  cardsToReview: number; // Карточек к повторению для владельца
  averageProgress: number; // Средний процент завершенных наборов
  totalMasteredCards: number; // Уникальных карточек освоено
  completedSetsCount: number; // Количество пользователей, завершивших набор
}

const loadStats = async () => {
  try {
    const data = await flashcardService.getSetStats(Number(id));
    console.log('📊 Статистика набора:', data);
    setStats(data);
    setError(null);
  } catch (error) {
    console.error('Ошибка загрузки статистики:', error);
    setError('Не удалось загрузить статистику');
  } finally {
    setLoading(false);
  }
};
```

**Обновленное отображение:**
- "Всего карточек": `stats.totalCards`
- "Изучающих": `stats.uniqueStudents` (реальное количество)
- "К повторению (мне)": `stats.cardsToReview`
- "Полностью изучили": `stats.completedSetsCount`
- Секция "Активность" теперь показывает:
  - Освоенных карточек: `stats.totalMasteredCards` из `stats.totalCards`
  - Завершили набор: `stats.completedSetsCount` из `stats.uniqueStudents`
  - Средний прогресс: `stats.averageProgress%`

#### 3. StudentDetailPage.tsx - Детальная статистика по карточкам

**Файл:** `unistart-frontend\src\pages\StudentDetailPage.tsx`

**Изменения:**
1. Использование типа `StudentDetailedStats` вместо локального интерфейса
2. Обновлено отображение карточек в общей статистике
3. Добавлена отдельная карточка "Статистика по карточкам"
4. Добавлена новая секция "Прогресс по наборам карточек" с детализацией по каждому набору

```typescript
// Общая статистика - первая карточка
<Card className="p-6">
  <div className="flex items-center justify-between">
    <div>
      <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">Карточек освоено</p>
      <p className="text-3xl font-bold text-gray-900 dark:text-white">
        {student.masteredCards || 0}
      </p>
      <p className="text-xs text-gray-500 dark:text-gray-500 mt-1">
        Просмотрено: {student.reviewedCards || 0}
      </p>
    </div>
    <div className="bg-green-500 p-4 rounded-lg">
      <CheckCircle className="w-8 h-8 text-white" />
    </div>
  </div>
</Card>

// Детальная статистика - третья карточка
<Card className="p-6">
  <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Статистика по карточкам</h3>
  <div className="space-y-3">
    <div className="flex justify-between">
      <span className="text-gray-600 dark:text-gray-400">Завершено наборов:</span>
      <span className="font-medium text-gray-900 dark:text-white">{student.completedFlashcardSets || 0}</span>
    </div>
    <div className="flex justify-between">
      <span className="text-gray-600 dark:text-gray-400">Просмотрено карточек:</span>
      <span className="font-medium text-gray-900 dark:text-white">{student.reviewedCards || 0}</span>
    </div>
    <div className="flex justify-between">
      <span className="text-gray-600 dark:text-gray-400">Освоено карточек:</span>
      <span className="font-medium text-green-600 dark:text-green-400">{student.masteredCards || 0}</span>
    </div>
    <div className="flex justify-between">
      <span className="text-gray-600 dark:text-gray-400">Процент освоения:</span>
      <span className="font-medium text-gray-900 dark:text-white">
        {student.reviewedCards > 0 
          ? ((student.masteredCards / student.reviewedCards) * 100).toFixed(1)
          : 0}%
      </span>
    </div>
  </div>
</Card>
```

**Новая секция "Прогресс по наборам карточек":**
- Общая статистика: наборов открыто, завершено, карточек просмотрено, освоено
- Детализация по каждому набору:
  - Название набора и статус (завершен/не завершен)
  - Последний доступ
  - Количество карточек: всего, просмотрено, освоено
  - Прогресс-бары: просмотр и освоение

#### 4. StudentProgressPage.tsx - Обновлена статистика

**Файл:** `unistart-frontend\src\pages\StudentProgressPage.tsx`

**Изменения:**
1. Добавлены поля `masteredCards` и `completedFlashcardSets` в интерфейс `ProgressStats`
2. Обновлено отображение карточки "Изучено карточек" → "Просмотрено карточек" с дополнительной информацией об освоенных

```typescript
interface ProgressStats {
  totalCardsStudied: number; // ReviewedCards
  masteredCards: number; // НОВОЕ: освоенные карточки
  completedFlashcardSets: number; // НОВОЕ: завершенные наборы
  totalQuizzesTaken: number;
  averageQuizScore: number;
  totalTimeSpent: number;
  currentStreak: number;
  longestStreak: number;
  totalAchievements: number;
}

// Отображение:
<Card className="p-6 bg-gradient-to-br from-blue-500 to-blue-600 text-white">
  <div className="flex items-center justify-between">
    <div>
      <p className="text-blue-100 text-sm mb-1">Просмотрено карточек</p>
      <p className="text-3xl font-bold">{stats.totalCardsStudied}</p>
      <p className="text-xs text-blue-200 mt-1">Освоено: {stats.masteredCards}</p>
    </div>
    <BookOpen className="w-12 h-12 text-blue-200" />
  </div>
</Card>
```

#### 5. types/index.ts - Обновлены интерфейсы

**Файл:** `unistart-frontend\src\types\index.ts`

**Изменения:**
1. Исправлено `FlashcardProgress.totalCardsMastered` → `masteredCards`
2. Исправлено `FlashcardProgress.setDetails: FlashcardSetDetail[]` → `FlashcardSetProgressDetail[]`
3. Добавлен новый интерфейс `FlashcardSetProgressDetail`
4. Дополнен `StudentDetailedStats` полями `completedFlashcardSets`, `reviewedCards`, `masteredCards`

```typescript
// Детализированный прогресс по набору карточек (для админа/учителя)
export interface FlashcardSetProgressDetail {
  setId: number;
  setTitle: string;
  totalCards: number;
  reviewedCards: number; // Просмотрено хотя бы раз
  masteredCards: number; // Полностью освоено
  isCompleted: boolean;
  lastAccessed: string;
}

export interface FlashcardProgress {
  setsAccessed: number;
  setsCompleted: number;
  totalCardsReviewed: number;
  masteredCards: number; // ИСПРАВЛЕНО: было totalCardsMastered
  setDetails: FlashcardSetProgressDetail[]; // ИСПРАВЛЕНО: было FlashcardSetDetail
}

export interface StudentDetailedStats extends StudentStats {
  // ... existing fields ...
  
  // Детальная статистика по карточкам (ДОПОЛНЕНО)
  completedFlashcardSets: number;
  reviewedCards: number;
  masteredCards: number;
  flashcardProgress?: FlashcardProgress;
  
  // Общая статистика
  averageScore: number;
}
```

## Терминология

### Четкое разделение понятий:

1. **ReviewedCards (Просмотренные карточки)** - количество карточек, которые пользователь просмотрел хотя бы раз (`UserFlashcardProgress.LastReviewedAt != null`)

2. **MasteredCards (Освоенные карточки)** - количество карточек, которые пользователь полностью освоил (`UserFlashcardProgress.IsMastered == true`)

3. **CompletedFlashcardSets (Завершенные наборы)** - количество наборов карточек, которые пользователь полностью изучил (все карточки в наборе освоены, `UserFlashcardSetAccess.IsCompleted == true`)

4. **UniqueStudents (Изучающих)** - количество уникальных пользователей, которые открыли набор хотя бы раз

5. **CardsStudiedCount** - количество освоенных карточек в конкретном наборе (для `UserFlashcardSetAccess`)

6. **AverageProgress** - средний процент студентов, завершивших набор полностью

## Тестирование

### Проверка функционала:

1. **Review карточки без открытия набора:**
   - POST `/flashcards/cards/review` с новой карточкой
   - Проверить, что создается `UserFlashcardSetAccess`
   - Проверить логи: "Создан UserFlashcardSetAccess для userId=..."

2. **Завершение набора:**
   - Review всех карточек набора с quality >= 4
   - Проверить, что `UserFlashcardSetAccess.IsCompleted = true`
   - Проверить логи: "Набор карточек завершен! userId=..."

3. **Статистика набора:**
   - Открыть страницу статистики набора (GET `/flashcards/sets/{id}/stats` + UI)
   - Проверить, что показываются реальные данные:
     - Изучающих > 0
     - Полностью изучили ≥ 0
     - Средний прогресс корректен

4. **Детальная страница студента:**
   - Открыть детальную страницу студента (админ/учитель)
   - Проверить наличие секции "Прогресс по наборам карточек"
   - Проверить корректность данных по каждому набору

5. **Страница прогресса студента:**
   - Открыть `/student/progress`
   - Проверить, что показываются `masteredCards` и `completedFlashcardSets`

## Влияние на производительность

### Оптимизация запросов:

1. В `ReviewFlashcard` добавлено 2 дополнительных запроса при создании нового `UserFlashcardSetAccess` (однократно)
2. В `StudentController.GetProgress` добавлено 2 дополнительных запроса (всегда)

**Рекомендация:** В будущем можно кэшировать статистику или денормализовать данные для быстрого доступа.

## Связанные файлы

### Backend:
- `Controllers\FlashcardsController.cs` (ReviewFlashcard)
- `Controllers\StudentController.cs` (GetProgress)

### Frontend:
- `unistart-frontend\src\services\flashcardService.ts`
- `unistart-frontend\src\pages\FlashcardStatsPage.tsx`
- `unistart-frontend\src\pages\StudentDetailPage.tsx`
- `unistart-frontend\src\pages\StudentProgressPage.tsx`
- `unistart-frontend\src\types\index.ts`

## Выводы

Исправления обеспечивают:
1. ✅ Автоматическое создание и обновление статистики при review карточек
2. ✅ Правильное отображение статистики набора с реальными данными из БД
3. ✅ Детальную статистику по карточкам для админа/учителя
4. ✅ Четкую терминологию и разделение понятий
5. ✅ Улучшенный UX для студентов с визуализацией прогресса

**Статус:** ✅ Завершено и протестировано


