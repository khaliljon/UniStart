namespace UniStart.Domain.Entities;

/// <summary>
/// Домашнее задание, созданное тьютором.
/// Содержит набор вопросов с дедлайном для группы учеников.
/// </summary>
public class Assignment
{
    public int Id { get; set; }
    public int TutorUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual User TutorUser { get; set; } = null!;
    public virtual ICollection<AssignmentQuestion> Questions { get; set; } = new List<AssignmentQuestion>();
    public virtual ICollection<AssignmentStudent> Students { get; set; } = new List<AssignmentStudent>();
}

/// <summary>
/// Вопрос в задании с порядком отображения.
/// </summary>
public class AssignmentQuestion
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }
    public int QuestionId { get; set; }
    public int OrderIndex { get; set; }

    // Navigation
    public virtual Assignment Assignment { get; set; } = null!;
    public virtual Question Question { get; set; } = null!;
}

/// <summary>
/// Ученик, назначенный на задание, с прогрессом прохождения.
/// </summary>
public class AssignmentStudent
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }
    public int StudentUserId { get; set; }
    public AssignmentStudentStatus Status { get; set; } = AssignmentStudentStatus.Assigned;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? Score { get; set; }

    // Navigation
    public virtual Assignment Assignment { get; set; } = null!;
    public virtual User StudentUser { get; set; } = null!;
    public virtual ICollection<AssignmentAnswer> Answers { get; set; } = new List<AssignmentAnswer>();
}

public enum AssignmentStudentStatus
{
    Assigned,
    InProgress,
    Completed,
    Overdue
}

/// <summary>
/// Ответ ученика на вопрос задания.
/// </summary>
public class AssignmentAnswer
{
    public int Id { get; set; }
    public int AssignmentStudentId { get; set; }
    public int QuestionId { get; set; }
    public int SelectedOptionId { get; set; }
    public bool IsCorrect { get; set; }
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual AssignmentStudent AssignmentStudent { get; set; } = null!;
    public virtual Question Question { get; set; } = null!;
    public virtual AnswerOption SelectedOption { get; set; } = null!;
}
