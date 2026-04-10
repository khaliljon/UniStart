using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.Interfaces;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/referral")]
[Authorize]
[ApiVersion("1.0")]
public class ReferralController : ControllerBase
{
    private readonly IReferralService _referralService;

    public ReferralController(IReferralService referralService)
    {
        _referralService = referralService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    [HttpPost("activate")]
    public async Task<IActionResult> Activate()
    {
        var result = await _referralService.ActivateAsync(GetUserId());
        return Ok(result);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _referralService.GetMyStatsAsync(GetUserId());
        return Ok(result);
    }

    [HttpGet("referrals")]
    public async Task<IActionResult> GetReferrals()
    {
        var result = await _referralService.GetMyReferralsAsync(GetUserId());
        return Ok(result);
    }
}
