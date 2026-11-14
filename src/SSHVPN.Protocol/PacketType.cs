namespace SSHVPN.Protocol;

public enum PacketType : byte
{
    Handshake = 1,
    Data = 2,
    Disconnect = 3,
    Error = 4
}
