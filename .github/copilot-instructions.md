# BlakePlugin.DocsRenderer

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

BlakePlugin.DocsRenderer is a .NET 9.0 Blazor Razor library plugin for Blake static site generator that adds documentation rendering capabilities including scrollspy navigation, table of contents (TOC) generation, and enhanced Prism.js code syntax highlighting.

## Working Effectively

### Prerequisites and Environment Setup
- **CRITICAL**: Install .NET 9.0 SDK if not present. The project targets net9.0 and will not build with older versions.
- Download and install .NET 9.0: `wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && chmod +x dotnet-install.sh && ./dotnet-install.sh --version 9.0.101`
- Set environment variables: `export PATH="/home/runner/.dotnet:$PATH"` and `export DOTNET_ROOT="/home/runner/.dotnet"`
- Verify installation: `dotnet --version` should return 9.0.101 or higher

### Build and Package Commands
- **Restore packages**: `dotnet restore` -- takes ~9 seconds. NEVER CANCEL. Set timeout to 3+ minutes.
- **Build (Debug)**: `dotnet build` -- takes ~8.7 seconds. NEVER CANCEL. Set timeout to 3+ minutes.
- **Build (Release)**: `dotnet build --configuration Release` -- takes ~8.7 seconds. NEVER CANCEL. Set timeout to 3+ minutes.
- **Create NuGet package**: `dotnet pack --configuration Release` -- takes ~2.2 seconds. Set timeout to 2+ minutes.

### Code Quality and Validation
- **Format validation**: `dotnet format --verify-no-changes` -- currently fails with 25+ whitespace formatting errors. This is a known issue but does NOT prevent building.
- **Apply code formatting**: `dotnet format` -- fixes whitespace issues automatically. Run this before committing changes.
- **No unit tests exist**: `dotnet test` passes but runs zero tests. This is expected - the project has no test projects.

### Manual Validation and Testing
Since this is a Blake plugin library, validation requires integrating with the Blake ecosystem:

1. **Install Blake CLI**: `dotnet tool install --global blake.cli`
2. **Set Blake environment**: Ensure `export DOTNET_ROOT="/home/runner/.dotnet"` and `export PATH="/home/runner/.dotnet:/home/runner/.dotnet/tools:$PATH"`
3. **Create test site**: `blake new test-site`
4. **Add plugin reference**: Edit the test-site.csproj to include `<ProjectReference Include="[path-to-plugin]/src/BlakePlugin.DocsRenderer.csproj" />`
5. **Test plugin**: `blake bake test-site` -- should complete successfully
6. **Serve site**: `blake serve test-site` -- validates full integration

### CI Validation Commands
- The GitHub Actions workflow in `.github/workflows/ci.yml` runs the same build commands
- **Always run** `dotnet format` before committing to avoid CI failures
- **Always test** your changes by running the full build pipeline: restore → build → pack

## Common Tasks and File Locations

### Key Project Structure
```
/
├── src/                           # Main source directory
│   ├── BlakePlugin.DocsRenderer.csproj  # Main project file (net9.0)
│   ├── Plugin.cs                  # Main plugin entry point
│   ├── Components/                # Razor components (PageToc, SiteToc, etc.)
│   ├── MarkdownExtensions/        # Markdig extensions for document processing
│   ├── Types/                     # Section, TocNode data types
│   ├── Utils/                     # TocUtils helper class
│   └── wwwroot/lib/               # Static assets (CSS, JS, Prism.js)
├── .github/workflows/ci.yml       # CI build pipeline
├── BlakePlugin.DocsRenderer.sln   # Visual Studio solution
└── README.md                      # Usage documentation
```

### Critical Build Times and Timeouts
- **NEVER CANCEL BUILDS**: All operations complete within 10 seconds under normal conditions
- **Set minimum 3-minute timeouts** for all dotnet commands to handle slow networks or first-time package downloads
- **Expected timing**:
  - `dotnet restore`: ~9 seconds (first time may take longer)
  - `dotnet build`: ~8.7 seconds  
  - `dotnet pack`: ~2.2 seconds
  - `dotnet format`: ~10 seconds

### Known Issues and Limitations
1. **Code formatting**: Project has whitespace formatting issues that must be fixed with `dotnet format` before committing
2. **No tests**: Zero unit tests exist. Validation requires manual Blake integration testing
3. **Environment dependency**: Requires exact .NET 9.0 - will not build with .NET 8.0 or earlier
4. **Blake dependency**: Runtime testing requires Blake CLI and understanding of Blake static site generator workflow

### Plugin Architecture
This plugin extends Blake's Markdig pipeline with:
- **Document sectioning**: Automatically generates sections from headings
- **TOC generation**: Creates hierarchical table of contents  
- **Prism.js integration**: Enhanced code syntax highlighting with line numbers, copy buttons, and diff highlighting
- **Razor components**: Ready-to-use SiteToc and PageToc Blazor components

### Dependencies
- **Blake.BuildTools**: Core Blake plugin interface and build tools
- **Microsoft.AspNetCore.Components.Web**: Blazor component framework
- **Targets**: net9.0 framework only

## Validation Scenarios

### After Making Changes
1. **Code formatting**: Run `dotnet format` to fix whitespace issues
2. **Build validation**: Run `dotnet build --configuration Release` to ensure compilation succeeds
3. **Package creation**: Run `dotnet pack --configuration Release` to verify NuGet package generation
4. **Integration test**: Create a simple Blake site and verify the plugin loads and functions correctly
5. **Manual verification**: Check that generated TOCs, sections, and code highlighting work as expected in a Blake site

Always build and test your changes before committing. The CI pipeline will fail if code formatting is incorrect or if the build fails.