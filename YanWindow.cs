#if WINDOWS
using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace MRYAN;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public static class YanWindow
{
    public static void Run(
        LogStore    logStore,
        AppState    appState,
        AlertMonitor monitor,
        ChatService chat,
        JsonSerializerOptions jsonOpts)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var form = new Form
        {
            Text          = "Mr. YAN – Meteorological Responses Your Accurate Notification",
            Size          = new Size(1280, 800),
            MinimumSize   = new Size(960, 640),
            StartPosition = FormStartPosition.CenterScreen,
            BackColor     = Color.FromArgb(14, 17, 23)
        };

        var webView = new WebView2 { Dock = DockStyle.Fill };
        form.Controls.Add(webView);

        // ── Helpers ───────────────────────────────────────────────────────────

        object Snapshot() => new
        {
            type       = "state",
            appState.Running,
            appState.PollMinutes,
            appState.RepostMinutes,
            appState.PollsRun,
            appState.PostedToday,
            Uptime     = appState.UptimeString,
            LastPollAt = appState.LastPollTime,
            NextPollAt = appState.NextPollTime,
            WebhookOk  = !string.IsNullOrWhiteSpace(appState.WebhookUrl),
            Alerts     = appState.CurrentAlerts
        };

        void Push(object payload)
        {
            if (form.IsDisposed) return;
            try
            {
                form.BeginInvoke(() =>
                {
                    try { webView.CoreWebView2?.PostWebMessageAsString(
                            JsonSerializer.Serialize(payload, jsonOpts)); }
                    catch { }
                });
            }
            catch { }
        }

        // ── Wire everything up once WebView2 is ready ─────────────────────────

        form.Load += async (_, _) =>
        {
            await webView.EnsureCoreWebView2Async();

            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled    = false;
            webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
            webView.CoreWebView2.Settings.IsStatusBarEnabled               = false;

            // ── Receive actions from JavaScript ───────────────────────────────
            webView.CoreWebView2.WebMessageReceived += (_, args) =>
            {
                try
                {
                    using var doc    = JsonDocument.Parse(args.WebMessageAsJson);
                    var       action = doc.RootElement.GetProperty("action").GetString();

                    switch (action)
                    {
                        case "ready":
                            Push(Snapshot());
                            var backlog = logStore.GetRecent(100);
                            backlog.Reverse();
                            foreach (var e in backlog)
                                Push(new { type = "log", e.Time, Level = e.Level.ToString(), e.Message });
                            break;

                        case "toggle":
                            if (appState.Running) monitor.Stop(); else monitor.Start();
                            Push(Snapshot());
                            break;

                        case "poll":
                            if (appState.Running)
                                _ = Task.Run(() => monitor.ManualPollAsync());
                            break;

                        case "intervals":
                            var p = doc.RootElement.GetProperty("pollMinutes").GetInt32();
                            var r = doc.RootElement.GetProperty("repostMinutes").GetInt32();
                            appState.SetIntervals(p, r);
                            logStore.Info($"Intervals updated — poll: {appState.PollMinutes} min | repost: {appState.RepostMinutes} min");
                            if (appState.Running) monitor.Restart();
                            Push(new { type = "toast", message = $"Applied: poll {appState.PollMinutes} min · repost {appState.RepostMinutes} min" });
                            Push(Snapshot());
                            break;

                        case "message":
                            var text = doc.RootElement.GetProperty("text").GetString() ?? "";
                            _ = Task.Run(async () =>
                            {
                                var ok = await chat.PostCustomAsync(text);
                                Push(new { type = "toast", message = ok ? "Posted to Google Chat ✓" : "Post failed — check log" });
                            });
                            break;
                    }
                }
                catch { }
            };

            // ── Push new log entries to the window as they arrive ─────────────
            logStore.EntryAdded += entry => Push(new
            {
                type    = "log",
                entry.Time,
                Level   = entry.Level.ToString(),
                entry.Message
            });

            // ── Push state snapshots every 2 seconds ──────────────────────────
            var stateTimer = new System.Timers.Timer(2000);
            stateTimer.Elapsed += (_, _) => Push(Snapshot());
            stateTimer.Start();
            form.FormClosed += (_, _) => stateTimer.Dispose();

            // ── Load the UI ───────────────────────────────────────────────────
            webView.NavigateToString(HtmlPage.Content);
        };

        Application.Run(form);
    }
}
#endif
