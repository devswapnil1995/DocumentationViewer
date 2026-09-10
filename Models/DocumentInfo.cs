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

        /// <summary>
        /// Extracts the numerical step number from the title for proper sorting.
        /// Example: "Step 10 - Configuration" returns 10
        /// </summary>
        public int GetSortOrder()
        {
            if (string.IsNullOrEmpty(Title))
                return int.MaxValue;

            // Try to extract "Step X" pattern
            var match = System.Text.RegularExpressions.Regex.Match(Title, @"Step\s+(\d+)", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (match.Success && int.TryParse(match.Groups[1].Value, out var stepNumber))
            {
                return stepNumber;
            }

            // Fallback: try to extract any leading number
            match = System.Text.RegularExpressions.Regex.Match(Title, @"^\s*(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var number))
            {
                return number;
            }

            // If no number found, sort alphabetically
            return int.MaxValue;
        }
    }
}
