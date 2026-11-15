using System.IO;
using System.Text;
using System.Windows;
using SSHVPN.Client.Core.Connection;
using SSHVPN.Protocol;
using SSHVPN.Client.Core.Connection.Abstractions;

namespace SSHVPN.Client;

public partial class MainWindow : Window
{
    private readonly IVpnConnection _vpnConnection;
    private readonly ISshTransport _sshTransport;

    public MainWindow(IVpnConnection vpnConnection, ISshTransport sshTransport)
    {
        InitializeComponent();

        _vpnConnection = vpnConnection;
        _sshTransport = sshTransport;
    }

    private async void ConnectionButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ConnectionButton.IsEnabled = false;

        try
        {
            if (_vpnConnection.IsConnected)
            {
                StatusText.Text = "Disconnecting...";

                await _vpnConnection.DisconnectAsync(
                    CancellationToken.None);

                StatusText.Text = "Disconnected";
                ConnectionButton.Content = "Connect";
            }
            else
            {
                StatusText.Text = "Connecting...";

                await _vpnConnection.ConnectAsync(
                    CancellationToken.None);

                StatusText.Text = "Connected";
                ConnectionButton.Content = "Disconnect";
                
                StatusText.Text = "Executing whoami...";
                
                var output = await _sshTransport.ExecuteCommandAsync("whoami", CancellationToken.None);

                MessageBox.Show(output);

                StatusText.Text = "Opening SSH data channel...";

                //Protocol Testing
                await using var channel = await _sshTransport.OpenDataChannelAsync("dotnet /home/vpnuser/sshvpn/SSHVPN.Server.dll 2>" +
                    "/home/vpnuser/sshvpn/server.log", CancellationToken.None);
                
                StatusText.Text = "Performing handshake...";
                
                var handshake = new VpnHandshake();

                await handshake.PerformAsync(channel, CancellationToken.None);
                
                StatusText.Text = "Handshake successful. Sending data...";

                var sentPacket = new Packet(PacketType.Data, Encoding.UTF8.GetBytes("Hello from SSHVPN.Client"));

                await PacketSerializer.WriteAsync(channel.Input, sentPacket);
                
                StatusText.Text = "Waiting for server response...";

                Packet receivedPacket = await PacketSerializer.ReadAsync(channel.Output);

                MessageBox.Show($"Handshake: successful\n" +
                                            $"Type: {receivedPacket.Type}\n" +
                                            $"Payload: {Encoding.UTF8.GetString(receivedPacket.Payload)}");
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error: {ex.Message}";
            ConnectionButton.Content = "Connect";
        }
        finally
        {
            ConnectionButton.IsEnabled = true;
        }
        
        
    }
}