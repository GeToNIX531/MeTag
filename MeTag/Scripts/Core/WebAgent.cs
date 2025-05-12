using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public class WebAgent : IDisposable
{
    private readonly HttpClient _httpClient;

    public WebAgent()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30),
            DefaultRequestHeaders =
            {
                UserAgent = { ProductInfoHeaderValue.Parse("SeoAnalyzer/1.0") }
            }
        };
    }

    public async Task<string> LoadPageHtml(string url)
    {
        ValidateUrl(ref url);
        return await FetchHtml(url);
    }

    public void ValidateUrl(ref string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty");

        if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            url = "https://" + url;

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            throw new UriFormatException("Invalid URL format");
    }

    private async Task<string> FetchHtml(string url)
    {
        using (var response = await _httpClient.GetAsync(url))
        {
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}