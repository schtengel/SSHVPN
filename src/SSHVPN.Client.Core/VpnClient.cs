using SSHVPN.Client.Core.Connection.Abstractions;

namespace SSHVPN.Client.Core;

public class VpnClient(IVpnConnection vpnConnection)
{
    public bool IsConnected => vpnConnection.IsConnected;

    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        return vpnConnection.ConnectAsync(cancellationToken);
    }

    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        return vpnConnection.DisconnectAsync(cancellationToken);
    }
}