namespace SSHVPN.Client.Core.Connection.Abstractions;

public sealed class VpnConnectionLostEventArgs(Exception? error) : EventArgs
{
    public Exception? Error { get; } = error;

    public string Message =>
        Error is null ? "Server process exited unexpectedly." : $"Connection lost: {Error.Message}";
}