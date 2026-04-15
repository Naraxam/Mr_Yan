using System.Text.Json.Serialization;

namespace MRYAN;

// ── NWS API models ────────────────────────────────────────────────────────────

public class NwsAlertCollection
{
    [JsonPropertyName("features")]
    public List<NwsFeature> Features { get; set; } = new();
}

public class NwsFeature
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("properties")]
    public NwsAlertProps Properties { get; set; } = new();
}

public class NwsAlertProps
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("areaDesc")]
    public string AreaDesc { get; set; } = "";

    [JsonPropertyName("sent")]
    public DateTimeOffset? Sent { get; set; }

    [JsonPropertyName("effective")]
    public DateTimeOffset? Effective { get; set; }

    [JsonPropertyName("onset")]
    public DateTimeOffset? Onset { get; set; }

    [JsonPropertyName("expires")]
    public DateTimeOffset? Expires { get; set; }

    [JsonPropertyName("ends")]
    public DateTimeOffset? Ends { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("severity")]
    public string Severity { get; set; } = "";

    [JsonPropertyName("certainty")]
    public string Certainty { get; set; } = "";

    [JsonPropertyName("urgency")]
    public string Urgency { get; set; } = "";

    [JsonPropertyName("event")]
    public string Event { get; set; } = "";

    [JsonPropertyName("senderName")]
    public string SenderName { get; set; } = "";

    [JsonPropertyName("headline")]
    public string? Headline { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("instruction")]
    public string? Instruction { get; set; }

    [JsonPropertyName("messageType")]
    public string MessageType { get; set; } = "";

    public string SeverityIcon => Event.ToLowerInvariant() switch
    {
        var e when e.Contains("tornado")                           => "🌪️",
        var e when e.Contains("severe thunderstorm")               => "⛈️",
        var e when e.Contains("flash flood")                       => "🌊",
        var e when e.Contains("flood")                             => "💧",
        var e when e.Contains("blizzard")                          => "❄️",
        var e when e.Contains("winter storm")                      => "🌨️",
        var e when e.Contains("winter weather")                    => "🌨️",
        var e when e.Contains("ice storm")                         => "🧊",
        var e when e.Contains("snow")                              => "🌨️",
        var e when e.Contains("wind chill")                        => "🥶",
        var e when e.Contains("freeze") || e.Contains("frost")     => "🥶",
        var e when e.Contains("high wind")                         => "💨",
        var e when e.Contains("wind")                              => "💨",
        var e when e.Contains("heat")                              => "🌡️",
        var e when e.Contains("fog")                               => "🌫️",
        var e when e.Contains("fire")                              => "🔥",
        var e when e.Contains("hurricane")                         => "🌀",
        _ => Severity switch
        {
            "Extreme"  => "🚨",
            "Severe"   => "⚠️",
            "Moderate" => "🔔",
            _          => "ℹ️"
        }
    };

    public string ExpiresLocal => (Ends ?? Expires)?.ToLocalTime().ToString("ddd MM/dd h:mm tt") ?? "Unknown";
    public string OnsetLocal   => Onset?.ToLocalTime().ToString("ddd MM/dd h:mm tt") ?? "Unknown";
    public string SentLocal    => Sent?.ToLocalTime().ToString("ddd MM/dd h:mm tt") ?? "Unknown";
}

// ── App settings ──────────────────────────────────────────────────────────────

public class AppSettings
{
    public string WebhookUrl            { get; set; } = "";
    public string Zone                  { get; set; } = "INZ027";
    public int    CheckIntervalMinutes  { get; set; } = 5;
    public int    RepostIntervalMinutes { get; set; } = 60;
    public bool   PostAllClear          { get; set; } = true;
    public bool   IncludeDescription    { get; set; } = true;
    public bool   IncludeInstructions   { get; set; } = true;
    public int    MaxDescriptionChars   { get; set; } = 800;
}

// ── Log ───────────────────────────────────────────────────────────────────────

public enum YanLogLevel { Info, Success, Warning, Error, Custom }

public record LogEntry(string Message, YanLogLevel Level, DateTime Timestamp)
{
    public string Time => Timestamp.ToString("HH:mm:ss");
}
