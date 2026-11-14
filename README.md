# SSHVPN

SSHVPN is an experimental VPN project written in C# and .NET 10. It aims to provide a VPN connection between a Windows client and a Linux server using SSH as the transport layer.

The project is currently under active development. It is not yet a functional VPN and should not be used in production.

## Features

- SSH connection using private key authentication.
- SSH server host key fingerprint verification.
- Remote command execution.
- Persistent bidirectional SSH data channel.
- Custom binary communication protocol (in development).
- Windows desktop client built with WPF.

## Architecture

The solution follows a layered architecture with separate responsibilities for connection management, transport implementation, and protocol handling.

| Project | Description |
|---|---|
| `SSHVPN.Client` | Windows desktop application built with WPF. |
| `SSHVPN.Client.Core` | Core connection logic, abstractions, and configuration models. |
| `SSHVPN.Client.Infrastructure` | SSH transport implementation using SSH.NET. |
| `SSHVPN.Protocol` | Shared binary communication protocol between client and server. |
| `SSHVPN.Server` | Linux server application. |
| `SSHVPN.UnitTests` | Unit tests for core components and protocol functionality. |

## Technologies

- C#
- .NET 10
- WPF
- SSH.NET
- xUnit
- Linux

## Planned Architecture

The intended data flow:

```text
Windows Client
      |
      v
   Wintun
      |
      v
 SSHVPN.Client
      |
      v
 SSHVPN.Protocol
      |
      v
  SSH Channel
      |
      v
  SSHVPN.Server
      |
      v
     TUN
      |
      v
   Internet
```

SSH is used as the transport mechanism, while the custom protocol defines how network packets and control messages are exchanged between the client and server.

## Getting Started

### Requirements

- .NET 10 SDK
- Windows for the WPF client
- Linux server with OpenSSH installed
- SSH key pair for authentication

### Configuration

Configure the SSH connection in `appsettings.json`:

```json
{
  "Ssh": {
    "Host": "your-server-ip",
    "Port": 22,
    "Username": "your-ssh-user",
    "PrivateKeyPath": "path-to-private-key",
    "ExpectedHostKeyFingerprint": "your-server-fingerprint"
  }
}
```

Do not commit private keys or personal server configuration.

### Build

```bash
dotnet restore SSHVPN.slnx
dotnet build SSHVPN.slnx
dotnet test SSHVPN.slnx
```

## Security Notice

This project is experimental and has not undergone a security audit. It is not intended for production use.

A successful SSH connection does not mean that VPN traffic is being routed through the tunnel.
