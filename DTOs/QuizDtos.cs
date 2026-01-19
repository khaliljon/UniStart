using System.ComponentModel.DataAnnotations;

namespace UniStart.DTOs
{
    // DTOs для Quiz
    public class QuizDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<SubjectDto> Subjects { get; set; } = new();
        public int TimeLimit { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
        public int TotalPoints { get; set; }
        public bool IsPublic { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class QuizDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<SubjectDto> Subjects { get; set; } = new();
        public int TimeLimit { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public bool IsPublished { get; set; }
        public bool IsLearningMode { get; set; }
        public List<QuizQuestionDto> Questions { get; set; } = new();
    }

    public class CreateQuizDto
    {
        [Required(ErrorMessage = "Название теста обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Описание обязательно")]
        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string Description { get; set; } = string.Empty;
        
        [Range(0, 300, ErrorMessage = "Время должно быть от 0 до 300 минут")]
        public int TimeLimit { get; set; }
        
        [Required(ErrorMessage = "Уровень сложности обязателен")]
        [RegularExpression("^(Easy|Medium|Hard)$", ErrorMessage = "Сложность должна быть: Easy, Medium или Hard")]
        public string Difficulty { get; set; } = "Medium";
        
        public List<int> SubjectIds { get; set; } = new();
        
        public bool IsPublic { get; set; }
        public bool IsPublished { get; set; }
        public bool IsLearningMode { get; set; }
    }

    public class UpdateQuizDto
    {
        [Required(ErrorMessage = "Название теста обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string Description { get; set; } = string.Empty;
        
        [Range(0, 300, ErrorMessage = "Время должно быть от 0 до 300 минут")]
        public int TimeLimit { get; set; }
        
        [Required(ErrorMessage = "Уровень сложности обязателен")]
        [RegularExpression("^(Easy|Medium|Hard)$", ErrorMessage = "Сложность должна быть: Easy, Medium или Hard")]
        public string Difficulty { get; set; } = "Medium";
        
        public List<int> SubjectIds { get; set; } = new();

        public bool IsPublic { get; set; }
        public bool IsPublished { get; set; }
        public bool IsLearningMode { get; set; } = true;
        
        public string? QuizType { get; set; }
        
        public List<UpdateQuestionWithAnswersDto> Questions { get; set; } = new();
    }

    public class UpdateQuestionWithAnswersDto
    {
        [Required]
        public string Text { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "SingleChoice";
        public int Points { get; set; } = 1;
        public string? Explanation { get; set; }
        public int Order { get; set; }
        public List<UpdateAnswerInQuestionDto> Answers { get; set; } = new();
    }

    public class UpdateAnswerInQuestionDto
    {
        [Required]
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int Order { get; set; }
    }

    public class QuizQuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Points { get; set; }
        public string? ImageUrl { get; set; }
        public string? Explanation { get; set; }
        public List<QuizAnswerDto> Answers { get; set; } = new();
    }

    public class CreateQuizQuestionDto
    {
        [Required(ErrorMessage = "Текст вопроса обязателен")]
        [StringLength(1000, ErrorMessage = "Текст вопроса не должен превышать 1000 символов")]
        public string Text { get; set; } = string.Empty;
        
        [Range(1, 100, ErrorMessage = "Баллы должны быть от 1 до 100")]
        public int Points { get; set; } = 1;
        
        [StringLength(500, ErrorMessage = "URL не должен превышать 500 символов")]
        public string? ImageUrl { get; set; }
        
        [StringLength(2000, ErrorMessage = "Объяснение не должно превышать 2000 символов")]
        public string? Explanation { get; set; }
        
        [Required(ErrorMessage = "ID квиза обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "ID квиза должен быть больше 0")]
        public int QuizId { get; set; }
    }

    public class UpdateQuizQuestionDto
    {
        [Required(ErrorMessage = "Текст вопроса обязателен")]
        [StringLength(1000, ErrorMessage = "Текст вопроса не должен превышать 1000 символов")]
        public string Text { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Тип вопроса обязателен")]
        [RegularExpression("^(SingleChoice|MultipleChoice|TrueFalse)$", ErrorMessage = "Тип должен быть: SingleChoice, MultipleChoice или TrueFalse")]
        public string QuestionType { get; set; } = "SingleChoice";
        
        [Range(1, 100, ErrorMessage = "Баллы должны быть от 1 до 100")]
        public int Points { get; set; } = 1;
        
        [StringLength(500, ErrorMessage = "URL не должен превышать 500 символов")]
        public string? ImageUrl { get; set; }
        
        [StringLength(2000, ErrorMessage = "Объяснение не должно превышать 2000 символов")]
        public string? Explanation { get; set; }
    }

    public class QuizAnswerDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool? IsCorrect { get; set; } // Null при отправке пользователю
    }

    public class CreateQuizAnswerDto
    {
        [Required(ErrorMessage = "Текст ответа обязателен")]
        [StringLength(500, ErrorMessage = "Текст ответа не должен превышать 500 символов")]
        public string Text { get; set; } = string.Empty;
        
        public bool IsCorrect { get; set; } = false;
        
        [Required(ErrorMessage = "ID вопроса обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "ID вопроса должен быть больше 0")]
        public int QuestionId { get; set; }
    }

    public class UpdateQuizAnswerDto
    {
        [Required(ErrorMessage = "Текст ответа обязателен")]
        [StringLength(500, ErrorMessage = "Текст ответа не должен превышать 500 символов")]
        public string Text { get; set; } = string.Empty;
        
        public bool IsCorrect { get; set; } = false;
    }

    public class SubmitQuizDto
    {
        [Required(ErrorMessage = "ID теста обязателен")]
        [Range(1, int.MaxValue, ErrorMessage = "ID теста должен быть больше 0")]
        public int QuizId { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Время должно быть положительным")]
        public int TimeSpentSeconds { get; set; }
        
        [Required(ErrorMessage = "Ответы обязательны")]
        public Dictionary<int, List<int>> UserAnswers { get; set; } = new(); // QuestionId -> AnswerIds
    }

    public class QuizResultDto
    {
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public double Percentage { get; set; }
        public int TimeSpentSeconds { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectQuestions { get; set; }
        public bool Passed { get; set; }
        public List<QuizQuestionResultDto> QuestionResults { get; set; } = new();
    }

    public class QuizQuestionResultDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int PointsEarned { get; set; }
        public int MaxPoints { get; set; }
        public List<int> CorrectAnswerIds { get; set; } = new();
        public List<int> UserAnswerIds { get; set; } = new();
        public string? Explanation { get; set; }
    }
}
