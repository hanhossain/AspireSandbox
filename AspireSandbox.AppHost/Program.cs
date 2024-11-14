var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.AspireSandbox_ApiService>("apiservice")
    .WithReference(redis)
    .WaitFor(redis);

builder.AddProject<Projects.AspireSandbox_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
