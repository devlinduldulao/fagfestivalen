using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace McpServer;

public static class Program
{
    private static readonly JsonSerializerOptions ProtocolJson = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static readonly IReadOnlyDictionary<string, object> Inventory =
        new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["sku-100"] = new { name = "Conference badge", available = 42 },
            ["sku-200"] = new { name = "Workshop seat", available = 7 },
        };

    public static async Task Main()
    {
        while (await Console.In.ReadLineAsync() is { } line)
        {
            var message = JsonNode.Parse(line)?.AsObject()
                ?? throw new InvalidOperationException("Expected a JSON-RPC object.");
            var response = Handle(message);
            if (response is not null)
            {
                await Console.Out.WriteLineAsync(response.ToJsonString(ProtocolJson));
                await Console.Out.FlushAsync();
            }
        }
    }

    private static JsonObject? Handle(JsonObject message)
    {
        if (message["jsonrpc"]?.GetValue<string>() != "2.0")
        {
            return Error(message, -32600, "invalid JSON-RPC version");
        }

        var method = message["method"]?.GetValue<string>();
        var parameters = message["params"] as JsonObject ?? [];

        object result;
        switch (method)
        {
            case "notifications/initialized":
                return null;
            case "initialize":
                result = new
                {
                    protocolVersion = parameters["protocolVersion"]?.GetValue<string>() ?? "2025-11-25",
                    capabilities = new { tools = new { } },
                    serverInfo = new { name = "inventory-demo", version = "1.0" },
                };
                break;
            case "tools/list":
                result = new
                {
                    tools = new object[]
                    {
                        new
                        {
                            name = "time.now",
                            description = "Return the current UTC time.",
                            inputSchema = new { type = "object", additionalProperties = false },
                        },
                        new
                        {
                            name = "inventory.lookup",
                            description = "Look up inventory by SKU.",
                            inputSchema = new
                            {
                                type = "object",
                                properties = new { sku = new { type = "string" } },
                                required = new[] { "sku" },
                            },
                        },
                    },
                };
                break;
            case "tools/call" when parameters["name"]?.GetValue<string>() == "time.now":
                result = TextContent(DateTimeOffset.UtcNow.ToString("O"));
                break;
            case "tools/call" when parameters["name"]?.GetValue<string>() == "inventory.lookup":
                var sku = parameters["arguments"]?["sku"]?.GetValue<string>();
                var item = sku is not null && Inventory.TryGetValue(sku, out var match)
                    ? match
                    : new { error = "unknown sku" };
                result = TextContent(JsonSerializer.Serialize(item, ProtocolJson));
                break;
            case "tools/call":
                return Error(message, -32602, $"unknown tool: {parameters["name"]}");
            default:
                return Error(message, -32601, $"unsupported method: {method}");
        }

        return new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = message["id"]?.DeepClone(),
            ["result"] = JsonSerializer.SerializeToNode(result),
        };
    }

    private static object TextContent(string text) =>
        new { content = new[] { new { type = "text", text } } };

    private static JsonObject Error(JsonObject message, int code, string errorMessage) => new()
    {
        ["jsonrpc"] = "2.0",
        ["id"] = message["id"]?.DeepClone(),
        ["error"] = new JsonObject { ["code"] = code, ["message"] = errorMessage },
    };
}
