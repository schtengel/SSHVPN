namespace SSHVPN.Client.Core.Connection.Abstractions;

public interface ISshTransport
{
    bool IsConnected { get; }

    Task ConnectAsync(
        SshConnectionOptions options,
        CancellationToken cancellationToken);

    void Disconnect();
}