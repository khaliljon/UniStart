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

        // Seed Skills
        if (!await _context.Skills.AnyAsync())
        {
            await SeedSkillsAsync();
        }

        // Seed Topics
        if (!await _context.Topics.AnyAsync())
        {
            await SeedTopicsAsync();
        }

        // Seed Questions with AnswerOptions
        if (!await _context.Questions.AnyAsync())
        {
            await SeedQuestionsAsync();
        }

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

        if (!await _context.FlashcardDecks.AnyAsync())
        {
            await SeedFlashcardDecksAsync();
        }

        if (!await _context.StrategyGuides.AnyAsync())
        {
            await SeedStrategyGuidesAsync();
        }

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
            new ExamType { Code = "NUET", Name = "NUET (Математика + Критическое мышление)" }
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
            new ExamSection { ExamTypeCode = "NUET", Name = "Critical Thinking", MinScore = 0, MaxScore = 140 }
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
            new Skill { Code = "SK_MATH", Name = "Математика (Mathematics)", Description = "Алгебра, геометрия, анализ данных" },
            new Skill { Code = "SK_CRIT", Name = "Критическое мышление (Critical Thinking)", Description = "Логика, анализ аргументов, решение проблем" }
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

        var skillRead = await _context.Skills.FirstAsync(s => s.Code == "SK_READ");
        var skillWrite = await _context.Skills.FirstAsync(s => s.Code == "SK_WRITE");
        var skillListen = await _context.Skills.FirstAsync(s => s.Code == "SK_LISTEN");
        var skillSpeak = await _context.Skills.FirstAsync(s => s.Code == "SK_SPEAK");
        var skillMath = await _context.Skills.FirstAsync(s => s.Code == "SK_MATH");
        var skillCrit = await _context.Skills.FirstAsync(s => s.Code == "SK_CRIT");

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
            new Topic { Name = "Argument Analysis", SkillId = skillCrit.Id, SectionId = nuetCritical.Id }
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
              "https://www.youtube.com/watch?v=YK6MU_H2msg"),

            L("Main Idea & Summary", 2,
              "Summarising a Passage",
              "## How to Summarise\n\nA good summary captures the **key points** without adding personal opinion.\n\n### Steps\n1. Identify the **thesis** (main claim).\n2. Note **supporting points** — usually one per paragraph.\n3. Combine them in 1–2 sentences.\n\n### Tips\n- Use your **own words** — avoid copying phrases.\n- A correct summary should work as a replacement for the passage."),

            L("Grammar & Sentence Structure", 1,
              "Subject-Verb Agreement",
              "## Subject-Verb Agreement\n\nThe verb must agree in number with its subject.\n\n### Rules\n| Subject | Verb |\n|---------|------|\n| Singular (The dog) | runs |\n| Plural (The dogs) | run |\n\n### Tricky Cases\n- **Prepositional phrases**: \"The box *of chocolates* **is** heavy.\" (subject = box)\n- **Compound subjects**: \"Tom **and** Jerry **are** friends.\"\n- **Either/or**: \"Either the cats **or** the dog **is** sleeping.\" (verb matches nearest subject)\n\n### Quick Check\nCross out words between subject and verb to test agreement.",
              "https://www.youtube.com/watch?v=14fXm4FOMPM"),

            L("Grammar & Sentence Structure", 2,
              "Sentence Fragments & Run-ons",
              "## Sentence Fragments\n\nA fragment is missing a **subject**, **verb**, or **complete thought**.\n\n❌ *Because it was raining.* (dependent clause alone)\n✅ *We stayed inside because it was raining.*\n\n## Run-on Sentences\n\nTwo independent clauses joined without proper punctuation.\n\n❌ *I love reading I go to the library every week.*\n\n### How to Fix\n1. **Period**: I love reading. I go to the library every week.\n2. **Semicolon**: I love reading; I go to the library every week.\n3. **Conjunction**: I love reading, so I go to the library every week."),

            L("Vocabulary in Context", 1,
              "Determining Word Meaning from Context",
              "## Context Clues Strategy\n\nWhen an unfamiliar word appears in a passage, the surrounding words help reveal its meaning.\n\n### Types of Context Clues\n1. **Definition clue**: The word is directly defined. *\"Ubiquitous, meaning everywhere, ...\"*\n2. **Synonym clue**: A similar word is nearby. *\"She was elated — truly joyful.\"*\n3. **Antonym clue**: An opposite word provides contrast. *\"Unlike his timid brother, Jake was audacious.\"*\n4. **Example clue**: Examples illustrate the meaning.\n\n### Technique\n- Substitute each answer choice into the sentence.\n- Choose the one that maintains the **tone** and **logic** of the passage.",
              "https://www.youtube.com/watch?v=CnlBahcJCeg"),

            // ──── SAT Math ─────────────────────────────────────
            L("Linear Equations", 1,
              "Solving Linear Equations",
              "## Linear Equations\n\nA linear equation has the form **ax + b = c** where the variable has exponent 1.\n\n### Solving Steps\n1. **Simplify** both sides (combine like terms).\n2. **Isolate** the variable using inverse operations.\n3. **Check** by substituting back.\n\n### Example\n$$2x + 5 = 13$$\n$$2x = 8$$\n$$x = 4$$\n\n### Slope-Intercept Form\n$$y = mx + b$$\n- **m** = slope (rise/run)\n- **b** = y-intercept",
              "https://www.youtube.com/watch?v=GmMX3-nTWbE"),

            L("Linear Equations", 2,
              "Systems of Linear Equations",
              "## Systems of Equations\n\nTwo or more equations with the same variables.\n\n### Methods\n1. **Substitution**: Solve one equation for a variable, substitute into the other.\n2. **Elimination**: Add/subtract equations to eliminate a variable.\n3. **Graphing**: The solution is where lines intersect.\n\n### Example (Elimination)\n$$x + y = 10$$\n$$x - y = 4$$\n\nAdd both: $2x = 14 \\Rightarrow x = 7$, then $y = 3$.\n\n### No Solution vs. Infinite Solutions\n- **Parallel lines** (same slope, different intercept): no solution.\n- **Same line**: infinitely many solutions."),

            L("Geometry", 1,
              "Angles, Triangles & Circles",
              "## Key Geometry Facts\n\n### Angles\n- Supplementary: $a + b = 180°$\n- Complementary: $a + b = 90°$\n- Vertical angles are equal.\n\n### Triangles\n- Interior angles sum to **180°**.\n- **Pythagorean theorem** (right triangle): $a^2 + b^2 = c^2$\n- Area = $\\frac{1}{2} \\times base \\times height$\n\n### Circles\n- Area = $\\pi r^2$\n- Circumference = $2\\pi r$\n- Arc length = $\\frac{\\theta}{360} \\times 2\\pi r$",
              "https://www.youtube.com/watch?v=mLeNaj2A9_0"),

            L("Data Analysis", 1,
              "Mean, Median, Mode & Graphs",
              "## Central Tendency\n\n| Measure | Formula |\n|---------|---------|\n| Mean | Sum of values / count |\n| Median | Middle value when sorted |\n| Mode | Most frequent value |\n\n### Reading Graphs\n- **Bar chart**: compare categories.\n- **Line graph**: trends over time.\n- **Scatter plot**: correlation between two variables.\n\n### Tips\n- Watch for **outliers** — they pull the mean but not the median.\n- If the question says \"average,\" it usually means **mean**.",
              "https://www.youtube.com/watch?v=kn83BA7cRNM"),

            L("Quadratic Equations", 1,
              "Solving Quadratic Equations",
              "## Quadratic Form\n\n$$ax^2 + bx + c = 0$$\n\n### Three Methods\n1. **Factoring**: $(x - 2)(x + 3) = 0 \\Rightarrow x = 2$ or $x = -3$\n2. **Quadratic Formula**: $x = \\frac{-b \\pm \\sqrt{b^2 - 4ac}}{2a}$\n3. **Completing the square**\n\n### Discriminant $\\Delta = b^2 - 4ac$\n- $\\Delta > 0$: two real solutions\n- $\\Delta = 0$: one real solution (vertex touches x-axis)\n- $\\Delta < 0$: no real solutions\n\n### Vertex Form\n$$y = a(x - h)^2 + k$$\nVertex at $(h, k)$.",
              "https://www.youtube.com/watch?v=IlNAJl36-10"),

            // ──── TOEFL ────────────────────────────────────────
            L("Academic Reading", 1,
              "Strategies for Academic Passages",
              "## TOEFL Reading Overview\n\nYou'll encounter 3–4 academic passages (~700 words each) on science, history, or social topics.\n\n### Reading Strategy\n1. **Skim first** — read the title, first sentence of each paragraph.\n2. **Note the structure**: compare/contrast, cause/effect, chronological.\n3. **Read questions first** to know what to look for.\n\n### Question Types\n- **Factual**: stated directly in the passage.\n- **Inference**: implied but not stated.\n- **Vocabulary**: meaning from context.\n- **Summary/Insert**: overall organization.",
              "https://www.youtube.com/watch?v=PRZepFNXnJk"),

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
              "https://www.youtube.com/watch?v=SkMNREVMgJo"),

            L("Problem Solving", 1,
              "Problem-Solving Strategies",
              "## NUET Problem Solving\n\n### General Approach\n1. **Read carefully** — identify what is asked.\n2. **Choose a method**: algebraic, numerical, or diagrammatic.\n3. **Estimate first** — eliminate clearly wrong answers.\n4. **Work backwards** from answer choices if stuck.\n\n### Common Patterns\n- **Rate × Time = Distance**\n- **Percentage**: part/whole × 100\n- **Ratio & Proportion**: cross-multiply to solve.\n\n### Tips\n- Draw diagrams for geometry problems.\n- Convert units early (meters to cm, etc.).\n- Check your answer against the question's constraints."),

            L("Logical Reasoning", 1,
              "Logical Arguments & Fallacies",
              "## NUET Logical Reasoning\n\n### Argument Structure\n- **Premise**: a statement assumed to be true.\n- **Conclusion**: what follows from the premises.\n\n### Validity vs. Truth\n- An argument can be **logically valid** even if premises are false.\n- A **sound** argument is valid AND has true premises.\n\n### Common Fallacies\n| Fallacy | Description |\n|---------|-------------|\n| Ad hominem | Attacking the person, not the argument |\n| Straw man | Misrepresenting the opponent's position |\n| False dilemma | Presenting only two options when more exist |\n| Circular reasoning | Conclusion restates the premise |",
              "https://www.youtube.com/watch?v=q3KSVfkPs7c"),

            L("Argument Analysis", 1,
              "Evaluating Arguments",
              "## NUET Argument Analysis\n\n### What to Look For\n1. **Identify the conclusion** — what is being argued?\n2. **Find the evidence** — what supports it?\n3. **Assess the gap** — is the logic strong?\n\n### Strengthen / Weaken Questions\n- To **strengthen**: find evidence that supports the conclusion.\n- To **weaken**: find evidence that challenges the link between premise and conclusion.\n\n### Assumption Questions\nAn assumption is an **unstated premise** the argument depends on.\n- Ask: \"If this were NOT true, would the argument fall apart?\"\n- If yes → it's a necessary assumption.")
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
                        SortOrder = 0,
                        Instructions = "This section measures your ability to comprehend, analyze, and use information and ideas presented in texts. Read each passage and question carefully, then select the best answer. You may refer back to the passage as often as needed. Time limit: 15 minutes."
                    },
                    new MockExamSection
                    {
                        ExamSectionId = satMathNC?.Id,
                        Name = "Math (No Calculator)",
                        TimeLimitMinutes = 15,
                        SortOrder = 1,
                        Instructions = "This section tests your mathematical reasoning without the use of a calculator. You must show your understanding of concepts and perform calculations by hand. Focus on accuracy and efficient problem-solving. Time limit: 15 minutes."
                    },
                    new MockExamSection
                    {
                        ExamSectionId = satMathC?.Id,
                        Name = "Math (Calculator)",
                        TimeLimitMinutes = 15,
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
                        SortOrder = 0,
                        Instructions = "Read the academic passages carefully and answer the questions that follow. Each passage is followed by a set of questions. You can navigate between questions within this section and change your answers. Time limit: 18 minutes."
                    },
                    new MockExamSection
                    {
                        ExamSectionId = toeflListening?.Id,
                        Name = "Listening",
                        TimeLimitMinutes = 12,
                        SortOrder = 1,
                        Instructions = "Answer questions about academic lectures and campus conversations. In a real TOEFL test, you would listen to audio recordings. In this practice version, you will read transcribed excerpts. Time limit: 12 minutes."
                    },
                    new MockExamSection
                    {
                        ExamSectionId = toeflSpeaking?.Id,
                        Name = "Speaking",
                        TimeLimitMinutes = 10,
                        SortOrder = 2,
                        Instructions = "Answer questions that test your ability to speak about familiar topics and synthesize information. In this practice version, select the best response option. Time limit: 10 minutes."
                    },
                    new MockExamSection
                    {
                        ExamSectionId = toeflWriting?.Id,
                        Name = "Writing",
                        TimeLimitMinutes = 10,
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
                        SortOrder = 0,
                        Instructions = "This section tests your mathematical knowledge and problem-solving skills. Topics include algebra, functions, geometry, and applied mathematics. Work through each problem carefully. Time limit: 25 minutes."
                    },
                    new MockExamSection
                    {
                        ExamSectionId = nuetCritical?.Id,
                        Name = "Critical Thinking",
                        TimeLimitMinutes = 25,
                        SortOrder = 1,
                        Instructions = "This section tests your logical reasoning and argument analysis skills. You will evaluate arguments, identify assumptions, draw conclusions, and analyze reasoning patterns. Time limit: 25 minutes."
                    }
                }
            }
        };

        await _context.MockExams.AddRangeAsync(mockExams);
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
                Headline = "Эксперт ЕГЭ по математике и физике",
                Bio = "Преподаю математику и физику более 10 лет. Мои ученики стабильно набирают 90+ баллов на ЕГЭ. Индивидуальный подход к каждому студенту.",
                Experience = "Кандидат физико-математических наук. 12 лет преподавания в МФТИ. Автор учебных пособий по подготовке к ЕГЭ.",
                Specializations = "EGE,OGE",
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
        };

        await _context.StrategyGuides.AddRangeAsync(guides);
        await _context.SaveChangesAsync();
    }
}
