using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Data;
using UrlShortener.Api.Models;
using UrlShortener.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is missing.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServer => sqlServer.EnableRetryOnFailure()));

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var codePattern = new Regex("^[a-zA-Z0-9_-]{3,20}$", RegexOptions.Compiled);

app.MapPost("/api/links", async (
    CreateLinkRequest request,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var originalUri) ||
        (originalUri.Scheme != Uri.UriSchemeHttp && originalUri.Scheme != Uri.UriSchemeHttps))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["url"] = ["Enter an absolute URL that starts with http:// or https://."]
        });
    }

    if (originalUri.AbsoluteUri.Length > 2048)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["url"] = ["The URL cannot be longer than 2048 characters."]
        });
    }

    if (request.ExpiresInDays is <= 0 or > 365)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["expiresInDays"] = ["Choose a value between 1 and 365 days."]
        });
    }

    var customCode = string.IsNullOrWhiteSpace(request.CustomCode)
        ? null
        : request.CustomCode.Trim();

    if (customCode is not null && !codePattern.IsMatch(customCode))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["customCode"] = ["Use 3-20 letters, numbers, hyphens or underscores."]
        });
    }

    // The unique index is the final authority. A pre-check alone cannot prevent concurrent duplicates.
    const int maxAttempts = 5;

    for (var attempt = 0; attempt < maxAttempts; attempt++)
    {
        var link = new ShortLink
        {
            Code = customCode ?? ShortCodeGenerator.Create(7),
            OriginalUrl = originalUri.AbsoluteUri,
            ExpiresAt = request.ExpiresInDays is int days
                ? DateTime.UtcNow.AddDays(days)
                : null
        };

        db.Links.Add(link);

        try
        {
            await db.SaveChangesAsync(cancellationToken);

            var response = new CreateLinkResponse(
                link.Code,
                $"/{link.Code}",
                link.OriginalUrl,
                link.ExpiresAt);

            return Results.Created($"/api/links/{link.Code}", response);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            db.Entry(link).State = EntityState.Detached;

            if (customCode is not null)
                return Results.Conflict(new { message = "That short code is already in use." });

            if (attempt == maxAttempts - 1)
                return Results.Problem(
                    title: "A unique short code could not be generated. Please try again.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    return Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable);
})
.WithName("CreateLink")
.Produces<CreateLinkResponse>(StatusCodes.Status201Created)
.ProducesValidationProblem()
.Produces(StatusCodes.Status409Conflict);

app.MapGet("/api/links/{code}", async (
    string code,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    var link = await db.Links.AsNoTracking()
        .FirstOrDefaultAsync(item => item.Code == code, cancellationToken);

    if (link is null)
        return Results.NotFound();

    return Results.Ok(new LinkStatsResponse(
        link.Code,
        link.OriginalUrl,
        link.Clicks,
        link.CreatedAt,
        link.ExpiresAt));
})
.WithName("GetLinkStats");

app.MapDelete("/api/links/{code}", async (
    string code,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    var deleted = await db.Links
        .Where(link => link.Code == code)
        .ExecuteDeleteAsync(cancellationToken);

    return deleted == 0 ? Results.NotFound() : Results.NoContent();
})
.WithName("DeleteLink");

// This catch-all route is excluded from Swagger so it does not clutter the API description.
app.MapGet("/{code}", async (
    string code,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    var now = DateTime.UtcNow;
    var link = await db.Links.AsNoTracking()
        .FirstOrDefaultAsync(item => item.Code == code, cancellationToken);

    if (link is null)
        return Results.NotFound();

    if (link.ExpiresAt.HasValue && link.ExpiresAt.Value <= now)
        return Results.StatusCode(StatusCodes.Status410Gone);

    // Include the expiry condition in the update to handle a link expiring between the read and write.
    var updated = await db.Links
        .Where(item => item.Id == link.Id &&
            (item.ExpiresAt == null || item.ExpiresAt > now))
        .ExecuteUpdateAsync(
            setters => setters.SetProperty(item => item.Clicks, item => item.Clicks + 1),
            cancellationToken);

    if (updated == 0)
    {
        var stillExists = await db.Links.AnyAsync(item => item.Id == link.Id, cancellationToken);
        return stillExists
            ? Results.StatusCode(StatusCodes.Status410Gone)
            : Results.NotFound();
    }

    return Results.Redirect(link.OriginalUrl);
})
.ExcludeFromDescription();

app.Run();

static bool IsUniqueConstraintViolation(DbUpdateException exception)
{
    var sqlException = exception.GetBaseException() as SqlException;
    return sqlException?.Number is 2601 or 2627;
}

public partial class Program { }
