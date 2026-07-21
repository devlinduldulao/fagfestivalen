using AgentInstructionsDemo.Services;

namespace AgentInstructionsDemo;

public static class Program
{
    public static void Main(string[] args)
    {
        var start = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        var files = AgentInstructionFinder.Find(start);

        Console.WriteLine($"Working directory: {start}");
        Console.WriteLine($"Instructions loaded: {files.Count}");

        if (files.Count == 0)
        {
            Console.WriteLine("No AGENTS.md found between here and the repo root.");
            return;
        }

        for (var index = 0; index < files.Count; index++)
        {
            Console.WriteLine($"\n{new string('=', 72)}");
            Console.WriteLine($"{index + 1}. {files[index]}");
            Console.WriteLine(new string('-', 72));
            Console.WriteLine(File.ReadAllText(files[index]).Trim());
        }

        Console.WriteLine("\nEffective behavior:");
        Console.WriteLine("- General repo rules load first.");
        Console.WriteLine("- More local rules load later and can narrow the behavior.");
    }
}
