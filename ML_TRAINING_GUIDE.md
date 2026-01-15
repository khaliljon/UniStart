# ML Model Training Guide - Руководство по обучению ML модели

## Как работает автоматическое обучение

### Архитектура системы

```
Пользователь → Флешкарты → История взаимодействий → ML Model → Предсказания
                                    ↓
                            Тренировочные данные
                                    ↓
                            Автоматическое обучение
```

### Процесс обучения модели

1. **Сбор данных** - система собирает данные из истории повторений всех пользователей
2. **Анализ паттернов** - ML.NET FastTree анализирует паттерны обучения
3. **Обучение** - модель обучается предсказывать оптимальное время повторения
4. **Применение** - обученная модель используется для персонализированных рекомендаций

## Способы добавления тестовых данных

### 1. Через API (Рекомендуется для разработки)

#### Добавление данных вручную

```http
POST /api/mltraining/training-data
Authorization: Bearer <admin_token>
Content-Type: application/json

[
  {
    "userId": "user123",
    "flashcardId": 1,
    "easeFactor": 2.5,
    "interval": 1,
    "repetitions": 1,
    "daysSinceLastReview": 0.5,
    "userRetentionRate": 75.0,
    "userForgettingSpeed": 1.0,
    "correctAfterBreak": 70.0,
    "isMastered": false,
    "optimalReviewHours": 24
  }
]
```

#### Генерация синтетических данных

```http
POST /api/mltraining/generate-synthetic-data?count=500
Authorization: Bearer <admin_token>
```

Это создаст 500 реалистичных записей на основе существующих пользователей и флешкарт.

#### Импорт из CSV

```http
POST /api/mltraining/import-csv
Authorization: Bearer <admin_token>
Content-Type: multipart/form-data

file: training_data.csv
```

**Формат CSV** (шаблон в `/wwwroot/templates/training_data_template.csv`):

```csv
UserId,FlashcardId,EaseFactor,Interval,Repetitions,DaysSinceLastReview,UserRetentionRate,UserForgettingSpeed,CorrectAfterBreak,IsMastered,OptimalReviewHours
user123,1,2.5,1,1,0.5,75.0,1.0,70.0,false,24
```

### 2. Запуск переобучения модели

```http
POST /api/mltraining/retrain
Authorization: Bearer <admin_token>
```

**Требования:**
- Минимум 100 записей в базе данных
- Записи должны содержать разнообразные сценарии (разные пользователи, интервалы, уровни сложности)

### 3. Получение статистики

```http
GET /api/mltraining/training-stats
Authorization: Bearer <admin_token>
```

**Ответ:**
```json
{
  "totalRecords": 150,
  "recordsLast24Hours": 10,
  "recordsLast7Days": 50,
  "recordsLast30Days": 120,
  "canTrain": true,
  "isModelTrained": true,
  "lastTrainingDate": "2026-01-15T10:30:00Z",
  "uniqueUsers": 5,
  "uniqueFlashcards": 30,
  "averageEaseFactor": 2.35,
  "averageInterval": 4.2,
  "averageRetentionRate": 72.5
}
```

## Параметры тренировочных данных

| Параметр | Описание | Диапазон | Пример |
|----------|----------|----------|--------|
| `easeFactor` | Коэффициент сложности (SM-2 алгоритм) | 1.3 - 2.5 | 2.5 (легко), 1.3 (сложно) |
| `interval` | Интервал между повторениями (дни) | 0 - 365 | 1, 2, 4, 8, 16... |
| `repetitions` | Количество успешных повторений | 0 - 100 | 0 (новая), 5 (освоена) |
| `daysSinceLastReview` | Дней с последнего повторения | 0 - 365 | 0.5, 2.0, 7.0 |
| `userRetentionRate` | Процент запоминания пользователя | 0 - 100 | 75.0 (хорошо) |
| `userForgettingSpeed` | Скорость забывания | 0.1 - 5.0 | 1.0 (нормально), 2.0 (быстро забывает) |
| `correctAfterBreak` | % правильных ответов после перерыва | 0 - 100 | 70.0 |
| `isMastered` | Карточка освоена | true/false | false |
| `optimalReviewHours` | **Целевое значение** - оптимальное время для повторения | 1 - 8760 | 24 (1 день), 168 (1 неделя) |

## Сценарии использования

### Сценарий 1: Быстрый старт с синтетическими данными

```bash
# 1. Генерируем 500 записей
curl -X POST "https://yourapi.com/api/mltraining/generate-synthetic-data?count=500" \
  -H "Authorization: Bearer YOUR_TOKEN"

# 2. Обучаем модель
curl -X POST "https://yourapi.com/api/mltraining/retrain" \
  -H "Authorization: Bearer YOUR_TOKEN"

# 3. Проверяем статус
curl -X GET "https://yourapi.com/api/mltraining/training-stats" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Сценарий 2: Импорт реальных данных

1. Подготовьте CSV файл с реальными данными из другой системы
2. Убедитесь, что UserId и FlashcardId существуют в базе
3. Импортируйте через API endpoint `/api/mltraining/import-csv`
4. Запустите переобучение

### Сценарий 3: Постепенное обучение

Модель **автоматически** улучшается по мере использования:
- Каждый раз, когда пользователь повторяет карточку, данные сохраняются
- Периодически запускайте `/api/mltraining/retrain` (например, раз в неделю)
- Модель будет адаптироваться к реальным паттернам пользователей

## Автоматическое переобучение

### Вариант 1: Background Service (рекомендуется)

Добавьте в `Program.cs`:

```csharp
builder.Services.AddHostedService<MLRetrainingBackgroundService>();
```

Создайте сервис, который будет переобучать модель каждую ночь:

```csharp
public class MLRetrainingBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Обучаем модель каждую ночь в 2:00
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            // Вызов RetrainModelAsync()
        }
    }
}
```

### Вариант 2: Планировщик задач (Hangfire/Quartz)

```csharp
// Переобучение каждое воскресенье в 3:00
RecurringJob.AddOrUpdate(
    "retrain-ml-model",
    () => mlService.RetrainModelAsync(),
    Cron.Weekly(DayOfWeek.Sunday, 3));
```

## Мониторинг качества модели

### Метрики для отслеживания

1. **Количество тренировочных данных** - чем больше, тем лучше
2. **Разнообразие пользователей** - минимум 10 разных пользователей
3. **Покрытие интервалов** - данные для коротких (1 день) и длинных (30+ дней) интервалов
4. **Последняя дата обучения** - модель должна обновляться регулярно

### Пример dashboard запроса

```typescript
const stats = await fetch('/api/mltraining/training-stats');
console.log(`Модель обучена: ${stats.isModelTrained}`);
console.log(`Всего записей: ${stats.totalRecords}`);
console.log(`Можно обучить: ${stats.canTrain ? 'Да' : 'Нет (нужно >= 100 записей)'}`);
```

## Troubleshooting

### Проблема: "Недостаточно данных для обучения"

**Решение:**
```bash
# Сгенерируйте синтетические данные
POST /api/mltraining/generate-synthetic-data?count=200
```

### Проблема: "Модель не загружается"

**Причины:**
1. Файл `Models/flashcard_review_model.zip` отсутствует
2. Модель устарела (несовместимая версия ML.NET)

**Решение:**
```bash
# Переобучите модель заново
POST /api/mltraining/retrain
```

### Проблема: "Низкое качество предсказаний"

**Решения:**
1. Добавьте больше разнообразных данных (разные пользователи, карточки, интервалы)
2. Убедитесь, что `OptimalReviewHours` в тренировочных данных реалистичны
3. Переобучите модель на свежих данных

## Best Practices

1. ✅ **Начните с синтетических данных** - быстрый старт для тестирования
2. ✅ **Регулярно переобучайте** - минимум раз в неделю
3. ✅ **Мониторьте статистику** - следите за количеством и качеством данных
4. ✅ **Валидация данных** - проверяйте корректность перед импортом
5. ✅ **Backup модели** - сохраняйте предыдущие версии перед обучением
6. ❌ **Не перегружайте** - слишком частое переобучение (каждый час) не улучшит качество
7. ❌ **Не используйте слишком мало данных** - минимум 100, оптимально 500+ записей

## Интеграция с фронтендом

Пример React компонента для управления обучением:

```typescript
const MLTrainingPanel = () => {
  const [stats, setStats] = useState<TrainingStats>();
  
  const loadStats = async () => {
    const response = await fetch('/api/mltraining/training-stats');
    setStats(await response.json());
  };
  
  const generateData = async () => {
    await fetch('/api/mltraining/generate-synthetic-data?count=500', {
      method: 'POST'
    });
    await loadStats();
  };
  
  const retrain = async () => {
    await fetch('/api/mltraining/retrain', { method: 'POST' });
    await loadStats();
  };
  
  return (
    <div>
      <h3>ML Model Status</h3>
      <p>Records: {stats?.totalRecords}</p>
      <p>Can train: {stats?.canTrain ? 'Yes' : 'No'}</p>
      <button onClick={generateData}>Generate Test Data</button>
      <button onClick={retrain} disabled={!stats?.canTrain}>
        Retrain Model
      </button>
    </div>
  );
};
```
