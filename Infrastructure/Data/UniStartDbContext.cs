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
        });

        // UserSkillProfile configuration (composite key)
        modelBuilder.Entity<UserSkillProfile>(entity =>
        {
            entity.ToTable("UserSkillProfiles");
            entity.HasKey(e => new { e.UserId, e.SkillId });
            entity.HasOne(e => e.User)
                  .WithMany(u => u.SkillProfiles)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Skill)
                  .WithMany(s => s.UserProfiles)
                  .HasForeignKey(e => e.SkillId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed exam types
        modelBuilder.Entity<ExamType>().HasData(
            new ExamType { Code = "SAT", Name = "SAT (Scholastic Assessment Test)" },
            new ExamType { Code = "TOEFL", Name = "TOEFL (Test of English as a Foreign Language)" },
            new ExamType { Code = "NUET", Name = "NUET (Nazarbayev University Entrance Test)" }
        );
    }
}
