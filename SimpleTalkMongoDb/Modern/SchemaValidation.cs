using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;

using SimpleTalkMongoDb.Configuration;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// MongoDB 3.6: a collection can require its documents to match a JSON Schema ($jsonSchema).
    /// MongoDB 5.0: a rejected write says which rule failed and why, instead of only
    /// "Document failed validation".
    /// </summary>
    internal static class SchemaValidation
    {
        private const string CollectionName = "validatedOffers";

        public static async Task RunAsync()
        {
            var schema = new BsonDocument("$jsonSchema", new BsonDocument
            {
                { "bsonType", "object" },
                { "required", new BsonArray { "description", "category", "discountPct" } },
                {
                    "properties", new BsonDocument
                    {
                        { "description", new BsonDocument { { "bsonType", "string" }, { "minLength", 3 } } },
                        { "category", new BsonDocument("enum", new BsonArray { "Reseller", "Customer" }) },
                        { "discountPct", new BsonDocument { { "bsonType", "decimal" }, { "minimum", 0 }, { "maximum", 0.5 } } }
                    }
                }
            });

            var db = SampleConfig.Db;
            await db.DropCollectionAsync(CollectionName);
            await db.CreateCollectionAsync(CollectionName, new CreateCollectionOptions<BsonDocument>
            {
                Validator = schema,
                ValidationAction = DocumentValidationAction.Error,
                ValidationLevel = DocumentValidationLevel.Strict
            });
            var offers = db.GetCollection<BsonDocument>(CollectionName);

            ConsoleEx.WriteLine("Schema validation with $jsonSchema (MongoDB 3.6), detailed errors (5.0)", ConsoleColor.Magenta);
            ConsoleEx.WriteLine(schema.ToJson(new JsonWriterSettings { Indent = true }), ConsoleColor.Gray);
            Console.WriteLine();

            await InsertAsync(offers, new BsonDocument
            {
                { "description", "Summer sale" },
                { "category", "Customer" },
                { "discountPct", new Decimal128(0.15m) }
            });

            // A category the schema doesn't allow, and a discount that is too large
            await InsertAsync(offers, new BsonDocument
            {
                { "description", "Everything must go" },
                { "category", "Everyone" },
                { "discountPct", new Decimal128(0.9m) }
            });

            ConsoleEx.WriteLine($"Documents in '{CollectionName}': {await offers.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty)}", ConsoleColor.Cyan);
        }

        private static async Task InsertAsync(IMongoCollection<BsonDocument> offers, BsonDocument offer)
        {
            ConsoleEx.WriteLine($"Inserting {offer}", ConsoleColor.Cyan);
            try
            {
                await offers.InsertOneAsync(offer);
                ConsoleEx.WriteLine("  inserted", ConsoleColor.Yellow);
            }
            catch (MongoWriteException e) when (e.WriteError.Code == 121)
            {
                // 121 is DocumentValidationFailure; the details name each failed rule
                ConsoleEx.WriteLine($"  rejected: {e.WriteError.Message}", ConsoleColor.Red);
                ConsoleEx.WriteLine(e.WriteError.Details.ToJson(new JsonWriterSettings { Indent = true }), ConsoleColor.Yellow);
            }
        }
    }
}
