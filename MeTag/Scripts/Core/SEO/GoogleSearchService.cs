using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class GoogleSearchService
{
    private const string ApiKey = "AIzaSyDDmdBVuKm8qyTbibP6DIaK-664dMerqPw";
    private const string SearchEngineId = "60f3973d76bb74895";
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task<int> GetSearchPosition(string query, string siteUrl)
    {
        var requestUrl = $"https://www.googleapis.com/customsearch/v1?key={ApiKey}&cx={SearchEngineId}&q={Uri.EscapeDataString(query)}&num=10";
        var response = await _httpClient.GetStringAsync(requestUrl);
        var json = JsonDocument.Parse(response);

        var items = json.RootElement.GetProperty("items").EnumerateArray();
        int position = 1;

        foreach (var item in items)
        {
            if (item.GetProperty("link").ToString().Contains(siteUrl))
            {
                return position;
            }
            position++;
        }

        return -1; // Не найдено в топ-100
    }
}