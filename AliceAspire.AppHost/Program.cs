var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");
var rabbitMq = builder.AddRabbitMQ("EventBus");

var apiService = builder.AddProject<Projects.AliceAspire_ApiService>("api-service")
    .WithReference(cache)
    .WithReference(rabbitMq);

builder.AddProject<Projects.AliceAspire_Web>("web-frontend")
    .WithReference(cache)
    .WithReference(rabbitMq)
    .WithReference(apiService);

builder.Build().Run();
