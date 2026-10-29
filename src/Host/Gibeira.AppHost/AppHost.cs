var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Gibeira_Web>("web");

builder.Build().Run();