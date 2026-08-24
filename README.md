# Documentation Viewer

A modern, web-based markdown documentation viewer built with ASP.NET Core Razor Pages.

## Features

✨ **Core Features:**
- 📄 **Markdown Rendering** - Beautiful HTML rendering of markdown files
- 🔍 **Full-Text Search** - Search across all documentation
- 📋 **Table of Contents** - Auto-generated TOC from headings
- 💾 **Syntax Highlighting** - Code blocks with language-specific highlighting (via Highlight.js)
- 📋 **Copy to Clipboard** - Easy copy button for code blocks
- 🎨 **Responsive Design** - Works on desktop, tablet, and mobile
- 📱 **Mobile Friendly** - Touch-optimized interface

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio Community 2026 (or VS Code)

### Running the Application

1. **From Visual Studio**
   - Open `TopicDemoApp.slnx` in Visual Studio
   - In Solution Explorer, right-click on **DocumentationViewer** → Set as Startup Project
   - Press **F5** to run
   - The app will open in your browser at `https://localhost:5001`

2. **From Command Line**
   ```powershell
   cd D:\Swapnil\Projects\TopicDemoApp\DocumentationViewer
   dotnet run
   ```
   - Navigate to `https://localhost:5001` in your browser

### Configuration

By default, the viewer looks for markdown files in the `Docs` folder at the parent directory of the application.

To customize the docs path, edit `appsettings.json`:

```json
{
  "DocsPath": "C:/path/to/your/docs"
}
```

## Project Structure

```
DocumentationViewer/
├── Models/
│   └── DocumentInfo.cs          # Document metadata model
├── Services/
│   └── MarkdownService.cs       # Markdown parsing & conversion
├── Pages/
│   ├── Index.cshtml             # Documentation listing & search
│   ├── Index.cshtml.cs          # Index page model
│   ├── View.cshtml              # Markdown viewer with TOC
│   └── View.cshtml.cs           # View page model
├── Shared/
│   └── _Layout.cshtml           # Master layout (styling)
├── Program.cs                   # Application startup
└── DocumentationViewer.csproj   # Project file
```

## How It Works

### 1. **Documentation Discovery**
- Scans the `Docs` folder for `.md` files
- Extracts metadata (title, size, modification date)

### 2. **Rendering**
- Uses **Markdig** library to convert markdown to HTML
- Applies Bootstrap styling for clean presentation

### 3. **Table of Contents**
- Extracts all headings (H1-H6) from markdown
- Creates clickable navigation sidebar
- Smooth scrolling to sections

### 4. **Search**
- Full-text search across all documentation
- Case-insensitive matching
- Instant results

### 5. **Syntax Highlighting**
- Detects code blocks and language
- Highlights using Highlight.js (from CDN)
- One-click copy button for code

## Usage

### Viewing Documentation

1. **Home Page** - Lists all markdown files in a searchable grid
2. **Document View** - Click any document to view it
3. **Table of Contents** - Navigate to any section via sidebar TOC
4. **Search** - Use search bar to find content across all docs

### Example: Adding New Documentation

1. Create or edit a `.md` file in the `Docs` folder
2. Refresh the browser - new content appears automatically
3. Include markdown headings for automatic TOC generation

```markdown
# Document Title

## Section 1
Content here...

### Subsection
More content...

## Section 2
...
```

## Dependencies

- **Markdig** (v1.3.2) - Markdown to HTML conversion with extensions
- **Bootstrap 5** - Responsive CSS framework
- **Highlight.js** - Code syntax highlighting (CDN)
- **Font Awesome 6** - Icons (CDN)

## Hosting

### Local Development
```powershell
dotnet run
```

### Production Deployment

**Docker:**
```bash
dotnet publish -c Release
# Then containerize and deploy
```

**Azure App Service:**
```powershell
dotnet publish -c Release -o ./publish
# Upload ./publish to Azure App Service
```

**IIS:**
```powershell
dotnet publish -c Release -o "./bin/Release/publish"
# Copy to IIS web root
```

## Features Coming Soon

- 📌 Pinned/favorite documents
- 🏷️ Tag-based categorization
- 📝 In-browser editing
- 💬 Comment/discussion threads
- 📊 View analytics
- 🌙 Dark mode toggle

## Troubleshooting

**Q: No documents appear**
- Ensure markdown files (`.md`) are in the `Docs` folder
- Check `appsettings.json` `DocsPath` setting
- Verify file permissions

**Q: Markdown not rendering correctly**
- Some markdown extensions may not render as expected
- Markdig supports CommonMark and extensions like tables, strikethrough, etc.
- Check the Markdig documentation for supported syntax

**Q: Code highlighting not working**
- Ensure internet connection for Highlight.js CDN
- Check browser console for JavaScript errors

## License

This project is free to use and modify for your documentation needs.

## Support

For issues or questions, refer to the project documentation or GitHub repository.

---

**Built with ❤️ for DocumentationViewer**
