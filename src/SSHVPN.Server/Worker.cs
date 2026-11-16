namespace SSHVPN.Server;

public class Worker(ServerSession session, IHostApplicationLifetime applicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Stream input = Console.OpenStandardInput();
        Stream output = Console.OpenStandardOutput();

        try
        {
            await session.RunAsync(input, output, stoppingToken);
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }
}
