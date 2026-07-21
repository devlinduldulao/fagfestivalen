using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace McpClient;

public static class Program
{
    private static readonly JsonSerializerOptions DisplayJson = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static async Task Main()
    {
        var serverAssembly = Path.Combine(AppContext.BaseDirectory, "McpServer.dll");
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            ArgumentList = { serverAssembly },
        }) ?? throw new InvalidOperationException("Could not start the MCP server.");

        await CallAsync(
            process,
            1,
            "initialize",
            new
            {
                protocolVersion = "2025-11-25",
                capabilities = new { },
                clientInfo = new { name = "conference-demo", version = "1.0" },
            });
        await NotifyAsync(process, "notifications/initialized");

        Console.WriteLine("Client initializes a standard server, then asks what tools it exposes:");
        Console.WriteLine((await CallAsync(process, 2, "tools/list")).ToJsonString(DisplayJson));

        Console.WriteLine("\nClient calls a tool without knowing its internal API:");
        Console.WriteLine((await CallAsync(
            process,
            3,
            "tools/call",
            new { name = "inventory.lookup", arguments = new { sku = "sku-200" } }))
            .ToJsonString(DisplayJson));

        Console.WriteLine("\n(The demo uses the JSON-RPC 2.0 initialization flow and keeps the tool payloads small.)");

        process.StandardInput.Close();
        await process.WaitForExitAsync();
    }

    private static async Task<JsonObject> CallAsync(
        Process process,
        int id,
        string method,
        object? parameters = null)
    {
        var request = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            id,
            method,
            @params = parameters ?? new { },
        });

        await process.StandardInput.WriteLineAsync(request);
        await process.StandardInput.FlushAsync();

        var line = await process.StandardOutput.ReadLineAsync()
            ?? throw new InvalidOperationException("MCP server exited before responding.");
        return JsonNode.Parse(line)?.AsObject()
            ?? throw new InvalidOperationException("MCP server returned invalid JSON.");
    }

    private static async Task NotifyAsync(Process process, string method, object? parameters = null)
    {
        var request = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method,
            @params = parameters ?? new { },
        });

        await process.StandardInput.WriteLineAsync(request);
        await process.StandardInput.FlushAsync();
    }
}
