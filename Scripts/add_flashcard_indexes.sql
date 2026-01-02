-- Оптимизация индексов для системы интервального обучения
-- Создано: 2026-01-02

-- Удаляем устаревший индекс на Flashcard.NextReviewDate (если существует)
DROP INDEX IF EXISTS "IX_Flashcards_NextReviewDate";

-- Создаем индекс для быстрого поиска прогресса пользователя по карточке
CREATE UNIQUE INDEX IF NOT EXISTS "IX_UserFlashcardProgresses_UserId_FlashcardId"
    ON "UserFlashcardProgresses" ("UserId", "FlashcardId");

-- Создаем индекс для получения карточек к повторению (дата истекла)
CREATE INDEX IF NOT EXISTS "IX_UserFlashcardProgresses_UserId_NextReviewDate"
    ON "UserFlashcardProgresses" ("UserId", "NextReviewDate");

-- Создаем индекс для подсчета освоенных карточек
CREATE INDEX IF NOT EXISTS "IX_UserFlashcardProgresses_UserId_IsMastered"
    ON "UserFlashcardProgresses" ("UserId", "IsMastered");

-- Создаем индекс для доступа к наборам карточек
CREATE UNIQUE INDEX IF NOT EXISTS "IX_UserFlashcardSetAccesses_UserId_FlashcardSetId"
    ON "UserFlashcardSetAccesses" ("UserId", "FlashcardSetId");

-- Создаем индекс для поиска завершенных наборов
CREATE INDEX IF NOT EXISTS "IX_UserFlashcardSetAccesses_UserId_IsCompleted"
    ON "UserFlashcardSetAccesses" ("UserId", "IsCompleted");

-- Проверка созданных индексов
SELECT 
    schemaname,
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename IN ('UserFlashcardProgresses', 'UserFlashcardSetAccesses')
ORDER BY tablename, indexname;
