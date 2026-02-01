# AGENTS.md - CFG2Lib Developer Guide

This document provides guidance for AI agents and developers working with the CFG2Lib repository.

## Repository Overview

**CFG2Lib** is a modular collection of .NET class libraries (DLLs) designed for reusable components in .NET applications. Each library can be used independently or together.

- **Language:** C# / .NET 9.0
- **License:** MIT
- **Repository:** https://github.com/cfg2-com/CFG2Lib
- **Build System:** MSBuild, dotnet CLI
- **Testing Framework:** xUnit 2.9.2

## Library Structure

The repository contains the following libraries:

| Library | Purpose | Dependencies | NuGet Package |
|---------|---------|--------------|---------------|
| **SysLib** | System utilities (filesystem, environment, special folders, domain objects) | None | CFG2.SysLib |
| **LogLib** | Simple logging system with file output | SysLib | CFG2.LogLib |
| **HttpLib** | HTTP/web client utilities | LogLib | CFG2.HttpLib |
| **SecLib** | Security functions (PBKDF2, DPAPI, password algorithms) | LogLib | CFG2.SecLib |
| **SQLiteLib** | SQLite database wrapper with CRUD operations | None | CFG2.SQLiteLib |
| **AppLib** | Integrated application framework | SysLib, LogLib, SecLib, SQLiteLib | CFG2.AppLib |
| **TestApp** | Reference application demonstrating library usage | AppLib, HttpLib | N/A |

### Test Projects
- **SysLib.Tests** - xUnit tests for SysLib
- **HttpLib.Tests** - xUnit tests for HttpLib

## Building the Project

### Using Batch Scripts (Windows)
```batch
# Build a specific library
bSysLib.bat
bLogLib.bat
bHttpLib.bat
# ... etc

# Clean all build outputs
clean.bat
```

### Using dotnet CLI
```bash
# Build entire solution
dotnet build CFG2Lib.sln

# Build specific project
dotnet build SysLib/SysLib.csproj

# Build for release (without project references)
dotnet build SysLib/SysLib.csproj -p:UseProjectReferences=false

# Create NuGet packages
dotnet build -c Release
```

### Build Process Notes
1. `setRelease.bat` manages version variables for all libraries
2. Individual `b*.bat` scripts call `setRelease.bat` then invoke `dotnet build`
3. Projects automatically generate NuGet packages on build (`GeneratePackageOnBuild=true`)

## Running Tests

```bash
# Run all tests
dotnet test CFG2Lib.sln

# Run specific test project
dotnet test SysLib.Tests/SysLib.Tests.csproj
dotnet test HttpLib.Tests/HttpLib.Tests.csproj

# Run with verbose output
dotnet test --logger:console --verbosity:detailed

# Generate code coverage
dotnet test /p:CollectCoverage=true
```

### Test Conventions
- Use xUnit `[Fact]` attribute for test methods
- Test classes should implement `IDisposable` for cleanup
- Follow Arrange-Act-Assert (AAA) pattern
- Test namespace: `CFG2.Utils.{LibName}.Tests`
- Method naming: `MethodName_ExpectedBehavior_ExpectedResult`

## Coding Conventions

### C# Language Features
- **ImplicitUsings:** Enabled
- **Nullable:** Enabled
- **Target Framework:** .NET 9.0

### Namespace Convention
```csharp
namespace CFG2.Utils.{LibName};
namespace CFG2.Utils.{LibName}.Tests;  // For tests
```

### Code Style
```csharp
// Use modern C# syntax
private static readonly Dictionary<SpecialFolder, Guid> _guids = new() { ... };

// XML documentation comments for public APIs
/// <summary>
/// Returns true if a file is in use/locked
/// </summary>
/// <param name="file"></param>
/// <returns></returns>
public static bool IsFileLocked(string file) { ... }

// Pragma suppression for known warnings (use sparingly)
#pragma warning disable CS8603
// code
#pragma warning restore CS8603
```

### Class Organization
1. Namespace declaration
2. Using statements
3. Class declaration
4. Private static fields
5. Constructors
6. Public methods
7. Private methods

### Naming Patterns
- Utility classes: `*Utils.cs` (e.g., `SysUtils.cs`, `HttpUtils.cs`)
- Domain objects: Located in `Domain/` subdirectory
- Assembly names: `CFG2.{LibName}`

## Dependencies and Versioning

### Version Management
All library versions are managed in `setRelease.bat`:
```batch
SET SYS_LIB_VER=1.0.13
SET LOG_LIB_VER=1.0.5
SET HTTP_LIB_VER=1.0.5
SET APP_LIB_VER=1.0.24
SET SEC_LIB_VER=1.0.3
SET SQLITE_LIB_VER=1.0.1
```

### Conditional Dependencies
Projects use conditional compilation for flexible dependency management:
```xml
<!-- During development, use project references -->
<ItemGroup Condition="'$(UseProjectReferences)' == 'true'">
  <ProjectReference Include="..\SysLib\SysLib.csproj" />
</ItemGroup>

<!-- During packaging, use NuGet packages -->
<ItemGroup Condition="'$(UseProjectReferences)' != 'true'">
  <PackageReference Include="CFG2.SysLib" Version="[$(SYS_LIB_VER),)" />
</ItemGroup>
```

### NuGet Publishing
- All libraries are published to NuGet.org
- Package IDs follow pattern: `CFG2.{LibName}`
- Packages are generated automatically on build
- Follow semantic versioning (major.minor.patch)

## Documentation

### README Files
Each library has its own README.md in the library root directory containing:
- Library description and philosophy
- Usage examples with code
- Release notes with version history
- Dependencies and installation instructions

### Documentation Standards
- Use Markdown format
- Include practical code examples
- Document breaking changes in release notes
- Link to NuGet package pages where relevant
- Highlight library dependencies clearly

## Project Structure

### Typical Library Layout
```
{LibName}/
├── README.md              # Library documentation
├── {LibName}.csproj       # Project file
└── src/                   # Source code (optional)
    ├── {ClassName}.cs
    ├── {UtilName}Utils.cs
    └── Domain/            # Domain objects
        └── {DomainObject}.cs
```

### .csproj Properties
Key properties to maintain:
```xml
<TargetFramework>net9.0</TargetFramework>
<ImplicitUsings>enable</ImplicitUsings>
<Nullable>enable</Nullable>
<AssemblyName>CFG2.{LibName}</AssemblyName>
<GeneratePackageOnBuild>true</GeneratePackageOnBuild>
<RepositoryUrl>https://github.com/cfg2-com/CFG2Lib</RepositoryUrl>
<PackageReadmeFile>README.md</PackageReadmeFile>
<PackageLicenseExpression>MIT</PackageLicenseExpression>
```

## Key Architectural Decisions

1. **Modular Design:** Each library is independent; compose as needed
2. **Opinionated Defaults:** Libraries favor simplicity with sensible defaults
3. **Minimal External Dependencies:** SysLib and SQLiteLib have zero external dependencies
4. **Automatic Packaging:** NuGet packages generated on every build
5. **Version Centralization:** All versions managed in `setRelease.bat`
6. **Conditional Project References:** Supports both development and published package modes
7. **Semantic Versioning:** All libraries follow major.minor.patch versioning

## Best Practices for Making Changes

### Before Making Changes
1. Run existing tests to establish baseline: `dotnet test`
2. Understand the library's dependencies and dependents
3. Review existing documentation and conventions

### Making Changes
1. Make minimal, focused changes
2. Follow existing code style and patterns
3. Add or update XML documentation comments for public APIs
4. Update README.md if adding new features or breaking changes
5. Update version in `setRelease.bat` if needed

### After Making Changes
1. Run tests: `dotnet test {LibName}.Tests/{LibName}.Tests.csproj`
2. Build the library: `dotnet build {LibName}/{LibName}.csproj`
3. Verify NuGet package generation
4. Update release notes in README.md
5. Test with TestApp if making significant changes

### Adding New Tests
1. Add test class to appropriate `*.Tests` project
2. Implement `IDisposable` for test setup/teardown
3. Use `[Fact]` attribute for test methods
4. Follow AAA (Arrange-Act-Assert) pattern
5. Use descriptive test method names

### Adding New Libraries
1. Follow existing naming conventions: `CFG2.{LibName}`
2. Create corresponding `.csproj` with standard properties
3. Add README.md with library documentation
4. Create test project: `{LibName}.Tests`
5. Add build script: `b{LibName}.bat`
6. Update `setRelease.bat` with version variable
7. Add entry to main README.md

## Common Tasks

### Update Library Version
1. Edit `setRelease.bat` to update version variable
2. Update release notes in library's README.md
3. Rebuild: `b{LibName}.bat` or `dotnet build`

### Add New Dependency
1. Update `.csproj` with both conditional and unconditional references
2. Update README.md to document new dependency
3. Test both development and package build modes

### Fix a Bug
1. Write a failing test that reproduces the bug
2. Fix the bug with minimal changes
3. Verify test passes
4. Update version if needed (patch increment)

### Add New Feature
1. Design API following existing patterns
2. Implement feature with tests
3. Add XML documentation comments
4. Update README.md with examples
5. Increment version appropriately (minor or major)

## Troubleshooting

### Build Failures
- Ensure `setRelease.bat` has been executed
- Check for missing dependencies
- Verify .NET 9.0 SDK is installed
- Clean and rebuild: `clean.bat` then `dotnet build`

### Test Failures
- Run tests individually to isolate issues
- Check for environment-specific dependencies
- Verify test data and fixtures are available
- Use `--verbosity:detailed` for more information

### Package Generation Issues
- Ensure `GeneratePackageOnBuild` is `true`
- Verify version variables are set correctly
- Check for `.csproj` property errors
- Build in Release configuration for final packages

## Additional Resources

- Main README: `/README.md`
- Library READMEs: `/{LibName}/README.md`
- Solution File: `/CFG2Lib.sln`
- License: `/LICENSE` (MIT)
- GitHub: https://github.com/cfg2-com/CFG2Lib
- NuGet Packages: Search for "CFG2" on NuGet.org

## Notes for AI Agents

- **Always** run tests before and after making changes
- **Follow** existing patterns and conventions strictly
- **Update** documentation when changing public APIs
- **Use** batch scripts for building when on Windows
- **Test** with both project references and NuGet packages
- **Increment** versions appropriately for changes
- **Maintain** modular design - avoid tight coupling between libraries
- **Document** breaking changes clearly in README release notes
