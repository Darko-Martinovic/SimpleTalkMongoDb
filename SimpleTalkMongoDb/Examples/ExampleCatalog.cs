using SimpleTalkMongoDb.Aggregation;
using SimpleTalkMongoDb.Authentication;
using SimpleTalkMongoDb.CRUD_operations;
using SimpleTalkMongoDb.Filtering;
using SimpleTalkMongoDb.Loaders;
using SimpleTalkMongoDb.Modern;
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
        public const string ModernCategory = "Modern MongoDB (any MongoDB 8)";
        public const string ReplicaSetCategory = "Modern MongoDB (needs a replica set)";

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

            new("pipeline-updates", ModernCategory, "Updates written as aggregation pipelines (4.2)", PipelineUpdates.RunAsync),
            new("merge-union", ModernCategory, "$merge into a materialized view (4.2) and $unionWith (4.4)", MergeAndUnion.RunAsync),
            new("window-functions", ModernCategory, "$setWindowFields: running totals and moving averages (5.0)", WindowFunctions.RunAsync),
            new("gap-filling", ModernCategory, "$densify and $fill for days without sales (5.1, 5.3)", GapFilling.RunAsync),
            new("top-n", ModernCategory, "$top, $topN and $maxN instead of sort and $last (5.2)", TopN.RunAsync),
            new("percentiles", ModernCategory, "$median and $percentile (7.0)", Percentiles.RunAsync),
            new("time-series", ModernCategory, "Time series collections (5.0) with secondary indexes (6.0)", TimeSeries.RunAsync),
            new("schema-validation", ModernCategory, "$jsonSchema validation (3.6) with detailed errors (5.0)", SchemaValidation.RunAsync),
            new("modern-indexes", ModernCategory, "Wildcard, hidden, compound wildcard, partial and TTL indexes, clustered collections", ModernIndexes.RunAsync),
            new("stable-api", ModernCategory, "The Stable API, strict (5.0)", StableApi.RunAsync),
            new("client-bulk-write", ModernCategory, "One bulk write across collections (8.0)", ClientBulkWrite.RunAsync),

            new("transactions", ReplicaSetCategory, "Multi-document transactions with WithTransactionAsync (4.0)", Transactions.RunAsync),
            new("change-streams", ReplicaSetCategory, "Change streams (3.6) with pre-images (6.0) and resuming", ChangeStreams.RunAsync),

            new("create-json-products", LoadersCategory, "Writes Product.json from AdventureWorks2016", CreateJsonProducts.RunAsync),
            new("create-json-sales", LoadersCategory, "Writes Sales.json from AdventureWorks2016", CreateJsonSales.RunAsync),
            new("create-json-special-offers", LoadersCategory, "Writes SpecialOffer.json from AdventureWorks2016", CreateJsonSpecialOffer.RunAsync),
        ];

        public static Example? Find(string name) =>
            All.FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}
