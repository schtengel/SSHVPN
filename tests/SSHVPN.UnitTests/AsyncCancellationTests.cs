namespace SSHVPN.UnitTests;

public class AsyncCancellationTests
{
    [Fact]
    public async Task CancelOperation_ShouldOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();

        Task operation = Task.Delay(
            TimeSpan.FromSeconds(10),
            cts.Token);
        
        await cts.CancelAsync();
        
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await operation;
        });
    }
}