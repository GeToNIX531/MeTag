using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class PageSpeedAnalyzer
{
    private const string ApiUrl = "https://www.googleapis.com/pagespeedonline/v5/runPagespeed";
    private readonly string _apiKey;

    public PageSpeedAnalyzer(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<PageSpeedResult> Analyze(string url)
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetStringAsync($"{ApiUrl}?url={Uri.EscapeDataString(url)}&key={_apiKey}");

            var json = JsonDocument.Parse(response);
            return new PageSpeedResult
            {
                PerformanceScore = json.RootElement
                    .GetProperty("lighthouseResult")
                    .GetProperty("categories")
                    .GetProperty("performance")
                    .GetProperty("score")
                    .GetDouble() * 100,
                FirstContentfulPaint = json.RootElement
                    .GetProperty("lighthouseResult")
                    .GetProperty("audits")
                    .GetProperty("first-contentful-paint")
                    .GetProperty("numericValue")
                    .GetDouble()
            };
        }
    }
}

public class PageSpeedResult
{
    public double PerformanceScore { get; set; }
    public double FirstContentfulPaint { get; set; }
}