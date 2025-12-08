using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models;

namespace UniStart.Seeders
{
    /// <summary>
    /// Класс для наполнения базы данных тестовыми данными
    /// </summary>
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            // Не создаём тестовые данные
            Console.WriteLine("DatabaseSeeder: тестовые данные отключены.");
            await Task.CompletedTask;
        }

        private static async Task<ApplicationUser> CreateTestUserAsync(UserManager<ApplicationUser> userManager)
        {
            var user = new ApplicationUser
            {
                UserName = "test@unistart.kz",
                Email = "test@unistart.kz",
                FirstName = "Айдар",
                LastName = "Тестов",
                EmailConfirmed = true,
                TotalCardsStudied = 0,
                TotalQuizzesTaken = 0,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, "Test123!");
            
            if (result.Succeeded)
            {
                Console.WriteLine($"✅ Создан тестовый пользователь: {user.Email}");
                return user;
            }

            throw new Exception($"Ошибка создания пользователя: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        private static async Task CreateFlashcardSetsAsync(ApplicationDbContext context, string userId)
        {
            // Набор 1: Математика - Алгебра
            var mathSet = new FlashcardSet
            {
                Title = "Алгебра - Основы",
                Description = "Базовые формулы и понятия алгебры для подготовки к ЕНТ",
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Flashcards = new List<Flashcard>
                {
                    new Flashcard
                    {
                        Question = "Формула квадрата суммы",
                        Answer = "(a + b)² = a² + 2ab + b²",
                        Explanation = "Квадрат суммы двух чисел равен квадрату первого числа плюс удвоенное произведение первого числа на второе плюс квадрат второго числа.",
                        OrderIndex = 0
                    },
                    new Flashcard
                    {
                        Question = "Формула квадрата разности",
                        Answer = "(a - b)² = a² - 2ab + b²",
                        Explanation = "Квадрат разности двух чисел равен квадрату первого числа минус удвоенное произведение первого числа на второе плюс квадрат второго числа.",
                        OrderIndex = 1
                    },
                    new Flashcard
                    {
                        Question = "Формула разности квадратов",
                        Answer = "a² - b² = (a - b)(a + b)",
                        Explanation = "Разность квадратов двух чисел равна произведению разности этих чисел на их сумму.",
                        OrderIndex = 2
                    },
                    new Flashcard
                    {
                        Question = "Теорема Виета для квадратного уравнения",
                        Answer = "x₁ + x₂ = -b/a; x₁ · x₂ = c/a",
                        Explanation = "Для уравнения ax² + bx + c = 0: сумма корней равна -b/a, произведение корней равно c/a.",
                        OrderIndex = 3
                    },
                    new Flashcard
                    {
                        Question = "Формула дискриминанта",
                        Answer = "D = b² - 4ac",
                        Explanation = "Дискриминант показывает количество корней квадратного уравнения: D > 0 (два корня), D = 0 (один корень), D < 0 (нет действительных корней).",
                        OrderIndex = 4
                    }
                }
            };

            // Набор 2: Физика - Механика
            var physicsSet = new FlashcardSet
            {
                Title = "Физика - Механика",
                Description = "Основные формулы кинематики и динамики",
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Flashcards = new List<Flashcard>
                {
                    new Flashcard
                    {
                        Question = "Формула скорости при равномерном движении",
                        Answer = "v = S / t",
                        Explanation = "Скорость равна отношению пройденного пути ко времени движения.",
                        OrderIndex = 0
                    },
                    new Flashcard
                    {
                        Question = "Формула ускорения",
                        Answer = "a = (v - v₀) / t",
                        Explanation = "Ускорение равно изменению скорости, деленному на время.",
                        OrderIndex = 1
                    },
                    new Flashcard
                    {
                        Question = "Второй закон Ньютона",
                        Answer = "F = ma",
                        Explanation = "Сила равна произведению массы тела на его ускорение.",
                        OrderIndex = 2
                    },
                    new Flashcard
                    {
                        Question = "Формула кинетической энергии",
                        Answer = "E = mv² / 2",
                        Explanation = "Кинетическая энергия равна половине произведения массы на квадрат скорости.",
                        OrderIndex = 3
                    }
                }
            };

            // Набор 3: История Казахстана
            var historySet = new FlashcardSet
            {
                Title = "История Казахстана - Ключевые даты",
                Description = "Важные события в истории Казахстана",
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Flashcards = new List<Flashcard>
                {
                    new Flashcard
                    {
                        Question = "Когда был образован Казахстанский ханат?",
                        Answer = "1465 год",
                        Explanation = "Казахское ханство было основано султанами Жанибеком и Кереем в 1465 году.",
                        OrderIndex = 0
                    },
                    new Flashcard
                    {
                        Question = "Когда Казахстан получил независимость?",
                        Answer = "16 декабря 1991 года",
                        Explanation = "16 декабря 1991 года Верховный Совет принял Конституционный закон о государственной независимости Республики Казахстан.",
                        OrderIndex = 1
                    },
                    new Flashcard
                    {
                        Question = "Кто был первым президентом Казахстана?",
                        Answer = "Нурсултан Назарбаев",
                        Explanation = "Нурсултан Абишевич Назарбаев был первым и единственным президентом Казахстана с 1991 по 2019 год.",
                        OrderIndex = 2
                    }
                }
            };

            // Набор 4: Английский язык
            var englishSet = new FlashcardSet
            {
                Title = "English - Irregular Verbs",
                Description = "Неправильные глаголы английского языка",
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Flashcards = new List<Flashcard>
                {
                    new Flashcard
                    {
                        Question = "go (идти) - три формы",
                        Answer = "go - went - gone",
                        Explanation = "Настоящее - Прошедшее - Причастие прошедшего времени",
                        OrderIndex = 0
                    },
                    new Flashcard
                    {
                        Question = "write (писать) - три формы",
                        Answer = "write - wrote - written",
                        Explanation = "Настоящее - Прошедшее - Причастие прошедшего времени",
                        OrderIndex = 1
                    },
                    new Flashcard
                    {
                        Question = "take (брать) - три формы",
                        Answer = "take - took - taken",
                        Explanation = "Настоящее - Прошедшее - Причастие прошедшего времени",
                        OrderIndex = 2
                    },
                    new Flashcard
                    {
                        Question = "see (видеть) - три формы",
                        Answer = "see - saw - seen",
                        Explanation = "Настоящее - Прошедшее - Причастие прошедшего времени",
                        OrderIndex = 3
                    }
                }
            };

            context.FlashcardSets.AddRange(mathSet, physicsSet, historySet, englishSet);
            await context.SaveChangesAsync();
            
            Console.WriteLine($"✅ Создано {4} наборов карточек с общим количеством {mathSet.Flashcards.Count + physicsSet.Flashcards.Count + historySet.Flashcards.Count + englishSet.Flashcards.Count} карточек");
        }

        private static async Task CreateQuizzesAsync(ApplicationDbContext context, string userId)
        {
            // Тест 1: Математика - Квадратные уравнения
            var mathQuiz = new Quiz
            {
                Title = "Квадратные уравнения",
                Description = "Проверьте свои знания по теме квадратных уравнений",
                TimeLimit = 15, // 15 минут
                Subject = "Математика",
                Difficulty = "Medium",
                IsPublished = true,
                UserId = userId,
                Questions = new List<QuizQuestion>
                {
                    new QuizQuestion
                    {
                        Text = "Решите уравнение: x² - 5x + 6 = 0",
                        Points = 2,
                        OrderIndex = 0,
                        Explanation = "Используя теорему Виета: x₁ + x₂ = 5, x₁ · x₂ = 6. Корни: x₁ = 2, x₂ = 3",
                        Answers = new List<QuizAnswer>
                        {
                            new QuizAnswer { Text = "x₁ = 2, x₂ = 3", IsCorrect = true, OrderIndex = 0 },
                            new QuizAnswer { Text = "x₁ = 1, x₂ = 6", IsCorrect = false, OrderIndex = 1 },
                            new QuizAnswer { Text = "x₁ = -2, x₂ = -3", IsCorrect = false, OrderIndex = 2 },
                            new QuizAnswer { Text = "x₁ = 5, x₂ = 1", IsCorrect = false, OrderIndex = 3 }
                        }
                    },
                    new QuizQuestion
                    {
                        Text = "Чему равен дискриминант уравнения x² + 4x + 4 = 0?",
                        Points = 1,
                        OrderIndex = 1,
                        Explanation = "D = b² - 4ac = 16 - 16 = 0. При D = 0 уравнение имеет один корень.",
                        Answers = new List<QuizAnswer>
                        {
                            new QuizAnswer { Text = "0", IsCorrect = true, OrderIndex = 0 },
                            new QuizAnswer { Text = "4", IsCorrect = false, OrderIndex = 1 },
                            new QuizAnswer { Text = "16", IsCorrect = false, OrderIndex = 2 },
                            new QuizAnswer { Text = "-4", IsCorrect = false, OrderIndex = 3 }
                        }
                    },
                    new QuizQuestion
                    {
                        Text = "Какие из следующих утверждений верны? (Выберите все правильные)",
                        Points = 3,
                        OrderIndex = 2,
                        Explanation = "Правильные утверждения: квадратное уравнение может иметь два корня, и сумма корней равна -b/a.",
                        Answers = new List<QuizAnswer>
                        {
                            new QuizAnswer { Text = "Квадратное уравнение всегда имеет два корня", IsCorrect = false, OrderIndex = 0 },
                            new QuizAnswer { Text = "Квадратное уравнение может иметь два корня", IsCorrect = true, OrderIndex = 1 },
                            new QuizAnswer { Text = "Сумма корней квадратного уравнения равна -b/a", IsCorrect = true, OrderIndex = 2 },
                            new QuizAnswer { Text = "Дискриминант всегда положителен", IsCorrect = false, OrderIndex = 3 }
                        }
                    }
                }
            };

            // Тест 2: Физика - Механика
            var physicsQuiz = new Quiz
            {
                Title = "Основы механики",
                Description = "Тест по кинематике и динамике",
                TimeLimit = 20,
                Subject = "Физика",
                Difficulty = "Easy",
                IsPublished = true,
                UserId = userId,
                Questions = new List<QuizQuestion>
                {
                    new QuizQuestion
                    {
                        Text = "Автомобиль проехал 120 км за 2 часа. Какова его средняя скорость?",
                        Points = 1,
                        OrderIndex = 0,
                        Explanation = "v = S / t = 120 км / 2 ч = 60 км/ч",
                        Answers = new List<QuizAnswer>
                        {
                            new QuizAnswer { Text = "60 км/ч", IsCorrect = true, OrderIndex = 0 },
                            new QuizAnswer { Text = "120 км/ч", IsCorrect = false, OrderIndex = 1 },
                            new QuizAnswer { Text = "240 км/ч", IsCorrect = false, OrderIndex = 2 },
                            new QuizAnswer { Text = "30 км/ч", IsCorrect = false, OrderIndex = 3 }
                        }
                    },
                    new QuizQuestion
                    {
                        Text = "Второй закон Ньютона формулируется как:",
                        Points = 1,
                        OrderIndex = 1,
                        Explanation = "F = ma - сила равна произведению массы на ускорение",
                        Answers = new List<QuizAnswer>
                        {
                            new QuizAnswer { Text = "F = ma", IsCorrect = true, OrderIndex = 0 },
                            new QuizAnswer { Text = "F = mv", IsCorrect = false, OrderIndex = 1 },
                            new QuizAnswer { Text = "F = m/a", IsCorrect = false, OrderIndex = 2 },
                            new QuizAnswer { Text = "F = a/m", IsCorrect = false, OrderIndex = 3 }
                        }
                    }
                }
            };

            // Тест 3: История Казахстана
            var historyQuiz = new Quiz
            {
                Title = "История Казахстана - Важные даты",
                Description = "Проверьте знание ключевых исторических событий",
                TimeLimit = 10,
                Subject = "История",
                Difficulty = "Easy",
                IsPublished = true,
                UserId = userId,
                Questions = new List<QuizQuestion>
                {
                    new QuizQuestion
                    {
                        Text = "В каком году Казахстан получил независимость?",
                        Points = 1,
                        OrderIndex = 0,
                        Explanation = "16 декабря 1991 года была провозглашена независимость Республики Казахстан",
                        Answers = new List<QuizAnswer>
                        {
                            new QuizAnswer { Text = "1991", IsCorrect = true, OrderIndex = 0 },
                            new QuizAnswer { Text = "1990", IsCorrect = false, OrderIndex = 1 },
                            new QuizAnswer { Text = "1992", IsCorrect = false, OrderIndex = 2 },
                            new QuizAnswer { Text = "1989", IsCorrect = false, OrderIndex = 3 }
                        }
                    },
                    new QuizQuestion
                    {
                        Text = "Когда был образован Казахский ханат?",
                        Points = 2,
                        OrderIndex = 1,
                        Explanation = "Казахский ханат был образован в 1465 году Жанибеком и Кереем",
                        Answers = new List<QuizAnswer>
                        {
                            new QuizAnswer { Text = "1465", IsCorrect = true, OrderIndex = 0 },
                            new QuizAnswer { Text = "1456", IsCorrect = false, OrderIndex = 1 },
                            new QuizAnswer { Text = "1500", IsCorrect = false, OrderIndex = 2 },
                            new QuizAnswer { Text = "1400", IsCorrect = false, OrderIndex = 3 }
                        }
                    }
                }
            };

            context.Quizzes.AddRange(mathQuiz, physicsQuiz, historyQuiz);
            await context.SaveChangesAsync();
            
            Console.WriteLine($"✅ Создано {3} теста с общим количеством {mathQuiz.Questions.Count + physicsQuiz.Questions.Count + historyQuiz.Questions.Count} вопросов");
        }
    }
}
