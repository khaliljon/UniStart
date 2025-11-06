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
        public DbSet<FlashcardSet> FlashcardSets { get; set; }
        public DbSet<Flashcard> Flashcards { get; set; }
        
        // Quizzes
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<UserQuizAttempt> UserQuizAttempts { get; set; }
        
        // Tests
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestQuestion> TestQuestions { get; set; }
        public DbSet<TestAnswer> TestAnswers { get; set; }
        public DbSet<UserTestAttempt> UserTestAttempts { get; set; }
        public DbSet<UserTestAnswer> UserTestAnswers { get; set; }
        
        // Tags & Achievements
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        
        // Reviews
        public DbSet<QuizReview> QuizReviews { get; set; }
        public DbSet<FlashcardSetReview> FlashcardSetReviews { get; set; }
        
        // Streaks
        public DbSet<UserStreak> UserStreaks { get; set; }
        
        // Social
        public DbSet<UserFollow> UserFollows { get; set; }
        public DbSet<ActivityFeed> ActivityFeeds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // ВАЖНО для Identity!

            // Конфигурация Flashcards
            modelBuilder.Entity<Flashcard>()
                .HasOne(f => f.FlashcardSet)
                .WithMany(fs => fs.Flashcards)
                .HasForeignKey(f => f.FlashcardSetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Конфигурация Quiz -> Questions
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // Конфигурация Question -> Answers
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Конфигурация Quiz -> UserQuizAttempts
            modelBuilder.Entity<UserQuizAttempt>()
                .HasOne(ua => ua.Quiz)
                .WithMany(q => q.Attempts)
                .HasForeignKey(ua => ua.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // Конфигурация Test -> TestQuestions
            modelBuilder.Entity<TestQuestion>()
                .HasOne(tq => tq.Test)
                .WithMany(t => t.Questions)
                .HasForeignKey(tq => tq.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Конфигурация TestQuestion -> TestAnswers
            modelBuilder.Entity<TestAnswer>()
                .HasOne(ta => ta.Question)
                .WithMany(tq => tq.Answers)
                .HasForeignKey(ta => ta.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Конфигурация Test -> UserTestAttempts
            modelBuilder.Entity<UserTestAttempt>()
                .HasOne(uta => uta.Test)
                .WithMany(t => t.Attempts)
                .HasForeignKey(uta => uta.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Конфигурация UserTestAttempt -> UserTestAnswers
            modelBuilder.Entity<UserTestAnswer>()
                .HasOne(uta => uta.Attempt)
                .WithMany(a => a.UserAnswers)
                .HasForeignKey(uta => uta.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            // Индексы для производительности
            modelBuilder.Entity<Flashcard>()
                .HasIndex(f => f.NextReviewDate);

            modelBuilder.Entity<UserQuizAttempt>()
                .HasIndex(ua => ua.UserId);

            modelBuilder.Entity<UserTestAttempt>()
                .HasIndex(uta => uta.UserId);
        }
    }
}
