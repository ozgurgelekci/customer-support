using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerSupport.Models;

public class Message
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [ForeignKey("Conversation")]
    public Guid ConversationId { get; set; }

    [Required]
    public MessageSender Sender { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual Conversation Conversation { get; set; } = null!;
}

public enum MessageSender
{
    User,
    AI
}
