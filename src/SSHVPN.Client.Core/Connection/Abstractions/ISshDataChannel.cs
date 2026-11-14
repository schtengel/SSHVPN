namespace SSHVPN.Client.Core.Connection.Abstractions;

public interface ISshDataChannel : IAsyncDisposable
{
    Stream Input { get; }
    Stream Output { get; }
    Task Completion { get; }
}