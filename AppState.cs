namespace MRYAN;

/// <summary>
/// Singleton that holds mutable runtime state shared between the monitor
/// background service and the HTTP API endpoints.
/// </summary>
public sealed class AppState
{
    private readonly object _lock = new();

    // ── Monitor state ─────────────────────────────────────────────────────────
    private bool _running;
    private DateTime _startedAt;
    private DateTime? _lastPollAt;
    private List<NwsFeature> _currentAlerts = new();
    private int _pollsRun;
    private int _postedToday;
    private readonly DateTime _dayStart = DateTime.Today;

    // ── Config (can be changed at runtime via GUI) ────────────────────────────
    private int _pollIntervalMinutes;
    private int _repostIntervalMinutes;
    private readonly AppSettings _baseSettings;

    public AppState(AppSettings baseSettings)
    {
        _baseSettings          = baseSettings;
        _pollIntervalMinutes   = baseSettings.CheckIntervalMinutes;
        _repostIntervalMinutes = baseSettings.RepostIntervalMinutes;
    }

    // ── Thread-safe accessors ─────────────────────────────────────────────────

    public bool Running          { get { lock (_lock) return _running; } }
    public int  PollMinutes      { get { lock (_lock) return _pollIntervalMinutes; } }
    public int  RepostMinutes    { get { lock (_lock) return _repostIntervalMinutes; } }
    public int  PollsRun         { get { lock (_lock) return _pollsRun; } }
    public int  PostedToday      { get { lock (_lock) return _postedToday; } }
    public bool PostAllClear     => _baseSettings.PostAllClear;
    public bool IncludeDesc      => _baseSettings.IncludeDescription;
    public bool IncludeInstr     => _baseSettings.IncludeInstructions;
    public int  MaxDescChars     => _baseSettings.MaxDescriptionChars;
    public string WebhookUrl     => _baseSettings.WebhookUrl;

    public List<NwsFeature> CurrentAlerts
    {
        get { lock (_lock) return new List<NwsFeature>(_currentAlerts); }
    }

    public string UptimeString
    {
        get
        {
            lock (_lock)
            {
                if (!_running) return "00:00:00";
                var s = (int)(DateTime.Now - _startedAt).TotalSeconds;
                return $"{s / 3600:D2}:{(s % 3600) / 60:D2}:{s % 60:D2}";
            }
        }
    }

    public string? LastPollTime
    {
        get { lock (_lock) return _lastPollAt?.ToString("HH:mm:ss"); }
    }

    public string? NextPollTime
    {
        get
        {
            lock (_lock)
            {
                if (_lastPollAt == null || !_running) return null;
                return _lastPollAt.Value.AddMinutes(_pollIntervalMinutes).ToString("HH:mm:ss");
            }
        }
    }

    // ── Mutators ──────────────────────────────────────────────────────────────

    public void SetRunning(bool v)
    {
        lock (_lock)
        {
            _running = v;
            if (v) _startedAt = DateTime.Now;
        }
    }

    public void SetIntervals(int pollMinutes, int repostMinutes)
    {
        lock (_lock)
        {
            _pollIntervalMinutes   = Math.Max(1, pollMinutes);
            _repostIntervalMinutes = Math.Max(5, repostMinutes);
        }
    }

    public void RecordPoll()
    {
        lock (_lock)
        {
            _pollsRun++;
            _lastPollAt = DateTime.Now;
            // Reset daily counter at midnight
            if (DateTime.Today > _dayStart) _postedToday = 0;
        }
    }

    public void RecordPosted()
    {
        lock (_lock) _postedToday++;
    }

    public void SetAlerts(List<NwsFeature> alerts)
    {
        lock (_lock) _currentAlerts = alerts;
    }
}
