using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

using SimpleTalkMongoDb.Configuration;

namespace SimpleTalkMongoDb.Seed
{
    /// <summary>
    /// Loads the JSON files in Data/ into MongoDB, replacing what is there, and creates the
    /// user the Auth example signs in as. Takes the place of mongoimport and AddUser.js.
    /// </summary>
    public static class Seeder
    {
        private const int BatchSize = 1000;

        public static async Task RunAsync()
        {
            var settings = SampleConfig.Settings;
            await LoadAsync(SampleConfig.Db, settings["MongoProducts"]!, "Product.json");
            await LoadAsync(SampleConfig.Db, settings["MongoSpecialOfferCollectionName"]!, "SpecialOffer.json");
            await LoadAsync(SampleConfig.Db, settings["MongoCollectionName"]!, "Sales.json");
            await CreateSampleUserAsync(settings["SampleUser"]!, settings["SamplePassword"]!);
        }

        private static async Task LoadAsync(IMongoDatabase db, string collectionName, string fileName)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Data", fileName);
            ConsoleEx.WriteLine($"Loading {fileName} into '{collectionName}'...", ConsoleColor.Cyan);

            // The files are extended JSON ({"$date": ...}, {"$numberDecimal": ...}), as mongoimport reads them
            var documents = BsonSerializer.Deserialize<BsonArray>(await File.ReadAllTextAsync(path))
                .Select(value => value.AsBsonDocument)
                .ToList();

            await db.DropCollectionAsync(collectionName);
            var collection = db.GetCollection<BsonDocument>(collectionName);
            foreach (var batch in documents.Chunk(BatchSize))
            {
                await collection.InsertManyAsync(batch, new InsertManyOptions { IsOrdered = false });
            }

            ConsoleEx.WriteLine($"  {await collection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty)} documents", ConsoleColor.Yellow);
        }

        /// <summary>
        /// Creates the user with readWrite on the databases the examples use, unless it exists.
        /// </summary>
        private static async Task CreateSampleUserAsync(string user, string password)
        {
            var db = SampleConfig.Db;
            try
            {
                var info = await db.RunCommandAsync<BsonDocument>(new BsonDocument("usersInfo", user));
                if (info["users"].AsBsonArray.Count > 0)
                {
                    ConsoleEx.WriteLine($"User '{user}' already exists", ConsoleColor.Yellow);
                    return;
                }

                var roles = new BsonArray(
                    new[] { db.DatabaseNamespace.DatabaseName, SampleConfig.DbSampleLookup.DatabaseNamespace.DatabaseName, "Types" }
                        .Select(name => new BsonDocument { { "role", "readWrite" }, { "db", name } }));
                await db.RunCommandAsync<BsonDocument>(new BsonDocument
                {
                    { "createUser", user },
                    { "pwd", password },
                    { "roles", roles }
                });
                ConsoleEx.WriteLine($"Created user '{user}'", ConsoleColor.Yellow);
            }
            catch (MongoCommandException e)
            {
                // A server with access control needs a user allowed to create users
                throw new InvalidOperationException(
                    $"The data is loaded, but user '{user}', which the auth example needs, couldn't be created: {e.ErrorMessage}", e);
            }
        }
    }
}
