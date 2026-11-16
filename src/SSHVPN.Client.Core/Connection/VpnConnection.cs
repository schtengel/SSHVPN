using SSHVPN.Client.Core.Connection.Abstractions;

namespace SSHVPN.Client.Core.Connection;

public class VpnConnection(ISshTransport sshTransport, SshConnectionOptions options, VpnHandshake handshake) : IVpnConnection
{

    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private ISshDataChannel? _dataChannel;

    public event EventHandler<VpnConnectionLostEventArgs>? ConnectionLost;

    public bool IsConnected => sshTransport.IsConnected && _dataChannel is { Completion.IsCompleted: false };

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            if (IsConnected) return;

            try
            {
                var previousChannel = _dataChannel;

                if (previousChannel is not null)
                {
                    _dataChannel = null;

                    try
                    {
                        await previousChannel.DisposeAsync();
                    }
                    finally
                    {
                        sshTransport.Disconnect();
                    }
                }
                
                if (!sshTransport.IsConnected)
                    await sshTransport.ConnectAsync(options, cancellationToken);

                _dataChannel = await sshTransport.OpenDataChannelAsync(options.ServerCommand, cancellationToken);

                await handshake.PerformAsync(_dataChannel, cancellationToken);

                _ = MonitorDataChannelAsync(_dataChannel);
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

    private async Task MonitorDataChannelAsync(ISshDataChannel channel)
    {
        Exception? completionError = null;

        try
        {
            await channel.Completion;
        }
        catch (Exception ex)
        {
            completionError = ex;
        }

        await _connectionLock.WaitAsync();

        try
        {
            if (!ReferenceEquals(_dataChannel, channel)) return;

            _dataChannel = null;

            try
            {
                await channel.DisposeAsync();
            }
            catch (Exception ex)
            {
                completionError ??= ex;
            }

            try
            {
                sshTransport.Disconnect();
            }
            catch (Exception ex)
            {
                completionError ??= ex;
            }
        }
        finally
        {
            _connectionLock.Release();
        }

        ConnectionLost?.Invoke(this, new VpnConnectionLostEventArgs(completionError));
    }
}