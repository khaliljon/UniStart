using System.Collections.Generic;
using System.Linq;
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

    private static readonly (string Code, string Name, string Description, string[] Topics)[] NuetMathTaxonomy =
    {
        ("SK_NUET_UNITS", "Units & Measures",
            "NUET Mathematics — standard and compound units, approximation and estimation.",
            new[]
            {
                "Standard and compound units",
                "Changing Between Standard and Compound Units",
            }),
        ("SK_NUET_NUMBER", "Number",
            "NUET Mathematics — number, operations, indices, standard form and bounds.",
            new[]
            {
                "Ordering integers, decimals, fractions; symbols =, ≠, <, >, ≤, ≥",
                "Four operations with integers, decimals, fractions, mixed numbers; place value",
                "Prime numbers, factors, multiples, HCF, LCM, prime factorisation",
                "Inverse operations, cancellation, priority of operations (BODMAS)",
                "Systematic listing strategies",
                "Squares, square roots, cubes, cube roots",
                "Index laws (numerical)",
                "Standard index form (standard form)",
                "Converting between decimals, percentages and fractions (including recurring)",
                "Fractions, decimals and percentages interchangeably",
                "Exact calculations with fractions, surds and multiples of π",
                "Upper and lower bounds",
                "Rounding and error intervals",
                "Approximation and estimates",
            }),
        ("SK_NUET_RATIO", "Ratio & Proportion",
            "NUET Mathematics — ratio, proportion, percentages, growth and decay.",
            new[]
            {
                "Quantity as a fraction of another",
                "Ratio notation",
                "Dividing a quantity in a given ratio",
                "Applying ratio to real contexts; multiplicative relationships",
                "Proportion; ratios, fractions and linear functions",
                "Fractions in ratio problems",
                "Percentages and percentage change",
                "Direct and inverse proportion",
                "Lengths, areas and volumes in ratio; similarity and scale factors",
                "Growth and decay; compound interest; iterative processes",
            }),
        ("SK_NUET_ALGEBRA", "Algebra",
            "NUET Mathematics — algebraic manipulation, functions, graphs and sequences.",
            new[]
            {
                "Algebraic notation",
                "Index laws in algebra",
                "Substitution into formulae and expressions; algebraic vocabulary",
                "Expanding and factorising (single bracket, binomials, common factors)",
                "Factorising quadratic expressions",
                "Simplifying expressions; rational algebraic expressions",
                "Rearranging formulae",
                "Equations vs identities; algebraic equivalence",
                "Coordinates in all four quadrants",
                "Linear functions (y = mx + c); parallel and perpendicular lines",
                "Quadratic functions: roots, intercepts, turning points; completing the square",
                "Recognising and sketching graphs",
                "Interpreting graphs (reciprocal, exponential, kinematic)",
                "Gradients and areas under graphs",
                "Solving equations and simultaneous equations (linear/linear, linear/quadratic)",
                "Simultaneous equations and algebraic modelling",
                "Linear inequalities in one or two variables",
                "Sequences: term-to-term and position-to-term rules",
                "nth term of linear and quadratic sequences",
            }),
        ("SK_NUET_GEOMETRY", "Geometry",
            "NUET Mathematics — geometry, mensuration, trigonometry and vectors.",
            new[]
            {
                "Conventional terms and notation (points, lines, polygons, symmetry)",
                "Angle properties (straight lines, parallel lines, triangles, quadrilaterals, polygons)",
                "Properties of quadrilaterals and triangles",
                "Congruence criteria (SSS, SAS, ASA, RHS)",
                "Angle facts, congruence, similarity and quadrilateral properties",
                "Congruent and similar shapes; transformations (rotation, reflection, translation, enlargement); vectors as translations",
                "Pythagoras' theorem (2D and 3D)",
                "Circle terminology",
                "Circle theorems",
                "Coordinate geometry (2D)",
                "3D shape terminology (faces, edges, vertices)",
                "Plans and elevations",
                "Maps, scale drawings and bearings",
                "Areas of triangles, parallelograms, trapezia; volumes of prisms",
                "Circles, cylinders, spheres, pyramids, cones and composite solids",
                "Arc lengths, angles and areas of sectors",
                "Congruence and similarity: lengths, areas and volumes",
                "Trigonometric ratios",
                "Vectors: addition, subtraction, scalar multiplication, geometric proofs",
            }),
    };

    public async Task SeedAsync()
    {
        var hasCorrectData = await _context.ExamTypes.AnyAsync(e => e.Code == "CSCA");

        if (await _context.Topics.AnyAsync() && !hasCorrectData)
        {
            if (await HasRealUserDataAsync())
            {
                System.Console.WriteLine(
                    "[Seeder] Seed marker missing but real user data detected — skipping ClearAllDataAsync to prevent data loss.");
            }
            else
            {
                await ClearAllDataAsync();
            }
        }

        var totalQuestions = await _context.Questions.CountAsync();
        var questionsWithExplanation = await _context.Questions.CountAsync(q => q.Explanation != null);

        if (totalQuestions > 0 && questionsWithExplanation < totalQuestions)
        {
            var hasMockData = await _context.MockExamAnswers.AnyAsync()
                           || await _context.MockExamAttempts.AnyAsync();
            var hasUserData = await _context.UserAnswers.AnyAsync();

            if (!hasMockData && !hasUserData)
            {
                _context.AnswerOptions.RemoveRange(_context.AnswerOptions);
                _context.Questions.RemoveRange(_context.Questions);
                await _context.SaveChangesAsync();
            }
        }

        if (!await _context.ExamTypes.AnyAsync())
        {
            await SeedExamTypesAsync();
        }
        else
        {
            await EnsureExamTypesAsync();
        }

        if (!await _context.Users.AnyAsync())
        {
            await SeedTestUserAsync();
        }

        if (!await _context.Users.AnyAsync(u => u.Role == UserRole.Admin))
        {
            await SeedAdminUserAsync();
        }

        if (!await _context.ExamSections.AnyAsync())
        {
            await SeedExamSectionsAsync();
        }

        if (!await _context.Topics.AnyAsync())
        {
            await SeedTopicsAsync();
        }


        await UpdateIrtParametersAsync();

        if (!await _context.MockExams.AnyAsync())
        {
            await SeedMockExamsAsync();
        }

        await SeedLegalDocumentsAsync();
        await SeedSpecialtyTracksAsync();
        await BackfillNewsSlugsAsync();

        await _context.SaveChangesAsync();
    }

    // Replaces the migration's "news-{id}" placeholders with readable slugs, once.
    private async Task BackfillNewsSlugsAsync()
    {
        var placeholders = await _context.NewsArticles
            .Where(n => n.Slug.StartsWith("news-"))
            .ToListAsync();
        if (placeholders.Count == 0) return;

        var taken = (await _context.NewsArticles.Select(n => n.Slug).ToListAsync())
            .Concat(await _context.NewsSlugHistories.Select(h => h.Slug).ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var article in placeholders)
        {
            var baseSlug = SlugifyTitle(article.Title);
            if (baseSlug.Length == 0) continue; // keep the placeholder rather than risk a clash

            var candidate = baseSlug;
            var i = 2;
            while (taken.Contains(candidate) && candidate != article.Slug)
                candidate = $"{baseSlug}-{i++}";

            taken.Remove(article.Slug);
            taken.Add(candidate);
            article.Slug = candidate;
        }
    }

    private static string SlugifyTitle(string title)
    {
        const string cyr = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";
        string[] lat = { "a","b","v","g","d","e","e","zh","z","i","y","k","l","m","n","o","p","r","s","t","u","f","h","c","ch","sh","sch","","y","","e","yu","ya" };
        var sb = new System.Text.StringBuilder();
        foreach (var ch in title.ToLowerInvariant())
        {
            var idx = cyr.IndexOf(ch);
            if (idx >= 0) sb.Append(lat[idx]);
            else if (char.IsLetterOrDigit(ch) && ch < 128) sb.Append(ch);
            else if (char.IsWhiteSpace(ch) || ch == '-' || ch == '_') sb.Append('-');
        }
        var slug = System.Text.RegularExpressions.Regex.Replace(sb.ToString(), "-{2,}", "-").Trim('-');
        return slug.Length > 180 ? slug[..180].Trim('-') : slug;
    }

    /// <summary>Seeds the default field-of-study → subjects mapping once; admins edit it afterwards.</summary>
    private async Task SeedSpecialtyTracksAsync()
    {
        if (await _context.SpecialtyTracks.AnyAsync()) return;

        var defaults = new (string Ru, string Kz, string En, string Subjects, bool Cond)[]
        {
            ("IT и Computer Science", "IT және Computer Science", "IT & Computer Science", "math,physics", false),
            ("Инженерия и робототехника", "Инженерия және робототехника", "Engineering & robotics", "math,physics", false),
            ("Химия и химтехнологии", "Химия және химиялық технология", "Chemistry & chemical engineering", "math,chemistry", false),
            ("Медицина и фармацевтика", "Медицина және фармацевтика", "Medicine & pharmacy", "math,chemistry", false),
            ("Экономика и бизнес", "Экономика және бизнес", "Economics & business", "math", false),
            ("Международные отношения", "Халықаралық қатынастар", "International relations", "math,chineseHum", true),
            ("Гуманитарные направления", "Гуманитарлық бағыттар", "Humanities", "math,chineseHum", true),
            ("Архитектура и строительство", "Сәулет және құрылыс", "Architecture & construction", "math,physics", false),
            ("Машиностроение и электротехника", "Машина жасау және электротехника", "Mechanical & electrical engineering", "math,physics", false),
        };

        var order = 1;
        foreach (var (ru, kz, en, subjects, cond) in defaults)
        {
            _context.SpecialtyTracks.Add(new SpecialtyTrack
            {
                Name = ru, NameKz = kz, NameEn = en,
                Subjects = subjects, ConditionalChinese = cond,
                SortOrder = order++, IsActive = true,
            });
        }
    }

    private async Task SeedLegalDocumentsAsync()
    {
        var existing = await _context.LegalDocuments
            .Select(d => d.Slug)
            .ToListAsync();

        var docs = new[]
        {
            new LegalDocument { Slug = "privacy", Title = "Политика конфиденциальности", LastUpdatedLabel = "", Content = "" },
            new LegalDocument { Slug = "terms", Title = "Пользовательское соглашение", LastUpdatedLabel = "", Content = "" },
        };

        var toAdd = docs.Where(d => !existing.Contains(d.Slug)).ToList();
        if (toAdd.Count > 0)
        {
            _context.LegalDocuments.AddRange(toAdd);
        }
    }

    private async Task<bool> HasRealUserDataAsync()
    {
        if (await _context.UserAnswers.AnyAsync()) return true;
        if (await _context.MockExamAnswers.AnyAsync()) return true;
        if (await _context.MockExamAttempts.AnyAsync()) return true;
        if (await _context.TimedDrillResults.AnyAsync()) return true;

        var realUsers = await _context.Users
            .CountAsync(u => u.Email != "test@unistart.kz" && u.Email != "admin@unistart.kz");
        return realUsers > 0;
    }

    private async Task ClearAllDataAsync()
    {
        _context.UserAnswers.RemoveRange(_context.UserAnswers);
        _context.MockExamAnswers.RemoveRange(_context.MockExamAnswers);
        _context.MockExamAttempts.RemoveRange(_context.MockExamAttempts);
        _context.TimedDrillResults.RemoveRange(_context.TimedDrillResults);
        _context.UserSkillProfiles.RemoveRange(_context.UserSkillProfiles);
        _context.AnswerOptions.RemoveRange(_context.AnswerOptions);
        _context.Questions.RemoveRange(_context.Questions);
        _context.TopicDependencies.RemoveRange(_context.TopicDependencies);
        _context.Topics.RemoveRange(_context.Topics);
        _context.ExamSections.RemoveRange(_context.ExamSections);
        _context.ExamTypes.RemoveRange(_context.ExamTypes);
        await _context.SaveChangesAsync();
    }

    private async Task SeedTestUserAsync()
    {
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
            new ExamType { Code = "CSCA", Name = "CSCA (China Scholastic Competency Assessment)" },
        };

        await _context.ExamTypes.AddRangeAsync(examTypes);
        await _context.SaveChangesAsync();
    }

    private async Task EnsureExamTypesAsync()
    {
        var existing = new HashSet<string>(
            await _context.ExamTypes.Select(e => e.Code).ToListAsync());

        if (!existing.Contains("CSCA"))
        {
            await _context.ExamTypes.AddAsync(
                new ExamType { Code = "CSCA", Name = "CSCA (China Scholastic Competency Assessment)" });
            await _context.SaveChangesAsync();
        }
    }

    private Task SeedExamSectionsAsync()
    {
        return Task.CompletedTask;
    }

    private Task SeedSkillsAsync()
    {
        return Task.CompletedTask;
    }

    private Task SeedTopicsAsync()
    {
        return Task.CompletedTask;
    }

    private async Task UpdateIrtParametersAsync()
    {
        var defaultQuestions = await _context.Questions
            .Where(q => q.DifficultyParam == 0 && q.DiscriminationParam == 0 && q.GuessParam == 0)
            .ToListAsync();

        foreach (var question in defaultQuestions)
        {
            question.DifficultyParam = 0.0;
            question.DiscriminationParam = 1.0;
            question.GuessParam = 0.25;
        }

        if (defaultQuestions.Any())
        {
            await _context.SaveChangesAsync();
        }
    }

    private Task SeedMockExamsAsync()
    {
        return Task.CompletedTask;
    }
}
