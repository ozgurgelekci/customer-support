using CustomerSupport.Models;

namespace CustomerSupport.Services;

public interface IAIService
{
    Task<string> GenerateResponseAsync(List<Message> conversationHistory);
    Task<string> GenerateResponseAsync(string userMessage, List<Message> conversationHistory);
}
