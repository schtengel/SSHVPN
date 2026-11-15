using SSHVPN.Server;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton<ServerSession>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
