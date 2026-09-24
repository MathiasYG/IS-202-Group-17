var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.WebApplicationInAspire>("webapplicationinaspire");

builder.Build().Run();

