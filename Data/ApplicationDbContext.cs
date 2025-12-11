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

        // Quizzes
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<QuizQuestion> QuizQuestions { get; set; } = null!;
        public DbSet<QuizAnswer> QuizAnswers { get; set; } = null!;
        public DbSet<UserQuizAttempt> UserQuizAttempts { get; set; } = null!;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Flashcards
            modelBuilder.Entity<Flashcard>()
                .HasOne(f => f.FlashcardSet)
                .WithMany(fs => fs.Flashcards)
                .HasForeignKey(f => f.FlashcardSetId)
                .OnDelete(DeleteBehavior.Cascade);

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
            modelBuilder.Entity<Flashcard>()
                .HasIndex(f => f.NextReviewDate);

            modelBuilder.Entity<UserQuizAttempt>()
                .HasIndex(ua => ua.UserId);

            modelBuilder.Entity<UserExamAttempt>()
                .HasIndex(uea => uea.UserId);

            // Many-to-many: Exam - Subject
            modelBuilder.Entity<Exam>()
                .HasMany(e => e.Subjects)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "ExamSubject",
                    j => j.HasOne<Subject>().WithMany().HasForeignKey("SubjectsId"),
                    j => j.HasOne<Exam>().WithMany().HasForeignKey("ExamsId"),
                    j => j.HasKey("ExamsId", "SubjectsId"));
            
            // Learning hierarchy configuration
            // Course -> Subject
            modelBuilder.Entity<Subject>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Subjects)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.SetNull); // Subject может существовать без курса
            
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
            
            // Topic -> Theory (один к одному)
            modelBuilder.Entity<TheoryContent>()
                .HasOne(tc => tc.LearningTopic)
                .WithOne(lt => lt.Theory)
                .HasForeignKey<TheoryContent>(tc => tc.LearningTopicId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}