using System.Net.Http.Json;
using System.Text.Json;

namespace MRYAN;

public sealed class NwsService
{
    private readonly LogStore _log;
    private readonly AppState _state;

    private static readonly HttpClient _http = new(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    })
    {
        BaseAddress = new Uri("https://api.weather.gov/"),
        DefaultRequestHeaders =
        {
            { "User-Agent", "(MrYAN weather bot)" },
            { "Accept",     "application/geo+json" }
        },
        Timeout = TimeSpan.FromSeconds(30)
    };

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public NwsService(LogStore log, AppState state) { _log = log; _state = state; }

    public async Task<List<NwsFeature>> GetActiveAlertsAsync()
    {
        try
        {
            var response = await _http.GetAsync($"alerts/active?zone={_state.Zone}");
            response.EnsureSuccessStatusCode();

            var collection = await response.Content
                .ReadFromJsonAsync<NwsAlertCollection>(_jsonOpts);

            if (collection is null) { _log.Warn("NWS returned empty response"); return new(); }

            return collection.Features
                .Where(f => f.Properties.Status.Equals("Actual", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        catch (HttpRequestException ex) { _log.Error($"NWS HTTP error: {ex.Message}"); return new(); }
        catch (TaskCanceledException)   { _log.Warn("NWS request timed out");           return new(); }
        catch (Exception ex)            { _log.Error($"NWS unexpected error: {ex.Message}"); return new(); }
    }
}
