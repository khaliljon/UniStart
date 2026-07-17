using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class EntitlementService : IEntitlementService
{
    private readonly UniStartDbContext _db;

    public EntitlementService(UniStartDbContext db)
    {
        _db = db;
    }

    public async Task<MockCatalogDto> GetCatalogAsync(int userId)
    {
        var exams = await _db.MockExams
            .Include(m => m.Sections)
            .Where(m => m.IsActive)
            .ToListAsync();

        var tiers = await _db.MockPriceTiers
            .Where(t => t.IsActive)
            .ToListAsync();

        var runsByMock = userId > 0
            ? await _db.UserMockRuns
                .Where(r => r.UserId == userId)
                .ToDictionaryAsync(r => r.MockExamId, r => r.RunsRemaining)
            : new Dictionary<int, int>();

        var packages = await _db.MockPackages
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .Select(p => new MockPackageDto(p.Id, p.Key, p.Name, p.PickCount, p.RunsEach, p.Price, p.Currency, p.SortOrder))
            .ToListAsync();

        var user = userId > 0 ? await _db.Users.FindAsync(userId) : null;
        var freeAvailable = user != null && !user.FreeMockUsed;

        var templates = exams
            .OrderBy(m => m.Id)
            .Select(m => new MockTemplateDto(
                m.Id,
                m.Title,
                m.ExamTypeCode,
                m.Sections.Sum(s => s.QuestionCount),
                m.TotalTimeMinutes,
                runsByMock.TryGetValue(m.Id, out var rr) ? rr : 0,
                tiers.Where(t => t.MockExamId == m.Id)
                     .OrderBy(t => t.Runs)
                     .Select(t => new MockTierDto(t.Id, t.Runs, t.Price, t.Currency))
                     .ToList()))
            .ToList();

        return new MockCatalogDto(freeAvailable, templates, packages);
    }

    public async Task<CheckoutQuoteDto> QuoteAsync(List<CheckoutLineDto> lines)
    {
        var (total, currency, _) = await ResolveAsync(lines);
        return new CheckoutQuoteDto(total, currency);
    }

    public async Task<CheckoutQuoteDto> GrantAsync(int userId, List<CheckoutLineDto> lines)
    {
        var (total, currency, resolved) = await ResolveAsync(lines);

        foreach (var line in resolved)
        {
            // Grant runs (upsert balance per template)
            foreach (var (mockExamId, runs) in line.Grants)
            {
                var balance = await _db.UserMockRuns
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.MockExamId == mockExamId);
                if (balance == null)
                {
                    _db.UserMockRuns.Add(new UserMockRuns { UserId = userId, MockExamId = mockExamId, RunsRemaining = runs });
                }
                else
                {
                    balance.RunsRemaining += runs;
                }
            }

            // Record the sale for the admin "Продажи" view
            _db.Purchases.Add(new Purchase
            {
                UserId = userId,
                ItemType = line.ItemType,
                ItemCode = line.ItemCode,
                Title = line.Title,
                Subjects = line.Subjects,
                Amount = line.Price,
                Currency = currency,
                Status = "Paid",
                PurchasedAt = DateTime.UtcNow,
            });
        }

        await _db.SaveChangesAsync();
        return new CheckoutQuoteDto(total, currency);
    }

    // ── internals ──────────────────────────────────────────

    private static string MoksLabel(int n)
    {
        var mod10 = n % 10;
        var mod100 = n % 100;
        string word = (mod10 == 1 && mod100 != 11) ? "мок"
            : (mod10 >= 2 && mod10 <= 4 && !(mod100 >= 12 && mod100 <= 14)) ? "мока"
            : "моков";
        return $"{n} {word}";
    }

    private sealed record ResolvedLine(
        string ItemType,
        string ItemCode,
        string Title,
        string? Subjects,
        decimal Price,
        List<(int MockExamId, int Runs)> Grants);

    private async Task<(decimal Total, string Currency, List<ResolvedLine> Lines)> ResolveAsync(List<CheckoutLineDto> lines)
    {
        if (lines == null || lines.Count == 0)
            throw new ArgumentException("Empty checkout");

        var activeMocks = await _db.MockExams.Where(m => m.IsActive).ToListAsync();
        var mockById = activeMocks.ToDictionary(m => m.Id);

        var resolved = new List<ResolvedLine>();
        decimal total = 0m;
        string currency = "KZT";

        foreach (var line in lines)
        {
            if (line.Kind == "mock")
            {
                if (line.MockExamId is not int mockId || !mockById.TryGetValue(mockId, out var mock))
                    throw new ArgumentException("Invalid mock");

                var tier = await _db.MockPriceTiers
                    .FirstOrDefaultAsync(t => t.IsActive && t.MockExamId == mockId && t.Runs == line.Runs)
                    ?? throw new ArgumentException("Invalid run tier");

                currency = tier.Currency;
                total += tier.Price;
                resolved.Add(new ResolvedLine(
                    "mock",
                    mockId.ToString(),
                    $"{mock.Title} · {MoksLabel(tier.Runs)}",
                    mock.ExamTypeCode,
                    tier.Price,
                    new List<(int, int)> { (mockId, tier.Runs) }));
            }
            else if (line.Kind == "package")
            {
                var pkg = await _db.MockPackages
                    .FirstOrDefaultAsync(p => p.IsActive && p.Key == line.PackageKey)
                    ?? throw new ArgumentException("Invalid package");

                List<int> chosen;
                if (pkg.PickCount == 0)
                {
                    chosen = activeMocks.Select(m => m.Id).ToList();
                }
                else
                {
                    chosen = (line.SelectedMockIds ?? new List<int>()).Distinct().ToList();
                    if (chosen.Count != pkg.PickCount || chosen.Any(id => !mockById.ContainsKey(id)))
                        throw new ArgumentException($"Package requires exactly {pkg.PickCount} valid subjects");
                }

                currency = pkg.Currency;
                total += pkg.Price;
                resolved.Add(new ResolvedLine(
                    "package",
                    pkg.Key,
                    pkg.Name,
                    string.Join(",", chosen),
                    pkg.Price,
                    chosen.Select(id => (id, pkg.RunsEach)).ToList()));
            }
            else if (line.Kind == "book")
            {
                var material = await _db.StudyMaterials
                    .FirstOrDefaultAsync(m => m.IsActive && m.Id == line.BookMaterialId)
                    ?? throw new ArgumentException("Invalid book");

                total += material.Price;
                resolved.Add(new ResolvedLine(
                    "book",
                    material.Id.ToString(),   // itemCode = material id (per-material ownership)
                    material.Title,
                    material.SubjectKey,
                    material.Price,
                    new List<(int, int)>())); // no runs; grants access via the Purchase record
            }
            else
            {
                throw new ArgumentException("Unknown line kind");
            }
        }

        return (total, currency, resolved);
    }
}
