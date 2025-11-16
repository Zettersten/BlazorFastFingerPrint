using BlazorSupercookie;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddSingleton<FingerprintEngine>(_ => 
    new FingerprintEngine("blazor-supercookie-demo", 32));

var app = builder.Build();

app.UseCors();

app.MapGet("/f/{route}", (string route, FingerprintEngine engine, HttpContext context) =>
{
    if (!engine.IsValidRoute(route))
    {
        return Results.NotFound();
    }

    var data = engine.FaviconData;
    
    context.Response.Headers.CacheControl = "public, max-age=31536000";
    context.Response.Headers.Expires = DateTimeOffset.UtcNow.AddYears(1).ToString("R");
    
    return Results.File(
        data.ToArray(),
        "image/png",
        null,
        enableRangeProcessing: true);
})
.WithName("GetFavicon")
.WithOpenApi();

app.MapGet("/", () => Results.Json(new
{
    message = "BlazorSupercookie Favicon API",
    endpoints = new
    {
        favicon = "/f/{route}",
        health = "/health"
    }
}));

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
