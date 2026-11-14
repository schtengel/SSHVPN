using SSHVPN.Protocol;

namespace SSHVPN.UnitTests;

public class PacketSerializerTests
{
    [Fact]
    public async Task WriteAsync_ShouldWritePacketCorrectly()
    {
        // Arrange
        using var stream = new MemoryStream();

        var payload = new byte[] { 10, 20, 30 };

        var packet = new Packet(PacketType.Data, payload);

        // Act
        await PacketSerializer.WriteAsync(stream, packet);

        // Assert
        byte[] result = stream.ToArray();

        Assert.Equal(8, result.Length);
        Assert.Equal((byte)PacketType.Data, result[0]);

        // Big Endian: длина 3.
        Assert.Equal(new byte[] { 0, 0, 0, 3 }, result[1..5]);

        Assert.Equal(payload, result[5..]);
    }

    [Fact]
    public async Task ReadAsync_ShouldReadPacketCorrectly()
    {
        // Arrange
        byte[] data =
        {
            (byte)PacketType.Data,
            0, 0, 0, 3,
            10, 20, 30
        };

        using var stream = new MemoryStream(data);

        // Act
        Packet packet = await PacketSerializer.ReadAsync(stream);

        // Assert
        Assert.Equal(PacketType.Data, packet.Type);
        Assert.Equal(new byte[] { 10, 20, 30 }, packet.Payload);
    }

    [Fact]
    public async Task WriteAsync_ShouldSupportEmptyPayload()
    {
        // Arrange
        using var stream = new MemoryStream();

        var packet = new Packet(PacketType.Disconnect, []);

        // Act
        await PacketSerializer.WriteAsync(stream, packet);

        // Assert
        Assert.Equal(5, stream.Length);
    }

    [Fact]
    public async Task ReadAsync_ShouldRejectNegativePayloadLength()
    {
        // Arrange
        byte[] data =
        {
            (byte)PacketType.Data,
            255, 255, 255, 255
        };

        using var stream = new MemoryStream(data);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(
            () => PacketSerializer.ReadAsync(stream));
    }

    [Fact]
    public async Task WriteAsync_ShouldRejectOversizedPayload()
    {
        // Arrange
        using var stream = new MemoryStream();

        var packet = new Packet(
            PacketType.Data,
            new byte[65537]);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(
            () => PacketSerializer.WriteAsync(stream, packet));

        Assert.Equal(0, stream.Length);
    }
    
    [Fact]
    public async Task ReadAsync_ShouldRejectUnknownPacketType()
    {
        // Arrange
        byte[] data =
        {
            255,
            0, 0, 0, 0
        };

        using var stream = new MemoryStream(data);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(
            () => PacketSerializer.ReadAsync(stream));
    }
    
    [Fact]
    public async Task ReadAsync_ShouldHandlePartialReads()
    {
        // Arrange
        byte[] data =
        {
            (byte)PacketType.Data,
            0, 0, 0, 3,
            10, 20, 30
        };

        await using var stream = new PartialReadStream(data);

        // Act
        Packet packet = await PacketSerializer.ReadAsync(stream);

        // Assert
        Assert.Equal(PacketType.Data, packet.Type);
        Assert.Equal(new byte[] { 10, 20, 30 }, packet.Payload);
    }
}

internal sealed class PartialReadStream(byte[] data) : MemoryStream(data)
{
    public override ValueTask<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        return base.ReadAsync(
            buffer[..Math.Min(buffer.Length, 2)],
            cancellationToken);
    }
}