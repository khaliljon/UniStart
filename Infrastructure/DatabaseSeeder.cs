using Microsoft.EntityFrameworkCore;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Infrastructure;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(UniStartDbContext context)
    {
        // Seed Exam Sections
        if (!await context.ExamSections.AnyAsync())
        {
            var sections = new List<ExamSection>
            {
                // SAT Sections
                new() { ExamTypeCode = "SAT", Name = "Reading", MinScore = 200, MaxScore = 800 },
                new() { ExamTypeCode = "SAT", Name = "Writing and Language", MinScore = 200, MaxScore = 800 },
                new() { ExamTypeCode = "SAT", Name = "Math (No Calculator)", MinScore = 200, MaxScore = 800 },
                new() { ExamTypeCode = "SAT", Name = "Math (Calculator)", MinScore = 200, MaxScore = 800 },
                
                // TOEFL Sections
                new() { ExamTypeCode = "TOEFL", Name = "Reading", MinScore = 0, MaxScore = 30 },
                new() { ExamTypeCode = "TOEFL", Name = "Listening", MinScore = 0, MaxScore = 30 },
                new() { ExamTypeCode = "TOEFL", Name = "Speaking", MinScore = 0, MaxScore = 30 },
                new() { ExamTypeCode = "TOEFL", Name = "Writing", MinScore = 0, MaxScore = 30 },
                
                // NUET Sections
                new() { ExamTypeCode = "NUET", Name = "Critical Thinking", MinScore = 0, MaxScore = 100 },
                new() { ExamTypeCode = "NUET", Name = "Quantitative Reasoning", MinScore = 0, MaxScore = 100 },
                new() { ExamTypeCode = "NUET", Name = "English Proficiency", MinScore = 0, MaxScore = 100 }
            };
            
            await context.ExamSections.AddRangeAsync(sections);
            await context.SaveChangesAsync();
        }

        // Seed Skills
        if (!await context.Skills.AnyAsync())
        {
            var skills = new List<Skill>
            {
                new() { Code = "READING_COMP", Name = "Reading Comprehension", Description = "Understanding and analyzing written text" },
                new() { Code = "VOCABULARY", Name = "Vocabulary", Description = "Word knowledge and usage" },
                new() { Code = "GRAMMAR", Name = "Grammar", Description = "Understanding of English grammar rules" },
                new() { Code = "ALGEBRA", Name = "Algebra", Description = "Algebraic equations and expressions" },
                new() { Code = "GEOMETRY", Name = "Geometry", Description = "Geometric concepts and calculations" },
                new() { Code = "DATA_ANALYSIS", Name = "Data Analysis", Description = "Interpreting data and statistics" },
                new() { Code = "CRITICAL_THINK", Name = "Critical Thinking", Description = "Logical reasoning and analysis" },
                new() { Code = "LISTENING", Name = "Listening Comprehension", Description = "Understanding spoken English" }
            };
            
            await context.Skills.AddRangeAsync(skills);
            await context.SaveChangesAsync();
        }

        // Seed Topics (connecting skills to sections)
        if (!await context.Topics.AnyAsync())
        {
            var readingSection = await context.ExamSections.FirstOrDefaultAsync(s => s.Name == "Reading" && s.ExamTypeCode == "SAT");
            var mathSection = await context.ExamSections.FirstOrDefaultAsync(s => s.Name == "Math (Calculator)" && s.ExamTypeCode == "SAT");
            var writingSection = await context.ExamSections.FirstOrDefaultAsync(s => s.Name == "Writing and Language" && s.ExamTypeCode == "SAT");
            
            var readingSkill = await context.Skills.FirstOrDefaultAsync(s => s.Code == "READING_COMP");
            var algebraSkill = await context.Skills.FirstOrDefaultAsync(s => s.Code == "ALGEBRA");
            var grammarSkill = await context.Skills.FirstOrDefaultAsync(s => s.Code == "GRAMMAR");

            if (readingSection != null && mathSection != null && writingSection != null &&
                readingSkill != null && algebraSkill != null && grammarSkill != null)
            {
                var topics = new List<Topic>
                {
                    new() { SkillId = readingSkill.Id, SectionId = readingSection.Id, Name = "Main Idea and Central Theme" },
                    new() { SkillId = readingSkill.Id, SectionId = readingSection.Id, Name = "Supporting Details" },
                    new() { SkillId = readingSkill.Id, SectionId = readingSection.Id, Name = "Inference and Conclusions" },
                    new() { SkillId = algebraSkill.Id, SectionId = mathSection.Id, Name = "Linear Equations" },
                    new() { SkillId = algebraSkill.Id, SectionId = mathSection.Id, Name = "Quadratic Equations" },
                    new() { SkillId = algebraSkill.Id, SectionId = mathSection.Id, Name = "Systems of Equations" },
                    new() { SkillId = grammarSkill.Id, SectionId = writingSection.Id, Name = "Subject-Verb Agreement" },
                    new() { SkillId = grammarSkill.Id, SectionId = writingSection.Id, Name = "Punctuation" },
                    new() { SkillId = grammarSkill.Id, SectionId = writingSection.Id, Name = "Sentence Structure" }
                };
                
                await context.Topics.AddRangeAsync(topics);
                await context.SaveChangesAsync();
            }
        }

        // Seed sample Questions
        if (!await context.Questions.AnyAsync())
        {
            var topics = await context.Topics.ToListAsync();
            
            foreach (var topic in topics.Take(3)) // Add questions for first 3 topics
            {
                var questions = CreateSampleQuestionsForTopic(topic);
                await context.Questions.AddRangeAsync(questions);
            }
            
            await context.SaveChangesAsync();
        }
    }

    private static List<Question> CreateSampleQuestionsForTopic(Topic topic)
    {
        var questions = new List<Question>();
        
        if (topic.Name.Contains("Linear"))
        {
            questions.Add(new Question
            {
                TopicId = topic.Id,
                Text = "Solve for x: 2x + 5 = 13",
                Difficulty = QuestionDifficulty.Easy,
                AnswerOptions = new List<AnswerOption>
                {
                    new() { Text = "x = 4", IsCorrect = true },
                    new() { Text = "x = 5", IsCorrect = false },
                    new() { Text = "x = 3", IsCorrect = false },
                    new() { Text = "x = 6", IsCorrect = false }
                }
            });
            
            questions.Add(new Question
            {
                TopicId = topic.Id,
                Text = "Solve for x: 3(x - 2) = 2x + 7",
                Difficulty = QuestionDifficulty.Medium,
                AnswerOptions = new List<AnswerOption>
                {
                    new() { Text = "x = 13", IsCorrect = true },
                    new() { Text = "x = 11", IsCorrect = false },
                    new() { Text = "x = 7", IsCorrect = false },
                    new() { Text = "x = 1", IsCorrect = false }
                }
            });
            
            questions.Add(new Question
            {
                TopicId = topic.Id,
                Text = "If 5x - 3 = 2(x + 6), what is the value of x?",
                Difficulty = QuestionDifficulty.Hard,
                AnswerOptions = new List<AnswerOption>
                {
                    new() { Text = "x = 5", IsCorrect = true },
                    new() { Text = "x = 3", IsCorrect = false },
                    new() { Text = "x = 7", IsCorrect = false },
                    new() { Text = "x = 9", IsCorrect = false }
                }
            });
        }
        else if (topic.Name.Contains("Main Idea"))
        {
            questions.Add(new Question
            {
                TopicId = topic.Id,
                Text = "What is the primary purpose of identifying the main idea in a passage?",
                Difficulty = QuestionDifficulty.Easy,
                AnswerOptions = new List<AnswerOption>
                {
                    new() { Text = "To understand the central message the author wants to convey", IsCorrect = true },
                    new() { Text = "To count the number of paragraphs", IsCorrect = false },
                    new() { Text = "To identify all the characters", IsCorrect = false },
                    new() { Text = "To memorize specific dates", IsCorrect = false }
                }
            });
            
            questions.Add(new Question
            {
                TopicId = topic.Id,
                Text = "Which sentence best represents the main idea: 'Climate change is affecting ecosystems worldwide. Rising temperatures cause ice caps to melt. Sea levels are increasing. Wildlife habitats are being destroyed.'",
                Difficulty = QuestionDifficulty.Medium,
                AnswerOptions = new List<AnswerOption>
                {
                    new() { Text = "Climate change is affecting ecosystems worldwide", IsCorrect = true },
                    new() { Text = "Rising temperatures cause ice caps to melt", IsCorrect = false },
                    new() { Text = "Sea levels are increasing", IsCorrect = false },
                    new() { Text = "Wildlife habitats are being destroyed", IsCorrect = false }
                }
            });
        }
        else if (topic.Name.Contains("Subject-Verb"))
        {
            questions.Add(new Question
            {
                TopicId = topic.Id,
                Text = "Choose the correct sentence:",
                Difficulty = QuestionDifficulty.Easy,
                AnswerOptions = new List<AnswerOption>
                {
                    new() { Text = "The team plays well together.", IsCorrect = true },
                    new() { Text = "The team play well together.", IsCorrect = false },
                    new() { Text = "The team are play well together.", IsCorrect = false },
                    new() { Text = "The team playing well together.", IsCorrect = false }
                }
            });
            
            questions.Add(new Question
            {
                TopicId = topic.Id,
                Text = "Select the grammatically correct option: 'Neither the students nor the teacher _____ ready for the exam.'",
                Difficulty = QuestionDifficulty.Medium,
                AnswerOptions = new List<AnswerOption>
                {
                    new() { Text = "was", IsCorrect = true },
                    new() { Text = "were", IsCorrect = false },
                    new() { Text = "are", IsCorrect = false },
                    new() { Text = "been", IsCorrect = false }
                }
            });
        }
        
        return questions;
    }
}
