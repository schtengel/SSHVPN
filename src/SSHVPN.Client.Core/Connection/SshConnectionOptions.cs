namespace SSHVPN.Client.Core.Connection;

public sealed record SshConnectionOptions(
    string Host,
    int Port,
    string Username,
    string PrivateKeyPath,
    string ExpectedHostKeyFingerprint);