using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models;

namespace UniStart.Seeders
{
    public static class InternationalSeeder
    {
        public static async Task SeedInternationalData(ApplicationDbContext context)
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
                        case "Moscow State University":
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
                            AddExamTypesIfExist(university, examTypesDict, "A-LEVEL", "IELTS", "TOEFL");
                            break;

                        // Китай
                        case "Tsinghua University":
                            AddExamTypesIfExist(university, examTypesDict, "GAOKAO", "IELTS", "TOEFL");
                            break;
                    }
                }

                await context.SaveChangesAsync();
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
    }
}
