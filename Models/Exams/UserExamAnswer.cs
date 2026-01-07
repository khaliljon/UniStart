using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;
namespace UniStart.Models.Exams;

/// <summary>
/// Ответ студента на вопрос экзамена
/// </summary>
public class UserExamAnswer
{
    public int Id { get; set; }
    
    public bool IsCorrect { get; set; } // Правильно ли ответил
    public int PointsEarned { get; set; } // Заработанные баллы за этот вопрос
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    
    // Связи
    public int AttemptId { get; set; }
    public UserExamAttempt Attempt { get; set; } = null!;
    public int QuestionId { get; set; }
    public ExamQuestion Question { get; set; } = null!;
    public int SelectedAnswerId { get; set; }
    public ExamAnswer SelectedAnswer { get; set; } = null!;
}
