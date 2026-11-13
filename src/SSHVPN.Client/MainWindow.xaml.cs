using System.Windows;
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
                
                var output = await _sshTransport.ExecuteCommandAsync("whoami", CancellationToken.None);

                MessageBox.Show(output);
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