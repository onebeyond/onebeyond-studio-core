namespace OneBeyond.Studio.Hosting.BackgroundServices;

public interface IBackgroundService
{
    Task ExecuteAsync(CancellationToken stoppingToken);
}
