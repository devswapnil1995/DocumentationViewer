namespace DocumentationViewer.Models
{
    /// <summary>
    /// Represents metadata for a documentation file.
    /// </summary>
    public class DocumentInfo
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string HtmlContent { get; set; }
        public DateTime LastModified { get; set; }
        public long FileSize { get; set; }
    }
}
