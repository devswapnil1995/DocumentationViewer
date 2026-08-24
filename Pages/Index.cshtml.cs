using DocumentationViewer.Models;
using DocumentationViewer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocumentationViewer.Pages;

public class IndexModel : PageModel
{
    private readonly MarkdownService _markdownService;

    [BindProperty]
    public string SearchQuery { get; set; }

    public List<DocumentInfo> Documents { get; set; } = new();

    public IndexModel(MarkdownService markdownService)
    {
        _markdownService = markdownService;
    }

    public void OnGet(string search)
    {
        SearchQuery = search;
        LoadDocuments();
    }

    public void OnPost()
    {
        LoadDocuments();
    }

    private void LoadDocuments()
    {
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            Documents = _markdownService.SearchDocuments(SearchQuery);
        }
        else
        {
            Documents = _markdownService.GetAllDocuments();
        }
    }
}
