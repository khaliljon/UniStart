using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniStart.Models;

namespace UniStart.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Flashcards
        public DbSet<FlashcardSet> FlashcardSets { get; set; } = null!;
        public DbSet<Flashcard> Flashcards { get; set; } = null!;
        public DbSet<UserFlashcardProgress> UserFlashcardProgresses { get; set; } = null!;
        public DbSet<UserFlashcardSetAccess> UserFlashcardSetAccesses { get; set; } = null!;

        // Quizzes
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<QuizQuestion> QuizQuestions { get; set; } = null!;
        public DbSet<QuizAnswer> QuizAnswers { get; set; } = null!;
        public DbSet<UserQuizAttempt> UserQuizAttempts { get; set; } = null!;
        public DbSet<UserQuizAnswer> UserQuizAnswers { get; set; } = null!;

        // Exams
        public DbSet<Exam> Exams { get; set; } = null!;
        public DbSet<ExamQuestion> ExamQuestions { get; set; } = null!;
        public DbSet<ExamAnswer> ExamAnswers { get; set; } = null!;
        public DbSet<UserExamAttempt> UserExamAttempts { get; set; } = null!;
        public DbSet<UserExamAnswer> UserExamAnswers { get; set; } = null!;

        // Tags & Achievements
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<Achievement> Achievements { get; set; } = null!;
        public DbSet<UserAchievement> UserAchievements { get; set; } = null!;

        // Reviews
        public DbSet<QuizReview> QuizReviews { get; set; } = null!;
        public DbSet<FlashcardSetReview> FlashcardSetReviews { get; set; } = null!;

        // Streaks
        public DbSet<UserStreak> UserStreaks { get; set; } = null!;

        // Social
        public DbSet<UserFollow> UserFollows { get; set; } = null!;
        public DbSet<ActivityFeed> ActivityFeeds { get; set; } = null!;

        // Subjects and new models
        public DbSet<Subject> Subjects { get; set; } = null!;
        public DbSet<Country> Countries { get; set; } = null!;
        public DbSet<University> Universities { get; set; } = null!;
        public DbSet<ExamType> ExamTypes { get; set; } = null!;
        
        // Learning hierarchy
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<LearningModule> LearningModules { get; set; } = null!;
        public DbSet<LearningCompetency> LearningCompetencies { get; set; } = null!;
        public DbSet<LearningTopic> LearningTopics { get; set; } = null!;
        public DbSet<TheoryContent> TheoryContents { get; set; } = null!;
        
        // System Settings
        public DbSet<SystemSettings> SystemSettings { get; set; } = null!;
        public DbSet<UserLearningPattern> UserLearningPatterns { get; set; } = null!;
        public DbSet<UniversityRecommendation> UniversityRecommendations { get; set; } = null!;
        public DbSet<UserPreferences> UserPreferences { get; set; } = null!;

        // AI & Machine Learning
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Flashcards
            modelBuilder.Entity<Flashcard>()
                .HasOne(f => f.FlashcardSet)
                .WithMany(fs => fs.Flashcards)
                .HasForeignKey(f => f.FlashcardSetId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserFlashcardProgress
            modelBuilder.Entity<UserFlashcardProgress>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFlashcardProgress>()
                .HasOne(p => p.Flashcard)
                .WithMany()
                .HasForeignKey(p => p.FlashcardId)
                .OnDelete(DeleteBehavior.Cascade);

            // ������������: ���� ������������ - ���� ������ ��������� �� ��������
            modelBuilder.Entity<UserFlashcardProgress>()
                .HasIndex(p => new { p.UserId, p.FlashcardId })
                .IsUnique();

            // UserFlashcardSetAccess
            modelBuilder.Entity<UserFlashcardSetAccess>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFlashcardSetAccess>()
                .HasOne(a => a.FlashcardSet)
                .WithMany()
                .HasForeignKey(a => a.FlashcardSetId)
                .OnDelete(DeleteBehavior.Cascade);

            // ������������: ���� ������������ - ���� ������ ������� � ������
            modelBuilder.Entity<UserFlashcardSetAccess>()
                .HasIndex(a => new { a.UserId, a.FlashcardSetId })
                .IsUnique();

            // Quizzes
            modelBuilder.Entity<QuizQuestion>()
                .HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizAnswer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserQuizAttempt>()
                .HasOne(ua => ua.Quiz)
                .WithMany(q => q.Attempts)
                .HasForeignKey(ua => ua.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserQuizAnswer
            modelBuilder.Entity<UserQuizAnswer>()
                .HasOne(ua => ua.Attempt)
                .WithMany(a => a.UserAnswers)
                .HasForeignKey(ua => ua.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserQuizAnswer>()
                .HasOne(ua => ua.Question)
                .WithMany()
                .HasForeignKey(ua => ua.QuestionId)
                .OnDelete(DeleteBehavior.Restrict); // �� ������� ������, ���� ���� ������

            modelBuilder.Entity<UserQuizAnswer>()
                .HasOne(ua => ua.SelectedAnswer)
                .WithMany()
                .HasForeignKey(ua => ua.SelectedAnswerId)
                .OnDelete(DeleteBehavior.Restrict); // �� ������� �����, ���� ���� ����� ������������

            // Exams
            modelBuilder.Entity<ExamQuestion>()
                .HasOne(eq => eq.Exam)
                .WithMany(e => e.Questions)
                .HasForeignKey(eq => eq.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExamAnswer>()
                .HasOne(ea => ea.Question)
                .WithMany(eq => eq.Answers)
                .HasForeignKey(ea => ea.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserExamAttempt>()
                .HasOne(uea => uea.Exam)
                .WithMany(e => e.Attempts)
                .HasForeignKey(uea => uea.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserExamAnswer>()
                .HasOne(uea => uea.Attempt)
                .WithMany(ua => ua.UserAnswers)
                .HasForeignKey(uea => uea.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserExamAnswer>()
                .HasOne(uea => uea.Question)
                .WithMany()
                .HasForeignKey(uea => uea.QuestionId)
                .OnDelete(DeleteBehavior.Restrict); // �� ������� ������, ���� ���� ������

            modelBuilder.Entity<UserExamAnswer>()
                .HasOne(uea => uea.SelectedAnswer)
                .WithMany()
                .HasForeignKey(uea => uea.SelectedAnswerId)
                .OnDelete(DeleteBehavior.Restrict); // �� ������� �����, ���� ���� ����� ������������

            // New models configuration
            modelBuilder.Entity<University>()
                .HasOne(u => u.Country)
                .WithMany(c => c.Universities)
                .HasForeignKey(u => u.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamType>()
                .HasOne(et => et.DefaultCountry)
                .WithMany()
                .HasForeignKey(et => et.DefaultCountryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            // Flashcard.NextReviewDate ������ �� ������������ (���������� � UserFlashcardProgress)
            // ��������� ��� �������� �������������, �� �� �����������

            modelBuilder.Entity<UserQuizAttempt>()
                .HasIndex(ua => ua.UserId);

            modelBuilder.Entity<UserExamAttempt>()
                .HasIndex(uea => uea.UserId);

            // ������� ��� ����� ������
            modelBuilder.Entity<UserFlashcardProgress>()
                .HasIndex(p => p.UserId);

            modelBuilder.Entity<UserFlashcardProgress>()
                .HasIndex(p => p.NextReviewDate);

            modelBuilder.Entity<UserFlashcardProgress>()
                .HasIndex(p => p.LastReviewedAt);

            modelBuilder.Entity<UserFlashcardProgress>()
                .HasIndex(p => p.IsMastered);

            modelBuilder.Entity<UserFlashcardSetAccess>()
                .HasIndex(a => a.UserId);

            modelBuilder.Entity<UserFlashcardSetAccess>()
                .HasIndex(a => a.FlashcardSetId);

            modelBuilder.Entity<UserFlashcardSetAccess>()
                .HasIndex(a => a.IsCompleted);

            modelBuilder.Entity<UserFlashcardSetAccess>()
                .HasIndex(a => a.LastAccessedAt);

            modelBuilder.Entity<UserQuizAnswer>()
                .HasIndex(a => a.AttemptId);

            modelBuilder.Entity<UserQuizAnswer>()
                .HasIndex(a => a.QuestionId);

            // Many-to-many: Exam - Subject
            modelBuilder.Entity<Exam>()
                .HasMany(e => e.Subjects)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "ExamSubject",
                    j => j.HasOne<Subject>().WithMany().HasForeignKey("SubjectsId"),
                    j => j.HasOne<Exam>().WithMany().HasForeignKey("ExamsId"),
                    j => j.HasKey("ExamsId", "SubjectsId"));
            
            // Many-to-many: Quiz - Subject
            modelBuilder.Entity<Quiz>()
                .HasMany(q => q.Subjects)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "QuizSubject",
                    j => j.HasOne<Subject>().WithMany().HasForeignKey("SubjectsId"),
                    j => j.HasOne<Quiz>().WithMany().HasForeignKey("QuizzesId"),
                    j => j.HasKey("QuizzesId", "SubjectsId"));
            
            // Many-to-many: FlashcardSet - Subject
            modelBuilder.Entity<FlashcardSet>()
                .HasMany(fs => fs.Subjects)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "FlashcardSetSubject",
                    j => j.HasOne<Subject>().WithMany().HasForeignKey("SubjectsId"),
                    j => j.HasOne<FlashcardSet>().WithMany().HasForeignKey("FlashcardSetsId"),
                    j => j.HasKey("FlashcardSetsId", "SubjectsId"));
            
            // UserPreferences - User (One-to-One)
            modelBuilder.Entity<UserPreferences>()
                .HasOne(up => up.User)
                .WithOne()
                .HasForeignKey<UserPreferences>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // UserPreferences - TargetUniversity (Many-to-One)
            modelBuilder.Entity<UserPreferences>()
                .HasOne(up => up.TargetUniversity)
                .WithMany()
                .HasForeignKey(up => up.TargetUniversityId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Many-to-many: UserPreferences - InterestedSubjects
            modelBuilder.Entity<UserPreferences>()
                .HasMany(up => up.InterestedSubjects)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "UserPreferencesInterestedSubject",
                    j => j.HasOne<Subject>().WithMany().HasForeignKey("SubjectsId"),
                    j => j.HasOne<UserPreferences>().WithMany().HasForeignKey("UserPreferencesId"),
                    j => j.HasKey("UserPreferencesId", "SubjectsId"));
            
            // Many-to-many: UserPreferences - StrongSubjects
            modelBuilder.Entity<UserPreferences>()
                .HasMany(up => up.StrongSubjects)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "UserPreferencesStrongSubject",
                    j => j.HasOne<Subject>().WithMany().HasForeignKey("SubjectsId"),
                    j => j.HasOne<UserPreferences>().WithMany().HasForeignKey("UserPreferencesId"),
                    j => j.HasKey("UserPreferencesId", "SubjectsId"));
            
            // Many-to-many: UserPreferences - WeakSubjects
            modelBuilder.Entity<UserPreferences>()
                .HasMany(up => up.WeakSubjects)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "UserPreferencesWeakSubject",
                    j => j.HasOne<Subject>().WithMany().HasForeignKey("SubjectsId"),
                    j => j.HasOne<UserPreferences>().WithMany().HasForeignKey("UserPreferencesId"),
                    j => j.HasKey("UserPreferencesId", "SubjectsId"));
            
            // Learning hierarchy configuration
            // Course -> Subject
            modelBuilder.Entity<Subject>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Subjects)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.SetNull); // Subject ����� ������������ ��� �����
            
            // Subject -> Module
            modelBuilder.Entity<LearningModule>()
                .HasOne(lm => lm.Subject)
                .WithMany(s => s.LearningModules)
                .HasForeignKey(lm => lm.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<LearningModule>()
                .HasOne(lm => lm.CaseStudyQuiz)
                .WithMany()
                .HasForeignKey(lm => lm.CaseStudyQuizId)
                .OnDelete(DeleteBehavior.SetNull);
            
            modelBuilder.Entity<LearningModule>()
                .HasOne(lm => lm.ModuleFinalQuiz)
                .WithMany()
                .HasForeignKey(lm => lm.ModuleFinalQuizId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Module -> Competency
            modelBuilder.Entity<LearningCompetency>()
                .HasOne(lc => lc.LearningModule)
                .WithMany(lm => lm.Competencies)
                .HasForeignKey(lc => lc.LearningModuleId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Competency -> Topic
            modelBuilder.Entity<LearningTopic>()
                .HasOne(lt => lt.LearningCompetency)
                .WithMany(lc => lc.Topics)
                .HasForeignKey(lt => lt.LearningCompetencyId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<LearningTopic>()
                .HasOne(lt => lt.PracticeQuiz)
                .WithMany()
                .HasForeignKey(lt => lt.PracticeQuizId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Topic -> Theory (���� � ������)
            modelBuilder.Entity<TheoryContent>()
                .HasOne(tc => tc.LearningTopic)
                .WithOne(lt => lt.Theory)
                .HasForeignKey<TheoryContent>(tc => tc.LearningTopicId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // SystemSettings - ������ ���� ������
            modelBuilder.Entity<SystemSettings>()
                .HasKey(s => s.Id);
            
            modelBuilder.Entity<SystemSettings>()
                .HasData(new SystemSettings
                {
                    Id = 1,
                    SiteName = "UniStart",
                    SiteDescription = "��������������� ��������� ��� �������� � ������� �������� � ������",
                    AllowRegistration = true,
                    RequireEmailVerification = false,
                    MaxQuizAttempts = 3,
                    SessionTimeout = 30,
                    EnableNotifications = true,
                    UpdatedAt = DateTime.UtcNow
                });
        }
    }
}
