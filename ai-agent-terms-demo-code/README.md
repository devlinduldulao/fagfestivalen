# AI Agent Terms Demo Code

These demos are intentionally small, dependency-free C# examples for .NET 10.
Build the solution once from this directory before running the demos:

```bash
dotnet build AgentTermsDemos.slnx
```

## 1. AGENTS.md

Shows how project instructions can be discovered from the working directory.

```bash
dotnet run --project 01-agents-md -- 01-agents-md/app/services
```

The zero-dependency verification harness is also runnable directly:

```bash
dotnet run --project 01-agents-md/tests
```

## 2. Agent Skills

Shows progressive disclosure: the agent only loads the skill whose description
matches the task, and loads nothing when no description matches. The matching
skill can also run a bundled C# console app.

```bash
dotnet run --project 02-agent-skills -- "make a conference deck outline"
dotnet run --project 02-agent-skills -- "review this code for secrets"
dotnet run --project 02-agent-skills -- "what is the weather"
```

## 3. MCP

Shows an MCP-shaped client/server exchange over standard input and output using
`System.Text.Json` and a child .NET process.

```bash
dotnet run --project 03-mcp/McpClient
```

## 4. A2A

Shows one agent discovering another agent card and delegating a task over HTTP.
The local finance agent is an ASP.NET Core minimal API.

```bash
dotnet run --project 04-a2a
```

## 5. Subagents

Shows a parent agent splitting independent checks across asynchronous workers
with `Task.WhenAny`.

```bash
dotnet run --project 05-subagents
```
