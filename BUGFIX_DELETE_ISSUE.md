# Исправление ошибки 500 при удалении квизов и экзаменов

## Проблема

При попытке удаления квизов или экзаменов возникала ошибка 500 (Internal Server Error) из-за нарушения ограничений внешних ключей в базе данных.

## Причина

### Конфликт каскадного удаления

В `ApplicationDbContext.cs` были настроены следующие отношения:

1. **Quiz → UserQuizAttempt** (`DeleteBehavior.Cascade`)
2. **UserQuizAttempt → UserQuizAnswer** (`DeleteBehavior.Cascade`)
3. **UserQuizAnswer → QuizQuestion** (`DeleteBehavior.Restrict`)
4. **UserQuizAnswer → QuizAnswer** (`DeleteBehavior.Restrict`)

Проблема возникала из-за того, что:
- При удалении Quiz EF пытался каскадно удалить QuizQuestion и QuizAnswer
- Но эти сущности не могли быть удалены из-за `DeleteBehavior.Restrict` на связях с `UserQuizAnswer`
- В методе `DeleteQuiz` не удалялись записи `UserQuizAnswer` перед удалением вопросов и ответов

Аналогичная проблема существовала и для экзаменов.

## Решение

### 1. Исправлен `QuizzesController.DeleteQuiz` (строки 275-312)

**До:**
```csharp
// Удаляем связанные вопросы и ответы
var questions = await _context.QuizQuestions.Where(q => q.QuizId == id).ToListAsync();
foreach (var question in questions)
{
    var answers = await _context.QuizAnswers.Where(a => a.QuestionId == question.Id).ToListAsync();
    _context.QuizAnswers.RemoveRange(answers);
}
_context.QuizQuestions.RemoveRange(questions);

// Удаляем попытки прохождения
var attempts = await _context.UserQuizAttempts.Where(a => a.QuizId == id).ToListAsync();
_context.UserQuizAttempts.RemoveRange(attempts);

// Удаляем сам квиз
_context.Quizzes.Remove(quiz);
```

**После:**
```csharp
// Получаем все попытки и их ответы для удаления
var attempts = await _context.UserQuizAttempts
    .Where(a => a.QuizId == id)
    .ToListAsync();

// ВАЖНО: Удаляем сначала UserQuizAnswer (из-за DeleteBehavior.Restrict на Question и SelectedAnswer)
var attemptIds = attempts.Select(a => a.Id).ToList();
var userQuizAnswers = await _context.UserQuizAnswers
    .Where(ua => attemptIds.Contains(ua.AttemptId))
    .ToListAsync();
_context.UserQuizAnswers.RemoveRange(userQuizAnswers);

// Теперь удаляем попытки
_context.UserQuizAttempts.RemoveRange(attempts);

// Удаляем связанные вопросы и ответы
var questions = await _context.QuizQuestions.Where(q => q.QuizId == id).ToListAsync();
foreach (var question in questions)
{
    var answers = await _context.QuizAnswers.Where(a => a.QuestionId == question.Id).ToListAsync();
    _context.QuizAnswers.RemoveRange(answers);
}
_context.QuizQuestions.RemoveRange(questions);

// Удаляем сам квиз
_context.Quizzes.Remove(quiz);
```

### 2. Исправлен `ExamsController.DeleteExam` (строки 924-946)

Аналогичное исправление для экзаменов - добавлено явное удаление `UserExamAnswer` перед удалением вопросов и ответов.

### 3. Улучшен `ApplicationDbContext.cs`

Добавлены явные настройки для `UserExamAnswer`:

```csharp
modelBuilder.Entity<UserExamAnswer>()
    .HasOne(uea => uea.Question)
    .WithMany()
    .HasForeignKey(uea => uea.QuestionId)
    .OnDelete(DeleteBehavior.Restrict); // Не удаляем вопрос, если есть ответы

modelBuilder.Entity<UserExamAnswer>()
    .HasOne(uea => uea.SelectedAnswer)
    .WithMany()
    .HasForeignKey(uea => uea.SelectedAnswerId)
    .OnDelete(DeleteBehavior.Restrict); // Не удаляем ответ, если есть выбор пользователя
```

### 4. Создана миграция

Миграция `FixUserAnswerDeleteBehavior` изменяет FK для `UserExamAnswer`, устанавливая `ON DELETE RESTRICT` для связей с вопросами и ответами.

## Порядок удаления

Правильный порядок удаления сущностей:

1. **UserQuizAnswer** / **UserExamAnswer** (ответы пользователей)
2. **UserQuizAttempt** / **UserExamAttempt** (попытки прохождения)
3. **QuizAnswer** / **ExamAnswer** (варианты ответов)
4. **QuizQuestion** / **ExamQuestion** (вопросы)
5. **Quiz** / **Exam** (сам квиз/экзамен)

## Дата исправления

12 декабря 2025

## Затронутые файлы

- `Controllers/QuizzesController.cs`
- `Controllers/ExamsController.cs`
- `Data/ApplicationDbContext.cs`
- `Migrations/20251212182255_FixUserAnswerDeleteBehavior.cs`


