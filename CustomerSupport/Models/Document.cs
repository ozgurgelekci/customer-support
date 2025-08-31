using System.ComponentModel.DataAnnotations;

namespace CustomerSupport.Models;

public class Document
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(50)]
    public string? FileType { get; set; }

    [MaxLength(1000)]
    public string? SourceUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Metadata as JSON
    public string? Metadata { get; set; }

    // Navigation property
    public virtual ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
}

public class DocumentChunk
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid DocumentId { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public int ChunkIndex { get; set; }

    public int StartPosition { get; set; }
    public int EndPosition { get; set; }

    // Embedding vector - stored as comma-separated floats
    public string? EmbeddingVector { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual Document Document { get; set; } = null!;
}

// RAG related DTOs
public class DocumentUploadRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? FileType { get; set; }
    public string? SourceUrl { get; set; }
    public string? Metadata { get; set; }
}

public class DocumentSearchRequest
{
    public string Query { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int TopK { get; set; } = 5;
    public double SimilarityThreshold { get; set; } = 0.7;
}

public class DocumentSearchResult
{
    public Guid DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public double SimilarityScore { get; set; }
    public int ChunkIndex { get; set; }
}
