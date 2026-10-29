using System.Diagnostics;

using SimpleTalkMongoDb.Examples;
using SimpleTalkMongoDb.Seed;

using Spectre.Console;

// dotnet run                    -> pick an example from a menu
// dotnet run -- <example name>  -> run one example
// dotnet run -- list            -> list the examples
// dotnet run -- seed            -> load the sample data into MongoDB

const string Seed = "seed";

return args switch
{
    [] => Menu(),
    ["list"] => List(),
    [var name] when name.Equals(Seed, StringComparison.OrdinalIgnoreCase) => await RunAsync("Seed the database", Seeder.RunAsync),
    [var name] when ExampleCatalog.Find(name) is { } example => await RunAsync(example.Description, example.RunAsync),
    _ => Unknown(),
};

static async Task<int> RunAsync(string title, Func<Task> run)
{
    AnsiConsole.Write(new Rule($"[yellow]{Markup.Escape(title)}[/]").LeftJustified());
    try
    {
        await run();
        return 0;
    }
    catch (Exception e)
    {
        AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
        return 1;
    }
}

// Each example runs in a process of its own, as each had its own Main before:
// the serialization examples register class maps and conventions for the whole process.
static int Menu()
{
    var seed = new Example(Seed, "Setup", "Seed the database (loads Data/*.json and creates the sample user)", () => Task.CompletedTask);
    var exit = new Example("exit", "Setup", "Exit", () => Task.CompletedTask);
    while (true)
    {
        var prompt = new SelectionPrompt<Example>()
            .Title("[green]SimpleTalk MongoDB examples[/]: pick one to run")
            .PageSize(30)
            .Mode(SelectionMode.Leaf)
            .UseConverter(e => e.Description.Length == 0
                ? $"[yellow]{Markup.Escape(e.Name)}[/]"
                : $"{e.Name,-28} [grey]{Markup.Escape(e.Description)}[/]")
            .AddChoiceGroup(Group("Setup"), seed, exit);
        foreach (var group in ExampleCatalog.All.GroupBy(e => e.Category))
        {
            prompt.AddChoiceGroup(Group(group.Key), group);
        }

        var choice = AnsiConsole.Prompt(prompt);
        if (choice == exit)
        {
            return 0;
        }

        var exitCode = RunInOwnProcess(choice.Name);
        if (exitCode != 0)
        {
            AnsiConsole.MarkupLine($"[red]The example ended with exit code {exitCode}[/]");
        }

        AnsiConsole.MarkupLine("[grey]Press any key to return to the menu[/]");
        Console.ReadKey(intercept: true);
    }
}

// A group heading in the menu, which can't be picked
static Example Group(string name) => new(name, name, "", () => Task.CompletedTask);

static int RunInOwnProcess(string name)
{
    // Started as SimpleTalkMongoDb.exe, or as `dotnet SimpleTalkMongoDb.dll`
    var host = Environment.ProcessPath!;
    var start = Path.GetFileNameWithoutExtension(host).Equals("dotnet", StringComparison.OrdinalIgnoreCase)
        ? new ProcessStartInfo(host) { ArgumentList = { Path.Combine(AppContext.BaseDirectory, "SimpleTalkMongoDb.dll"), name } }
        : new ProcessStartInfo(host) { ArgumentList = { name } };
    using var process = Process.Start(start)!;
    process.WaitForExit();
    return process.ExitCode;
}

static int List()
{
    var table = new Table().AddColumns("Example", "Group", "What it shows");
    table.AddRow(Seed, "Setup", "Loads Data/*.json into MongoDB and creates the sample user");
    foreach (var e in ExampleCatalog.All)
    {
        table.AddRow(e.Name, Markup.Escape(e.Category), Markup.Escape(e.Description));
    }

    AnsiConsole.Write(table);
    return 0;
}

static int Unknown()
{
    AnsiConsole.MarkupLine("[red]Unknown example.[/] Run with no arguments for the menu, or with one of these names:");
    List();
    return 2;
}
