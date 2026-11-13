namespace SSHVPN.Client.Core.Connection.Abstraction;

public interface IVpnConnection
{
    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);
    bool IsConnected { get; }
}