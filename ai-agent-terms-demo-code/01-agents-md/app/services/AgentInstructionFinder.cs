namespace AgentInstructionsDemo.Services;

public static class AgentInstructionFinder
{
    /// <summary>
    /// Returns AGENTS.md files from the repository root toward the working directory.
    /// </summary>
    public static IReadOnlyList<string> Find(string start)
    {
        var current = Path.GetFullPath(start);
        if (File.Exists(current))
        {
            current = Path.GetDirectoryName(current)
                ?? throw new InvalidOperationException("The file has no parent directory.");
        }

        var found = new List<string>();

        while (current is not null)
        {
            var candidate = Path.Combine(current, "AGENTS.md");
            if (File.Exists(candidate))
            {
                found.Add(candidate);
            }

            if (Directory.Exists(Path.Combine(current, ".git")))
            {
                break;
            }

            current = Directory.GetParent(current)?.FullName;
        }

        found.Reverse();
        return found;
    }
}
