using CustomerSupport.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerSupport.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Conversation> Conversations { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<DocumentChunk> DocumentChunks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Conversation configuration
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StartedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
            entity.Property(e => e.EndedAt)
                .IsRequired(false);

            // One-to-many relationship with Messages
            entity.HasMany(e => e.Messages)
                .WithOne(e => e.Conversation)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Message configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConversationId)
                .IsRequired();
            entity.Property(e => e.Sender)
                .IsRequired()
                .HasConversion<string>();
            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("TEXT");
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            // Foreign key relationship
            entity.HasOne(e => e.Conversation)
                .WithMany(e => e.Messages)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Document configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("TEXT");
            entity.Property(e => e.Category)
                .HasMaxLength(100);
            entity.Property(e => e.FileType)
                .HasMaxLength(50);
            entity.Property(e => e.SourceUrl)
                .HasMaxLength(1000);
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt)
                .IsRequired();
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            entity.Property(e => e.Metadata)
                .HasColumnType("TEXT");

            // One-to-many relationship with DocumentChunks
            entity.HasMany(e => e.Chunks)
                .WithOne(e => e.Document)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for faster searches
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
        });

        // DocumentChunk configuration
        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentId)
                .IsRequired();
            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("TEXT");
            entity.Property(e => e.ChunkIndex)
                .IsRequired();
            entity.Property(e => e.StartPosition)
                .IsRequired();
            entity.Property(e => e.EndPosition)
                .IsRequired();
            entity.Property(e => e.EmbeddingVector)
                .HasColumnType("TEXT");
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            // Foreign key relationship
            entity.HasOne(e => e.Document)
                .WithMany(e => e.Chunks)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for faster searches
            entity.HasIndex(e => e.DocumentId);
            entity.HasIndex(e => e.ChunkIndex);
        });
    }
}
