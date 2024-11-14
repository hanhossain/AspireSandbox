var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("cache");

var kafka = builder.AddKafka("kafka")
    .WithKafkaUI();

var apiService = builder.AddProject<Projects.AspireSandbox_ApiService>("apiservice")
    .WithReference(redis)
    .WithReference(kafka)
    .WaitFor(redis)
    .WaitFor(kafka);

builder.AddProject<Projects.AspireSandbox_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
