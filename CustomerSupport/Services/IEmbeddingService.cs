namespace CustomerSupport.Services;

public interface IEmbeddingService
{
    Task<float[]> GetEmbeddingAsync(string text);
    Task<List<float[]>> GetEmbeddingsAsync(List<string> texts);
    double CalculateSimilarity(float[] embedding1, float[] embedding2);
    string VectorToString(float[] vector);
    float[] StringToVector(string vectorString);
}
