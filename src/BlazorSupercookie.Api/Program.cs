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

app.MapGet("/f/{route}", (string route, FingerprintEngine engine) =>
{
    if (!engine.IsValidRoute(route))
    {
        return Results.NotFound();
    }

    var data = engine.FaviconData;
    
    return Results.File(
        data.ToArray(),
        "image/png",
        null,
        new Microsoft.Net.Http.Headers.CacheControlHeaderValue
        {
            Public = true,
            MaxAge = TimeSpan.FromDays(365)
        });
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
