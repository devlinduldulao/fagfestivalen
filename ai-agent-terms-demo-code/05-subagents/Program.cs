using System.Diagnostics;
using System.Globalization;

namespace SubagentsDemo;

public sealed record Check(string Name, string[] Files, TimeSpan Duration);

public static class Program
{
    public static async Task Main()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        Check[] checks =
        [
            new("API contracts", ["Api/Users.cs", "Api/Orders.cs"], TimeSpan.FromSeconds(0.8)),
            new("Database migrations", ["Migrations/001_Init.sql", "Migrations/002_Orders.sql"], TimeSpan.FromSeconds(0.6)),
            new("Frontend forms", ["Web/Login.razor", "Web/Checkout.razor"], TimeSpan.FromSeconds(0.7)),
            new("Security hotspots", ["Auth/Session.cs", "Billing/Webhook.cs"], TimeSpan.FromSeconds(0.9)),
        ];

        Console.WriteLine("Parent agent splits four independent reviews into subagents.\n");

        var stopwatch = Stopwatch.StartNew();
        var pending = checks.Select(RunSubagentAsync).ToList();

        while (pending.Count > 0)
        {
            var completed = await Task.WhenAny(pending);
            pending.Remove(completed);
            Console.WriteLine(await completed);
        }

        stopwatch.Stop();
        var sequential = checks.Sum(check => check.Duration.TotalSeconds);

        Console.WriteLine(
            $"\nWall time: {stopwatch.Elapsed.TotalSeconds:F1}s in parallel " +
            $"vs {sequential:F1}s if run one at a time.");
        Console.WriteLine("Parent agent keeps the final summaries, not every token each worker read.");
    }

    private static async Task<string> RunSubagentAsync(Check check)
    {
        // Each worker receives only the slice of context needed for its job.
        await Task.Delay(check.Duration);
        return $"{check.Name}: inspected {check.Files.Length} files in " +
            $"{check.Duration.TotalSeconds:F1}s ({string.Join(", ", check.Files)})";
    }
}
