using Microsoft.AspNetCore.Mvc;

namespace UniStart.Controllers;

// Branded short links for social bios. Each returns a temporary (302) redirect to
// a hardcoded UTM-tagged destination so first-touch attribution (attribution.ts)
// can read the UTM params after the browser lands. Destinations are fixed here —
// no user input is used, so there is no open-redirect surface.
[ApiController]
public class SocialRedirectController : ControllerBase
{
    private const string Threads = "https://unistart.kz/?utm_source=threads&utm_medium=social&utm_campaign=khalil_personal&utm_content=link_in_bio";
    private const string Instagram = "https://unistart.kz/landing?utm_source=instagram&utm_medium=social&utm_campaign=unistart&utm_content=link_in_bio";
    private const string TikTok = "https://unistart.kz/landing?utm_source=tiktok&utm_medium=social&utm_campaign=unistart&utm_content=link_in_bio";
    private const string Telegram = "https://unistart.kz/landing?utm_source=telegram&utm_medium=social&utm_campaign=unistart&utm_content=link_in_bio";

    [HttpGet("/threads")]
    public IActionResult ThreadsRedirect() => Redirect(Threads);

    [HttpGet("/instagram")]
    public IActionResult InstagramRedirect() => Redirect(Instagram);

    [HttpGet("/tiktok")]
    public IActionResult TikTokRedirect() => Redirect(TikTok);

    [HttpGet("/telegram")]
    public IActionResult TelegramRedirect() => Redirect(Telegram);
}
