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

    public MainWindow(IVpnConnection vpnConnection, ISshTransport sshTransport)
    {
        InitializeComponent();

        _vpnConnection = vpnConnection;
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

                await _vpnConnection.DisconnectAsync(CancellationToken.None);

                StatusText.Text = "Disconnected";
                ConnectionButton.Content = "Connect";
            }
            else
            {
                StatusText.Text = "Connecting...";

                await _vpnConnection.ConnectAsync(CancellationToken.None);

                StatusText.Text = "Connected";
                ConnectionButton.Content = "Disconnect";
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