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
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizAnswer> QuizAnswers { get; set; }
        public DbSet<UserQuizAttempt> UserQuizAttempts { get; set; }
        
        // Exams
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }
        public DbSet<ExamAnswer> ExamAnswers { get; set; }
        public DbSet<UserExamAttempt> UserExamAttempts { get; set; }
        public DbSet<UserExamAnswer> UserExamAnswers { get; set; }
        
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

        public DbSet<Subject> Subjects { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Р’РђР–РќРћ РґР»СЏ Identity!

            // РљРѕРЅС„РёРіСѓСЂР°С†РёСЏ Flashcards
            modelBuilder.Entity<Flashcard>()
                .HasOne(f => f.FlashcardSet)
                .WithMany(fs => fs.Flashcards)
                .HasForeignKey(f => f.FlashcardSetId)
                .OnDelete(DeleteBehavior.Cascade);

            // РљРѕРЅС„РёРіСѓСЂР°С†РёСЏ Quiz -> Questions
            modelBuilder.Entity<QuizQuestion>()
                .HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // РљРѕРЅС„РёРіСѓСЂР°С†РёСЏ Question -> Answers
            modelBuilder.Entity<QuizAnswer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // РљРѕРЅС„РёРіСѓСЂР°С†РёСЏ Quiz -> UserQuizAttempts
            modelBuilder.Entity<UserQuizAttempt>()
                .HasOne(ua => ua.Quiz)
                .WithMany(q => q.Attempts)
                .HasForeignKey(ua => ua.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // РљРѕРЅС„РёРіСѓСЂР°С†РёСЏ Test -> ExamQuestions
            modelBuilder.Entity<ExamQuestion>()
                .HasOne(eq => eq.Exam)
                .WithMany(e => e.Questions)
                .HasForeignKey(eq => eq.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            // РљРѕРЅС„РёРіСѓСЂР°С†РёСЏ ExamQuestion -> ExamAnswers
            modelBuilder.Entity<ExamAnswer>()
                .HasOne(ta => ta.Question)
                .WithMany(tq => tq.Answers)
                .HasForeignKey(ta => ta.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // РљРѕРЅС„РёРіСѓСЂР°С†РёСЏ Test -> UserExamAttempts
            modelBuilder.Entity<UserExamAttempt>()
                .HasOne(uea => uea.Exam)
                .WithMany(t => t.Attempts)
                .HasForeignKey(uea => uea.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            // РљРѕРЅС„РёРіСѓСЂР°С†РёСЏ UserExamAttempt -> UserExamAnswers
            modelBuilder.Entity<UserExamAnswer>()
                .HasOne(uta => uta.Attempt)
                .WithMany(a => a.UserAnswers)
                .HasForeignKey(uta => uta.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            // РљРѕРЅС„РёРіСѓСЂР°С†РёСЏ Exam -> ExamQuestions
            modelBuilder.Entity<ExamQuestion>()
                .HasOne(eq => eq.Exam)
                .WithMany(e => e.Questions)
                .HasForeignKey(eq => eq.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            // РљРѕРЅС„РёРіСѓСЂР°С†РёСЏ ExamQuestion -> ExamAnswers
            modelBuilder.Entity<ExamAnswer>()
                .HasOne(ea => ea.Question)
                .WithMany(eq => eq.Answers)
                .HasForeignKey(ea => ea.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // РљРѕРЅС„РёРіСѓСЂР°С†РёСЏ Exam -> UserExamAttempts
            modelBuilder.Entity<UserExamAttempt>()
                .HasOne(uea => uea.Exam)
                .WithMany(e => e.Attempts)
                .HasForeignKey(uea => uea.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            // РљРѕРЅС„РёРіСѓСЂР°С†РёСЏ UserExamAttempt -> UserExamAnswers
            modelBuilder.Entity<UserExamAnswer>()
                .HasOne(uea => uea.Attempt)
                .WithMany(a => a.UserAnswers)
                .HasForeignKey(uea => uea.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            // РРЅРґРµРєСЃС‹ РґР»СЏ РїСЂРѕРёР·РІРѕРґРёС‚РµР»СЊРЅРѕСЃС‚Рё
            modelBuilder.Entity<Flashcard>()
                .HasIndex(f => f.NextReviewDate);

            modelBuilder.Entity<UserQuizAttempt>()
                .HasIndex(ua => ua.UserId);

            modelBuilder.Entity<UserExamAttempt>()
                .HasIndex(uea => uea.UserId);
        }
    }
}


