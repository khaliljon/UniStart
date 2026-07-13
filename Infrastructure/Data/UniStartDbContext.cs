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

    public DbSet<TsaClassification> TsaClassifications => Set<TsaClassification>();

    // Referral program (Sprint 8)
    public DbSet<ReferralCode> ReferralCodes => Set<ReferralCode>();
    public DbSet<ReferralUsage> ReferralUsages => Set<ReferralUsage>();
    public DbSet<ReferralReward> ReferralRewards => Set<ReferralReward>();

    // Editable legal documents (privacy / terms / referral)
    public DbSet<LegalDocument> LegalDocuments => Set<LegalDocument>();

    // CSCA news / information articles
    public DbSet<NewsArticle> NewsArticles => Set<NewsArticle>();

    // User order history (mock packages / books / courses)
    public DbSet<Purchase> Purchases => Set<Purchase>();

    // PDF study materials (textbooks per subject)
    public DbSet<StudyMaterial> StudyMaterials => Set<StudyMaterial>();

    // Telegram support bot tickets
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<SupportMessage> SupportMessages => Set<SupportMessage>();

    // Admin-editable key/value settings (pricing, etc.)
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(50).HasDefaultValue("");
            entity.Property(e => e.LastName).HasMaxLength(50).HasDefaultValue("");
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

        // Topic configuration
        modelBuilder.Entity<Topic>(entity =>
        {
            entity.ToTable("Topics");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.HasOne(e => e.Section)
                  .WithMany(s => s.Topics)
                  .HasForeignKey(e => e.SectionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Question configuration
        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("Questions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired();
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.DifficultyParam).HasDefaultValue(0.0);
            entity.Property(e => e.DiscriminationParam).HasDefaultValue(1.0);
            entity.Property(e => e.GuessParam).HasDefaultValue(0.25);
            entity.Property(e => e.ResponseCount).HasDefaultValue(0);
            entity.Property(e => e.IsCalibrated).HasDefaultValue(false);
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

            entity.Property(e => e.ImageUrl).HasMaxLength(1000);

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

        // TsaClassification configuration (per-question unit cache)
        modelBuilder.Entity<TsaClassification>(entity =>
        {
            entity.ToTable("TsaClassifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuestionHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.SkillName).IsRequired().HasMaxLength(300);
            entity.Property(e => e.TopicName).HasMaxLength(300);
            entity.Property(e => e.ExamSectionName).HasMaxLength(200);
            entity.Property(e => e.QuestionPreview).HasMaxLength(500);
            entity.HasIndex(e => e.QuestionHash).IsUnique().HasDatabaseName("IX_TsaClassifications_QuestionHash");
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

        // UserSkillProfile configuration (composite key: user + exam section)
        modelBuilder.Entity<UserSkillProfile>(entity =>
        {
            entity.ToTable("UserSkillProfiles");
            entity.HasKey(e => new { e.UserId, e.SectionId });
            entity.Property(e => e.Theta).HasDefaultValue(0.0);
            entity.Property(e => e.ThetaSE).HasDefaultValue(1.0);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.SkillProfiles)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Section)
                  .WithMany()
                  .HasForeignKey(e => e.SectionId)
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





        // ═══════════════════════════════════════════════════════
        //  TUTOR INVITE CODES (S-6)
        // ═══════════════════════════════════════════════════════



        // ═══════════════════════════════════════════════════════
        //  REFERRAL PROGRAM (Sprint 8)
        // ═══════════════════════════════════════════════════════

        modelBuilder.Entity<ReferralCode>(entity =>
        {
            entity.ToTable("ReferralCodes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(8);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UsedCount).HasDefaultValue(0);
            entity.HasOne(e => e.Owner)
                  .WithMany()
                  .HasForeignKey(e => e.OwnerUserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.Code)
                  .IsUnique()
                  .HasDatabaseName("IX_ReferralCodes_Code");
            entity.HasIndex(e => e.OwnerUserId)
                  .IsUnique()
                  .HasDatabaseName("IX_ReferralCodes_OwnerUserId");
        });

        modelBuilder.Entity<ReferralUsage>(entity =>
        {
            entity.ToTable("ReferralUsages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RewardGranted).HasDefaultValue(false);
            entity.HasOne(e => e.ReferralCode)
                  .WithMany()
                  .HasForeignKey(e => e.ReferralCodeId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ReferredUser)
                  .WithMany()
                  .HasForeignKey(e => e.ReferredUserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.ReferredUserId)
                  .IsUnique()
                  .HasDatabaseName("IX_ReferralUsages_ReferredUserId");
        });

        modelBuilder.Entity<ReferralReward>(entity =>
        {
            entity.ToTable("ReferralRewards");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RewardType).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.IsPaidOut).HasDefaultValue(false);
            entity.HasOne(e => e.Owner)
                  .WithMany()
                  .HasForeignKey(e => e.OwnerUserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Usage)
                  .WithOne()
                  .HasForeignKey<ReferralReward>(e => e.ReferralUsageId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasOne(e => e.ReferredByCode)
                  .WithMany()
                  .HasForeignKey(e => e.ReferredByCodeId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ═══════════════════════════════════════════════════════
        //  LEGAL DOCUMENTS (editable from admin panel)
        // ═══════════════════════════════════════════════════════
        modelBuilder.Entity<LegalDocument>(entity =>
        {
            entity.ToTable("LegalDocuments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.LastUpdatedLabel).HasMaxLength(100);
            entity.HasIndex(e => e.Slug)
                  .IsUnique()
                  .HasDatabaseName("IX_LegalDocuments_Slug");
        });

        modelBuilder.Entity<NewsArticle>(entity =>
        {
            entity.ToTable("NewsArticles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Summary).HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.HasIndex(e => new { e.IsPublished, e.PublishedAt })
                  .HasDatabaseName("IX_NewsArticles_Published");
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.ToTable("Purchases");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ItemType).IsRequired().HasMaxLength(30);
            entity.Property(e => e.ItemCode).IsRequired().HasMaxLength(60);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Subjects).HasMaxLength(200);
            entity.Property(e => e.Amount).HasColumnType("numeric(12,2)");
            entity.Property(e => e.Currency).HasMaxLength(8).HasDefaultValue("KZT");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Paid");
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_Purchases_UserId");
        });

        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.ToTable("SupportTickets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Open");
            entity.HasIndex(e => e.TelegramUserId).IsUnique().HasDatabaseName("IX_SupportTickets_TelegramUserId");
        });

        modelBuilder.Entity<SupportMessage>(entity =>
        {
            entity.ToTable("SupportMessages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Direction).IsRequired().HasMaxLength(4);
            entity.Property(e => e.Text).HasMaxLength(4096);
            entity.HasOne(e => e.Ticket)
                  .WithMany(t => t.Messages)
                  .HasForeignKey(e => e.TicketId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.GroupMessageId).HasDatabaseName("IX_SupportMessages_GroupMessageId");
        });

        // StudyMaterial configuration
        modelBuilder.Entity<StudyMaterial>(entity =>
        {
            entity.ToTable("StudyMaterials");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SubjectKey).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.PdfUrl).HasMaxLength(2000);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasIndex(e => e.SubjectKey);
        });

        // AppSetting configuration (key/value)
        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.ToTable("AppSettings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(60);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Key).IsUnique();
        });

        // CSCA-only platform: exam types are managed at runtime (seeder / admin),
        // no longer seeded via HasData. Legacy SAT/NUET seed rows are removed by the
        // RemoveNonCscaSeedExamTypes migration.
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
