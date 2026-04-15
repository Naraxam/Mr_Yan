using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Options;
using MRYAN;

// ── P/Invoke: hide console on Windows when showing GUI ────────────────────────
const int SW_HIDE = 0;
[DllImport("kernel32.dll")] static extern IntPtr GetConsoleWindow();
[DllImport("user32.dll")]   static extern bool   ShowWindow(IntPtr hWnd, int nCmdShow);

// ── Detect run mode ───────────────────────────────────────────────────────────
bool isService = WindowsServiceHelpers.IsWindowsService()
              || Environment.GetEnvironmentVariable("INVOCATION_ID") != null
              || args.Contains("--headless");

bool hasDisplay = OperatingSystem.IsWindows()
               || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY"))
               || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));

bool showGui = !isService && hasDisplay;

// ── Build web application ─────────────────────────────────────────────────────
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5000");

if (isService)
    builder.Host
        .UseWindowsService(o => o.ServiceName = "MrYAN")
        .UseSystemd();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("MrYAN"));
builder.Services.AddSingleton<LogStore>();
builder.Services.AddSingleton<AppState>(sp =>
    new AppState(sp.GetRequiredService<IOptions<AppSettings>>().Value));
builder.Services.AddSingleton<NwsService>();
builder.Services.AddSingleton<ChatService>();
builder.Services.AddSingleton<AlertMonitor>();
builder.Services.AddHostedService<MonitorWorker>();

var app = builder.Build();

var jsonOpts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

// ── HTTP routes (browser access in headless mode) ─────────────────────────────
app.MapGet("/", () => Results.Content(HtmlPage.Content, "text/html; charset=utf-8"));

app.MapGet("/api/events", async (LogStore log, HttpContext ctx, CancellationToken ct) =>
{
    ctx.Response.Headers["Content-Type"]      = "text/event-stream";
    ctx.Response.Headers["Cache-Control"]     = "no-cache";
    ctx.Response.Headers["X-Accel-Buffering"] = "no";

    var backlog = log.GetRecent(100);
    backlog.Reverse();
    foreach (var e in backlog)
    {
        await ctx.Response.WriteAsync(
            $"data: {JsonSerializer.Serialize(new { e.Time, e.Level, e.Message }, jsonOpts)}\n\n", ct);
    }
    await ctx.Response.Body.FlushAsync(ct);

    while (!ct.IsCancellationRequested)
    {
        try
        {
            var e = await log.WaitForNextAsync(ct);
            await ctx.Response.WriteAsync(
                $"data: {JsonSerializer.Serialize(new { e.Time, e.Level, e.Message }, jsonOpts)}\n\n", ct);
            await ctx.Response.Body.FlushAsync(ct);
        }
        catch { break; }
    }
});

app.MapGet("/api/state", (AppState s) => Results.Json(new
{
    s.Running, s.PollMinutes, s.RepostMinutes, s.PollsRun, s.PostedToday,
    s.Zone,
    Uptime     = s.UptimeString,
    LastPollAt = s.LastPollTime,
    NextPollAt = s.NextPollTime,
    WebhookOk  = !string.IsNullOrWhiteSpace(s.WebhookUrl),
    Alerts     = s.CurrentAlerts
}, jsonOpts));

app.MapPost("/api/toggle", (AlertMonitor m, AppState s) =>
{
    if (s.Running) m.Stop(); else m.Start();
    return Results.Json(new { s.Running }, jsonOpts);
});

app.MapPost("/api/poll", (AlertMonitor m, AppState s) =>
{
    if (!s.Running) return Results.Json(new { success = false }, jsonOpts);
    _ = Task.Run(() => m.ManualPollAsync());
    return Results.Json(new { success = true }, jsonOpts);
});

app.MapPost("/api/intervals", async (HttpContext ctx, AlertMonitor m, AppState s, LogStore log) =>
{
    using var doc = await JsonDocument.ParseAsync(ctx.Request.Body);
    s.SetIntervals(doc.RootElement.GetProperty("pollMinutes").GetInt32(),
                   doc.RootElement.GetProperty("repostMinutes").GetInt32());
    log.Info($"Intervals updated — poll: {s.PollMinutes} min | repost: {s.RepostMinutes} min");
    if (s.Running) { log.Info("Restarting monitor…"); m.Restart(); }
    return Results.Json(new { s.PollMinutes, s.RepostMinutes }, jsonOpts);
});

app.MapPost("/api/message", async (HttpContext ctx, ChatService chat, AppState s) =>
{
    if (!s.Running) return Results.Json(new { success = false }, jsonOpts);
    using var doc = await JsonDocument.ParseAsync(ctx.Request.Body);
    var text = doc.RootElement.GetProperty("text").GetString() ?? "";
    if (string.IsNullOrWhiteSpace(text)) return Results.Json(new { success = false }, jsonOpts);
    return Results.Json(new { success = await chat.PostCustomAsync(text) }, jsonOpts);
});

app.MapGet("/health", (AppState s) => Results.Json(new { status = "ok", s.Running }, jsonOpts));

// ── Run ───────────────────────────────────────────────────────────────────────
if (showGui && OperatingSystem.IsWindows())
{
    // Start web server (available at localhost:5000 even in GUI mode)
    await app.StartAsync();
    ShowWindow(GetConsoleWindow(), SW_HIDE);

    var logStore = app.Services.GetRequiredService<LogStore>();
    var appState = app.Services.GetRequiredService<AppState>();
    var monitor  = app.Services.GetRequiredService<AlertMonitor>();
    var chat     = app.Services.GetRequiredService<ChatService>();

#if WINDOWS
    // WinForms must run on a dedicated STA thread
    var winThread = new Thread(() =>
        YanWindow.Run(logStore, appState, monitor, chat, jsonOpts));
    winThread.SetApartmentState(ApartmentState.STA);
    winThread.Start();
    winThread.Join();
#endif

    await app.StopAsync();
}
else
{
    // Headless: service mode or Linux without display
    await app.RunAsync();
}
