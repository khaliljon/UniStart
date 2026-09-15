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

        var runsRows = userId > 0
            ? await _db.UserMockRuns.Where(r => r.UserId == userId).ToListAsync()
            : new List<UserMockRuns>();
        var runsByMock = runsRows
            .GroupBy(r => r.MockExamId)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.RunsRemaining));
        var runsByMockLang = runsRows
            .GroupBy(r => r.MockExamId)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(r => r.Language).ToDictionary(x => x.Key, x => x.Sum(r => r.RunsRemaining)));

        var packages = await _db.MockPackages
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .Select(p => new MockPackageDto(p.Id, p.Key, p.Name, p.PickCount, p.RunsEach, p.Price, p.Currency, p.SortOrder, p.NameKz, p.NameEn))
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
                     .ToList(),
                m.TitleKz,
                m.TitleEn,
                m.Description,
                m.DescriptionKz,
                m.DescriptionEn,
                runsByMockLang.TryGetValue(m.Id, out var rbl) ? rbl : null))
            .ToList();

        return new MockCatalogDto(freeAvailable, templates, packages);
    }

    public async Task<CheckoutQuoteDto> QuoteAsync(List<CheckoutLineDto> lines)
    {
        var (total, currency, _) = await ResolveAsync(lines);
        return new CheckoutQuoteDto(total, currency);
    }

    public async Task<OrderSnapshot> ResolveSnapshotAsync(List<CheckoutLineDto> lines)
    {
        var (total, currency, resolved) = await ResolveAsync(lines);
        var snapLines = resolved
            .Select(l => new OrderLineSnapshot(
                l.ItemType, l.ItemCode, l.Title, l.Subjects, l.Price, l.Language,
                l.Grants.Select(g => new OrderGrantSnapshot(g.MockExamId, g.Runs)).ToList()))
            .ToList();
        return new OrderSnapshot(total, currency, snapLines);
    }

    public async Task<CheckoutQuoteDto> GrantAsync(int userId, List<CheckoutLineDto> lines, PurchaseAmountsDto? amounts = null, string? externalPaymentId = null, string? checkoutRef = null, PaymentOrder? order = null)
    {
        var snapshot = await ResolveSnapshotAsync(lines);
        await GrantFromSnapshotAsync(userId, snapshot, amounts, externalPaymentId, checkoutRef, order);
        return new CheckoutQuoteDto(snapshot.Total, snapshot.Currency);
    }

    public async Task GrantFromSnapshotAsync(int userId, OrderSnapshot snapshot, PurchaseAmountsDto? amounts = null, string? externalPaymentId = null, string? checkoutRef = null, PaymentOrder? order = null)
    {
        var currency = snapshot.Currency;
        var totalLinePrice = snapshot.Lines.Sum(l => l.Price);

        // order == null means the legacy Polar webhook (checkout created before PaymentOrder existed).
        var isLegacyPolar = order == null;
        var provider = isLegacyPolar ? PaymentProviders.Polar : order!.Provider;
        var isPolar = provider == PaymentProviders.Polar;
        var polarOrderId = isPolar ? externalPaymentId : null;

        foreach (var line in snapshot.Lines)
        {
            foreach (var grant in line.Grants)
            {
                var balance = await _db.UserMockRuns
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.MockExamId == grant.MockExamId && r.Language == line.Language);
                if (balance == null)
                {
                    _db.UserMockRuns.Add(new UserMockRuns { UserId = userId, MockExamId = grant.MockExamId, RunsRemaining = grant.Runs, Language = line.Language });
                }
                else
                {
                    balance.RunsRemaining += grant.Runs;
                }
            }

            decimal share = amounts == null ? 0m
                : totalLinePrice > 0 ? line.Price / totalLinePrice
                : (snapshot.Lines.Count > 0 ? 1m / snapshot.Lines.Count : 0m);

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
                GrossAmount = amounts != null ? Math.Round(amounts.Gross * share, 2) : 0m,
                TaxAmount = amounts != null ? Math.Round(amounts.Tax * share, 2) : 0m,
                PlatformFeeAmount = amounts != null ? Math.Round(amounts.PlatformFee * share, 2) : 0m,
                PlatformFeeCurrency = amounts?.PlatformFeeCurrency,
                NetAmount = amounts != null ? Math.Round(amounts.Net * share, 2) : 0m,
                TotalAmount = amounts != null ? Math.Round(amounts.Total * share, 2) : 0m,
                PolarOrderId = polarOrderId,
                CheckoutRef = checkoutRef ?? order?.CheckoutRef,
                PaymentProvider = provider,
                ExternalPaymentId = externalPaymentId,
                PaymentOrderId = order?.Id,
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task UpdatePurchaseAmountsAsync(string polarOrderId, PurchaseAmountsDto amounts)
    {
        var purchases = await _db.Purchases
            .Where(p => p.PolarOrderId == polarOrderId)
            .ToListAsync();
        if (purchases.Count == 0) return;

        var totalPrice = purchases.Sum(p => p.Amount);
        var changed = false;

        foreach (var p in purchases)
        {
            decimal share = totalPrice > 0 ? p.Amount / totalPrice : 1m / purchases.Count;

            var gross = Math.Round(amounts.Gross * share, 2);
            var tax = Math.Round(amounts.Tax * share, 2);
            var fee = Math.Round(amounts.PlatformFee * share, 2);
            var net = Math.Round(amounts.Net * share, 2);
            var totalAmt = Math.Round(amounts.Total * share, 2);

            if (gross > 0 && p.GrossAmount != gross) { p.GrossAmount = gross; changed = true; }
            if (tax > 0 && p.TaxAmount != tax) { p.TaxAmount = tax; changed = true; }
            if (fee > 0 && p.PlatformFeeAmount != fee) { p.PlatformFeeAmount = fee; changed = true; }
            if (!string.IsNullOrEmpty(amounts.PlatformFeeCurrency) && p.PlatformFeeCurrency != amounts.PlatformFeeCurrency)
            { p.PlatformFeeCurrency = amounts.PlatformFeeCurrency; changed = true; }
            if (net > 0 && p.NetAmount != net) { p.NetAmount = net; changed = true; }
            if (totalAmt > 0 && p.TotalAmount != totalAmt) { p.TotalAmount = totalAmt; changed = true; }
        }

        if (changed) await _db.SaveChangesAsync();
    }


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
        string Language,
        List<(int MockExamId, int Runs)> Grants);

    private async Task<(decimal Total, string Currency, List<ResolvedLine> Lines)> ResolveAsync(List<CheckoutLineDto> lines)
    {
        if (lines == null || lines.Count == 0)
            throw new ArgumentException("Empty checkout");

        var activeMocks = await _db.MockExams.Where(m => m.IsActive).ToListAsync();
        var mockById = activeMocks.ToDictionary(m => m.Id);

        var resolved = new List<ResolvedLine>();
        decimal total = 0m;
        string? currency = null;

        // A checkout may only contain items in a single currency; the first line fixes it.
        void ApplyCurrency(string lineCurrency)
        {
            if (currency == null) currency = lineCurrency;
            else if (!string.Equals(currency, lineCurrency, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Все товары в одном заказе должны быть в одной валюте.");
        }

        foreach (var line in lines)
        {
            if (line.Kind == "mock")
            {
                if (line.MockExamId is not int mockId || !mockById.TryGetValue(mockId, out var mock))
                    throw new ArgumentException("Invalid mock");

                var tier = await _db.MockPriceTiers
                    .FirstOrDefaultAsync(t => t.IsActive && t.MockExamId == mockId && t.Runs == line.Runs)
                    ?? throw new ArgumentException("Invalid run tier");

                var lang = line.Language == "zh" ? "zh" : "en";
                ApplyCurrency(tier.Currency);
                total += tier.Price;
                resolved.Add(new ResolvedLine(
                    "mock",
                    mockId.ToString(),
                    $"{mock.Title} · {MoksLabel(tier.Runs)}",
                    mock.ExamTypeCode,
                    tier.Price,
                    lang,
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

                var lang = line.Language == "zh" ? "zh" : "en";
                ApplyCurrency(pkg.Currency);
                total += pkg.Price;
                resolved.Add(new ResolvedLine(
                    "package",
                    pkg.Key,
                    pkg.Name,
                    string.Join(",", chosen),
                    pkg.Price,
                    lang,
                    chosen.Select(id => (id, pkg.RunsEach)).ToList()));
            }
            else if (line.Kind == "book")
            {
                var material = await _db.StudyMaterials
                    .FirstOrDefaultAsync(m => m.IsActive && m.Id == line.BookMaterialId)
                    ?? throw new ArgumentException("Invalid book");

                // StudyMaterial has no own currency yet; books are priced in KZT.
                ApplyCurrency("KZT");
                total += material.Price;
                resolved.Add(new ResolvedLine(
                    "book",
                    material.Id.ToString(),
                    material.Title,
                    material.SubjectKey,
                    material.Price,
                    "en",
                    new List<(int, int)>()));
            }
            else
            {
                throw new ArgumentException("Unknown line kind");
            }
        }

        return (total, currency ?? "KZT", resolved);
    }
}
