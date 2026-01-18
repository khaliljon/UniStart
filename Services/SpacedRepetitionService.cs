using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

namespace UniStart.Services
{
    /// <summary>
    /// Сервис для реализации алгоритма интервального повторения SM-2 (SuperMemo 2)
    /// </summary>
    public interface ISpacedRepetitionService
    {
        /// <summary>
        /// Обновляет прогресс пользователя по карточке после повторения
        /// </summary>
        /// <param name="progress">Прогресс пользователя по карточке</param>
        /// <param name="quality">Качество ответа (0-5)</param>
        void UpdateUserFlashcardProgress(UserFlashcardProgress progress, int quality);
        
        /// <summary>
        /// Проверяет, нужно ли повторять карточку пользователю
        /// </summary>
        bool IsDueForReview(UserFlashcardProgress progress);
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
        public void UpdateUserFlashcardProgress(UserFlashcardProgress progress, int quality)
        {
            // Валидация качества ответа
            if (quality < 0 || quality > 5)
            {
                throw new ArgumentException("Quality must be between 0 and 5", nameof(quality));
            }

            var now = DateTime.UtcNow;

            // Устанавливаем дату первого изучения, если это первое повторение
            if (progress.FirstReviewedAt == null)
            {
                progress.FirstReviewedAt = now;
            }

            progress.LastReviewedAt = now;
            progress.TotalReviews++;
            progress.UpdatedAt = now;

            // Если качество >= 3, считаем правильным ответом
            if (quality >= 3)
            {
                progress.CorrectReviews++;
            }

            // ИСПРАВЛЕНО: Обновляем EaseFactor ПЕРЕД проверкой IsMastered
            // EF' = EF + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02))
            double newEaseFactor = progress.EaseFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));
            
            // EaseFactor не должен быть меньше 1.3
            progress.EaseFactor = Math.Max(1.3, newEaseFactor);

            // Если качество < 3, сбрасываем прогресс
            if (quality < 3)
            {
                progress.Repetitions = 0;
                progress.Interval = 0;
                progress.NextReviewDate = now; // Повторить сразу
                progress.IsMastered = false;
            }
            else
            {
                // Увеличиваем счетчик успешных повторений
                progress.Repetitions++;

                // Вычисляем новый интервал
                if (progress.Repetitions == 1)
                {
                    progress.Interval = 1; // 1 день
                }
                else if (progress.Repetitions == 2)
                {
                    progress.Interval = 6; // 6 дней
                }
                else
                {
                    // Для последующих повторений умножаем на EaseFactor
                    progress.Interval = (int)Math.Ceiling(progress.Interval * progress.EaseFactor);
                }

                // Устанавливаем дату следующего повторения
                progress.NextReviewDate = now.AddDays(progress.Interval);

                // ОПТИМИЗИРОВАНО: Карточка считается освоенной, если Repetitions >= 3 и EaseFactor >= 1.8
                // Порог снижен до 1.8 для более щадящей оценки освоения
                // При трех ответах с качеством 3 EaseFactor = 2.08 (комфортно выше порога)
                // При смешанных ответах (3,3,4) EaseFactor может быть около 2.2
                // Это позволяет считать карточку освоенной даже при некоторых затруднениях
                progress.IsMastered = progress.Repetitions >= 3 && progress.EaseFactor >= 1.8;
            }
        }

        /// <summary>
        /// Проверяет, нужно ли повторять карточку пользователю
        /// </summary>
        public bool IsDueForReview(UserFlashcardProgress progress)
        {
            // Если карточка никогда не повторялась
            if (progress.NextReviewDate == null)
            {
                return true;
            }

            // Если дата повторения наступила или прошла
            return progress.NextReviewDate <= DateTime.UtcNow;
        }
    }
}
