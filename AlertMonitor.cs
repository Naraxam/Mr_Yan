namespace MRYAN;

/// <summary>
/// Core polling loop. Restartable so interval changes take effect immediately.
/// All state mutations go through AppState; all log output goes through LogStore.
/// </summary>
public sealed class AlertMonitor
{
    private readonly LogStore    _log;
    private readonly AppState    _state;
    private readonly NwsService  _nws;
    private readonly ChatService _chat;

    private CancellationTokenSource? _cts;
    private Task?                    _loopTask;
    private readonly object          _controlLock = new();

    private readonly Dictionary<string, DateTime> _postedAlerts = new();
    private int _lastAlertCount;

    public AlertMonitor(LogStore log, AppState state, NwsService nws, ChatService chat)
    {
        _log   = log;
        _state = state;
        _nws   = nws;
        _chat  = chat;
    }

    // ── Control ───────────────────────────────────────────────────────────────

    public void Start()
    {
        lock (_controlLock)
        {
            if (_state.Running) return;
            _cts = new CancellationTokenSource();
            _state.SetRunning(true);
            _loopTask = Task.Run(() => LoopAsync(_cts.Token));
            _log.Info($"Monitor started — polling every {_state.PollMinutes} min | repost every {_state.RepostMinutes} min");
        }
    }

    public void Stop()
    {
        lock (_controlLock)
        {
            if (!_state.Running) return;
            _cts?.Cancel();
            _state.SetRunning(false);
            _log.Info("Mr. YAN monitor stopped");
        }
    }

    public void Restart()
    {
        Stop();
        Thread.Sleep(200);
        Start();
    }

    /// <summary>Trigger a single poll outside the normal schedule.</summary>
    public Task ManualPollAsync() => PollAsync();

    // ── Loop ──────────────────────────────────────────────────────────────────

    private async Task LoopAsync(CancellationToken ct)
    {
        // Poll immediately on start, then on interval
        await PollAsync();

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(_state.PollMinutes), ct);
                if (!ct.IsCancellationRequested) await PollAsync();
            }
            catch (OperationCanceledException) { break; }
        }
    }

    // ── Poll ──────────────────────────────────────────────────────────────────

    private async Task PollAsync()
    {
        _log.Info($"Polling NWS alerts for zone {_state.Zone}…");
        _state.RecordPoll();

        var alerts = await _nws.GetActiveAlertsAsync();
        _state.SetAlerts(alerts);

        var now          = DateTime.UtcNow;
        var repostWindow = TimeSpan.FromMinutes(_state.RepostMinutes);
        var currentIds   = alerts.Select(a => a.Properties.Id).ToHashSet();

        // Expire cleared alerts
        foreach (var id in _postedAlerts.Keys.Where(id => !currentIds.Contains(id)).ToList())
            _postedAlerts.Remove(id);

        // All-clear
        if (_lastAlertCount > 0 && alerts.Count == 0 && _state.PostAllClear)
        {
            _log.Info($"All alerts cleared — posting all-clear");
            await _chat.PostAllClearAsync(_lastAlertCount);
        }

        _lastAlertCount = alerts.Count;

        if (alerts.Count == 0)
        {
            _log.Info($"No active alerts for zone {_state.Zone}");
            return;
        }

        _log.Info($"{alerts.Count} active alert(s): {string.Join(", ", alerts.Select(a => a.Properties.Event))}");

        foreach (var alert in alerts)
        {
            var id     = alert.Properties.Id;
            var isNew  = !_postedAlerts.ContainsKey(id);
            var isStale = !isNew && (now - _postedAlerts[id]) >= repostWindow;

            if (isNew || isStale)
            {
                var reason = isNew ? "new alert" : $"repost after {_state.RepostMinutes} min";
                _log.Info($"Posting [{reason}]: {alert.Properties.Event}");
                var ok = await _chat.PostAlertAsync(alert);
                if (ok) { _postedAlerts[id] = now; _state.RecordPosted(); }
            }
            else
            {
                var mins = (int)(now - _postedAlerts[id]).TotalMinutes;
                _log.Info($"Skipping already-posted alert: {alert.Properties.Event} (posted {mins} min ago)");
            }
        }
    }
}
