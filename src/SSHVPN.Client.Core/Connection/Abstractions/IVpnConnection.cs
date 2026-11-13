namespace SSHVPN.Client.Core.Connection.Abstractions;

public interface IVpnConnection
{
    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);
    bool IsConnected { get; }
}