using SSHVPN.Client.Core.Connection;

namespace SSHVPN.UnitTests;

public class VpnConnectionTests
{
    private readonly SshConnectionOptions _options = new(
        Host: "192.168.1.100",
        Port: 22,
        Username: "vpnuser",
        PrivateKeyPath: "C:\\Keys\\vpn_key",
        ExpectedHostKeyFingerprint: "test-fingerprint");
    
    [Fact]
    public async Task ConnectAsync_ShouldConnectTransport()
    {
        var transport = new TestSshTransport();
        var connection = new VpnConnection(transport, _options);

        await connection.ConnectAsync(CancellationToken.None);
        
        Assert.True(connection.IsConnected);
        Assert.Equal(1, transport.ConnectCalls);
        Assert.Equal(_options, transport.LastOptions);
    }

    [Fact]
    public async Task ConnectAsync_WhenAlreadyConnected_ShouldNotConnectedAgain()
    {
        var transport = new TestSshTransport();
        var connection = new VpnConnection(transport, _options);

        await connection.ConnectAsync(CancellationToken.None);
        await connection.ConnectAsync(CancellationToken.None);
        
        Assert.Equal(1, transport.ConnectCalls);
        Assert.True(connection.IsConnected);
    }
    
    [Fact]
    public async Task DisconnectAsync_ShouldDisconnectTransport()
    {
        // Arrange
        var transport = new TestSshTransport();
        var connection = new VpnConnection(transport, _options);

        await connection.ConnectAsync(CancellationToken.None);

        // Act
        await connection.DisconnectAsync(CancellationToken.None);

        // Assert
        Assert.False(connection.IsConnected);
        Assert.Equal(1, transport.DisconnectCalls);
    }

    [Fact]
    public async Task ConnectAsync_WhenCancelled_ShouldNotConnect()
    {
        // Arrange
        var transport = new TestSshTransport();
        var connection = new VpnConnection(transport, _options);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.ConnectAsync(cts.Token));

        Assert.False(connection.IsConnected);
        Assert.Equal(0, transport.ConnectCalls);
    }
}