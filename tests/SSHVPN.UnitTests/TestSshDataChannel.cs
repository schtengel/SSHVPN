using SSHVPN.Client.Core.Connection.Abstractions;

namespace SSHVPN.UnitTests;

public class TestSshDataChannel : ISshDataChannel
{
    public async ValueTask DisposeAsync()
    {
        await Input.DisposeAsync();
        await Output.DisposeAsync();
    }

    public Stream Input { get; } = new MemoryStream();
    public Stream Output { get; } = new MemoryStream();
    
    public Task Completion => Task.CompletedTask;
}