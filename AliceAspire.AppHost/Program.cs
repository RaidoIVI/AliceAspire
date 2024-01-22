var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");
var rabbitMq = builder.AddRabbitMQContainer("EventBus");

var apiService = builder.AddProject<Projects.AliceAspire_ApiService>("apiservice");

builder.AddProject<Projects.AliceAspire_Web>("webfrontend")
    .WithReference(cache)
    .WithReference(rabbitMq)
    .WithReference(apiService);



builder.Build().Run();
