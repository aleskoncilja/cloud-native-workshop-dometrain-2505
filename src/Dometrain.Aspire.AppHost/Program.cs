using Aspire.Hosting.Azure;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var mainDbUsername = builder.AddParameter("postgres-username");
var mainDbPassword = builder.AddParameter("postgres-password");

var mainDb = builder.AddPostgres("main-db", mainDbUsername, mainDbPassword, port: 5432)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .AddDatabase("dometrain");

var cartDb = builder.AddAzureCosmosDB("cosmosdb")
    .AddCosmosDatabase("cartdb");

var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent);

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddContainer("prometheus", "prom/prometheus")
    .WithBindMount("../../prometheus", "/etc/prometheus", isReadOnly: true)
    .WithHttpEndpoint(port: 9090, targetPort: 9090);

var grafana = builder.AddContainer("grafana", "grafana/grafana")
    .WithBindMount("../../grafana/config", "/etc/grafana", isReadOnly: true)
    .WithBindMount("../../grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: true)
    .WithHttpEndpoint(targetPort: 3000, name: "http");

builder.AddProject<Projects.Dometrain_Cart_Processor>("dometrain-cart-processor")
    .WithReference(cartDb);

var mainApi = builder.AddProject<Projects.Dometrain_Monolith_Api>("dometrain-api")
    .WithReplicas(1)
    .WithReference(mainDb)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("http"));

builder.AddProject<Projects.Dometrain_Cart_Api>("cart-api")
    .WithReference(cartDb)
    .WithReference(redis)
    .WithEnvironment("MainApi__BaseUrl", mainApi.GetEndpoint("http"));

var app = builder.Build();
    
app.Run();
