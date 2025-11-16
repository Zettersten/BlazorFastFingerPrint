# BlazorSupercookie

Enterprise-grade Blazor component for browser fingerprinting using favicon caching technique. Built with performance, thread-safety, and developer experience in mind.

## 🎯 Overview

BlazorSupercookie implements the supercookie concept - a tracking method that uses browser favicon cache to create persistent identifiers. Unlike traditional tracking methods, these identifiers survive cache clearing, browser restarts, and even incognito mode.

**⚠️ Warning**: This library is for **educational and demonstration purposes only**. Use responsibly and only with explicit user consent.

## ✨ Features

- 🚀 **High Performance**: Zero-allocation patterns, optimized for minimal memory usage
- 🔒 **Thread-Safe**: Built with concurrency in mind using modern C# patterns
- 🎯 **Developer Experience**: Simple, intuitive API with comprehensive documentation
- 📦 **NuGet Ready**: Fully configured for NuGet package distribution
- 🧪 **Well Tested**: Comprehensive unit tests included
- 🎨 **Modern UI**: Beautiful, responsive component with progress indicators
- 🌐 **GitHub Pages Demo**: Working demo deployable to GitHub Pages

## 🏗️ Project Structure

```
BlazorSupercookie/
├── src/
│   ├── BlazorSupercookie/          # Main component library
│   └── BlazorSupercookie.Demo/      # Demo Blazor WebAssembly app
├── tests/
│   └── BlazorSupercookie.Tests/    # Unit tests
├── .github/
│   └── workflows/                    # CI/CD pipelines
└── README.md
```

## 🚀 Quick Start

### Installation

```bash
dotnet add package BlazorSupercookie
```

### Usage

```razor
@using BlazorSupercookie

<SupercookieComponent 
    CacheIdentifier="my-app"
    RouteCount="32"
    BaseUrl="/"
    OnFingerprintComplete="HandleFingerprint"
    @ref="_component" />

@code {
    private SupercookieComponent? _component;

    private async Task HandleFingerprint(ulong? identifier)
    {
        if (identifier.HasValue)
        {
            Console.WriteLine($"Fingerprint: {identifier.Value}");
        }
    }

    private async Task ReadFingerprint()
    {
        var identifier = await _component?.ReadFingerprintAsync();
    }

    private async Task WriteFingerprint(ulong identifier)
    {
        await _component?.WriteFingerprintAsync(identifier);
    }
}
```

## 🧪 Running the Demo

```bash
cd src/BlazorSupercookie.Demo
dotnet run
```

Navigate to `https://localhost:5001` to see the demo in action.

## 📚 Documentation

See the [component library README](src/BlazorSupercookie/README.md) for detailed API documentation.

## 🔧 Development

### Prerequisites

- .NET 10.0 SDK or later
- Modern browser (Chrome, Firefox, Edge, Safari)

### Building

```bash
dotnet build
```

### Testing

```bash
dotnet test
```

### Creating NuGet Package

```bash
dotnet pack -c Release
```

## 🌐 GitHub Pages Deployment

The demo can be deployed to GitHub Pages using the included GitHub Actions workflow. The workflow:

1. Builds the Blazor WebAssembly app
2. Publishes the output
3. Deploys to GitHub Pages

**Note**: The demo requires a backend API to serve favicons. Deploy the `BlazorSupercookie.Api` project separately and configure the `FAVICON_API_URL` environment variable.

See [DEPLOYMENT.md](DEPLOYMENT.md) for detailed deployment instructions.

## 🏛️ Architecture

### Core Components

- **FingerprintEngine**: Core engine for fingerprint operations (thread-safe, zero-allocation)
- **FingerprintService**: Service for managing fingerprint sessions (concurrent dictionary)
- **FingerprintSession**: Represents a single fingerprinting session
- **SupercookieComponent**: Blazor component with JS interop for favicon manipulation

### Design Principles

- **Thread Safety**: All public APIs are thread-safe
- **Zero Allocation**: Minimizes allocations in hot paths
- **Fail Fast**: Validates inputs early and throws descriptive exceptions
- **Modern C#**: Uses latest language features (records, pattern matching, etc.)

## 📝 License

MIT License - see [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

Inspired by the research paper from University of Illinois, Chicago:
[www.cs.uic.edu](https://www.cs.uic.edu/~polakis/papers/solomos-ndss21.pdf)

Original supercookie implementation:
[github.com/jonasstrehle/supercookie](https://github.com/jonasstrehle/supercookie)

## ⚖️ Ethical Considerations

This library demonstrates a privacy-invasive tracking technique. Please:

- Only use with explicit user consent
- Follow privacy regulations (GDPR, CCPA, etc.)
- Be transparent about tracking practices
- Consider privacy-preserving alternatives

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📧 Support

For issues and questions, please use the GitHub issue tracker.
