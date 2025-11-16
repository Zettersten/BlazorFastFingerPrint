# Contributing to BlazorSupercookie

Thank you for your interest in contributing to BlazorSupercookie! This document provides guidelines and instructions for contributing.

## Code of Conduct

- Be respectful and inclusive
- Welcome newcomers and help them learn
- Focus on constructive feedback
- Respect different viewpoints and experiences

## Development Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/BlazorSupercookie.git
   cd BlazorSupercookie
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

4. Run tests:
   ```bash
   dotnet test
   ```

## Coding Standards

- Follow SOLID and DRY principles
- Write thread-safe code
- Minimize allocations in hot paths
- Use modern C# features (pattern matching, records, etc.)
- Write concise comments for top-level functions
- Prefer fail-fast techniques
- Use shorthand syntax where appropriate

## Code Style

- Use 4 spaces for indentation
- Use `var` when type is obvious
- Use expression-bodied members when appropriate
- Prefer `is not null` over `!= null`
- Use file-scoped namespaces
- Use `ArgumentNullException.ThrowIfNull()` for null checks

## Testing

- Write unit tests for all public APIs
- Aim for high code coverage
- Use descriptive test names
- Follow Arrange-Act-Assert pattern
- Test edge cases and error conditions

## Pull Request Process

1. Create a feature branch from `main`
2. Make your changes
3. Write or update tests
4. Ensure all tests pass
5. Update documentation if needed
6. Submit a pull request

### PR Checklist

- [ ] Code follows style guidelines
- [ ] Tests pass locally
- [ ] Documentation updated
- [ ] CHANGELOG.md updated (if applicable)
- [ ] No breaking changes (or documented)

## Commit Messages

Use clear, descriptive commit messages:

```
feat: Add support for custom cache identifiers
fix: Resolve thread-safety issue in FingerprintService
docs: Update README with deployment instructions
test: Add unit tests for FingerprintEngine
refactor: Optimize route generation algorithm
```

## Project Structure

```
BlazorSupercookie/
├── src/
│   ├── BlazorSupercookie/          # Main component library
│   ├── BlazorSupercookie.Demo/     # Demo application
│   └── BlazorSupercookie.Api/      # Backend API
├── tests/
│   └── BlazorSupercookie.Tests/    # Unit tests
└── .github/
    └── workflows/                   # CI/CD pipelines
```

## Areas for Contribution

- Performance optimizations
- Additional test coverage
- Documentation improvements
- Bug fixes
- Feature enhancements
- Examples and samples

## Questions?

Feel free to open an issue for questions or discussions.

Thank you for contributing! 🎉
