using UniStart.Data;
using UniStart.Models;

namespace UniStart.Seeders
{
    public static class AchievementSeeder
    {
        public static async Task SeedAchievementsAsync(ApplicationDbContext context)
        {
            // Проверяем, есть ли уже достижения
            if (context.Achievements.Any())
            {
                Console.WriteLine("✅ Достижения уже существуют");
                return;
            }

            var achievements = new List<Achievement>
            {
                // Достижения за квизы
                new Achievement
                {
                    Title = "Первый шаг",
                    Description = "Пройдите свой первый квиз",
                    Icon = "🎯",
                    Type = "QuizCompletion",
                    TargetValue = 1,
                    Level = 1
                },
                new Achievement
                {
                    Title = "Ученик",
                    Description = "Пройдите 10 квизов",
                    Icon = "📚",
                    Type = "QuizCompletion",
                    TargetValue = 10,
                    Level = 2
                },
                new Achievement
                {
                    Title = "Знаток",
                    Description = "Пройдите 50 квизов",
                    Icon = "🎓",
                    Type = "QuizCompletion",
                    TargetValue = 50,
                    Level = 3
                },
                new Achievement
                {
                    Title = "Эксперт",
                    Description = "Пройдите 100 квизов",
                    Icon = "⭐",
                    Type = "QuizCompletion",
                    TargetValue = 100,
                    Level = 4
                },
                new Achievement
                {
                    Title = "Мастер",
                    Description = "Пройдите 500 квизов",
                    Icon = "👑",
                    Type = "QuizCompletion",
                    TargetValue = 500,
                    Level = 5
                },

                // Достижения за оценки
                new Achievement
                {
                    Title = "Отличник",
                    Description = "Наберите средний балл 90%+",
                    Icon = "💯",
                    Type = "AverageScore",
                    TargetValue = 90,
                    Level = 3
                },
                new Achievement
                {
                    Title = "Перфекционист",
                    Description = "Получите 100% на любом квизе",
                    Icon = "✨",
                    Type = "PerfectScore",
                    TargetValue = 100,
                    Level = 2
                },

                // Достижения за карточки
                new Achievement
                {
                    Title = "Создатель",
                    Description = "Создайте 5 наборов карточек",
                    Icon = "🎨",
                    Type = "FlashcardSetCreation",
                    TargetValue = 5,
                    Level = 2
                },
                new Achievement
                {
                    Title = "Коллекционер",
                    Description = "Создайте 100 карточек",
                    Icon = "🗂️",
                    Type = "FlashcardCreation",
                    TargetValue = 100,
                    Level = 3
                },
                new Achievement
                {
                    Title = "Архивариус",
                    Description = "Создайте 20 наборов карточек",
                    Icon = "📖",
                    Type = "FlashcardSetCreation",
                    TargetValue = 20,
                    Level = 4
                },

                // Достижения за стримы
                new Achievement
                {
                    Title = "Постоянство",
                    Description = "Занимайтесь 7 дней подряд",
                    Icon = "🔥",
                    Type = "Streak",
                    TargetValue = 7,
                    Level = 2
                },
                new Achievement
                {
                    Title = "Дисциплина",
                    Description = "Занимайтесь 30 дней подряд",
                    Icon = "💪",
                    Type = "Streak",
                    TargetValue = 30,
                    Level = 3
                },
                new Achievement
                {
                    Title = "Легенда",
                    Description = "Занимайтесь 100 дней подряд",
                    Icon = "🏆",
                    Type = "Streak",
                    TargetValue = 100,
                    Level = 5
                },

                // Социальные достижения
                new Achievement
                {
                    Title = "Популярный",
                    Description = "Получите 10 подписчиков",
                    Icon = "👥",
                    Type = "Followers",
                    TargetValue = 10,
                    Level = 2
                },
                new Achievement
                {
                    Title = "Звезда",
                    Description = "Получите 50 подписчиков",
                    Icon = "🌟",
                    Type = "Followers",
                    TargetValue = 50,
                    Level = 3
                },
                new Achievement
                {
                    Title = "Помощник",
                    Description = "Оставьте 10 отзывов",
                    Icon = "💬",
                    Type = "Reviews",
                    TargetValue = 10,
                    Level = 2
                }
            };

            context.Achievements.AddRange(achievements);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ Создано {achievements.Count} достижений");
        }
    }
}
