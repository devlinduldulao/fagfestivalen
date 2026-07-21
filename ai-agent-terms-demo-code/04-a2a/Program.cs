using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Logging.ClearProviders();
builder.WebHost.UseUrls($"http://127.0.0.1:{GetAvailablePort()}");

var financeAgent = builder.Build();
financeAgent.MapGet("/.well-known/agent-card.json", () => new
{
    name = "finance-agent",
    description = "Approves spend requests under a policy limit.",
    tasks = new[] { new { name = "approve_spend", url = "/tasks/approve_spend" } },
});
financeAgent.MapPost("/tasks/approve_spend", (SpendRequest request) => new
{
    decision = request.Amount <= 5_000 ? "approved" : "needs director review",
    amount = request.Amount,
    policy_limit = 5_000,
});

await financeAgent.StartAsync();

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    WriteIndented = true,
};

try
{
    var baseUrl = financeAgent.Urls.Single();
    using var procurementAgent = new HttpClient { BaseAddress = new Uri(baseUrl) };

    var card = await procurementAgent.GetFromJsonAsync<AgentCard>("/.well-known/agent-card.json")
        ?? throw new InvalidOperationException("Finance agent returned no agent card.");

    Console.WriteLine("Procurement agent discovered this finance agent card:");
    Console.WriteLine(JsonSerializer.Serialize(card, jsonOptions));

    var taskUrl = card.Tasks[0].Url;

    var first = await DelegateAsync(procurementAgent, taskUrl, 4_200);
    Console.WriteLine("\nProcurement delegated a 4200 request and received:");
    Console.WriteLine(JsonSerializer.Serialize(first, jsonOptions));

    var second = await DelegateAsync(procurementAgent, taskUrl, 9_500);
    Console.WriteLine("\nProcurement delegated a 9500 request and received:");
    Console.WriteLine(JsonSerializer.Serialize(second, jsonOptions));
}
finally
{
    await financeAgent.StopAsync();
}

static async Task<SpendDecision> DelegateAsync(HttpClient client, string taskUrl, int amount)
{
    using var response = await client.PostAsJsonAsync(
        taskUrl,
        new SpendRequest("Acme Training", amount));
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<SpendDecision>()
        ?? throw new InvalidOperationException("Finance agent returned no decision.");
}

static int GetAvailablePort()
{
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    return ((IPEndPoint)listener.LocalEndpoint).Port;
}

public sealed record AgentTask(string Name, string Url);
public sealed record AgentCard(string Name, string Description, AgentTask[] Tasks);
public sealed record SpendRequest(string Vendor, int Amount);
public sealed record SpendDecision(
    string Decision,
    int Amount,
    [property: JsonPropertyName("policy_limit")] int PolicyLimit);
