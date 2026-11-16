using System.Text;
using SSHVPN.Protocol;

namespace SSHVPN.Server;

public sealed class ServerSession(ILogger<ServerSession> logger)
{
    public async Task RunAsync(Stream input, Stream output, CancellationToken cancellationToken)
    {
        logger.LogInformation("Server session started.");

        try
        {
            if (!await PerformHandshakeAsync(input, output, cancellationToken))
                return;

            await ProcessPacketAsync(input, output, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Server session cancelled.");
        }
        finally
        {
            logger.LogInformation("Server session ended.");
        }
        
        logger.LogInformation("Server session ended.");
    }
    
    private async Task<bool> PerformHandshakeAsync(Stream input, Stream output, CancellationToken cancellationToken)
    {
        Packet packet;
        
        try
        {
            packet = await PacketSerializer.ReadAsync(input, cancellationToken);
        }
        catch (EndOfStreamException)
        {
            logger.LogWarning("Connection closed before handshake.");
            return false;
        }

        if (packet.Type != PacketType.Handshake)
        {
            logger.LogWarning("Expected handshake packet.");
            await SendErrorAsync(output, "Handshake required.", cancellationToken);
            return false;
        }

        if (!ProtocolVersion.TryParse(packet.Payload, out var clientVersion))
        {
            logger.LogWarning("Invalid handshake payload.");
            await SendErrorAsync(output, "Invalid handshake payload.", cancellationToken);
            return false;
        }

        if (clientVersion != ProtocolVersion.Current)
        {
            logger.LogWarning("Unsupported protocol version: {Major}.{Minor}", clientVersion.Major, clientVersion.Minor);

            await SendErrorAsync(output, "Unsupported protocol version.", cancellationToken);

            return false;
        }

        var response = new Packet(PacketType.Handshake, ProtocolVersion.Current.ToPayload());

        await PacketSerializer.WriteAsync(output, response, cancellationToken);
        
        logger.LogInformation("Handshake completed. Protocol version: {Major}.{Minor}", clientVersion.Major, clientVersion.Minor);

        return true;
    }

    private static Task SendErrorAsync(Stream output, string message, CancellationToken cancellationToken)
    {
        var packet = new Packet(PacketType.Error, Encoding.UTF8.GetBytes(message));

        return PacketSerializer.WriteAsync(output, packet, cancellationToken);
    }

    private async Task ProcessPacketAsync(Stream input, Stream output, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Packet packet;

            try
            {
                packet = await PacketSerializer.ReadAsync(input, cancellationToken);
            }
            catch (EndOfStreamException)
            {
                logger.LogInformation("Client closed the connection.");
                break;
            }
            
            switch(packet.Type)
            {
                case PacketType.Data:
                    await PacketSerializer.WriteAsync(output, packet, cancellationToken);
                    break;

                case PacketType.Disconnect:
                    logger.LogInformation("Disconnect packet received.");
                    return;

                case PacketType.Error:
                    logger.LogWarning("Client error: {Message}", Encoding.UTF8.GetString(packet.Payload));
                    break;

                case PacketType.Handshake:
                    logger.LogWarning("Unexpected handshake packet.");
                    await SendErrorAsync(output, "Handshake already completed.", cancellationToken);
                    return;

                default:
                    throw new InvalidDataException($"Unsupported packet type: {packet.Type}");}
        }
    }
}