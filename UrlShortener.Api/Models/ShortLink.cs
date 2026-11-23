using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Api.Models;

public class ShortLink
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string OriginalUrl { get; set; } = string.Empty;
    public long Clicks { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class CreateLinkRequest
{
    [Required]
    public string Url { get; set; } = string.Empty;

    [StringLength(20, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_-]{3,20}$")]
    public string? CustomCode { get; set; }

    [Range(1, 365)]
    public int? ExpiresInDays { get; set; }
}

public record LinkStatsResponse(
    string Code,
    string OriginalUrl,
    long Clicks,
    DateTime CreatedAt,
    DateTime? ExpiresAt);

public record CreateLinkResponse(string Code, string ShortPath, string OriginalUrl, DateTime? ExpiresAt);
