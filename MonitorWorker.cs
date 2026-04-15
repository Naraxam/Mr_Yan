using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MRYAN;

public sealed class MonitorWorker : BackgroundService
{
    private readonly ILogger<MonitorWorker> _logger;
    private readonly AlertMonitor           _monitor;
    private readonly ChatService            _chat;
    private readonly AppState               _state;

    public MonitorWorker(ILogger<MonitorWorker> logger, AlertMonitor monitor, ChatService chat, AppState state)
    {
        _logger  = logger;
        _monitor = monitor;
        _chat    = chat;
        _state   = state;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_state.WebhookUrl))
        {
            _logger.LogCritical("WebhookUrl is not configured in appsettings.json — cannot start.");
            return;
        }

        _logger.LogInformation("Mr. YAN starting — GUI available at http://localhost:5000");

        await _chat.PostStartupAsync();
        _monitor.Start();

        try { await Task.Delay(Timeout.Infinite, stoppingToken); }
        catch (OperationCanceledException) { }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _monitor.Stop();
        return base.StopAsync(cancellationToken);
    }
}
