using SSHVPN.Client.Core.Connection.Abstractions;

namespace SSHVPN.UnitTests;

public class TestSshDataChannel : ISshDataChannel
{
    public Stream Input { get; } = new MemoryStream();

    public Stream Output { get; } = new MemoryStream();

    public Task Completion => Task.CompletedTask;

    public bool IsDisposed { get; private set; }

    public async ValueTask DisposeAsync()
    {
        IsDisposed = true;

        await Input.DisposeAsync();
        await Output.DisposeAsync();
    }
}