using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;
namespace UniStart.Models.Flashcards
{
    /// <summary>
    /// Типы интерактивных карточек
    /// </summary>
    public enum FlashcardType
    {
        /// <summary>
        /// Выбор правильного ответа из нескольких вариантов
        /// </summary>
        SingleChoice = 0,
        
        /// <summary>
        /// Заполнение пропуска в тексте
        /// </summary>
        FillInTheBlank = 1,
        
        /// <summary>
        /// Сопоставление двух колонок (термин - определение)
        /// </summary>
        Matching = 2,
        
        /// <summary>
        /// Восстановление правильного порядка элементов
        /// </summary>
        Sequencing = 3
    }
}
