using SimpleTalkMongoDb.Aggregation;
using SimpleTalkMongoDb.Authentication;
using SimpleTalkMongoDb.CRUD_operations;
using SimpleTalkMongoDb.Filtering;
using SimpleTalkMongoDb.Loaders;
using SimpleTalkMongoDb.Serialization;

namespace SimpleTalkMongoDb.Examples
{
    /// <summary>
    /// One runnable example: its command-line name, the menu group it belongs to, what it shows and how to run it.
    /// </summary>
    public sealed record Example(string Name, string Category, string Description, Func<Task> RunAsync);

    /// <summary>
    /// Every example the menu offers, in the order the README lists them.
    /// </summary>
    public static class ExampleCatalog
    {
        public const string LoadersCategory = "Loaders (need SQL Server with AdventureWorks2016)";

        public static IReadOnlyList<Example> All { get; } =
        [
            new("aggregation-sales", "Aggregation", "Basic aggregation using LINQ, IAggregateFluent and shell-like BsonDocuments", AggregationSales.RunAsync),
            new("aggregation-unwind", "Aggregation", "Various aggregations on embedded documents with the $unwind operator", AggregationUnwind.RunAsync),
            new("sample-lookup", "Aggregation", "Combining two collections with the $lookup operator", SampleLookup.RunAsync),
            new("sample-unwind", "Aggregation", "Using the $unwind operator", SampleUnwind.RunAsync),

            new("auth", "Authentication", "Connecting to MongoDB with credentials and a connection string", Auth.RunAsync),

            new("crud", "CRUD operations", "Basic CRUD operations: insert, replace, update, upsert and delete", CrudDemo.RunAsync),

            new("explain", "Filtering", "Getting the query plan from code", Explain.RunAsync),
            new("filter-detail", "Filtering", "Querying for a document nested in an array", FilterDetail.RunAsync),
            new("filter-null-or-missing", "Filtering", "Querying for null or missing fields", FilterForNullOrMissingFields.RunAsync),
            new("filter-header", "Filtering", "Various ways to query documents", FilterHeader.RunAsync),
            new("filter-nested", "Filtering", "Querying nested documents", FilterNestedDocuments.RunAsync),
            new("find-with-cursor", "Filtering", "Using cursors, lists and ForEachAsync", FindWithCursor.RunAsync),

            new("attribute-decoration", "Serialization", "Customising serialization with attributes", AttributeDecoration.RunAsync),
            new("class-map", "Serialization", "Customising serialization with a ClassMap", ClassMap.RunAsync),
            new("class-map-automap", "Serialization", "A ClassMap with AutoMap", ClassMapAutoMap.RunAsync),
            new("convention-pack", "Serialization", "Customising serialization with a ConventionPack", TestConventionPack.RunAsync),
            new("test-types", "Serialization", "How various .NET types are serialized", TestTypes.RunAsync),

            new("create-json-products", LoadersCategory, "Writes Product.json from AdventureWorks2016", CreateJsonProducts.RunAsync),
            new("create-json-sales", LoadersCategory, "Writes Sales.json from AdventureWorks2016", CreateJsonSales.RunAsync),
            new("create-json-special-offers", LoadersCategory, "Writes SpecialOffer.json from AdventureWorks2016", CreateJsonSpecialOffer.RunAsync),
        ];

        public static Example? Find(string name) =>
            All.FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}
