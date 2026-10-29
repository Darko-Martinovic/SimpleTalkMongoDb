using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

using SimpleTalkMongoDb.Pocos;

namespace SimpleTalkMongoDb.Configuration
{
    public static class SampleConfig
    {
        static SampleConfig()
        {
            // appsettings.json, then user secrets, then environment variables
            // (for example ConnectionStrings__Mongo), the later ones winning
            Settings = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddUserSecrets(typeof(SampleConfig).Assembly, optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Path to the AdventureWorks2016 database
            ConnectionString = Settings.GetConnectionString("AdventureWorks") ?? "";

            // MongoDB Client
            MongoConnectionString = Required("ConnectionStrings:Mongo");
            Client = new MongoClient(MongoConnectionString);

            // MongoDB Database - default 'simpleTalk'
            Db = Client.GetDatabase(Required("MongoDbName"));

            // To demonstrate $lookup operator, 'sampleLookup' database is used
            DbSampleLookup = Client.GetDatabase(Required("SampleLookup"));

            // The main collection 'adventureWorks2016'
            Collection = Db.GetCollection<SalesHeader>(Required("MongoCollectionName"));

            // The collection that supports dynamic schema
            CollectionDynamic = Db.GetCollection<BsonDocument>(Required("MongoCollectionName"));

            // The Special Offer Collection
            CollSpecialOffer = Db.GetCollection<SpecialOffer>(Required("MongoSpecialOfferCollectionName"));

            // The Product Collection
            CollProducts = Db.GetCollection<Product>(Required("MongoProducts"));

            // Collections used to demonstate $lookup operator
            CollPerson = DbSampleLookup.GetCollection<Person>(Required("MongoPersons"));
            CollItems = DbSampleLookup.GetCollection<Items>(Required("MongoItems"));
            CollNullNonExistingElements = DbSampleLookup.GetCollection<Tester>(Required("MongoNullOrNonExisting"));
            CollInventory = DbSampleLookup.GetCollection<Inventory>(Required("MongoInventory"));
            CollMeanings = DbSampleLookup.GetCollection<NameMeaning>(Required("MongoMeanings"));

            // T-SQL statements used to generate JSON files
            TsqlSales = Required("TSQL");
            TsqlSpecialOffers = Required("TSQLSpecialOffers");
            TsqlProducts = Required("TSQLProducts");
        }

        private static string Required(string key) =>
            Settings[key] ?? throw new InvalidOperationException($"'{key}' is missing from appsettings.json");

        public static IConfiguration Settings { get; }

        public static string ConnectionString { get; }

        public static string MongoConnectionString { get; }

        public static MongoClient Client { get; }

        public static IMongoDatabase Db { get; }

        public static IMongoDatabase DbSampleLookup { get; }

        public static IMongoCollection<SalesHeader> Collection { get; }

        public static IMongoCollection<BsonDocument> CollectionDynamic { get; }

        public static IMongoCollection<SpecialOffer> CollSpecialOffer { get; }

        public static IMongoCollection<Product> CollProducts { get; }

        public static IMongoCollection<Person> CollPerson { get; }

        public static IMongoCollection<Items> CollItems { get; }

        public static IMongoCollection<Tester> CollNullNonExistingElements { get; }

        public static IMongoCollection<Inventory> CollInventory { get; }

        public static IMongoCollection<NameMeaning> CollMeanings { get; }

        public static string TsqlSales { get; }

        public static string TsqlSpecialOffers { get; }

        public static string TsqlProducts { get; }
    }
}
