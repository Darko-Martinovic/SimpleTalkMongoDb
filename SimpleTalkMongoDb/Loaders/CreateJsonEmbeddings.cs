using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using MongoDB.Driver;

using SimpleTalkMongoDb.Configuration;

namespace SimpleTalkMongoDb.Loaders
{
    /// <summary>
    /// Writes ProductEmbeddings.json: a vector for each product's name and category, and for the
    /// questions the vector search example asks, made by a local Ollama model. The file is
    /// committed, so the vector search example runs without Ollama.
    /// </summary>
    internal static class CreateJsonEmbeddings
    {
        // The questions VectorSearch asks; they are embedded with the same model as the products
        public static readonly string[] Questions =
        [
            "a bike for riding on mountain trails",
            "keep my hands warm in winter",
            "a bike for long trips with luggage",
            "protection for my head"
        ];

        public static async Task RunAsync()
        {
            var url = SampleConfig.Settings["Ollama:Url"]!;
            var model = SampleConfig.Settings["Ollama:Model"]!;
            using var http = new HttpClient { BaseAddress = new Uri(url), Timeout = TimeSpan.FromMinutes(5) };

            var products = await SampleConfig.CollProducts.Find(FilterDefinition<Pocos.Product>.Empty).SortBy(p => p.ProductId).ToListAsync();
            Console.WriteLine($"Embedding {products.Count} products and {Questions.Length} questions with {model} at {url}");

            // nomic-embed-text expects these prefixes, telling documents and questions apart
            var productVectors = await EmbedAsync(http, model, products.Select(p => $"search_document: {Describe(p)}").ToList());
            var questionVectors = await EmbedAsync(http, model, Questions.Select(q => $"search_query: {q}").ToList());

            var file = new EmbeddingsFile(
                model,
                products.Zip(productVectors, (p, v) => new ProductEmbedding(p.ProductId, v)).ToList(),
                Questions.Zip(questionVectors, (q, v) => new QuestionEmbedding(q, v)).ToList());
            var path = Path.Combine(Directory.GetCurrentDirectory(), "ProductEmbeddings.json");
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(file));
            Console.WriteLine($"Wrote {path}; copy it to SimpleTalkMongoDb/Data");
        }

        /// <summary>The text a product's vector stands for.</summary>
        public static string Describe(Pocos.Product p) =>
            string.Join(", ", new[] { p.ProductName, p.SubCategoryName, p.Category }.Where(s => !string.IsNullOrWhiteSpace(s)));

        private static async Task<List<float[]>> EmbedAsync(HttpClient http, string model, List<string> input)
        {
            var vectors = new List<float[]>();
            foreach (var batch in input.Chunk(64))
            {
                var response = await http.PostAsJsonAsync("api/embed", new { model, input = batch });
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadFromJsonAsync<EmbedResponse>()
                    ?? throw new InvalidOperationException("Ollama returned no embeddings");
                // Five decimals are plenty for cosine similarity and keep the file small
                vectors.AddRange(body.Embeddings.Select(v => v.Select(x => MathF.Round(x, 5)).ToArray()));
            }

            return vectors;
        }

        private sealed record EmbedResponse([property: JsonPropertyName("embeddings")] float[][] Embeddings);
    }

    public sealed record EmbeddingsFile(string Model, List<ProductEmbedding> Products, List<QuestionEmbedding> Questions);

    public sealed record ProductEmbedding(int ProductId, float[] Embedding);

    public sealed record QuestionEmbedding(string Text, float[] Embedding);
}
