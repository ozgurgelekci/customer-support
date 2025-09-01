using CustomerSupport.Models;
using OpenAI;
using OpenAI.Chat;
using System.Text;

namespace CustomerSupport.Services;

public class RagService : IRagService
{
    private readonly IDocumentProcessingService _documentService;
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<RagService> _logger;

    public RagService(
        IDocumentProcessingService documentService,
        IConfiguration configuration,
        ILogger<RagService> logger)
    {
        _documentService = documentService;
        var apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API Key bulunamadı.");
        _openAIClient = new OpenAIClient(apiKey);
        _logger = logger;
    }

    public async Task<string> GenerateContextAwareResponseAsync(string userQuery, List<Message> conversationHistory)
    {
        try
        {
            _logger.LogInformation("RAG response üretiliyor. Query: {Query}", userQuery);

            // 1. İlgili dokümanları bul
            var relevantDocs = await RetrieveRelevantDocumentsAsync(userQuery);

            // 2. Context ile birlikte yanıt üret
            var response = await GenerateResponseWithContextAsync(userQuery, relevantDocs, conversationHistory);

            _logger.LogInformation("RAG response üretildi. Doküman sayısı: {DocCount}", relevantDocs.Count);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RAG response üretilirken hata oluştu. Query: {Query}", userQuery);
            
            // Fallback: Basit AI yanıtı
            return await GenerateSimpleResponseAsync(userQuery, conversationHistory);
        }
    }

    public async Task<List<DocumentSearchResult>> RetrieveRelevantDocumentsAsync(string query, int maxResults = 5)
    {
        try
        {
            var searchRequest = new DocumentSearchRequest
            {
                Query = query,
                TopK = maxResults,
                SimilarityThreshold = 0.7
            };

            var results = await _documentService.SearchSimilarDocumentsAsync(searchRequest);
            
            _logger.LogInformation("İlgili dokümanlar bulundu. Sorgu: {Query}, Bulunan: {Count}", query, results.Count);
            
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Doküman arama hatası. Query: {Query}", query);
            return new List<DocumentSearchResult>();
        }
    }

    public async Task<string> GenerateResponseWithContextAsync(string userQuery, List<DocumentSearchResult> relevantDocs, List<Message> conversationHistory)
    {
        try
        {
            var contextPrompt = BuildContextPrompt(userQuery, relevantDocs, conversationHistory);
            
            // Chat messages oluştur
            var messages = BuildChatMessages(contextPrompt, conversationHistory);
            
            // OpenAI ile yanıt üret
            var chatCompletion = await _openAIClient.GetChatClient("gpt-4o-mini")
                .CompleteChatAsync(messages);

            var response = chatCompletion.Value.Content[0].Text;
            
            _logger.LogInformation("RAG yanıt oluşturuldu. Token sayısı: {TokenCount}", 
                chatCompletion.Value.Usage?.TotalTokenCount ?? 0);
            
            return response ?? "Üzgünüm, şu anda bir yanıt oluşturamıyorum. Lütfen tekrar deneyin.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Context ile yanıt üretilirken hata oluştu");
            throw;
        }
    }

    private string BuildContextPrompt(string userQuery, List<DocumentSearchResult> relevantDocs, List<Message> conversationHistory)
    {
        var promptBuilder = new StringBuilder();

        // System prompt'a ek context bilgisi
        promptBuilder.AppendLine("Sen bir müşteri destek asistanısın. Aşağıdaki firma dokümanlarından yararlanarak sorulara cevap ver.");
        promptBuilder.AppendLine();

        // İlgili dokümanları context olarak ekle
        if (relevantDocs.Any())
        {
            promptBuilder.AppendLine("=== İLGİLİ FİRMA DOKÜMANLARI ===");
            
            foreach (var doc in relevantDocs.Take(3)) // En fazla 3 doküman
            {
                promptBuilder.AppendLine($"📄 **{doc.Title}**");
                if (!string.IsNullOrEmpty(doc.Category))
                {
                    promptBuilder.AppendLine($"Kategori: {doc.Category}");
                }
                promptBuilder.AppendLine($"Benzerlik: {doc.SimilarityScore:P1}");
                promptBuilder.AppendLine();
                
                // İçeriği kısalt
                var content = doc.Content.Length > 800 ? doc.Content[..800] + "..." : doc.Content;
                promptBuilder.AppendLine(content);
                promptBuilder.AppendLine();
                promptBuilder.AppendLine("---");
                promptBuilder.AppendLine();
            }
            
            promptBuilder.AppendLine("=== DOKÜMANLARIN SONU ===");
            promptBuilder.AppendLine();
        }

        // Mikro Yazılım Müşteri Destek Uzmanı Talimatları
        promptBuilder.AppendLine("🎯 SEN MIKRO YAZILIM A.Ş. MÜŞTERİ DESTEK UZMANISIN:");
        promptBuilder.AppendLine("");
        promptBuilder.AppendLine("YANITLAMA TALİMATLARI:");
        promptBuilder.AppendLine("1. Öncelikle yukarıdaki Mikro Yazılım dokümanlarını kullanarak cevap ver");
        promptBuilder.AppendLine("2. Sadece Mikro RUN, Mikro JUMP, Mikro FLY, Mikro Müşavir, Paraşüt ve Buluo hakkında destek sağla");
        promptBuilder.AppendLine("3. Dokümanlardan alıntı yaparken kaynak belirt");
        promptBuilder.AppendLine("4. Mikro Yazılım dışı konularda: 'Bu konuda sadece Mikro Yazılım ürünleri hakkında destek sağlayabilirim' de");
        promptBuilder.AppendLine("5. Fiyat sorularında: 'Fiyat bilgileri için satış ekibimizle iletişime geçin: 0 850 222 65 76'");
        promptBuilder.AppendLine("6. Emin olmadığın veya tam cevaplayamadığın konularda:");
        promptBuilder.AppendLine("   - Önce Buluo destek topluluğunu öner: '<a href=\"https://buluo.mikro.com.tr/s/\" target=\"_blank\">buluo.mikro.com.tr/s/</a> adresinde benzer sorulara bakabilirsiniz'");
        promptBuilder.AppendLine("   - Sonra: '<a href=\"https://www.mikro.com.tr\" target=\"_blank\">mikro.com.tr</a> veya müşteri hizmetlerimize ulaşabilirsiniz'");
        promptBuilder.AppendLine("7. Profesyonel, samimi ve çözüm odaklı yaklaş");
        promptBuilder.AppendLine();

        // Kullanıcı sorusu
        promptBuilder.AppendLine($"KULLANICI SORUSU: {userQuery}");
        
        var finalPrompt = promptBuilder.ToString();
        
        // Token limitini aş kısalt
        if (finalPrompt.Length > 6000)
        {
            finalPrompt = finalPrompt[..6000] + "\n\n[Context kısaltıldı...]";
        }

        _logger.LogDebug("Context prompt oluşturuldu. Uzunluk: {Length}, Doküman sayısı: {DocCount}", 
            finalPrompt.Length, relevantDocs.Count);

        return finalPrompt;
    }

    private async Task<string> GenerateSimpleResponseAsync(string userQuery, List<Message> conversationHistory)
    {
        try
        {
            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage("Sen bir müşteri destek asistanısın. Profesyonel, yardımsever ve anlayışlı bir şekilde müşterilere yardım et. Türkçe olarak yanıtla.")
            };

            // Conversation history ekle
            foreach (var message in conversationHistory.OrderBy(m => m.CreatedAt))
            {
                if (message.Sender == MessageSender.User)
                {
                    messages.Add(ChatMessage.CreateUserMessage(message.Content));
                }
                else if (message.Sender == MessageSender.AI)
                {
                    messages.Add(ChatMessage.CreateAssistantMessage(message.Content));
                }
            }

            // Kullanıcı sorusunu ekle
            messages.Add(ChatMessage.CreateUserMessage(userQuery));

            var chatCompletion = await _openAIClient.GetChatClient("gpt-4o-mini")
                .CompleteChatAsync(messages);

            return chatCompletion.Value.Content[0].Text ?? "Üzgünüm, şu anda bir yanıt oluşturamıyorum.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Basit AI yanıt üretilirken hata oluştu");
            return "Üzgünüm, teknik bir sorun yaşandı. Lütfen daha sonra tekrar deneyin.";
        }
    }

    private List<ChatMessage> BuildChatMessages(string contextPrompt, List<Message> conversationHistory)
    {
        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(contextPrompt)
        };

        foreach (var message in conversationHistory.OrderBy(m => m.CreatedAt))
        {
            if (message.Sender == MessageSender.User)
            {
                messages.Add(ChatMessage.CreateUserMessage(message.Content));
            }
            else if (message.Sender == MessageSender.AI)
            {
                messages.Add(ChatMessage.CreateAssistantMessage(message.Content));
            }
        }

        return messages;
    }
}