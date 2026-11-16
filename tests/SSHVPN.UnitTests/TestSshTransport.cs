
using SSHVPN.Client.Core.Connection;
using SSHVPN.Client.Core.Connection.Abstractions;
using SSHVPN.Protocol;

namespace SSHVPN.UnitTests;

public class TestSshTransport : ISshTransport
{
    public bool IsConnected { get; private set; }

    public int ConnectCalls { get; private set; }

    public int DisconnectCalls { get; private set; }

    public int OpenDataChannelCalls { get; private set; }

    public SshConnectionOptions? LastOptions { get; private set; }

    public string? LastCommand { get; private set; }

    public TestSshDataChannel? DataChannel { get; private set; }

    public Packet HandshakeResponse { get; set; } = new(
        PacketType.Handshake,
        ProtocolVersion.Current.ToPayload());

    public Task ConnectAsync(
        SshConnectionOptions options,
        CancellationToken cancellationToken)
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

    public async Task<ISshDataChannel> OpenDataChannelAsync(
        string command,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsConnected)
            throw new InvalidOperationException(
                "SSH connection is not established.");

        OpenDataChannelCalls++;
        LastCommand = command;

        var channel = new TestSshDataChannel();

        await PacketSerializer.WriteAsync(
            channel.Output,
            HandshakeResponse,
            cancellationToken);

        channel.Output.Position = 0;

        DataChannel = channel;

        return channel;
    }
}
