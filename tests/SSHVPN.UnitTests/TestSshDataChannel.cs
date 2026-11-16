using SSHVPN.Client.Core.Connection.Abstractions;

namespace SSHVPN.UnitTests;

public class TestSshDataChannel : ISshDataChannel
{
    private readonly TaskCompletionSource<bool> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    
    public Stream Input { get; } = new MemoryStream();

    public Stream Output { get; } = new MemoryStream();

    public Task Completion => _completion.Task;

    public void Complete()
    {
        _completion.TrySetResult(true);
    }
    
    public void Fail(Exception exception)
    {
        _completion.TrySetException(exception);
    }

    public bool IsDisposed { get; private set; }

    public async ValueTask DisposeAsync()
    {
        _completion.TrySetCanceled();
        
        IsDisposed = true;

        await Input.DisposeAsync();
        await Output.DisposeAsync();
    }
}