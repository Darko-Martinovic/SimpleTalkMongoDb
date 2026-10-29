using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Search;

using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// Atlas Search: full-text search with relevance scores, typo tolerance and autocomplete, through
    /// the $search stage. It runs in MongoDB Atlas, and locally in the mongodb-atlas-local container.
    /// </summary>
    internal static class AtlasSearch
    {
        private const string IndexName = "products_text";

        public static async Task RunAsync()
        {
            var products = SampleConfig.CollProducts;

            // productName is searchable as words and, for autocomplete, as prefixes of 2 to 15 letters
            await SearchIndexes.EnsureAsync(products, IndexName, SearchIndexType.Search, new BsonDocument("mappings", new BsonDocument
            {
                { "dynamic", false },
                {
                    "fields", new BsonDocument
                    {
                        {
                            "productName", new BsonArray
                            {
                                new BsonDocument("type", "string"),
                                new BsonDocument { { "type", "autocomplete" }, { "tokenization", "edgeGram" }, { "minGrams", 2 }, { "maxGrams", 15 } }
                            }
                        },
                        { "category", new BsonDocument("type", "token") }
                    }
                }
            }));

            var search = Builders<Product>.Search;
            ConsoleEx.WriteLine("Atlas Search ($search)", ConsoleColor.Magenta);

            // A typo: a regular query finds nothing, fuzzy text search finds the mountain frames
            const string typo = "mountian frme";
            var exact = await products.CountDocumentsAsync(Builders<Product>.Filter.Regex(p => p.ProductName, new BsonRegularExpression(typo, "i")));
            ConsoleEx.WriteLine($"Products whose name contains '{typo}', by regular expression: {exact}", ConsoleColor.Cyan);
            await ShowAsync($"Fuzzy text search for '{typo}' (up to one typo per word)",
                search.Text(p => p.ProductName, typo, new SearchFuzzyOptions { MaxEdits = 1 }), products);

            // As someone types: matches any word of the name that starts with the letters so far
            await ShowAsync("Autocomplete for 'tou'", search.Autocomplete(p => p.ProductName, "tou"), products);

            // Several conditions: must be about a jersey, should preferably be long-sleeve, only clothing
            await ShowAsync("Compound: 'jersey', preferably 'long sleeve', in Clothing",
                search.Compound()
                    .Must(search.Text(p => p.ProductName, "jersey"))
                    .Should(search.Phrase(p => p.ProductName, "long-sleeve"))
                    .Filter(search.Equals(p => p.Category, "Clothing")),
                products);
        }

        private static async Task ShowAsync(string title, SearchDefinition<Product> query, IMongoCollection<Product> products)
        {
            var found = await products.Aggregate()
                .Search(query, indexName: IndexName)
                .Project<ScoredProduct>(Builders<Product>.Projection
                    .Include(p => p.ProductName)
                    .MetaSearchScore(nameof(ScoredProduct.Score)))
                .Limit(5)
                .ToListAsync();

            ConsoleEx.WriteLine(title, ConsoleColor.Cyan);
            foreach (var p in found)
            {
                ConsoleEx.WriteLine($"  {p.Score,6:N2}  {p.ProductName}", ConsoleColor.Yellow);
            }
        }

        private sealed class ScoredProduct
        {
            [BsonId]
            public int ProductId { get; set; }

            [BsonElement("productName")]
            public string ProductName { get; set; } = "";

            public double Score { get; set; }
        }
    }
}
