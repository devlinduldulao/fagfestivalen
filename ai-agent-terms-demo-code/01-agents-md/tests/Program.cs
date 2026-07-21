using AgentInstructionsDemo.Services;

namespace AgentInstructionsDemo.Tests;

public static class Program
{
    public static int Main()
    {
        var tests = new (string Name, Action Run)[]
        {
            ("loads root rules before local rules", LoadsRootRulesBeforeLocalRules),
            ("stops at the repository root", StopsAtRepositoryRoot),
        };

        try
        {
            foreach (var test in tests)
            {
                test.Run();
                Console.WriteLine($"PASS: {test.Name}");
            }

            Console.WriteLine($"{tests.Length} tests passed.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"FAIL: {exception.Message}");
            return 1;
        }
    }

    private static void LoadsRootRulesBeforeLocalRules()
    {
        InTemporaryDirectory(root =>
        {
            Directory.CreateDirectory(Path.Combine(root, ".git"));
            var rootInstruction = Path.Combine(root, "AGENTS.md");
            File.WriteAllText(rootInstruction, "root rules");

            var service = Path.Combine(root, "app", "services");
            Directory.CreateDirectory(service);
            var serviceInstruction = Path.Combine(service, "AGENTS.md");
            File.WriteAllText(serviceInstruction, "service rules");

            AssertSequence(
                AgentInstructionFinder.Find(service),
                [rootInstruction, serviceInstruction]);
        });
    }

    private static void StopsAtRepositoryRoot()
    {
        InTemporaryDirectory(directory =>
        {
            var root = Path.Combine(directory, "repo");
            var nested = Path.Combine(root, "app");
            Directory.CreateDirectory(Path.Combine(root, ".git"));
            Directory.CreateDirectory(nested);

            AssertSequence(AgentInstructionFinder.Find(nested), []);
        });
    }

    private static void AssertSequence(IReadOnlyList<string> actual, IReadOnlyList<string> expected)
    {
        if (!actual.SequenceEqual(expected))
        {
            throw new InvalidOperationException(
                $"Expected [{string.Join(", ", expected)}], got [{string.Join(", ", actual)}].");
        }
    }

    private static void InTemporaryDirectory(Action<string> action)
    {
        var directory = Path.Combine(Path.GetTempPath(), $"agent-demo-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        try
        {
            action(directory);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
