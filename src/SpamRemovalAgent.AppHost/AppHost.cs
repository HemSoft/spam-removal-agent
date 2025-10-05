var builder = DistributedApplication.CreateBuilder(args);

// Add reference to main console application
var spamAgent = builder.AddProject<Projects.SpamRemovalAgent>("spam-agent");

builder.Build().Run();
