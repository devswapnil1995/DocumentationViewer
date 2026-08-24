using DocumentationViewer.Models;
using DocumentationViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocumentationViewer.Pages
{
    public class ViewPageModel : PageModel
    {
        private readonly MarkdownService _markdownService;

        public DocumentInfo Document { get; set; }
        public List<(int Level, string Text, string Id)> Headings { get; set; } = new();

        public ViewPageModel(MarkdownService markdownService)
        {
            _markdownService = markdownService;
        }

        public IActionResult OnGet(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                return RedirectToPage("Index");
            }

            Document = _markdownService.GetDocument(file);
            if (Document == null)
            {
                return NotFound();
            }

            Headings = _markdownService.ExtractHeadings(Document.Content);
            return Page();
        }
    }
}
