# Deployment Guide

This guide explains how to deploy BlazorSupercookie components and the demo application.

## Demo Deployment

### Option 1: GitHub Pages (Static Hosting)

The demo Blazor WebAssembly app can be deployed to GitHub Pages using the included workflow:

1. Push your code to GitHub
2. The workflow will automatically build and deploy to GitHub Pages
3. **Important**: You'll need to deploy the API backend separately (see below)

### Option 2: Azure Static Web Apps

1. Create an Azure Static Web App resource
2. Connect it to your GitHub repository
3. Configure build settings:
   - App location: `src/BlazorSupercookie.Demo`
   - Api location: `src/BlazorSupercookie.Api`
   - Output location: `publish/wwwroot`

### Option 3: Other Static Hosting

1. Build the demo:
   ```bash
   dotnet publish src/BlazorSupercookie.Demo -c Release -o publish
   ```
2. Upload the contents of `publish/wwwroot` to your hosting provider
3. Deploy the API backend separately

## API Backend Deployment

The favicon API is required for the demo to function. Deploy it separately:

### Option 1: Azure App Service

```bash
cd src/BlazorSupercookie.Api
dotnet publish -c Release -o ./publish
az webapp deploy --resource-group <resource-group> --name <app-name> --src-path ./publish
```

### Option 2: Docker

Create a `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/BlazorSupercookie.Api/BlazorSupercookie.Api.csproj", "src/BlazorSupercookie.Api/"]
COPY ["src/BlazorSupercookie/BlazorSupercookie.csproj", "src/BlazorSupercookie/"]
RUN dotnet restore "src/BlazorSupercookie.Api/BlazorSupercookie.Api.csproj"
COPY . .
WORKDIR "/src/src/BlazorSupercookie.Api"
RUN dotnet build "BlazorSupercookie.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlazorSupercookie.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlazorSupercookie.Api.dll"]
```

Build and run:
```bash
docker build -t blazor-supercookie-api .
docker run -p 5002:80 blazor-supercookie-api
```

### Option 3: Serverless Functions

You can deploy the favicon endpoint as an Azure Function or AWS Lambda. The endpoint is simple:

```csharp
[FunctionName("GetFavicon")]
public static IActionResult Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "f/{route}")] HttpRequest req,
    string route,
    ILogger log)
{
    var engine = new FingerprintEngine("blazor-supercookie-demo", 32);
    if (!engine.IsValidRoute(route))
        return new NotFoundResult();
    
    var data = engine.FaviconData;
    return new FileContentResult(data.ToArray(), "image/png")
    {
        FileDownloadName = "favicon.png"
    };
}
```

## Configuration

### Environment Variables

Set the `FAVICON_API_URL` environment variable to point to your deployed API:

```bash
export FAVICON_API_URL=https://your-api.azurewebsites.net/
```

### CORS Configuration

Ensure your API allows CORS from your demo domain:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://your-demo.github.io")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

## Local Development

1. Start the API:
   ```bash
   cd src/BlazorSupercookie.Api
   dotnet run
   ```

2. Start the demo:
   ```bash
   cd src/BlazorSupercookie.Demo
   dotnet run
   ```

3. Navigate to `https://localhost:5001`

## Troubleshooting

### Favicons not loading

- Check that the API is running and accessible
- Verify CORS is configured correctly
- Check browser console for errors
- Ensure the `BaseUrl` parameter matches your API URL

### GitHub Pages deployment issues

- Ensure the workflow has write permissions for GitHub Pages
- Check that the build completes successfully
- Verify the API URL is configured correctly for production
