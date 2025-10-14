var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Yippy_Emailing>("yippy-emailing");

builder.AddProject<Projects.Yippy_Identity>("yippy-identity");

builder.AddProject<Projects.Yippy_News>("yippy-news");

builder.AddProject<Projects.Yippy_Proxy>("yippy-proxy");

builder.AddProject<Projects.Yippy_Templating>("yippy-templating");

builder.Build().Run();
