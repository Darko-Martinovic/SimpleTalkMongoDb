using System.Diagnostics;

using SimpleTalkMongoDb.Examples;

using Testcontainers.MongoDb;

namespace SimpleTalkMongoDb.Tests;

/// <summary>
/// Starts MongoDB in a container and seeds it once, for all the examples.
/// </summary>
public sealed class MongoFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _mongo = new MongoDbBuilder("mongo:8.0").Build();

    public string ConnectionString => _mongo.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _mongo.StartAsync();
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

    // Results that show each way of querying still finds what the article shows
    [Theory]
    [InlineData("aggregation-sales", "29818")]
    [InlineData("aggregation-unwind", "Touring-1000 Promotion")]
    [InlineData("sample-unwind", "121317")]
    [InlineData("filter-header", "69508")]
    [InlineData("find-with-cursor", "51711")]
    [InlineData("auth", "31465/31465/31465")]
    public async Task Example_finds_the_expected_result(string name, string expected)
    {
        var result = await App.RunAsync(name, mongo.ConnectionString);

        Assert.Contains(expected, result.Output);
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
        await process.WaitForExitAsync(timeout.Token);
        return (process.ExitCode, await output + await error);
    }
}
