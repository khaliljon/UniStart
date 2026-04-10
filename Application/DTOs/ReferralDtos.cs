namespace UniStart.Application.DTOs;

public record ReferralStatsDto(
    string? Code,
    bool IsActive,
    int TotalReferred,
    int TotalPaid,
    decimal TotalEarned,
    decimal AvailableBalance,
    int BonusDays,
    string RewardType  // "money" or "days"
);

public record ReferralActivateResponseDto(
    string Code,
    string Message
);

public record ReferralUsageDto(
    int Id,
    string UserName,
    DateTime RegisteredAt,
    DateTime? PaidAt,
    bool RewardGranted
);
