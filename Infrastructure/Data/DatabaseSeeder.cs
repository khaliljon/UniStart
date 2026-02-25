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
}
