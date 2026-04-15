using System.Net.Http.Json;
using System.Text;

namespace MRYAN;

public sealed class ChatService
{
    private readonly LogStore _log;
    private readonly AppState _state;

    private static readonly HttpClient _http = new(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    })
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    public ChatService(LogStore log, AppState state) { _log = log; _state = state; }

    public async Task<bool> PostAlertAsync(NwsFeature alert)
    {
        var text = BuildAlertMessage(alert);
        return await PostRawAsync(text);
    }

    public async Task<bool> PostAllClearAsync(int previousCount)
    {
        var text = previousCount == 1
            ? "✅ *Mr. YAN — All Clear*\nThe active weather alert for Monroe County, IN has expired."
            : $"✅ *Mr. YAN — All Clear*\nAll {previousCount} weather alerts for Monroe County, IN have expired.";
        return await PostRawAsync(text);
    }

    public async Task<bool> PostStartupAsync()
    {
        return await PostRawAsync(
            "🌤️ *Mr. YAN is online*\n_Meteorological Responses Your Accurate Notification_\nMonitoring NWS alerts for Monroe County, IN.");
    }

    public async Task<bool> PostCustomAsync(string text)
    {
        _log.Custom($"Sending custom message: \"{(text.Length > 60 ? text[..60] + "…" : text)}\"");
        var ok = await PostRawAsync(text);
        if (ok) _state.RecordPosted();
        return ok;
    }

    private string BuildAlertMessage(NwsFeature alert)
    {
        var p  = alert.Properties;
        var sb = new StringBuilder();
        sb.AppendLine($"{p.SeverityIcon} *Mr. YAN ALERT — {p.Event.ToUpperInvariant()}*");
        sb.AppendLine($"_{p.SenderName}_");
        sb.AppendLine();
        sb.AppendLine($"*Area:* {p.AreaDesc}");
        sb.AppendLine($"*Severity:* {p.Severity}   *Certainty:* {p.Certainty}   *Urgency:* {p.Urgency}");
        sb.AppendLine($"*Onset:* {p.OnsetLocal}");
        sb.AppendLine($"*Expires:* {p.ExpiresLocal}");
        if (!string.IsNullOrWhiteSpace(p.Headline)) { sb.AppendLine(); sb.AppendLine($"*{p.Headline}*"); }
        if (_state.IncludeDesc && !string.IsNullOrWhiteSpace(p.Description))
        {
            sb.AppendLine();
            var desc = p.Description.Trim();
            if (desc.Length > _state.MaxDescChars) desc = desc[.._state.MaxDescChars].TrimEnd() + "… _(truncated)_";
            sb.AppendLine(desc);
        }
        if (_state.IncludeInstr && !string.IsNullOrWhiteSpace(p.Instruction))
        {
            sb.AppendLine(); sb.AppendLine("*Instructions:*"); sb.AppendLine(p.Instruction.Trim());
        }
        sb.AppendLine(); sb.Append($"_Issued: {p.SentLocal}_");
        return sb.ToString();
    }

    private async Task<bool> PostRawAsync(string text)
    {
        var url = _state.WebhookUrl;
        if (string.IsNullOrWhiteSpace(url)) { _log.Error("WebhookUrl not configured"); return false; }
        try
        {
            var response = await _http.PostAsJsonAsync(url, new { text });
            if (response.IsSuccessStatusCode) { _log.Success("Message posted to Google Chat successfully"); return true; }
            var body = await response.Content.ReadAsStringAsync();
            _log.Error($"Webhook returned {(int)response.StatusCode}: {body}");
            return false;
        }
        catch (HttpRequestException ex) { _log.Error($"Webhook HTTP error: {ex.Message}"); return false; }
        catch (TaskCanceledException)   { _log.Warn("Webhook POST timed out");             return false; }
        catch (Exception ex)            { _log.Error($"Webhook unexpected error: {ex.Message}"); return false; }
    }
}
