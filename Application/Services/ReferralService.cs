using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Domain.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class ReferralService : IReferralService
{
    private readonly UniStartDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public ReferralService(UniStartDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReferralActivateResponseDto> ActivateAsync(int userId)
    {
        var existing = await _context.ReferralCodes.FirstOrDefaultAsync(r => r.OwnerUserId == userId);
        if (existing != null)
            return new ReferralActivateResponseDto(existing.Code, "Referral code already active");

        var code = GenerateCode();
        while (await _context.ReferralCodes.AnyAsync(r => r.Code == code))
            code = GenerateCode();

        var referralCode = new ReferralCode
        {
            OwnerUserId = userId,
            Code = code,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ReferralCodes.Add(referralCode);
        await _unitOfWork.SaveChangesAsync();

        return new ReferralActivateResponseDto(code, "Referral program activated");
    }

    public async Task<ReferralStatsDto> GetMyStatsAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        var referralCode = await _context.ReferralCodes
            .FirstOrDefaultAsync(r => r.OwnerUserId == userId);

        if (referralCode == null)
            return new ReferralStatsDto(null, false, 0, 0, 0, 0, 0,
                user.Role == UserRole.Student ? "days" : "money");

        var usages = await _context.ReferralUsages
            .Where(u => u.ReferralCodeId == referralCode.Id)
            .ToListAsync();

        var rewards = await _context.ReferralRewards
            .Where(r => r.OwnerUserId == userId)
            .ToListAsync();

        var rewardType = user.Role == UserRole.Student ? "days" : "money";
        var totalEarned = rewards.Sum(r => r.Amount);
        var paidOut = rewards.Where(r => r.IsPaidOut).Sum(r => r.Amount);
        var bonusDays = rewardType == "days" ? (int)totalEarned : 0;

        return new ReferralStatsDto(
            Code: referralCode.Code,
            IsActive: referralCode.IsActive,
            TotalReferred: usages.Count,
            TotalPaid: usages.Count(u => u.PaidAt != null),
            TotalEarned: rewardType == "money" ? totalEarned : 0,
            AvailableBalance: rewardType == "money" ? totalEarned - paidOut : 0,
            BonusDays: bonusDays,
            RewardType: rewardType
        );
    }

    public async Task<List<ReferralUsageDto>> GetMyReferralsAsync(int userId)
    {
        var referralCode = await _context.ReferralCodes
            .FirstOrDefaultAsync(r => r.OwnerUserId == userId);

        if (referralCode == null) return new List<ReferralUsageDto>();

        return await _context.ReferralUsages
            .Where(u => u.ReferralCodeId == referralCode.Id)
            .Include(u => u.ReferredUser)
            .OrderByDescending(u => u.RegisteredAt)
            .Select(u => new ReferralUsageDto(
                u.Id,
                u.ReferredUser.Name,
                u.RegisteredAt,
                u.PaidAt,
                u.RewardGranted
            ))
            .ToListAsync();
    }

    /// <summary>
    /// Called when a user upgrades to Pro. Grants reward to the referrer.
    /// </summary>
    public async Task GrantRewardForProUpgradeAsync(int userId)
    {
        var usage = await _context.ReferralUsages
            .Include(u => u.ReferralCode)
            .FirstOrDefaultAsync(u => u.ReferredUserId == userId && u.PaidAt == null);

        if (usage == null) return;

        usage.PaidAt = DateTime.UtcNow;
        usage.RewardGranted = true;
        usage.ReferralCode.UsedCount++;

        var owner = await _context.Users.FindAsync(usage.ReferralCode.OwnerUserId);
        if (owner == null) return;

        var isStudent = owner.Role == UserRole.Student;
        var reward = new ReferralReward
        {
            OwnerUserId = owner.Id,
            ReferralUsageId = usage.Id,
            RewardType = isStudent ? "days" : "money",
            Amount = isStudent ? 5 : 500,
            GrantedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMonths(12)
        };

        _context.ReferralRewards.Add(reward);

        // For students: auto-extend subscription by 5 days
        if (isStudent && owner.IsPro && owner.SubscriptionExpiresAt.HasValue)
        {
            owner.SubscriptionExpiresAt = owner.SubscriptionExpiresAt.Value.AddDays(5);
        }
        else if (isStudent && owner.IsPro && owner.SubscriptionExpiresAt == null)
        {
            owner.SubscriptionExpiresAt = DateTime.UtcNow.AddDays(5);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no I/O/0/1 for clarity
        var random = new Random();
        return new string(Enumerable.Range(0, 8).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}
