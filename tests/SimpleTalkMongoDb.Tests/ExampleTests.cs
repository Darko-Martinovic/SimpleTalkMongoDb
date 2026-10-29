using System.Diagnostics;

using SimpleTalkMongoDb.Examples;

using Testcontainers.MongoDb;

namespace SimpleTalkMongoDb.Tests;

/// <summary>
/// Starts MongoDB in a container and seeds it once, for all the examples. The container is
/// mongodb-atlas-local, as in compose.yaml: a single-node replica set (for transactions and change
/// streams) with Atlas Search and Vector Search.
/// </summary>
public sealed class MongoFixture : IAsyncLifetime
{
    // atlas-local sets up its own replica set and runs without access control
    private readonly MongoDbContainer _mongo = new MongoDbBuilder("mongodb/mongodb-atlas-local:8.0")
        .WithUsername(string.Empty)
        .WithPassword(string.Empty)
        .Build();

    // directConnection: the replica set's member name is only known inside the container
    public string ConnectionString => new MongoDB.Driver.MongoUrlBuilder(_mongo.GetConnectionString()) { DirectConnection = true }.ToString();

    public async ValueTask InitializeAsync()
    {
        try
        {
            await _mongo.StartAsync();
        }
        catch (Exception e)
        {
            // Show why MongoDB didn't start, not only that it didn't
            var (stdout, stderr) = await _mongo.GetLogsAsync();
            throw new InvalidOperationException($"MongoDB didn't start:\n{stdout}\n{stderr}", e);
        }

        var seed = await App.RunAsync("seed", ConnectionString);
        Assert.True(seed.ExitCode == 0, seed.Output);
    }

    public ValueTask DisposeAsync() => _mongo.DisposeAsync();
}

/// <summary>
/// Runs every example the way the menu does, in a process of its own, against the seeded database.
/// </summary>
public sealed class ExampleTests(MongoFixture mongo) : IClassFixture<MongoFixture>
{
    // The loaders read SQL Server, which the tests don't have
    public static TheoryData<string> Examples { get; } =
        new(ExampleCatalog.All.Where(e => e.Category != ExampleCatalog.LoadersCategory).Select(e => e.Name));

    [Theory]
    [MemberData(nameof(Examples))]
    public async Task Example_runs_to_the_end(string name)
    {
        var result = await App.RunAsync(name, mongo.ConnectionString);

        Assert.True(result.ExitCode == 0, result.Output);
    }

    // Results that show each way of querying still finds what the article shows, and that the
    // modern examples get their point across
    [Theory]
    [InlineData("aggregation-sales", "29818")]
    [InlineData("aggregation-unwind", "Touring-1000 Promotion")]
    [InlineData("sample-unwind", "121317")]
    [InlineData("filter-header", "69508")]
    [InlineData("find-with-cursor", "51711")]
    [InlineData("auth", "31465/31465/31465")]
    [InlineData("top-n", "29818, 29617, 29580")]
    [InlineData("gap-filling", "16 days with sales, 14 added")]
    [InlineData("time-series", "31465 measurements")]
    [InlineData("schema-validation", "Documents in 'validatedOffers': 1")]
    [InlineData("modern-indexes", "territory_wildcard_details")]
    [InlineData("stable-api", "APIStrictError")]
    [InlineData("client-bulk-write", "deleted 1")]
    [InlineData("atlas-search", "Long-Sleeve Logo Jersey")]
    [InlineData("vector-search", "Full-Finger Gloves")]
    [InlineData("ef-core", "Updated #50: EF Core launch offer, extended")]
    [InlineData("change-streams", "before: { \"_id\" : 1, \"description\" : \"Autumn sale\", \"discountPct\" : 15 }")]
    public async Task Example_finds_the_expected_result(string name, string expected)
    {
        var result = await App.RunAsync(name, mongo.ConnectionString);

        Assert.Contains(expected, result.Output);
    }

    [Fact]
    public async Task A_rolled_back_order_leaves_no_trace()
    {
        var result = await App.RunAsync("transactions", mongo.ConnectionString);

        // Order 2 was inserted inside the aborted transaction: it must be gone, and the stock as order 1 left it
        var after = result.Output.Split('\n').Select(line => line.TrimEnd('\r')).Single(line => line.StartsWith("After trying to order 10"));
        Assert.Equal("After trying to order 10: stock 3, orders: #1 x2", after);
    }

    [Fact]
    public async Task An_unknown_example_lists_the_examples_and_fails()
    {
        var result = await App.RunAsync("no-such-example", mongo.ConnectionString);

        Assert.Equal(2, result.ExitCode);
        Assert.Contains("aggregation-sales", result.Output);
    }
}

/// <summary>
/// Runs the console app with `dotnet SimpleTalkMongoDb.dll`, pointed at a MongoDB.
/// </summary>
internal static class App
{
    public static async Task<(int ExitCode, string Output)> RunAsync(string argument, string connectionString)
    {
        var start = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet")
        {
            ArgumentList = { Path.Combine(AppContext.BaseDirectory, "SimpleTalkMongoDb.dll"), argument },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Environment = { ["ConnectionStrings__Mongo"] = connectionString },
        };
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            // A hung example mustn't outlive the test and its container
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            return (-1, $"Timed out after 3 minutes\n{await output}{await error}");
        }

        return (process.ExitCode, await output + await error);
    }
}
