using Microsoft.EntityFrameworkCore;
using UniStart.Domain.Entities;

namespace UniStart.Infrastructure.Data;

public class UniStartDbContext : DbContext
{
    public UniStartDbContext(DbContextOptions<UniStartDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ExamType> ExamTypes => Set<ExamType>();
    public DbSet<ExamSection> ExamSections => Set<ExamSection>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
    public DbSet<UserAnswer> UserAnswers => Set<UserAnswer>();
    public DbSet<UserSkillProfile> UserSkillProfiles => Set<UserSkillProfile>();
    public DbSet<TestSession> TestSessions => Set<TestSession>();
    public DbSet<TopicDependency> TopicDependencies => Set<TopicDependency>();
    public DbSet<StudyGoal> StudyGoals => Set<StudyGoal>();
    public DbSet<StudyPlan> StudyPlans => Set<StudyPlan>();
    public DbSet<StudyPlanEntry> StudyPlanEntries => Set<StudyPlanEntry>();
    public DbSet<UserMilestone> UserMilestones => Set<UserMilestone>();
    public DbSet<TopicLesson> TopicLessons => Set<TopicLesson>();
    public DbSet<ReadingPassage> ReadingPassages => Set<ReadingPassage>();
    public DbSet<MockExam> MockExams => Set<MockExam>();
    public DbSet<MockExamSection> MockExamSections => Set<MockExamSection>();
    public DbSet<MockExamAttempt> MockExamAttempts => Set<MockExamAttempt>();
    public DbSet<MockExamAnswer> MockExamAnswers => Set<MockExamAnswer>();
    public DbSet<NotificationPreferences> NotificationPreferences => Set<NotificationPreferences>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // ExamType configuration
        modelBuilder.Entity<ExamType>(entity =>
        {
            entity.ToTable("ExamTypes");
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        // ExamSection configuration
        modelBuilder.Entity<ExamSection>(entity =>
        {
            entity.ToTable("ExamSections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.ExamType)
                  .WithMany(et => et.Sections)
                  .HasForeignKey(e => e.ExamTypeCode)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Skill configuration
        modelBuilder.Entity<Skill>(entity =>
        {
            entity.ToTable("Skills");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // Topic configuration
        modelBuilder.Entity<Topic>(entity =>
        {
            entity.ToTable("Topics");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.Skill)
                  .WithMany(s => s.Topics)
                  .HasForeignKey(e => e.SkillId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Section)
                  .WithMany(s => s.Topics)
                  .HasForeignKey(e => e.SectionId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Question configuration
        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("Questions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired();
            entity.Property(e => e.DifficultyParam).HasDefaultValue(0.0);
            entity.Property(e => e.DiscriminationParam).HasDefaultValue(1.0);
            entity.Property(e => e.GuessParam).HasDefaultValue(0.25);
            entity.HasOne(e => e.Topic)
                  .WithMany(t => t.Questions)
                  .HasForeignKey(e => e.TopicId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // AnswerOption configuration
        modelBuilder.Entity<AnswerOption>(entity =>
        {
            entity.ToTable("AnswerOptions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired();
            entity.HasOne(e => e.Question)
                  .WithMany(q => q.AnswerOptions)
                  .HasForeignKey(e => e.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // UserAnswer configuration
        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.ToTable("UserAnswers");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.UserAnswers)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Question)
                  .WithMany(q => q.UserAnswers)
                  .HasForeignKey(e => e.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.AnswerOption)
                  .WithMany(ao => ao.UserAnswers)
                  .HasForeignKey(e => e.AnswerOptionId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.TestSession)
                  .WithMany(ts => ts.Answers)
                  .HasForeignKey(e => e.TestSessionId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // TestSession configuration
        modelBuilder.Entity<TestSession>(entity =>
        {
            entity.ToTable("TestSessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Mode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ExamTypeCode).IsRequired().HasMaxLength(10);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.TestSessions)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ExamType)
                  .WithMany()
                  .HasForeignKey(e => e.ExamTypeCode)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // UserSkillProfile configuration (composite key)
        modelBuilder.Entity<UserSkillProfile>(entity =>
        {
            entity.ToTable("UserSkillProfiles");
            entity.HasKey(e => new { e.UserId, e.SkillId });
            entity.Property(e => e.Theta).HasDefaultValue(0.0);
            entity.Property(e => e.ThetaSE).HasDefaultValue(1.0);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.SkillProfiles)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Skill)
                  .WithMany(s => s.UserProfiles)
                  .HasForeignKey(e => e.SkillId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TopicDependency configuration (composite key)
        modelBuilder.Entity<TopicDependency>(entity =>
        {
            entity.ToTable("TopicDependencies");
            entity.HasKey(e => new { e.TopicId, e.PrerequisiteTopicId });
            entity.Property(e => e.Weight).HasDefaultValue(1.0);
            entity.HasOne(e => e.Topic)
                  .WithMany(t => t.Prerequisites)
                  .HasForeignKey(e => e.TopicId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.PrerequisiteTopic)
                  .WithMany(t => t.DependentTopics)
                  .HasForeignKey(e => e.PrerequisiteTopicId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Study Goal ─────────────────────────────────────────
        modelBuilder.Entity<StudyGoal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ExamType)
                  .WithMany()
                  .HasForeignKey(e => e.ExamTypeCode)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // ─── Study Plan ─────────────────────────────────────────
        modelBuilder.Entity<StudyPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Goal)
                  .WithMany(g => g.StudyPlans)
                  .HasForeignKey(e => e.GoalId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // ─── Study Plan Entry ───────────────────────────────────
        modelBuilder.Entity<StudyPlanEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Plan)
                  .WithMany(p => p.Entries)
                  .HasForeignKey(e => e.PlanId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Topic)
                  .WithMany()
                  .HasForeignKey(e => e.TopicId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Type)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.Property(e => e.IsCompleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<UserMilestone>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(10).HasDefaultValue("🏆");
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.Code }).IsUnique();
        });

        // ─── Topic Lesson ───────────────────────────────────
        modelBuilder.Entity<TopicLesson>(entity =>
        {
            entity.ToTable("TopicLessons");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.VideoUrl).HasMaxLength(500);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.HasOne(e => e.Topic)
                  .WithMany(t => t.Lessons)
                  .HasForeignKey(e => e.TopicId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── Reading Passage ───────────────────────────────
        modelBuilder.Entity<ReadingPassage>(entity =>
        {
            entity.ToTable("ReadingPassages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.HasOne(e => e.Topic)
                  .WithMany()
                  .HasForeignKey(e => e.TopicId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Question → ReadingPassage (optional)
        modelBuilder.Entity<Question>()
            .HasOne(e => e.ReadingPassage)
            .WithMany(p => p.Questions)
            .HasForeignKey(e => e.ReadingPassageId)
            .OnDelete(DeleteBehavior.SetNull);

        // ─── Mock Exam ──────────────────────────────────────
        modelBuilder.Entity<MockExam>(entity =>
        {
            entity.ToTable("MockExams");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasOne(e => e.ExamType)
                  .WithMany()
                  .HasForeignKey(e => e.ExamTypeCode)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Mock Exam Section ──────────────────────────────
        modelBuilder.Entity<MockExamSection>(entity =>
        {
            entity.ToTable("MockExamSections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Instructions).HasMaxLength(2000);
            entity.HasOne(e => e.MockExam)
                  .WithMany(m => m.Sections)
                  .HasForeignKey(e => e.MockExamId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ExamSection)
                  .WithMany()
                  .HasForeignKey(e => e.ExamSectionId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ─── Mock Exam Attempt ──────────────────────────────
        modelBuilder.Entity<MockExamAttempt>(entity =>
        {
            entity.ToTable("MockExamAttempts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.MockExam)
                  .WithMany(m => m.Attempts)
                  .HasForeignKey(e => e.MockExamId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── Mock Exam Answer ───────────────────────────────
        modelBuilder.Entity<MockExamAnswer>(entity =>
        {
            entity.ToTable("MockExamAnswers");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Attempt)
                  .WithMany(a => a.Answers)
                  .HasForeignKey(e => e.AttemptId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Question)
                  .WithMany()
                  .HasForeignKey(e => e.QuestionId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.SelectedOption)
                  .WithMany()
                  .HasForeignKey(e => e.SelectedOptionId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ─── Notification Preferences ───────────────────────
        modelBuilder.Entity<NotificationPreferences>(entity =>
        {
            entity.ToTable("NotificationPreferences");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WelcomeEmail).HasDefaultValue(true);
            entity.Property(e => e.StreakReminder).HasDefaultValue(true);
            entity.Property(e => e.WeeklyDigest).HasDefaultValue(true);
            entity.Property(e => e.StudyPlanReminder).HasDefaultValue(true);
            entity.Property(e => e.AchievementNotification).HasDefaultValue(true);
            entity.HasOne(e => e.User)
                  .WithOne(u => u.NotificationPreferences)
                  .HasForeignKey<NotificationPreferences>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Seed exam types
        modelBuilder.Entity<ExamType>().HasData(
            new ExamType { Code = "SAT", Name = "SAT (Scholastic Assessment Test)" },
            new ExamType { Code = "TOEFL", Name = "TOEFL (Test of English as a Foreign Language)" },
            new ExamType { Code = "NUET", Name = "NUET (Nazarbayev University Entrance Test)" }
        );
    }
}
