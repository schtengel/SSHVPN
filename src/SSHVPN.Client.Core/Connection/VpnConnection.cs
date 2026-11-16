using SSHVPN.Client.Core.Connection.Abstractions;

namespace SSHVPN.Client.Core.Connection;

public class VpnConnection(ISshTransport sshTransport, SshConnectionOptions options, VpnHandshake handshake) : IVpnConnection
{

    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private ISshDataChannel? _dataChannel;

    public bool IsConnected => sshTransport.IsConnected && _dataChannel is not null;

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            if (IsConnected) return;

            try
            {
                if (!sshTransport.IsConnected)
                    await sshTransport.ConnectAsync(options, cancellationToken);

                _dataChannel = await sshTransport.OpenDataChannelAsync(options.ServerCommand, cancellationToken);

                await handshake.PerformAsync(_dataChannel, cancellationToken);
            }
            catch
            {
                var channel = _dataChannel;
                _dataChannel = null;

                try
                {
                    if (channel is not null)
                        await channel.DisposeAsync();
                }
                finally
                {
                    sshTransport.Disconnect();
                }

                throw;
            }
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
            var channel = _dataChannel;
            _dataChannel = null;

            try
            {
                if (channel is not null)
                    await channel.DisposeAsync();
            }
            finally
            {
                sshTransport.Disconnect();
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }
}