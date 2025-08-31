using System.ComponentModel.DataAnnotations;

namespace CustomerSupport.Models;

public class Conversation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? EndedAt { get; set; }

    // Navigation property
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
