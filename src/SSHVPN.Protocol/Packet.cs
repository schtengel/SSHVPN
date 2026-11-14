namespace SSHVPN.Protocol;

public sealed record Packet(PacketType Type, byte[] Payload);