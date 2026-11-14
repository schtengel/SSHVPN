using Renci.SshNet;
using SSHVPN.Client.Core.Connection.Abstractions;

namespace SSHVPN.Client.Infrastructure.Ssh;

public class SshDataChannel : ISshDataChannel
{
    private readonly SshCommand _command;
    private readonly Stream _input;
    private readonly Stream _output;
    private readonly CancellationTokenSource _lifetimeCts = new();

    private readonly Task _completion;
    private bool _disposed;
    
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        _disposed = true;

        try
        {
            await _input.DisposeAsync();
        }
        finally
        {
            _lifetimeCts.Cancel();

            try
            {
                await _completion;
            }
            catch (OperationCanceledException)
            {

            }
            finally
            {
                _command.Dispose();
                _lifetimeCts.Dispose();
            }
        }
    }

    public Stream Input => _input;
    public Stream Output => _output;
    public Task Completion => _completion;

    internal SshDataChannel(SshCommand command)
    {
        _command = command;

        _completion = _command.ExecuteAsync(_lifetimeCts.Token);

        _input = _command.CreateInputStream();
        _output = _command.OutputStream;
    }
    
    
}