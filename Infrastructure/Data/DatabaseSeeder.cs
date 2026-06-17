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
                "Approximation and estimates (π, surds)",
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
        var hasCorrectData = await _context.Topics
            .Include(t => t.Section)
            .AnyAsync(t => t.Section != null && t.Section.ExamTypeCode == "SAT" && t.Name == "Main Idea & Summary");

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

        if (!await _context.Skills.AnyAsync())
        {
            await SeedSkillsAsync();
        }

        if (!await _context.ExamSections.AnyAsync())
        {
            await SeedExamSectionsAsync();
        }

        if (!await _context.Topics.AnyAsync())
        {
            await SeedTopicsAsync();
        }

        // Reconcile the NUET Math backbone taxonomy. Idempotent and unconditional so the
        // canonical unit/topic tree is present even on databases seeded before it existed.
        await EnsureNuetMathSkillsAsync();
        await EnsureNuetMathTopicsAsync();

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
        _context.Skills.RemoveRange(_context.Skills);
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
            new ExamType { Code = "SAT",  Name = "SAT (English + Mathematics)" },
            new ExamType { Code = "NUET", Name = "NUET (Mathematics + Critical Thinking)" },
            new ExamType { Code = "CSCA", Name = "CSCA (China Standardized College Admission)" },
            new ExamType { Code = "IELTS", Name = "IELTS (International English Language Testing System)" },
            new ExamType { Code = "TOEFL", Name = "TOEFL (Test of English as a Foreign Language)" },
        };

        await _context.ExamTypes.AddRangeAsync(examTypes);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Ensures CSCA / IELTS / TOEFL exist even if the table was already partially seeded.
    /// Needed after the AddImageUrlToQuestion migration removed them from HasData.
    /// </summary>
    private async Task EnsureExamTypesAsync()
    {
        var existing = new HashSet<string>(
            await _context.ExamTypes.Select(e => e.Code).ToListAsync());
        var missing = new List<ExamType>();

        void Ensure(string code, string name)
        {
            if (!existing.Contains(code))
                missing.Add(new ExamType { Code = code, Name = name });
        }

        Ensure("CSCA",  "CSCA (China Standardized College Admission)");
        Ensure("IELTS", "IELTS (International English Language Testing System)");
        Ensure("TOEFL", "TOEFL (Test of English as a Foreign Language)");

        if (missing.Count > 0)
        {
            await _context.ExamTypes.AddRangeAsync(missing);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedExamSectionsAsync()
    {
        var sections = new List<ExamSection>
        {
            new ExamSection { ExamTypeCode = "SAT", Name = "Reading & Writing", MinScore = 200, MaxScore = 800 },
            new ExamSection { ExamTypeCode = "SAT", Name = "Math (No Calculator)", MinScore = 200, MaxScore = 400 },
            new ExamSection { ExamTypeCode = "SAT", Name = "Math (Calculator)", MinScore = 200, MaxScore = 400 },
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
            new Skill { Code = "SK_READ", Name = "Reading", Description = "Reading comprehension and analysis." },
            new Skill { Code = "SK_WRITE", Name = "Writing", Description = "Writing clarity and structure." },
            new Skill { Code = "SK_MATH", Name = "Mathematics", Description = "Mathematical problem solving." },
            new Skill { Code = "SK_CRIT", Name = "Critical Thinking", Description = "Logic, reasoning, and analysis." }
        };

        await _context.Skills.AddRangeAsync(skills);
        await _context.SaveChangesAsync();
    }

    private async Task SeedTopicsAsync()
    {
        var satReadWrite = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "SAT" && s.Name == "Reading & Writing");
        var satMathNoCalc = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "SAT" && s.Name == "Math (No Calculator)");
        var satMathCalc = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "SAT" && s.Name == "Math (Calculator)");
        var nuetCritical = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "NUET" && s.Name == "Critical Thinking");

        var skillRead = await _context.Skills.FirstAsync(s => s.Code == "SK_READ");
        var skillWrite = await _context.Skills.FirstAsync(s => s.Code == "SK_WRITE");
        var skillMath = await _context.Skills.FirstAsync(s => s.Code == "SK_MATH");
        var skillCrit = await _context.Skills.FirstAsync(s => s.Code == "SK_CRIT");

        var topics = new List<Topic>
        {
            new Topic { Name = "Main Idea & Summary", SkillId = skillRead.Id, SectionId = satReadWrite.Id },
            new Topic { Name = "Grammar & Sentence Structure", SkillId = skillWrite.Id, SectionId = satReadWrite.Id },
            new Topic { Name = "Linear Equations", SkillId = skillMath.Id, SectionId = satMathNoCalc.Id },
            new Topic { Name = "Quadratic Equations", SkillId = skillMath.Id, SectionId = satMathCalc.Id },
            new Topic { Name = "Logical Reasoning", SkillId = skillCrit.Id, SectionId = nuetCritical.Id }
        };

        await _context.Topics.AddRangeAsync(topics);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Get-or-create the per-Unit Skills of the NUET Math backbone (keyed by Code).
    /// Safe to run on every startup — only missing Skills are inserted.
    /// </summary>
    private async Task EnsureNuetMathSkillsAsync()
    {
        var existing = new HashSet<string>(
            await _context.Skills.Select(s => s.Code).ToListAsync());

        var toAdd = NuetMathTaxonomy
            .Where(u => !existing.Contains(u.Code))
            .Select(u => new Skill { Code = u.Code, Name = u.Name, Description = u.Description })
            .ToList();

        if (toAdd.Count > 0)
        {
            await _context.Skills.AddRangeAsync(toAdd);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Get-or-create the NUET Math Topics under the "Math" section, one per taxonomy
    /// item, with a 1-based SortOrder inside each Unit/Skill. Deduplicated by Topic
    /// name within the section so re-running never creates duplicates.
    /// </summary>
    private async Task EnsureNuetMathTopicsAsync()
    {
        var nuetMath = await _context.ExamSections
            .FirstOrDefaultAsync(s => s.ExamTypeCode == "NUET" && s.Name == "Math");
        if (nuetMath == null) return;

        var codes = NuetMathTaxonomy.Select(u => u.Code).ToList();
        var skillIdByCode = await _context.Skills
            .Where(s => codes.Contains(s.Code))
            .ToDictionaryAsync(s => s.Code, s => s.Id);

        var existingNames = new HashSet<string>(
            await _context.Topics
                .Where(t => t.SectionId == nuetMath.Id)
                .Select(t => t.Name)
                .ToListAsync());

        var toAdd = new List<Topic>();
        foreach (var unit in NuetMathTaxonomy)
        {
            if (!skillIdByCode.TryGetValue(unit.Code, out var skillId)) continue;
            for (var i = 0; i < unit.Topics.Length; i++)
            {
                var name = unit.Topics[i];
                if (existingNames.Contains(name)) continue;
                toAdd.Add(new Topic
                {
                    Name = name,
                    SkillId = skillId,
                    SectionId = nuetMath.Id,
                    SortOrder = i + 1,
                });
            }
        }

        if (toAdd.Count > 0)
        {
            await _context.Topics.AddRangeAsync(toAdd);
            await _context.SaveChangesAsync();
        }
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

    private async Task SeedMockExamsAsync()
    {
        var satRw = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "SAT" && s.Name == "Reading & Writing");
        var satMathNoCalc = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "SAT" && s.Name == "Math (No Calculator)");
        var satMathCalc = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "SAT" && s.Name == "Math (Calculator)");
        var nuetMath = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "NUET" && s.Name == "Math");
        var nuetCritical = await _context.ExamSections.FirstAsync(s => s.ExamTypeCode == "NUET" && s.Name == "Critical Thinking");

        var mockExams = new List<MockExam>
        {
            new MockExam
            {
                ExamTypeCode = "SAT",
                Title = "SAT Practice Test",
                Description = "SAT practice test covering Reading & Writing and Math sections.",
                TotalTimeMinutes = 50,
                IsActive = true,
                Sections = new List<MockExamSection>
                {
                    new MockExamSection { ExamSectionId = satRw.Id, Name = "Reading & Writing", TimeLimitMinutes = 15, QuestionCount = 12, SortOrder = 0 },
                    new MockExamSection { ExamSectionId = satMathNoCalc.Id, Name = "Math (No Calculator)", TimeLimitMinutes = 15, QuestionCount = 8, SortOrder = 1 },
                    new MockExamSection { ExamSectionId = satMathCalc.Id, Name = "Math (Calculator)", TimeLimitMinutes = 15, QuestionCount = 8, SortOrder = 2 }
                }
            },
            new MockExam
            {
                ExamTypeCode = "NUET",
                Title = "NUET Practice Test",
                Description = "NUET practice test with Math and Critical Thinking sections.",
                TotalTimeMinutes = 50,
                IsActive = true,
                Sections = new List<MockExamSection>
                {
                    new MockExamSection { ExamSectionId = nuetMath.Id, Name = "Math", TimeLimitMinutes = 25, QuestionCount = 20, SortOrder = 0 },
                    new MockExamSection { ExamSectionId = nuetCritical.Id, Name = "Critical Thinking", TimeLimitMinutes = 25, QuestionCount = 20, SortOrder = 1 }
                }
            }
        };

        await _context.MockExams.AddRangeAsync(mockExams);
        await _context.SaveChangesAsync();
    }
}
