using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SSHVPN.Client.Core.Connection.Abstractions;

namespace SSHVPN.Client;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IVpnConnection _vpnConnection;
    
    public MainWindow(IVpnConnection vpnConnection)
    {
        InitializeComponent();

        _vpnConnection = vpnConnection;
    }
}