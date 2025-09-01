using CustomerSupport.Models;

namespace CustomerSupport.Services;

public interface IRagService
{
    Task<string> GenerateContextAwareResponseAsync(string userQuery, List<Message> conversationHistory);
    Task<List<DocumentSearchResult>> RetrieveRelevantDocumentsAsync(string query, int maxResults = 5);
    Task<string> GenerateResponseWithContextAsync(string userQuery, List<DocumentSearchResult> relevantDocs, List<Message> conversationHistory);
}

public class RagContext
{
    public string UserQuery { get; set; } = string.Empty;
    public List<DocumentSearchResult> RelevantDocuments { get; set; } = new();
    public List<Message> ConversationHistory { get; set; } = new();
    public double MinSimilarityScore { get; set; } = 0.7;
    public int MaxContextLength { get; set; } = 4000;
}
