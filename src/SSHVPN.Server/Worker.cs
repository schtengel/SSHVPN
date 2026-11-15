namespace SSHVPN.Server;

public class Worker(ServerSession session) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Stream input = Console.OpenStandardInput();
        Stream output = Console.OpenStandardOutput();

        await session.RunAsync(input, output, stoppingToken);
    }
}
