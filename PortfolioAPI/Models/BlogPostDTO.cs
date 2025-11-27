namespace PortfolioAPI.Models
{
    public class BlogPostDTO
    {
        public string? Title { get; set; }

        public string? Slug { get; set; }

        public string? ContentJson { get; set; }

        public string? Summary { get; set; }

        public bool IsPublished { get; set; } = false;

        public int? AuthorId { get; set; }
    }
}
