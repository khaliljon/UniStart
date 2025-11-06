using UniStart.Models;

namespace UniStart.Services
{
    /// <summary>
    /// Сервис для реализации алгоритма интервального повторения SM-2 (SuperMemo 2)
    /// </summary>
    public interface ISpacedRepetitionService
    {
        /// <summary>
        /// Обновляет параметры карточки после повторения
        /// </summary>
        /// <param name="flashcard">Карточка для обновления</param>
        /// <param name="quality">Качество ответа (0-5)</param>
        void UpdateFlashcard(Flashcard flashcard, int quality);
        
        /// <summary>
        /// Проверяет, нужно ли повторять карточку
        /// </summary>
        bool IsDueForReview(Flashcard flashcard);
    }

    public class SpacedRepetitionService : ISpacedRepetitionService
    {
        /// <summary>
        /// Алгоритм SM-2 для интервального повторения
        /// Quality scale:
        /// 5 - идеальный ответ
        /// 4 - правильный ответ после колебаний
        /// 3 - правильный ответ с трудом
        /// 2 - неправильный ответ, но вспомнилось при показе
        /// 1 - неправильный ответ, показалось знакомым
        /// 0 - полное незнание
        /// </summary>
        public void UpdateFlashcard(Flashcard flashcard, int quality)
        {
            // Валидация качества ответа
            if (quality < 0 || quality > 5)
            {
                throw new ArgumentException("Quality must be between 0 and 5", nameof(quality));
            }

            flashcard.LastReviewedAt = DateTime.UtcNow;

            // Если качество < 3, сбрасываем прогресс
            if (quality < 3)
            {
                flashcard.Repetitions = 0;
                flashcard.Interval = 0;
                flashcard.NextReviewDate = DateTime.UtcNow; // Повторить сразу
            }
            else
            {
                // Увеличиваем счетчик успешных повторений
                flashcard.Repetitions++;

                // Вычисляем новый интервал
                if (flashcard.Repetitions == 1)
                {
                    flashcard.Interval = 1; // 1 день
                }
                else if (flashcard.Repetitions == 2)
                {
                    flashcard.Interval = 6; // 6 дней
                }
                else
                {
                    // Для последующих повторений умножаем на EaseFactor
                    flashcard.Interval = (int)Math.Ceiling(flashcard.Interval * flashcard.EaseFactor);
                }

                // Устанавливаем дату следующего повторения
                flashcard.NextReviewDate = DateTime.UtcNow.AddDays(flashcard.Interval);
            }

            // Обновляем EaseFactor на основе качества ответа
            // EF' = EF + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02))
            double newEaseFactor = flashcard.EaseFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));
            
            // EaseFactor не должен быть меньше 1.3
            flashcard.EaseFactor = Math.Max(1.3, newEaseFactor);
        }

        /// <summary>
        /// Проверяет, нужно ли повторять карточку
        /// </summary>
        public bool IsDueForReview(Flashcard flashcard)
        {
            // Если карточка никогда не повторялась
            if (flashcard.NextReviewDate == null)
            {
                return true;
            }

            // Если дата повторения наступила или прошла
            return flashcard.NextReviewDate <= DateTime.UtcNow;
        }
    }
}
