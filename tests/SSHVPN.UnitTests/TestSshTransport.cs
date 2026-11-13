using SSHVPN.Client.Core.Connection;
using SSHVPN.Client.Core.Connection.Abstractions;

namespace SSHVPN.UnitTests;

public class TestSshTransport : ISshTransport
{
    public bool IsConnected { get; private set; }
    
    public int ConnectCalls { get; private set; }
    
    public int DisconnectCalls { get; private set; }
    
    public SshConnectionOptions? LastOptions { get; private set; }
    
    
    public Task ConnectAsync(SshConnectionOptions options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ConnectCalls++;
        LastOptions = options;
        IsConnected = true;

        return Task.CompletedTask;
    }

    public void Disconnect()
    {
        DisconnectCalls++;
        IsConnected = false;
    }
    
    public string CommandOutput { get; set; } = "test output";

    public string? LastCommand { get; private set; }

    public Task<string> ExecuteCommandAsync(
        string command,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsConnected)
            throw new InvalidOperationException(
                "SSH connection is not established.");

        LastCommand = command;

        return Task.FromResult(CommandOutput);
    }
}