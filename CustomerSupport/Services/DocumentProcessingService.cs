using CustomerSupport.Data;
using CustomerSupport.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CustomerSupport.Services;

public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<DocumentProcessingService> _logger;

    // 🚀 OPTIMAL CHUNK AYARLARI (Türkçe & OpenAI için optimize)
    private const int DefaultChunkSize = 800;        // Optimal: 512-800 karakter (OpenAI embedding için)
    private const int MinChunkSize = 100;           // Minimum chunk boyutu
    private const int MaxChunkSize = 1200;          // Maximum chunk boyutu
    private const double OverlapPercentage = 0.15;  // %15 dynamic overlap
    private const int MinOverlap = 50;              // Minimum overlap
    private const int MaxOverlap = 250;             // Maximum overlap

    public DocumentProcessingService(
        ApplicationDbContext context, 
        IEmbeddingService embeddingService,
        ILogger<DocumentProcessingService> logger)
    {
        _context = context;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<Document> ProcessDocumentAsync(DocumentUploadRequest request)
    {
        try
        {
            // Yeni doküman oluştur
            var document = new Document
            {
                Title = request.Title,
                Content = request.Content,
                Category = request.Category,
                FileType = request.FileType,
                SourceUrl = request.SourceUrl,
                Metadata = request.Metadata,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Veritabanına kaydet
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            // Chunk'ları oluştur ve embed et
            var chunks = await CreateChunksAsync(document);
            document.Chunks = chunks;

            _logger.LogInformation("Doküman işlendi. ID: {DocumentId}, Chunk sayısı: {ChunkCount}", 
                document.Id, chunks.Count);

            return document;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Doküman işlenirken hata oluştu: {Title}", request.Title);
            throw;
        }
    }

    public async Task<List<DocumentChunk>> CreateChunksAsync(Document document)
    {
        try
        {
            var chunks = new List<DocumentChunk>();
            var content = document.Content;

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Boş içerik için chunk oluşturulmaya çalışıldı. DocumentId: {DocumentId}", document.Id);
                return chunks;
            }

            // Text'i optimal chunk'lara böl
            var textChunks = CreateOptimalChunks(content);

            // Her chunk için embedding hesapla
            for (int i = 0; i < textChunks.Count; i++)
            {
                var chunkText = textChunks[i].Text;
                var embedding = await _embeddingService.GetEmbeddingAsync(chunkText);

                var chunk = new DocumentChunk
                {
                    DocumentId = document.Id,
                    Content = chunkText,
                    ChunkIndex = i,
                    StartPosition = textChunks[i].StartIndex,
                    EndPosition = textChunks[i].EndIndex,
                    EmbeddingVector = _embeddingService.VectorToString(embedding),
                    CreatedAt = DateTime.UtcNow
                };

                chunks.Add(chunk);
            }

            // Chunk'ları veritabanına kaydet
            _context.DocumentChunks.AddRange(chunks);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Chunk'lar oluşturuldu. DocumentId: {DocumentId}, Chunk sayısı: {Count}", 
                document.Id, chunks.Count);

            return chunks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chunk'lar oluşturulurken hata oluştu. DocumentId: {DocumentId}", document.Id);
            throw;
        }
    }

    public async Task<List<DocumentSearchResult>> SearchSimilarDocumentsAsync(DocumentSearchRequest request)
    {
        try
        {
            // Query için embedding hesapla
            var queryEmbedding = await _embeddingService.GetEmbeddingAsync(request.Query);
            
            if (queryEmbedding.Length == 0)
            {
                _logger.LogWarning("Query için embedding hesaplanamadı: {Query}", request.Query);
                return new List<DocumentSearchResult>();
            }

            // Aktif dokümanların chunk'larını al
            var query = _context.DocumentChunks
                .Include(c => c.Document)
                .Where(c => c.Document.IsActive);

            // Kategori filtresi varsa uygula
            if (!string.IsNullOrEmpty(request.Category))
            {
                query = query.Where(c => c.Document.Category == request.Category);
            }

            var allChunks = await query.ToListAsync();
            var results = new List<DocumentSearchResult>();

            // Her chunk için similarity hesapla
            foreach (var chunk in allChunks)
            {
                if (string.IsNullOrEmpty(chunk.EmbeddingVector))
                    continue;

                var chunkEmbedding = _embeddingService.StringToVector(chunk.EmbeddingVector);
                var similarity = _embeddingService.CalculateSimilarity(queryEmbedding, chunkEmbedding);

                if (similarity >= request.SimilarityThreshold)
                {
                    results.Add(new DocumentSearchResult
                    {
                        DocumentId = chunk.Document.Id,
                        Title = chunk.Document.Title,
                        Content = chunk.Content,
                        Category = chunk.Document.Category,
                        SimilarityScore = similarity,
                        ChunkIndex = chunk.ChunkIndex
                    });
                }
            }

            // Similarity'ye göre sırala ve top-k al
            var topResults = results
                .OrderByDescending(r => r.SimilarityScore)
                .Take(request.TopK)
                .ToList();

            _logger.LogInformation("Similarity search tamamlandı. Query: {Query}, Sonuç sayısı: {Count}/{Total}", 
                request.Query, topResults.Count, results.Count);

            return topResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Similarity search sırasında hata oluştu: {Query}", request.Query);
            throw;
        }
    }

    public async Task<bool> ReprocessDocumentAsync(Guid documentId)
    {
        try
        {
            var document = await _context.Documents
                .Include(d => d.Chunks)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                _logger.LogWarning("Yeniden işlenecek doküman bulunamadı: {DocumentId}", documentId);
                return false;
            }

            // Eski chunk'ları sil
            _context.DocumentChunks.RemoveRange(document.Chunks);
            await _context.SaveChangesAsync();

            // Yeni chunk'lar oluştur
            document.Chunks.Clear();
            await CreateChunksAsync(document);

            document.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Doküman yeniden işlendi: {DocumentId}", documentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Doküman yeniden işlenirken hata oluştu: {DocumentId}", documentId);
            return false;
        }
    }

    public async Task<bool> DeleteDocumentAsync(Guid documentId)
    {
        try
        {
            var document = await _context.Documents
                .Include(d => d.Chunks)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                _logger.LogWarning("Silinecek doküman bulunamadı: {DocumentId}", documentId);
                return false;
            }

            // Cascade delete ile chunk'lar da silinir
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Doküman silindi: {DocumentId}", documentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Doküman silinirken hata oluştu: {DocumentId}", documentId);
            return false;
        }
    }

    /// <summary>
    /// 🚀 OPTIMAL CHUNKING STRATEGY - Türkçe içerik için optimize edilmiş
    /// </summary>
    private List<TextChunk> CreateOptimalChunks(string text)
    {
        var chunks = new List<TextChunk>();
        
        if (string.IsNullOrWhiteSpace(text))
            return chunks;

        // 1. Paragraf bazlı ön-bölme (büyük paragrafları ayırmak için)
        var paragraphs = SplitIntoParagraphs(text);
        
        foreach (var paragraph in paragraphs)
        {
            if (string.IsNullOrWhiteSpace(paragraph))
                continue;
                
            // Eğer paragraf zaten optimal boyuttaysa direkt ekle
            if (paragraph.Length <= MaxChunkSize && paragraph.Length >= MinChunkSize)
            {
                chunks.Add(new TextChunk
                {
                    Text = paragraph.Trim(),
                    StartIndex = 0, // Bu indexleri daha sonra ayarlarız
                    EndIndex = paragraph.Length
                });
            }
            // Büyük paragrafları daha küçük chunk'lara böl
            else if (paragraph.Length > MaxChunkSize)
            {
                var subChunks = SplitLargeParagraph(paragraph);
                chunks.AddRange(subChunks);
            }
            // Çok küçük paragrafları birleştir
            else if (paragraph.Length < MinChunkSize && chunks.Count > 0)
            {
                var lastChunk = chunks[chunks.Count - 1];
                if (lastChunk.Text.Length + paragraph.Length <= MaxChunkSize)
                {
                    lastChunk.Text += "\n\n" + paragraph.Trim();
                    lastChunk.EndIndex = lastChunk.StartIndex + lastChunk.Text.Length;
                    continue;
                }
            }
        }

        // 2. Chunk'ları optimize et ve overlap ekle
        var optimizedChunks = ApplyOverlapAndOptimize(chunks, text);
        
        // 3. Kalite filtresi uygula ve scoring
        var filteredChunks = FilterAndCleanChunks(optimizedChunks);
        var finalChunks = ScoreAndRankChunks(filteredChunks);
        
        _logger.LogInformation("🎯 OPTIMAL CHUNKING TAMAMLANDI! Chunk sayısı: {Count}, Ortalama boyut: {AvgSize} karakter, Ortalama kalite: {AvgQuality}", 
            finalChunks.Count, 
            finalChunks.Count > 0 ? finalChunks.Average(c => c.Text.Length) : 0,
            finalChunks.Count > 0 ? finalChunks.Average(c => CalculateChunkQuality(c.Text)) : 0);
        
        return finalChunks;
    }

    private List<string> SplitIntoParagraphs(string text)
    {
        // Türkçe paragraf ayırıcıları
        return text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                   .Where(p => !string.IsNullOrWhiteSpace(p))
                   .Select(p => p.Trim())
                   .ToList();
    }

    private List<TextChunk> SplitLargeParagraph(string paragraph)
    {
        var chunks = new List<TextChunk>();
        var sentences = SplitIntoTurkishSentences(paragraph);
        var currentChunk = new StringBuilder();
        int currentLength = 0;

        foreach (var sentence in sentences)
        {
            // Dynamic overlap hesapla
            var dynamicOverlap = Math.Max(MinOverlap, Math.Min(MaxOverlap, (int)(DefaultChunkSize * OverlapPercentage)));
            
            if (currentLength + sentence.Length > DefaultChunkSize && currentChunk.Length > 0)
            {
                // Chunk'ı kaydet
                var chunkText = currentChunk.ToString().Trim();
                if (chunkText.Length >= MinChunkSize)
                {
                    chunks.Add(new TextChunk
                    {
                        Text = chunkText,
                        StartIndex = 0,
                        EndIndex = chunkText.Length
                    });
                }

                // Yeni chunk başlat (smart overlap ile)
                var overlapText = GetSmartOverlap(chunkText, dynamicOverlap);
                currentChunk.Clear();
                currentChunk.Append(overlapText);
                currentLength = overlapText.Length;
            }

            currentChunk.Append(sentence);
            currentLength += sentence.Length;
        }

        // Son chunk
        if (currentChunk.Length >= MinChunkSize)
        {
            chunks.Add(new TextChunk
            {
                Text = currentChunk.ToString().Trim(),
                StartIndex = 0,
                EndIndex = currentChunk.Length
            });
        }

        return chunks;
    }

    private List<TextChunk> ApplyOverlapAndOptimize(List<TextChunk> chunks, string originalText)
    {
        // Bu metodda chunk'lar arası overlap ve pozisyon optimizasyonu yapılacak
        // Şimdilik basit bir implementasyon
        for (int i = 0; i < chunks.Count; i++)
        {
            chunks[i].StartIndex = i * DefaultChunkSize;
            chunks[i].EndIndex = chunks[i].StartIndex + chunks[i].Text.Length;
        }
        
        return chunks;
    }

    private List<TextChunk> FilterAndCleanChunks(List<TextChunk> chunks)
    {
        return chunks.Where(chunk => 
        {
            var cleanText = chunk.Text.Trim();
            
            // Minimum boyut kontrolü
            if (cleanText.Length < MinChunkSize) return false;
            
            // Çok tekrar eden içerik filtresi
            if (IsRepetitiveContent(cleanText)) return false;
            
            // Sadece noktalama veya sayı olan chunk'ları filtrele
            if (cleanText.All(c => char.IsPunctuation(c) || char.IsWhiteSpace(c) || char.IsDigit(c))) return false;
            
            return true;
        }).ToList();
    }

    private bool IsRepetitiveContent(string text)
    {
        // Basit tekrar tespiti - çok basit bir implementasyon
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < 3) return false;
        
        var uniqueWords = words.Distinct().Count();
        return (double)uniqueWords / words.Length < 0.3; // %30'dan az unique word varsa tekrarlı kabul et
    }

    /// <summary>
    /// 📊 Chunk'ları kalite puanına göre sırala ve optimize et
    /// </summary>
    private List<TextChunk> ScoreAndRankChunks(List<TextChunk> chunks)
    {
        return chunks.OrderByDescending(chunk => CalculateChunkQuality(chunk.Text)).ToList();
    }

    /// <summary>
    /// 🎯 Chunk kalite puanı hesapla (0-100 arası)
    /// </summary>
    private double CalculateChunkQuality(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;

        var score = 0.0;
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // 1. Optimal boyut puanı (%30)
        var lengthScore = CalculateLengthScore(text.Length) * 0.3;

        // 2. Cümle yapısı puanı (%25)
        var sentenceScore = CalculateSentenceStructureScore(text) * 0.25;

        // 3. Kelime çeşitliliği puanı (%20)
        var diversityScore = CalculateWordDiversityScore(words) * 0.2;

        // 4. İçerik zenginliği puanı (%15) - Mikro Yazılım terimleri
        var contentScore = CalculateContentRichnessScore(text) * 0.15;

        // 5. Readability puanı (%10)
        var readabilityScore = CalculateReadabilityScore(text) * 0.1;

        score = lengthScore + sentenceScore + diversityScore + contentScore + readabilityScore;

        return Math.Round(Math.Max(0, Math.Min(100, score)), 2);
    }

    private double CalculateLengthScore(int length)
    {
        // Optimal uzunluk 600-800 karakter
        if (length >= 600 && length <= 800) return 100;
        if (length >= 400 && length <= 1000) return 80;
        if (length >= 200 && length <= 1200) return 60;
        if (length >= MinChunkSize && length <= MaxChunkSize) return 40;
        return 20;
    }

    private double CalculateSentenceStructureScore(string text)
    {
        var sentences = SplitIntoTurkishSentences(text);
        if (sentences.Count == 0) return 0;

        var score = 0.0;

        // İdeal cümle sayısı: 2-6 cümle
        if (sentences.Count >= 2 && sentences.Count <= 6)
            score += 40;
        else if (sentences.Count >= 1 && sentences.Count <= 8)
            score += 25;
        else
            score += 10;

        // Cümle uzunluk tutarlılığı
        var avgSentenceLength = sentences.Average(s => s.Length);
        if (avgSentenceLength >= 50 && avgSentenceLength <= 150)
            score += 35;
        else if (avgSentenceLength >= 30 && avgSentenceLength <= 200)
            score += 20;

        // Noktalama zenginliği
        var punctuationVariety = text.Count(c => ".,!?:;".Contains(c));
        if (punctuationVariety >= sentences.Count) // En az her cümlede bir noktalama
            score += 25;

        return score;
    }

    private double CalculateWordDiversityScore(string[] words)
    {
        if (words.Length == 0) return 0;

        var uniqueWords = words.Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var diversityRatio = (double)uniqueWords / words.Length;

        // İdeal çeşitlilik oranı: %70-90
        if (diversityRatio >= 0.7 && diversityRatio <= 0.9) return 100;
        if (diversityRatio >= 0.5 && diversityRatio <= 0.95) return 80;
        if (diversityRatio >= 0.3) return 60;
        return 20;
    }

    private double CalculateContentRichnessScore(string text)
    {
        var lowerText = text.ToLowerInvariant();
        var score = 0.0;

        // Mikro Yazılım ile ilgili terimler (bonus puan)
        var mikroTerms = new[]
        {
            "mikro", "yazılım", "erp", "crm", "muhasebe", "bordro", "ticaret",
            "run", "jump", "fly", "müşavir", "paraşüt", "buluo",
            "fatura", "stok", "satış", "alış", "rapor", "analiz",
            "veritabanı", "entegrasyon", "api", "modül", "sistem"
        };

        var foundTerms = mikroTerms.Count(term => lowerText.Contains(term));
        score += Math.Min(50, foundTerms * 5); // Her terim +5 puan, max 50

        // Teknik terimler
        var technicalTerms = new[]
        {
            "yönetim", "süreç", "işlem", "hesap", "kayıt", "data",
            "kullanıcı", "firma", "şirket", "işletme", "platform"
        };

        var foundTechTerms = technicalTerms.Count(term => lowerText.Contains(term));
        score += Math.Min(30, foundTechTerms * 3); // Her terim +3 puan, max 30

        // Sayısal veri varlığı (tablolar, istatistikler için önemli)
        if (System.Text.RegularExpressions.Regex.IsMatch(text, @"\d+"))
            score += 20;

        return score;
    }

    private double CalculateReadabilityScore(string text)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return 0;

        var score = 0.0;

        // Ortalama kelime uzunluğu (Türkçe için ideal: 4-8 harf)
        var avgWordLength = words.Average(w => w.Length);
        if (avgWordLength >= 4 && avgWordLength <= 8)
            score += 50;
        else if (avgWordLength >= 3 && avgWordLength <= 10)
            score += 30;

        // Çok uzun kelime oranı (12+ harf)
        var longWordRatio = words.Count(w => w.Length > 12) / (double)words.Length;
        if (longWordRatio <= 0.1) // %10'dan az uzun kelime
            score += 30;
        else if (longWordRatio <= 0.2)
            score += 15;

        // Başlık/liste formatı bonus
        if (text.Contains("-") || text.Contains("•") || text.Contains("1."))
            score += 20;

        return score;
    }

    /// <summary>
    /// 🇹🇷 Türkçe için optimize edilmiş cümle bölme
    /// </summary>
    private List<string> SplitIntoTurkishSentences(string text)
    {
        // Türkçe noktalama işaretleri ve yaygın kısaltmalar
        var turkishAbbreviations = new HashSet<string>
        {
            "Dr.", "Doç.", "Prof.", "Yrd.", "Öğr.", "Arş.", "Uzm.", "Av.", "Müh.", "İng.", "Mim.", 
            "Ltd.", "Şti.", "A.Ş.", "T.C.", "vb.", "vs.", "örn.", "yani", "Sayfa", "s.", "No.", "Tel."
        };

        var sentences = new List<string>();
        var currentSentence = new StringBuilder();
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < words.Length; i++)
        {
            var word = words[i];
            currentSentence.Append(word + " ");

            // Cümle sonu tespiti
            if (word.EndsWith(".") || word.EndsWith("!") || word.EndsWith("?") || word.EndsWith(":"))
            {
                // Kısaltma kontrolü
                var wordWithoutPunc = word.TrimEnd('.', '!', '?', ':');
                
                if (!turkishAbbreviations.Contains(word) && 
                    !turkishAbbreviations.Contains(wordWithoutPunc + ".") &&
                    currentSentence.Length > 10) // Minimum cümle uzunluğu
                {
                    sentences.Add(currentSentence.ToString().Trim());
                    currentSentence.Clear();
                }
            }
        }

        // Kalan metin
        if (currentSentence.Length > 0)
        {
            sentences.Add(currentSentence.ToString().Trim());
        }

        // Boş veya çok kısa cümleleri filtrele
        return sentences.Where(s => !string.IsNullOrWhiteSpace(s) && s.Length >= 10).ToList();
    }

    /// <summary>
    /// 🧠 Akıllı overlap - cümle sınırlarını koruyarak overlap oluştur
    /// </summary>
    private string GetSmartOverlap(string text, int targetOverlapSize)
    {
        if (text.Length <= targetOverlapSize)
            return text;

        // Hedef overlap pozisyonundan geriye doğru cümle başlangıcı ara
        var startPos = text.Length - targetOverlapSize;
        var sentences = SplitIntoTurkishSentences(text);
        
        if (sentences.Count <= 1)
        {
            // Tek cümle varsa, kelime sınırında kes
            return GetWordBoundaryOverlap(text, targetOverlapSize);
        }

        // Son 1-2 cümleyi al (boyut kontrolüyle)
        var overlapText = "";
        for (int i = sentences.Count - 1; i >= 0; i--)
        {
            var candidateOverlap = sentences[i] + " " + overlapText;
            if (candidateOverlap.Length <= targetOverlapSize * 1.5) // %50 tolerance
            {
                overlapText = candidateOverlap.Trim();
            }
            else
            {
                break;
            }
        }

        return string.IsNullOrWhiteSpace(overlapText) ? 
            GetWordBoundaryOverlap(text, targetOverlapSize) : 
            overlapText;
    }

    private string GetWordBoundaryOverlap(string text, int targetSize)
    {
        if (text.Length <= targetSize)
            return text;

        var startPos = text.Length - targetSize;
        
        // Kelime sınırı bul
        while (startPos > 0 && !char.IsWhiteSpace(text[startPos]))
            startPos--;

        return text.Substring(startPos).Trim();
    }

    private class TextChunk
    {
        public string Text { get; set; } = string.Empty;
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }
}
