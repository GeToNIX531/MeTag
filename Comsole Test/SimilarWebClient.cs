using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class SimilarWebClient
{
    readonly HttpClient _client = new HttpClient();
    const string ApiKey = "YOUR_API_KEY";

    public async Task<TrafficStats> GetTrafficStats(string domain)
    {
        var response = await _client.GetStringAsync(
            $"https://api.similarweb.com/v1/sites/{domain}/traffic?api_key={ApiKey}");

        var doc = JsonDocument.Parse(response);
        return new TrafficStats
        {
            MonthlyVisitors = doc.RootElement.GetProperty("visits").GetInt32(),
            BounceRate = doc.RootElement.GetProperty("bounce_rate").GetDouble(),
            AvgVisitDuration = doc.RootElement.GetProperty("visit_duration").GetDouble()
        };
    }
}