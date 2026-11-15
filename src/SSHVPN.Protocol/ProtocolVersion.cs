namespace SSHVPN.Protocol;

public readonly record struct ProtocolVersion(byte Major, byte Minor)
{
    public static ProtocolVersion Current { get; } = new(1, 0);

    public byte[] ToPayload()
    {
        return [Major, Minor];
    }

    public static bool TryParse(byte[] payload, out ProtocolVersion version)
    {
        if (payload.Length != 2)
        {
            version = default;
            return false;
        }

        version = new ProtocolVersion(payload[0], payload[1]);
        return true;
    }
}