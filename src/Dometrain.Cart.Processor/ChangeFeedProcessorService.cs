using Dometrain.Cart.Api.ShoppingCarts;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Dometrain.Cart.Processor;

public class ChangeFeedProcessorService : BackgroundService
{
    private const string DatabaseId = "cartdb";
    private const string SourceContainerId = "carts";
    private const string LeaseContainerId = "carts-leases";

    private readonly CosmosClient _cosmosClient;
    private readonly ILogger<ChangeFeedProcessorService> _logger;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public ChangeFeedProcessorService(CosmosClient cosmosClient, ILogger<ChangeFeedProcessorService> logger, IConnectionMultiplexer connectionMultiplexer)
    {
        _cosmosClient = cosmosClient;
        _logger = logger;
        _connectionMultiplexer = connectionMultiplexer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var database = _cosmosClient.GetDatabase(DatabaseId);

        var leaseContainer = _cosmosClient.GetContainer(DatabaseId, LeaseContainerId);
        var changeFeedProcessor = _cosmosClient.GetContainer(DatabaseId, SourceContainerId)
            .GetChangeFeedProcessorBuilder<ShoppingCart>(processorName: "cache-processor",
                onChangesDelegate: HandleChangesAsync)
            .WithInstanceName($"cache-processor-{Guid.NewGuid().ToString()}")
            .WithLeaseContainer(leaseContainer)
            .Build();

        await changeFeedProcessor.StartAsync();
    }

    async Task HandleChangesAsync(
        ChangeFeedProcessorContext context,
        IReadOnlyCollection<ShoppingCart> changes,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Started handling changes for lease {LeaseToken}", context.LeaseToken);
        _logger.LogDebug("Change Feed request consumed {RequestCharge} RU.", context.Headers.RequestCharge);
        _logger.LogDebug("SessionToken {SessionToken}", context.Headers.Session);

        foreach (ShoppingCart item in changes)
        {
            string serialized = JsonConvert.SerializeObject(item);
            _logger.LogInformation(serialized);


            var db = _connectionMultiplexer.GetDatabase();
            var cachedCartString = await db.StringGetAsync($"cart_id_{item.StudentId}");
            if (!cachedCartString.IsNull)
            {
                var cachedShoppingCard = System.Text.Json.JsonSerializer.Deserialize<ShoppingCart>(cachedCartString.ToString());
                _logger.LogInformation("cached shopping card: " + cachedShoppingCard);
            }

            await Task.Delay(10);
        }

        _logger.LogDebug("Finished handling changes.");
    }
}
