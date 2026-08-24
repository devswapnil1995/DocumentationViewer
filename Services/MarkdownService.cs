using DocumentationViewer.Models;
using Markdig;
using System.Text.RegularExpressions;

namespace DocumentationViewer.Services
{
    /// <summary>
    /// Service for reading, processing, and converting markdown files to HTML.
    /// </summary>
    public class MarkdownService
    {
        private readonly string _docsPath;
        private readonly MarkdownPipeline _pipeline;

        public MarkdownService(IConfiguration configuration)
        {
            // Get docs path from configuration or default to Docs folder in parent directory
            _docsPath = configuration["DocsPath"] ?? Path.Combine(Directory.GetParent(AppContext.BaseDirectory).FullName, "Docs");

            // Configure Markdig pipeline with extensions
            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
        }

        /// <summary>
        /// Get all markdown files from the docs directory.
        /// </summary>
        public List<DocumentInfo> GetAllDocuments()
        {
            var documents = new List<DocumentInfo>();

            if (!Directory.Exists(_docsPath))
                return documents;

            var files = Directory.GetFiles(_docsPath, "*.md", SearchOption.AllDirectories);

            foreach (var file in files.OrderBy(f => f))
            {
                try
                {
                    var info = GetDocumentInfo(file);
                    if (info != null)
                        documents.Add(info);
                }
                catch
                {
                    // Skip files that can't be read
                }
            }

            return documents;
        }

        /// <summary>
        /// Get a specific document by file name.
        /// </summary>
        public DocumentInfo GetDocument(string fileName)
        {
            var filePath = Path.Combine(_docsPath, fileName);

            // Security: prevent directory traversal
            var fullPath = Path.GetFullPath(filePath);
            var fullDocsPath = Path.GetFullPath(_docsPath);
            if (!fullPath.StartsWith(fullDocsPath))
                return null;

            if (!File.Exists(fullPath))
                return null;

            return GetDocumentInfo(fullPath);
        }

        /// <summary>
        /// Search documents by keyword.
        /// </summary>
        public List<DocumentInfo> SearchDocuments(string keyword)
        {
            var results = new List<DocumentInfo>();

            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
                return results;

            var documents = GetAllDocuments();
            var lowerKeyword = keyword.ToLower();

            foreach (var doc in documents)
            {
                if (doc.Content.Contains(lowerKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(doc);
                }
            }

            return results;
        }

        /// <summary>
        /// Extract heading hierarchy from markdown for table of contents.
        /// </summary>
        public List<(int Level, string Text, string Id)> ExtractHeadings(string markdownContent)
        {
            var headings = new List<(int, string, string)>();
            var lines = markdownContent.Split('\n');
            int headingIndex = 0;

            foreach (var line in lines)
            {
                var match = Regex.Match(line, @"^(#{1,6})\s+(.+)$");
                if (match.Success)
                {
                    var level = match.Groups[1].Value.Length;
                    var text = match.Groups[2].Value;
                    var id = $"heading-{headingIndex++}";
                    headings.Add((level, text, id));
                }
            }

            return headings;
        }

        /// <summary>
        /// Get document information and convert markdown to HTML.
        /// </summary>
        private DocumentInfo GetDocumentInfo(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var content = File.ReadAllText(filePath);

            // Extract title from first heading or filename
            var title = ExtractTitle(content);
            if (string.IsNullOrEmpty(title))
                title = Path.GetFileNameWithoutExtension(filePath);

            // Convert markdown to HTML
            var htmlContent = Markdown.ToHtml(content, _pipeline);

            return new DocumentInfo
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                Title = title,
                Content = content,
                HtmlContent = htmlContent,
                LastModified = fileInfo.LastWriteTime,
                FileSize = fileInfo.Length
            };
        }

        /// <summary>
        /// Extract title from markdown (first H1 heading).
        /// </summary>
        private string ExtractTitle(string markdownContent)
        {
            var match = Regex.Match(markdownContent, @"^#\s+(.+)$", RegexOptions.Multiline);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        /// <summary>
        /// Get relative path for display.
        /// </summary>
        public string GetRelativePath(string filePath)
        {
            var fullDocsPath = Path.GetFullPath(_docsPath);
            var fullPath = Path.GetFullPath(filePath);
            var relativePath = Path.GetRelativePath(fullDocsPath, fullPath);
            return relativePath;
        }
    }
}
