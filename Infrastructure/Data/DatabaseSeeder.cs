using Microsoft.EntityFrameworkCore;
using UniStart.Application.Services;
using UniStart.Domain.Entities;

namespace UniStart.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly UniStartDbContext _context;

    public DatabaseSeeder(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Check if data already exists with correct structure
        var hasCorrectData = await _context.Topics
            .Include(t => t.Section)
            .AnyAsync(t => t.Section != null && t.Section.ExamTypeCode == "SAT" && t.Name == "Main Idea & Summary");

        // If data exists but is incorrect (old seeding), clear and reseed
        if (await _context.Topics.AnyAsync() && !hasCorrectData)
        {
            await ClearAllDataAsync();
        }

        // Check if questions need Explanation update - ALL questions should have Explanation
        var totalQuestions = await _context.Questions.CountAsync();
        var questionsWithExplanation = await _context.Questions.CountAsync(q => q.Explanation != null);
        
        // If questions exist but not all have Explanation, clear questions and reseed
        if (totalQuestions > 0 && questionsWithExplanation < totalQuestions)
        {
            _context.UserAnswers.RemoveRange(_context.UserAnswers);
            _context.AnswerOptions.RemoveRange(_context.AnswerOptions);
            _context.Questions.RemoveRange(_context.Questions);
            await _context.SaveChangesAsync();
        }

        // Seed ExamTypes
        if (!await _context.ExamTypes.AnyAsync())
        {
            await SeedExamTypesAsync();
        }
        // Add IELTS/CSCA exam types if they don't exist
        else
        {
            if (!await _context.ExamTypes.AnyAsync(e => e.Code == "IELTS"))
            {
                _context.ExamTypes.Add(new ExamType { Code = "IELTS", Name = "IELTS Academic (Listening/Reading/Writing/Speaking)" });
                await _context.SaveChangesAsync();
            }
            if (!await _context.ExamTypes.AnyAsync(e => e.Code == "CSCA"))
            {
                _context.ExamTypes.Add(new ExamType { Code = "CSCA", Name = "CSCA \u2014 China Scholastic Competency Assessment" });
                await _context.SaveChangesAsync();
            }
        }

        // Seed default test user
        if (!await _context.Users.AnyAsync())
        {
            await SeedTestUserAsync();
        }

        // Seed admin user
        if (!await _context.Users.AnyAsync(u => u.Role == UserRole.Admin))
        {
            await SeedAdminUserAsync();
        }

        // Seed ExamSections
        if (!await _context.ExamSections.AnyAsync())
        {
            await SeedExamSectionsAsync();
        }
        // Add IELTS/CSCA sections if they don't exist
        else if (!await _context.ExamSections.AnyAsync(s => s.ExamTypeCode == "IELTS"))
        {
            await SeedIeltsCscaSectionsAsync();
        }

        // Seed Skills
        if (!await _context.Skills.AnyAsync())
        {
            await SeedSkillsAsync();
        }
        // Add Physics/Chemistry skills if missing
        if (!await _context.Skills.AnyAsync(s => s.Code == "SK_PHYS"))
        {
            _context.Skills.AddRange(
                new Skill { Code = "SK_PHYS", Name = "Физика EN (Physics EN)", Description = "Механика, электричество, оптика, термодинамика" },
                new Skill { Code = "SK_CHEM", Name = "Химия EN (Chemistry EN)", Description = "Неорганическая, органическая, аналитическая химия" }
            );
            await _context.SaveChangesAsync();
        }
        // Add Chinese Technical/Humanitarian skills if missing
        if (!await _context.Skills.AnyAsync(s => s.Code == "SK_CN_TECH"))
        {
            _context.Skills.AddRange(
                new Skill { Code = "SK_CN_TECH", Name = "Китайский технический (Chinese Technical)", Description = "Техническая лексика, научные тексты на китайском языке" },
                new Skill { Code = "SK_CN_HUM", Name = "Китайский гуманитарный (Chinese Humanitarian)", Description = "Литература, история, культура на китайском языке" }
            );
            await _context.SaveChangesAsync();
        }
        // Add CN variant skills if missing (for 8-section CSCA)
        if (!await _context.Skills.AnyAsync(s => s.Code == "SK_MATH_CN"))
        {
            _context.Skills.AddRange(
                new Skill { Code = "SK_MATH_CN", Name = "Математика CN (Mathematics CN)", Description = "Алгебра, геометрия, анализ данных — на китайском языке" },
                new Skill { Code = "SK_PHYS_CN", Name = "Физика CN (Physics CN)", Description = "Механика, электричество, оптика, термодинамика — на китайском языке" },
                new Skill { Code = "SK_CHEM_CN", Name = "Химия CN (Chemistry CN)", Description = "Неорганическая, органическая, аналитическая химия — на китайском языке" }
            );
            await _context.SaveChangesAsync();
        }

        // Seed Topics
        if (!await _context.Topics.AnyAsync())
        {
            await SeedTopicsAsync();
        }
        // Add IELTS/CSCA topics if they don't exist
        else if (!await _context.Topics.AnyAsync(t => t.Section != null && t.Section.ExamTypeCode == "IELTS"))
        {
            await SeedIeltsCscaTopicsAsync();
        }

        // Restructure CSCA: create detailed topic hierarchy for Math/Physics/Chemistry
        var cscaChemSec = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Chemistry") && !s.Name.Contains("(CN)"));
        if (cscaChemSec == null || !await _context.Topics.AnyAsync(t => t.SectionId == cscaChemSec.Id))
        {
            await SeedCscaRestructureAsync();
        }

        // Seed Questions with AnswerOptions
        if (!await _context.Questions.AnyAsync())
        {
            await SeedQuestionsAsync();
        }

        // Seed CSCA Mathematics EN questions (20 chapters, ~250 questions)
        await new CscaMathEnQuestionSeeder(_context).SeedAsync();

        // Seed CSCA Physics EN questions (12 chapters, ~200 questions)
        await new CscaPhysicsEnQuestionSeeder(_context).SeedAsync();

        // Seed CSCA Chemistry EN questions (14 chapters, ~140 questions)
        await new CscaChemistryEnQuestionSeeder(_context).SeedAsync();

        // Seed CSCA Mathematics CN questions (10 topics, ~78 questions, Chinese)
        await new CscaMathChQuestionSeeder(_context).SeedAsync();

        // Seed CSCA Physics CN questions (10 topics, ~70 questions, Chinese)
        await new CscaPhysicsChQuestionSeeder(_context).SeedAsync();

        // Seed CSCA Chemistry CN questions (10 topics, ~72 questions, Chinese)
        await new CscaChemistryChQuestionSeeder(_context).SeedAsync();

        // Seed CSCA Chinese Humanitarian questions (8 chapters, ~75 questions, Chinese)
        await new CscaChineseHumQuestionSeeder(_context).SeedAsync();

        // Seed CSCA Chinese Technical questions (8 chapters, ~80 questions, Chinese)
        await new CscaChineseTechQuestionSeeder(_context).SeedAsync();

        // Update IRT parameters on existing questions that still have defaults
        await UpdateIrtParametersAsync();

        // Seed Topic Dependencies (knowledge graph)
        if (!await _context.TopicDependencies.AnyAsync())
        {
            await SeedTopicDependenciesAsync();
        }

        // Seed Learning Materials (Topic Lessons + Question Hints)
        if (!await _context.TopicLessons.AnyAsync())
        {
            await SeedTopicLessonsAsync();
        }

        // Seed hints for questions that don't have them
        if (!await _context.Questions.AnyAsync(q => q.Hint != null))
        {
            await SeedQuestionHintsAsync();
        }

        // Seed Reading Passages (TOEFL)
        if (!await _context.ReadingPassages.AnyAsync())
        {
            await SeedReadingPassagesAsync();
        }

        // Seed Mock Exams
        if (!await _context.MockExams.AnyAsync())
        {
            await SeedMockExamsAsync();
        }
        // Add IELTS/CSCA mocks if they don't exist yet
        else if (!await _context.MockExams.AnyAsync(m => m.ExamTypeCode == "IELTS"))
        {
            await SeedIeltsCscaMockExamsAsync();
        }

        // Fix existing CSCA mock exam: ensure it has 5 sections matching actual ExamSections
        await FixCscaMockExamSectionsAsync();

        // Seed tutor profiles, reviews, and conversations
        if (!await _context.TutorProfiles.AnyAsync())
        {
            await SeedTutorDataAsync();
        }

        // ─── Learning v2 seed data ────────────────────────────
        if (!await _context.FormulaCards.AnyAsync())
        {
            await SeedFormulaCardsAsync();
        }
        // Add IELTS/CSCA formulas if missing
        else if (!await _context.FormulaCards.AnyAsync(f => f.Topic.Section != null && f.Topic.Section.ExamTypeCode == "IELTS"))
        {
            await SeedIeltsCscaFormulaCardsAsync();
        }

        if (!await _context.FlashcardDecks.AnyAsync())
        {
            await SeedFlashcardDecksAsync();
        }
        // Add IELTS/CSCA flashcard decks if missing
        else if (!await _context.FlashcardDecks.AnyAsync(d => d.ExamTypeCode == "IELTS"))
        {
            await SeedIeltsCscaFlashcardDecksAsync();
        }

        if (!await _context.StrategyGuides.AnyAsync())
        {
            await SeedStrategyGuidesAsync();
        }
        // Add IELTS/CSCA strategy guides if missing
        else if (!await _context.StrategyGuides.AnyAsync(g => g.ExamTypeCode == "IELTS"))
        {
            await SeedIeltsCscaStrategyGuidesAsync();
        }

        // Seed default drill templates
        if (!await _context.DrillTemplates.AnyAsync())
        {
            await SeedDrillTemplatesAsync();
        }

        // Expand CSCA from 5 to 8 sections (EN/CN split) — must run last, after all name-based lookups
        await ExpandCscaTo8SectionsAsync();

        // Seed partner schools
        await SeedTutorSchoolsAsync();

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Clears all seed data to allow re-seeding with correct structure
    /// </summary>
    private async Task ClearAllDataAsync()
    {
        // Clear in order of dependencies
        _context.UserAnswers.RemoveRange(_context.UserAnswers);
        _context.UserSkillProfiles.RemoveRange(_context.UserSkillProfiles);
        _context.AnswerOptions.RemoveRange(_context.AnswerOptions);
        _context.Questions.RemoveRange(_context.Questions);
        _context.TopicDependencies.RemoveRange(_context.TopicDependencies);
        _context.Topics.RemoveRange(_context.Topics);
        _context.ExamSections.RemoveRange(_context.ExamSections);
        _context.Skills.RemoveRange(_context.Skills);
        _context.ExamTypes.RemoveRange(_context.ExamTypes);
        await _context.SaveChangesAsync();
    }

    private async Task SeedTestUserAsync()
    {
        // Create a default test user for development/testing
        var testUser = new User
        {
            Email = "test@unistart.kz",
            Name = "Test User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
            Role = UserRole.Student,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(testUser);
        await _context.SaveChangesAsync();
    }

    private async Task SeedAdminUserAsync()
    {
        var adminUser = new User
        {
            Email = "admin@unistart.kz",
            Name = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = UserRole.Admin,
            HasCompletedOnboarding = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(adminUser);
        await _context.SaveChangesAsync();
    }

    private async Task SeedExamTypesAsync()
    {
        var examTypes = new List<ExamType>
        {
            new ExamType { Code = "SAT", Name = "SAT (Английский + Математика)" },
            new ExamType { Code = "TOEFL", Name = "TOEFL (Чтение/Аудирование/Говорение/Письмо)" },
            new ExamType { Code = "NUET", Name = "NUET (Математика + Критическое мышление)" },
            new ExamType { Code = "IELTS", Name = "IELTS Academic (Listening/Reading/Writing/Speaking)" },
            new ExamType { Code = "CSCA", Name = "CSCA (Mathematics + Physics + Chemistry)" }
        };

        await _context.ExamTypes.AddRangeAsync(examTypes);
        await _context.SaveChangesAsync();
    }

    private async Task SeedExamSectionsAsync()
    {
        var sections = new List<ExamSection>
        {
            // SAT Sections
            new ExamSection { ExamTypeCode = "SAT", Name = "Reading & Writing", MinScore = 200, MaxScore = 800 },
            new ExamSection { ExamTypeCode = "SAT", Name = "Math (No Calculator)", MinScore = 200, MaxScore = 400 },
            new ExamSection { ExamTypeCode = "SAT", Name = "Math (Calculator)", MinScore = 200, MaxScore = 400 },

            // TOEFL Sections
            new ExamSection { ExamTypeCode = "TOEFL", Name = "Reading", MinScore = 0, MaxScore = 30 },
            new ExamSection { ExamTypeCode = "TOEFL", Name = "Listening", MinScore = 0, MaxScore = 30 },
            new ExamSection { ExamTypeCode = "TOEFL", Name = "Speaking", MinScore = 0, MaxScore = 30 },
            new ExamSection { ExamTypeCode = "TOEFL", Name = "Writing", MinScore = 0, MaxScore = 30 },

            // NUET Sections
            new ExamSection { ExamTypeCode = "NUET", Name = "Math", MinScore = 0, MaxScore = 140 },
            new ExamSection { ExamTypeCode = "NUET", Name = "Critical Thinking", MinScore = 0, MaxScore = 140 },

            // IELTS Sections (band scores 0–9)
            new ExamSection { ExamTypeCode = "IELTS", Name = "Listening", MinScore = 0, MaxScore = 9 },
            new ExamSection { ExamTypeCode = "IELTS", Name = "Reading", MinScore = 0, MaxScore = 9 },
            new ExamSection { ExamTypeCode = "IELTS", Name = "Writing", MinScore = 0, MaxScore = 9 },
            new ExamSection { ExamTypeCode = "IELTS", Name = "Speaking", MinScore = 0, MaxScore = 9 },

            // CSCA Sections (5 subjects, 0–100 each per official spec)
            new ExamSection { ExamTypeCode = "CSCA", Name = "Mathematics", MinScore = 0, MaxScore = 100 },
            new ExamSection { ExamTypeCode = "CSCA", Name = "Physics", MinScore = 0, MaxScore = 100 },
            new ExamSection { ExamTypeCode = "CSCA", Name = "Chemistry", MinScore = 0, MaxScore = 100 },
            new ExamSection { ExamTypeCode = "CSCA", Name = "Chinese Technical", MinScore = 0, MaxScore = 100 },
            new ExamSection { ExamTypeCode = "CSCA", Name = "Chinese Humanitarian", MinScore = 0, MaxScore = 100 }
        };

        await _context.ExamSections.AddRangeAsync(sections);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds only IELTS and CSCA exam sections for existing databases.
    /// </summary>
    private async Task SeedIeltsCscaSectionsAsync()
    {
        var sections = new List<ExamSection>
        {
            // IELTS Sections (band scores 0–9)
            new ExamSection { ExamTypeCode = "IELTS", Name = "Listening", MinScore = 0, MaxScore = 9 },
            new ExamSection { ExamTypeCode = "IELTS", Name = "Reading", MinScore = 0, MaxScore = 9 },
            new ExamSection { ExamTypeCode = "IELTS", Name = "Writing", MinScore = 0, MaxScore = 9 },
            new ExamSection { ExamTypeCode = "IELTS", Name = "Speaking", MinScore = 0, MaxScore = 9 },

            // CSCA Sections (5 subjects, 0–100 each per official spec)
            new ExamSection { ExamTypeCode = "CSCA", Name = "Mathematics", MinScore = 0, MaxScore = 100 },
            new ExamSection { ExamTypeCode = "CSCA", Name = "Physics", MinScore = 0, MaxScore = 100 },
            new ExamSection { ExamTypeCode = "CSCA", Name = "Chemistry", MinScore = 0, MaxScore = 100 },
            new ExamSection { ExamTypeCode = "CSCA", Name = "Chinese Technical", MinScore = 0, MaxScore = 100 },
            new ExamSection { ExamTypeCode = "CSCA", Name = "Chinese Humanitarian", MinScore = 0, MaxScore = 100 }
        };

        await _context.ExamSections.AddRangeAsync(sections);
        await _context.SaveChangesAsync();
    }

    private async Task SeedSkillsAsync()
    {
        var skills = new List<Skill>
        {
            new Skill { Code = "SK_READ", Name = "Чтение (Reading)", Description = "Понимание текстов и анализ информации" },
            new Skill { Code = "SK_WRITE", Name = "Письмо (Writing)", Description = "Грамматика, структура предложений, эссе" },
            new Skill { Code = "SK_LISTEN", Name = "Аудирование (Listening)", Description = "Понимание устной речи" },
            new Skill { Code = "SK_SPEAK", Name = "Говорение (Speaking)", Description = "Устная речь и произношение" },
            new Skill { Code = "SK_MATH", Name = "Математика EN (Mathematics EN)", Description = "Алгебра, геометрия, анализ данных" },
            new Skill { Code = "SK_CRIT", Name = "Критическое мышление (Critical Thinking)", Description = "Логика, анализ аргументов, решение проблем" },
            new Skill { Code = "SK_PHYS", Name = "Физика EN (Physics EN)", Description = "Механика, электричество, оптика, термодинамика" },
            new Skill { Code = "SK_CHEM", Name = "Химия EN (Chemistry EN)", Description = "Неорганическая, органическая, аналитическая химия" },
            new Skill { Code = "SK_CN_TECH", Name = "Китайский технический (Chinese Technical)", Description = "Техническая лексика, научные тексты на китайском языке" },
            new Skill { Code = "SK_CN_HUM", Name = "Китайский гуманитарный (Chinese Humanitarian)", Description = "Литература, история, культура на китайском языке" },
            new Skill { Code = "SK_MATH_CN", Name = "Математика CN (Mathematics CN)", Description = "Алгебра, геометрия, анализ данных — на китайском языке" },
            new Skill { Code = "SK_PHYS_CN", Name = "Физика CN (Physics CN)", Description = "Механика, электричество, оптика, термодинамика — на китайском языке" },
            new Skill { Code = "SK_CHEM_CN", Name = "Химия CN (Chemistry CN)", Description = "Неорганическая, органическая, аналитическая химия — на китайском языке" }
        };

        await _context.Skills.AddRangeAsync(skills);
        await _context.SaveChangesAsync();
    }

    private async Task SeedTopicsAsync()
    {
        // Get sections and skills for linking - use ExamTypeCode to avoid conflicts
        var satReadWrite = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "SAT" && s.Name == "Reading & Writing");
        var satMathNoCalc = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "SAT" && s.Name == "Math (No Calculator)");
        var satMathCalc = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "SAT" && s.Name == "Math (Calculator)");
        var toeflReading = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "TOEFL" && s.Name == "Reading");
        var toeflListening = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "TOEFL" && s.Name == "Listening");
        var toeflSpeaking = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "TOEFL" && s.Name == "Speaking");
        var toeflWriting = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "TOEFL" && s.Name == "Writing");
        var nuetMath = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "NUET" && s.Name == "Math");
        var nuetCritical = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "NUET" && s.Name == "Critical Thinking");
        var ieltsListening = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "IELTS" && s.Name == "Listening");
        var ieltsReading = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "IELTS" && s.Name == "Reading");
        var ieltsWriting = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "IELTS" && s.Name == "Writing");
        var ieltsSpeaking = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "IELTS" && s.Name == "Speaking");
        var cscaMath = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "CSCA" && s.Name == "Mathematics");
        var cscaPhys = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "CSCA" && s.Name == "Physics");

        var skillRead = await _context.Skills.FirstAsync(s => s.Code == "SK_READ");
        var skillWrite = await _context.Skills.FirstAsync(s => s.Code == "SK_WRITE");
        var skillListen = await _context.Skills.FirstAsync(s => s.Code == "SK_LISTEN");
        var skillSpeak = await _context.Skills.FirstAsync(s => s.Code == "SK_SPEAK");
        var skillMath = await _context.Skills.FirstAsync(s => s.Code == "SK_MATH");
        var skillCrit = await _context.Skills.FirstAsync(s => s.Code == "SK_CRIT");
        var skillPhys = await _context.Skills.FirstAsync(s => s.Code == "SK_PHYS");

        var topics = new List<Topic>
        {
            // SAT Reading & Writing Topics
            new Topic { Name = "Main Idea & Summary", SkillId = skillRead.Id, SectionId = satReadWrite.Id },
            new Topic { Name = "Grammar & Sentence Structure", SkillId = skillWrite.Id, SectionId = satReadWrite.Id },
            new Topic { Name = "Vocabulary in Context", SkillId = skillRead.Id, SectionId = satReadWrite.Id },

            // SAT Math Topics
            new Topic { Name = "Linear Equations", SkillId = skillMath.Id, SectionId = satMathNoCalc.Id },
            new Topic { Name = "Geometry", SkillId = skillMath.Id, SectionId = satMathNoCalc.Id },
            new Topic { Name = "Data Analysis", SkillId = skillMath.Id, SectionId = satMathCalc.Id },
            new Topic { Name = "Quadratic Equations", SkillId = skillMath.Id, SectionId = satMathCalc.Id },

            // TOEFL Topics
            new Topic { Name = "Academic Reading", SkillId = skillRead.Id, SectionId = toeflReading.Id },
            new Topic { Name = "Lecture Comprehension", SkillId = skillListen.Id, SectionId = toeflListening.Id },
            new Topic { Name = "Conversation Understanding", SkillId = skillListen.Id, SectionId = toeflListening.Id },
            new Topic { Name = "Independent Speaking", SkillId = skillSpeak.Id, SectionId = toeflSpeaking.Id },
            new Topic { Name = "Integrated Writing", SkillId = skillWrite.Id, SectionId = toeflWriting.Id },

            // NUET Topics
            new Topic { Name = "Algebra & Functions", SkillId = skillMath.Id, SectionId = nuetMath.Id },
            new Topic { Name = "Problem Solving", SkillId = skillMath.Id, SectionId = nuetMath.Id },
            new Topic { Name = "Logical Reasoning", SkillId = skillCrit.Id, SectionId = nuetCritical.Id },
            new Topic { Name = "Argument Analysis", SkillId = skillCrit.Id, SectionId = nuetCritical.Id },

            // IELTS Topics
            new Topic { Name = "IELTS Listening Comprehension", SkillId = skillListen.Id, SectionId = ieltsListening.Id },
            new Topic { Name = "IELTS Note & Form Completion", SkillId = skillListen.Id, SectionId = ieltsListening.Id },
            new Topic { Name = "IELTS Academic Reading", SkillId = skillRead.Id, SectionId = ieltsReading.Id },
            new Topic { Name = "IELTS Reading: Matching & True/False", SkillId = skillRead.Id, SectionId = ieltsReading.Id },
            new Topic { Name = "IELTS Task 1: Data Description", SkillId = skillWrite.Id, SectionId = ieltsWriting.Id },
            new Topic { Name = "IELTS Task 2: Essay Writing", SkillId = skillWrite.Id, SectionId = ieltsWriting.Id },
            new Topic { Name = "IELTS Speaking Parts 1 & 2", SkillId = skillSpeak.Id, SectionId = ieltsSpeaking.Id },
            new Topic { Name = "IELTS Speaking Part 3: Discussion", SkillId = skillSpeak.Id, SectionId = ieltsSpeaking.Id },

            // CSCA Topics — Math Ch.1 (detailed; remaining chapters seeded by SeedCscaRestructureAsync)
            new Topic { Name = "1.1.1 Elements of Sets", SkillId = skillMath.Id, SectionId = cscaMath.Id },
            new Topic { Name = "1.1.2 Common Number Sets", SkillId = skillMath.Id, SectionId = cscaMath.Id },
            new Topic { Name = "1.2.1 Intersection of Sets", SkillId = skillMath.Id, SectionId = cscaMath.Id },
            new Topic { Name = "1.2.2 Union of Sets", SkillId = skillMath.Id, SectionId = cscaMath.Id },
            new Topic { Name = "1.3.1 Necessary and Sufficient Conditions", SkillId = skillCrit.Id, SectionId = cscaMath.Id },

            // CSCA Topics — Physics placeholder
            new Topic { Name = "P1.1 Types of Forces", SkillId = skillPhys.Id, SectionId = cscaPhys.Id },
            new Topic { Name = "P1.2 Force Analysis", SkillId = skillPhys.Id, SectionId = cscaPhys.Id }
        };

        await _context.Topics.AddRangeAsync(topics);
        await _context.SaveChangesAsync();
    }

    private async Task SeedQuestionsAsync()
    {
        // Get topics for linking
        var mainIdea = await _context.Topics.FirstAsync(t => t.Name == "Main Idea & Summary");
        var grammar = await _context.Topics.FirstAsync(t => t.Name == "Grammar & Sentence Structure");
        var vocabulary = await _context.Topics.FirstAsync(t => t.Name == "Vocabulary in Context");
        var linearEq = await _context.Topics.FirstAsync(t => t.Name == "Linear Equations");
        var geometry = await _context.Topics.FirstAsync(t => t.Name == "Geometry");
        var dataAnalysis = await _context.Topics.FirstAsync(t => t.Name == "Data Analysis");
        var quadratic = await _context.Topics.FirstAsync(t => t.Name == "Quadratic Equations");
        var academicReading = await _context.Topics.FirstAsync(t => t.Name == "Academic Reading");
        var lectureComp = await _context.Topics.FirstAsync(t => t.Name == "Lecture Comprehension");
        var algebra = await _context.Topics.FirstAsync(t => t.Name == "Algebra & Functions");
        var problemSolving = await _context.Topics.FirstAsync(t => t.Name == "Problem Solving");
        var logicalReasoning = await _context.Topics.FirstAsync(t => t.Name == "Logical Reasoning");
        var argumentAnalysis = await _context.Topics.FirstAsync(t => t.Name == "Argument Analysis");
        var ieltsListenComp = await _context.Topics.FirstAsync(t => t.Name == "IELTS Listening Comprehension");
        var ieltsNoteForm = await _context.Topics.FirstAsync(t => t.Name == "IELTS Note & Form Completion");
        var ieltsAcadRead = await _context.Topics.FirstAsync(t => t.Name == "IELTS Academic Reading");
        var ieltsReadMatch = await _context.Topics.FirstAsync(t => t.Name == "IELTS Reading: Matching & True/False");
        var ieltsTask1 = await _context.Topics.FirstAsync(t => t.Name == "IELTS Task 1: Data Description");
        var ieltsTask2 = await _context.Topics.FirstAsync(t => t.Name == "IELTS Task 2: Essay Writing");
        var ieltsSpeaking12 = await _context.Topics.FirstAsync(t => t.Name == "IELTS Speaking Parts 1 & 2");
        var ieltsSpeaking3 = await _context.Topics.FirstAsync(t => t.Name == "IELTS Speaking Part 3: Discussion");
        var calculus = await _context.Topics.FirstAsync(t => t.Name == "Calculus & Analysis");
        var probStats = await _context.Topics.FirstAsync(t => t.Name == "Probability & Statistics");
        var discreteMath = await _context.Topics.FirstAsync(t => t.Name == "Discrete Mathematics");
        var formalLogic = await _context.Topics.FirstAsync(t => t.Name == "Formal Logic");
        var algoThinking = await _context.Topics.FirstAsync(t => t.Name == "Algorithmic Thinking");
        var dataInterp = await _context.Topics.FirstAsync(t => t.Name == "Data Interpretation");

        var questions = new List<Question>();

        // ================== SAT Reading & Writing Questions ==================

        // Q1 - Main Idea (Easy)
        var q1 = new Question
        {
            TopicId = mainIdea.Id,
            Text = "What is the main idea of the passage about economic growth?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -1.2,
            DiscriminationParam = 0.8,
            GuessParam = 0.25,
            Explanation = "The passage discusses how government policies directly influence economic indicators and growth rates, making 'Growth depends on policy' the central theme.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Growth is unpredictable", IsCorrect = false },
                new AnswerOption { Text = "Growth depends on policy", IsCorrect = true },
                new AnswerOption { Text = "Growth is linear", IsCorrect = false },
                new AnswerOption { Text = "Growth is irrelevant", IsCorrect = false }
            }
        };
        questions.Add(q1);

        // Q2 - Main Idea (Medium)
        var q2 = new Question
        {
            TopicId = mainIdea.Id,
            Text = "Based on the passage, which statement best summarizes the author's central argument about climate change?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.2,
            DiscriminationParam = 1.1,
            GuessParam = 0.25,
            Explanation = "The author emphasizes urgency and collective responsibility throughout the passage, which aligns with the need for immediate global action rather than passive or localized responses.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Climate change is a natural phenomenon", IsCorrect = false },
                new AnswerOption { Text = "Immediate global action is essential to mitigate climate change effects", IsCorrect = true },
                new AnswerOption { Text = "Technology alone can solve climate problems", IsCorrect = false },
                new AnswerOption { Text = "Climate change primarily affects coastal regions", IsCorrect = false }
            }
        };
        questions.Add(q2);

        // Q3 - Grammar (Easy)
        var q3 = new Question
        {
            TopicId = grammar.Id,
            Text = "Choose the correct form of the underlined word: \"The scientist's argument was _____...\"",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -1.0,
            DiscriminationParam = 0.7,
            GuessParam = 0.25,
            Explanation = "The sentence requires an adjective to describe the argument. 'Flawed' is the past participle used as an adjective, meaning having defects or errors.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "flawed", IsCorrect = true },
                new AnswerOption { Text = "flaw", IsCorrect = false },
                new AnswerOption { Text = "flawing", IsCorrect = false },
                new AnswerOption { Text = "flaws", IsCorrect = false }
            }
        };
        questions.Add(q3);

        // Q4 - Grammar (Medium)
        var q4 = new Question
        {
            TopicId = grammar.Id,
            Text = "Which version of the sentence is grammatically correct?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.3,
            DiscriminationParam = 1.0,
            GuessParam = 0.25,
            Explanation = "With 'neither...nor', the verb agrees with the noun closest to it. Since 'teacher' is singular, we use 'was'. Also, 'neither' always pairs with 'nor', not 'or'.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Neither the students nor the teacher were prepared.", IsCorrect = false },
                new AnswerOption { Text = "Neither the students nor the teacher was prepared.", IsCorrect = true },
                new AnswerOption { Text = "Neither the students or the teacher were prepared.", IsCorrect = false },
                new AnswerOption { Text = "Neither the students or the teacher was prepared.", IsCorrect = false }
            }
        };
        questions.Add(q4);

        // Q5 - Grammar (Hard)
        var q5 = new Question
        {
            TopicId = grammar.Id,
            Text = "Select the sentence that correctly uses parallel structure.",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.3,
            DiscriminationParam = 1.2,
            GuessParam = 0.25,
            Explanation = "Parallel structure requires consistency in form. 'Hiking, swimming, and riding' are all gerunds (-ing forms), maintaining grammatical parallelism throughout the list.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "She likes hiking, swimming, and to ride bikes.", IsCorrect = false },
                new AnswerOption { Text = "She likes hiking, swimming, and riding bikes.", IsCorrect = true },
                new AnswerOption { Text = "She likes to hike, swimming, and riding bikes.", IsCorrect = false },
                new AnswerOption { Text = "She likes hiking, to swim, and bike riding.", IsCorrect = false }
            }
        };
        questions.Add(q5);

        // Q6 - Vocabulary (Medium)
        var q6 = new Question
        {
            TopicId = vocabulary.Id,
            Text = "In the context of the passage, the word \"ubiquitous\" most nearly means:",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.1,
            DiscriminationParam = 0.9,
            GuessParam = 0.25,
            Explanation = "'Ubiquitous' comes from Latin 'ubique' meaning 'everywhere'. It describes something that is present, appears, or is encountered everywhere.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "rare", IsCorrect = false },
                new AnswerOption { Text = "everywhere", IsCorrect = true },
                new AnswerOption { Text = "dangerous", IsCorrect = false },
                new AnswerOption { Text = "expensive", IsCorrect = false }
            }
        };
        questions.Add(q6);

        // ================== SAT Math Questions ==================

        // Q7 - Linear Equations (Easy)
        var q7 = new Question
        {
            TopicId = linearEq.Id,
            Text = "If 3x + 4 = 19, what is x?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -1.5,
            DiscriminationParam = 0.9,
            GuessParam = 0.25,
            Explanation = "Subtract 4 from both sides: 3x = 15. Then divide by 3: x = 5.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "5", IsCorrect = true },
                new AnswerOption { Text = "4", IsCorrect = false },
                new AnswerOption { Text = "6", IsCorrect = false },
                new AnswerOption { Text = "3", IsCorrect = false }
            }
        };
        questions.Add(q7);

        // Q8 - Linear Equations (Medium)
        var q8 = new Question
        {
            TopicId = linearEq.Id,
            Text = "If 2(x - 3) + 5 = 3x - 4, what is x?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.0,
            DiscriminationParam = 1.1,
            GuessParam = 0.25,
            Explanation = "Expand: 2x - 6 + 5 = 3x - 4. Simplify: 2x - 1 = 3x - 4. Subtract 2x: -1 = x - 4. Add 4: x = 3.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "3", IsCorrect = true },
                new AnswerOption { Text = "2", IsCorrect = false },
                new AnswerOption { Text = "4", IsCorrect = false },
                new AnswerOption { Text = "1", IsCorrect = false }
            }
        };
        questions.Add(q8);

        // Q9 - Geometry (Easy)
        var q9 = new Question
        {
            TopicId = geometry.Id,
            Text = "A triangle has sides 5, 12, and 13. What is its area?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -0.8,
            DiscriminationParam = 0.8,
            GuessParam = 0.25,
            Explanation = "This is a right triangle (5² + 12² = 25 + 144 = 169 = 13²). Area = (1/2) × base × height = (1/2) × 5 × 12 = 30.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "30", IsCorrect = true },
                new AnswerOption { Text = "60", IsCorrect = false },
                new AnswerOption { Text = "24", IsCorrect = false },
                new AnswerOption { Text = "35", IsCorrect = false }
            }
        };
        questions.Add(q9);

        // Q10 - Geometry (Medium)
        var q10 = new Question
        {
            TopicId = geometry.Id,
            Text = "A circle has a circumference of 31.4 cm. What is its approximate radius?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.1,
            DiscriminationParam = 1.0,
            GuessParam = 0.25,
            Explanation = "Circumference = 2πr. So r = C/(2π) = 31.4/(2×3.14) = 31.4/6.28 = 5 cm.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "5 cm", IsCorrect = true },
                new AnswerOption { Text = "10 cm", IsCorrect = false },
                new AnswerOption { Text = "15 cm", IsCorrect = false },
                new AnswerOption { Text = "2.5 cm", IsCorrect = false }
            }
        };
        questions.Add(q10);

        // Q11 - Geometry (Hard)
        var q11 = new Question
        {
            TopicId = geometry.Id,
            Text = "In a right triangle, one leg is 8 and the hypotenuse is 17. What is the length of the other leg?",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.0,
            DiscriminationParam = 1.3,
            GuessParam = 0.25,
            Explanation = "Using Pythagorean theorem: a² + b² = c². So 8² + b² = 17². 64 + b² = 289. b² = 225. b = 15.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "15", IsCorrect = true },
                new AnswerOption { Text = "9", IsCorrect = false },
                new AnswerOption { Text = "12", IsCorrect = false },
                new AnswerOption { Text = "13", IsCorrect = false }
            }
        };
        questions.Add(q11);

        // Q12 - Quadratic (Medium)
        var q12 = new Question
        {
            TopicId = quadratic.Id,
            Text = "What are the solutions to x² - 5x + 6 = 0?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = -0.1,
            DiscriminationParam = 1.0,
            GuessParam = 0.25,
            Explanation = "Factor the quadratic: (x - 2)(x - 3) = 0. Set each factor to zero: x - 2 = 0 gives x = 2, x - 3 = 0 gives x = 3.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "x = 2 and x = 3", IsCorrect = true },
                new AnswerOption { Text = "x = 1 and x = 6", IsCorrect = false },
                new AnswerOption { Text = "x = -2 and x = -3", IsCorrect = false },
                new AnswerOption { Text = "x = 5 and x = 1", IsCorrect = false }
            }
        };
        questions.Add(q12);

        // Q13 - Quadratic (Hard)
        var q13 = new Question
        {
            TopicId = quadratic.Id,
            Text = "For the equation x² + 6x + k = 0 to have exactly one solution, what must k equal?",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.6,
            DiscriminationParam = 1.4,
            GuessParam = 0.25,
            Explanation = "For exactly one solution, discriminant must equal 0: b² - 4ac = 0. So 36 - 4(1)(k) = 0. 36 = 4k. k = 9.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "9", IsCorrect = true },
                new AnswerOption { Text = "6", IsCorrect = false },
                new AnswerOption { Text = "3", IsCorrect = false },
                new AnswerOption { Text = "12", IsCorrect = false }
            }
        };
        questions.Add(q13);

        // Q14 - Data Analysis (Easy)
        var q14 = new Question
        {
            TopicId = dataAnalysis.Id,
            Text = "What is the mean of the numbers: 4, 8, 6, 10, 12?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -1.3,
            DiscriminationParam = 0.7,
            GuessParam = 0.25,
            Explanation = "Mean = sum of values / count. (4 + 8 + 6 + 10 + 12) / 5 = 40 / 5 = 8.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "8", IsCorrect = true },
                new AnswerOption { Text = "6", IsCorrect = false },
                new AnswerOption { Text = "10", IsCorrect = false },
                new AnswerOption { Text = "7", IsCorrect = false }
            }
        };
        questions.Add(q14);

        // Q15 - Data Analysis (Medium)
        var q15 = new Question
        {
            TopicId = dataAnalysis.Id,
            Text = "A dataset has values 3, 7, 7, 10, 15. What is the median?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = -0.2,
            DiscriminationParam = 0.9,
            GuessParam = 0.25,
            Explanation = "The median is the middle value when data is sorted. With 5 values (3, 7, 7, 10, 15), the middle (3rd) value is 7.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "7", IsCorrect = true },
                new AnswerOption { Text = "8", IsCorrect = false },
                new AnswerOption { Text = "10", IsCorrect = false },
                new AnswerOption { Text = "8.4", IsCorrect = false }
            }
        };
        questions.Add(q15);

        // ================== TOEFL Questions ==================

        // Q16 - Academic Reading (Easy)
        var q16 = new Question
        {
            TopicId = academicReading.Id,
            Text = "According to the passage, what is the primary function of photosynthesis?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -1.1,
            DiscriminationParam = 0.8,
            GuessParam = 0.25,
            Explanation = "Photosynthesis is the process by which plants convert light energy into chemical energy (glucose) using carbon dioxide and water.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Converting light energy into chemical energy", IsCorrect = true },
                new AnswerOption { Text = "Breaking down organic matter", IsCorrect = false },
                new AnswerOption { Text = "Transporting water through plants", IsCorrect = false },
                new AnswerOption { Text = "Absorbing carbon dioxide only", IsCorrect = false }
            }
        };
        questions.Add(q16);

        // Q17 - Academic Reading (Medium)
        var q17 = new Question
        {
            TopicId = academicReading.Id,
            Text = "Which sentence best summarizes the author's opinion on climate change?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.4,
            DiscriminationParam = 1.0,
            GuessParam = 0.25,
            Explanation = "The author repeatedly emphasizes urgency and the need for collective action, indicating that immediate steps must be taken.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "It's negligible", IsCorrect = false },
                new AnswerOption { Text = "Immediate action is needed", IsCorrect = true },
                new AnswerOption { Text = "Only scientists should act", IsCorrect = false },
                new AnswerOption { Text = "The economy should be ignored", IsCorrect = false }
            }
        };
        questions.Add(q17);

        // Q18 - Academic Reading (Hard)
        var q18 = new Question
        {
            TopicId = academicReading.Id,
            Text = "The author's use of the phrase \"double-edged sword\" in paragraph 3 suggests that technology:",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.4,
            DiscriminationParam = 1.2,
            GuessParam = 0.25,
            Explanation = "'Double-edged sword' is an idiom meaning something that has both advantages and disadvantages, or can cause both good and harm.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Has both positive and negative consequences", IsCorrect = true },
                new AnswerOption { Text = "Is primarily destructive", IsCorrect = false },
                new AnswerOption { Text = "Should be avoided entirely", IsCorrect = false },
                new AnswerOption { Text = "Is only beneficial in certain contexts", IsCorrect = false }
            }
        };
        questions.Add(q18);

        // Q19 - Lecture Comprehension (Medium)
        var q19 = new Question
        {
            TopicId = lectureComp.Id,
            Text = "The lecturer's main point about biodiversity was:",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.2,
            DiscriminationParam = 1.0,
            GuessParam = 0.25,
            Explanation = "The professor cited multiple examples of species loss and habitat destruction as evidence that biodiversity is declining globally.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "It affects only forests", IsCorrect = false },
                new AnswerOption { Text = "It is decreasing worldwide", IsCorrect = true },
                new AnswerOption { Text = "It's unrelated to water quality", IsCorrect = false },
                new AnswerOption { Text = "It is improving", IsCorrect = false }
            }
        };
        questions.Add(q19);

        // Q20 - Lecture Comprehension (Hard)
        var q20 = new Question
        {
            TopicId = lectureComp.Id,
            Text = "In the lecture, why does the professor mention the extinction of the dodo bird?",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.2,
            DiscriminationParam = 1.1,
            GuessParam = 0.25,
            Explanation = "The dodo bird is used as a classic example of human-caused extinction through hunting and habitat destruction by European settlers.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "To illustrate the impact of human activity on species", IsCorrect = true },
                new AnswerOption { Text = "To discuss the evolution of birds", IsCorrect = false },
                new AnswerOption { Text = "To explain natural selection", IsCorrect = false },
                new AnswerOption { Text = "To compare with modern bird species", IsCorrect = false }
            }
        };
        questions.Add(q20);

        // ================== NUET Questions ==================

        // Q21 - Algebra (Easy)
        var q21 = new Question
        {
            TopicId = algebra.Id,
            Text = "Simplify: 3(x + 2) - 2(x - 1)",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -1.0,
            DiscriminationParam = 0.8,
            GuessParam = 0.25,
            Explanation = "Distribute: 3x + 6 - 2x + 2. Combine like terms: (3x - 2x) + (6 + 2) = x + 8.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "x + 8", IsCorrect = true },
                new AnswerOption { Text = "x + 4", IsCorrect = false },
                new AnswerOption { Text = "5x + 4", IsCorrect = false },
                new AnswerOption { Text = "x + 6", IsCorrect = false }
            }
        };
        questions.Add(q21);

        // Q22 - Algebra (Medium)
        var q22 = new Question
        {
            TopicId = algebra.Id,
            Text = "If f(x) = 2x² - 3x + 1, what is f(2)?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.0,
            DiscriminationParam = 1.0,
            GuessParam = 0.25,
            Explanation = "Substitute x = 2: f(2) = 2(2)² - 3(2) + 1 = 2(4) - 6 + 1 = 8 - 6 + 1 = 3.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "3", IsCorrect = true },
                new AnswerOption { Text = "5", IsCorrect = false },
                new AnswerOption { Text = "7", IsCorrect = false },
                new AnswerOption { Text = "1", IsCorrect = false }
            }
        };
        questions.Add(q22);

        // Q23 - Problem Solving (Easy)
        var q23 = new Question
        {
            TopicId = problemSolving.Id,
            Text = "A train travels 120 km in 2 hours. What is its average speed?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -1.4,
            DiscriminationParam = 0.7,
            GuessParam = 0.25,
            Explanation = "Average speed = distance / time = 120 km / 2 hours = 60 km/h.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "60 km/h", IsCorrect = true },
                new AnswerOption { Text = "120 km/h", IsCorrect = false },
                new AnswerOption { Text = "80 km/h", IsCorrect = false },
                new AnswerOption { Text = "40 km/h", IsCorrect = false }
            }
        };
        questions.Add(q23);

        // Q24 - Problem Solving (Medium)
        var q24 = new Question
        {
            TopicId = problemSolving.Id,
            Text = "If 5 workers can complete a job in 12 days, how many days would it take 10 workers?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.3,
            DiscriminationParam = 1.1,
            GuessParam = 0.25,
            Explanation = "Work is inversely proportional to workers. Total work = 5 × 12 = 60 worker-days. With 10 workers: 60 / 10 = 6 days.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "6 days", IsCorrect = true },
                new AnswerOption { Text = "24 days", IsCorrect = false },
                new AnswerOption { Text = "8 days", IsCorrect = false },
                new AnswerOption { Text = "10 days", IsCorrect = false }
            }
        };
        questions.Add(q24);

        // Q25 - Problem Solving (Hard)
        var q25 = new Question
        {
            TopicId = problemSolving.Id,
            Text = "A mixture contains milk and water in the ratio 3:2. How much water must be added to 10 liters of mixture to make the ratio 3:4?",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.5,
            DiscriminationParam = 1.3,
            GuessParam = 0.25,
            Explanation = "In 10L with ratio 3:2, milk = 6L, water = 4L. For ratio 3:4, if milk is 6L, water should be 8L. Need to add 8 - 4 = 4 liters.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "4 liters", IsCorrect = true },
                new AnswerOption { Text = "2 liters", IsCorrect = false },
                new AnswerOption { Text = "6 liters", IsCorrect = false },
                new AnswerOption { Text = "8 liters", IsCorrect = false }
            }
        };
        questions.Add(q25);

        // Q26 - Logical Reasoning (Easy)
        var q26 = new Question
        {
            TopicId = logicalReasoning.Id,
            Text = "If all dogs are animals, and Rex is a dog, then:",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -1.3,
            DiscriminationParam = 0.8,
            GuessParam = 0.25,
            Explanation = "This is a basic syllogism. If all members of set A (dogs) belong to set B (animals), and Rex is in set A, then Rex must be in set B.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Rex is an animal", IsCorrect = true },
                new AnswerOption { Text = "All animals are dogs", IsCorrect = false },
                new AnswerOption { Text = "Rex is not an animal", IsCorrect = false },
                new AnswerOption { Text = "Some animals are not dogs", IsCorrect = false }
            }
        };
        questions.Add(q26);

        // Q27 - Logical Reasoning (Medium)
        var q27 = new Question
        {
            TopicId = logicalReasoning.Id,
            Text = "If all A are B and some B are C, which must be true?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.4,
            DiscriminationParam = 1.0,
            GuessParam = 0.25,
            Explanation = "Since all A are B, and some B are C, there's a possibility (but not certainty) that some of those B that are C could also be A.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "All C are A", IsCorrect = false },
                new AnswerOption { Text = "Some A may be C", IsCorrect = true },
                new AnswerOption { Text = "No C are A", IsCorrect = false },
                new AnswerOption { Text = "All B are C", IsCorrect = false }
            }
        };
        questions.Add(q27);

        // Q28 - Logical Reasoning (Hard)
        var q28 = new Question
        {
            TopicId = logicalReasoning.Id,
            Text = "Statement: All managers are leaders. Some leaders are not effective. Conclusion: Some managers are not effective. This conclusion is:",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.7,
            DiscriminationParam = 1.4,
            GuessParam = 0.25,
            Explanation = "The ineffective leaders might not include any managers — we only know some leaders are ineffective, not that any of those are managers.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Definitely true", IsCorrect = false },
                new AnswerOption { Text = "Definitely false", IsCorrect = false },
                new AnswerOption { Text = "Possibly true but not certain", IsCorrect = true },
                new AnswerOption { Text = "Cannot be determined", IsCorrect = false }
            }
        };
        questions.Add(q28);

        // Q29 - Argument Analysis (Medium)
        var q29 = new Question
        {
            TopicId = argumentAnalysis.Id,
            Text = "\"Sales increased after the new advertisement campaign. Therefore, the campaign was successful.\" What is the logical flaw?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.1,
            DiscriminationParam = 1.1,
            GuessParam = 0.25,
            Explanation = "Post hoc fallacy: just because B followed A doesn't mean A caused B. Other factors could have increased sales.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Assumes correlation implies causation", IsCorrect = true },
                new AnswerOption { Text = "Uses circular reasoning", IsCorrect = false },
                new AnswerOption { Text = "Appeals to authority", IsCorrect = false },
                new AnswerOption { Text = "Contains a false dichotomy", IsCorrect = false }
            }
        };
        questions.Add(q29);

        // Q30 - Argument Analysis (Hard)
        var q30 = new Question
        {
            TopicId = argumentAnalysis.Id,
            Text = "\"We should not listen to John's opinion on healthy eating because he is overweight.\" This argument is flawed because:",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.1,
            DiscriminationParam = 1.2,
            GuessParam = 0.25,
            Explanation = "This is an ad hominem fallacy — attacking the person's characteristics rather than addressing the merit of their argument.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "It attacks the person rather than the argument", IsCorrect = true },
                new AnswerOption { Text = "It uses false statistics", IsCorrect = false },
                new AnswerOption { Text = "It makes an emotional appeal", IsCorrect = false },
                new AnswerOption { Text = "It generalizes from one example", IsCorrect = false }
            }
        };
        questions.Add(q30);

        // ================== IELTS Questions ==================

        // IELTS Listening Comprehension
        var iq1 = new Question
        {
            TopicId = ieltsListenComp.Id,
            Text = "The speaker says the museum opens at what time on weekdays?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -1.1,
            DiscriminationParam = 0.8,
            GuessParam = 0.25,
            Explanation = "In IELTS Listening Section 1, factual details such as times, dates, and prices are commonly tested. Listen for specific numbers mentioned in context.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "9:00 AM", IsCorrect = true },
                new AnswerOption { Text = "10:00 AM", IsCorrect = false },
                new AnswerOption { Text = "8:30 AM", IsCorrect = false },
                new AnswerOption { Text = "11:00 AM", IsCorrect = false }
            }
        };
        questions.Add(iq1);

        var iq2 = new Question
        {
            TopicId = ieltsListenComp.Id,
            Text = "According to the lecture, what is the main advantage of renewable energy sources over fossil fuels?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.3,
            DiscriminationParam = 1.0,
            GuessParam = 0.25,
            Explanation = "The lecturer explicitly contrasts renewable sources with fossil fuels, emphasising that renewables do not deplete finite resources.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "They are cheaper to install", IsCorrect = false },
                new AnswerOption { Text = "They do not deplete finite resources", IsCorrect = true },
                new AnswerOption { Text = "They produce more energy per unit", IsCorrect = false },
                new AnswerOption { Text = "They require less maintenance", IsCorrect = false }
            }
        };
        questions.Add(iq2);

        var iq3 = new Question
        {
            TopicId = ieltsListenComp.Id,
            Text = "The professor mentions three factors contributing to urbanisation. Which factor does she describe as 'the most significant'?",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.4,
            DiscriminationParam = 1.2,
            GuessParam = 0.25,
            Explanation = "When a speaker uses superlatives ('most significant', 'primary', 'key'), the word immediately following is the tested detail.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Industrial growth", IsCorrect = false },
                new AnswerOption { Text = "Employment opportunities", IsCorrect = true },
                new AnswerOption { Text = "Better healthcare", IsCorrect = false },
                new AnswerOption { Text = "Educational institutions", IsCorrect = false }
            }
        };
        questions.Add(iq3);

        // IELTS Note & Form Completion
        var iq4 = new Question
        {
            TopicId = ieltsNoteForm.Id,
            Text = "Complete the note: The library card costs _____ per year for non-students.",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -1.0,
            DiscriminationParam = 0.7,
            GuessParam = 0.25,
            Explanation = "Note-completion questions require listening for specific factual information. The speaker mentions the exact annual fee for non-student library memberships.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "£25", IsCorrect = true },
                new AnswerOption { Text = "£15", IsCorrect = false },
                new AnswerOption { Text = "£50", IsCorrect = false },
                new AnswerOption { Text = "£35", IsCorrect = false }
            }
        };
        questions.Add(iq4);

        var iq5 = new Question
        {
            TopicId = ieltsNoteForm.Id,
            Text = "Complete the form: Applicant's previous employer was a _____ company.",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.2,
            DiscriminationParam = 1.0,
            GuessParam = 0.25,
            Explanation = "Form-completion tasks test the ability to capture specific details. Listen for the type of company mentioned in the applicant's work history.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "marketing", IsCorrect = true },
                new AnswerOption { Text = "finance", IsCorrect = false },
                new AnswerOption { Text = "technology", IsCorrect = false },
                new AnswerOption { Text = "engineering", IsCorrect = false }
            }
        };
        questions.Add(iq5);

        // IELTS Academic Reading
        var iq6 = new Question
        {
            TopicId = ieltsAcadRead.Id,
            Text = "According to the passage, the primary cause of coral reef bleaching is:",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -1.0,
            DiscriminationParam = 0.8,
            GuessParam = 0.25,
            Explanation = "The passage explicitly states that rising sea temperatures cause coral to expel symbiotic algae, resulting in bleaching.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Rising sea temperatures", IsCorrect = true },
                new AnswerOption { Text = "Ocean acidification", IsCorrect = false },
                new AnswerOption { Text = "Overfishing", IsCorrect = false },
                new AnswerOption { Text = "Pollution from ships", IsCorrect = false }
            }
        };
        questions.Add(iq6);

        var iq7 = new Question
        {
            TopicId = ieltsAcadRead.Id,
            Text = "The author suggests that genetic modification of crops is controversial primarily because:",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.4,
            DiscriminationParam = 1.1,
            GuessParam = 0.25,
            Explanation = "The passage discusses both potential benefits and unknown long-term ecological impacts, framing the controversy around unresolved safety concerns.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "It reduces crop diversity", IsCorrect = false },
                new AnswerOption { Text = "Long-term ecological effects remain unknown", IsCorrect = true },
                new AnswerOption { Text = "Farmers cannot afford the technology", IsCorrect = false },
                new AnswerOption { Text = "Consumers refuse to buy GM products", IsCorrect = false }
            }
        };
        questions.Add(iq7);

        var iq8 = new Question
        {
            TopicId = ieltsAcadRead.Id,
            Text = "Which of the following best describes the author's tone when discussing artificial intelligence in the workplace?",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.3,
            DiscriminationParam = 1.2,
            GuessParam = 0.25,
            Explanation = "The author acknowledges both benefits and risks of AI, using phrases like 'while promising' and 'yet concerns persist', indicating a balanced, cautious stance.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Enthusiastically supportive", IsCorrect = false },
                new AnswerOption { Text = "Cautiously optimistic", IsCorrect = true },
                new AnswerOption { Text = "Deeply pessimistic", IsCorrect = false },
                new AnswerOption { Text = "Entirely neutral", IsCorrect = false }
            }
        };
        questions.Add(iq8);

        // IELTS Reading: Matching & True/False
        var iq9 = new Question
        {
            TopicId = ieltsReadMatch.Id,
            Text = "The passage states: 'Solar energy will replace all fossil fuels by 2030.' Is this TRUE, FALSE, or NOT GIVEN?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -0.8,
            DiscriminationParam = 0.8,
            GuessParam = 0.25,
            Explanation = "The passage discusses solar energy's growing role but never makes an absolute claim about replacing ALL fossil fuels by a specific date. The answer is NOT GIVEN.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "True", IsCorrect = false },
                new AnswerOption { Text = "False", IsCorrect = false },
                new AnswerOption { Text = "Not Given", IsCorrect = true },
                new AnswerOption { Text = "Partially True", IsCorrect = false }
            }
        };
        questions.Add(iq9);

        var iq10 = new Question
        {
            TopicId = ieltsReadMatch.Id,
            Text = "Match the researcher to their finding: Dr. Chen's study focused on:",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.3,
            DiscriminationParam = 1.0,
            GuessParam = 0.25,
            Explanation = "Matching questions require scanning the passage for proper nouns. Dr. Chen's work is mentioned in paragraph 4 in connection with sleep patterns.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Sleep patterns in adolescents", IsCorrect = true },
                new AnswerOption { Text = "Dietary habits in elderly populations", IsCorrect = false },
                new AnswerOption { Text = "Exercise and cognitive function", IsCorrect = false },
                new AnswerOption { Text = "Stress management techniques", IsCorrect = false }
            }
        };
        questions.Add(iq10);

        var iq11 = new Question
        {
            TopicId = ieltsReadMatch.Id,
            Text = "The passage states that early childhood bilingualism delays language development. TRUE, FALSE, or NOT GIVEN?",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.2,
            DiscriminationParam = 1.1,
            GuessParam = 0.25,
            Explanation = "The passage explicitly contradicts this — it states that while bilingual children may initially mix languages, overall development timelines are comparable. The answer is FALSE.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "True", IsCorrect = false },
                new AnswerOption { Text = "False", IsCorrect = true },
                new AnswerOption { Text = "Not Given", IsCorrect = false },
                new AnswerOption { Text = "Cannot be determined", IsCorrect = false }
            }
        };
        questions.Add(iq11);

        // IELTS Task 1: Data Description
        var iq12 = new Question
        {
            TopicId = ieltsTask1.Id,
            Text = "When describing a line graph in IELTS Task 1, which phrase best describes a sharp increase followed by a plateau?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -0.9,
            DiscriminationParam = 0.8,
            GuessParam = 0.25,
            Explanation = "'Rose sharply before levelling off' accurately conveys a steep increase followed by stabilisation — key vocabulary for Task 1 descriptions.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Rose sharply before levelling off", IsCorrect = true },
                new AnswerOption { Text = "Fluctuated wildly throughout", IsCorrect = false },
                new AnswerOption { Text = "Declined steadily over the period", IsCorrect = false },
                new AnswerOption { Text = "Remained constant at all times", IsCorrect = false }
            }
        };
        questions.Add(iq12);

        var iq13 = new Question
        {
            TopicId = ieltsTask1.Id,
            Text = "In a Task 1 report comparing two pie charts, what should the overview paragraph include?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.2,
            DiscriminationParam = 1.0,
            GuessParam = 0.25,
            Explanation = "The overview must identify the most significant trends or differences without specific figures — it's a summary of key patterns.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "All exact percentages from both charts", IsCorrect = false },
                new AnswerOption { Text = "The most notable overall trends and differences", IsCorrect = true },
                new AnswerOption { Text = "Your personal opinion on the data", IsCorrect = false },
                new AnswerOption { Text = "A list of every category shown", IsCorrect = false }
            }
        };
        questions.Add(iq13);

        var iq14 = new Question
        {
            TopicId = ieltsTask1.Id,
            Text = "Which sentence correctly uses data comparison language for IELTS Task 1?",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.1,
            DiscriminationParam = 1.2,
            GuessParam = 0.25,
            Explanation = "'Nearly three times as many' is a precise comparative structure appropriate for academic writing in Task 1.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "The USA spent a lot more than France on defence.", IsCorrect = false },
                new AnswerOption { Text = "Nearly three times as many people chose train travel compared to bus.", IsCorrect = true },
                new AnswerOption { Text = "I think the graph shows that exports were better.", IsCorrect = false },
                new AnswerOption { Text = "Obviously, Japan's population is the biggest.", IsCorrect = false }
            }
        };
        questions.Add(iq14);

        // IELTS Task 2: Essay Writing
        var iq15 = new Question
        {
            TopicId = ieltsTask2.Id,
            Text = "What is the recommended structure for an IELTS Task 2 opinion essay?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -1.0,
            DiscriminationParam = 0.7,
            GuessParam = 0.25,
            Explanation = "A standard 4-paragraph structure (introduction, 2 body paragraphs, conclusion) is recommended for clarity and coherence in Task 2.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Introduction, Body 1, Body 2, Conclusion", IsCorrect = true },
                new AnswerOption { Text = "Three body paragraphs only", IsCorrect = false },
                new AnswerOption { Text = "Introduction and one long paragraph", IsCorrect = false },
                new AnswerOption { Text = "Five short paragraphs", IsCorrect = false }
            }
        };
        questions.Add(iq15);

        var iq16 = new Question
        {
            TopicId = ieltsTask2.Id,
            Text = "Which of these is the best thesis statement for a 'discuss both views and give your opinion' essay on remote work?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.4,
            DiscriminationParam = 1.1,
            GuessParam = 0.25,
            Explanation = "An effective thesis for this essay type acknowledges both perspectives and clearly states the writer's own position.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Remote work is bad.", IsCorrect = false },
                new AnswerOption { Text = "While remote work offers flexibility, it can reduce collaboration; I believe a hybrid model is most effective.", IsCorrect = true },
                new AnswerOption { Text = "In this essay I will discuss remote work.", IsCorrect = false },
                new AnswerOption { Text = "Many people work from home nowadays.", IsCorrect = false }
            }
        };
        questions.Add(iq16);

        // IELTS Speaking Parts 1 & 2
        var iq17 = new Question
        {
            TopicId = ieltsSpeaking12.Id,
            Text = "In IELTS Speaking Part 1, the examiner asks 'Do you enjoy cooking?' Which response would score highest?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -0.8,
            DiscriminationParam = 0.8,
            GuessParam = 0.25,
            Explanation = "An extended answer with reasons and examples demonstrates range of vocabulary and fluency, which are key assessment criteria.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Yes, I do.", IsCorrect = false },
                new AnswerOption { Text = "Yes, I really enjoy cooking, especially Italian dishes. I find it relaxing after a long day.", IsCorrect = true },
                new AnswerOption { Text = "Cooking? No.", IsCorrect = false },
                new AnswerOption { Text = "Sometimes when I have time which is rare.", IsCorrect = false }
            }
        };
        questions.Add(iq17);

        var iq18 = new Question
        {
            TopicId = ieltsSpeaking12.Id,
            Text = "For a Part 2 cue card 'Describe a place you have visited', which preparation strategy is best during the 1-minute prep time?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.1,
            DiscriminationParam = 1.0,
            GuessParam = 0.25,
            Explanation = "Jotting key words for each bullet point ensures you cover all parts of the cue card systematically during your 1–2 minute response.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Write complete sentences for the entire response", IsCorrect = false },
                new AnswerOption { Text = "Jot key words for each bullet point on the cue card", IsCorrect = true },
                new AnswerOption { Text = "Memorise a pre-prepared answer on a different topic", IsCorrect = false },
                new AnswerOption { Text = "Skip preparation and start speaking immediately", IsCorrect = false }
            }
        };
        questions.Add(iq18);

        // IELTS Speaking Part 3: Discussion
        var iq19 = new Question
        {
            TopicId = ieltsSpeaking3.Id,
            Text = "In Part 3, the examiner asks 'Why do you think some people prefer living in cities?' Which response best demonstrates critical thinking?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.5,
            DiscriminationParam = 1.1,
            GuessParam = 0.25,
            Explanation = "A Band 7+ response analyses reasons with specific examples and uses complex structures like 'I would argue that…'",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Because cities are nice.", IsCorrect = false },
                new AnswerOption { Text = "I'd argue it's mainly due to career opportunities and access to amenities, though the pace of life can be stressful.", IsCorrect = true },
                new AnswerOption { Text = "I don't know, maybe they like it.", IsCorrect = false },
                new AnswerOption { Text = "Cities have many people.", IsCorrect = false }
            }
        };
        questions.Add(iq19);

        var iq20 = new Question
        {
            TopicId = ieltsSpeaking3.Id,
            Text = "How should you respond if the examiner asks a Part 3 question you find very difficult?",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.2,
            DiscriminationParam = 1.2,
            GuessParam = 0.25,
            Explanation = "Paraphrasing the question and offering a tentative answer demonstrates communication strategies and maintains fluency, both positively scored.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Say 'I don't know' and wait for the next question", IsCorrect = false },
                new AnswerOption { Text = "Ask the examiner for a different question", IsCorrect = false },
                new AnswerOption { Text = "Paraphrase the question aloud and give your best tentative answer", IsCorrect = true },
                new AnswerOption { Text = "Recite an unrelated memorised answer", IsCorrect = false }
            }
        };
        questions.Add(iq20);

        // ================== CSCA Questions ==================

        // Calculus & Analysis
        var cq1 = new Question
        {
            TopicId = calculus.Id,
            Text = "What is the derivative of $f(x) = 3x^2 + 2x - 5$?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -1.2,
            DiscriminationParam = 0.8,
            GuessParam = 0.25,
            Explanation = "Using the power rule: d/dx(ax^n) = nax^(n-1). So f'(x) = 6x + 2.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "$6x + 2$", IsCorrect = true },
                new AnswerOption { Text = "$3x + 2$", IsCorrect = false },
                new AnswerOption { Text = "$6x^2 + 2$", IsCorrect = false },
                new AnswerOption { Text = "$6x - 5$", IsCorrect = false }
            }
        };
        questions.Add(cq1);

        var cq2 = new Question
        {
            TopicId = calculus.Id,
            Text = "Evaluate $\\int_0^2 (4x + 1)\\,dx$.",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.1,
            DiscriminationParam = 1.0,
            GuessParam = 0.25,
            Explanation = "Integrate: 2x² + x. Evaluate from 0 to 2: (2·4 + 2) − (0) = 10.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "10", IsCorrect = true },
                new AnswerOption { Text = "8", IsCorrect = false },
                new AnswerOption { Text = "12", IsCorrect = false },
                new AnswerOption { Text = "9", IsCorrect = false }
            }
        };
        questions.Add(cq2);

        var cq3 = new Question
        {
            TopicId = calculus.Id,
            Text = "Find the limit: $\\lim_{x \\to 0} \\frac{\\sin x}{x}$",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.3,
            DiscriminationParam = 1.1,
            GuessParam = 0.25,
            Explanation = "This is a fundamental limit in calculus. By L'Hôpital's rule or the squeeze theorem, the limit equals 1.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "1", IsCorrect = true },
                new AnswerOption { Text = "0", IsCorrect = false },
                new AnswerOption { Text = "∞", IsCorrect = false },
                new AnswerOption { Text = "Does not exist", IsCorrect = false }
            }
        };
        questions.Add(cq3);

        var cq4 = new Question
        {
            TopicId = calculus.Id,
            Text = "Find the second derivative of $f(x) = x^4 - 3x^2 + x$.",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.1,
            DiscriminationParam = 1.2,
            GuessParam = 0.25,
            Explanation = "f'(x) = 4x³ - 6x + 1. f''(x) = 12x² - 6.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "$12x^2 - 6$", IsCorrect = true },
                new AnswerOption { Text = "$4x^3 - 6x + 1$", IsCorrect = false },
                new AnswerOption { Text = "$12x^2 - 3$", IsCorrect = false },
                new AnswerOption { Text = "$24x$", IsCorrect = false }
            }
        };
        questions.Add(cq4);

        // Probability & Statistics
        var cq5 = new Question
        {
            TopicId = probStats.Id,
            Text = "A fair die is rolled twice. What is the probability of getting a sum of 7?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -0.8,
            DiscriminationParam = 0.8,
            GuessParam = 0.25,
            Explanation = "There are 6 favourable outcomes (1+6, 2+5, 3+4, 4+3, 5+2, 6+1) out of 36 total. P = 6/36 = 1/6.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "1/6", IsCorrect = true },
                new AnswerOption { Text = "1/12", IsCorrect = false },
                new AnswerOption { Text = "7/36", IsCorrect = false },
                new AnswerOption { Text = "1/36", IsCorrect = false }
            }
        };
        questions.Add(cq5);

        var cq6 = new Question
        {
            TopicId = probStats.Id,
            Text = "The standard deviation of a dataset {2, 4, 4, 4, 5, 5, 7, 9} is closest to:",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.4,
            DiscriminationParam = 1.0,
            GuessParam = 0.25,
            Explanation = "Mean = 5. Variance = [(9+1+1+1+0+0+4+16)/8] = 32/8 = 4. SD = √4 = 2.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "2", IsCorrect = true },
                new AnswerOption { Text = "4", IsCorrect = false },
                new AnswerOption { Text = "1.5", IsCorrect = false },
                new AnswerOption { Text = "3", IsCorrect = false }
            }
        };
        questions.Add(cq6);

        var cq7 = new Question
        {
            TopicId = probStats.Id,
            Text = "If events A and B are independent with P(A) = 0.3 and P(B) = 0.5, what is P(A ∩ B)?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.0,
            DiscriminationParam = 1.1,
            GuessParam = 0.25,
            Explanation = "For independent events: P(A ∩ B) = P(A) × P(B) = 0.3 × 0.5 = 0.15.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "0.15", IsCorrect = true },
                new AnswerOption { Text = "0.80", IsCorrect = false },
                new AnswerOption { Text = "0.20", IsCorrect = false },
                new AnswerOption { Text = "0.35", IsCorrect = false }
            }
        };
        questions.Add(cq7);

        var cq8 = new Question
        {
            TopicId = probStats.Id,
            Text = "In a normal distribution, approximately what percentage of data lies within one standard deviation of the mean?",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.0,
            DiscriminationParam = 1.2,
            GuessParam = 0.25,
            Explanation = "The empirical rule (68-95-99.7 rule) states that about 68% of data falls within ±1 standard deviation of the mean.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "68%", IsCorrect = true },
                new AnswerOption { Text = "50%", IsCorrect = false },
                new AnswerOption { Text = "95%", IsCorrect = false },
                new AnswerOption { Text = "34%", IsCorrect = false }
            }
        };
        questions.Add(cq8);

        // Discrete Mathematics
        var cq9 = new Question
        {
            TopicId = discreteMath.Id,
            Text = "How many subsets does a set with 4 elements have?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -1.0,
            DiscriminationParam = 0.8,
            GuessParam = 0.25,
            Explanation = "A set with n elements has 2^n subsets. 2^4 = 16.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "16", IsCorrect = true },
                new AnswerOption { Text = "8", IsCorrect = false },
                new AnswerOption { Text = "4", IsCorrect = false },
                new AnswerOption { Text = "24", IsCorrect = false }
            }
        };
        questions.Add(cq9);

        var cq10 = new Question
        {
            TopicId = discreteMath.Id,
            Text = "In how many ways can 5 books be arranged on a shelf?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -0.9,
            DiscriminationParam = 0.7,
            GuessParam = 0.25,
            Explanation = "The number of permutations of 5 items is 5! = 5 × 4 × 3 × 2 × 1 = 120.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "120", IsCorrect = true },
                new AnswerOption { Text = "25", IsCorrect = false },
                new AnswerOption { Text = "60", IsCorrect = false },
                new AnswerOption { Text = "24", IsCorrect = false }
            }
        };
        questions.Add(cq10);

        var cq11 = new Question
        {
            TopicId = discreteMath.Id,
            Text = "What is the value of $\\binom{7}{3}$?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.2,
            DiscriminationParam = 1.0,
            GuessParam = 0.25,
            Explanation = "C(7,3) = 7! / (3! × 4!) = (7 × 6 × 5) / (3 × 2 × 1) = 35.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "35", IsCorrect = true },
                new AnswerOption { Text = "21", IsCorrect = false },
                new AnswerOption { Text = "42", IsCorrect = false },
                new AnswerOption { Text = "210", IsCorrect = false }
            }
        };
        questions.Add(cq11);

        var cq12 = new Question
        {
            TopicId = discreteMath.Id,
            Text = "A graph has 6 vertices and each vertex has degree 2. How many edges does the graph have?",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.3,
            DiscriminationParam = 1.2,
            GuessParam = 0.25,
            Explanation = "By the handshaking lemma, the sum of all degrees equals 2 × edges. Sum of degrees = 6 × 2 = 12. So edges = 12/2 = 6.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "6", IsCorrect = true },
                new AnswerOption { Text = "12", IsCorrect = false },
                new AnswerOption { Text = "3", IsCorrect = false },
                new AnswerOption { Text = "9", IsCorrect = false }
            }
        };
        questions.Add(cq12);

        // Formal Logic
        var cq13 = new Question
        {
            TopicId = formalLogic.Id,
            Text = "If P → Q is true and P is true, what can we conclude?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -1.3,
            DiscriminationParam = 0.8,
            GuessParam = 0.25,
            Explanation = "This is modus ponens — one of the fundamental rules of inference. If P implies Q, and P is true, then Q must be true.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Q is true", IsCorrect = true },
                new AnswerOption { Text = "Q is false", IsCorrect = false },
                new AnswerOption { Text = "P is false", IsCorrect = false },
                new AnswerOption { Text = "Cannot be determined", IsCorrect = false }
            }
        };
        questions.Add(cq13);

        var cq14 = new Question
        {
            TopicId = formalLogic.Id,
            Text = "What is the contrapositive of 'If it rains, then the ground is wet'?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.1,
            DiscriminationParam = 1.0,
            GuessParam = 0.25,
            Explanation = "The contrapositive of P → Q is ¬Q → ¬P. So: 'If the ground is NOT wet, then it did NOT rain.' A contrapositive always has the same truth value as the original.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "If the ground is not wet, then it did not rain", IsCorrect = true },
                new AnswerOption { Text = "If it does not rain, the ground is not wet", IsCorrect = false },
                new AnswerOption { Text = "If the ground is wet, then it rained", IsCorrect = false },
                new AnswerOption { Text = "It rains only if the ground is wet", IsCorrect = false }
            }
        };
        questions.Add(cq14);

        var cq15 = new Question
        {
            TopicId = formalLogic.Id,
            Text = "Which logical equivalence is correct?",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.4,
            DiscriminationParam = 1.3,
            GuessParam = 0.25,
            Explanation = "De Morgan's Law states that ¬(P ∧ Q) ≡ ¬P ∨ ¬Q. The negation of a conjunction is the disjunction of the negations.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "¬(P ∧ Q) ≡ ¬P ∨ ¬Q", IsCorrect = true },
                new AnswerOption { Text = "¬(P ∧ Q) ≡ ¬P ∧ ¬Q", IsCorrect = false },
                new AnswerOption { Text = "¬(P ∨ Q) ≡ ¬P ∨ ¬Q", IsCorrect = false },
                new AnswerOption { Text = "P → Q ≡ P ∧ ¬Q", IsCorrect = false }
            }
        };
        questions.Add(cq15);

        // Algorithmic Thinking
        var cq16 = new Question
        {
            TopicId = algoThinking.Id,
            Text = "What is the time complexity of binary search on a sorted array of n elements?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -0.9,
            DiscriminationParam = 0.8,
            GuessParam = 0.25,
            Explanation = "Binary search halves the search space at each step, giving O(log n) time complexity.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "O(log n)", IsCorrect = true },
                new AnswerOption { Text = "O(n)", IsCorrect = false },
                new AnswerOption { Text = "O(n²)", IsCorrect = false },
                new AnswerOption { Text = "O(1)", IsCorrect = false }
            }
        };
        questions.Add(cq16);

        var cq17 = new Question
        {
            TopicId = algoThinking.Id,
            Text = "A recursive function computes Fibonacci(n). Without memoisation, what is its time complexity?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.5,
            DiscriminationParam = 1.1,
            GuessParam = 0.25,
            Explanation = "Naive recursive Fibonacci makes two recursive calls at each step, leading to an exponential O(2^n) time complexity with many redundant computations.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "O(2^n)", IsCorrect = true },
                new AnswerOption { Text = "O(n)", IsCorrect = false },
                new AnswerOption { Text = "O(n²)", IsCorrect = false },
                new AnswerOption { Text = "O(n log n)", IsCorrect = false }
            }
        };
        questions.Add(cq17);

        var cq18 = new Question
        {
            TopicId = algoThinking.Id,
            Text = "Which sorting algorithm has the best average-case time complexity?",
            Difficulty = QuestionDifficulty.Hard,
            DifficultyParam = 1.0,
            DiscriminationParam = 1.2,
            GuessParam = 0.25,
            Explanation = "Merge sort guarantees O(n log n) in all cases (best, average, worst). Bubble sort and insertion sort are O(n²) on average.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "Merge sort — O(n log n)", IsCorrect = true },
                new AnswerOption { Text = "Bubble sort — O(n²)", IsCorrect = false },
                new AnswerOption { Text = "Insertion sort — O(n²)", IsCorrect = false },
                new AnswerOption { Text = "Selection sort — O(n²)", IsCorrect = false }
            }
        };
        questions.Add(cq18);

        // Data Interpretation
        var cq19 = new Question
        {
            TopicId = dataInterp.Id,
            Text = "A bar chart shows Company A's revenue as $4M and Company B's as $6M. What is B's revenue as a percentage of the total?",
            Difficulty = QuestionDifficulty.Easy,
            DifficultyParam = -1.1,
            DiscriminationParam = 0.7,
            GuessParam = 0.25,
            Explanation = "Total = 4 + 6 = 10. B's share = 6/10 = 60%.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "60%", IsCorrect = true },
                new AnswerOption { Text = "40%", IsCorrect = false },
                new AnswerOption { Text = "66%", IsCorrect = false },
                new AnswerOption { Text = "50%", IsCorrect = false }
            }
        };
        questions.Add(cq19);

        var cq20 = new Question
        {
            TopicId = dataInterp.Id,
            Text = "A line graph shows sales rising from 100 to 150 units over 3 years. What is the average annual growth rate?",
            Difficulty = QuestionDifficulty.Medium,
            DifficultyParam = 0.3,
            DiscriminationParam = 1.0,
            GuessParam = 0.25,
            Explanation = "Total growth = 50 units over 3 years. Average annual growth = (150 - 100) / 3 ≈ 16.7 units per year.",
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = "≈16.7 units/year", IsCorrect = true },
                new AnswerOption { Text = "50 units/year", IsCorrect = false },
                new AnswerOption { Text = "25 units/year", IsCorrect = false },
                new AnswerOption { Text = "10 units/year", IsCorrect = false }
            }
        };
        questions.Add(cq20);

        await _context.Questions.AddRangeAsync(questions);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates IRT parameters on existing questions that still have default values.
    /// Uses IrtMath helper methods to map from QuestionDifficulty enum.
    /// </summary>
    private async Task UpdateIrtParametersAsync()
    {
        // Find questions that still have default IRT params (DiscriminationParam == 1.0 and DifficultyParam == 0.0)
        var questionsToUpdate = await _context.Questions
            .Where(q => q.DiscriminationParam == 1.0 && q.DifficultyParam == 0.0 && q.GuessParam == 0.25)
            .ToListAsync();

        if (!questionsToUpdate.Any()) return;

        var random = new Random(42); // deterministic seed for reproducibility

        foreach (var q in questionsToUpdate)
        {
            // Base IRT params from difficulty enum
            var baseB = IrtMath.DifficultyToParam(q.Difficulty);
            var baseA = IrtMath.DifficultyToDiscrimination(q.Difficulty);

            // Add small per-question variation for realism
            q.DifficultyParam = baseB + (random.NextDouble() - 0.5) * 0.4; // ±0.2
            q.DiscriminationParam = Math.Max(0.5, baseA + (random.NextDouble() - 0.5) * 0.3); // ±0.15, min 0.5
            q.GuessParam = 0.25; // 4-option MCQ
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds prerequisite relationships between topics (knowledge graph).
    /// </summary>
    private async Task SeedTopicDependenciesAsync()
    {
        var topics = await _context.Topics.ToListAsync();

        TopicDependency? MakeDep(string topicName, string prereqName, double weight)
        {
            var topic = topics.FirstOrDefault(t => t.Name == topicName);
            var prereq = topics.FirstOrDefault(t => t.Name == prereqName);
            if (topic == null || prereq == null) return null;
            return new TopicDependency
            {
                TopicId = topic.Id,
                PrerequisiteTopicId = prereq.Id,
                Weight = weight
            };
        }

        var dependencies = new List<TopicDependency?>
        {
            // SAT Math: Linear Equations → Quadratic Equations (strong prerequisite)
            MakeDep("Quadratic Equations", "Linear Equations", 0.9),

            // SAT Math: Geometry builds on basic algebra
            MakeDep("Geometry", "Linear Equations", 0.4),

            // SAT Math: Data Analysis benefits from algebra
            MakeDep("Data Analysis", "Linear Equations", 0.3),

            // SAT Reading: Vocabulary helps Main Idea comprehension
            MakeDep("Main Idea & Summary", "Vocabulary in Context", 0.5),

            // SAT Writing: Grammar is foundational for sentence structure
            // (Grammar is the topic itself, so no self-dep needed)

            // NUET: Algebra & Functions → Problem Solving (must know algebra to solve problems)
            MakeDep("Problem Solving", "Algebra & Functions", 0.8),

            // NUET: Logical Reasoning → Argument Analysis
            MakeDep("Argument Analysis", "Logical Reasoning", 0.7),

            // Cross-exam: SAT Math topics feed into NUET Algebra
            MakeDep("Algebra & Functions", "Linear Equations", 0.6),
            MakeDep("Algebra & Functions", "Quadratic Equations", 0.5),

            // TOEFL: Academic Reading builds on general reading skills
            MakeDep("Academic Reading", "Main Idea & Summary", 0.5),
            MakeDep("Academic Reading", "Vocabulary in Context", 0.4),

            // TOEFL: Integrated Writing needs reading comprehension
            MakeDep("Integrated Writing", "Academic Reading", 0.6),
            MakeDep("Integrated Writing", "Grammar & Sentence Structure", 0.5),

            // IELTS: Reading matching depends on basic reading
            MakeDep("IELTS Reading: Matching & True/False", "IELTS Academic Reading", 0.7),
            // IELTS: Task 2 essay benefits from Task 1 data description
            MakeDep("IELTS Task 2: Essay Writing", "IELTS Task 1: Data Description", 0.4),
            // IELTS: Speaking Part 3 builds on Parts 1 & 2
            MakeDep("IELTS Speaking Part 3: Discussion", "IELTS Speaking Parts 1 & 2", 0.8),
            // IELTS: Note completion depends on comprehension
            MakeDep("IELTS Note & Form Completion", "IELTS Listening Comprehension", 0.7),
            // Cross-exam: IELTS Reading builds on TOEFL Academic Reading
            MakeDep("IELTS Academic Reading", "Academic Reading", 0.4),

            // CSCA: Algorithmic Thinking requires Formal Logic
            MakeDep("Algorithmic Thinking", "Formal Logic", 0.7),
            // CSCA: Data Interpretation benefits from Probability & Statistics
            MakeDep("Data Interpretation", "Probability & Statistics", 0.6),
            // CSCA: Probability builds on Calculus
            MakeDep("Probability & Statistics", "Calculus & Analysis", 0.5),
            // CSCA: Discrete Math builds on Calculus
            MakeDep("Discrete Mathematics", "Calculus & Analysis", 0.4),
            // Cross-exam: CSCA Calculus builds on SAT/NUET algebra
            MakeDep("Calculus & Analysis", "Algebra & Functions", 0.6),
            MakeDep("Calculus & Analysis", "Quadratic Equations", 0.5),
        };

        var validDeps = dependencies.Where(d => d != null).Cast<TopicDependency>().ToList();
        await _context.TopicDependencies.AddRangeAsync(validDeps);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds mini-lessons for every topic — brief theory content students review before practice.
    /// </summary>
    private async Task SeedTopicLessonsAsync()
    {
        var topics = await _context.Topics.ToListAsync();

        TopicLesson L(string topicName, int order, string title, string content, string? videoUrl = null)
        {
            var topic = topics.First(t => t.Name == topicName);
            return new TopicLesson
            {
                TopicId = topic.Id,
                Title = title,
                Content = content,
                VideoUrl = videoUrl,
                SortOrder = order
            };
        }

        var lessons = new List<TopicLesson>
        {
            // ──── SAT Reading & Writing ────────────────────────
            L("Main Idea & Summary", 1,
              "Finding the Main Idea",
              "## What is a Main Idea?\n\nThe **main idea** is the central point the author wants to communicate. It answers the question: *What is this passage mostly about?*\n\n### Strategy\n1. **Read the first and last sentences** of the passage — they often frame the argument.\n2. **Identify the topic** (subject) and **what the author says** about it.\n3. Eliminate answer choices that are too broad, too narrow, or off-topic.\n\n### Common Traps\n- **Too specific**: focuses on one detail instead of the whole passage.\n- **Too general**: could apply to many passages, not this one specifically.\n- **Opposite meaning**: contradicts the author's argument.\n\n### Example\n> \"Government policies shape economic growth by setting tax rates, regulating markets, and funding infrastructure.\"\n\n**Main idea:** Government policies influence economic growth.",
              null),

            L("Main Idea & Summary", 2,
              "Summarising a Passage",
              "## How to Summarise\n\nA good summary captures the **key points** without adding personal opinion.\n\n### Steps\n1. Identify the **thesis** (main claim).\n2. Note **supporting points** — usually one per paragraph.\n3. Combine them in 1–2 sentences.\n\n### Tips\n- Use your **own words** — avoid copying phrases.\n- A correct summary should work as a replacement for the passage."),

            L("Grammar & Sentence Structure", 1,
              "Subject-Verb Agreement",
              "## Subject-Verb Agreement\n\nThe verb must agree in number with its subject.\n\n### Rules\n| Subject | Verb |\n|---------|------|\n| Singular (The dog) | runs |\n| Plural (The dogs) | run |\n\n### Tricky Cases\n- **Prepositional phrases**: \"The box *of chocolates* **is** heavy.\" (subject = box)\n- **Compound subjects**: \"Tom **and** Jerry **are** friends.\"\n- **Either/or**: \"Either the cats **or** the dog **is** sleeping.\" (verb matches nearest subject)\n\n### Quick Check\nCross out words between subject and verb to test agreement.",
              null),

            L("Grammar & Sentence Structure", 2,
              "Sentence Fragments & Run-ons",
              "## Sentence Fragments\n\nA fragment is missing a **subject**, **verb**, or **complete thought**.\n\n❌ *Because it was raining.* (dependent clause alone)\n✅ *We stayed inside because it was raining.*\n\n## Run-on Sentences\n\nTwo independent clauses joined without proper punctuation.\n\n❌ *I love reading I go to the library every week.*\n\n### How to Fix\n1. **Period**: I love reading. I go to the library every week.\n2. **Semicolon**: I love reading; I go to the library every week.\n3. **Conjunction**: I love reading, so I go to the library every week."),

            L("Vocabulary in Context", 1,
              "Determining Word Meaning from Context",
              "## Context Clues Strategy\n\nWhen an unfamiliar word appears in a passage, the surrounding words help reveal its meaning.\n\n### Types of Context Clues\n1. **Definition clue**: The word is directly defined. *\"Ubiquitous, meaning everywhere, ...\"*\n2. **Synonym clue**: A similar word is nearby. *\"She was elated — truly joyful.\"*\n3. **Antonym clue**: An opposite word provides contrast. *\"Unlike his timid brother, Jake was audacious.\"*\n4. **Example clue**: Examples illustrate the meaning.\n\n### Technique\n- Substitute each answer choice into the sentence.\n- Choose the one that maintains the **tone** and **logic** of the passage.",
              null),

            // ──── SAT Math ─────────────────────────────────────
            L("Linear Equations", 1,
              "Solving Linear Equations",
              "## Linear Equations\n\nA linear equation has the form **ax + b = c** where the variable has exponent 1.\n\n### Solving Steps\n1. **Simplify** both sides (combine like terms).\n2. **Isolate** the variable using inverse operations.\n3. **Check** by substituting back.\n\n### Example\n$$2x + 5 = 13$$\n$$2x = 8$$\n$$x = 4$$\n\n### Slope-Intercept Form\n$$y = mx + b$$\n- **m** = slope (rise/run)\n- **b** = y-intercept",
              null),

            L("Linear Equations", 2,
              "Systems of Linear Equations",
              "## Systems of Equations\n\nTwo or more equations with the same variables.\n\n### Methods\n1. **Substitution**: Solve one equation for a variable, substitute into the other.\n2. **Elimination**: Add/subtract equations to eliminate a variable.\n3. **Graphing**: The solution is where lines intersect.\n\n### Example (Elimination)\n$$x + y = 10$$\n$$x - y = 4$$\n\nAdd both: $2x = 14 \\Rightarrow x = 7$, then $y = 3$.\n\n### No Solution vs. Infinite Solutions\n- **Parallel lines** (same slope, different intercept): no solution.\n- **Same line**: infinitely many solutions."),

            L("Geometry", 1,
              "Angles, Triangles & Circles",
              "## Key Geometry Facts\n\n### Angles\n- Supplementary: $a + b = 180°$\n- Complementary: $a + b = 90°$\n- Vertical angles are equal.\n\n### Triangles\n- Interior angles sum to **180°**.\n- **Pythagorean theorem** (right triangle): $a^2 + b^2 = c^2$\n- Area = $\\frac{1}{2} \\times base \\times height$\n\n### Circles\n- Area = $\\pi r^2$\n- Circumference = $2\\pi r$\n- Arc length = $\\frac{\\theta}{360} \\times 2\\pi r$",
              null),

            L("Data Analysis", 1,
              "Mean, Median, Mode & Graphs",
              "## Central Tendency\n\n| Measure | Formula |\n|---------|---------|\n| Mean | Sum of values / count |\n| Median | Middle value when sorted |\n| Mode | Most frequent value |\n\n### Reading Graphs\n- **Bar chart**: compare categories.\n- **Line graph**: trends over time.\n- **Scatter plot**: correlation between two variables.\n\n### Tips\n- Watch for **outliers** — they pull the mean but not the median.\n- If the question says \"average,\" it usually means **mean**.",
              null),

            L("Quadratic Equations", 1,
              "Solving Quadratic Equations",
              "## Quadratic Form\n\n$$ax^2 + bx + c = 0$$\n\n### Three Methods\n1. **Factoring**: $(x - 2)(x + 3) = 0 \\Rightarrow x = 2$ or $x = -3$\n2. **Quadratic Formula**: $x = \\frac{-b \\pm \\sqrt{b^2 - 4ac}}{2a}$\n3. **Completing the square**\n\n### Discriminant $\\Delta = b^2 - 4ac$\n- $\\Delta > 0$: two real solutions\n- $\\Delta = 0$: one real solution (vertex touches x-axis)\n- $\\Delta < 0$: no real solutions\n\n### Vertex Form\n$$y = a(x - h)^2 + k$$\nVertex at $(h, k)$.",
              null),

            // ──── TOEFL ────────────────────────────────────────
            L("Academic Reading", 1,
              "Strategies for Academic Passages",
              "## TOEFL Reading Overview\n\nYou'll encounter 3–4 academic passages (~700 words each) on science, history, or social topics.\n\n### Reading Strategy\n1. **Skim first** — read the title, first sentence of each paragraph.\n2. **Note the structure**: compare/contrast, cause/effect, chronological.\n3. **Read questions first** to know what to look for.\n\n### Question Types\n- **Factual**: stated directly in the passage.\n- **Inference**: implied but not stated.\n- **Vocabulary**: meaning from context.\n- **Summary/Insert**: overall organization.",
              null),

            L("Lecture Comprehension", 1,
              "Note-taking for Lectures",
              "## TOEFL Listening: Lectures\n\nLectures are 3–5 minutes long on academic subjects.\n\n### Note-taking Tips\n- Write **keywords**, not full sentences.\n- Use abbreviations: → (leads to), ≈ (approximately), ∵ (because).\n- Note the **main topic** at the top, then **sub-points**.\n\n### What to Listen For\n- The professor's **main point** (usually stated early).\n- **Examples** that support the point.\n- **Contrasts** or **corrections** (\"Actually,\" \"But what's interesting is...\").\n- **Rhetorical questions** — they signal key ideas."),

            L("Conversation Understanding", 1,
              "Campus Conversations",
              "## TOEFL Listening: Conversations\n\nConversations are between a student and professor or campus staff.\n\n### Key Focus Areas\n- **Purpose**: Why did the student initiate the conversation?\n- **Attitude**: How does the speaker feel? (frustrated, confused, grateful)\n- **Outcome**: What is decided or suggested?\n\n### Common Scenarios\n- Office hours: asking about assignments or grades.\n- Registration: course selection, schedule conflicts.\n- Library/services: finding resources."),

            L("Independent Speaking", 1,
              "Structuring Your Response",
              "## TOEFL Speaking: Independent Task\n\nYou get 15 seconds to prepare, 45 seconds to speak.\n\n### Template\n1. **State your opinion** (5 sec): \"I believe that...\"\n2. **Reason 1 + example** (15 sec): \"First, ... For example, ...\"\n3. **Reason 2 + example** (15 sec): \"Second, ... For instance, ...\"\n4. **Conclusion** (5 sec): \"That's why I think...\"\n\n### Tips\n- Speak clearly, not fast.\n- Use **transition words**: First, Second, However, Therefore.\n- It's okay to pause briefly — better than filler words."),

            L("Integrated Writing", 1,
              "Reading–Listening–Writing Strategy",
              "## TOEFL Integrated Writing\n\nYou read a passage (3 min), listen to a lecture, then write 150–225 words showing how the lecture **challenges** the reading.\n\n### Template Structure\n1. **Intro**: \"The reading states X. However, the lecturer argues Y.\"\n2. **Body 1**: Reading point 1 → Lecture counter-argument 1.\n3. **Body 2**: Reading point 2 → Lecture counter-argument 2.\n4. **Body 3**: Reading point 3 → Lecture counter-argument 3.\n\n### Key Phrases\n- \"The reading claims... whereas the professor contends...\"\n- \"This directly contradicts the idea that...\"\n- \"According to the lecturer,...\""),

            // ──── NUET ─────────────────────────────────────────
            L("Algebra & Functions", 1,
              "Core Algebra Concepts",
              "## NUET Algebra & Functions\n\n### Expressions & Simplification\n- Combine like terms: $3x + 5x = 8x$\n- Distribute: $a(b + c) = ab + ac$\n- Factor: $x^2 - 9 = (x-3)(x+3)$\n\n### Functions\n- $f(x) = 2x + 1$ means \"plug in x and compute.\"\n- **Domain**: all valid inputs.\n- **Range**: all possible outputs.\n- **Composition**: $f(g(x))$ — apply g first, then f.\n\n### Inequalities\n- Flip the sign when multiplying/dividing by a negative:\n$$-2x > 6 \\Rightarrow x < -3$$",
              null),

            L("Problem Solving", 1,
              "Problem-Solving Strategies",
              "## NUET Problem Solving\n\n### General Approach\n1. **Read carefully** — identify what is asked.\n2. **Choose a method**: algebraic, numerical, or diagrammatic.\n3. **Estimate first** — eliminate clearly wrong answers.\n4. **Work backwards** from answer choices if stuck.\n\n### Common Patterns\n- **Rate × Time = Distance**\n- **Percentage**: part/whole × 100\n- **Ratio & Proportion**: cross-multiply to solve.\n\n### Tips\n- Draw diagrams for geometry problems.\n- Convert units early (meters to cm, etc.).\n- Check your answer against the question's constraints."),

            L("Logical Reasoning", 1,
              "Logical Arguments & Fallacies",
              "## NUET Logical Reasoning\n\n### Argument Structure\n- **Premise**: a statement assumed to be true.\n- **Conclusion**: what follows from the premises.\n\n### Validity vs. Truth\n- An argument can be **logically valid** even if premises are false.\n- A **sound** argument is valid AND has true premises.\n\n### Common Fallacies\n| Fallacy | Description |\n|---------|-------------|\n| Ad hominem | Attacking the person, not the argument |\n| Straw man | Misrepresenting the opponent's position |\n| False dilemma | Presenting only two options when more exist |\n| Circular reasoning | Conclusion restates the premise |",
              null),

            L("Argument Analysis", 1,
              "Evaluating Arguments",
              "## NUET Argument Analysis\n\n### What to Look For\n1. **Identify the conclusion** — what is being argued?\n2. **Find the evidence** — what supports it?\n3. **Assess the gap** — is the logic strong?\n\n### Strengthen / Weaken Questions\n- To **strengthen**: find evidence that supports the conclusion.\n- To **weaken**: find evidence that challenges the link between premise and conclusion.\n\n### Assumption Questions\nAn assumption is an **unstated premise** the argument depends on.\n- Ask: \"If this were NOT true, would the argument fall apart?\"\n- If yes → it's a necessary assumption."),

            // ──── IELTS ────────────────────────────────────────
            L("IELTS Listening Comprehension", 1,
              "IELTS Listening Overview",
              "## IELTS Listening Test\n\nThe listening test lasts approximately **30 minutes** (plus 10 minutes transfer time) and has **4 sections** with 40 questions total.\n\n### Section Breakdown\n| Section | Context | Speakers |\n|---------|---------|----------|\n| 1 | Everyday social context | 2 speakers |\n| 2 | Everyday social context | 1 speaker |\n| 3 | Educational/training context | 2–4 speakers |\n| 4 | Academic lecture | 1 speaker |\n\n### Question Types\n- **Multiple choice**: Choose one or more correct answers.\n- **Matching**: Match items from a list.\n- **Map/plan/diagram labelling**: Label places on a visual.\n- **Note/form completion**: Fill in missing words.\n\n### Key Strategies\n1. **Read ahead** — use the time before each section to preview questions.\n2. **Listen for signpost words**: 'however', 'for example', 'in other words'.\n3. **Write answers as you hear them** — don't rely on memory.\n4. **Spelling counts** — practise common academic words."),

            L("IELTS Listening Comprehension", 2,
              "Predicting Answers & Distractors",
              "## Predicting & Avoiding Traps\n\n### Prediction Strategy\nBefore you listen, look at each question and predict:\n- The **type of word** needed (noun, number, name).\n- The **topic area** the answer will relate to.\n\n### Common Distractors\n- **Changed answers**: A speaker says one thing then corrects themselves. *\"The meeting is on Tuesday... actually, sorry, Wednesday.\"*\n- **Multiple speakers**: One person suggests something, another disagrees.\n- **Paraphrasing**: The question uses different words from the audio.\n\n### Practice Tips\n- Listen to BBC podcasts and academic lectures.\n- Practice with the official IELTS listening samples.\n- Time yourself to build stamina for the 30-minute test."),

            L("IELTS Note & Form Completion", 1,
              "Completing Notes & Forms",
              "## Note & Form Completion\n\nThese questions appear mainly in Sections 1 and 2.\n\n### How It Works\n- You see a form, table, or set of notes with gaps.\n- Listen and fill in the missing words.\n- Usually **no more than 3 words and/or a number**.\n\n### Strategy\n1. **Read the instructions** — note the word limit.\n2. **Predict the answer type**: name, date, number, or noun.\n3. **Follow the order** — answers come in sequence.\n4. **Check grammar** — your answer must fit grammatically.\n\n### Common Formats\n- Booking forms: name, address, phone number, dates.\n- Class/course information: times, locations, requirements.\n- Notes from a talk: main points summarised."),

            L("IELTS Academic Reading", 1,
              "IELTS Reading Overview",
              "## IELTS Academic Reading\n\nThe reading test lasts **60 minutes** with **3 passages** and **40 questions** total.\n\n### Passage Topics\n- Science, technology, history, sociology, education.\n- Passages increase in difficulty from 1 to 3.\n- Each passage is 700–900 words.\n\n### Question Types\n1. **True / False / Not Given**: Does the passage agree, disagree, or not mention this?\n2. **Matching headings**: Choose the best heading for each paragraph.\n3. **Sentence completion**: Complete sentences using words from the passage.\n4. **Multiple choice**: Select the correct answer.\n5. **Summary completion**: Fill gaps in a summary.\n\n### Reading Strategy\n- **Skim first** (2 min): Read the title, first & last sentences.\n- **Scan for answers**: Use keywords from questions to locate relevant parts.\n- **Don't read word-by-word** — you don't have time."),

            L("IELTS Academic Reading", 2,
              "Identifying the Writer's Views",
              "## Writer's Views & Claims\n\n### Yes / No / Not Given Questions\nThese test your ability to identify the **writer's opinion** (not just facts).\n\n- **Yes**: The writer clearly agrees with the statement.\n- **No**: The writer clearly disagrees.\n- **Not Given**: The writer doesn't express an opinion on this.\n\n### Tips\n- Look for opinion language: 'I believe', 'it is argued', 'evidence suggests'.\n- **Not Given** means the topic might be mentioned but the specific claim isn't addressed.\n- Don't use your own knowledge — only use what's in the passage."),

            L("IELTS Reading: Matching & True/False", 1,
              "True/False/Not Given Strategy",
              "## True / False / Not Given (TFNG)\n\nThis is the most common IELTS Reading question type.\n\n### Definitions\n- **TRUE**: The passage states exactly this (may use different words).\n- **FALSE**: The passage says the opposite.\n- **NOT GIVEN**: The passage doesn't mention this specific point.\n\n### Strategy\n1. Underline **keywords** in the question.\n2. **Locate** the relevant part of the passage.\n3. Compare **carefully** — paraphrasing is common.\n4. If you can't find the information, it's likely NOT GIVEN.\n\n### Common Mistakes\n- Confusing FALSE with NOT GIVEN.\n- Using outside knowledge instead of the passage.\n- Assuming something is TRUE because it *could* be true."),

            L("IELTS Reading: Matching & True/False", 2,
              "Matching Headings & Information",
              "## Matching Questions\n\n### Matching Headings\n- Match a heading to each paragraph.\n- Read the **first and last sentence** of each paragraph.\n- Eliminate headings that are too specific or too general.\n\n### Matching Information\n- Match statements to the correct paragraph.\n- You may need to read specific parts more carefully.\n- Answers are NOT in order — scan the text for each statement.\n\n### Matching Features\n- Match researchers/dates/places to findings.\n- Look for **proper nouns** as scanning targets.\n- The answer is usually in the sentence where the name appears."),

            L("IELTS Task 1: Data Description", 1,
              "Describing Charts & Graphs",
              "## IELTS Writing Task 1\n\nYou must write **at least 150 words** in **20 minutes** describing visual data.\n\n### Structure\n1. **Introduction** (paraphrase the question): \"The chart illustrates...\"\n2. **Overview** (2 key trends): \"Overall, ... while ...\"\n3. **Body 1**: Describe the first set of data with specific figures.\n4. **Body 2**: Describe the second set, making comparisons.\n\n### Useful Language\n| Trend | Vocabulary |\n|-------|------------|\n| Increase | rose, climbed, surged, grew |\n| Decrease | fell, dropped, declined, plummeted |\n| No change | remained stable, stayed constant |\n| Fluctuation | fluctuated, varied |\n\n### Top Tips\n- **Do NOT** give your opinion.\n- **Always include** an overview paragraph.\n- Use **specific data** (numbers, percentages, years)."),

            L("IELTS Task 2: Essay Writing", 1,
              "Essay Structure & Planning",
              "## IELTS Writing Task 2\n\nWrite **at least 250 words** in **40 minutes**. Task 2 is worth **twice as much** as Task 1.\n\n### Common Essay Types\n1. **Opinion (Agree/Disagree)**: State and defend your view.\n2. **Discussion (Both Views)**: Present both sides, then your opinion.\n3. **Problem/Solution**: Outline causes and propose solutions.\n4. **Advantages/Disadvantages**: Weigh pros and cons.\n\n### Planning (5 minutes)\n- Identify the **essay type** from the question.\n- Brainstorm **2 main ideas** for each body paragraph.\n- Plan your **thesis statement**.\n\n### Structure\n- **Introduction**: Hook + paraphrase + thesis (2–3 sentences).\n- **Body 1**: Topic sentence + explanation + example.\n- **Body 2**: Topic sentence + explanation + example.\n- **Conclusion**: Restate thesis + final thought.\n\n### Band 7+ Tips\n- Use **complex sentences**: 'Although..., ...' / 'Not only... but also...'\n- Avoid **repetition** — use synonyms.\n- **Link ideas** clearly: Furthermore, However, Consequently."),

            L("IELTS Speaking Parts 1 & 2", 1,
              "Speaking Parts 1 & 2 Guide",
              "## IELTS Speaking: Parts 1 & 2\n\n### Part 1 (4–5 minutes)\nThe examiner asks **general questions** about familiar topics: home, work, studies, hobbies.\n\n**Strategy:**\n- Give **extended answers** (2–3 sentences), not just 'yes' or 'no'.\n- Use a **reason + example** pattern.\n- Be **natural** — don't memorise scripts.\n\n### Part 2 (3–4 minutes)\nYou receive a **cue card** with a topic and 3–4 bullet points. You have **1 minute to prepare** and **1–2 minutes to speak**.\n\n**Strategy:**\n1. **Jot down key words** (not sentences) for each bullet.\n2. **Start with a clear opening**: \"I'd like to talk about...\"\n3. **Cover all bullet points** — examiners check this.\n4. **Extend naturally** — add feelings, opinions, reasons.\n\n### Fluency Tips\n- Use **fillers naturally**: 'Well,', 'Actually,', 'You know,'.\n- **Self-correct** if you make a mistake — this is positive.\n- **Vary your intonation** — don't speak in a monotone."),

            L("IELTS Speaking Part 3: Discussion", 1,
              "Part 3: Abstract Discussion",
              "## IELTS Speaking Part 3\n\nThe examiner asks **abstract, analytical questions** related to the Part 2 topic.\n\n### What Examiners Look For\n- **Depth of response**: Can you explain and justify your views?\n- **Complex language**: Conditionals, passives, modals.\n- **Coherence**: Structured, logical answers.\n\n### Response Framework\n1. **State your position**: \"I believe that...\"\n2. **Explain why**: \"The main reason is...\"\n3. **Give an example**: \"For instance, in my country...\"\n4. **Consider the other side**: \"However, some might argue...\"\n5. **Conclude**: \"So overall, I think...\"\n\n### Useful Phrases for Band 7+\n- \"That's an interesting question. I'd say that...\"\n- \"It's a complex issue, but generally speaking...\"\n- \"From my perspective,...\"\n- \"There's a strong argument that...\"\n- \"I'm inclined to think that...\""),

            // ──── CSCA ─────────────────────────────────────────
            L("Calculus & Analysis", 1,
              "Derivatives & Differentiation",
              "## Differentiation\n\nThe derivative measures the **rate of change** of a function.\n\n### Basic Rules\n| Rule | Formula |\n|------|--------|\n| Power Rule | $\\frac{d}{dx} x^n = nx^{n-1}$ |\n| Constant Rule | $\\frac{d}{dx} c = 0$ |\n| Sum Rule | $(f + g)' = f' + g'$ |\n| Product Rule | $(fg)' = f'g + fg'$ |\n| Chain Rule | $(f(g(x)))' = f'(g(x)) \\cdot g'(x)$ |\n\n### Example\n$$f(x) = 3x^4 - 2x^2 + 7$$\n$$f'(x) = 12x^3 - 4x$$\n\n### Applications\n- **Tangent line slope** at a point.\n- **Velocity** from a position function.\n- **Finding maxima/minima** (set f'(x) = 0)."),

            L("Calculus & Analysis", 2,
              "Integration & Area Under Curves",
              "## Integration\n\nIntegration is the **reverse of differentiation**.\n\n### Definite Integral\n$$\\int_a^b f(x)\\,dx = F(b) - F(a)$$\n\nwhere $F$ is an antiderivative of $f$.\n\n### Basic Rules\n| Rule | Formula |\n|------|--------|\n| Power Rule | $\\int x^n\\,dx = \\frac{x^{n+1}}{n+1} + C$ |\n| Constant | $\\int c\\,dx = cx + C$ |\n| Sum | $\\int (f+g)\\,dx = \\int f\\,dx + \\int g\\,dx$ |\n\n### Example\n$$\\int_1^3 (2x + 1)\\,dx = [x^2 + x]_1^3 = (9+3) - (1+1) = 10$$\n\n### Applications\n- **Area under a curve**.\n- **Total distance** from velocity.\n- **Accumulation** problems."),

            L("Probability & Statistics", 1,
              "Probability Fundamentals",
              "## Probability Basics\n\n### Key Definitions\n- **Experiment**: An action producing measurable outcomes.\n- **Sample space**: Set of all possible outcomes.\n- **Event**: A subset of the sample space.\n\n### Formulas\n$$P(A) = \\frac{|A|}{|S|}$$\n\n$$P(A \\cup B) = P(A) + P(B) - P(A \\cap B)$$\n\n$$P(A | B) = \\frac{P(A \\cap B)}{P(B)}$$\n\n### Independence\nEvents A and B are independent if:\n$$P(A \\cap B) = P(A) \\cdot P(B)$$\n\n### Bayes' Theorem\n$$P(A|B) = \\frac{P(B|A)P(A)}{P(B)}$$"),

            L("Probability & Statistics", 2,
              "Descriptive Statistics",
              "## Descriptive Statistics\n\n### Measures of Central Tendency\n- **Mean**: $\\bar{x} = \\frac{\\sum x_i}{n}$\n- **Median**: Middle value when sorted.\n- **Mode**: Most frequent value.\n\n### Measures of Spread\n- **Range**: max − min\n- **Variance**: $\\sigma^2 = \\frac{\\sum(x_i - \\bar{x})^2}{n}$\n- **Standard Deviation**: $\\sigma = \\sqrt{\\sigma^2}$\n\n### Normal Distribution\n- Bell-shaped, symmetric around the mean.\n- **68-95-99.7 Rule**:\n  - 68% within $\\pm 1\\sigma$\n  - 95% within $\\pm 2\\sigma$\n  - 99.7% within $\\pm 3\\sigma$"),

            L("Discrete Mathematics", 1,
              "Sets, Counting & Combinatorics",
              "## Sets & Counting\n\n### Set Operations\n- **Union**: $A \\cup B$ — elements in A or B.\n- **Intersection**: $A \\cap B$ — elements in both.\n- **Complement**: $A'$ — elements not in A.\n\n### Counting Principles\n- **Addition principle**: If A has $m$ ways and B has $n$ ways (mutually exclusive), total = $m + n$.\n- **Multiplication principle**: If step 1 has $m$ ways and step 2 has $n$ ways, total = $m \\times n$.\n\n### Permutations & Combinations\n$$P(n, r) = \\frac{n!}{(n-r)!}$$\n$$C(n, r) = \\binom{n}{r} = \\frac{n!}{r!(n-r)!}$$\n\n### Example\nChoosing a committee of 3 from 10 people: $\\binom{10}{3} = 120$."),

            L("Discrete Mathematics", 2,
              "Graph Theory Basics",
              "## Graph Theory\n\n### Definitions\n- **Graph**: A set of **vertices** (nodes) connected by **edges**.\n- **Degree**: Number of edges incident to a vertex.\n- **Path**: Sequence of vertices connected by edges.\n- **Cycle**: A path that starts and ends at the same vertex.\n\n### Key Theorems\n- **Handshaking Lemma**: $\\sum \\deg(v) = 2|E|$\n- **Euler's Formula** (planar graphs): $V - E + F = 2$\n\n### Types of Graphs\n| Type | Description |\n|------|------------|\n| Complete ($K_n$) | Every pair of vertices is connected |\n| Bipartite | Vertices split into two sets with edges only between sets |\n| Tree | Connected graph with no cycles |\n\n### Applications\n- Network routing, scheduling, social network analysis."),

            L("Formal Logic", 1,
              "Propositional Logic",
              "## Propositional Logic\n\n### Logical Connectives\n| Symbol | Name | Meaning |\n|--------|------|---------|\n| $\\neg$ | Negation | NOT |\n| $\\wedge$ | Conjunction | AND |\n| $\\vee$ | Disjunction | OR |\n| $\\rightarrow$ | Implication | IF...THEN |\n| $\\leftrightarrow$ | Biconditional | IF AND ONLY IF |\n\n### Truth Tables\nFor $P \\rightarrow Q$:\n| P | Q | P → Q |\n|---|---|-------|\n| T | T | T |\n| T | F | F |\n| F | T | T |\n| F | F | T |\n\n### Key Equivalences\n- **Contrapositive**: $P \\rightarrow Q \\equiv \\neg Q \\rightarrow \\neg P$\n- **De Morgan's**: $\\neg(P \\wedge Q) \\equiv \\neg P \\vee \\neg Q$\n- **De Morgan's**: $\\neg(P \\vee Q) \\equiv \\neg P \\wedge \\neg Q$"),

            L("Algorithmic Thinking", 1,
              "Algorithm Design & Complexity",
              "## Algorithms & Complexity\n\n### What is an Algorithm?\nA **finite set of instructions** that solves a problem step by step.\n\n### Big-O Notation\nDescribes how an algorithm scales with input size $n$.\n\n| Complexity | Name | Example |\n|-----------|-------|--------|\n| $O(1)$ | Constant | Array access |\n| $O(\\log n)$ | Logarithmic | Binary search |\n| $O(n)$ | Linear | Linear search |\n| $O(n \\log n)$ | Linearithmic | Merge sort |\n| $O(n^2)$ | Quadratic | Bubble sort |\n| $O(2^n)$ | Exponential | Naive Fibonacci |\n\n### Common Patterns\n- **Divide and conquer**: Split problem into sub-problems (merge sort).\n- **Greedy**: Make locally optimal choices (coin change).\n- **Dynamic programming**: Store sub-problem results to avoid recomputation."),

            L("Data Interpretation", 1,
              "Reading & Analysing Data",
              "## Data Interpretation\n\n### Chart Types\n| Chart | Best For |\n|-------|----------|\n| Bar chart | Comparing categories |\n| Line graph | Trends over time |\n| Pie chart | Parts of a whole |\n| Scatter plot | Correlation between variables |\n\n### Key Skills\n1. **Read the axes** — understand units and scale.\n2. **Identify trends** — increasing, decreasing, stable.\n3. **Calculate differences** — absolute and percentage.\n4. **Draw conclusions** — what does the data suggest?\n\n### Common Calculations\n- **Percentage change**: $\\frac{\\text{new} - \\text{old}}{\\text{old}} \\times 100\\%$\n- **Average**: Sum / Count\n- **Proportion**: Part / Total")
        };

        await _context.TopicLessons.AddRangeAsync(lessons);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds hints for existing questions (step-by-step guidance without revealing the answer).
    /// </summary>
    private async Task SeedQuestionHintsAsync()
    {
        var questions = await _context.Questions.Include(q => q.Topic).ToListAsync();

        // Map of topic → hint templates by difficulty
        var hintMap = new Dictionary<string, Dictionary<QuestionDifficulty, string>>
        {
            ["Main Idea & Summary"] = new()
            {
                [QuestionDifficulty.Easy] = "Focus on the first and last sentences of the passage. What single idea connects them?",
                [QuestionDifficulty.Medium] = "Ask yourself: if I had to describe this passage in ONE sentence, what would it be? Eliminate answers that are too specific or too broad.",
                [QuestionDifficulty.Hard] = "Consider the author's PURPOSE, not just the topic. What is the author trying to convince you of? Look for qualifying words like 'however,' 'therefore,' or 'most importantly.'"
            },
            ["Grammar & Sentence Structure"] = new()
            {
                [QuestionDifficulty.Easy] = "Read the sentence aloud — does it sound right? Check that the subject and verb agree in number (singular/plural).",
                [QuestionDifficulty.Medium] = "Identify the main subject first, ignoring any phrases between the subject and verb. Then check: is the verb form correct?",
                [QuestionDifficulty.Hard] = "Break the sentence into clauses. Does each clause have a subject and verb? Check for fragments, run-ons, and parallel structure."
            },
            ["Vocabulary in Context"] = new()
            {
                [QuestionDifficulty.Easy] = "Substitute each answer choice into the sentence. Which one makes the most sense in the context?",
                [QuestionDifficulty.Medium] = "Look at the tone of the passage — is it positive, negative, or neutral? Eliminate words that don't match the tone.",
                [QuestionDifficulty.Hard] = "Consider the SECONDARY meaning of the word. Many SAT vocabulary questions test less common definitions. Read the surrounding sentences for clues."
            },
            ["Linear Equations"] = new()
            {
                [QuestionDifficulty.Easy] = "Isolate the variable by performing the same operation on both sides. Start by subtracting, then divide.",
                [QuestionDifficulty.Medium] = "First simplify both sides (distribute, combine like terms). Then use inverse operations to solve for the variable.",
                [QuestionDifficulty.Hard] = "Set up the equation from the word problem first. Identify what the variable represents, then translate the relationships."
            },
            ["Geometry"] = new()
            {
                [QuestionDifficulty.Easy] = "Write down the formula you need. Plug in the values given and solve step by step.",
                [QuestionDifficulty.Medium] = "Draw a diagram if there isn't one. Label all known values and look for geometric relationships (supplementary angles, similar triangles).",
                [QuestionDifficulty.Hard] = "Break the figure into simpler shapes. Use the Pythagorean theorem or special triangles (30-60-90, 45-45-90) where applicable."
            },
            ["Data Analysis"] = new()
            {
                [QuestionDifficulty.Easy] = "Read the axis labels carefully. What does each axis represent? Find the data point the question asks about.",
                [QuestionDifficulty.Medium] = "Calculate the mean by summing all values and dividing by the count. For median, sort the values and find the middle one.",
                [QuestionDifficulty.Hard] = "Think about how adding or removing data points affects the mean vs. median. Outliers shift the mean but rarely change the median."
            },
            ["Quadratic Equations"] = new()
            {
                [QuestionDifficulty.Easy] = "Try factoring: find two numbers that multiply to c and add to b. Then set each factor equal to zero.",
                [QuestionDifficulty.Medium] = "If factoring doesn't work easily, use the quadratic formula: x = (-b ± √(b²-4ac)) / 2a.",
                [QuestionDifficulty.Hard] = "Start by finding the discriminant (b²-4ac) to determine how many solutions exist, then solve accordingly."
            },
            ["Academic Reading"] = new()
            {
                [QuestionDifficulty.Easy] = "Scan for keywords from the question in the passage. The answer is usually stated directly nearby.",
                [QuestionDifficulty.Medium] = "For inference questions, find the relevant paragraph, then ask: what can I logically conclude from these facts?",
                [QuestionDifficulty.Hard] = "For 'insert sentence' questions, check if the new sentence refers to concepts mentioned BEFORE the insertion point and introduces ideas discussed AFTER."
            },
            ["Lecture Comprehension"] = new()
            {
                [QuestionDifficulty.Easy] = "Think about the main topic the professor introduced at the beginning. The correct answer usually relates to the central theme.",
                [QuestionDifficulty.Medium] = "Recall any examples the professor gave. What point was each example supporting?",
                [QuestionDifficulty.Hard] = "Pay attention to the professor's tone shifts. When they say 'but' or 'actually,' a key contrast or correction follows."
            },
            ["Conversation Understanding"] = new()
            {
                [QuestionDifficulty.Easy] = "Focus on WHY the student started the conversation. What problem or question do they bring up?",
                [QuestionDifficulty.Medium] = "Listen for the RESULT of the conversation. What does the student decide to do?",
                [QuestionDifficulty.Hard] = "Notice the speaker's attitude — are they confident, uncertain, frustrated? This often determines the correct answer for 'attitude' questions."
            },
            ["Independent Speaking"] = new()
            {
                [QuestionDifficulty.Easy] = "State your opinion clearly, then give ONE specific reason with an example from your own experience.",
                [QuestionDifficulty.Medium] = "Organize your response: Opinion → Reason 1 + Example → Reason 2 + Example → Brief conclusion.",
                [QuestionDifficulty.Hard] = "Consider both sides before choosing. Briefly acknowledge the other view, then explain why your position is stronger."
            },
            ["Integrated Writing"] = new()
            {
                [QuestionDifficulty.Easy] = "Identify one point from the reading, then explain how the lecture responds to that point.",
                [QuestionDifficulty.Medium] = "Structure your essay: for each reading point, state the claim, then describe the lecture's counter-argument.",
                [QuestionDifficulty.Hard] = "Be specific about HOW the lecture contradicts the reading — use evidence from both sources, not just summaries."
            },
            ["Algebra & Functions"] = new()
            {
                [QuestionDifficulty.Easy] = "Plug the given value into the function and simplify step by step. Follow order of operations (PEMDAS).",
                [QuestionDifficulty.Medium] = "Simplify the expression first (factor, distribute). Then isolate the variable to solve.",
                [QuestionDifficulty.Hard] = "For composition f(g(x)): compute g(x) first, then substitute that result into f. Check the domain restrictions."
            },
            ["Problem Solving"] = new()
            {
                [QuestionDifficulty.Easy] = "Identify what the question asks for. Write down the given information and the formula you need.",
                [QuestionDifficulty.Medium] = "Estimate the answer first to eliminate obviously wrong choices. Then compute carefully.",
                [QuestionDifficulty.Hard] = "Try working backwards from the answer choices — plug each one into the problem to see which satisfies all conditions."
            },
            ["Logical Reasoning"] = new()
            {
                [QuestionDifficulty.Easy] = "Find the conclusion first (what is being claimed). Then identify what evidence supports it.",
                [QuestionDifficulty.Medium] = "Look for the GAP between the evidence and the conclusion. The correct answer often addresses this gap.",
                [QuestionDifficulty.Hard] = "For 'assumption' questions, try negating each answer choice. If negating it destroys the argument, it's a necessary assumption."
            },
            ["Argument Analysis"] = new()
            {
                [QuestionDifficulty.Easy] = "Ask: What is the author's main point? What evidence do they provide?",
                [QuestionDifficulty.Medium] = "To weaken an argument, find evidence that breaks the link between premise and conclusion. To strengthen, find supporting evidence.",
                [QuestionDifficulty.Hard] = "Identify the unstated assumption. The argument depends on something not explicitly mentioned — if that assumption fails, the argument collapses."
            },
            // IELTS Hints
            ["IELTS Listening Comprehension"] = new()
            {
                [QuestionDifficulty.Easy] = "Listen for specific details like times, names, and numbers — they are usually stated clearly.",
                [QuestionDifficulty.Medium] = "Pay attention to signal words: 'however', 'actually', 'the most important'. They introduce key information.",
                [QuestionDifficulty.Hard] = "Watch for distractors — speakers may mention multiple options and then correct themselves. The last stated answer is usually correct."
            },
            ["IELTS Note & Form Completion"] = new()
            {
                [QuestionDifficulty.Easy] = "Read the form/notes first and predict what type of word is needed (name, number, noun).",
                [QuestionDifficulty.Medium] = "Follow the order of the audio — answers appear in sequence. Don't get stuck on one answer.",
                [QuestionDifficulty.Hard] = "Check the word limit carefully. If it says 'no more than two words', three words will be wrong even if correct in meaning."
            },
            ["IELTS Academic Reading"] = new()
            {
                [QuestionDifficulty.Easy] = "Scan for keywords from the question in the passage. The answer is usually in the same sentence or the next one.",
                [QuestionDifficulty.Medium] = "Look for paraphrasing — the passage will use different words to express the same idea as the question.",
                [QuestionDifficulty.Hard] = "For 'tone' and 'attitude' questions, look for adjectives and adverbs that reveal the author's position."
            },
            ["IELTS Reading: Matching & True/False"] = new()
            {
                [QuestionDifficulty.Easy] = "For TFNG: if the passage says the same thing in different words, it's TRUE. If it says the opposite, it's FALSE.",
                [QuestionDifficulty.Medium] = "For matching, scan for proper nouns (names, places) first — they're easy to locate in the passage.",
                [QuestionDifficulty.Hard] = "NOT GIVEN is the trickiest option — the topic might be mentioned, but the specific claim in the question is not addressed."
            },
            ["IELTS Task 1: Data Description"] = new()
            {
                [QuestionDifficulty.Easy] = "Use clear trend language: 'rose', 'fell', 'remained stable'. Avoid vague words like 'went up'.",
                [QuestionDifficulty.Medium] = "Always include an overview paragraph identifying the most notable trends and differences.",
                [QuestionDifficulty.Hard] = "Use precise comparison structures: 'twice as many as', 'three times higher than', 'the majority of'."
            },
            ["IELTS Task 2: Essay Writing"] = new()
            {
                [QuestionDifficulty.Easy] = "Your thesis statement should clearly state your opinion or position on the question.",
                [QuestionDifficulty.Medium] = "Each body paragraph needs: topic sentence → explanation → specific example.",
                [QuestionDifficulty.Hard] = "For Band 7+, use a mix of simple and complex sentences. Include conditionals and relative clauses."
            },
            ["IELTS Speaking Parts 1 & 2"] = new()
            {
                [QuestionDifficulty.Easy] = "Extend your Part 1 answers with a reason and an example. Don't give one-word answers.",
                [QuestionDifficulty.Medium] = "In Part 2 preparation, jot key words for each bullet point. Make sure you cover all of them.",
                [QuestionDifficulty.Hard] = "Use a variety of tenses (past, present, conditional) naturally to demonstrate grammatical range."
            },
            ["IELTS Speaking Part 3: Discussion"] = new()
            {
                [QuestionDifficulty.Medium] = "Structure your answer: opinion → reason → example → alternative view.",
                [QuestionDifficulty.Hard] = "If the question is difficult, paraphrase it aloud and give a tentative answer — this shows communication skills."
            },
            // CSCA Hints
            ["Calculus & Analysis"] = new()
            {
                [QuestionDifficulty.Easy] = "Apply the power rule: bring down the exponent and subtract 1. d/dx(x^n) = n·x^(n-1).",
                [QuestionDifficulty.Medium] = "For definite integrals, find the antiderivative first, then evaluate at the upper and lower bounds.",
                [QuestionDifficulty.Hard] = "For second derivatives, differentiate twice. Check your work at each step."
            },
            ["Probability & Statistics"] = new()
            {
                [QuestionDifficulty.Easy] = "Count favourable outcomes and total outcomes. P = favourable/total.",
                [QuestionDifficulty.Medium] = "For standard deviation, first find the mean, then calculate variance (average of squared deviations), then take the square root.",
                [QuestionDifficulty.Hard] = "Remember the 68-95-99.7 rule for normal distributions."
            },
            ["Discrete Mathematics"] = new()
            {
                [QuestionDifficulty.Easy] = "A set with n elements has 2^n subsets. Permutations of n items = n!.",
                [QuestionDifficulty.Medium] = "For combinations, use C(n,r) = n! / (r!(n-r)!). Order doesn't matter.",
                [QuestionDifficulty.Hard] = "The handshaking lemma: sum of vertex degrees = 2 × number of edges."
            },
            ["Formal Logic"] = new()
            {
                [QuestionDifficulty.Easy] = "Modus ponens: if P → Q and P is true, then Q is true.",
                [QuestionDifficulty.Medium] = "The contrapositive of P → Q is ¬Q → ¬P. It always has the same truth value.",
                [QuestionDifficulty.Hard] = "Use De Morgan's laws: ¬(P ∧ Q) ≡ ¬P ∨ ¬Q, and ¬(P ∨ Q) ≡ ¬P ∧ ¬Q."
            },
            ["Algorithmic Thinking"] = new()
            {
                [QuestionDifficulty.Easy] = "Binary search is O(log n) because it halves the search space at each step.",
                [QuestionDifficulty.Medium] = "Naive recursive algorithms often have exponential time complexity due to redundant computations.",
                [QuestionDifficulty.Hard] = "Compare sorting algorithms by their average-case complexity: O(n log n) beats O(n²)."
            },
            ["Data Interpretation"] = new()
            {
                [QuestionDifficulty.Easy] = "Read the axis labels first. Calculate proportions as part/total × 100%.",
                [QuestionDifficulty.Medium] = "For growth rates, use (new − old) / number of periods.",
                [QuestionDifficulty.Hard] = "Look for misleading scales or cherry-picked data ranges that could distort the picture."
            }
        };

        foreach (var q in questions)
        {
            if (q.Topic != null && hintMap.TryGetValue(q.Topic.Name, out var difficultyHints))
            {
                if (difficultyHints.TryGetValue(q.Difficulty, out var hint))
                {
                    q.Hint = hint;
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>Seed TOEFL-style reading passages and link Academic Reading questions to them</summary>
    private async Task SeedReadingPassagesAsync()
    {
        var readingTopic = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "Academic Reading");
        if (readingTopic == null) return;

        var passages = new List<ReadingPassage>
        {
            new ReadingPassage
            {
                TopicId = readingTopic.Id,
                Title = "The Cambrian Explosion",
                SortOrder = 1,
                Content = @"The Cambrian Explosion, which occurred approximately 541 million years ago, represents one of the most significant events in the history of life on Earth. During a relatively brief period of geological time — roughly 20 to 25 million years — most of the major animal phyla that exist today first appeared in the fossil record. This dramatic diversification of life forms has puzzled scientists since Charles Darwin first noted it as a potential challenge to his theory of gradual evolution.

Before the Cambrian period, life on Earth was dominated by simple, mostly single-celled organisms and some soft-bodied multicellular forms known collectively as the Ediacaran biota. These organisms left limited fossil evidence, making it difficult for paleontologists to trace the evolutionary lineage connecting them to the complex animals that suddenly appeared in Cambrian rocks.

Several hypotheses have been proposed to explain this rapid diversification. One leading theory suggests that rising atmospheric oxygen levels reached a critical threshold that allowed larger, more metabolically active organisms to evolve. Another hypothesis points to the evolution of predation as a driving force: once some organisms developed the ability to consume others, an evolutionary arms race began, leading to the rapid development of shells, skeletons, eyes, and other defensive and sensory structures.

Recent genetic studies have revealed that many of the genes responsible for animal body plans — known as Hox genes — were already present before the Cambrian Explosion. This suggests that the genetic toolkit for complex body forms existed well before these forms actually appeared in the fossil record. Environmental triggers, rather than genetic innovation alone, may have been the key factor in unleashing this burst of evolutionary creativity.

The Cambrian Explosion remains an active area of research, with new fossil discoveries from sites such as the Burgess Shale in Canada and the Chengjiang formation in China continuing to reshape our understanding of early animal evolution."
            },
            new ReadingPassage
            {
                TopicId = readingTopic.Id,
                Title = "Urban Heat Islands",
                SortOrder = 2,
                Content = @"Urban heat islands (UHIs) are metropolitan areas that are significantly warmer than their surrounding rural areas due to human activities and modifications to the landscape. The temperature difference between an urban center and its rural surroundings can range from 1°C to as much as 12°C, depending on factors such as city size, population density, and climate conditions. This phenomenon was first documented in the early 19th century by the amateur meteorologist Luke Howard, who recorded temperature differences between central London and the surrounding countryside.

The primary cause of urban heat islands is the replacement of natural vegetation with impervious surfaces such as asphalt, concrete, and buildings. These materials absorb and store solar radiation more efficiently than natural landscapes and release this stored heat slowly, particularly during nighttime hours. Additionally, the geometric arrangement of tall buildings in urban canyons traps heat by reducing airflow and reflecting solar radiation between surfaces.

Human activities contribute significantly to the UHI effect. Vehicles, air conditioning systems, industrial operations, and even human metabolism all generate waste heat that raises ambient temperatures. In dense urban areas, the concentration of these heat sources can significantly amplify the temperature differential.

The consequences of urban heat islands extend beyond mere discomfort. Higher temperatures increase energy demand for cooling, leading to greater electricity consumption and higher greenhouse gas emissions. Heat-related health risks, including heat stroke, dehydration, and cardiovascular stress, disproportionately affect vulnerable populations such as the elderly, children, and those with pre-existing health conditions. Furthermore, elevated temperatures can worsen air quality by accelerating the formation of ground-level ozone and other pollutants.

Mitigation strategies include increasing urban tree canopy coverage, installing green roofs and cool reflective surfaces, implementing permeable pavements, and redesigning urban spaces to promote natural ventilation. Cities such as Singapore, Melbourne, and New York have implemented ambitious greening programs that have demonstrated measurable reductions in local temperatures."
            },
            new ReadingPassage
            {
                TopicId = readingTopic.Id,
                Title = "The Psychology of Decision-Making",
                SortOrder = 3,
                Content = @"For much of the 20th century, economists and psychologists assumed that human beings make decisions rationally — that is, they gather available information, weigh the costs and benefits of each option, and choose the one that maximizes their utility or satisfaction. This model of human behavior, known as the rational actor model, formed the basis of classical economic theory and influenced fields ranging from public policy to marketing.

However, beginning in the 1970s, psychologists Daniel Kahneman and Amos Tversky published a series of groundbreaking studies demonstrating that human decision-making is systematically biased in predictable ways. Their research, which eventually earned Kahneman the Nobel Prize in Economics in 2002, revealed that people rely on mental shortcuts called heuristics when making judgments under uncertainty. While these heuristics are often useful, they can also lead to significant errors.

One of the most well-documented heuristics is anchoring — the tendency to rely too heavily on the first piece of information encountered when making decisions. In experiments, Kahneman and Tversky showed that even arbitrary numbers could influence people's estimates of completely unrelated quantities. For example, participants who were first asked whether the percentage of African nations in the United Nations was higher or lower than 65% subsequently gave much higher estimates than those who were first asked about a comparison number of 10%.

Another important finding is loss aversion — the observation that people feel the pain of losing something roughly twice as strongly as the pleasure of gaining something of equal value. This asymmetry explains why people often make irrational choices to avoid losses, such as holding onto losing investments far too long or refusing fair gambles with positive expected values.

The implications of these findings have been profound. In the field of behavioral economics, researchers have developed ""nudge"" strategies that account for cognitive biases to encourage better decision-making without restricting freedom of choice. Applications include automatic enrollment in retirement savings plans, strategic placement of healthy food options in cafeterias, and simplified forms for government services."
            }
        };

        await _context.ReadingPassages.AddRangeAsync(passages);
        await _context.SaveChangesAsync();

        // Link existing Academic Reading questions to passages (distribute evenly)
        var readingQuestions = await _context.Questions
            .Where(q => q.TopicId == readingTopic.Id && q.ReadingPassageId == null)
            .OrderBy(q => q.Id)
            .ToListAsync();

        var savedPassages = await _context.ReadingPassages
            .Where(p => p.TopicId == readingTopic.Id)
            .OrderBy(p => p.SortOrder)
            .ToListAsync();

        if (savedPassages.Count > 0 && readingQuestions.Count > 0)
        {
            for (int i = 0; i < readingQuestions.Count; i++)
            {
                readingQuestions[i].ReadingPassageId = savedPassages[i % savedPassages.Count].Id;
            }
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>Seed mock exam templates for SAT, TOEFL, NUET</summary>
    private async Task SeedMockExamsAsync()
    {
        // Get section IDs
        var satRW = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "SAT" && s.Name == "Reading & Writing");
        var satMathNC = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "SAT" && s.Name == "Math (No Calculator)");
        var satMathC = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "SAT" && s.Name == "Math (Calculator)");
        var toeflReading = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "TOEFL" && s.Name == "Reading");
        var toeflListening = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "TOEFL" && s.Name == "Listening");
        var toeflSpeaking = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "TOEFL" && s.Name == "Speaking");
        var toeflWriting = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "TOEFL" && s.Name == "Writing");
        var nuetMath = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "NUET" && s.Name == "Math");
        var nuetCritical = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "NUET" && s.Name == "Critical Thinking");
        var ieltsListeningS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "IELTS" && s.Name == "Listening");
        var ieltsReadingS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "IELTS" && s.Name == "Reading");
        var ieltsWritingS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "IELTS" && s.Name == "Writing");
        var ieltsSpeakingS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "IELTS" && s.Name == "Speaking");
        var cscaMathS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Mathematics") && !s.Name.Contains("(CN)"));
        var cscaPhysS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Physics") && !s.Name.Contains("(CN)"));
        var cscaChemS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Chemistry") && !s.Name.Contains("(CN)"));
        var cscaCnTechS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Chinese Technical"));
        var cscaCnHumS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Chinese Humanitarian"));

        var mockExams = new List<MockExam>
        {
            // ── SAT Mock Exam ──────────────────────────────────
            new MockExam
            {
                ExamTypeCode = "SAT",
                Title = "SAT Practice Test",
                Description = "Full SAT practice test with Reading & Writing and Math sections. Simulates real exam conditions with timed sections. Real SAT has 154 questions (R&W 96 + Math 58); this practice version uses all available questions.",
                TotalTimeMinutes = 45,
                IsActive = true,
                Sections = new List<MockExamSection>
                {
                    new MockExamSection
                    {
                        ExamSectionId = satRW?.Id,
                        Name = "Reading & Writing",
                        TimeLimitMinutes = 15,
                        QuestionCount = 54,
                        SortOrder = 0,
                        Instructions = "This section measures your ability to comprehend, analyze, and use information and ideas presented in texts. Read each passage and question carefully, then select the best answer. You may refer back to the passage as often as needed. Time limit: 15 minutes."
                    },
                    new MockExamSection
                    {
                        ExamSectionId = satMathNC?.Id,
                        Name = "Math (No Calculator)",
                        TimeLimitMinutes = 15,
                        QuestionCount = 20,
                        SortOrder = 1,
                        Instructions = "This section tests your mathematical reasoning without the use of a calculator. You must show your understanding of concepts and perform calculations by hand. Focus on accuracy and efficient problem-solving. Time limit: 15 minutes."
                    },
                    new MockExamSection
                    {
                        ExamSectionId = satMathC?.Id,
                        Name = "Math (Calculator)",
                        TimeLimitMinutes = 15,
                        QuestionCount = 38,
                        SortOrder = 2,
                        Instructions = "This section tests your mathematical reasoning. A calculator is permitted for this section. Problems may involve more complex computations and data interpretation. Time limit: 15 minutes."
                    }
                }
            },
            // ── TOEFL Mock Exam ────────────────────────────────
            new MockExam
            {
                ExamTypeCode = "TOEFL",
                Title = "TOEFL iBT Practice Test",
                Description = "TOEFL iBT practice test covering Reading, Listening, Speaking, and Writing sections. The Reading section features academic passages with multiple questions per passage. Real TOEFL has ~80 questions in ~3.5 hours; this practice version uses available questions.",
                TotalTimeMinutes = 50,
                IsActive = true,
                Sections = new List<MockExamSection>
                {
                    new MockExamSection
                    {
                        ExamSectionId = toeflReading?.Id,
                        Name = "Reading",
                        TimeLimitMinutes = 18,
                        QuestionCount = 20,
                        SortOrder = 0,
                        Instructions = "Read the academic passages carefully and answer the questions that follow. Each passage is followed by a set of questions. You can navigate between questions within this section and change your answers. Time limit: 18 minutes."
                    },
                    new MockExamSection
                    {
                        ExamSectionId = toeflListening?.Id,
                        Name = "Listening",
                        TimeLimitMinutes = 12,
                        QuestionCount = 28,
                        SortOrder = 1,
                        Instructions = "Answer questions about academic lectures and campus conversations. In a real TOEFL test, you would listen to audio recordings. In this practice version, you will read transcribed excerpts. Time limit: 12 minutes."
                    },
                    new MockExamSection
                    {
                        ExamSectionId = toeflSpeaking?.Id,
                        Name = "Speaking",
                        TimeLimitMinutes = 10,
                        QuestionCount = 4,
                        SortOrder = 2,
                        Instructions = "Answer questions that test your ability to speak about familiar topics and synthesize information. In this practice version, select the best response option. Time limit: 10 minutes."
                    },
                    new MockExamSection
                    {
                        ExamSectionId = toeflWriting?.Id,
                        Name = "Writing",
                        TimeLimitMinutes = 10,
                        QuestionCount = 2,
                        SortOrder = 3,
                        Instructions = "Answer questions that test your ability to write in English in an academic context. In this practice version, select the best response option. Time limit: 10 minutes."
                    }
                }
            },
            // ── NUET Mock Exam ─────────────────────────────────
            new MockExam
            {
                ExamTypeCode = "NUET",
                Title = "NUET Practice Test",
                Description = "NUET practice test with Math and Critical Thinking sections. Each section is individually timed. Real NUET has ~80 questions in 2.5 hours; this practice version uses available questions.",
                TotalTimeMinutes = 50,
                IsActive = true,
                Sections = new List<MockExamSection>
                {
                    new MockExamSection
                    {
                        ExamSectionId = nuetMath?.Id,
                        Name = "Quantitative Reasoning",
                        TimeLimitMinutes = 25,
                        QuestionCount = 40,
                        SortOrder = 0,
                        Instructions = "This section tests your mathematical knowledge and problem-solving skills. Topics include algebra, functions, geometry, and applied mathematics. Work through each problem carefully. Time limit: 25 minutes."
                    },
                    new MockExamSection
                    {
                        ExamSectionId = nuetCritical?.Id,
                        Name = "Critical Thinking",
                        TimeLimitMinutes = 25,
                        QuestionCount = 40,
                        SortOrder = 1,
                        Instructions = "This section tests your logical reasoning and argument analysis skills. You will evaluate arguments, identify assumptions, draw conclusions, and analyze reasoning patterns. Time limit: 25 minutes."
                    }
                }
            },
            // ── IELTS Mock Exam ────────────────────────────────
            new MockExam
            {
                ExamTypeCode = "IELTS",
                Title = "IELTS Academic Practice Test",
                Description = "IELTS Academic practice test with Listening, Reading, Writing, and Speaking sections. Band scores range from 0 to 9. This practice version uses available question bank content.",
                TotalTimeMinutes = 60,
                IsActive = true,
                Sections = new List<MockExamSection>
                {
                    new MockExamSection
                    {
                        ExamSectionId = ieltsListeningS?.Id,
                        Name = "Listening",
                        TimeLimitMinutes = 15,
                        QuestionCount = 40,
                        SortOrder = 0,
                        Instructions = "The Listening section tests your ability to understand spoken English in academic and everyday contexts. In this practice version, read the transcript-based questions and select the best answer. Time limit: 15 minutes."
                    },
                    new MockExamSection
                    {
                        ExamSectionId = ieltsReadingS?.Id,
                        Name = "Reading",
                        TimeLimitMinutes = 20,
                        QuestionCount = 40,
                        SortOrder = 1,
                        Instructions = "The Academic Reading section contains three long texts from books, journals, and newspapers. Texts range from descriptive to analytical. Read carefully and answer the questions. Time limit: 20 minutes."
                    },
                    new MockExamSection
                    {
                        ExamSectionId = ieltsWritingS?.Id,
                        Name = "Writing",
                        TimeLimitMinutes = 15,
                        QuestionCount = 2,
                        SortOrder = 2,
                        Instructions = "The Writing section has two tasks: Task 1 (describe visual data, 150+ words) and Task 2 (write an essay, 250+ words). In this practice version, answer questions testing writing knowledge. Time limit: 15 minutes."
                    },
                    new MockExamSection
                    {
                        ExamSectionId = ieltsSpeakingS?.Id,
                        Name = "Speaking",
                        TimeLimitMinutes = 10,
                        QuestionCount = 3,
                        SortOrder = 3,
                        Instructions = "The Speaking section tests your ability to communicate effectively. In this practice version, select the best response strategies for different speaking scenarios. Time limit: 10 minutes."
                    }
                }
            },
            // ── CSCA Mock Exam — configurable: student picks subjects before starting ─────
            new MockExam
            {
                ExamTypeCode = "CSCA",
                Title = "CSCA Practice Test",
                Description = "Configurable CSCA practice test. Choose any combination of 8 subjects (EN/CN) before starting.",
                TotalTimeMinutes = 540,
                IsActive = true,
                Sections = new List<MockExamSection>
                {
                    new MockExamSection { ExamSectionId = cscaMathS?.Id, Name = "Mathematics (EN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 0, Instructions = "48 multiple-choice questions covering algebra, calculus, geometry, probability, and statistics. 60 minutes." },
                    new MockExamSection { ExamSectionId = cscaPhysS?.Id, Name = "Physics (EN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 1, Instructions = "48 multiple-choice questions on mechanics, thermodynamics, electromagnetism, and optics. 60 minutes." },
                    new MockExamSection { ExamSectionId = cscaChemS?.Id, Name = "Chemistry (EN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 2, Instructions = "48 multiple-choice questions on general, organic, and inorganic chemistry. 60 minutes." },
                    new MockExamSection { ExamSectionId = cscaMathS?.Id, Name = "Mathematics (CN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 3, Instructions = "48 вопросов по математике: алгебра, геометрия, анализ, вероятность и статистика. 60 минут." },
                    new MockExamSection { ExamSectionId = cscaPhysS?.Id, Name = "Physics (CN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 4, Instructions = "48 вопросов по физике: механика, термодинамика, электромагнетизм, оптика. 60 минут." },
                    new MockExamSection { ExamSectionId = cscaChemS?.Id, Name = "Chemistry (CN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 5, Instructions = "48 вопросов по химии: общая, органическая и неорганическая химия. 60 минут." },
                    new MockExamSection { ExamSectionId = cscaCnTechS?.Id, Name = "Chinese Technical", TimeLimitMinutes = 90, QuestionCount = 80, SortOrder = 6, Instructions = "80 вопросов: научная терминология, чтение технических текстов на китайском. 90 минут." },
                    new MockExamSection { ExamSectionId = cscaCnHumS?.Id, Name = "Chinese Humanitarian", TimeLimitMinutes = 90, QuestionCount = 80, SortOrder = 7, Instructions = "80 вопросов: литература, история, культура, философия Китая. 90 минут." }
                }
            }
        };

        await _context.MockExams.AddRangeAsync(mockExams);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds only IELTS and CSCA mock exams (for existing databases that already have SAT/TOEFL/NUET mocks).
    /// </summary>
    private async Task SeedIeltsCscaMockExamsAsync()
    {
        var ieltsListeningS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "IELTS" && s.Name == "Listening");
        var ieltsReadingS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "IELTS" && s.Name == "Reading");
        var ieltsWritingS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "IELTS" && s.Name == "Writing");
        var ieltsSpeakingS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "IELTS" && s.Name == "Speaking");
        var cscaMathS2 = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Mathematics") && !s.Name.Contains("(CN)"));
        var cscaPhysS2 = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Physics") && !s.Name.Contains("(CN)"));
        var cscaChemS2 = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Chemistry") && !s.Name.Contains("(CN)"));
        var cscaCnTechS2 = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Chinese Technical"));
        var cscaCnHumS2 = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Chinese Humanitarian"));

        var mockExams = new List<MockExam>
        {
            new MockExam
            {
                ExamTypeCode = "IELTS",
                Title = "IELTS Academic Practice Test",
                Description = "IELTS Academic practice test with Listening, Reading, Writing, and Speaking sections. Band scores range from 0 to 9.",
                TotalTimeMinutes = 60,
                IsActive = true,
                Sections = new List<MockExamSection>
                {
                    new MockExamSection { ExamSectionId = ieltsListeningS?.Id, Name = "Listening", TimeLimitMinutes = 15, QuestionCount = 40, SortOrder = 0, Instructions = "Listen carefully and answer the questions based on what you hear." },
                    new MockExamSection { ExamSectionId = ieltsReadingS?.Id, Name = "Reading", TimeLimitMinutes = 20, QuestionCount = 40, SortOrder = 1, Instructions = "Read the passages carefully and answer the questions." },
                    new MockExamSection { ExamSectionId = ieltsWritingS?.Id, Name = "Writing", TimeLimitMinutes = 15, QuestionCount = 2, SortOrder = 2, Instructions = "Complete the writing tasks." },
                    new MockExamSection { ExamSectionId = ieltsSpeakingS?.Id, Name = "Speaking", TimeLimitMinutes = 10, QuestionCount = 3, SortOrder = 3, Instructions = "Answer the speaking prompts." }
                }
            },
            // ── CSCA Mock Exam — configurable: student picks subjects before starting ─────
            new MockExam
            {
                ExamTypeCode = "CSCA",
                Title = "CSCA Practice Test",
                Description = "Configurable CSCA practice test. Choose any combination of 8 subjects (EN/CN) before starting.",
                TotalTimeMinutes = 540,
                IsActive = true,
                Sections = new List<MockExamSection>
                {
                    new MockExamSection { ExamSectionId = cscaMathS2?.Id, Name = "Mathematics (EN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 0, Instructions = "48 multiple-choice questions covering algebra, calculus, geometry, probability, and statistics. 60 minutes." },
                    new MockExamSection { ExamSectionId = cscaPhysS2?.Id, Name = "Physics (EN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 1, Instructions = "48 multiple-choice questions on mechanics, thermodynamics, electromagnetism, and optics. 60 minutes." },
                    new MockExamSection { ExamSectionId = cscaChemS2?.Id, Name = "Chemistry (EN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 2, Instructions = "48 multiple-choice questions on general, organic, and inorganic chemistry. 60 minutes." },
                    new MockExamSection { ExamSectionId = cscaMathS2?.Id, Name = "Mathematics (CN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 3, Instructions = "48 вопросов по математике: алгебра, геометрия, анализ, вероятность и статистика. 60 минут." },
                    new MockExamSection { ExamSectionId = cscaPhysS2?.Id, Name = "Physics (CN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 4, Instructions = "48 вопросов по физике: механика, термодинамика, электромагнетизм, оптика. 60 минут." },
                    new MockExamSection { ExamSectionId = cscaChemS2?.Id, Name = "Chemistry (CN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 5, Instructions = "48 вопросов по химии: общая, органическая и неорганическая химия. 60 минут." },
                    new MockExamSection { ExamSectionId = cscaCnTechS2?.Id, Name = "Chinese Technical", TimeLimitMinutes = 90, QuestionCount = 80, SortOrder = 6, Instructions = "80 вопросов: научная терминология, чтение технических текстов на китайском. 90 минут." },
                    new MockExamSection { ExamSectionId = cscaCnHumS2?.Id, Name = "Chinese Humanitarian", TimeLimitMinutes = 90, QuestionCount = 80, SortOrder = 7, Instructions = "80 вопросов: литература, история, культура, философия Китая. 90 минут." }
                }
            }
        };

        await _context.MockExams.AddRangeAsync(mockExams);
        await _context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════
    //  FIX: Migrate existing CSCA data to correct structure
    // ═══════════════════════════════════════════════════════
    private async Task FixCscaMockExamSectionsAsync()
    {
        // 1. Fix MaxScores on existing CSCA ExamSections (should all be 100 per official spec)
        var cscaSections = await _context.ExamSections.Where(s => s.ExamTypeCode == "CSCA").ToListAsync();
        foreach (var sec in cscaSections)
        {
            if (sec.MaxScore != 100) sec.MaxScore = 100;
        }

        // 2. Fix ExamType name
        var cscaExamType = await _context.ExamTypes.FirstOrDefaultAsync(e => e.Code == "CSCA");
        if (cscaExamType != null && !cscaExamType.Name.Contains("China"))
            cscaExamType.Name = "CSCA \u2014 China Scholastic Competency Assessment";

        await _context.SaveChangesAsync();

        // 3. Migrate to single configurable CSCA mock (replaces old 7-track or single-mock approach)
        var cscaMocks = await _context.MockExams
            .Include(m => m.Sections)
            .Where(m => m.ExamTypeCode == "CSCA")
            .ToListAsync();

        // Already have exactly 1 mock with 8 sections — just fix QuestionCount if needed
        if (cscaMocks.Count == 1 && cscaMocks[0].Sections.Count == 8 && cscaMocks[0].Title == "CSCA Practice Test")
        {
            await FixMockExamSectionQuestionCountsAsync();
            return;
        }

        // 4. Delete old CSCA mocks (7 track-based or any incorrect mocks)
        foreach (var oldMock in cscaMocks)
        {
            // Delete related attempts and answers first
            var attempts = await _context.MockExamAttempts.Where(a => a.MockExamId == oldMock.Id).ToListAsync();
            foreach (var attempt in attempts)
            {
                var answers = await _context.MockExamAnswers.Where(a => a.AttemptId == attempt.Id).ToListAsync();
                _context.MockExamAnswers.RemoveRange(answers);
            }
            _context.MockExamAttempts.RemoveRange(attempts);
            _context.MockExamSections.RemoveRange(oldMock.Sections);
            _context.MockExams.Remove(oldMock);
        }
        await _context.SaveChangesAsync();

        // 5. Create single configurable CSCA mock with all 8 subject options
        var mathS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Mathematics") && !s.Name.Contains("(CN)"));
        var physS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Physics") && !s.Name.Contains("(CN)"));
        var chemS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Chemistry") && !s.Name.Contains("(CN)"));
        var cnTechS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Chinese Technical"));
        var cnHumS = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Chinese Humanitarian"));

        var newMock = new MockExam
        {
            ExamTypeCode = "CSCA",
            Title = "CSCA Practice Test",
            Description = "Configurable CSCA practice test. Choose any combination of 8 subjects (EN/CN) before starting.",
            TotalTimeMinutes = 540,
            IsActive = true,
            Sections = new List<MockExamSection>
            {
                new MockExamSection { ExamSectionId = mathS?.Id, Name = "Mathematics (EN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 0, Instructions = "48 multiple-choice questions covering algebra, calculus, geometry, probability, and statistics. 60 minutes." },
                new MockExamSection { ExamSectionId = physS?.Id, Name = "Physics (EN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 1, Instructions = "48 multiple-choice questions on mechanics, thermodynamics, electromagnetism, and optics. 60 minutes." },
                new MockExamSection { ExamSectionId = chemS?.Id, Name = "Chemistry (EN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 2, Instructions = "48 multiple-choice questions on general, organic, and inorganic chemistry. 60 minutes." },
                new MockExamSection { ExamSectionId = mathS?.Id, Name = "Mathematics (CN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 3, Instructions = "48 вопросов по математике: алгебра, геометрия, анализ, вероятность и статистика. 60 минут." },
                new MockExamSection { ExamSectionId = physS?.Id, Name = "Physics (CN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 4, Instructions = "48 вопросов по физике: механика, термодинамика, электромагнетизм, оптика. 60 минут." },
                new MockExamSection { ExamSectionId = chemS?.Id, Name = "Chemistry (CN)", TimeLimitMinutes = 60, QuestionCount = 48, SortOrder = 5, Instructions = "48 вопросов по химии: общая, органическая и неорганическая химия. 60 минут." },
                new MockExamSection { ExamSectionId = cnTechS?.Id, Name = "中文技术 (Chinese Technical)", TimeLimitMinutes = 90, QuestionCount = 80, SortOrder = 6, Instructions = "80 вопросов: научная терминология, чтение технических текстов на китайском. 90 минут." },
                new MockExamSection { ExamSectionId = cnHumS?.Id, Name = "中文人文 (Chinese Humanitarian)", TimeLimitMinutes = 90, QuestionCount = 80, SortOrder = 7, Instructions = "80 вопросов: литература, история, культура, философия Китая. 90 минут." }
            }
        };

        await _context.MockExams.AddAsync(newMock);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Fills QuestionCount on existing MockExamSections that still have 0 (after migration added the column).
    /// </summary>
    private async Task FixMockExamSectionQuestionCountsAsync()
    {
        var sections = await _context.MockExamSections.Where(s => s.QuestionCount == 0).ToListAsync();
        if (sections.Count == 0) return;

        foreach (var s in sections)
        {
            s.QuestionCount = s.Name switch
            {
                // SAT
                "Reading & Writing" => 54,
                "Math (No Calculator)" => 20,
                "Math (Calculator)" => 38,
                // TOEFL
                "Reading" when s.MockExam == null => 0, // loaded below
                "Listening" when s.MockExam == null => 0,
                // NUET
                "Quantitative Reasoning" => 40,
                "Critical Thinking" => 40,
                // CSCA
                "Mathematics (EN)" or "Mathematics (CN)" => 48,
                "Physics (EN)" or "Physics (CN)" => 48,
                "Chemistry (EN)" or "Chemistry (CN)" => 48,
                "Chinese Technical" => 80,
                "Chinese Humanitarian" => 80,
                _ => 0
            };
        }

        // Resolve exam type for ambiguous names (Reading/Listening/Writing/Speaking used by both TOEFL and IELTS)
        var ambiguous = sections.Where(s => s.QuestionCount == 0 && s.Name is "Reading" or "Listening" or "Writing" or "Speaking").ToList();
        if (ambiguous.Count > 0)
        {
            var mockExamTypes = await _context.MockExams
                .Where(m => ambiguous.Select(a => a.MockExamId).Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m.ExamTypeCode);

            foreach (var s in ambiguous)
            {
                var examType = mockExamTypes.GetValueOrDefault(s.MockExamId, "");
                s.QuestionCount = (examType, s.Name) switch
                {
                    ("TOEFL", "Reading") => 20,
                    ("TOEFL", "Listening") => 28,
                    ("TOEFL", "Speaking") => 4,
                    ("TOEFL", "Writing") => 2,
                    ("IELTS", "Listening") => 40,
                    ("IELTS", "Reading") => 40,
                    ("IELTS", "Writing") => 2,
                    ("IELTS", "Speaking") => 3,
                    _ => 0
                };
            }
        }

        await _context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════
    //  T-5  TUTOR SEED DATA
    // ═══════════════════════════════════════════════════════
    private async Task SeedTutorDataAsync()
    {
        // --- 1. Create tutor users ---
        var tutors = new[]
        {
            new User
            {
                Email = "tutor.elena@unistart.kz",
                Name = "Елена Смирнова",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("tutor123"),
                Role = UserRole.Tutor,
                HasCompletedOnboarding = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            },
            new User
            {
                Email = "tutor.dmitry@unistart.kz",
                Name = "Дмитрий Козлов",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("tutor123"),
                Role = UserRole.Tutor,
                HasCompletedOnboarding = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            new User
            {
                Email = "tutor.anna@unistart.kz",
                Name = "Анна Волкова",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("tutor123"),
                Role = UserRole.Tutor,
                HasCompletedOnboarding = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            }
        };

        await _context.Users.AddRangeAsync(tutors);
        await _context.SaveChangesAsync();

        // --- 2. Create tutor profiles ---
        var profiles = new[]
        {
            new TutorProfile
            {
                UserId = tutors[0].Id,
                Headline = "Expert SAT & NUET Math Tutor",
                Bio = "Преподаю математику и физику более 10 лет. Мои ученики стабильно набирают высокие баллы. Индивидуальный подход к каждому студенту.",
                Experience = "Кандидат физико-математических наук. 12 лет преподавания. Автор учебных пособий по подготовке к SAT и NUET.",
                Specializations = "SAT,NUET",
                HourlyRate = 2500m,
                IsAvailable = true,
                IsVerified = true,
                ContactPreference = ContactPreference.Both,
                AverageRating = 4.8m,
                TotalReviews = 3,
                TotalStudents = 2,
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                UpdatedAt = DateTime.UtcNow
            },
            new TutorProfile
            {
                UserId = tutors[1].Id,
                Headline = "IELTS/TOEFL Preparation Specialist",
                Bio = "Certified IELTS instructor with 8+ years of experience. Band 9.0 holder. I help students achieve their target scores efficiently through proven strategies.",
                Experience = "MA in Applied Linguistics. Cambridge CELTA certified. Former British Council examiner. 500+ students prepared.",
                Specializations = "IELTS,TOEFL,SAT",
                HourlyRate = 3000m,
                IsAvailable = true,
                IsVerified = true,
                ContactPreference = ContactPreference.Chat,
                AverageRating = 4.6m,
                TotalReviews = 2,
                TotalStudents = 1,
                CreatedAt = DateTime.UtcNow.AddMonths(-4),
                UpdatedAt = DateTime.UtcNow
            },
            new TutorProfile
            {
                UserId = tutors[2].Id,
                Headline = "Подготовка к SAT и международным экзаменам",
                Bio = "Помогаю студентам подготовиться к SAT, ACT и другим международным тестам. Фокус на академическом английском и критическом мышлении.",
                Experience = "Выпускница Columbia University. 5 лет опыта подготовки к SAT. Средний прирост студентов — 200 баллов.",
                Specializations = "SAT,IELTS",
                HourlyRate = 2000m,
                IsAvailable = false,
                IsVerified = false,
                ContactPreference = ContactPreference.Email,
                AverageRating = 4.9m,
                TotalReviews = 2,
                TotalStudents = 1,
                CreatedAt = DateTime.UtcNow.AddMonths(-2),
                UpdatedAt = DateTime.UtcNow
            }
        };

        await _context.TutorProfiles.AddRangeAsync(profiles);
        await _context.SaveChangesAsync();

        // --- 3. Schedule slots ---
        var scheduleSlots = new List<TutorScheduleSlot>();

        // Elena — weekdays 10:00-18:00
        foreach (var dow in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday })
        {
            scheduleSlots.Add(new TutorScheduleSlot
            {
                TutorProfileId = profiles[0].Id,
                DayOfWeek = dow,
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(18, 0)
            });
        }

        // Dmitry — Mon, Wed, Fri 14:00-20:00
        foreach (var dow in new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday })
        {
            scheduleSlots.Add(new TutorScheduleSlot
            {
                TutorProfileId = profiles[1].Id,
                DayOfWeek = dow,
                StartTime = new TimeOnly(14, 0),
                EndTime = new TimeOnly(20, 0)
            });
        }

        // Anna — Sat, Sun 09:00-15:00
        foreach (var dow in new[] { DayOfWeek.Saturday, DayOfWeek.Sunday })
        {
            scheduleSlots.Add(new TutorScheduleSlot
            {
                TutorProfileId = profiles[2].Id,
                DayOfWeek = dow,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(15, 0)
            });
        }

        await _context.TutorScheduleSlots.AddRangeAsync(scheduleSlots);
        await _context.SaveChangesAsync();

        // --- 4. Reviews (from test user) ---
        var testUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@unistart.kz");
        if (testUser != null)
        {
            var reviews = new[]
            {
                new TutorReview
                {
                    TutorProfileId = profiles[0].Id,
                    StudentId = testUser.Id,
                    Rating = 5,
                    Comment = "Отличный преподаватель! За три месяца подняла балл с 60 до 92. Очень рекомендую!",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new TutorReview
                {
                    TutorProfileId = profiles[1].Id,
                    StudentId = testUser.Id,
                    Rating = 5,
                    Comment = "Fantastic teacher! Helped me get IELTS 7.5 in just 2 months of preparation.",
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new TutorReview
                {
                    TutorProfileId = profiles[2].Id,
                    StudentId = testUser.Id,
                    Rating = 5,
                    Comment = "Анна помогла структурировать подготовку к SAT. Очень системный подход.",
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                }
            };

            await _context.TutorReviews.AddRangeAsync(reviews);
            await _context.SaveChangesAsync();

            // --- 5. Test conversation + messages ---
            var conversation = new Conversation
            {
                StudentId = testUser.Id,
                TutorId = tutors[0].Id,
                LastMessagePreview = "Спасибо, до встречи!",
                LastMessageAt = DateTime.UtcNow.AddHours(-2),
                UnreadCountStudent = 1,
                UnreadCountTutor = 0,
                Status = ConversationStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            };

            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            var messages = new[]
            {
                new Message
                {
                    ConversationId = conversation.Id,
                    SenderId = testUser.Id,
                    Text = "Здравствуйте, Елена! Хочу подготовиться к ЕГЭ по математике. Можете помочь?",
                    SentAt = DateTime.UtcNow.AddDays(-7),
                    Type = MessageType.Text
                },
                new Message
                {
                    ConversationId = conversation.Id,
                    SenderId = tutors[0].Id,
                    Text = "Здравствуйте! Конечно, с удовольствием помогу. Какой у вас текущий уровень? Сколько времени до экзамена?",
                    SentAt = DateTime.UtcNow.AddDays(-7).AddMinutes(15),
                    Type = MessageType.Text
                },
                new Message
                {
                    ConversationId = conversation.Id,
                    SenderId = testUser.Id,
                    Text = "Пробник написала на 65 баллов. До экзамена 4 месяца.",
                    SentAt = DateTime.UtcNow.AddDays(-7).AddMinutes(30),
                    Type = MessageType.Text
                },
                new Message
                {
                    ConversationId = conversation.Id,
                    SenderId = tutors[0].Id,
                    Text = "Отлично, за 4 месяца есть все шансы выйти на 85+. Давайте начнём с диагностики — пройдите тест на платформе, и мы обсудим план.",
                    SentAt = DateTime.UtcNow.AddDays(-6).AddHours(10),
                    Type = MessageType.Text
                },
                new Message
                {
                    ConversationId = conversation.Id,
                    SenderId = tutors[0].Id,
                    Text = "Спасибо, до встречи!",
                    SentAt = DateTime.UtcNow.AddHours(-2),
                    Type = MessageType.Text
                }
            };

            await _context.Messages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();
        }
    }

    // ═══════════════════════════════════════════════════════
    //  LEARNING V2 SEED DATA
    // ═══════════════════════════════════════════════════════

    private async Task SeedFormulaCardsAsync()
    {
        var linearEq = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "Linear Equations");
        var geometry = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "Geometry");
        var quadratic = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "Quadratic Equations");
        var dataAnalysis = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "Data Analysis");
        var algebra = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "Algebra & Functions");
        var problemSolving = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "Problem Solving");

        var cards = new List<FormulaCard>();
        int sort = 1;

        if (linearEq != null)
        {
            cards.Add(new FormulaCard { TopicId = linearEq.Id, Title = "Slope-Intercept Form", Formula = "y = mx + b", Description = "m = slope, b = y-intercept", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = linearEq.Id, Title = "Point-Slope Form", Formula = "y - y1 = m(x - x1)", Description = "Useful when you know a point and the slope", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = linearEq.Id, Title = "Slope Formula", Formula = "m = (y2 - y1) / (x2 - x1)", Description = "Calculate slope from two points", SortOrder = sort++ });
        }

        if (geometry != null)
        {
            cards.Add(new FormulaCard { TopicId = geometry.Id, Title = "Area of Circle", Formula = "A = pi * r^2", Description = "r = radius", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = geometry.Id, Title = "Circumference", Formula = "C = 2 * pi * r", Description = "r = radius", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = geometry.Id, Title = "Pythagorean Theorem", Formula = "a^2 + b^2 = c^2", Description = "c = hypotenuse of right triangle", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = geometry.Id, Title = "Area of Triangle", Formula = "A = (1/2) * b * h", Description = "b = base, h = height", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = geometry.Id, Title = "Volume of Cylinder", Formula = "V = pi * r^2 * h", Description = "r = radius, h = height", SortOrder = sort++ });
        }

        if (quadratic != null)
        {
            cards.Add(new FormulaCard { TopicId = quadratic.Id, Title = "Quadratic Formula", Formula = "x = (-b +/- sqrt(b^2 - 4ac)) / 2a", Description = "Solves ax^2 + bx + c = 0", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = quadratic.Id, Title = "Discriminant", Formula = "D = b^2 - 4ac", Description = "D > 0: two real roots, D = 0: one root, D < 0: no real roots", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = quadratic.Id, Title = "Vertex Form", Formula = "y = a(x - h)^2 + k", Description = "Vertex at (h, k)", SortOrder = sort++ });
        }

        if (dataAnalysis != null)
        {
            cards.Add(new FormulaCard { TopicId = dataAnalysis.Id, Title = "Mean (Average)", Formula = "mean = sum(x_i) / n", Description = "Sum of values divided by count", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = dataAnalysis.Id, Title = "Probability", Formula = "P(A) = favorable / total", Description = "Basic probability formula", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = dataAnalysis.Id, Title = "Percent Change", Formula = "change = ((new - old) / old) * 100%", Description = "Positive = increase, negative = decrease", SortOrder = sort++ });
        }

        if (algebra != null)
        {
            cards.Add(new FormulaCard { TopicId = algebra.Id, Title = "Difference of Squares", Formula = "a^2 - b^2 = (a + b)(a - b)", Description = "Factoring pattern", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = algebra.Id, Title = "Exponent Rules", Formula = "a^m * a^n = a^(m+n); (a^m)^n = a^(mn)", Description = "Basic exponent operations", SortOrder = sort++ });
        }

        if (problemSolving != null)
        {
            cards.Add(new FormulaCard { TopicId = problemSolving.Id, Title = "Distance Formula", Formula = "d = sqrt((x2-x1)^2 + (y2-y1)^2)", Description = "Distance between two points", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = problemSolving.Id, Title = "Simple Interest", Formula = "I = P * r * t", Description = "P = principal, r = rate, t = time", SortOrder = sort++ });
        }

        // IELTS Writing formula-style templates
        var ieltsTask1Topic = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "IELTS Task 1: Data Description");
        var ieltsTask2Topic = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "IELTS Task 2: Essay Writing");

        if (ieltsTask1Topic != null)
        {
            cards.Add(new FormulaCard { TopicId = ieltsTask1Topic.Id, Title = "Task 1 Introduction", Formula = "The [chart type] illustrates [what] in [where/when].", Description = "Paraphrase the question — never copy it", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = ieltsTask1Topic.Id, Title = "Task 1 Overview", Formula = "Overall, [main trend 1], while [main trend 2].", Description = "Summary of 2 key features — no specific numbers", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = ieltsTask1Topic.Id, Title = "Comparison Phrase", Formula = "X was [approximately/nearly] [N] times higher than Y.", Description = "Use for comparing data points precisely", SortOrder = sort++ });
        }

        if (ieltsTask2Topic != null)
        {
            cards.Add(new FormulaCard { TopicId = ieltsTask2Topic.Id, Title = "Thesis Template", Formula = "While [view A], I believe [your position] because [reason].", Description = "For agree/disagree and discussion essays", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = ieltsTask2Topic.Id, Title = "Body Paragraph", Formula = "Topic sentence → Explanation → Example → Link back", Description = "Standard PEEL structure for each body paragraph", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = ieltsTask2Topic.Id, Title = "Band 7 Sentence", Formula = "Although [concession], [main point], which [result].", Description = "Complex sentence with concession clause", SortOrder = sort++ });
        }

        // CSCA Formula Cards
        var calculusTopic = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "Calculus & Analysis");
        var probStatsTopic = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "Probability & Statistics");
        var discreteMathTopic = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "Discrete Mathematics");
        var formalLogicTopic = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "Formal Logic");

        if (calculusTopic != null)
        {
            cards.Add(new FormulaCard { TopicId = calculusTopic.Id, Title = "Power Rule (Derivative)", Formula = "d/dx(x^n) = n * x^(n-1)", Description = "Fundamental differentiation rule", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = calculusTopic.Id, Title = "Power Rule (Integral)", Formula = "∫ x^n dx = x^(n+1)/(n+1) + C", Description = "n ≠ -1", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = calculusTopic.Id, Title = "Chain Rule", Formula = "d/dx[f(g(x))] = f'(g(x)) * g'(x)", Description = "For differentiating composite functions", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = calculusTopic.Id, Title = "Product Rule", Formula = "(fg)' = f'g + fg'", Description = "For differentiating products of functions", SortOrder = sort++ });
        }

        if (probStatsTopic != null)
        {
            cards.Add(new FormulaCard { TopicId = probStatsTopic.Id, Title = "Bayes' Theorem", Formula = "P(A|B) = P(B|A)*P(A) / P(B)", Description = "Updating probability with new evidence", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = probStatsTopic.Id, Title = "Variance", Formula = "σ² = Σ(xi - μ)² / n", Description = "Average of squared deviations from mean", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = probStatsTopic.Id, Title = "Standard Deviation", Formula = "σ = sqrt(σ²)", Description = "Square root of variance — same units as data", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = probStatsTopic.Id, Title = "Conditional Probability", Formula = "P(A|B) = P(A∩B) / P(B)", Description = "Probability of A given B has occurred", SortOrder = sort++ });
        }

        if (discreteMathTopic != null)
        {
            cards.Add(new FormulaCard { TopicId = discreteMathTopic.Id, Title = "Permutations", Formula = "P(n,r) = n! / (n-r)!", Description = "Ordered arrangements of r items from n", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = discreteMathTopic.Id, Title = "Combinations", Formula = "C(n,r) = n! / (r!(n-r)!)", Description = "Unordered selections of r items from n", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = discreteMathTopic.Id, Title = "Handshaking Lemma", Formula = "Σ deg(v) = 2|E|", Description = "Sum of vertex degrees = 2 × edges", SortOrder = sort++ });
        }

        if (formalLogicTopic != null)
        {
            cards.Add(new FormulaCard { TopicId = formalLogicTopic.Id, Title = "Modus Ponens", Formula = "P → Q, P ⊢ Q", Description = "If P implies Q and P is true, then Q is true", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = formalLogicTopic.Id, Title = "Modus Tollens", Formula = "P → Q, ¬Q ⊢ ¬P", Description = "If P implies Q and Q is false, then P is false", SortOrder = sort++ });
            cards.Add(new FormulaCard { TopicId = formalLogicTopic.Id, Title = "De Morgan's Laws", Formula = "¬(P∧Q) ≡ ¬P∨¬Q ; ¬(P∨Q) ≡ ¬P∧¬Q", Description = "Negation of conjunctions and disjunctions", SortOrder = sort++ });
        }

        if (cards.Count > 0)
        {
            await _context.FormulaCards.AddRangeAsync(cards);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedFlashcardDecksAsync()
    {
        var decks = new List<FlashcardDeck>
        {
            new FlashcardDeck
            {
                Title = "SAT Math Key Terms",
                Description = "Essential math vocabulary for SAT",
                ExamTypeCode = "SAT",
                IsSystem = true,
                Cards = new List<Flashcard>
                {
                    new Flashcard { Front = "What is a linear function?", Back = "A function whose graph is a straight line. General form: f(x) = mx + b", SortOrder = 1 },
                    new Flashcard { Front = "Define 'slope'", Back = "The rate of change of a line. Calculated as rise/run or (y2-y1)/(x2-x1)", SortOrder = 2 },
                    new Flashcard { Front = "What is a quadratic equation?", Back = "An equation of the form ax^2 + bx + c = 0, where a is not 0. Its graph is a parabola.", SortOrder = 3 },
                    new Flashcard { Front = "What is the vertex of a parabola?", Back = "The highest or lowest point of a parabola. For y = ax^2 + bx + c, the x-coordinate is -b/(2a).", SortOrder = 4 },
                    new Flashcard { Front = "Define 'median'", Back = "The middle value when data is arranged in order. For even count, average the two middle values.", SortOrder = 5 },
                    new Flashcard { Front = "What is standard deviation?", Back = "A measure of how spread out values are from the mean. Higher SD = more spread.", SortOrder = 6 },
                    new Flashcard { Front = "Define 'perpendicular lines'", Back = "Lines that intersect at a 90-degree angle. Their slopes are negative reciprocals (m1 * m2 = -1).", SortOrder = 7 },
                    new Flashcard { Front = "What is a system of equations?", Back = "Two or more equations with the same variables. Solutions satisfy all equations simultaneously.", SortOrder = 8 },
                },
            },
            new FlashcardDeck
            {
                Title = "TOEFL Academic Vocabulary",
                Description = "High-frequency academic words for TOEFL",
                ExamTypeCode = "TOEFL",
                IsSystem = true,
                Cards = new List<Flashcard>
                {
                    new Flashcard { Front = "Analyze", Back = "To examine something in detail in order to understand it or draw conclusions.", SortOrder = 1 },
                    new Flashcard { Front = "Hypothesis", Back = "A proposed explanation made as a starting point for further investigation.", SortOrder = 2 },
                    new Flashcard { Front = "Empirical", Back = "Based on observation or experience rather than theory or pure logic.", SortOrder = 3 },
                    new Flashcard { Front = "Paradigm", Back = "A typical example or pattern of something; a model or framework.", SortOrder = 4 },
                    new Flashcard { Front = "Synthesize", Back = "To combine elements to form a coherent whole; to integrate information.", SortOrder = 5 },
                    new Flashcard { Front = "Ubiquitous", Back = "Present, appearing, or found everywhere.", SortOrder = 6 },
                    new Flashcard { Front = "Mitigate", Back = "To make less severe, serious, or painful.", SortOrder = 7 },
                    new Flashcard { Front = "Innovative", Back = "Introducing new ideas; original and creative in thinking.", SortOrder = 8 },
                },
            },
            new FlashcardDeck
            {
                Title = "NUET Critical Thinking",
                Description = "Key concepts for NUET Critical Thinking section",
                ExamTypeCode = "NUET",
                IsSystem = true,
                Cards = new List<Flashcard>
                {
                    new Flashcard { Front = "What is a logical fallacy?", Back = "An error in reasoning that renders an argument invalid. Common types: ad hominem, straw man, false dilemma.", SortOrder = 1 },
                    new Flashcard { Front = "Define 'deductive reasoning'", Back = "Reasoning from general premises to a specific conclusion. If premises are true, the conclusion must be true.", SortOrder = 2 },
                    new Flashcard { Front = "Define 'inductive reasoning'", Back = "Reasoning from specific observations to a general conclusion. The conclusion is probable but not certain.", SortOrder = 3 },
                    new Flashcard { Front = "What is a 'straw man' fallacy?", Back = "Misrepresenting someone's argument to make it easier to attack.", SortOrder = 4 },
                    new Flashcard { Front = "What is 'correlation vs causation'?", Back = "Correlation means two things occur together; causation means one causes the other. Correlation does not imply causation.", SortOrder = 5 },
                    new Flashcard { Front = "What is a 'premise'?", Back = "A statement or proposition used as a basis for an argument or conclusion.", SortOrder = 6 },
                },
            },
            new FlashcardDeck
            {
                Title = "IELTS Academic Vocabulary",
                Description = "Essential academic vocabulary and phrases for IELTS",
                ExamTypeCode = "IELTS",
                IsSystem = true,
                Cards = new List<Flashcard>
                {
                    new Flashcard { Front = "What does 'fluctuate' mean in a graph?", Back = "To rise and fall irregularly — used in Task 1 to describe unstable data trends.", SortOrder = 1 },
                    new Flashcard { Front = "Distinguish TRUE vs NOT GIVEN", Back = "TRUE: the passage explicitly states this. NOT GIVEN: the passage doesn't mention this specific claim at all.", SortOrder = 2 },
                    new Flashcard { Front = "What is 'coherence and cohesion'?", Back = "IELTS Writing criterion: how well your ideas are organised and linked using paragraphs, connectors, and referencing.", SortOrder = 3 },
                    new Flashcard { Front = "Task 1: 'Overview' purpose", Back = "Summarise the main trends/features without specific data. It's the most important paragraph for Band 7+.", SortOrder = 4 },
                    new Flashcard { Front = "Speaking Part 3 vs Part 1", Back = "Part 1: personal, concrete questions. Part 3: abstract, analytical discussion requiring deeper reasoning.", SortOrder = 5 },
                    new Flashcard { Front = "What is 'lexical resource'?", Back = "IELTS criterion measuring range and accuracy of vocabulary. Use synonyms, collocations, and topic-specific terms.", SortOrder = 6 },
                    new Flashcard { Front = "Task 2: Opinion vs Discussion essay", Back = "Opinion: state your view and defend it. Discussion: present BOTH views, then give your opinion.", SortOrder = 7 },
                    new Flashcard { Front = "Define 'paraphrase'", Back = "To express the same meaning using different words. Essential for IELTS introductions (never copy the question).", SortOrder = 8 },
                },
            },
            new FlashcardDeck
            {
                Title = "CSCA Core Concepts",
                Description = "Key formulas and concepts for CSCA Math Analysis and Logical Reasoning",
                ExamTypeCode = "CSCA",
                IsSystem = true,
                Cards = new List<Flashcard>
                {
                    new Flashcard { Front = "What is the derivative of x^n?", Back = "n·x^(n-1) — the power rule of differentiation.", SortOrder = 1 },
                    new Flashcard { Front = "What is Big-O notation?", Back = "Describes the upper bound of an algorithm's growth rate. O(n) = linear, O(n²) = quadratic, O(log n) = logarithmic.", SortOrder = 2 },
                    new Flashcard { Front = "State De Morgan's Laws", Back = "¬(P ∧ Q) ≡ ¬P ∨ ¬Q and ¬(P ∨ Q) ≡ ¬P ∧ ¬Q", SortOrder = 3 },
                    new Flashcard { Front = "What is C(n,r)?", Back = "The number of ways to choose r items from n: n!/(r!(n-r)!). Also written as 'n choose r'.", SortOrder = 4 },
                    new Flashcard { Front = "What does the integral ∫f(x)dx represent?", Back = "The area under the curve f(x). The antiderivative of f plus a constant C.", SortOrder = 5 },
                    new Flashcard { Front = "What is modus ponens?", Back = "If P → Q and P is true, then Q must be true. A fundamental rule of deductive logic.", SortOrder = 6 },
                    new Flashcard { Front = "What is the 68-95-99.7 rule?", Back = "In a normal distribution: 68% within ±1σ, 95% within ±2σ, 99.7% within ±3σ of the mean.", SortOrder = 7 },
                    new Flashcard { Front = "What is the handshaking lemma?", Back = "The sum of all vertex degrees in a graph equals twice the number of edges: Σdeg(v) = 2|E|.", SortOrder = 8 },
                },
            },
        };

        await _context.FlashcardDecks.AddRangeAsync(decks);
        await _context.SaveChangesAsync();
    }

    private async Task SeedStrategyGuidesAsync()
    {
        var guides = new List<StrategyGuide>
        {
            // SAT Strategies
            new StrategyGuide
            {
                ExamTypeCode = "SAT",
                Title = "SAT Time Management",
                Summary = "How to pace yourself across SAT sections",
                Category = "time-management",
                EstimatedReadMinutes = 5,
                SortOrder = 1,
                Content = @"Time Management for the SAT

The SAT has strict time limits per section. Effective pacing is critical.

Reading & Writing (64 minutes, 54 questions):
- Aim for ~1 min 10 sec per question
- Do not spend more than 2 minutes on any single question
- If stuck, mark and move on -- return if time permits

Math No Calculator (25 minutes, 20 questions):
- About 75 seconds per question
- Solve easy questions first to bank time for harder ones

Math Calculator (55 minutes, 38 questions):
- About 87 seconds per question
- Use your calculator strategically, not for every problem

General Tips:
- Wear a watch (no smart watches)
- Practice with timed sections regularly
- In the last 2 minutes, answer all remaining questions (no penalty for guessing)"
            },
            new StrategyGuide
            {
                ExamTypeCode = "SAT",
                Title = "Process of Elimination",
                Summary = "Use elimination to increase accuracy on SAT",
                Category = "test-taking",
                EstimatedReadMinutes = 4,
                SortOrder = 2,
                Content = @"Process of Elimination (POE)

Since the SAT has no guessing penalty, always eliminate wrong answers before choosing.

Step 1: Read the question carefully
Step 2: Identify obviously wrong answers (usually 1-2 are clearly incorrect)
Step 3: Compare remaining options against the passage/problem
Step 4: Choose the best supported answer

For Reading:
- Wrong answers often use extreme language ('always', 'never', 'all')
- Correct answers are usually moderate and well-supported
- Watch for 'half-right' answers that are partially correct but miss something

For Math:
- Plug answer choices back into the equation
- Start with choice B or C for numerical answers (often in order)
- Estimate before calculating to eliminate unreasonable options"
            },
            new StrategyGuide
            {
                ExamTypeCode = "SAT",
                Title = "SAT Math Strategy: Plugging In",
                Summary = "When algebra gets complex, use numbers instead",
                Category = "section-specific",
                EstimatedReadMinutes = 4,
                SortOrder = 3,
                Content = @"Plugging In Numbers

When a problem uses variables and the answer choices also use variables, try plugging in specific numbers.

How to Plug In:
1. Choose simple numbers (2, 3, 5 -- avoid 0 and 1)
2. Substitute into the problem
3. Calculate the result
4. Check which answer choice gives the same result

Example: If x > 0, which equals (x^2 + 2x) / x?
- Plug in x = 3: (9 + 6)/3 = 5
- Check answers: x + 2 = 5 (correct!)

When to Use:
- 'If x is a positive integer...' type problems
- Percent problems -- plug in 100
- Ratio problems -- use the actual ratio numbers
- Any problem where the answer has variables"
            },

            // TOEFL Strategies
            new StrategyGuide
            {
                ExamTypeCode = "TOEFL",
                Title = "TOEFL Reading Strategies",
                Summary = "Approaches for the TOEFL reading section",
                Category = "section-specific",
                EstimatedReadMinutes = 5,
                SortOrder = 1,
                Content = @"TOEFL Reading Section Strategy

You have 54-72 minutes for 3-4 passages (about 700 words each).

Before Reading:
- Skim the first paragraph to understand the topic
- Read the first sentence of each paragraph for structure

Question Types and Approaches:
1. Factual Information: Find specific details in the passage
2. Vocabulary: Use context clues from surrounding sentences
3. Inference: The answer must be logically supported, not just possible
4. Insert Text: Check pronoun references and transition logic
5. Summary: Choose 3 main ideas (avoid minor details)

Time Strategy:
- Spend 18 minutes per passage
- Read the passage in 4-5 minutes
- Answer questions in order (they follow passage order)
- Save the summary question for last"
            },
            new StrategyGuide
            {
                ExamTypeCode = "TOEFL",
                Title = "TOEFL Note-Taking",
                Summary = "Effective note-taking for listening and speaking",
                Category = "test-taking",
                EstimatedReadMinutes = 4,
                SortOrder = 2,
                Content = @"Note-Taking for TOEFL

Good notes are the foundation of listening, speaking, and writing sections.

Listening Notes:
- Write main topic in a circle at the top
- Use abbreviations (bc = because, w/ = with, -> = leads to)
- Capture key terms and relationships, not every word
- Note speaker tone changes (disagreement, surprise)
- Mark examples with 'ex:' -- they often appear in questions

Speaking Notes (30 seconds prep):
- Write 3-4 bullet points maximum
- Include: main point, supporting detail 1, supporting detail 2
- Note transition words to use (however, in addition, for example)

Writing Notes:
- Create a quick outline: thesis, body 1, body 2, conclusion
- Jot down 2 examples per body paragraph
- Plan your word count (~300-350 for independent, ~150-225 for integrated)"
            },

            // NUET Strategies
            new StrategyGuide
            {
                ExamTypeCode = "NUET",
                Title = "NUET Critical Thinking Approach",
                Summary = "Systematic approach to NUET critical thinking questions",
                Category = "section-specific",
                EstimatedReadMinutes = 5,
                SortOrder = 1,
                Content = @"NUET Critical Thinking Strategy

The NUET Critical Thinking section tests logical reasoning and argument analysis.

For Argument Analysis:
1. Identify the conclusion (what the argument is trying to prove)
2. Find the premises (facts/reasons given to support the conclusion)
3. Look for assumptions (unstated beliefs connecting premises to conclusion)
4. Evaluate whether the reasoning is valid

For Logical Reasoning:
1. Read the stimulus carefully
2. Identify the question type (strengthen, weaken, assumption, inference)
3. Predict the answer before looking at options
4. Eliminate clearly wrong answers first

Common Patterns:
- Strengthen: Find an answer that provides additional support
- Weaken: Find an answer that undermines a key assumption
- Assumption: Find what MUST be true for the argument to work
- Inference: Find what logically follows from the given information

Avoid: Answers that go too far beyond the given information"
            },
            new StrategyGuide
            {
                ExamTypeCode = "NUET",
                Title = "NUET Math Problem-Solving",
                Summary = "Techniques for the NUET mathematics section",
                Category = "section-specific",
                EstimatedReadMinutes = 4,
                SortOrder = 2,
                Content = @"NUET Math Problem-Solving Techniques

Read-Plan-Solve-Check Approach:
1. READ: Understand what is being asked. Identify known and unknown values.
2. PLAN: Choose strategy (algebra, estimation, working backwards, drawing diagrams).
3. SOLVE: Execute the plan step by step.
4. CHECK: Verify the answer makes sense in context.

Key Strategies:
- Estimation: Round numbers to check if your answer is reasonable
- Working Backwards: Start from the answer choices
- Drawing Diagrams: Especially for geometry and word problems
- Pattern Recognition: Look for sequences, ratios, and common forms

Number Properties to Remember:
- Even + Even = Even, Odd + Odd = Even
- Anything times an even number is even
- If a number ends in 0 or 5, it is divisible by 5
- Divisibility by 3: sum of digits divisible by 3"
            },

            // IELTS Strategies
            new StrategyGuide
            {
                ExamTypeCode = "IELTS",
                Title = "IELTS Time Management",
                Summary = "Practical timing strategy for all four IELTS sections",
                Category = "time-management",
                EstimatedReadMinutes = 5,
                SortOrder = 1,
                Content = @"IELTS Time Management

Listening (30 min + 10 min transfer):
- Sections come in order 1-4 — each plays once
- Use the time before each section to read ahead
- Transfer your answers carefully during the 10-min window

Reading (60 min, 40 questions):
- Spend 15 min on Passage 1, 20 min on Passage 2, 25 min on Passage 3
- Passage 3 is hardest — save the most time for it
- If stuck on a question, move on and come back

Writing (60 min total):
- Task 1: 20 minutes (150+ words)
- Task 2: 40 minutes (250+ words) — worth twice the marks
- Plan for 5 minutes before writing each task

Speaking (11–14 min):
- Part 1: 4-5 min (general questions)
- Part 2: 3-4 min (1 min prep + 1-2 min talk)
- Part 3: 4-5 min (discussion)
- You cannot control the timing — focus on quality"
            },
            new StrategyGuide
            {
                ExamTypeCode = "IELTS",
                Title = "IELTS Writing Band 7 Strategy",
                Summary = "Key techniques to achieve Band 7+ in IELTS Writing",
                Category = "section-specific",
                EstimatedReadMinutes = 5,
                SortOrder = 2,
                Content = @"Achieving Band 7+ in IELTS Writing

Task 1 (Data Description):
1. Paraphrase the question in your introduction (never copy)
2. Write an overview identifying 2 main trends
3. Use specific data to support your observations
4. Vary your vocabulary: 'increased', 'rose', 'climbed', 'surged'
5. Use comparison structures: 'twice as many', 'a significantly higher proportion'

Task 2 (Essay):
1. Address ALL parts of the question
2. Present a clear position throughout
3. Use a mix of simple and complex sentences
4. Include specific examples, not generalisations
5. Link paragraphs with cohesive devices: 'Furthermore', 'However', 'In contrast'

Common errors to avoid:
- Copying the question word-for-word
- Writing less than the minimum word count
- Not including an overview in Task 1
- Using informal language ('gonna', 'lots of', 'stuff')
- Spending too long on Task 1 (it's worth less)"
            },
            new StrategyGuide
            {
                ExamTypeCode = "IELTS",
                Title = "IELTS Speaking Confidence",
                Summary = "How to speak confidently and score higher",
                Category = "test-taking",
                EstimatedReadMinutes = 4,
                SortOrder = 3,
                Content = @"IELTS Speaking: Building Confidence

Assessment Criteria (each 25%):
1. Fluency and Coherence
2. Lexical Resource (vocabulary)
3. Grammatical Range and Accuracy
4. Pronunciation

Dos:
- Extend your answers naturally with reasons and examples
- Use discourse markers: 'Well', 'Actually', 'To be honest'
- Self-correct if you make a mistake — examiners view this positively
- Vary your intonation — show enthusiasm
- Use idiomatic language where it feels natural

Don'ts:
- Give one-word answers
- Memorise entire answers (examiners can tell)
- Speak too fast — clarity beats speed
- Panic if you don't understand — ask the examiner to repeat

Part 2 Preparation Strategy:
- Note down 4 key words (one per bullet point)
- Start with 'I'd like to talk about...'
- Use the past tense for experiences, future for plans
- Aim for 1.5-2 minutes — don't stop too early"
            },

            // CSCA Strategies
            new StrategyGuide
            {
                ExamTypeCode = "CSCA",
                Title = "CSCA Math Analysis Strategy",
                Summary = "Approaches for calculus, probability, and discrete math",
                Category = "section-specific",
                EstimatedReadMinutes = 5,
                SortOrder = 1,
                Content = @"CSCA Math Analysis Strategy

Calculus Questions:
- Always check: is this asking for a derivative or integral?
- For derivatives: apply power rule, chain rule, or product rule
- For integrals: find the antiderivative, then evaluate bounds
- Check your answer by differentiating it

Probability & Statistics:
- Draw a tree diagram or Venn diagram if helpful
- For conditional probability: use P(A|B) = P(A∩B)/P(B)
- Remember: independent events → P(A∩B) = P(A)×P(B)
- For normal distribution: use the 68-95-99.7 rule

Discrete Mathematics:
- Permutations = order matters; Combinations = order doesn't
- For graph theory: start with the handshaking lemma
- Count systematically — don't try to enumerate manually
- Double-check factorials: 5! = 120, 6! = 720, 7! = 5040"
            },
            new StrategyGuide
            {
                ExamTypeCode = "CSCA",
                Title = "CSCA Logical Reasoning Strategy",
                Summary = "Systematic approach to logic and algorithm questions",
                Category = "section-specific",
                EstimatedReadMinutes = 4,
                SortOrder = 2,
                Content = @"CSCA Logical Reasoning Strategy

Formal Logic:
- Identify the logical structure first (P → Q, ¬P ∨ Q, etc.)
- Use truth tables for complex expressions
- Remember key equivalences: contrapositive, De Morgan's laws
- For arguments: find the conclusion, then check if premises support it

Algorithmic Thinking:
- For time complexity: count the number of operations as a function of n
- Nested loops → multiply complexities (O(n) × O(n) = O(n²))
- Binary search halves the problem → O(log n)
- Compare algorithms by their worst-case or average-case complexity

Data Interpretation:
- Read all axis labels and legends before analysing
- Calculate percentage changes: (new - old)/old × 100%
- Look for trends, outliers, and anomalies
- Be careful with different scales on dual-axis charts

General Tips:
- Eliminate obviously wrong answers first
- Show your working — even for multiple choice, write out steps
- Manage time: don't spend over 3 minutes on any single question"
            },
        };

        await _context.StrategyGuides.AddRangeAsync(guides);
        await _context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════
    //  INCREMENTAL IELTS/CSCA SEED METHODS
    //  (for existing DBs that already have SAT/TOEFL/NUET data)
    // ═══════════════════════════════════════════════════════

    private async Task SeedIeltsCscaTopicsAsync()
    {
        var ieltsListening = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "IELTS" && s.Name == "Listening");
        var ieltsReading = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "IELTS" && s.Name == "Reading");
        var ieltsWriting = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "IELTS" && s.Name == "Writing");
        var ieltsSpeaking = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "IELTS" && s.Name == "Speaking");
        var cscaMath = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name == "Math Analysis");
        var cscaLogic = await _context.ExamSections.FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name == "Logical Reasoning");

        if (ieltsListening == null || cscaMath == null) return;

        var skillRead = await _context.Skills.FirstAsync(s => s.Code == "SK_READ");
        var skillWrite = await _context.Skills.FirstAsync(s => s.Code == "SK_WRITE");
        var skillListen = await _context.Skills.FirstAsync(s => s.Code == "SK_LISTEN");
        var skillSpeak = await _context.Skills.FirstAsync(s => s.Code == "SK_SPEAK");
        var skillMath = await _context.Skills.FirstAsync(s => s.Code == "SK_MATH");
        var skillCrit = await _context.Skills.FirstAsync(s => s.Code == "SK_CRIT");

        var topics = new List<Topic>
        {
            new Topic { Name = "IELTS Listening Comprehension", SkillId = skillListen.Id, SectionId = ieltsListening!.Id },
            new Topic { Name = "IELTS Note & Form Completion", SkillId = skillListen.Id, SectionId = ieltsListening.Id },
            new Topic { Name = "IELTS Academic Reading", SkillId = skillRead.Id, SectionId = ieltsReading!.Id },
            new Topic { Name = "IELTS Reading: Matching & True/False", SkillId = skillRead.Id, SectionId = ieltsReading.Id },
            new Topic { Name = "IELTS Task 1: Data Description", SkillId = skillWrite.Id, SectionId = ieltsWriting!.Id },
            new Topic { Name = "IELTS Task 2: Essay Writing", SkillId = skillWrite.Id, SectionId = ieltsWriting.Id },
            new Topic { Name = "IELTS Speaking Parts 1 & 2", SkillId = skillSpeak.Id, SectionId = ieltsSpeaking!.Id },
            new Topic { Name = "IELTS Speaking Part 3: Discussion", SkillId = skillSpeak.Id, SectionId = ieltsSpeaking.Id },
            new Topic { Name = "Calculus & Analysis", SkillId = skillMath.Id, SectionId = cscaMath!.Id },
            new Topic { Name = "Probability & Statistics", SkillId = skillMath.Id, SectionId = cscaMath.Id },
            new Topic { Name = "Discrete Mathematics", SkillId = skillMath.Id, SectionId = cscaMath.Id },
            new Topic { Name = "Formal Logic", SkillId = skillCrit.Id, SectionId = cscaLogic!.Id },
            new Topic { Name = "Algorithmic Thinking", SkillId = skillCrit.Id, SectionId = cscaLogic.Id },
            new Topic { Name = "Data Interpretation", SkillId = skillCrit.Id, SectionId = cscaLogic.Id }
        };

        await _context.Topics.AddRangeAsync(topics);
        await _context.SaveChangesAsync();
    }

    private async Task SeedIeltsCscaFormulaCardsAsync()
    {
        var ieltsWritingTopic = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "IELTS Task 2: Essay Writing");
        var calculusTopic = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "Calculus & Analysis");
        var probTopic = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "Probability & Statistics");
        var discreteTopic = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "Discrete Mathematics");
        var formalLogicTopic = await _context.Topics.FirstOrDefaultAsync(t => t.Name == "Formal Logic");

        if (ieltsWritingTopic == null || calculusTopic == null) return;

        var cards = new List<FormulaCard>
        {
            // IELTS
            new FormulaCard { TopicId = ieltsWritingTopic.Id, Title = "Essay Introduction Template", Formula = @"\text{Topic sentence} + \text{Thesis} + \text{Outline}", Description = "Introduction = paraphrase question + state position + preview 2 main points", SortOrder = 0 },
            new FormulaCard { TopicId = ieltsWritingTopic.Id, Title = "Body Paragraph Structure", Formula = @"\text{TS} \to \text{Explain} \to \text{Example} \to \text{Link}", Description = "Topic Sentence -> Explanation -> Example -> Link back to thesis", SortOrder = 1 },
            new FormulaCard { TopicId = ieltsWritingTopic.Id, Title = "Cohesion Devices", Formula = @"\text{Moreover, Furthermore, However, Nevertheless, In contrast}", Description = "Use linking words to connect ideas across paragraphs", SortOrder = 2 },
            // CSCA
            new FormulaCard { TopicId = calculusTopic.Id, Title = "Power Rule", Formula = @"\frac{d}{dx} x^n = n \cdot x^{n-1}", Description = "Differentiate polynomial terms", SortOrder = 0 },
            new FormulaCard { TopicId = calculusTopic.Id, Title = "Chain Rule", Formula = @"\frac{d}{dx} f(g(x)) = f'(g(x)) \cdot g'(x)", Description = "Differentiate composite functions", SortOrder = 1 },
            new FormulaCard { TopicId = calculusTopic.Id, Title = "Integration by Parts", Formula = @"\int u\,dv = uv - \int v\,du", Description = "Integrate products of functions", SortOrder = 2 },
            new FormulaCard { TopicId = probTopic!.Id, Title = "Bayes' Theorem", Formula = @"P(A|B) = \frac{P(B|A) \cdot P(A)}{P(B)}", Description = "Calculate conditional probability", SortOrder = 3 },
            new FormulaCard { TopicId = probTopic.Id, Title = "Variance", Formula = @"\sigma^2 = \frac{1}{n}\sum_{i=1}^n (x_i - \bar{x})^2", Description = "Measure of spread around the mean", SortOrder = 4 },
            new FormulaCard { TopicId = discreteTopic!.Id, Title = "Permutations", Formula = @"P(n,r) = \frac{n!}{(n-r)!}", Description = "Number of ordered arrangements", SortOrder = 5 },
            new FormulaCard { TopicId = discreteTopic.Id, Title = "Combinations", Formula = @"C(n,r) = \frac{n!}{r!(n-r)!}", Description = "Number of unordered selections", SortOrder = 6 },
            new FormulaCard { TopicId = formalLogicTopic!.Id, Title = "Modus Ponens", Formula = @"(P \to Q) \wedge P \Rightarrow Q", Description = "If P implies Q and P is true, then Q is true", SortOrder = 7 },
            new FormulaCard { TopicId = formalLogicTopic.Id, Title = "De Morgan's Laws", Formula = @"\neg(P \wedge Q) = \neg P \vee \neg Q", Description = "Negate conjunctions and disjunctions", SortOrder = 8 }
        };

        await _context.FormulaCards.AddRangeAsync(cards);
        await _context.SaveChangesAsync();
    }

    private async Task SeedIeltsCscaFlashcardDecksAsync()
    {
        var decks = new List<FlashcardDeck>
        {
            new FlashcardDeck
            {
                ExamTypeCode = "IELTS",
                Title = "IELTS Academic Vocabulary",
                Description = "Essential academic words for IELTS Reading and Writing",
                Cards = new List<Flashcard>
                {
                    new Flashcard { Front = "Ubiquitous", Back = "Present, appearing, or found everywhere. Example: 'Smartphones have become ubiquitous in modern society.'", SortOrder = 0 },
                    new Flashcard { Front = "Paradigm", Back = "A typical example or model. Example: 'The discovery shifted the paradigm of climate science.'", SortOrder = 1 },
                    new Flashcard { Front = "Exacerbate", Back = "To make a problem or situation worse. Example: 'Pollution exacerbates respiratory problems.'", SortOrder = 2 },
                    new Flashcard { Front = "Mitigate", Back = "To make less severe or reduce. Example: 'Green spaces help mitigate urban heat islands.'", SortOrder = 3 },
                    new Flashcard { Front = "Unprecedented", Back = "Never done or known before. Example: 'The pandemic led to unprecedented remote work adoption.'", SortOrder = 4 },
                    new Flashcard { Front = "Pragmatic", Back = "Dealing with things in a practical way. Example: 'A pragmatic approach to environmental policy.'", SortOrder = 5 },
                    new Flashcard { Front = "Subsequently", Back = "After a particular thing has happened. Example: 'The economy grew; subsequently, employment rates rose.'", SortOrder = 6 },
                    new Flashcard { Front = "Inherent", Back = "Existing as a natural part. Example: 'There are inherent risks in any investment.'", SortOrder = 7 }
                }
            },
            new FlashcardDeck
            {
                ExamTypeCode = "CSCA",
                Title = "CSCA Core Concepts",
                Description = "Key mathematical and logical concepts for CSCA exam",
                Cards = new List<Flashcard>
                {
                    new Flashcard { Front = "Derivative", Back = "Rate of change of a function. $f'(x) = \\lim_{h \\to 0} \\frac{f(x+h) - f(x)}{h}$", SortOrder = 0 },
                    new Flashcard { Front = "Integral", Back = "Accumulation of quantity / area under curve. $\\int_a^b f(x)\\,dx = F(b) - F(a)$", SortOrder = 1 },
                    new Flashcard { Front = "Conditional Probability", Back = "$P(A|B) = \\frac{P(A \\cap B)}{P(B)}$. Probability of A given B occurred.", SortOrder = 2 },
                    new Flashcard { Front = "Graph Degree Sum", Back = "Handshaking Lemma: $\\sum \\deg(v) = 2|E|$. Sum of all vertex degrees equals twice the edges.", SortOrder = 3 },
                    new Flashcard { Front = "Modus Tollens", Back = "If $P \\to Q$ and $\\neg Q$, then $\\neg P$. Contrapositive reasoning.", SortOrder = 4 },
                    new Flashcard { Front = "Big-O Notation", Back = "$f(n) = O(g(n))$ means $f$ grows no faster than $g$. Example: binary search is $O(\\log n)$.", SortOrder = 5 },
                    new Flashcard { Front = "Normal Distribution", Back = "Bell curve: 68% within $\\pm 1\\sigma$, 95% within $\\pm 2\\sigma$, 99.7% within $\\pm 3\\sigma$.", SortOrder = 6 },
                    new Flashcard { Front = "Proof by Contradiction", Back = "Assume the negation of the statement, derive a logical contradiction, conclude original statement is true.", SortOrder = 7 }
                }
            }
        };

        await _context.FlashcardDecks.AddRangeAsync(decks);
        await _context.SaveChangesAsync();
    }

    private async Task SeedIeltsCscaStrategyGuidesAsync()
    {
        var guides = new List<StrategyGuide>
        {
            new StrategyGuide
            {
                ExamTypeCode = "IELTS",
                Title = "IELTS Writing Band 7+ Strategy",
                Summary = "Proven techniques for achieving Band 7 or higher in IELTS Writing",
                Category = "Writing",
                EstimatedReadMinutes = 8,
                Content = @"# IELTS Writing Band 7+ Strategy

## Task 1 (20 minutes)
- **Paraphrase** the question — never copy it word for word
- Identify **key trends** (rise, fall, peak, plateau)
- Use varied vocabulary: *increased → surged, climbed, soared*
- Include specific data points but don't list every number
- Structure: Introduction → Overview → Detail paragraph 1 → Detail paragraph 2

## Task 2 (40 minutes)
- Plan for 5 minutes before writing
- Clear position in introduction: agree, disagree, or balanced
- Each body paragraph: Topic sentence → Explain → Example → Link
- Use cohesive devices: *Moreover, However, In contrast, Consequently*
- Conclusion: restate your position, do NOT add new ideas

## Band 7 Criteria
- **Task Achievement**: fully address all parts of the task
- **Coherence**: logical paragraphing, clear progression
- **Lexical Resource**: use less common vocabulary accurately
- **Grammar**: mix of complex and simple sentences, few errors"
            },
            new StrategyGuide
            {
                ExamTypeCode = "IELTS",
                Title = "IELTS Reading Time Management",
                Summary = "How to finish all 40 questions in 60 minutes",
                Category = "Reading",
                EstimatedReadMinutes = 5,
                Content = @"# IELTS Reading Time Management

## Time Allocation
- Passage 1 (easiest): **15 minutes**
- Passage 2 (medium): **20 minutes**
- Passage 3 (hardest): **25 minutes**

## Strategies
1. **Skim first**: Read title, headings, first sentences — 2 minutes per passage
2. **Read questions before** re-reading the passage in detail
3. **Matching headings**: do these first, they give you the passage structure
4. **True/False/Not Given**: focus on exact wording, not assumptions
5. **Never leave blanks**: guess if you're running out of time

## Common Traps
- 'Not Given' ≠ 'False' — if the text doesn't mention it, it's Not Given
- Synonyms and paraphrasing are key — answers rarely use the same words as the passage
- Watch for qualifiers: *always, never, some, most* change the meaning"
            },
            new StrategyGuide
            {
                ExamTypeCode = "CSCA",
                Title = "CSCA Math Analysis Strategy",
                Summary = "Techniques for tackling calculus, probability, and discrete math problems",
                Category = "Math",
                EstimatedReadMinutes = 7,
                Content = @"# CSCA Math Analysis Strategy

## Calculus Problems
- Always check: is this asking for derivative or integral?
- **Chain rule**: when you see a function inside a function
- **Product/quotient rules**: when multiplying or dividing functions
- For definite integrals: find antiderivative, then evaluate at bounds

## Probability
- Draw a tree diagram or Venn diagram for complex scenarios
- **Bayes' Theorem**: use when given P(B|A) but need P(A|B)
- Independent events: P(A ∩ B) = P(A) × P(B)
- Complement rule: P(not A) = 1 - P(A)

## Discrete Math
- **Combinations vs Permutations**: does ORDER matter?
- Graph theory: count vertices, edges, check for Euler/Hamilton paths
- Modular arithmetic: look for patterns in remainders

## General Tips
- Show all working — partial credit may apply
- Check units and dimensions
- If stuck, try plugging in simple numbers to test"
            },
            new StrategyGuide
            {
                ExamTypeCode = "CSCA",
                Title = "CSCA Logical Reasoning Strategy",
                Summary = "Master formal logic, algorithms, and data interpretation",
                Category = "Logic",
                EstimatedReadMinutes = 6,
                Content = @"# CSCA Logical Reasoning Strategy

## Formal Logic
- Identify premise → conclusion structure
- **Modus Ponens**: P→Q, P ∴ Q
- **Modus Tollens**: P→Q, ¬Q ∴ ¬P
- Watch for common fallacies: affirming the consequent, denying the antecedent
- Use truth tables for complex compound statements

## Algorithmic Thinking
- Count operations as a function of input size n
- Nested loops → O(n²), binary search → O(log n)
- Recursion: identify base case and recursive case
- Compare algorithms by worst-case complexity

## Data Interpretation
- Read ALL axis labels and legends before answering
- Calculate percentage change: (new − old) / old × 100%
- Spot trends, outliers, and anomalies
- Dual-axis charts: be careful with different scales

## Time Management
- Don't spend more than 3 minutes on any single question
- Eliminate obviously wrong answers first
- If stuck, mark and return — later questions may be easier"
            }
        };

        await _context.StrategyGuides.AddRangeAsync(guides);
        await _context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════
    //  CSCA RESTRUCTURE: 3 subjects × deep chapter hierarchy
    //  Subject → Chapter → Section → Topic (encoded in topic names)
    //  ExamSection = Subject, Topic = leaf with numbered prefix
    // ═══════════════════════════════════════════════════════

    private async Task SeedCscaRestructureAsync()
    {
        // 1. Remove old CSCA sections, topics, and related data
        var oldSections = await _context.ExamSections
            .Where(s => s.ExamTypeCode == "CSCA"
                        && !s.Name.Contains("Mathematics") && !s.Name.Contains("Physics") && !s.Name.Contains("Chemistry")
                        && !s.Name.Contains("Chinese Technical") && !s.Name.Contains("Chinese Humanitarian"))
            .ToListAsync();

        if (oldSections.Any())
        {
            var oldSectionIds = oldSections.Select(s => s.Id).ToList();
            var oldTopics = await _context.Topics.Where(t => t.SectionId != null && oldSectionIds.Contains(t.SectionId.Value)).ToListAsync();
            if (oldTopics.Any())
            {
                var oldTopicIds = oldTopics.Select(t => t.Id).ToList();
                var oldQuestions = await _context.Questions.Where(q => oldTopicIds.Contains(q.TopicId)).ToListAsync();
                if (oldQuestions.Any())
                {
                    var qIds = oldQuestions.Select(q => q.Id).ToList();
                    _context.UserAnswers.RemoveRange(await _context.UserAnswers.Where(a => qIds.Contains(a.QuestionId)).ToListAsync());
                    _context.AnswerOptions.RemoveRange(await _context.AnswerOptions.Where(a => qIds.Contains(a.QuestionId)).ToListAsync());
                    _context.Questions.RemoveRange(oldQuestions);
                }
                _context.FormulaCards.RemoveRange(await _context.FormulaCards.Where(f => oldTopicIds.Contains(f.TopicId)).ToListAsync());
                _context.TopicLessons.RemoveRange(await _context.TopicLessons.Where(l => oldTopicIds.Contains(l.TopicId)).ToListAsync());
                _context.TopicDependencies.RemoveRange(await _context.TopicDependencies
                    .Where(d => oldTopicIds.Contains(d.TopicId) || oldTopicIds.Contains(d.PrerequisiteTopicId)).ToListAsync());
                _context.Topics.RemoveRange(oldTopics);
            }
            _context.ExamSections.RemoveRange(oldSections);
            await _context.SaveChangesAsync();
        }

        // 2. Add new CSCA sections if not yet present
        if (!await _context.ExamSections.AnyAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Mathematics")))
        {
            _context.ExamSections.AddRange(
                new ExamSection { ExamTypeCode = "CSCA", Name = "Mathematics", MinScore = 0, MaxScore = 100 },
                new ExamSection { ExamTypeCode = "CSCA", Name = "Physics", MinScore = 0, MaxScore = 100 },
                new ExamSection { ExamTypeCode = "CSCA", Name = "Chemistry", MinScore = 0, MaxScore = 100 }
            );
            await _context.SaveChangesAsync();
        }
        // Add Chinese Technical and Chinese Humanitarian sections if not yet present
        if (!await _context.ExamSections.AnyAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Chinese Technical")))
        {
            _context.ExamSections.AddRange(
                new ExamSection { ExamTypeCode = "CSCA", Name = "Chinese Technical", MinScore = 0, MaxScore = 100 },
                new ExamSection { ExamTypeCode = "CSCA", Name = "Chinese Humanitarian", MinScore = 0, MaxScore = 100 }
            );
            await _context.SaveChangesAsync();
        }

        // Update CSCA exam type name
        var cscaExam = await _context.ExamTypes.FirstOrDefaultAsync(e => e.Code == "CSCA");
        if (cscaExam != null && !cscaExam.Name.Contains("China"))
        {
            cscaExam.Name = "CSCA \u2014 China Scholastic Competency Assessment";
            await _context.SaveChangesAsync();
        }

        // 3. Resolve section & skill IDs (handle both old and renamed names)
        var mathSection = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Mathematics") && !s.Name.Contains("(CN)"));
        var physSection = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Physics") && !s.Name.Contains("(CN)"));
        var chemSection = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Chemistry") && !s.Name.Contains("(CN)"));
        var cnTechSection = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Chinese Technical"));
        var cnHumSection = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Chinese Humanitarian"));
        var skillMath = await _context.Skills.FirstAsync(s => s.Code == "SK_MATH");
        var skillPhys = await _context.Skills.FirstAsync(s => s.Code == "SK_PHYS");
        var skillChem = await _context.Skills.FirstAsync(s => s.Code == "SK_CHEM");
        var skillCnTech = await _context.Skills.FirstAsync(s => s.Code == "SK_CN_TECH");
        var skillCnHum = await _context.Skills.FirstAsync(s => s.Code == "SK_CN_HUM");
        var skillCrit = await _context.Skills.FirstAsync(s => s.Code == "SK_CRIT");

        // 4. Add topics per subject only if they don't already have the detailed set
        var hasMathDetailed = await _context.Topics.AnyAsync(t => t.SectionId == mathSection.Id && t.Name.StartsWith("1.1.1"));
        var hasPhysDetailed = await _context.Topics.AnyAsync(t => t.SectionId == physSection.Id && t.Name.StartsWith("P1.1.1"));
        var hasChemDetailed = await _context.Topics.AnyAsync(t => t.SectionId == chemSection.Id && t.Name.StartsWith("C1.1.1"));
        var hasCnTechDetailed = await _context.Topics.AnyAsync(t => t.SectionId == cnTechSection.Id && t.Name.StartsWith("CT1.1.1"));
        var hasCnHumDetailed = await _context.Topics.AnyAsync(t => t.SectionId == cnHumSection.Id && t.Name.StartsWith("CH1.1.1"));

        // If all subjects already have detailed topics, nothing to do
        if (hasMathDetailed && hasPhysDetailed && hasChemDetailed && hasCnTechDetailed && hasCnHumDetailed)
            return;

        // Remove old placeholder topics if they exist (they'll be replaced by detailed ones)
        if (!hasMathDetailed)
        {
            var oldMathTopics = await _context.Topics.Where(t => t.SectionId == mathSection.Id).ToListAsync();
            if (oldMathTopics.Any()) _context.Topics.RemoveRange(oldMathTopics);
        }
        if (!hasPhysDetailed)
        {
            var oldPhysTopics = await _context.Topics.Where(t => t.SectionId == physSection.Id).ToListAsync();
            if (oldPhysTopics.Any()) _context.Topics.RemoveRange(oldPhysTopics);
        }
        await _context.SaveChangesAsync();

        // ── MATHEMATICS — 20 Chapters ──
        var mathTopics = new List<Topic>
        {
            // Ch.1 Sets
            T("1.1.1 Elements of Sets", skillMath.Id, mathSection.Id),
            T("1.1.2 Common Number Sets", skillMath.Id, mathSection.Id),
            T("1.1.3 Element-Set Membership", skillMath.Id, mathSection.Id),
            T("1.1.4 Set Representations (roster, set-builder, Venn)", skillMath.Id, mathSection.Id),
            T("1.2.1 Intersection of Sets", skillMath.Id, mathSection.Id),
            T("1.2.2 Union of Sets", skillMath.Id, mathSection.Id),
            T("1.2.3 Complement (Difference) of Sets", skillMath.Id, mathSection.Id),
            T("1.3.1 Necessary and Sufficient Conditions", skillCrit.Id, mathSection.Id),
            T("1.3.2 Negation of Compound Statements", skillCrit.Id, mathSection.Id),
            T("1.3.3 Sets and Logical Conditions", skillCrit.Id, mathSection.Id),
            T("1.4.1 Universal Set and Properties", skillMath.Id, mathSection.Id),
            T("1.4.2 Operation Properties (commutativity, associativity)", skillMath.Id, mathSection.Id),
            T("1.4.3 De Morgan's Laws", skillMath.Id, mathSection.Id),
            T("1.4.4 Applied Set Problems", skillMath.Id, mathSection.Id),

            // Ch.2 Inequalities
            T("2.1.1 Linear Inequalities", skillMath.Id, mathSection.Id),
            T("2.1.2 Quadratic Inequalities", skillMath.Id, mathSection.Id),
            T("2.1.3 Rational Inequalities", skillMath.Id, mathSection.Id),
            T("2.2.1 Absolute Value Inequalities", skillMath.Id, mathSection.Id),
            T("2.2.2 AM-GM and Cauchy-Schwarz", skillMath.Id, mathSection.Id),
            T("2.3.1 Systems of Inequalities", skillMath.Id, mathSection.Id),
            T("2.3.2 Parametric Inequalities", skillMath.Id, mathSection.Id),

            // Ch.3 Functions
            T("3.1.1 Domain, Range, and Mapping", skillMath.Id, mathSection.Id),
            T("3.1.2 Piecewise Functions", skillMath.Id, mathSection.Id),
            T("3.2.1 Monotonicity and Extrema", skillMath.Id, mathSection.Id),
            T("3.2.2 Even and Odd Functions", skillMath.Id, mathSection.Id),
            T("3.2.3 Periodicity", skillMath.Id, mathSection.Id),
            T("3.3.1 Exponential Functions", skillMath.Id, mathSection.Id),
            T("3.3.2 Logarithmic Functions", skillMath.Id, mathSection.Id),
            T("3.3.3 Power Functions", skillMath.Id, mathSection.Id),
            T("3.4.1 Composite Functions", skillMath.Id, mathSection.Id),
            T("3.4.2 Inverse Functions", skillMath.Id, mathSection.Id),

            // Ch.4 Trigonometric Functions
            T("4.1.1 Radian Measure and Arc Length", skillMath.Id, mathSection.Id),
            T("4.1.2 Unit Circle Definition", skillMath.Id, mathSection.Id),
            T("4.2.1 Graphs of sin, cos, tan", skillMath.Id, mathSection.Id),
            T("4.2.2 Amplitude, Period, Phase Shift", skillMath.Id, mathSection.Id),
            T("4.3.1 Fundamental Trig Identities", skillMath.Id, mathSection.Id),
            T("4.3.2 Simplification with Identities", skillMath.Id, mathSection.Id),

            // Ch.5 Inverse Trigonometric Functions
            T("5.1.1 arcsin, arccos, arctan Definitions", skillMath.Id, mathSection.Id),
            T("5.1.2 Domains and Ranges of Inverse Trig", skillMath.Id, mathSection.Id),
            T("5.2.1 Evaluating Inverse Trig Expressions", skillMath.Id, mathSection.Id),
            T("5.2.2 Composite Inverse Trig Problems", skillMath.Id, mathSection.Id),

            // Ch.6 Trig Sum/Difference Formulas
            T("6.1.1 Sum and Difference Formulas", skillMath.Id, mathSection.Id),
            T("6.1.2 Double Angle Formulas", skillMath.Id, mathSection.Id),
            T("6.1.3 Half Angle Formulas", skillMath.Id, mathSection.Id),
            T("6.2.1 Product-to-Sum Transformations", skillMath.Id, mathSection.Id),
            T("6.2.2 Sum-to-Product Transformations", skillMath.Id, mathSection.Id),
            T("6.3.1 Trigonometric Equations", skillMath.Id, mathSection.Id),

            // Ch.7 Sequences
            T("7.1.1 Arithmetic Sequences", skillMath.Id, mathSection.Id),
            T("7.1.2 Arithmetic Series (Sum Formulas)", skillMath.Id, mathSection.Id),
            T("7.2.1 Geometric Sequences", skillMath.Id, mathSection.Id),
            T("7.2.2 Geometric Series (Sum Formulas)", skillMath.Id, mathSection.Id),
            T("7.3.1 Recursive Sequences", skillMath.Id, mathSection.Id),
            T("7.3.2 Summation Techniques (telescoping, partial fractions)", skillMath.Id, mathSection.Id),

            // Ch.8 Complex Numbers
            T("8.1.1 Imaginary Unit and Algebraic Form", skillMath.Id, mathSection.Id),
            T("8.1.2 Operations with Complex Numbers", skillMath.Id, mathSection.Id),
            T("8.2.1 Modulus and Conjugate", skillMath.Id, mathSection.Id),
            T("8.2.2 Trigonometric (Polar) Form", skillMath.Id, mathSection.Id),
            T("8.3.1 De Moivre's Theorem", skillMath.Id, mathSection.Id),

            // Ch.9 Lines in Plane
            T("9.1.1 Slope and Equation of a Line", skillMath.Id, mathSection.Id),
            T("9.1.2 Forms: slope-intercept, point-slope, general", skillMath.Id, mathSection.Id),
            T("9.2.1 Parallel and Perpendicular Lines", skillMath.Id, mathSection.Id),
            T("9.2.2 Distance from Point to Line", skillMath.Id, mathSection.Id),
            T("9.3.1 Systems of Linear Equations (2D)", skillMath.Id, mathSection.Id),

            // Ch.10 Conic Sections
            T("10.1.1 Circle: Standard and General Form", skillMath.Id, mathSection.Id),
            T("10.2.1 Ellipse: Definition and Equation", skillMath.Id, mathSection.Id),
            T("10.2.2 Ellipse: Eccentricity and Properties", skillMath.Id, mathSection.Id),
            T("10.3.1 Hyperbola: Definition and Equation", skillMath.Id, mathSection.Id),
            T("10.3.2 Hyperbola: Asymptotes and Properties", skillMath.Id, mathSection.Id),
            T("10.4.1 Parabola: Definition and Equation", skillMath.Id, mathSection.Id),
            T("10.5.1 Line-Conic Intersection Problems", skillMath.Id, mathSection.Id),

            // Ch.11 Plane Vectors
            T("11.1.1 Vector Concepts and Notation", skillMath.Id, mathSection.Id),
            T("11.1.2 Vector Addition and Scalar Multiplication", skillMath.Id, mathSection.Id),
            T("11.2.1 Dot Product and Angle Between Vectors", skillMath.Id, mathSection.Id),
            T("11.2.2 Projection and Decomposition", skillMath.Id, mathSection.Id),
            T("11.3.1 Coordinate Form of Vectors", skillMath.Id, mathSection.Id),

            // Ch.12 Space Vectors
            T("12.1.1 Vectors in 3D Space", skillMath.Id, mathSection.Id),
            T("12.1.2 Cross Product", skillMath.Id, mathSection.Id),
            T("12.2.1 Spatial Coordinate Systems", skillMath.Id, mathSection.Id),
            T("12.2.2 Distance and Angle in Space", skillMath.Id, mathSection.Id),

            // Ch.13 Space Planes & Lines
            T("13.1.1 Equation of a Plane", skillMath.Id, mathSection.Id),
            T("13.1.2 Line-Plane Relationships", skillMath.Id, mathSection.Id),
            T("13.2.1 Dihedral Angles", skillMath.Id, mathSection.Id),
            T("13.2.2 Distance Between Skew Lines", skillMath.Id, mathSection.Id),

            // Ch.14 Limits
            T("14.1.1 Concept of a Limit", skillMath.Id, mathSection.Id),
            T("14.1.2 Properties and Computation of Limits", skillMath.Id, mathSection.Id),
            T("14.2.1 Limits at Infinity", skillMath.Id, mathSection.Id),
            T("14.2.2 Continuity of Functions", skillMath.Id, mathSection.Id),
            T("14.3.1 Squeeze Theorem", skillMath.Id, mathSection.Id),

            // Ch.15 Derivatives
            T("15.1.1 Definition of a Derivative", skillMath.Id, mathSection.Id),
            T("15.1.2 Geometric Meaning (Tangent Line)", skillMath.Id, mathSection.Id),
            T("15.2.1 Basic Differentiation Rules", skillMath.Id, mathSection.Id),
            T("15.2.2 Chain Rule", skillMath.Id, mathSection.Id),
            T("15.2.3 Derivatives of Trig and Log Functions", skillMath.Id, mathSection.Id),

            // Ch.16 Applications of Derivatives
            T("16.1.1 Finding Monotonic Intervals", skillMath.Id, mathSection.Id),
            T("16.1.2 Local and Global Extrema", skillMath.Id, mathSection.Id),
            T("16.2.1 Optimization Problems", skillMath.Id, mathSection.Id),
            T("16.2.2 Second Derivative Test", skillMath.Id, mathSection.Id),
            T("16.3.1 Curve Sketching with Derivatives", skillMath.Id, mathSection.Id),

            // Ch.17 Permutations & Combinations
            T("17.1.1 Counting Principles (Addition & Multiplication)", skillMath.Id, mathSection.Id),
            T("17.1.2 Permutations", skillMath.Id, mathSection.Id),
            T("17.1.3 Combinations", skillMath.Id, mathSection.Id),
            T("17.2.1 Binomial Theorem", skillMath.Id, mathSection.Id),
            T("17.2.2 Pascal's Triangle Properties", skillMath.Id, mathSection.Id),

            // Ch.18 Random Events & Probability
            T("18.1.1 Sample Space and Events", skillMath.Id, mathSection.Id),
            T("18.1.2 Classical Probability", skillMath.Id, mathSection.Id),
            T("18.2.1 Conditional Probability", skillMath.Id, mathSection.Id),
            T("18.2.2 Independent Events", skillMath.Id, mathSection.Id),
            T("18.3.1 Bayes' Theorem", skillMath.Id, mathSection.Id),

            // Ch.19 Random Variables
            T("19.1.1 Discrete Random Variables", skillMath.Id, mathSection.Id),
            T("19.1.2 Probability Distribution Tables", skillMath.Id, mathSection.Id),
            T("19.2.1 Expectation (Mean)", skillMath.Id, mathSection.Id),
            T("19.2.2 Variance and Standard Deviation", skillMath.Id, mathSection.Id),
            T("19.3.1 Binomial Distribution", skillMath.Id, mathSection.Id),
            T("19.3.2 Normal Distribution Basics", skillMath.Id, mathSection.Id),

            // Ch.20 Statistics
            T("20.1.1 Sampling Methods", skillMath.Id, mathSection.Id),
            T("20.1.2 Frequency Distributions and Histograms", skillMath.Id, mathSection.Id),
            T("20.2.1 Measures of Central Tendency", skillMath.Id, mathSection.Id),
            T("20.2.2 Measures of Dispersion", skillMath.Id, mathSection.Id),
            T("20.3.1 Linear Regression", skillMath.Id, mathSection.Id),
            T("20.3.2 Correlation Coefficient", skillMath.Id, mathSection.Id),
        };

        // ── PHYSICS — 12 Chapters ──
        var physTopics = new List<Topic>
        {
            // Ch.1 Force
            T("P1.1.1 Types of Forces (gravity, tension, friction, normal)", skillPhys.Id, physSection.Id),
            T("P1.1.2 Force Analysis and Free Body Diagrams", skillPhys.Id, physSection.Id),
            T("P1.2.1 Vector Addition of Forces", skillPhys.Id, physSection.Id),
            T("P1.2.2 Equilibrium Conditions", skillPhys.Id, physSection.Id),
            T("P1.3.1 Hooke's Law (Spring Force)", skillPhys.Id, physSection.Id),

            // Ch.2 Motion
            T("P2.1.1 Displacement, Velocity, Acceleration", skillPhys.Id, physSection.Id),
            T("P2.1.2 Uniform Motion", skillPhys.Id, physSection.Id),
            T("P2.2.1 Uniformly Accelerated Motion", skillPhys.Id, physSection.Id),
            T("P2.2.2 Free Fall", skillPhys.Id, physSection.Id),
            T("P2.3.1 Projectile Motion", skillPhys.Id, physSection.Id),
            T("P2.3.2 Circular Motion", skillPhys.Id, physSection.Id),

            // Ch.3 Newton's Laws
            T("P3.1.1 Newton's First Law (Inertia)", skillPhys.Id, physSection.Id),
            T("P3.1.2 Newton's Second Law (F=ma)", skillPhys.Id, physSection.Id),
            T("P3.1.3 Newton's Third Law (Action-Reaction)", skillPhys.Id, physSection.Id),
            T("P3.2.1 Applications on Inclined Planes", skillPhys.Id, physSection.Id),
            T("P3.2.2 Connected Bodies and Pulley Systems", skillPhys.Id, physSection.Id),

            // Ch.4 Momentum
            T("P4.1.1 Impulse and Momentum", skillPhys.Id, physSection.Id),
            T("P4.1.2 Impulse-Momentum Theorem", skillPhys.Id, physSection.Id),
            T("P4.2.1 Conservation of Momentum", skillPhys.Id, physSection.Id),
            T("P4.2.2 Elastic and Inelastic Collisions", skillPhys.Id, physSection.Id),

            // Ch.5 Mechanical Energy
            T("P5.1.1 Work Done by a Force", skillPhys.Id, physSection.Id),
            T("P5.1.2 Work-Energy Theorem", skillPhys.Id, physSection.Id),
            T("P5.2.1 Kinetic and Potential Energy", skillPhys.Id, physSection.Id),
            T("P5.2.2 Conservation of Mechanical Energy", skillPhys.Id, physSection.Id),
            T("P5.3.1 Power", skillPhys.Id, physSection.Id),

            // Ch.6 Electric Field
            T("P6.1.1 Electric Charge and Coulomb's Law", skillPhys.Id, physSection.Id),
            T("P6.1.2 Electric Field Intensity", skillPhys.Id, physSection.Id),
            T("P6.2.1 Electric Potential and Potential Difference", skillPhys.Id, physSection.Id),
            T("P6.2.2 Capacitance and Capacitors", skillPhys.Id, physSection.Id),
            T("P6.3.1 Electric Field Lines and Equipotential Surfaces", skillPhys.Id, physSection.Id),

            // Ch.7 DC Circuits
            T("P7.1.1 Ohm's Law", skillPhys.Id, physSection.Id),
            T("P7.1.2 Resistors in Series and Parallel", skillPhys.Id, physSection.Id),
            T("P7.2.1 EMF and Internal Resistance", skillPhys.Id, physSection.Id),
            T("P7.2.2 Kirchhoff's Laws", skillPhys.Id, physSection.Id),
            T("P7.3.1 Electrical Power and Energy", skillPhys.Id, physSection.Id),

            // Ch.8 Magnetic Field
            T("P8.1.1 Magnetic Field and Magnetic Force", skillPhys.Id, physSection.Id),
            T("P8.1.2 Force on a Current-Carrying Conductor", skillPhys.Id, physSection.Id),
            T("P8.2.1 Lorentz Force and Charged Particle Motion", skillPhys.Id, physSection.Id),
            T("P8.2.2 Applications (mass spectrometer, cyclotron)", skillPhys.Id, physSection.Id),

            // Ch.9 Electromagnetic Induction
            T("P9.1.1 Magnetic Flux", skillPhys.Id, physSection.Id),
            T("P9.1.2 Faraday's Law of Induction", skillPhys.Id, physSection.Id),
            T("P9.2.1 Lenz's Law", skillPhys.Id, physSection.Id),
            T("P9.2.2 Self-Inductance", skillPhys.Id, physSection.Id),

            // Ch.10 Vibrations & Waves
            T("P10.1.1 Simple Harmonic Motion", skillPhys.Id, physSection.Id),
            T("P10.1.2 Pendulum and Spring Oscillator", skillPhys.Id, physSection.Id),
            T("P10.2.1 Transverse and Longitudinal Waves", skillPhys.Id, physSection.Id),
            T("P10.2.2 Wave Speed, Frequency, Wavelength", skillPhys.Id, physSection.Id),
            T("P10.3.1 Superposition and Interference", skillPhys.Id, physSection.Id),
            T("P10.3.2 Standing Waves and Resonance", skillPhys.Id, physSection.Id),
            T("P10.4.1 Sound Waves and Doppler Effect", skillPhys.Id, physSection.Id),

            // Ch.11 Heat
            T("P11.1.1 Temperature and Thermometers", skillPhys.Id, physSection.Id),
            T("P11.1.2 Ideal Gas Law", skillPhys.Id, physSection.Id),
            T("P11.2.1 Internal Energy and Heat Transfer", skillPhys.Id, physSection.Id),
            T("P11.2.2 First Law of Thermodynamics", skillPhys.Id, physSection.Id),
            T("P11.3.1 Phase Changes and Latent Heat", skillPhys.Id, physSection.Id),

            // Ch.12 Geometrical Optics
            T("P12.1.1 Reflection and Plane Mirrors", skillPhys.Id, physSection.Id),
            T("P12.1.2 Refraction and Snell's Law", skillPhys.Id, physSection.Id),
            T("P12.2.1 Total Internal Reflection", skillPhys.Id, physSection.Id),
            T("P12.2.2 Thin Lens Equation", skillPhys.Id, physSection.Id),
            T("P12.3.1 Image Formation (convex/concave lenses)", skillPhys.Id, physSection.Id),
        };

        // ── CHEMISTRY — 14 Chapters ──
        var chemTopics = new List<Topic>
        {
            // Ch.1 Chemical Fundamentals
            T("C1.1.1 Atoms, Molecules, and Ions", skillChem.Id, chemSection.Id),
            T("C1.1.2 Relative Atomic and Molecular Mass", skillChem.Id, chemSection.Id),
            T("C1.2.1 Chemical Formulas and Naming", skillChem.Id, chemSection.Id),
            T("C1.2.2 Valence and Oxidation States", skillChem.Id, chemSection.Id),

            // Ch.2 Chemical Reactions and Equations
            T("C2.1.1 Balancing Chemical Equations", skillChem.Id, chemSection.Id),
            T("C2.1.2 Types of Chemical Reactions", skillChem.Id, chemSection.Id),
            T("C2.2.1 Oxidation-Reduction (Redox) Reactions", skillChem.Id, chemSection.Id),
            T("C2.2.2 Identifying Oxidizing and Reducing Agents", skillChem.Id, chemSection.Id),

            // Ch.3 The Mole and Chemical Calculations
            T("C3.1.1 Mole Concept and Avogadro's Number", skillChem.Id, chemSection.Id),
            T("C3.1.2 Molar Mass Calculations", skillChem.Id, chemSection.Id),
            T("C3.2.1 Molar Volume of Gases (STP)", skillChem.Id, chemSection.Id),
            T("C3.2.2 Stoichiometric Calculations", skillChem.Id, chemSection.Id),
            T("C3.3.1 Solution Concentration (Molarity)", skillChem.Id, chemSection.Id),
            T("C3.3.2 Dilution Calculations", skillChem.Id, chemSection.Id),

            // Ch.4 Alkali Metals and Their Compounds
            T("C4.1.1 Properties of Sodium and Potassium", skillChem.Id, chemSection.Id),
            T("C4.1.2 Sodium Compounds (Na2O, NaOH, Na2CO3, NaHCO3)", skillChem.Id, chemSection.Id),
            T("C4.2.1 Flame Tests and Identification", skillChem.Id, chemSection.Id),

            // Ch.5 Halogens
            T("C5.1.1 Properties of Chlorine, Bromine, Iodine", skillChem.Id, chemSection.Id),
            T("C5.1.2 Halogen Reactivity Trends", skillChem.Id, chemSection.Id),
            T("C5.2.1 Hydrogen Halides and Halide Ions", skillChem.Id, chemSection.Id),
            T("C5.2.2 Halogen Displacement Reactions", skillChem.Id, chemSection.Id),

            // Ch.6 Sulfur, Nitrogen, and Their Compounds
            T("C6.1.1 Sulfuric Acid and Sulfur Oxides", skillChem.Id, chemSection.Id),
            T("C6.1.2 Industrial Synthesis of H2SO4 (Contact Process)", skillChem.Id, chemSection.Id),
            T("C6.2.1 Ammonia and Ammonium Compounds", skillChem.Id, chemSection.Id),
            T("C6.2.2 Nitric Acid and Nitrogen Oxides", skillChem.Id, chemSection.Id),
            T("C6.3.1 Industrial Synthesis of NH3 (Haber Process)", skillChem.Id, chemSection.Id),

            // Ch.7 The Periodic Table and Periodic Law
            T("C7.1.1 Structure of the Periodic Table", skillChem.Id, chemSection.Id),
            T("C7.1.2 Periodic Trends (atomic radius, ionization energy, electronegativity)", skillChem.Id, chemSection.Id),
            T("C7.2.1 Metallic vs Non-metallic Character", skillChem.Id, chemSection.Id),
            T("C7.2.2 Predicting Properties from Position", skillChem.Id, chemSection.Id),

            // Ch.8 Chemical Bonding
            T("C8.1.1 Ionic Bonding", skillChem.Id, chemSection.Id),
            T("C8.1.2 Covalent Bonding (polar/non-polar)", skillChem.Id, chemSection.Id),
            T("C8.2.1 Metallic Bonding", skillChem.Id, chemSection.Id),
            T("C8.2.2 Lewis Structures and VSEPR", skillChem.Id, chemSection.Id),
            T("C8.3.1 Intermolecular Forces (van der Waals, H-bonding)", skillChem.Id, chemSection.Id),

            // Ch.9 Chemical Reaction Rates
            T("C9.1.1 Factors Affecting Reaction Rate", skillChem.Id, chemSection.Id),
            T("C9.1.2 Collision Theory", skillChem.Id, chemSection.Id),
            T("C9.2.1 Catalysts and Activation Energy", skillChem.Id, chemSection.Id),

            // Ch.10 Chemical Equilibrium
            T("C10.1.1 Reversible Reactions and Dynamic Equilibrium", skillChem.Id, chemSection.Id),
            T("C10.1.2 Equilibrium Constant (Kc, Kp)", skillChem.Id, chemSection.Id),
            T("C10.2.1 Le Chatelier's Principle", skillChem.Id, chemSection.Id),
            T("C10.2.2 Equilibrium Calculations", skillChem.Id, chemSection.Id),

            // Ch.11 Solutions and Ionic Reactions
            T("C11.1.1 Electrolytes and Non-Electrolytes", skillChem.Id, chemSection.Id),
            T("C11.1.2 Ionic Equations", skillChem.Id, chemSection.Id),
            T("C11.2.1 Acid-Base Reactions and pH", skillChem.Id, chemSection.Id),
            T("C11.2.2 Hydrolysis of Salts", skillChem.Id, chemSection.Id),
            T("C11.3.1 Precipitation Reactions and Solubility Rules", skillChem.Id, chemSection.Id),

            // Ch.12 Electrochemistry
            T("C12.1.1 Galvanic (Voltaic) Cells", skillChem.Id, chemSection.Id),
            T("C12.1.2 Electrode Potentials and EMF", skillChem.Id, chemSection.Id),
            T("C12.2.1 Electrolysis (Faraday's Laws)", skillChem.Id, chemSection.Id),
            T("C12.2.2 Applications (electroplating, refining)", skillChem.Id, chemSection.Id),

            // Ch.13 Organic Chemistry Basics
            T("C13.1.1 Alkanes (nomenclature, isomerism)", skillChem.Id, chemSection.Id),
            T("C13.1.2 Alkenes (addition reactions)", skillChem.Id, chemSection.Id),
            T("C13.2.1 Alcohols and Ethers", skillChem.Id, chemSection.Id),
            T("C13.2.2 Carboxylic Acids and Esters", skillChem.Id, chemSection.Id),
            T("C13.3.1 Polymers and Polymerization", skillChem.Id, chemSection.Id),

            // Ch.14 Chemistry Experiments
            T("C14.1.1 Common Lab Apparatus and Techniques", skillChem.Id, chemSection.Id),
            T("C14.1.2 Gas Collection Methods", skillChem.Id, chemSection.Id),
            T("C14.2.1 Titration (acid-base, redox)", skillChem.Id, chemSection.Id),
            T("C14.2.2 Qualitative Analysis (ion identification)", skillChem.Id, chemSection.Id),
        };

        // ── CHINESE TECHNICAL (Professional Chinese STEM) — 8 Chapters ──
        var cnTechTopics = new List<Topic>
        {
            // Ch.1 Scientific Vocabulary
            T("CT1.1.1 Mathematics Terminology (数学术语)", skillCnTech.Id, cnTechSection.Id),
            T("CT1.1.2 Physics Terminology (物理术语)", skillCnTech.Id, cnTechSection.Id),
            T("CT1.1.3 Chemistry Terminology (化学术语)", skillCnTech.Id, cnTechSection.Id),
            T("CT1.2.1 Units and Measurements in Chinese (单位与测量)", skillCnTech.Id, cnTechSection.Id),

            // Ch.2 Reading Scientific Texts
            T("CT2.1.1 Reading Formulas and Equations (公式阅读)", skillCnTech.Id, cnTechSection.Id),
            T("CT2.1.2 Understanding Graphs and Tables (图表理解)", skillCnTech.Id, cnTechSection.Id),
            T("CT2.2.1 Scientific Article Structure (科技文章结构)", skillCnTech.Id, cnTechSection.Id),
            T("CT2.2.2 Abstract and Conclusion Comprehension (摘要与结论)", skillCnTech.Id, cnTechSection.Id),

            // Ch.3 Engineering and Technology
            T("CT3.1.1 Computer Science Terms (计算机科学)", skillCnTech.Id, cnTechSection.Id),
            T("CT3.1.2 Engineering Vocabulary (工程术语)", skillCnTech.Id, cnTechSection.Id),
            T("CT3.2.1 Environmental Science Terms (环境科学)", skillCnTech.Id, cnTechSection.Id),
            T("CT3.2.2 Medical and Biological Terms (医学与生物)", skillCnTech.Id, cnTechSection.Id),

            // Ch.4 Academic Writing (STEM)
            T("CT4.1.1 Writing Lab Reports (实验报告写作)", skillCnTech.Id, cnTechSection.Id),
            T("CT4.1.2 Technical Descriptions (技术描述)", skillCnTech.Id, cnTechSection.Id),
            T("CT4.2.1 Data Analysis Writing (数据分析写作)", skillCnTech.Id, cnTechSection.Id),

            // Ch.5 Listening Comprehension (STEM)
            T("CT5.1.1 Understanding Lectures (课堂听力)", skillCnTech.Id, cnTechSection.Id),
            T("CT5.1.2 Lab Instructions and Safety (实验指导)", skillCnTech.Id, cnTechSection.Id),
            T("CT5.2.1 Scientific Presentations (学术报告听力)", skillCnTech.Id, cnTechSection.Id),

            // Ch.6 Grammar for STEM Texts
            T("CT6.1.1 Passive Voice in Scientific Chinese (被动语态)", skillCnTech.Id, cnTechSection.Id),
            T("CT6.1.2 Conditional and Hypothetical Sentences (假设句)", skillCnTech.Id, cnTechSection.Id),
            T("CT6.2.1 Comparison and Contrast Structures (比较结构)", skillCnTech.Id, cnTechSection.Id),

            // Ch.7 Practice Sets
            T("CT7.1.1 Cloze Tests (STEM passages)", skillCnTech.Id, cnTechSection.Id),
            T("CT7.1.2 Reading Comprehension (STEM)", skillCnTech.Id, cnTechSection.Id),
            T("CT7.2.1 Vocabulary in Context", skillCnTech.Id, cnTechSection.Id),

            // Ch.8 Exam Strategies
            T("CT8.1.1 Time Management for 80 Questions in 90 Minutes", skillCnTech.Id, cnTechSection.Id),
            T("CT8.1.2 Elimination and Guessing Techniques", skillCnTech.Id, cnTechSection.Id),
        };

        // ── CHINESE HUMANITARIAN (Professional Chinese Humanities) — 8 Chapters ──
        var cnHumTopics = new List<Topic>
        {
            // Ch.1 Chinese Literature
            T("CH1.1.1 Classical Chinese Poetry (古诗词)", skillCnHum.Id, cnHumSection.Id),
            T("CH1.1.2 Modern Chinese Literature (现代文学)", skillCnHum.Id, cnHumSection.Id),
            T("CH1.2.1 Idioms and Proverbs (成语与谚语)", skillCnHum.Id, cnHumSection.Id),
            T("CH1.2.2 Literary Analysis Techniques (文学分析)", skillCnHum.Id, cnHumSection.Id),

            // Ch.2 Chinese History
            T("CH2.1.1 Ancient Chinese History (古代史)", skillCnHum.Id, cnHumSection.Id),
            T("CH2.1.2 Modern Chinese History (近现代史)", skillCnHum.Id, cnHumSection.Id),
            T("CH2.2.1 Historical Figures and Events (历史人物与事件)", skillCnHum.Id, cnHumSection.Id),

            // Ch.3 Chinese Philosophy and Thought
            T("CH3.1.1 Confucianism (儒家思想)", skillCnHum.Id, cnHumSection.Id),
            T("CH3.1.2 Taoism and Buddhism (道家与佛教)", skillCnHum.Id, cnHumSection.Id),
            T("CH3.2.1 Modern Chinese Thought (现代思想)", skillCnHum.Id, cnHumSection.Id),

            // Ch.4 Chinese Culture and Art
            T("CH4.1.1 Traditional Art Forms (传统艺术)", skillCnHum.Id, cnHumSection.Id),
            T("CH4.1.2 Festivals and Customs (节日与风俗)", skillCnHum.Id, cnHumSection.Id),
            T("CH4.2.1 Chinese Cinema and Music (电影与音乐)", skillCnHum.Id, cnHumSection.Id),

            // Ch.5 Chinese Society
            T("CH5.1.1 Chinese Education System (教育体系)", skillCnHum.Id, cnHumSection.Id),
            T("CH5.1.2 Social Issues and Current Events (社会问题)", skillCnHum.Id, cnHumSection.Id),
            T("CH5.2.1 Chinese Geography and Regions (地理与区域)", skillCnHum.Id, cnHumSection.Id),

            // Ch.6 Academic Writing (Humanities)
            T("CH6.1.1 Essay Structure in Chinese (论文结构)", skillCnHum.Id, cnHumSection.Id),
            T("CH6.1.2 Argumentative Writing (议论文写作)", skillCnHum.Id, cnHumSection.Id),
            T("CH6.2.1 Formal and Informal Register (正式与非正式用语)", skillCnHum.Id, cnHumSection.Id),

            // Ch.7 Reading Comprehension (Humanities)
            T("CH7.1.1 News and Media Articles (新闻阅读)", skillCnHum.Id, cnHumSection.Id),
            T("CH7.1.2 Academic Texts (学术文本)", skillCnHum.Id, cnHumSection.Id),
            T("CH7.2.1 Opinion and Editorial Analysis (评论分析)", skillCnHum.Id, cnHumSection.Id),

            // Ch.8 Exam Strategies
            T("CH8.1.1 Time Management for 80 Questions in 90 Minutes", skillCnHum.Id, cnHumSection.Id),
            T("CH8.1.2 Elimination and Guessing Techniques", skillCnHum.Id, cnHumSection.Id),
        };

        if (!hasMathDetailed) await _context.Topics.AddRangeAsync(mathTopics);
        if (!hasPhysDetailed) await _context.Topics.AddRangeAsync(physTopics);
        if (!hasChemDetailed) await _context.Topics.AddRangeAsync(chemTopics);
        if (!hasCnTechDetailed) await _context.Topics.AddRangeAsync(cnTechTopics);
        if (!hasCnHumDetailed) await _context.Topics.AddRangeAsync(cnHumTopics);
        await _context.SaveChangesAsync();
    }

    /// <summary>Helper to create a Topic with less boilerplate.</summary>
    private static Topic T(string name, int skillId, int sectionId) =>
        new Topic { Name = name, SkillId = skillId, SectionId = sectionId };

    private async Task SeedTutorSchoolsAsync()
    {
        if (await _context.TutorSchools.AnyAsync()) return;

        var linhao = new TutorSchool
        {
            Name = "Linhao Chinese",
            Slug = "linhao-chinese",
            Description = "Первая онлайн-школа в СНГ по подготовке студентов в Китай к экзамену CSCA. Авторский курс «Мандарин» с 0 до 1 HSK за 8 уроков.",
            LogoUrl = null,
            WebsiteUrl = null,
            InstagramUrl = "https://www.instagram.com/linhao.chinese/",
            TelegramUrl = "https://t.me/linhao_chinese",
            Specializations = "CSCA",
            IsPartner = true,
            IsActive = true,
        };
        _context.TutorSchools.Add(linhao);
        await _context.SaveChangesAsync();

        // Create tutors inside the school
        var tutorUsers = new[]
        {
            new User { Email = "teacher1@linhao.cn", Name = "Ли Вэй (李伟)", PasswordHash = BCrypt.Net.BCrypt.HashPassword("LinhaoTutor1!"), Role = UserRole.Tutor },
            new User { Email = "teacher2@linhao.cn", Name = "Чжан Мин (张明)", PasswordHash = BCrypt.Net.BCrypt.HashPassword("LinhaoTutor2!"), Role = UserRole.Tutor },
        };
        _context.Users.AddRange(tutorUsers);
        await _context.SaveChangesAsync();

        var profiles = new[]
        {
            new TutorProfile
            {
                UserId = tutorUsers[0].Id,
                SchoolId = linhao.Id,
                Headline = "Преподаватель CSCA математики и физики",
                Bio = "5 лет опыта подготовки к CSCA. Выпускники поступили в топ-10 университетов Китая.",
                Experience = "Пекинский университет, магистр педагогики. Сертифицированный преподаватель HSK.",
                Specializations = "CSCA",
                HourlyRate = 35,
                IsAvailable = true,
                IsVerified = true,
                AverageRating = 4.8m,
                TotalReviews = 12,
                TotalStudents = 45,
                ContactPreference = ContactPreference.Chat,
            },
            new TutorProfile
            {
                UserId = tutorUsers[1].Id,
                SchoolId = linhao.Id,
                Headline = "Преподаватель китайского языка и CSCA химии",
                Bio = "Носитель китайского языка. Помогаю студентам из СНГ освоить технический и гуманитарный китайский для CSCA.",
                Experience = "Шанхайский университет, бакалавр химии. 3 года преподавания HSK и CSCA.",
                Specializations = "CSCA",
                HourlyRate = 30,
                IsAvailable = true,
                IsVerified = true,
                AverageRating = 4.9m,
                TotalReviews = 8,
                TotalStudents = 30,
                ContactPreference = ContactPreference.Both,
            },
        };
        _context.TutorProfiles.AddRange(profiles);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Expands CSCA from 5 sections to 8 by splitting Math/Physics/Chemistry into EN and CN variants.
    /// Also creates CN skills, topics, and updates MockExamSection references.
    /// Safe to call multiple times — skips if already expanded.
    /// </summary>
    private async Task ExpandCscaTo8SectionsAsync()
    {
        var sections = await _context.ExamSections
            .Where(s => s.ExamTypeCode == "CSCA")
            .ToListAsync();

        if (!sections.Any()) return;

        // 1. Rename existing EN sections (idempotent — only renames if old name found)
        var renameMap = new Dictionary<string, string>
        {
            ["Mathematics"] = "Mathematics (EN)",
            ["Physics"] = "Physics (EN)",
            ["Chemistry"] = "Chemistry (EN)",
            ["Chinese Technical"] = "中文技术 (Chinese Technical)",
            ["Chinese Humanitarian"] = "中文人文 (Chinese Humanitarian)",
        };
        var legacyRenameMap = new Dictionary<string, string>
        {
            ["数学 (Mathematics)"] = "Mathematics (EN)",
            ["物理 (Physics)"] = "Physics (EN)",
            ["化学 (Chemistry)"] = "Chemistry (EN)",
            ["Математика (Mathematics)"] = "Mathematics (EN)",
            ["Физика (Physics)"] = "Physics (EN)",
            ["Химия (Chemistry)"] = "Chemistry (EN)",
            ["Китайский технический (Chinese Technical)"] = "中文技术 (Chinese Technical)",
            ["Китайский гуманитарный (Chinese Humanitarian)"] = "中文人文 (Chinese Humanitarian)",
        };

        bool renamed = false;
        foreach (var section in sections)
        {
            if (renameMap.TryGetValue(section.Name, out var newName))
            { section.Name = newName; renamed = true; }
            else if (legacyRenameMap.TryGetValue(section.Name, out var legacyName))
            { section.Name = legacyName; renamed = true; }
        }
        if (renamed) await _context.SaveChangesAsync();

        // 2. Ensure Chinese Technical and Chinese Humanitarian exist (always checked, even on re-runs)
        bool added = false;
        if (!sections.Any(s => s.Name.Contains("Chinese Technical")))
        {
            var cnTech = new ExamSection { ExamTypeCode = "CSCA", Name = "中文技术 (Chinese Technical)", MinScore = 0, MaxScore = 100 };
            _context.ExamSections.Add(cnTech);
            sections.Add(cnTech);
            added = true;
        }
        if (!sections.Any(s => s.Name.Contains("Chinese Humanitarian")))
        {
            var cnHum = new ExamSection { ExamTypeCode = "CSCA", Name = "中文人文 (Chinese Humanitarian)", MinScore = 0, MaxScore = 100 };
            _context.ExamSections.Add(cnHum);
            sections.Add(cnHum);
            added = true;
        }
        if (added)
        {
            await _context.SaveChangesAsync();
            // Reload sections to include newly created ones
            sections = await _context.ExamSections
                .Where(s => s.ExamTypeCode == "CSCA")
                .ToListAsync();
        }

        // 2b. Rename EN skill names to include (EN) suffix (idempotent)
        var skillRenameMap = new Dictionary<string, string>
        {
            ["Математика (Mathematics)"] = "Математика EN (Mathematics EN)",
            ["Физика (Physics)"] = "Физика EN (Physics EN)",
            ["Химия (Chemistry)"] = "Химия EN (Chemistry EN)",
        };
        var enSkills = await _context.Skills
            .Where(s => s.Code == "SK_MATH" || s.Code == "SK_PHYS" || s.Code == "SK_CHEM")
            .ToListAsync();
        bool skillRenamed = false;
        foreach (var sk in enSkills)
        {
            if (skillRenameMap.TryGetValue(sk.Name, out var newSkillName))
            { sk.Name = newSkillName; skillRenamed = true; }
        }
        if (skillRenamed) await _context.SaveChangesAsync();

        // 3. Create 3 CN ExamSections (Math/Phys/Chem) if not yet created
        var alreadyExpanded = await _context.ExamSections.AnyAsync(s => s.ExamTypeCode == "CSCA" && s.Name == "Mathematics (CN)");

        ExamSection? mathCn, physCn, chemCn;
        if (!alreadyExpanded)
        {
            mathCn = new ExamSection { ExamTypeCode = "CSCA", Name = "Mathematics (CN)", MinScore = 0, MaxScore = 100 };
            physCn = new ExamSection { ExamTypeCode = "CSCA", Name = "Physics (CN)", MinScore = 0, MaxScore = 100 };
            chemCn = new ExamSection { ExamTypeCode = "CSCA", Name = "Chemistry (CN)", MinScore = 0, MaxScore = 100 };
            _context.ExamSections.AddRange(mathCn, physCn, chemCn);
            await _context.SaveChangesAsync();
        }
        else
        {
            mathCn = sections.FirstOrDefault(s => s.Name == "Mathematics (CN)");
            physCn = sections.FirstOrDefault(s => s.Name == "Physics (CN)");
            chemCn = sections.FirstOrDefault(s => s.Name == "Chemistry (CN)");
        }

        // 4. Create CN skills if they don't exist
        if (!await _context.Skills.AnyAsync(s => s.Code == "SK_MATH_CN"))
        {
            _context.Skills.AddRange(
                new Skill { Code = "SK_MATH_CN", Name = "Математика CN (Mathematics CN)", Description = "Алгебра, геометрия, анализ данных — на китайском языке" },
                new Skill { Code = "SK_PHYS_CN", Name = "Физика CN (Physics CN)", Description = "Механика, электричество, оптика, термодинамика — на китайском языке" },
                new Skill { Code = "SK_CHEM_CN", Name = "Химия CN (Chemistry CN)", Description = "Неорганическая, органическая, аналитическая химия — на китайском языке" }
            );
            await _context.SaveChangesAsync();
        }

        var skMathCn = await _context.Skills.FirstAsync(s => s.Code == "SK_MATH_CN");
        var skPhysCn = await _context.Skills.FirstAsync(s => s.Code == "SK_PHYS_CN");
        var skChemCn = await _context.Skills.FirstAsync(s => s.Code == "SK_CHEM_CN");

        // 5. Create CN topics (mirroring EN chapter structure)
        if (mathCn != null && !await _context.Topics.AnyAsync(t => t.SectionId == mathCn.Id))
        {
            var cnTopics = new List<Topic>
            {
                // Mathematics (CN) — основные разделы
                T("CN-M1 集合 (Sets)", skMathCn.Id, mathCn.Id),
                T("CN-M2 函数 (Functions)", skMathCn.Id, mathCn.Id),
                T("CN-M3 指数与对数 (Exponents & Logarithms)", skMathCn.Id, mathCn.Id),
                T("CN-M4 三角函数 (Trigonometry)", skMathCn.Id, mathCn.Id),
                T("CN-M5 平面向量 (Plane Vectors)", skMathCn.Id, mathCn.Id),
                T("CN-M6 数列 (Sequences & Series)", skMathCn.Id, mathCn.Id),
                T("CN-M7 不等式 (Inequalities)", skMathCn.Id, mathCn.Id),
                T("CN-M8 立体几何 (Solid Geometry)", skMathCn.Id, mathCn.Id),
                T("CN-M9 解析几何 (Analytic Geometry)", skMathCn.Id, mathCn.Id),
                T("CN-M10 概率与统计 (Probability & Statistics)", skMathCn.Id, mathCn.Id),

                // Physics (CN) — основные разделы
                T("CN-P1 力学 (Mechanics)", skPhysCn.Id, physCn!.Id),
                T("CN-P2 运动学 (Kinematics)", skPhysCn.Id, physCn.Id),
                T("CN-P3 牛顿定律 (Newton's Laws)", skPhysCn.Id, physCn.Id),
                T("CN-P4 功和能 (Work & Energy)", skPhysCn.Id, physCn.Id),
                T("CN-P5 电场 (Electric Fields)", skPhysCn.Id, physCn.Id),
                T("CN-P6 电路 (Circuits)", skPhysCn.Id, physCn.Id),
                T("CN-P7 磁场 (Magnetic Fields)", skPhysCn.Id, physCn.Id),
                T("CN-P8 电磁感应 (Electromagnetic Induction)", skPhysCn.Id, physCn.Id),
                T("CN-P9 光学 (Optics)", skPhysCn.Id, physCn.Id),
                T("CN-P10 热学 (Thermodynamics)", skPhysCn.Id, physCn.Id),

                // Chemistry (CN) — основные разделы
                T("CN-C1 原子结构 (Atomic Structure)", skChemCn.Id, chemCn!.Id),
                T("CN-C2 化学反应 (Chemical Reactions)", skChemCn.Id, chemCn.Id),
                T("CN-C3 化学计量 (Stoichiometry)", skChemCn.Id, chemCn.Id),
                T("CN-C4 碱金属 (Alkali Metals)", skChemCn.Id, chemCn.Id),
                T("CN-C5 卤素 (Halogens)", skChemCn.Id, chemCn.Id),
                T("CN-C6 金属及化合物 (Metals & Compounds)", skChemCn.Id, chemCn.Id),
                T("CN-C7 非金属及化合物 (Non-metals & Compounds)", skChemCn.Id, chemCn.Id),
                T("CN-C8 有机化学基础 (Organic Chemistry)", skChemCn.Id, chemCn.Id),
                T("CN-C9 化学平衡 (Chemical Equilibrium)", skChemCn.Id, chemCn.Id),
                T("CN-C10 电化学 (Electrochemistry)", skChemCn.Id, chemCn.Id),
            };
            await _context.Topics.AddRangeAsync(cnTopics);
            await _context.SaveChangesAsync();
        }

        // 6. Ensure Chinese Technical / Humanitarian sections have topics
        var cnTechSection = sections.FirstOrDefault(s => s.Name.Contains("Chinese Technical"));
        var cnHumSection = sections.FirstOrDefault(s => s.Name.Contains("Chinese Humanitarian"));
        var skCnTech = await _context.Skills.FirstOrDefaultAsync(s => s.Code == "SK_CN_TECH");
        var skCnHum = await _context.Skills.FirstOrDefaultAsync(s => s.Code == "SK_CN_HUM");

        if (cnTechSection != null && skCnTech != null
            && !await _context.Topics.AnyAsync(t => t.SectionId == cnTechSection.Id))
        {
            var cnTechTopics = new List<Topic>
            {
                T("CT1.1.1 Mathematics Terminology (数学术语)", skCnTech.Id, cnTechSection.Id),
                T("CT1.1.2 Physics Terminology (物理术语)", skCnTech.Id, cnTechSection.Id),
                T("CT1.1.3 Chemistry Terminology (化学术语)", skCnTech.Id, cnTechSection.Id),
                T("CT1.2.1 Units and Measurements in Chinese (单位与测量)", skCnTech.Id, cnTechSection.Id),
                T("CT2.1.1 Reading Formulas and Equations (公式阅读)", skCnTech.Id, cnTechSection.Id),
                T("CT2.1.2 Understanding Graphs and Tables (图表理解)", skCnTech.Id, cnTechSection.Id),
                T("CT2.2.1 Scientific Article Structure (科技文章结构)", skCnTech.Id, cnTechSection.Id),
                T("CT2.2.2 Abstract and Conclusion Comprehension (摘要与结论)", skCnTech.Id, cnTechSection.Id),
                T("CT3.1.1 Computer Science Terms (计算机科学)", skCnTech.Id, cnTechSection.Id),
                T("CT3.1.2 Engineering Vocabulary (工程术语)", skCnTech.Id, cnTechSection.Id),
            };
            await _context.Topics.AddRangeAsync(cnTechTopics);
            await _context.SaveChangesAsync();
        }

        if (cnHumSection != null && skCnHum != null
            && !await _context.Topics.AnyAsync(t => t.SectionId == cnHumSection.Id))
        {
            var cnHumTopics = new List<Topic>
            {
                T("CH1.1.1 Classical Chinese Poetry (古诗词)", skCnHum.Id, cnHumSection.Id),
                T("CH1.1.2 Modern Chinese Literature (现代文学)", skCnHum.Id, cnHumSection.Id),
                T("CH1.2.1 Idioms and Proverbs (成语与谚语)", skCnHum.Id, cnHumSection.Id),
                T("CH2.1.1 Ancient Chinese History (古代史)", skCnHum.Id, cnHumSection.Id),
                T("CH2.1.2 Modern Chinese History (近现代史)", skCnHum.Id, cnHumSection.Id),
                T("CH3.1.1 Confucianism (儒家思想)", skCnHum.Id, cnHumSection.Id),
                T("CH3.1.2 Taoism and Buddhism (道家与佛教)", skCnHum.Id, cnHumSection.Id),
                T("CH4.1.1 Traditional Art Forms (传统艺术)", skCnHum.Id, cnHumSection.Id),
                T("CH4.1.2 Festivals and Customs (节日与风俗)", skCnHum.Id, cnHumSection.Id),
                T("CH5.1.1 Chinese Education System (教育体系)", skCnHum.Id, cnHumSection.Id),
            };
            await _context.Topics.AddRangeAsync(cnHumTopics);
            await _context.SaveChangesAsync();
        }

        // 7. Update MockExamSection ExamSectionIds for CN variants
        var cscaMockSections = await _context.MockExamSections
            .Where(ms => ms.MockExam.ExamTypeCode == "CSCA")
            .ToListAsync();

        foreach (var ms in cscaMockSections)
        {
            if (ms.Name == "Mathematics (CN)" && mathCn != null)
                ms.ExamSectionId = mathCn.Id;
            else if (ms.Name == "Physics (CN)" && physCn != null)
                ms.ExamSectionId = physCn.Id;
            else if (ms.Name == "Chemistry (CN)" && chemCn != null)
                ms.ExamSectionId = chemCn.Id;
            else if (ms.Name.Contains("Chinese Technical") && cnTechSection != null)
                ms.ExamSectionId = cnTechSection.Id;
            else if (ms.Name.Contains("Chinese Humanitarian") && cnHumSection != null)
                ms.ExamSectionId = cnHumSection.Id;
        }

        // 8. Also rename MockExamSection names for Chinese sections if still old names
        foreach (var ms in cscaMockSections)
        {
            if (ms.Name == "Chinese Technical")
                ms.Name = "中文技术 (Chinese Technical)";
            else if (ms.Name == "Chinese Humanitarian")
                ms.Name = "中文人文 (Chinese Humanitarian)";
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedDrillTemplatesAsync()
    {
        var templates = new[]
        {
            new DrillTemplate
            {
                Title = "Speed Round",
                Description = "Answer 10 questions as fast as you can",
                DrillType = DrillType.Speed,
                QuestionCount = 10,
                IsActive = true,
                SortOrder = 1,
            },
            new DrillTemplate
            {
                Title = "Marathon",
                Description = "Answer as many questions as possible in 3 minutes",
                DrillType = DrillType.Marathon,
                QuestionCount = 100,
                TimeLimitMinutes = 3,
                IsActive = true,
                SortOrder = 2,
            },
            new DrillTemplate
            {
                Title = "Streak Challenge",
                Description = "Keep answering correctly until you miss",
                DrillType = DrillType.Streak,
                QuestionCount = 999,
                IsActive = true,
                SortOrder = 3,
            },
        };

        await _context.DrillTemplates.AddRangeAsync(templates);
        await _context.SaveChangesAsync();
    }
}
