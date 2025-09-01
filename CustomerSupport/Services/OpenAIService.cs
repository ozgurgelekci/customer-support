using CustomerSupport.Models;
using OpenAI;
using OpenAI.Chat;

namespace CustomerSupport.Services;

public class OpenAIService : IAIService
{
    private readonly OpenAIClient _client;
    private readonly ILogger<OpenAIService> _logger;
    private const string SystemPrompt = @"Sen Mikro Yazılım A.Ş.'nin deneyimli müşteri destek uzmanısın. 

🎯 UZMANLUK ALANIN:
- Mikro RUN (ERP ve Muhasebe Sistemi)
- Mikro JUMP (Ticaret ve Satış Programı)  
- Mikro FLY (CRM ve Müşteri Yönetimi)
- Mikro Müşavir (İş Danışmanlığı Çözümleri)
- Paraşüt (Muhasebe ve Bordro Çözümleri)
- Buluo Platformu (<a href=""https://buluo.mikro.com.tr"" target=""_blank"">buluo.mikro.com.tr</a>)

📋 GÖREV TANİMIN:
1. Sadece Mikro Yazılım ürünleri ve hizmetleri hakkında destek sağla
2. Teknik sorunları analiz et ve çözüm öner
3. Ürün kullanımı konusunda eğitim ve rehberlik ver
4. Müşteri şikayetlerini anlayışla dinle ve çözüm odaklı yaklaş
5. Gerektiğinde ilgili departmanlara yönlendirme yap

💬 İLETİŞİM TARZI:
- Profesyonel, samimi ve yardımsever ol
- Türkçe ve anlaşılır bir dil kullan
- Empati kur ve sabırlı ol
- Teknik terimleri basit şekilde açıkla

⚠️ SINIRLAR:
- Mikro Yazılım dışındaki ürünler hakkında yorum yapma
- Rakip firma ürünleri ile karşılaştırma yapma
- Fiyat bilgisi verme (satış ekibine yönlendir)
- Kesin olmadığın konularda spekülasyon yapma

🔗 YÖNLENDIRME:
Emin olmadığın konularda:
- 'Bu konuda size daha detaylı bilgi verebilmem için ilgili uzmanlarımızla iletişime geçmenizi öneririm.'
- '<a href=""https://www.mikro.com.tr"" target=""_blank"">mikro.com.tr</a> adresinden detaylı bilgi alabilir veya 0 850 222 65 76 numarasından bizimle iletişime geçebilirsiniz.'
- 'Buluo platformunda (<a href=""https://buluo.mikro.com.tr"" target=""_blank"">buluo.mikro.com.tr</a>) bu konu hakkında detaylı makaleler bulunmaktadır.'

Sen Mikro Yazılım'ın müşteri memnuniyeti odaklı, çözüm üreten ve güvenilir destek uzmanısın!";

    public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        var apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API Key bulunamadı.");
        _client = new OpenAIClient(apiKey);
        _logger = logger;
    }

    public async Task<string> GenerateResponseAsync(List<Message> conversationHistory)
    {
        try
        {
            var messages = BuildChatMessages(conversationHistory);
            
            var chatCompletion = await _client.GetChatClient("gpt-4o-mini")
                .CompleteChatAsync(messages);

            var response = chatCompletion.Value.Content[0].Text;
            
            _logger.LogInformation("AI yanıt oluşturuldu. Token sayısı: {TokenCount}", 
                chatCompletion.Value.Usage?.TotalTokenCount ?? 0);
            
            return response ?? "Üzgünüm, şu anda bir yanıt oluşturamıyorum. Lütfen tekrar deneyin.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI yanıt oluşturulurken hata oluştu");
            return "Üzgünüm, teknik bir sorun yaşandı. Lütfen daha sonra tekrar deneyin.";
        }
    }

    public async Task<string> GenerateResponseAsync(string userMessage, List<Message> conversationHistory)
    {
        // Yeni mesajı geçmişe ekle
        var updatedHistory = conversationHistory.ToList();
        updatedHistory.Add(new Message 
        { 
            Content = userMessage, 
            Sender = MessageSender.User, 
            CreatedAt = DateTime.UtcNow 
        });

        return await GenerateResponseAsync(updatedHistory);
    }

    private List<ChatMessage> BuildChatMessages(List<Message> conversationHistory)
    {
        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(SystemPrompt)
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
