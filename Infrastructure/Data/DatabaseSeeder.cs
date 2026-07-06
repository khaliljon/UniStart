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

    // ─── NUET Mathematics backbone taxonomy ─────────────────────────────────
    // Canonical Unit → Topic tree for the NUET "Math" section. Each Unit becomes a
    // Skill (the grouping the IRT/prediction engine aggregates on, and the unit-level
    // bucket the content-ingestion pipeline maps files onto); each item becomes a
    // Topic with a 1-based SortOrder. Seeded on fresh databases and reconciled on
    // existing ones via EnsureNuetMathSkillsAsync / EnsureNuetMathTopicsAsync.
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
        // CSCA-only platform: the DB is considered correctly seeded once the CSCA
        // exam type exists. (Historically this checked for a SAT sample topic.)
        var hasCorrectData = await _context.ExamTypes.AnyAsync(e => e.Code == "CSCA");

        if (await _context.Topics.AnyAsync() && !hasCorrectData)
        {
            // SAFETY: never wipe a database that already holds real activity.
            // A schema/seed-marker change must not be allowed to destroy live data
            // (user answers, mock attempts, real accounts). Only auto-reset when the
            // DB is effectively empty of user activity (fresh/dev environment).
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
            // Only wipe questions if no real user data exists (dev/empty DB).
            // On production with MockExamAnswers/UserAnswers skip cleanup to avoid data loss.
            var hasMockData = await _context.MockExamAnswers.AnyAsync()
                           || await _context.MockExamAttempts.AnyAsync();
            var hasUserData = await _context.UserAnswers.AnyAsync();

            if (!hasMockData && !hasUserData)
            {
                _context.AnswerOptions.RemoveRange(_context.AnswerOptions);
                _context.Questions.RemoveRange(_context.Questions);
                await _context.SaveChangesAsync();
            }
            // else: skip — questions will remain as-is; new ones will be seeded below
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

        // Reconcile taxonomy. CSCA-only platform: no NUET/SAT taxonomy is rebuilt.

        // NOTE: Question seeding has been intentionally removed. The question base is
        // now built via the content-ingestion pipeline (admin upload / Google Drive sync),
        // so seeded SAT/NUET sample questions are no longer re-created on startup.

        await UpdateIrtParametersAsync();

        if (!await _context.MockExams.AnyAsync())
        {
            await SeedMockExamsAsync();
        }

        await SeedLegalDocumentsAsync();

        await _context.SaveChangesAsync();
    }

    private async Task SeedLegalDocumentsAsync()
    {
        var existing = await _context.LegalDocuments
            .Select(d => d.Slug)
            .ToListAsync();

        var docs = new[]
        {
            new LegalDocument
            {
                Slug = LegalDocumentSeedData.PrivacySlug,
                Title = LegalDocumentSeedData.PrivacyTitle,
                LastUpdatedLabel = LegalDocumentSeedData.PrivacyLastUpdated,
                Content = LegalDocumentSeedData.PrivacyContent,
            },
            new LegalDocument
            {
                Slug = LegalDocumentSeedData.TermsSlug,
                Title = LegalDocumentSeedData.TermsTitle,
                LastUpdatedLabel = LegalDocumentSeedData.TermsLastUpdated,
                Content = LegalDocumentSeedData.TermsContent,
            },
            new LegalDocument
            {
                Slug = LegalDocumentSeedData.ReferralSlug,
                Title = LegalDocumentSeedData.ReferralTitle,
                LastUpdatedLabel = LegalDocumentSeedData.ReferralLastUpdated,
                Content = LegalDocumentSeedData.ReferralContent,
            },
        };

        // Only insert documents that are missing — never overwrite admin edits.
        var toAdd = docs.Where(d => !existing.Contains(d.Slug)).ToList();
        if (toAdd.Count > 0)
        {
            _context.LegalDocuments.AddRange(toAdd);
        }
    }

    private async Task<bool> HasRealUserDataAsync()
    {
        // Treat the DB as "live" if any student activity exists, or if there are
        // user accounts beyond the seeded defaults (test/admin) and partner schools.
        if (await _context.UserAnswers.AnyAsync()) return true;
        if (await _context.MockExamAnswers.AnyAsync()) return true;
        if (await _context.MockExamAttempts.AnyAsync()) return true;
        if (await _context.TimedDrillResults.AnyAsync()) return true;
        if (await _context.TutorSchools.AnyAsync()) return true;

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

    /// <summary>
    /// Ensures the CSCA exam type exists even if the table was already partially seeded.
    /// CSCA-only platform: no other exam types are (re)created.
    /// </summary>
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
        // CSCA-only platform: the CSCA subject/section structure is not finalized yet,
        // so nothing is hard-seeded. Sections are created later via the admin panel /
        // content-ingestion pipeline once the material structure is known.
        return Task.CompletedTask;
    }

    private Task SeedSkillsAsync()
    {
        // CSCA-only platform: skills (subjects) are not hard-seeded yet — added later
        // via the admin panel once the CSCA material structure is defined.
        return Task.CompletedTask;
    }

    private Task SeedTopicsAsync()
    {
        // CSCA-only platform: topics are built via the content-ingestion pipeline
        // (admin upload / import), not hard-seeded here.
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
        // CSCA-only platform: mock exams are authored via the admin panel, not hard-seeded.
        return Task.CompletedTask;
    }
}
