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
    .WithRedisInsight();

builder.AddProject<Projects.Dometrain_Monolith_Api>("dometrain-api")
    .WithReplicas(5)
    .WithReference(mainDb)
    .WithReference(cartDb)
    .WithReference(redis);

var app = builder.Build();
    
app.Run();
