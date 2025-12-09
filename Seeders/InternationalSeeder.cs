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
        }
    }
}
