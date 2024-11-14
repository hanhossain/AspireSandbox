var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.AspireSandbox_ApiService>("apiservice");

builder.AddProject<Projects.AspireSandbox_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
