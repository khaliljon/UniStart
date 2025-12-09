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

            // Связь университетов с типами экзаменов
            if (context.Universities.Any() && context.ExamTypes.Any())
            {
                // Загружаем типы экзаменов
                var ent = await context.ExamTypes.FirstOrDefaultAsync(et => et.Code == "ENT");
                var ege = await context.ExamTypes.FirstOrDefaultAsync(et => et.Code == "EGE");
                var sat = await context.ExamTypes.FirstOrDefaultAsync(et => et.Code == "SAT");
                var act = await context.ExamTypes.FirstOrDefaultAsync(et => et.Code == "ACT");
                var ielts = await context.ExamTypes.FirstOrDefaultAsync(et => et.Code == "IELTS");
                var toefl = await context.ExamTypes.FirstOrDefaultAsync(et => et.Code == "TOEFL");
                var aLevel = await context.ExamTypes.FirstOrDefaultAsync(et => et.Code == "A-LEVEL");
                var gaokao = await context.ExamTypes.FirstOrDefaultAsync(et => et.Code == "GAOKAO");

                // Загружаем университеты
                var nazarbayev = await context.Universities
                    .Include(u => u.ExamTypes)
                    .FirstOrDefaultAsync(u => u.NameEn == "Nazarbayev University");
                var kaznu = await context.Universities
                    .Include(u => u.ExamTypes)
                    .FirstOrDefaultAsync(u => u.NameEn == "Al-Farabi Kazakh National University");
                var kbtu = await context.Universities
                    .Include(u => u.ExamTypes)
                    .FirstOrDefaultAsync(u => u.NameEn == "Kazakh-British Technical University");
                var enu = await context.Universities
                    .Include(u => u.ExamTypes)
                    .FirstOrDefaultAsync(u => u.NameEn == "L.N. Gumilyov Eurasian National University");
                var iitu = await context.Universities
                    .Include(u => u.ExamTypes)
                    .FirstOrDefaultAsync(u => u.NameEn == "International IT University");
                
                var msu = await context.Universities
                    .Include(u => u.ExamTypes)
                    .FirstOrDefaultAsync(u => u.NameEn == "Moscow State University");
                var spbu = await context.Universities
                    .Include(u => u.ExamTypes)
                    .FirstOrDefaultAsync(u => u.NameEn == "Saint Petersburg State University");
                
                var harvard = await context.Universities
                    .Include(u => u.ExamTypes)
                    .FirstOrDefaultAsync(u => u.NameEn == "Harvard University");
                var mit = await context.Universities
                    .Include(u => u.ExamTypes)
                    .FirstOrDefaultAsync(u => u.NameEn == "Massachusetts Institute of Technology");
                
                var oxford = await context.Universities
                    .Include(u => u.ExamTypes)
                    .FirstOrDefaultAsync(u => u.NameEn == "University of Oxford");
                var cambridge = await context.Universities
                    .Include(u => u.ExamTypes)
                    .FirstOrDefaultAsync(u => u.NameEn == "University of Cambridge");
                
                var tsinghua = await context.Universities
                    .Include(u => u.ExamTypes)
                    .FirstOrDefaultAsync(u => u.NameEn == "Tsinghua University");

                // Назначаем типы экзаменов университетам
                // Казахстанские университеты принимают ЕНТ + международные экзамены
                if (nazarbayev != null && ent != null && ielts != null && toefl != null && sat != null)
                {
                    nazarbayev.ExamTypes.Clear();
                    nazarbayev.ExamTypes.Add(ent);
                    nazarbayev.ExamTypes.Add(ielts);
                    nazarbayev.ExamTypes.Add(toefl);
                    nazarbayev.ExamTypes.Add(sat);
                }

                if (kaznu != null && ent != null && ielts != null)
                {
                    kaznu.ExamTypes.Clear();
                    kaznu.ExamTypes.Add(ent);
                    kaznu.ExamTypes.Add(ielts);
                }

                if (kbtu != null && ent != null && ielts != null && toefl != null)
                {
                    kbtu.ExamTypes.Clear();
                    kbtu.ExamTypes.Add(ent);
                    kbtu.ExamTypes.Add(ielts);
                    kbtu.ExamTypes.Add(toefl);
                }

                if (enu != null && ent != null)
                {
                    enu.ExamTypes.Clear();
                    enu.ExamTypes.Add(ent);
                }

                if (iitu != null && ent != null && ielts != null)
                {
                    iitu.ExamTypes.Clear();
                    iitu.ExamTypes.Add(ent);
                    iitu.ExamTypes.Add(ielts);
                }

                // Российские университеты принимают ЕГЭ + международные экзамены
                if (msu != null && ege != null && ielts != null && toefl != null)
                {
                    msu.ExamTypes.Clear();
                    msu.ExamTypes.Add(ege);
                    msu.ExamTypes.Add(ielts);
                    msu.ExamTypes.Add(toefl);
                }

                if (spbu != null && ege != null && ielts != null)
                {
                    spbu.ExamTypes.Clear();
                    spbu.ExamTypes.Add(ege);
                    spbu.ExamTypes.Add(ielts);
                }

                // Американские университеты принимают SAT/ACT + языковые экзамены
                if (harvard != null && sat != null && act != null && toefl != null && ielts != null)
                {
                    harvard.ExamTypes.Clear();
                    harvard.ExamTypes.Add(sat);
                    harvard.ExamTypes.Add(act);
                    harvard.ExamTypes.Add(toefl);
                    harvard.ExamTypes.Add(ielts);
                }

                if (mit != null && sat != null && act != null && toefl != null && ielts != null)
                {
                    mit.ExamTypes.Clear();
                    mit.ExamTypes.Add(sat);
                    mit.ExamTypes.Add(act);
                    mit.ExamTypes.Add(toefl);
                    mit.ExamTypes.Add(ielts);
                }

                // Британские университеты принимают A-Level + языковые экзамены
                if (oxford != null && aLevel != null && ielts != null && toefl != null)
                {
                    oxford.ExamTypes.Clear();
                    oxford.ExamTypes.Add(aLevel);
                    oxford.ExamTypes.Add(ielts);
                    oxford.ExamTypes.Add(toefl);
                }

                if (cambridge != null && aLevel != null && ielts != null && toefl != null)
                {
                    cambridge.ExamTypes.Clear();
                    cambridge.ExamTypes.Add(aLevel);
                    cambridge.ExamTypes.Add(ielts);
                    cambridge.ExamTypes.Add(toefl);
                }

                // Китайские университеты принимают Gaokao + языковые экзамены
                if (tsinghua != null && gaokao != null && ielts != null && toefl != null)
                {
                    tsinghua.ExamTypes.Clear();
                    tsinghua.ExamTypes.Add(gaokao);
                    tsinghua.ExamTypes.Add(ielts);
                    tsinghua.ExamTypes.Add(toefl);
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
