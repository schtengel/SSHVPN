using SSHVPN.Client.Core.Connection.Abstractions;

namespace SSHVPN.UnitTests;

public class TestVpnConnection : IVpnConnection
{
    public event EventHandler<VpnConnectionLostEventArgs>? ConnectionLost;

    public void RaiseConnectionLost(Exception? error = null)
    {
        ConnectionLost?.Invoke(
            this,
            new VpnConnectionLostEventArgs(error));
    }
    
    public bool IsConnected { get; private set; }

    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return Task.FromCanceled(cancellationToken);
        
        IsConnected = true;
        return Task.CompletedTask;
    }

    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        IsConnected = false;
        return Task.CompletedTask;
    }
}