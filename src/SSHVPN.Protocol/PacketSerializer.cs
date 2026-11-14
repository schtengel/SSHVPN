using System.Buffers.Binary;

namespace SSHVPN.Protocol;

public static class PacketSerializer
{
    private const int HeaderSize = 5;
    private const int MaxPayloadSize = 65536;
    
    public static async Task WriteAsync(Stream stream, Packet packet, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(packet);

        if (packet.Payload.Length > MaxPayloadSize)
            throw new InvalidDataException("Packet size is too large.");
        
        byte[] header = new byte[HeaderSize];

        header[0] = (byte)packet.Type;

        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(1), packet.Payload.Length);

        await stream.WriteAsync(header, cancellationToken);

        if (packet.Payload.Length > 0)
        {
            await stream.WriteAsync(
                packet.Payload.AsMemory(), cancellationToken);
        }

        await stream.FlushAsync(cancellationToken);
    }

    public static async Task<Packet> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        byte[] header = new byte[HeaderSize];

        await stream.ReadExactlyAsync(header.AsMemory(), cancellationToken);

        var type = (PacketType)header[0];

        if (!Enum.IsDefined(type))
            throw new InvalidDataException($"Unknown packet type: {(byte)type}");

        int payloadLength = BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(1, 4));

        if (payloadLength < 0 || payloadLength > MaxPayloadSize) 
            throw new InvalidDataException("Invalid packet payload size.");

        byte[] payload = new byte[payloadLength];

        if (payloadLength > 0)
            await stream.ReadExactlyAsync(payload.AsMemory(), cancellationToken);

        return new Packet(type, payload);

    }
}