using Nimbus;

namespace Profile;

public class NimbusBusService(Bus bus) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => bus.Start();
    public Task StopAsync(CancellationToken cancellationToken) => bus.Stop();
}