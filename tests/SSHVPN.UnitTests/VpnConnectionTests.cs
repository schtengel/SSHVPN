
using SSHVPN.Client.Core.Connection;
using SSHVPN.Protocol;

namespace SSHVPN.UnitTests;

public class VpnConnectionTests
{
    private readonly SshConnectionOptions _options = new(
        Host: "192.168.1.100",
        Port: 22,
        Username: "vpnuser",
        PrivateKeyPath: "C:\\Keys\\vpn_key",
        ExpectedHostKeyFingerprint: "test-fingerprint",
        ServerCommand: "dotnet server.dll");

    [Fact]
    public async Task ConnectAsync_ShouldCompleteHandshake()
    {
        // Arrange
        var transport = new TestSshTransport();
        var handshake = new VpnHandshake();
        var connection = new VpnConnection(transport, _options, handshake);

        // Act
        await connection.ConnectAsync(CancellationToken.None);

        // Assert
        Assert.True(connection.IsConnected);
        Assert.Equal(1, transport.ConnectCalls);
        Assert.Equal(1, transport.OpenDataChannelCalls);
        Assert.Equal(_options, transport.LastOptions);
        Assert.Equal(_options.ServerCommand, transport.LastCommand);
        Assert.NotNull(transport.DataChannel);
        Assert.False(transport.DataChannel.IsDisposed);
    }

    [Fact]
    public async Task ConnectAsync_WhenAlreadyConnected_ShouldNotConnectAgain()
    {
        // Arrange
        var transport = new TestSshTransport();
        var connection = new VpnConnection(
            transport,
            _options,
            new VpnHandshake());

        // Act
        await connection.ConnectAsync(CancellationToken.None);
        await connection.ConnectAsync(CancellationToken.None);

        // Assert
        Assert.True(connection.IsConnected);
        Assert.Equal(1, transport.ConnectCalls);
        Assert.Equal(1, transport.OpenDataChannelCalls);
    }

    [Fact]
    public async Task ConnectAsync_WhenHandshakeFails_ShouldCleanupResources()
    {
        // Arrange
        var transport = new TestSshTransport
        {
            HandshakeResponse = new Packet(
                PacketType.Error,
                System.Text.Encoding.UTF8.GetBytes("Handshake rejected."))
        };

        var connection = new VpnConnection(
            transport,
            _options,
            new VpnHandshake());

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => connection.ConnectAsync(CancellationToken.None));

        // Assert
        Assert.False(connection.IsConnected);
        Assert.Equal(1, transport.DisconnectCalls);
        Assert.NotNull(transport.DataChannel);
        Assert.True(transport.DataChannel.IsDisposed);
    }

    [Fact]
    public async Task DisconnectAsync_ShouldDisposeChannelAndDisconnectTransport()
    {
        // Arrange
        var transport = new TestSshTransport();
        var connection = new VpnConnection(
            transport,
            _options,
            new VpnHandshake());

        await connection.ConnectAsync(CancellationToken.None);

        var channel = transport.DataChannel;

        // Act
        await connection.DisconnectAsync(CancellationToken.None);

        // Assert
        Assert.False(connection.IsConnected);
        Assert.True(channel!.IsDisposed);
        Assert.Equal(1, transport.DisconnectCalls);
    }

    [Fact]
    public async Task ConnectAsync_WhenCancelled_ShouldNotConnect()
    {
        // Arrange
        var transport = new TestSshTransport();
        var connection = new VpnConnection(
            transport,
            _options,
            new VpnHandshake());

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => connection.ConnectAsync(cts.Token));

        Assert.False(connection.IsConnected);
        Assert.Equal(0, transport.ConnectCalls);
        Assert.Equal(0, transport.OpenDataChannelCalls);
    }
    
    [Fact]
    public async Task ConnectAsync_WhenServerProcessExits_ShouldDisconnectTransport()
    {
        // Arrange
        var transport = new TestSshTransport();

        var connection = new VpnConnection(
            transport,
            _options,
            new VpnHandshake());

        await connection.ConnectAsync(CancellationToken.None);

        var channel = transport.DataChannel!;

        // Act
        channel.Complete();

        await transport.DisconnectedSignal.Task.WaitAsync(
            TimeSpan.FromSeconds(2));

        // Assert
        Assert.False(connection.IsConnected);
        Assert.True(channel.IsDisposed);
        Assert.Equal(1, transport.DisconnectCalls);
    }
}
