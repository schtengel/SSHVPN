using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SSHVPN.Client.Core.Connection;
using SSHVPN.Client.Core.Connection.Abstractions;
using SSHVPN.Client.Infrastructure.Ssh;

namespace SSHVPN.Client;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(
                "appsettings.json",
                optional: false,
                reloadOnChange: false)
            .Build();

        var sshOptions = configuration
                             .GetSection("Ssh")
                             .Get<SshConnectionOptions>()
                         ?? throw new InvalidOperationException("SSH configuration is missing.");

        var services = new ServiceCollection();

        services.AddSingleton(sshOptions);

        services.AddSingleton<ISshTransport, SshTransport>();
        services.AddSingleton<IVpnConnection, VpnConnection>();
        services.AddTransient<MainWindow>();

        var provider = services.BuildServiceProvider();
        _serviceProvider = provider;
        
        var mainWindow = provider.GetRequiredService<MainWindow>();
        
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        
        base.OnExit(e);
    }
}

