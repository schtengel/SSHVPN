using SSHVPN.Client.Core.Connection.Abstractions;

namespace SSHVPN.Client.Core.Connection;

public class VpnConnection(ISshTransport sshTransport, SshConnectionOptions options) : IVpnConnection
{

    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public bool IsConnected => sshTransport.IsConnected;

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            if (sshTransport.IsConnected) return;

            await sshTransport.ConnectAsync(options, cancellationToken);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            sshTransport.Disconnect();
        }
        finally
        {
            _connectionLock.Release();
        }
    }
}