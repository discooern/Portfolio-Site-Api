using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PortfolioAPI.Models
{
    [Index(nameof(Timestamp))]
    [Index(nameof(CorrelationId))]
    [Index(nameof(StatusCode))]
    public class EventLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid CorrelationId { get; set; }

        [Required]
        [MaxLength(10)]
        public string Direction { get; set; } = default!;

        [Required]
        [MaxLength(30)]
        public string Level { get; set; } = default!;

        [Required]
        [MaxLength(20)]
        public string Method { get; set; } = default!;

        [Required]
        [MaxLength(100)]
        public string Path { get; set; } = default!;

        public int? StatusCode { get; set; }

        public string? QueryParameters { get; set; }

        public string? Body { get; set; }

        public int? BodySize { get; set; }

        public long? ElapsedMilliseconds { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
