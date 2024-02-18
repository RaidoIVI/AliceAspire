var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");
var rabbitMq = builder.AddRabbitMQContainer("EventBus");

var apiService = builder.AddProject<Projects.AliceAspire_ApiService>("api-service");

builder.AddProject<Projects.AliceAspire_Web>("web-frontend")
    .WithReference(cache)
    .WithReference(rabbitMq)
    .WithReference(apiService);



builder.Build().Run();
