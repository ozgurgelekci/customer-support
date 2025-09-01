using CustomerSupport.Models;

namespace CustomerSupport.Services;

public interface IDocumentProcessingService
{
    Task<Document> ProcessDocumentAsync(DocumentUploadRequest request);
    Task<List<DocumentChunk>> CreateChunksAsync(Document document);
    Task<List<DocumentSearchResult>> SearchSimilarDocumentsAsync(DocumentSearchRequest request);
    Task<bool> ReprocessDocumentAsync(Guid documentId);
    Task<bool> DeleteDocumentAsync(Guid documentId);
}
