using OpenAI;
using System.Text;

namespace CustomerSupport.Services;

public class OpenAIEmbeddingService : IEmbeddingService
{
    private readonly OpenAIClient _client;
    private readonly ILogger<OpenAIEmbeddingService> _logger;
    private const string EmbeddingModel = "text-embedding-3-small"; // En ucuz embedding model!

    public OpenAIEmbeddingService(IConfiguration configuration, ILogger<OpenAIEmbeddingService> logger)
    {
        var apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API Key bulunamadı.");
        _client = new OpenAIClient(apiKey);
        _logger = logger;
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Boş text için embedding alınmaya çalışıldı");
                return Array.Empty<float>();
            }

            // Metni temizle ve kısalt
            var cleanText = CleanText(text);
            if (cleanText.Length > 8000) // OpenAI token limiti
            {
                cleanText = cleanText[..8000];
            }

            var embeddingClient = _client.GetEmbeddingClient(EmbeddingModel);
            var embedding = await embeddingClient.GenerateEmbeddingAsync(cleanText);

            _logger.LogDebug("Embedding oluşturuldu. Text uzunluğu: {Length}", cleanText.Length);
            
            // OpenAI SDK'dan embedding vector'ü al
            return embedding.Value.ToFloats().ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedding oluşturulurken hata oluştu. Text: {Text}", text[..Math.Min(100, text.Length)]);
            return Array.Empty<float>();
        }
    }

    public async Task<List<float[]>> GetEmbeddingsAsync(List<string> texts)
    {
        try
        {
            var embeddings = new List<float[]>();
            
            // Batch olarak işle (OpenAI limitleri nedeniyle)
            const int batchSize = 100;
            
            for (int i = 0; i < texts.Count; i += batchSize)
            {
                var batch = texts.Skip(i).Take(batchSize).ToList();
                var batchTasks = batch.Select(GetEmbeddingAsync);
                var batchResults = await Task.WhenAll(batchTasks);
                embeddings.AddRange(batchResults);
            }

            _logger.LogInformation("Toplu embedding oluşturuldu. Text sayısı: {Count}", texts.Count);
            return embeddings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplu embedding oluşturulurken hata oluştu");
            return new List<float[]>();
        }
    }

    public double CalculateSimilarity(float[] embedding1, float[] embedding2)
    {
        if (embedding1.Length != embedding2.Length || embedding1.Length == 0)
        {
            return 0.0;
        }

        // Cosine similarity hesapla
        double dotProduct = 0.0;
        double norm1 = 0.0;
        double norm2 = 0.0;

        for (int i = 0; i < embedding1.Length; i++)
        {
            dotProduct += embedding1[i] * embedding2[i];
            norm1 += embedding1[i] * embedding1[i];
            norm2 += embedding2[i] * embedding2[i];
        }

        if (norm1 == 0.0 || norm2 == 0.0)
        {
            return 0.0;
        }

        return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
    }

    public string VectorToString(float[] vector)
    {
        if (vector == null || vector.Length == 0)
        {
            return string.Empty;
        }

        return string.Join(",", vector.Select(v => v.ToString("F6")));
    }

    public float[] StringToVector(string vectorString)
    {
        if (string.IsNullOrWhiteSpace(vectorString))
        {
            return Array.Empty<float>();
        }

        try
        {
            return vectorString.Split(',')
                              .Select(s => float.Parse(s.Trim()))
                              .ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vector string parse edilirken hata oluştu: {VectorString}", 
                vectorString[..Math.Min(100, vectorString.Length)]);
            return Array.Empty<float>();
        }
    }

    private static string CleanText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        
        foreach (char c in text)
        {
            if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t')
            {
                continue; // Kontrol karakterlerini atla
            }
            
            sb.Append(c);
        }

        // Fazla boşlukları temizle
        return System.Text.RegularExpressions.Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
    }
}
