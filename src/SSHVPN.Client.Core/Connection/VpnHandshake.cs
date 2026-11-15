using System.Text;
using SSHVPN.Client.Core.Connection.Abstractions;
using SSHVPN.Protocol;

namespace SSHVPN.Client.Core.Connection;

public sealed class VpnHandshake
{
    public async Task PerformAsync(ISshDataChannel channel, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(channel);

        var handshake = new Packet(PacketType.Handshake, ProtocolVersion.Current.ToPayload());

        await PacketSerializer.WriteAsync(channel.Input, handshake, cancellationToken);

        Packet response = await PacketSerializer.ReadAsync(channel.Output, cancellationToken);

        ValidateResponse(response);
    }

    private static void ValidateResponse(Packet response)
    {
        if (response.Type == PacketType.Error)
        {
            string error = Encoding.UTF8.GetString(response.Payload);

            throw new InvalidOperationException($"Server rejected handshake: {error}");
        }

        if (response.Type != PacketType.Handshake)
        {
            throw new InvalidDataException($"Unexpected packet type: {response.Type}");
        }

        if (!ProtocolVersion.TryParse(response.Payload, out var serverVersion))
        {
            throw new InvalidDataException($"Invalid server handshake payload.");
        }

        if (serverVersion != ProtocolVersion.Current)
        {
            throw new InvalidOperationException($"Unsupported server protocol version: " +
                                                $"{serverVersion.Major}.{serverVersion.Minor}");
        }
    }
}