using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AgentSkillsDemo;

public sealed record Skill(string Name, string Description, string Directory);

public static partial class Program
{
    private static readonly HashSet<string> Stopwords =
        ["a", "an", "and", "for", "is", "of", "or", "the", "this", "to", "what", "when"];

    public static async Task<int> Main(string[] args)
    {
        var task = args.Length > 0
            ? string.Join(' ', args)
            : "make a conference deck outline";
        var skills = LoadSkills();
        var ranked = skills
            .Select(skill => (Score: Score(task, skill), Skill: skill))
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Skill.Name, StringComparer.Ordinal)
            .ToArray();

        Console.WriteLine($"User task: {task}");
        Console.WriteLine("Available skill descriptions:");
        foreach (var skill in skills)
        {
            Console.WriteLine($"- {skill.Name}: {skill.Description}");
        }

        if (ranked.Length == 0 || ranked[0].Score == 0)
        {
            Console.WriteLine("\nNo skill matched. The agent keeps the extra instructions unloaded.");
            return 0;
        }

        var best = ranked[0].Skill;
        Console.WriteLine($"\nLoaded skill: {best.Name}");
        Console.WriteLine("(A real agent matches descriptions with the model itself; word overlap is a stand-in.)");
        Console.WriteLine(File.ReadAllText(Path.Combine(best.Directory, "SKILL.md")).Trim());

        var demoScript = Path.Combine(best.Directory, "scripts", "Program.cs");
        if (!File.Exists(demoScript))
        {
            return 0;
        }

        Console.WriteLine("\nSkill script output:");

        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            UseShellExecute = false,
            ArgumentList =
            {
                Path.Combine(AppContext.BaseDirectory, "DeckBuilderScript.dll"), task,
            },
        }) ?? throw new InvalidOperationException("Could not start the bundled skill script.");

        await process.WaitForExitAsync();
        return process.ExitCode;
    }

    private static IReadOnlyList<Skill> LoadSkills()
    {
        var skillsDirectory = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "skills"));

        return Directory.EnumerateFiles(skillsDirectory, "SKILL.md", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal)
            .Select(skillFile =>
            {
                var metadata = ParseFrontmatter(File.ReadAllText(skillFile));
                var directory = Path.GetDirectoryName(skillFile)!;
                return new Skill(
                    metadata.GetValueOrDefault("name", Path.GetFileName(directory)),
                    metadata.GetValueOrDefault("description", string.Empty),
                    directory);
            })
            .ToArray();
    }

    private static Dictionary<string, string> ParseFrontmatter(string text)
    {
        if (!text.StartsWith("---", StringComparison.Ordinal))
        {
            return [];
        }

        var closingMarker = text.IndexOf("---", 3, StringComparison.Ordinal);
        if (closingMarker < 0)
        {
            return [];
        }

        return text[3..closingMarker]
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.Split(':', 2, StringSplitOptions.TrimEntries))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0], parts => parts[1].Trim('"'), StringComparer.Ordinal);
    }

    private static int Score(string task, Skill skill)
    {
        var haystack = Tokenize($"{skill.Name.Replace('-', ' ')} {skill.Description}");
        return Tokenize(task).Intersect(haystack).Count();
    }

    private static HashSet<string> Tokenize(string text) =>
        WordRegex()
            .Matches(text.ToLowerInvariant())
            .Select(match => match.Value)
            .Where(word => !Stopwords.Contains(word))
            .ToHashSet(StringComparer.Ordinal);

    [GeneratedRegex("[a-z0-9]+")]
    private static partial Regex WordRegex();
}
