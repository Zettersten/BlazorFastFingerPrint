# BlazorSupercookie

Enterprise-grade Blazor component for browser fingerprinting using favicon caching technique. This component implements the supercookie concept - a tracking method that uses browser favicon cache to create persistent identifiers that survive cache clearing, incognito mode, and browser restarts.

## ⚠️ Warning

This library is for **educational and demonstration purposes only**. The technique demonstrated here can be used for privacy-invasive tracking. Use responsibly and only with explicit user consent.

## Features

- 🚀 **High Performance**: Zero-allocation patterns, thread-safe implementation
- 🔒 **Thread-Safe**: Built with concurrency in mind using modern C# patterns
- 🎯 **Developer Experience**: Simple, intuitive API with comprehensive documentation
- 📦 **NuGet Ready**: Fully configured for NuGet package distribution
- 🧪 **Well Tested**: Comprehensive unit tests included
- 🎨 **Modern UI**: Beautiful, responsive component with progress indicators

## Installation

```bash
dotnet add package BlazorSupercookie
```

## Quick Start

### 1. Add the component to your Blazor app

Add the CSS and JS references to your `index.html` or `_Host.cshtml`:

```html
<link href="_content/BlazorSupercookie/css/supercookie.css" rel="stylesheet" />
```

```razor
@using BlazorSupercookie

<SupercookieComponent 
    CacheIdentifier="my-app"
    RouteCount="32"
    BaseUrl="/"
    OnFingerprintComplete="HandleFingerprint"
    @ref="_component" />
```

### 2. Use the component in your code

```csharp
private SupercookieComponent? _component;

private async Task ReadFingerprint()
{
    var identifier = await _component?.ReadFingerprintAsync();
    Console.WriteLine($"Fingerprint: {identifier}");
}

private async Task WriteFingerprint(ulong identifier)
{
    await _component?.WriteFingerprintAsync(identifier);
}
```

## API Reference

### SupercookieComponent

#### Parameters

- `CacheIdentifier` (string): Unique identifier for route generation
- `RouteCount` (int): Number of routes to generate (default: 32)
- `BaseUrl` (string): Base URL for favicon routes
- `ShowProgress` (bool): Whether to show progress indicator
- `OnFingerprintComplete` (EventCallback<ulong?>): Callback when fingerprinting completes
- `OnError` (EventCallback<string>): Callback when an error occurs

#### Methods

- `ReadFingerprintAsync(int? storageSize)`: Reads existing fingerprint
- `WriteFingerprintAsync(ulong identifier)`: Writes a new fingerprint

### FingerprintEngine

Core engine for fingerprint operations. Thread-safe and optimized for performance.

```csharp
var engine = new FingerprintEngine("my-cache-id", routeCount: 32);
var routes = engine.GetRoutesForIdentifier(12345);
var identifier = engine.GetIdentifierFromRoutes(cachedRoutes);
```

## How It Works

1. **Write Mode**: Converts an identifier to binary, then caches favicons for routes where bit=1
2. **Read Mode**: Checks which favicon routes are cached, reconstructs the identifier from the binary pattern

The favicon cache persists even after:
- Clearing browser cache
- Closing the browser
- Restarting the OS
- Using incognito/private mode
- Using VPNs

## Requirements

- .NET 10.0 or later
- Blazor WebAssembly or Blazor Server
- Modern browser with favicon caching support

## License

MIT License - see LICENSE file for details

## Contributing

Contributions are welcome! Please read our contributing guidelines and code of conduct.

## Acknowledgments

Inspired by the research paper from University of Illinois, Chicago:
[www.cs.uic.edu](https://www.cs.uic.edu/~polakis/papers/solomos-ndss21.pdf)
