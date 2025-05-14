var builder = DistributedApplication.CreateBuilder(args);

var mainDbUsername = builder.AddParameter("postgres-username");
var mainDbPassword = builder.AddParameter("postgres-password");

var mainDb = builder.AddPostgres("main-db", mainDbUsername, mainDbPassword, port: 5432)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .AddDatabase("dometrain");

var cartDb = builder.AddAzureCosmosDB("cosmosdb")
    .AddCosmosDatabase("cartdb");

builder.AddProject<Projects.Dometrain_Monolith_Api>("dometrain-api")
    .WithReference(mainDb)
    .WithReference(cartDb);

var app = builder.Build();
    
app.Run();
