using Microsoft.EntityFrameworkCore;
using UniStart.Domain.Entities;

namespace UniStart.Infrastructure.Data;

public class UniStartDbContext : DbContext
{
    public UniStartDbContext(DbContextOptions<UniStartDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<SchoolApplication> SchoolApplications => Set<SchoolApplication>();
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
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<TutorProfile> TutorProfiles => Set<TutorProfile>();
    public DbSet<TutorSchool> TutorSchools => Set<TutorSchool>();
    public DbSet<TutorScheduleSlot> TutorScheduleSlots => Set<TutorScheduleSlot>();
    public DbSet<TutorReview> TutorReviews => Set<TutorReview>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<TutorStudent> TutorStudents => Set<TutorStudent>();
    public DbSet<TutorInviteCode> TutorInviteCodes => Set<TutorInviteCode>();
    public DbSet<TutorInviteCodeUsage> TutorInviteCodeUsages => Set<TutorInviteCodeUsage>();

    // Learning v2 entities
    public DbSet<LessonStep> LessonSteps => Set<LessonStep>();
    public DbSet<UserLessonProgress> UserLessonProgress => Set<UserLessonProgress>();
    public DbSet<FormulaCard> FormulaCards => Set<FormulaCard>();
    public DbSet<UserFormulaBookmark> UserFormulaBookmarks => Set<UserFormulaBookmark>();
    public DbSet<FlashcardDeck> FlashcardDecks => Set<FlashcardDeck>();
    public DbSet<Flashcard> Flashcards => Set<Flashcard>();
    public DbSet<UserFlashcardProgress> UserFlashcardProgress => Set<UserFlashcardProgress>();
    public DbSet<TimedDrillResult> TimedDrillResults => Set<TimedDrillResult>();
    public DbSet<DrillTemplate> DrillTemplates => Set<DrillTemplate>();
    public DbSet<StrategyGuide> StrategyGuides => Set<StrategyGuide>();
    public DbSet<UserGuideProgress> UserGuideProgress => Set<UserGuideProgress>();
    public DbSet<UserMistakeNote> UserMistakeNotes => Set<UserMistakeNote>();

    // Question Import entities
    public DbSet<QuestionImportJob> QuestionImportJobs => Set<QuestionImportJob>();
    public DbSet<ImportedQuestionDraft> ImportedQuestionDrafts => Set<ImportedQuestionDraft>();
    public DbSet<ImportJobFile> ImportJobFiles => Set<ImportJobFile>();

    // Assignment entities (Sprint 7 Этап 3)
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<AssignmentQuestion> AssignmentQuestions => Set<AssignmentQuestion>();
    public DbSet<AssignmentStudent> AssignmentStudents => Set<AssignmentStudent>();
    public DbSet<AssignmentAnswer> AssignmentAnswers => Set<AssignmentAnswer>();

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
            // Soft delete (OP-9)
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            // Block / Suspend (OP-14)
            entity.Property(e => e.IsBlocked).HasDefaultValue(false);
            entity.Property(e => e.BlockReason).HasMaxLength(500);
            // Free mock exam tracking (FIX-37)
            entity.Property(e => e.FreeMockUsed).HasDefaultValue(false);
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

            // Soft delete (OP-9)
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            // Tutor content (Sprint 7 Этап 2)
            entity.Property(e => e.IsPrivate).HasDefaultValue(false);
            entity.HasOne(e => e.CreatedByTutor)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedByTutorId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.CreatedByTutorId).HasDatabaseName("IX_Questions_CreatedByTutorId");

            // Performance index (OP-8)
            entity.HasIndex(e => e.TopicId).HasDatabaseName("IX_Questions_TopicId");
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

            // Performance indexes (OP-8)
            entity.HasIndex(e => new { e.UserId, e.AnsweredAt }).HasDatabaseName("IX_UserAnswers_UserId_AnsweredAt");
            entity.HasIndex(e => new { e.UserId, e.QuestionId, e.TestSessionId }).HasDatabaseName("IX_UserAnswers_UserId_QuestionId_SessionId");
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

            // Performance index (OP-8)
            entity.HasIndex(e => new { e.UserId, e.StartedAt }).HasDatabaseName("IX_TestSessions_UserId_StartedAt");
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
            entity.Property(e => e.SelectedSectionIds).HasMaxLength(200);
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

            // Performance index (OP-8)
            entity.HasIndex(e => new { e.PlanId, e.Date }).HasDatabaseName("IX_StudyPlanEntries_PlanId_Date");
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

            // Performance index (OP-8)
            entity.HasIndex(e => new { e.UserId, e.Status }).HasDatabaseName("IX_MockExamAttempts_UserId_Status");
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

        // ─── Audit Log (OP-7) ────────────────────────────
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EntityId).HasMaxLength(50);
            entity.Property(e => e.UserEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.OldValues).HasColumnType("jsonb");
            entity.Property(e => e.NewValues).HasColumnType("jsonb");
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.Timestamp).HasDatabaseName("IX_AuditLogs_Timestamp");
            entity.HasIndex(e => new { e.EntityType, e.EntityId }).HasDatabaseName("IX_AuditLogs_Entity");
            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_AuditLogs_UserId");
        });

        // ─── Tutor Profile ─────────────────────────────────
        modelBuilder.Entity<TutorProfile>(entity =>
        {
            entity.ToTable("TutorProfiles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Headline).HasMaxLength(200);
            entity.Property(e => e.Bio).HasMaxLength(2000);
            entity.Property(e => e.Experience).HasMaxLength(1000);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.Specializations).HasMaxLength(100);
            entity.Property(e => e.HourlyRate).HasPrecision(10, 2);
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.InviteCode).HasMaxLength(20);
            entity.Property(e => e.ContactPreference)
                  .HasConversion<string>()
                  .HasMaxLength(10);
            entity.HasOne(e => e.User)
                  .WithOne(u => u.TutorProfile)
                  .HasForeignKey<TutorProfile>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.IsAvailable).HasDatabaseName("IX_TutorProfiles_IsAvailable");
            entity.HasOne(e => e.School)
                  .WithMany(s => s.Tutors)
                  .HasForeignKey(e => e.SchoolId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ─── Tutor School ──────────────────────────────────
        modelBuilder.Entity<TutorSchool>(entity =>
        {
            entity.ToTable("TutorSchools");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.WebsiteUrl).HasMaxLength(500);
            entity.Property(e => e.InstagramUrl).HasMaxLength(500);
            entity.Property(e => e.TelegramUrl).HasMaxLength(500);
            entity.Property(e => e.Specializations).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsPartner).HasDefaultValue(true);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasOne(e => e.Owner)
                  .WithMany()
                  .HasForeignKey(e => e.OwnerUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ─── User → School (White Label) ─────────────────────
        modelBuilder.Entity<User>()
            .HasOne(u => u.School)
            .WithMany()
            .HasForeignKey(u => u.SchoolId)
            .OnDelete(DeleteBehavior.SetNull);

        // ─── Tutor Schedule Slot ────────────────────────────
        modelBuilder.Entity<TutorScheduleSlot>(entity =>
        {
            entity.ToTable("TutorScheduleSlots");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DayOfWeek).HasConversion<int>();
            entity.HasOne(e => e.TutorProfile)
                  .WithMany(tp => tp.Schedule)
                  .HasForeignKey(e => e.TutorProfileId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── Tutor Review ──────────────────────────────────
        modelBuilder.Entity<TutorReview>(entity =>
        {
            entity.ToTable("TutorReviews");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.HasOne(e => e.TutorProfile)
                  .WithMany(tp => tp.Reviews)
                  .HasForeignKey(e => e.TutorProfileId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Student)
                  .WithMany()
                  .HasForeignKey(e => e.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);
            // One review per student per tutor
            entity.HasIndex(e => new { e.TutorProfileId, e.StudentId })
                  .IsUnique()
                  .HasDatabaseName("IX_TutorReviews_Tutor_Student");
        });

        // ─── Conversation ──────────────────────────────────
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("Conversations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LastMessagePreview).HasMaxLength(100);
            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.HasOne(e => e.Student)
                  .WithMany()
                  .HasForeignKey(e => e.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Tutor)
                  .WithMany()
                  .HasForeignKey(e => e.TutorId)
                  .OnDelete(DeleteBehavior.Restrict);
            // One conversation per student-tutor pair
            entity.HasIndex(e => new { e.StudentId, e.TutorId })
                  .IsUnique()
                  .HasDatabaseName("IX_Conversations_Student_Tutor");
            entity.HasIndex(e => e.LastMessageAt).HasDatabaseName("IX_Conversations_LastMessageAt");
        });

        // ─── Message ───────────────────────────────────────
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).HasMaxLength(4000).IsRequired();
            entity.Property(e => e.Type)
                  .HasConversion<string>()
                  .HasMaxLength(10);
            entity.HasOne(e => e.Conversation)
                  .WithMany(c => c.Messages)
                  .HasForeignKey(e => e.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Sender)
                  .WithMany()
                  .HasForeignKey(e => e.SenderId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.ConversationId, e.SentAt })
                  .HasDatabaseName("IX_Messages_Conversation_SentAt");
        });

        // ─── Tutor-Student Binding ─────────────────────────
        modelBuilder.Entity<TutorStudent>(entity =>
        {
            entity.ToTable("TutorStudents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InviteCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.HasOne(e => e.TutorUser)
                  .WithMany()
                  .HasForeignKey(e => e.TutorUserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.StudentUser)
                  .WithMany()
                  .HasForeignKey(e => e.StudentUserId)
                  .OnDelete(DeleteBehavior.Restrict);
            // One active binding per student
            entity.HasIndex(e => e.StudentUserId)
                  .HasFilter("\"Status\" = 'Active'")
                  .IsUnique()
                  .HasDatabaseName("IX_TutorStudents_Student_Active");
            entity.HasIndex(e => e.TutorUserId)
                  .HasDatabaseName("IX_TutorStudents_TutorUserId");
        });

        // ─── Lesson Step (TH-1) ────────────────────────────
        modelBuilder.Entity<LessonStep>(entity =>
        {
            entity.ToTable("LessonSteps");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.StepType).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.HasOne(e => e.Lesson)
                  .WithMany(l => l.Steps)
                  .HasForeignKey(e => e.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.QuizQuestion)
                  .WithMany()
                  .HasForeignKey(e => e.QuizQuestionId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ─── User Lesson Progress (TH-1) ───────────────────
        modelBuilder.Entity<UserLessonProgress>(entity =>
        {
            entity.ToTable("UserLessonProgress");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.LessonStep)
                  .WithMany(s => s.UserProgress)
                  .HasForeignKey(e => e.LessonStepId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.LessonStepId })
                  .IsUnique()
                  .HasDatabaseName("IX_UserLessonProgress_User_Step");
        });

        // ─── Formula Card (TH-2) ───────────────────────────
        modelBuilder.Entity<FormulaCard>(entity =>
        {
            entity.ToTable("FormulaCards");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Formula).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.HasOne(e => e.Topic)
                  .WithMany()
                  .HasForeignKey(e => e.TopicId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── User Formula Bookmark (TH-2) ──────────────────
        modelBuilder.Entity<UserFormulaBookmark>(entity =>
        {
            entity.ToTable("UserFormulaBookmarks");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.FormulaCard)
                  .WithMany(f => f.Bookmarks)
                  .HasForeignKey(e => e.FormulaCardId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.FormulaCardId })
                  .IsUnique()
                  .HasDatabaseName("IX_UserFormulaBookmarks_User_Formula");
        });

        // ─── Flashcard Deck (TH-3) ─────────────────────────
        modelBuilder.Entity<FlashcardDeck>(entity =>
        {
            entity.ToTable("FlashcardDecks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsSystem).HasDefaultValue(false);
            entity.HasOne(e => e.ExamType)
                  .WithMany()
                  .HasForeignKey(e => e.ExamTypeCode)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Topic)
                  .WithMany()
                  .HasForeignKey(e => e.TopicId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ─── Flashcard (TH-3) ──────────────────────────────
        modelBuilder.Entity<Flashcard>(entity =>
        {
            entity.ToTable("Flashcards");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Front).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Back).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.HasOne(e => e.Deck)
                  .WithMany(d => d.Cards)
                  .HasForeignKey(e => e.DeckId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── User Flashcard Progress (TH-3, SM-2) ──────────
        modelBuilder.Entity<UserFlashcardProgress>(entity =>
        {
            entity.ToTable("UserFlashcardProgress");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EaseFactor).HasDefaultValue(2.5);
            entity.Property(e => e.IntervalDays).HasDefaultValue(1);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Flashcard)
                  .WithMany(f => f.UserProgress)
                  .HasForeignKey(e => e.FlashcardId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.FlashcardId })
                  .IsUnique()
                  .HasDatabaseName("IX_UserFlashcardProgress_User_Card");
            entity.HasIndex(e => new { e.UserId, e.NextReviewAt })
                  .HasDatabaseName("IX_UserFlashcardProgress_User_NextReview");
        });

        // ─── Timed Drill Result (TH-4) ─────────────────────
        modelBuilder.Entity<TimedDrillResult>(entity =>
        {
            entity.ToTable("TimedDrillResults");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DrillType).HasConversion<string>().HasMaxLength(20);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ExamType)
                  .WithMany()
                  .HasForeignKey(e => e.ExamTypeCode)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Topic)
                  .WithMany()
                  .HasForeignKey(e => e.TopicId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.UserId, e.DrillType })
                  .HasDatabaseName("IX_TimedDrillResults_User_Type");
        });

        // ─── Drill Template ────────────────────────────────
        modelBuilder.Entity<DrillTemplate>(entity =>
        {
            entity.ToTable("DrillTemplates");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DrillType).HasConversion<string>().HasMaxLength(20);
            entity.HasOne(e => e.ExamType)
                  .WithMany()
                  .HasForeignKey(e => e.ExamTypeCode)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Topic)
                  .WithMany()
                  .HasForeignKey(e => e.TopicId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ─── Strategy Guide (TH-5) ─────────────────────────
        modelBuilder.Entity<StrategyGuide>(entity =>
        {
            entity.ToTable("StrategyGuides");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Summary).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Category).IsRequired().HasMaxLength(30);
            entity.Property(e => e.EstimatedReadMinutes).HasDefaultValue(5);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.HasOne(e => e.ExamType)
                  .WithMany()
                  .HasForeignKey(e => e.ExamTypeCode)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── User Guide Progress (TH-5) ────────────────────
        modelBuilder.Entity<UserGuideProgress>(entity =>
        {
            entity.ToTable("UserGuideProgress");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Guide)
                  .WithMany(g => g.UserProgress)
                  .HasForeignKey(e => e.GuideId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.GuideId })
                  .IsUnique()
                  .HasDatabaseName("IX_UserGuideProgress_User_Guide");
        });

        // ─── User Mistake Note (TH-6) ──────────────────────
        modelBuilder.Entity<UserMistakeNote>(entity =>
        {
            entity.ToTable("UserMistakeNotes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ErrorType).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.NoteText).HasMaxLength(1000);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.UserAnswer)
                  .WithMany()
                  .HasForeignKey(e => e.UserAnswerId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.UserAnswerId })
                  .IsUnique()
                  .HasDatabaseName("IX_UserMistakeNotes_User_Answer");
        });

        // ─── Question Import ───────────────────────────────────────────
        modelBuilder.Entity<QuestionImportJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FileType).HasMaxLength(10).IsRequired();
            entity.Property(e => e.ExamTypeCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Instructions).HasMaxLength(2000);
            entity.HasOne(e => e.AdminUser)
                  .WithMany()
                  .HasForeignKey(e => e.AdminUserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Drafts)
                  .WithOne(d => d.ImportJob)
                  .HasForeignKey(d => d.ImportJobId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Files)
                  .WithOne(f => f.ImportJob)
                  .HasForeignKey(f => f.ImportJobId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ImportJobFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FileType).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Role).HasConversion<int>();
        });

        modelBuilder.Entity<ImportedQuestionDraft>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuestionText).IsRequired();
            entity.Property(e => e.OptionsJson).IsRequired().HasDefaultValue("[]");
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Source).HasConversion<int>();
            entity.Property(e => e.Difficulty).HasConversion<int>();
            entity.HasOne(e => e.Topic)
                  .WithMany()
                  .HasForeignKey(e => e.TopicId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.ImportJobId, e.Status })
                  .HasDatabaseName("IX_ImportedQuestionDrafts_Job_Status");
        });

        // ═══════════════════════════════════════════════════════
        //  ASSIGNMENT (Sprint 7 Этап 3)
        // ═══════════════════════════════════════════════════════

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.ToTable("Assignments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasOne(e => e.TutorUser)
                  .WithMany()
                  .HasForeignKey(e => e.TutorUserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.TutorUserId).HasDatabaseName("IX_Assignments_TutorUserId");
        });

        modelBuilder.Entity<AssignmentQuestion>(entity =>
        {
            entity.ToTable("AssignmentQuestions");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Assignment)
                  .WithMany(a => a.Questions)
                  .HasForeignKey(e => e.AssignmentId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Question)
                  .WithMany()
                  .HasForeignKey(e => e.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.AssignmentId, e.QuestionId })
                  .IsUnique()
                  .HasDatabaseName("IX_AssignmentQuestions_Assignment_Question");
        });

        modelBuilder.Entity<AssignmentStudent>(entity =>
        {
            entity.ToTable("AssignmentStudents");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Assignment)
                  .WithMany(a => a.Students)
                  .HasForeignKey(e => e.AssignmentId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.StudentUser)
                  .WithMany()
                  .HasForeignKey(e => e.StudentUserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.AssignmentId, e.StudentUserId })
                  .IsUnique()
                  .HasDatabaseName("IX_AssignmentStudents_Assignment_Student");
            entity.HasIndex(e => e.StudentUserId).HasDatabaseName("IX_AssignmentStudents_StudentUserId");
        });

        modelBuilder.Entity<AssignmentAnswer>(entity =>
        {
            entity.ToTable("AssignmentAnswers");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.AssignmentStudent)
                  .WithMany(s => s.Answers)
                  .HasForeignKey(e => e.AssignmentStudentId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Question)
                  .WithMany()
                  .HasForeignKey(e => e.QuestionId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.SelectedOption)
                  .WithMany()
                  .HasForeignKey(e => e.SelectedOptionId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.AssignmentStudentId, e.QuestionId })
                  .IsUnique()
                  .HasDatabaseName("IX_AssignmentAnswers_Student_Question");
        });

        // ═══════════════════════════════════════════════════════
        //  TUTOR INVITE CODES (S-6)
        // ═══════════════════════════════════════════════════════

        modelBuilder.Entity<TutorInviteCode>(entity =>
        {
            entity.ToTable("TutorInviteCodes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(8);
            entity.Property(e => e.Note).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UsedCount).HasDefaultValue(0);
            entity.HasOne(e => e.TutorUser)
                  .WithMany()
                  .HasForeignKey(e => e.TutorUserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.Code)
                  .IsUnique()
                  .HasDatabaseName("IX_TutorInviteCodes_Code");
            entity.HasIndex(e => e.TutorUserId)
                  .HasDatabaseName("IX_TutorInviteCodes_TutorUserId");
        });

        modelBuilder.Entity<TutorInviteCodeUsage>(entity =>
        {
            entity.ToTable("TutorInviteCodeUsages");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.InviteCode)
                  .WithMany(c => c.Usages)
                  .HasForeignKey(e => e.InviteCodeId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.StudentUser)
                  .WithMany()
                  .HasForeignKey(e => e.StudentUserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.InviteCodeId, e.StudentUserId })
                  .IsUnique()
                  .HasDatabaseName("IX_TutorInviteCodeUsages_Code_Student");
        });

        // Seed exam types
        modelBuilder.Entity<ExamType>().HasData(
            new ExamType { Code = "SAT", Name = "SAT (Scholastic Assessment Test)" },
            new ExamType { Code = "TOEFL", Name = "TOEFL (Test of English as a Foreign Language)" },
            new ExamType { Code = "NUET", Name = "NUET (Nazarbayev University Entrance Test)" },
            new ExamType { Code = "IELTS", Name = "IELTS Academic" },
            new ExamType { Code = "CSCA", Name = "Gaokao (China College Admission)" }
        );
    }

    // ═══════════════════════════════════════════════════════
    //  AUTO-FILL AUDIT COLUMNS (OP-16)
    // ═══════════════════════════════════════════════════════

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                // Prevent overwriting CreatedAt on updates
                entry.Property(nameof(IAuditable.CreatedAt)).IsModified = false;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
