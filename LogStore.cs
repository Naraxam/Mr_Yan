namespace MRYAN;

/// <summary>
/// Thread-safe in-memory log store.
/// Background services call Add(); the SSE endpoint awaits WaitForNextAsync()
/// so each connected browser tab gets every new entry in real time.
/// </summary>
public sealed class LogStore
{
    private readonly List<LogEntry> _entries = new();
    private readonly List<TaskCompletionSource<LogEntry>> _waiters = new();
    private readonly object _lock = new();

    public event Action<LogEntry>? EntryAdded;

    public void Add(YanLogLevel level, string msg)
    {
        var entry = new LogEntry(msg, level, DateTime.Now);
        List<TaskCompletionSource<LogEntry>> toNotify;

        lock (_lock)
        {
            _entries.Insert(0, entry);
            if (_entries.Count > 2000) _entries.RemoveAt(_entries.Count - 1);
            toNotify = new List<TaskCompletionSource<LogEntry>>(_waiters);
            _waiters.Clear();
        }

        EntryAdded?.Invoke(entry);

        foreach (var w in toNotify)
            w.TrySetResult(entry);
    }

    public void Info(string msg)    => Add(YanLogLevel.Info,    msg);
    public void Success(string msg) => Add(YanLogLevel.Success, msg);
    public void Warn(string msg)    => Add(YanLogLevel.Warning, msg);
    public void Error(string msg)   => Add(YanLogLevel.Error,   msg);
    public void Custom(string msg)  => Add(YanLogLevel.Custom,  msg);

    public List<LogEntry> GetRecent(int n = 200)
    {
        lock (_lock) return _entries.Take(n).ToList();
    }

    /// <summary>
    /// Awaits the next log entry. Used by the SSE endpoint to push entries
    /// to the browser as they arrive.
    /// </summary>
    public Task<LogEntry> WaitForNextAsync(CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<LogEntry>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        ct.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
        lock (_lock) _waiters.Add(tcs);
        return tcs.Task;
    }
}
