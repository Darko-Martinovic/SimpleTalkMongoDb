using System.Text.Json;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Loaders;
using SimpleTalkMongoDb.Pocos;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// Atlas Vector Search: find documents by meaning rather than words, through the $vectorSearch
    /// stage, from embeddings (vectors) of their text. The products' and the questions' embeddings
    /// were made once by a local model (create-json-embeddings) and are in Data/ProductEmbeddings.json.
    /// </summary>
    internal static class VectorSearch
    {
        private const string CollectionName = "productVectors";
        private const string IndexName = "products_vector";

        public static async Task RunAsync()
        {
            var file = JsonSerializer.Deserialize<EmbeddingsFile>(
                await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Data", "ProductEmbeddings.json")))!;

            // The products with their vectors, in a collection of their own. Dropping it drops its
            // index too, so the wait below covers a build over these documents; an index left in place
            // would take in a delete and reinsert in the background, and could answer half-empty
            await SampleConfig.Db.DropCollectionAsync(CollectionName);
            var vectors = SampleConfig.Db.GetCollection<ProductVector>(CollectionName);
            var embeddings = file.Products.ToDictionary(e => e.ProductId, e => e.Embedding);
            var products = await SampleConfig.CollProducts.Find(FilterDefinition<Product>.Empty).ToListAsync();
            await vectors.InsertManyAsync(products.Select(p => new ProductVector
            {
                ProductId = p.ProductId,
                Description = CreateJsonEmbeddings.Describe(p),
                Category = p.Category,
                Embedding = embeddings[p.ProductId]
            }));

            await SearchIndexes.EnsureAsync(vectors, IndexName, SearchIndexType.VectorSearch, new BsonDocument("fields", new BsonArray
            {
                new BsonDocument
                {
                    { "type", "vector" }, { "path", nameof(ProductVector.Embedding) },
                    { "numDimensions", file.Products[0].Embedding.Length }, { "similarity", "cosine" }
                },
                // Allows a filter on category in the same stage
                new BsonDocument { { "type", "filter" }, { "path", nameof(ProductVector.Category) } }
            }));

            ConsoleEx.WriteLine($"Atlas Vector Search ($vectorSearch) over {file.Model} embeddings", ConsoleColor.Magenta);
            foreach (var question in file.Questions)
            {
                await ShowAsync(vectors, question, filter: null);
            }

            // The last question again, only among accessories: the headset bearings (a component) drop out
            await ShowAsync(vectors, file.Questions[^1], Builders<ProductVector>.Filter.Eq(p => p.Category, "Accessories"));
            ConsoleEx.WriteLine("'keep my hands warm in winter' finds gloves without a word in common: the match is by meaning.", ConsoleColor.Cyan);
        }

        private static async Task ShowAsync(IMongoCollection<ProductVector> vectors, QuestionEmbedding question, FilterDefinition<ProductVector>? filter)
        {
            var found = await vectors.Aggregate()
                .VectorSearch(p => p.Embedding, new QueryVector(question.Embedding), limit: 4, new VectorSearchOptions<ProductVector>
                {
                    IndexName = IndexName,
                    NumberOfCandidates = 100,
                    Filter = filter
                })
                .Project<ScoredProduct>(Builders<ProductVector>.Projection
                    .Include(p => p.Description)
                    .MetaVectorSearchScore(nameof(ScoredProduct.Score)))
                .ToListAsync();

            ConsoleEx.WriteLine($"'{question.Text}'{(filter is null ? "" : ", in Accessories only")}", ConsoleColor.Cyan);
            foreach (var p in found)
            {
                ConsoleEx.WriteLine($"  {p.Score:N3}  {p.Description}", ConsoleColor.Yellow);
            }
        }

        public sealed class ProductVector
        {
            [BsonId]
            public int ProductId { get; set; }

            public string Description { get; set; } = "";

            public string Category { get; set; } = "";

            public float[] Embedding { get; set; } = [];
        }

        private sealed class ScoredProduct
        {
            [BsonId]
            public int ProductId { get; set; }

            public string Description { get; set; } = "";

            public double Score { get; set; }
        }
    }
}
