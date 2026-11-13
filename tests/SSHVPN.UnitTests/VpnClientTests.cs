using Microsoft.Extensions.DependencyInjection;
using SSHVPN.Client.Core;
using SSHVPN.Client.Core.Connection.Abstractions;

namespace SSHVPN.UnitTests;

public class VpnClientTests
{
    [Fact]
    public async Task ConnectAsync_ShouldCallConnection()
    {
        //Arrange
        var connection = new TestVpnConnection();
        var client = new VpnClient(connection);
        
        //Act
        await client.ConnectAsync(CancellationToken.None);
        
        //Assert
        Assert.True(client.IsConnected);
    }

    [Fact]
    public async Task DisconnectAsync_ShouldCallConnection()
    {
        var connection = new TestVpnConnection();
        var client = new VpnClient(connection);

        await client.DisconnectAsync(CancellationToken.None);
        
        Assert.False(client.IsConnected);
    }

    [Fact]
    public void DependencyInjection_ShouldResolveVpnClient()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IVpnConnection, TestVpnConnection>();
        services.AddTransient<VpnClient>();

        using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<VpnClient>();

        Assert.NotNull(client);
    }
}
