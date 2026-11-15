using System.Text;
using SSHVPN.Protocol;

namespace SSHVPN.Server;

public sealed class ServerSession(ILogger<ServerSession> logger)
{
    public async Task RunAsync(Stream input, Stream output, CancellationToken cancellationToken)
    {
        logger.LogInformation("Server session started.");

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

            switch (packet.Type)
            {
                case PacketType.Handshake:
                    logger.LogInformation("Handshake packet received.");
                    break;
                    
                case PacketType.Data:
                    logger.LogInformation("Data packet received. Payload size: {Size}", packet.Payload.Length);

                    await PacketSerializer.WriteAsync(output, packet, cancellationToken);
                    break;
                
                case PacketType.Disconnect:
                    logger.LogInformation("Disconnect packet received.");
                    return;
                
                case PacketType.Error:
                    logger.LogWarning("Client error: {Message}", Encoding.UTF8.GetString(packet.Payload));
                    break;
                
                default:
                    throw new InvalidDataException($"Unsupported packet type: {packet.Type}");
            }
        }
    }
}