using Microsoft.EntityFrameworkCore;
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
        _context.Topics.RemoveRange(_context.Topics);
        _context.ExamSections.RemoveRange(_context.ExamSections);
        _context.Skills.RemoveRange(_context.Skills);
        _context.ExamTypes.RemoveRange(_context.ExamTypes);
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
}
