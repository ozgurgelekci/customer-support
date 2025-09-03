using CustomerSupport.Data;
using CustomerSupport.Models;
using CustomerSupport.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CustomerSupport.Hubs;

public class ChatHub : Hub
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRagService _ragService;
    private readonly IAIService _aiService;
    private readonly ILogger<ChatHub> _logger;
    
    // Aggressive token tasarrufu - off-topic soruları direkt engelle!

    public ChatHub(ApplicationDbContext dbContext, IRagService ragService, IAIService aiService, ILogger<ChatHub> logger)
    {
        _dbContext = dbContext;
        _ragService = ragService;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<string> StartSession(string userName)
    {
        try
        {
            // Yeni konuşma oluştur
            var conversation = new Conversation
            {
                StartedAt = DateTime.UtcNow
            };

            _dbContext.Conversations.Add(conversation);
            await _dbContext.SaveChangesAsync();

            // Kullanıcıyı grup olarak conversation'a ekle
            await Groups.AddToGroupAsync(Context.ConnectionId, conversation.Id.ToString());

            _logger.LogInformation("Yeni oturum başlatıldı. ConversationId: {ConversationId}, Kullanıcı: {UserName}", 
                conversation.Id, userName);

            // Hoş geldin mesajı gönder
            var welcomeMessage = $"Merhaba {userName}! Size nasıl yardımcı olabilirim?";
            
            // AI hoş geldin mesajını kaydet
            var aiMessage = new Message
            {
                ConversationId = conversation.Id,
                Sender = MessageSender.AI,
                Content = welcomeMessage,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Messages.Add(aiMessage);
            await _dbContext.SaveChangesAsync();

            // Hoş geldin mesajını gönder
            await Clients.Caller.SendAsync("ReceiveMessage", conversation.Id.ToString(), "AI", welcomeMessage);

            return conversation.Id.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Oturum başlatılırken hata oluştu. Kullanıcı: {UserName}", userName);
            throw new HubException("Oturum başlatılırken bir hata oluştu.");
        }
    }

    // AGGRESSIVE Token tasarrufu - off-topic soru tespiti
    private bool IsOffTopicMessage(string message)
    {
        var messageLower = message.ToLower().Trim();
        var messageWords = messageLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        // 🟢 Mikro Yazılım anahtar kelimeleri (ON-TOPIC)
        var mikroKeywords = new[] { 
            "mikro", "run", "jump", "fly", "müşavir", "paraşüt", "buluo", 
            "muhasebe", "crm", "erp", "ticaret", "bordro", "yazılım", "program"
        };
        
        // Mikro Yazılım anahtar kelimesi varsa kesinlikle ON-TOPIC
        if (messageWords.Any(word => mikroKeywords.Contains(word)))
            return false;
            
        // 🔴 Kesin OFF-TOPIC durumlar
        var offTopicKeywords = new[] { 
            // Selamlaşmalar
            "merhaba", "selam", "hello", "hi", "hey", "günaydin", "iyi", "hoş", "hoşça",
            // Genel konular  
            "hava", "spor", "yemek", "film", "müzik", "oyun", "siyaset", "tarih", "matematik",
            "nasıl", "naber", "ne", "var", "yok", "oldu", "oluyor", "kim", "nerede", "ne zaman",
            // Alakasız teknoloji
            "google", "microsoft", "apple", "android", "iphone", "facebook", "twitter", "instagram"
        };
        
        // Off-topic anahtar kelimesi varsa OFF-TOPIC
        if (messageWords.Any(word => offTopicKeywords.Contains(word)))
            return true;
            
        // Çok kısa mesajlar (3 kelimeden az) OFF-TOPIC kabul et
        if (messageWords.Length < 3)
            return true;
            
        // Soru işareti var ama Mikro kelimesi yok ise şüpheli
        if (messageLower.Contains("?") && !messageLower.Contains("mikro"))
            return true;
            
        return false;
    }

    public async Task SendMessage(string conversationId, string message)
    {
        try
        {
            if (!Guid.TryParse(conversationId, out var convId))
            {
                throw new HubException("Geçersiz konuşma ID'si.");
            }

            // Konuşmanın var olduğunu kontrol et
            var conversation = await _dbContext.Conversations
                .FirstOrDefaultAsync(c => c.Id == convId);

            if (conversation == null)
            {
                throw new HubException("Konuşma bulunamadı.");
            }
            
            // 🔥 AGGRESSIVE TOKEN TASARRUFU: Off-topic soruları direkt engelle!
            if (IsOffTopicMessage(message))
            {
                var userId = conversationId; // ConversationId as user identifier
                
                _logger.LogInformation("❌ OFF-TOPIC soru tespit edildi - TOKEN HARCANMIYOR! Kullanıcı: {UserId}, Mesaj: {Message}", 
                    userId, message);
                
                // HEMEN otomatik cevap döndür - AI'ya hiç gitme!
                var genericResponse = "Merhaba! Ben Mikro Yazılım müşteri destek uzmanıyım ve sadece <a href=\"https://www.mikro.com.tr\" target=\"_blank\">Mikro Yazılım</a> ürünleri (Mikro RUN, Mikro JUMP, Mikro FLY, Mikro Müşavir, Paraşüt) hakkında yardım edebilirim. Size nasıl yardımcı olabilirim?<br><br><small><i>🤖 Bu mesaj otomatik üretildi - Token harcanmadı!</i></small>";
                
                _logger.LogInformation("✅ Hazır cevap döndürüldü - SIFIR TOKEN harcanmış! Kullanıcı: {UserId}", userId);
                
                // Hazır cevabı döndür - AI'ya gitme!
                await Clients.Group(conversationId).SendAsync("ReceiveMessage", conversationId, "AI", genericResponse);
                return; // Erken exit - AI'ya ASLA gitme!
            }
            
            _logger.LogInformation("✅ ON-TOPIC soru - AI'ya gönderiliyor. Kullanıcı: {UserId}, Mesaj: {Message}", 
                conversationId, message);

            // Kullanıcı mesajını kaydet
            var userMessage = new Message
            {
                ConversationId = convId,
                Sender = MessageSender.User,
                Content = message,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Messages.Add(userMessage);
            await _dbContext.SaveChangesAsync();

            // Kullanıcı mesajını tüm grup üyelerine gönder
            await Clients.Group(conversationId).SendAsync("ReceiveMessage", conversationId, "User", message);

            // Konuşma geçmişini al
            var conversationHistory = await _dbContext.Messages
                .Where(m => m.ConversationId == convId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            // RAG ile context-aware yanıt oluştur
            string aiResponse;
            try
            {
                aiResponse = await _ragService.GenerateContextAwareResponseAsync(message, conversationHistory);
            }
            catch (Exception ragEx)
            {
                _logger.LogWarning(ragEx, "RAG service başarısız, normal AI service'e geçiliyor");
                try
                {
                    aiResponse = await _aiService.GenerateResponseAsync(message, conversationHistory);
                }
                catch (Exception aiEx)
                {
                    _logger.LogWarning(aiEx, "AI service de başarısız, demo cevap döndürülüyor");
                    
                    // Mikro Yazılım Demo Cevapları (OpenAI quota bittiğinde)
                    var demoResponses = new[]
                    {
                        "Mikro Yazılım müşteri destek ekibinden selamlar! Sorununuz için size yardımcı olmaya çalışacağım. Hangi ürünümüzle ilgili destek istiyorsunuz?<br><br><small><i>🎭 Demo Mode - AI servisi kullanılamıyor</i></small>",
                        "Mikro RUN, Mikro JUMP, Mikro FLY, Mikro Müşavir veya Paraşüt ile ilgili hangi konuda yardıma ihtiyacınız var? Detaylı bilgi verir misiniz?<br><br><small><i>🎭 Demo Mode - AI servisi kullanılamıyor</i></small>", 
                        "Anlayışla yaklaşıyor ve sorununuzu çözmek için elimden geleni yapacağım. Bu arada <a href=\"https://www.mikro.com.tr\" target=\"_blank\">mikro.com.tr</a> adresinden de detaylı bilgi alabilirsiniz.<br><br><small><i>🎭 Demo Mode - AI servisi kullanılamıyor</i></small>",
                        "Teknik ekibimizle iletişime geçerek size en hızlı şekilde dönüş yapacağım. Buluo platformumuzdan (<a href=\"https://buluo.mikro.com.tr\" target=\"_blank\">buluo.mikro.com.tr</a>) da faydalanabilirsiniz.<br><br><small><i>🎭 Demo Mode - AI servisi kullanılamıyor</i></small>",
                        "Sorununuzu not ettim. Daha fazla yardım için 0 850 222 65 76 numarasından da bizimle iletişime geçebilirsiniz.<br><br><small><i>🎭 Demo Mode - AI servisi kullanılamıyor</i></small>"
                    };
                    
                    var random = new Random();
                    aiResponse = demoResponses[random.Next(demoResponses.Length)];
                }
            }

            // AI yanıtını kaydet
            var aiMessage = new Message
            {
                ConversationId = convId,
                Sender = MessageSender.AI,
                Content = aiResponse,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Messages.Add(aiMessage);
            await _dbContext.SaveChangesAsync();

            // AI yanıtını tüm grup üyelerine gönder
            await Clients.Group(conversationId).SendAsync("ReceiveMessage", conversationId, "AI", aiResponse);

            _logger.LogInformation("Mesaj işlendi. ConversationId: {ConversationId}, Kullanıcı mesajı: {UserMessage}", 
                conversationId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mesaj gönderilirken hata oluştu. ConversationId: {ConversationId}", conversationId);
            await Clients.Caller.SendAsync("ReceiveMessage", conversationId, "System", 
                "Üzgünüm, mesajınız işlenirken bir hata oluştu. Lütfen tekrar deneyin.");
        }
    }

    public async Task EndSession(string conversationId)
    {
        try
        {
            if (!Guid.TryParse(conversationId, out var convId))
            {
                return;
            }

            var conversation = await _dbContext.Conversations
                .FirstOrDefaultAsync(c => c.Id == convId);

            if (conversation != null)
            {
                conversation.EndedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Oturum sonlandırıldı. ConversationId: {ConversationId}", conversationId);
            }

            // Gruptan çıkar
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Oturum sonlandırılırken hata oluştu. ConversationId: {ConversationId}", conversationId);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Bağlantı kapandığında yapılacak işlemler
        _logger.LogInformation("Kullanıcı bağlantısı kesildi. ConnectionId: {ConnectionId}", Context.ConnectionId);
        
        if (exception != null)
        {
            _logger.LogError(exception, "Bağlantı hatası ile kesildi.");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
