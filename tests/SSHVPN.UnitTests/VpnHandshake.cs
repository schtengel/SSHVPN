
using SSHVPN.Client.Core.Connection;
using SSHVPN.Client.Core.Connection.Abstractions;
using SSHVPN.Protocol;

namespace SSHVPN.UnitTests;

public class VpnHandshakeTests
{
    [Fact]
    public async Task PerformAsync_ShouldCompleteWithMatchingVersion()
    {
        // Arrange
        var response = new Packet(
            PacketType.Handshake,
            ProtocolVersion.Current.ToPayload());

        var channel = await CreateChannelAsync(response);
        var handshake = new VpnHandshake();

        // Act
        await handshake.PerformAsync(channel, CancellationToken.None);

        // Assert
        channel.Input.Position = 0;

        var request = await PacketSerializer.ReadAsync(
            channel.Input,
            CancellationToken.None);

        Assert.Equal(PacketType.Handshake, request.Type);
        Assert.Equal(
            ProtocolVersion.Current.ToPayload(),
            request.Payload);
    }

    [Fact]
    public async Task PerformAsync_WhenServerReturnsError_ShouldThrow()
    {
        // Arrange
        var response = new Packet(
            PacketType.Error,
            System.Text.Encoding.UTF8.GetBytes("Handshake rejected."));

        var channel = await CreateChannelAsync(response);
        var handshake = new VpnHandshake();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handshake.PerformAsync(channel, CancellationToken.None));

        // Assert
        Assert.Contains("Handshake rejected.", exception.Message);
    }

    [Fact]
    public async Task PerformAsync_WhenServerReturnsUnexpectedPacket_ShouldThrow()
    {
        // Arrange
        var response = new Packet(PacketType.Data, []);

        var channel = await CreateChannelAsync(response);
        var handshake = new VpnHandshake();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(
            () => handshake.PerformAsync(channel, CancellationToken.None));
    }

    [Fact]
    public async Task PerformAsync_WhenServerReturnsInvalidPayload_ShouldThrow()
    {
        // Arrange
        var response = new Packet(
            PacketType.Handshake,
            [1]);

        var channel = await CreateChannelAsync(response);
        var handshake = new VpnHandshake();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(
            () => handshake.PerformAsync(channel, CancellationToken.None));
    }

    [Fact]
    public async Task PerformAsync_WhenServerVersionIsUnsupported_ShouldThrow()
    {
        // Arrange
        var response = new Packet(
            PacketType.Handshake,
            [2, 0]);

        var channel = await CreateChannelAsync(response);
        var handshake = new VpnHandshake();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handshake.PerformAsync(channel, CancellationToken.None));
    }

    private static async Task<TestSshDataChannel> CreateChannelAsync(
        Packet response)
    {
        var input = new MemoryStream();
        var output = new MemoryStream();

        await PacketSerializer.WriteAsync(
            output,
            response,
            CancellationToken.None);

        output.Position = 0;

        return new TestSshDataChannel(input, output);
    }

    private sealed class TestSshDataChannel(
        Stream input,
        Stream output) : ISshDataChannel
    {
        public Stream Input { get; } = input;

        public Stream Output { get; } = output;

        public Task Completion => Task.CompletedTask;

        public ValueTask DisposeAsync()
        {
            Input.Dispose();
            Output.Dispose();

            return ValueTask.CompletedTask;
        }
    }
}
