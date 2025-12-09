namespace UniStart.Models
{
    /// <summary>
    /// Типы интерактивных карточек
    /// </summary>
    public enum FlashcardType
    {
        /// <summary>
        /// Выбор правильного ответа из нескольких вариантов
        /// </summary>
        MultipleChoice = 0,
        
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
