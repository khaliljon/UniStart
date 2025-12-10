using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models;

namespace UniStart.Seeders
{
    /// <summary>
    /// Главный класс для наполнения базы данных начальными данными
    /// </summary>
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // 1. Создаём роли и администратора (ПЕРВЫМ!)
            await SeedRolesAndAdminAsync(roleManager, userManager);

            // 2. Создаём предметы (включая ЕНТ)
            await CreateSubjectsAsync(context);

            // 3. Создаём достижения
            await SeedAchievementsAsync(context);

            // 4. Создаём международные данные (страны, типы экзаменов, университеты)
            await SeedInternationalDataAsync(context);
        }

        #region Роли и пользователи

        private static async Task SeedRolesAndAdminAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            // Создание ролей, если их нет
            string[] roles = { UserRoles.Admin, UserRoles.Teacher, UserRoles.Student };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"✅ Роль '{role}' создана");
                }
            }

            // Создание администратора по умолчанию
            var adminEmail = "admin@unistart.kz";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Администратор",
                    LastName = "Системы",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                    Console.WriteLine($"✅ Администратор создан: {adminEmail} / Admin123!");
                }
                else
                {
                    Console.WriteLine($"❌ Ошибка создания администратора: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Убедимся, что у существующего админа есть роль
                if (!await userManager.IsInRoleAsync(adminUser, UserRoles.Admin))
                {
                    await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                    Console.WriteLine($"✅ Роль Admin добавлена пользователю {adminEmail}");
                }
            }

            // Назначаем роль Student тестовому пользователю
            var testUser = await userManager.FindByEmailAsync("test@unistart.kz");
            if (testUser != null && !await userManager.IsInRoleAsync(testUser, UserRoles.Student))
            {
                await userManager.AddToRoleAsync(testUser, UserRoles.Student);
                Console.WriteLine($"✅ Роль Student добавлена тестовому пользователю");
            }

            // Создаём тестового преподавателя
            var teacherEmail = "teacher@unistart.kz";
            var teacherUser = await userManager.FindByEmailAsync(teacherEmail);

            if (teacherUser == null)
            {
                teacherUser = new ApplicationUser
                {
                    UserName = teacherEmail,
                    Email = teacherEmail,
                    FirstName = "Иван",
                    LastName = "Преподавателев",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(teacherUser, "Teacher123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(teacherUser, UserRoles.Teacher);
                    Console.WriteLine($"✅ Преподаватель создан: {teacherEmail} / Teacher123!");
                }
                else
                {
                    Console.WriteLine($"❌ Ошибка создания преподавателя: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(teacherUser, UserRoles.Teacher))
                {
                    await userManager.AddToRoleAsync(teacherUser, UserRoles.Teacher);
                    Console.WriteLine($"✅ Роль Teacher добавлена пользователю {teacherEmail}");
                }
            }

            // Создаём тестового студента
            var studentEmail = "student@unistart.kz";
            var studentUser = await userManager.FindByEmailAsync(studentEmail);

            if (studentUser == null)
            {
                studentUser = new ApplicationUser
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    FirstName = "Алия",
                    LastName = "Студентова",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(studentUser, "Student123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(studentUser, UserRoles.Student);
                    Console.WriteLine($"✅ Студент создан: {studentEmail} / Student123!");
                }
                else
                {
                    Console.WriteLine($"❌ Ошибка создания студента: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(studentUser, UserRoles.Student))
                {
                    await userManager.AddToRoleAsync(studentUser, UserRoles.Student);
                    Console.WriteLine($"✅ Роль Student добавлена пользователю {studentEmail}");
                }
            }
        }

        #endregion

        #region Предметы

        private static async Task CreateSubjectsAsync(ApplicationDbContext context)
        {
            var subjects = new List<Subject>
            {
                // Обязательные предметы ЕНТ
                new Subject
                {
                    Name = "Грамотность чтения",
                    Description = "Тестирование навыков понимания и анализа текста, работы с информацией",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Subject
                {
                    Name = "Математическая грамотность",
                    Description = "Базовые математические навыки, решение практических задач",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Subject
                {
                    Name = "История Казахстана",
                    Description = "История Казахстана с древнейших времен до наших дней",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                
                // Профильные предметы ЕНТ
                new Subject
                {
                    Name = "Математика",
                    Description = "Алгебра, геометрия, тригонометрия, основы математического анализа",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Subject
                {
                    Name = "Физика",
                    Description = "Механика, термодинамика, электричество, оптика, атомная физика",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Subject
                {
                    Name = "Химия",
                    Description = "Общая химия, органическая химия, неорганическая химия",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Subject
                {
                    Name = "Биология",
                    Description = "Ботаника, зоология, анатомия, генетика, экология",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Subject
                {
                    Name = "География",
                    Description = "Физическая география, экономическая география, география Казахстана",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Subject
                {
                    Name = "Всемирная история",
                    Description = "История древнего мира, средних веков, нового и новейшего времени",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Subject
                {
                    Name = "Английский язык",
                    Description = "Грамматика, лексика, чтение, аудирование, письмо",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Subject
                {
                    Name = "Казахский язык",
                    Description = "Грамматика, литература, письмо, аудирование",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Subject
                {
                    Name = "Русский язык",
                    Description = "Грамматика, литература, письмо, аудирование",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Subject
                {
                    Name = "Информатика",
                    Description = "Программирование, алгоритмы, базы данных, компьютерные сети",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Subject
                {
                    Name = "Казахская литература",
                    Description = "Произведения казахских писателей и поэтов",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Subject
                {
                    Name = "Русская литература",
                    Description = "Произведения русских писателей и поэтов",
                    IsActive = true,
                CreatedAt = DateTime.UtcNow
                }
            };

            // Проверяем, какие предметы уже существуют
            var existingSubjects = await context.Subjects
                .Where(s => subjects.Select(sub => sub.Name).Contains(s.Name))
                .Select(s => s.Name)
                .ToListAsync();

            var newSubjects = subjects.Where(s => !existingSubjects.Contains(s.Name)).ToList();

            if (newSubjects.Any())
            {
                context.Subjects.AddRange(newSubjects);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Создано {newSubjects.Count} предметов (всего в базе: {await context.Subjects.CountAsync()})");
            }
            else
            {
                Console.WriteLine($"ℹ️ Все предметы уже существуют в базе (всего: {await context.Subjects.CountAsync()})");
            }
        }

        #endregion

        #region Достижения

        private static async Task SeedAchievementsAsync(ApplicationDbContext context)
        {
            // Проверяем, есть ли уже достижения
            if (context.Achievements.Any())
            {
                Console.WriteLine("ℹ️ Достижения уже существуют");
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

        #endregion

        #region Международные данные

        private static async Task SeedInternationalDataAsync(ApplicationDbContext context)
        {
            // Страны
            if (!context.Countries.Any())
            {
                var countries = new List<Country>
                {
                    new Country
                    {
                        Name = "Казахстан",
                        NameEn = "Kazakhstan",
                        Code = "KZ",
                        FlagEmoji = "🇰🇿",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Country
                    {
                        Name = "Россия",
                        NameEn = "Russia",
                        Code = "RU",
                        FlagEmoji = "🇷🇺",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Country
                    {
                        Name = "Китай",
                        NameEn = "China",
                        Code = "CN",
                        FlagEmoji = "🇨🇳",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Country
                    {
                        Name = "США",
                        NameEn = "United States",
                        Code = "US",
                        FlagEmoji = "🇺🇸",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Country
                    {
                        Name = "Великобритания",
                        NameEn = "United Kingdom",
                        Code = "GB",
                        FlagEmoji = "🇬🇧",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Country
                    {
                        Name = "Германия",
                        NameEn = "Germany",
                        Code = "DE",
                        FlagEmoji = "🇩🇪",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Country
                    {
                        Name = "Франция",
                        NameEn = "France",
                        Code = "FR",
                        FlagEmoji = "🇫🇷",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Country
                    {
                        Name = "Канада",
                        NameEn = "Canada",
                        Code = "CA",
                        FlagEmoji = "🇨🇦",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Country
                    {
                        Name = "Австралия",
                        NameEn = "Australia",
                        Code = "AU",
                        FlagEmoji = "🇦🇺",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Country
                    {
                        Name = "Южная Корея",
                        NameEn = "South Korea",
                        Code = "KR",
                        FlagEmoji = "🇰🇷",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Countries.AddRangeAsync(countries);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Создано {countries.Count} стран");
            }

            // Типы экзаменов
            if (!context.ExamTypes.Any())
            {
                var kazakhstan = await context.Countries.FirstOrDefaultAsync(c => c.Code == "KZ");
                var russia = await context.Countries.FirstOrDefaultAsync(c => c.Code == "RU");
                var china = await context.Countries.FirstOrDefaultAsync(c => c.Code == "CN");
                var usa = await context.Countries.FirstOrDefaultAsync(c => c.Code == "US");

                var examTypes = new List<ExamType>
                {
                    new ExamType
                    {
                        Name = "ЕНТ (Единое Национальное Тестирование)",
                        NameEn = "UNT (Unified National Testing)",
                        Code = "ENT",
                        Description = "Единое национальное тестирование для поступления в вузы Казахстана",
                        DefaultCountryId = kazakhstan?.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ExamType
                    {
                        Name = "ЕГЭ (Единый Государственный Экзамен)",
                        NameEn = "USE (Unified State Exam)",
                        Code = "EGE",
                        Description = "Единый государственный экзамен в России",
                        DefaultCountryId = russia?.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ExamType
                    {
                        Name = "Gaokao (高考)",
                        NameEn = "Gaokao",
                        Code = "GAOKAO",
                        Description = "Национальный вступительный экзамен в высшие учебные заведения Китая",
                        DefaultCountryId = china?.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ExamType
                    {
                        Name = "SAT",
                        NameEn = "SAT (Scholastic Assessment Test)",
                        Code = "SAT",
                        Description = "Стандартизированный тест для поступления в колледжи США",
                        DefaultCountryId = usa?.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ExamType
                    {
                        Name = "ACT",
                        NameEn = "ACT (American College Testing)",
                        Code = "ACT",
                        Description = "Альтернативный стандартизированный тест для поступления в США",
                        DefaultCountryId = usa?.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ExamType
                    {
                        Name = "IELTS",
                        NameEn = "IELTS (International English Language Testing System)",
                        Code = "IELTS",
                        Description = "Международная система тестирования по английскому языку",
                        DefaultCountryId = null,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ExamType
                    {
                        Name = "TOEFL",
                        NameEn = "TOEFL (Test of English as a Foreign Language)",
                        Code = "TOEFL",
                        Description = "Тест на знание английского языка как иностранного",
                        DefaultCountryId = null,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ExamType
                    {
                        Name = "A-Level",
                        NameEn = "A-Level (Advanced Level)",
                        Code = "ALEVEL",
                        Description = "Британская квалификация для поступления в университеты",
                        DefaultCountryId = null,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ExamType
                    {
                        Name = "IB",
                        NameEn = "IB (International Baccalaureate)",
                        Code = "IB",
                        Description = "Международный бакалавриат",
                        DefaultCountryId = null,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.ExamTypes.AddRangeAsync(examTypes);
            await context.SaveChangesAsync();
                Console.WriteLine($"✅ Создано {examTypes.Count} типов экзаменов");
            }

            // Университеты
            if (!context.Universities.Any())
            {
                var kazakhstan = await context.Countries.FirstOrDefaultAsync(c => c.Code == "KZ");
                var russia = await context.Countries.FirstOrDefaultAsync(c => c.Code == "RU");
                var usa = await context.Countries.FirstOrDefaultAsync(c => c.Code == "US");
                var uk = await context.Countries.FirstOrDefaultAsync(c => c.Code == "GB");
                var china = await context.Countries.FirstOrDefaultAsync(c => c.Code == "CN");

                var universities = new List<University>();

                // Казахстанские университеты
                if (kazakhstan != null)
                {
                    universities.AddRange(new[]
                    {
                        new University
                        {
                            Name = "Назарбаев Университет",
                            NameEn = "Nazarbayev University",
                            City = "Астана",
                            Description = "Первый автономный исследовательский университет Казахстана",
                            Website = "https://nu.edu.kz",
                            Type = UniversityType.International,
                            CountryId = kazakhstan.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new University
                        {
                            Name = "Казахский Национальный Университет имени аль-Фараби",
                            NameEn = "Al-Farabi Kazakh National University",
                            City = "Алматы",
                            Description = "Крупнейший классический университет Казахстана",
                            Website = "https://www.kaznu.kz",
                            Type = UniversityType.Public,
                            CountryId = kazakhstan.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new University
                        {
                            Name = "Казахстанско-Британский Технический Университет",
                            NameEn = "Kazakh-British Technical University",
                            City = "Алматы",
                            Description = "Технический университет с британскими образовательными стандартами",
                            Website = "https://kbtu.edu.kz",
                            Type = UniversityType.Private,
                            CountryId = kazakhstan.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new University
                        {
                            Name = "Евразийский Национальный Университет имени Л.Н. Гумилева",
                            NameEn = "L.N. Gumilyov Eurasian National University",
                            City = "Астана",
                            Description = "Один из ведущих университетов Казахстана",
                            Website = "https://www.enu.kz",
                            Type = UniversityType.Public,
                            CountryId = kazakhstan.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new University
                        {
                            Name = "Международный Университет Информационных Технологий",
                            NameEn = "International IT University",
                            City = "Алматы",
                            Description = "Специализированный IT-университет",
                            Website = "https://iitu.edu.kz",
                            Type = UniversityType.Private,
                            CountryId = kazakhstan.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }
                    });
                }

                // Российские университеты
                if (russia != null)
                {
                    universities.AddRange(new[]
                    {
                        new University
                        {
                            Name = "Московский Государственный Университет имени М.В. Ломоносова",
                            NameEn = "Lomonosov Moscow State University",
                            City = "Москва",
                            Description = "Старейший и крупнейший классический университет России",
                            Website = "https://www.msu.ru",
                            Type = UniversityType.Public,
                            CountryId = russia.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new University
                        {
                            Name = "Санкт-Петербургский Государственный Университет",
                            NameEn = "Saint Petersburg State University",
                            City = "Санкт-Петербург",
                            Description = "Один из старейших университетов России",
                            Website = "https://spbu.ru",
                            Type = UniversityType.Public,
                            CountryId = russia.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }
                    });
                }

                // Американские университеты
                if (usa != null)
                {
                    universities.AddRange(new[]
                    {
                        new University
                        {
                            Name = "Гарвардский Университет",
                            NameEn = "Harvard University",
                            City = "Cambridge, MA",
                            Description = "Старейший университет США, входит в Лигу Плюща",
                            Website = "https://www.harvard.edu",
                            Type = UniversityType.Private,
                            CountryId = usa.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new University
                        {
                            Name = "Массачусетский Технологический Институт",
                            NameEn = "Massachusetts Institute of Technology",
                            City = "Cambridge, MA",
                            Description = "Ведущий технический университет мира",
                            Website = "https://www.mit.edu",
                            Type = UniversityType.Private,
                            CountryId = usa.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }
                    });
                }

                // Британские университеты
                if (uk != null)
                {
                    universities.AddRange(new[]
                    {
                        new University
                        {
                            Name = "Оксфордский Университет",
                            NameEn = "University of Oxford",
                            City = "Oxford",
                            Description = "Старейший университет англоязычного мира",
                            Website = "https://www.ox.ac.uk",
                            Type = UniversityType.Public,
                            CountryId = uk.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new University
                        {
                            Name = "Кембриджский Университет",
                            NameEn = "University of Cambridge",
                            City = "Cambridge",
                            Description = "Один из старейших и престижнейших университетов мира",
                            Website = "https://www.cam.ac.uk",
                            Type = UniversityType.Public,
                            CountryId = uk.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }
                    });
                }

                // Китайские университеты
                if (china != null)
                {
                    universities.AddRange(new[]
                    {
                        new University
                        {
                            Name = "Университет Цинхуа",
                            NameEn = "Tsinghua University",
                            City = "Пекин",
                            Description = "Ведущий технический университет Китая",
                            Website = "https://www.tsinghua.edu.cn",
                            Type = UniversityType.Public,
                            CountryId = china.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }
                    });
                }

                if (universities.Any())
                {
                    await context.Universities.AddRangeAsync(universities);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ Создано {universities.Count} университетов");
                }
            }

            // Связь университетов с типами экзаменов - выполняется всегда
            var allUniversities = await context.Universities.Include(u => u.ExamTypes).ToListAsync();
            var allExamTypes = await context.ExamTypes.ToListAsync();

            if (allUniversities.Any() && allExamTypes.Any())
            {
                var examTypesDict = allExamTypes.ToDictionary(et => et.Code ?? "", et => et);

                foreach (var university in allUniversities)
                {
                    university.ExamTypes.Clear();

                    // Присваиваем типы на основе страны и университета
                    switch (university.NameEn)
                    {
                        // Казахстан
                        case "Nazarbayev University":
                            AddExamTypesIfExist(university, examTypesDict, "ENT", "IELTS", "TOEFL", "SAT");
                            break;
                        case "Al-Farabi Kazakh National University":
                            AddExamTypesIfExist(university, examTypesDict, "ENT", "IELTS");
                            break;
                        case "Kazakh-British Technical University":
                            AddExamTypesIfExist(university, examTypesDict, "ENT", "IELTS", "TOEFL");
                            break;
                        case "L.N. Gumilyov Eurasian National University":
                            AddExamTypesIfExist(university, examTypesDict, "ENT");
                            break;
                        case "International IT University":
                            AddExamTypesIfExist(university, examTypesDict, "ENT", "IELTS");
                            break;

                        // Россия
                        case "Lomonosov Moscow State University":
                            AddExamTypesIfExist(university, examTypesDict, "EGE", "IELTS", "TOEFL");
                            break;
                        case "Saint Petersburg State University":
                            AddExamTypesIfExist(university, examTypesDict, "EGE", "IELTS");
                            break;

                        // США
                        case "Harvard University":
                        case "Massachusetts Institute of Technology":
                            AddExamTypesIfExist(university, examTypesDict, "SAT", "ACT", "TOEFL", "IELTS");
                            break;

                        // Великобритания
                        case "University of Oxford":
                        case "University of Cambridge":
                            AddExamTypesIfExist(university, examTypesDict, "ALEVEL", "IELTS", "TOEFL");
                            break;

                        // Китай
                        case "Tsinghua University":
                            AddExamTypesIfExist(university, examTypesDict, "GAOKAO", "IELTS", "TOEFL");
                            break;
                    }
                }

            await context.SaveChangesAsync();
                Console.WriteLine("✅ Связи университетов с типами экзаменов обновлены");
            }
        }

        private static void AddExamTypesIfExist(University university, Dictionary<string, ExamType> examTypesDict, params string[] codes)
        {
            foreach (var code in codes)
            {
                if (examTypesDict.TryGetValue(code, out var examType))
                {
                    university.ExamTypes.Add(examType);
                }
            }
        }

        #endregion
    }
}
