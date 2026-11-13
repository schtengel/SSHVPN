using Renci.SshNet;
using SSHVPN.Client.Core.Connection;
using SSHVPN.Client.Core.Connection.Abstractions;

namespace SSHVPN.Client.Infrastructure.Ssh;

public sealed class SshTransport : ISshTransport, IDisposable
{
    private SshClient? _client;
    private PrivateKeyFile? _privateKeyFile;

    public bool IsConnected => _client?.IsConnected == true;

    public async Task ConnectAsync(
        SshConnectionOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (IsConnected)
            throw new InvalidOperationException("SSH connection is already established.");

        ValidateOptions(options);

        var privateKeyFile = new PrivateKeyFile(options.PrivateKeyPath);

        var authenticationMethod =
            new PrivateKeyAuthenticationMethod(
                options.Username,
                privateKeyFile);

        var client = new SshClient(
            options.Host,
            options.Port,
            options.Username,
            privateKeyFile);

        client.HostKeyReceived += (_, eventArgs) =>
        {
            eventArgs.CanTrust = string.Equals(eventArgs.FingerPrintSHA256,
                options.ExpectedHostKeyFingerprint,
                StringComparison.Ordinal);
        };

        try
        {
            await client.ConnectAsync(cancellationToken);

            _privateKeyFile = privateKeyFile;
            _client = client;
        }
        catch
        {
            client.Dispose();
            privateKeyFile.Dispose();

            throw;
        }
    }

    public void Disconnect()
    {
        if (_client is null)
            return;
        
        if(_client.IsConnected)
            _client.Disconnect();

        Dispose();
    }

    private static void ValidateOptions(SshConnectionOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Host))
            throw new ArgumentException("Host is required.");

        if (options.Port is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(
                nameof(options.Port));

        if (string.IsNullOrWhiteSpace(options.Username))
            throw new ArgumentException("Username is required.");

        if (string.IsNullOrWhiteSpace(options.PrivateKeyPath))
            throw new ArgumentException("Private key path is required.");

        if (!File.Exists(options.PrivateKeyPath))
            throw new FileNotFoundException(
                "SSH private key was not found.",
                options.PrivateKeyPath);

        if (string.IsNullOrWhiteSpace(
                options.ExpectedHostKeyFingerprint))
        {
            throw new ArgumentException(
                "Expected host key fingerprint is required.");
        }
    }

    public void Dispose()
    {
        _client?.Dispose();
        _privateKeyFile?.Dispose();

        _client = null;
        _privateKeyFile = null;
        
        
        GC.SuppressFinalize(this);
    }
}