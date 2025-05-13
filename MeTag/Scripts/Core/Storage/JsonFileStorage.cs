using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class JsonFileStorage : IStorage
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options;

    public JsonFileStorage(string filePath = "seo_history.json")
    {
        _filePath = filePath;
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    public async Task SaveAsync<T>(IEnumerable<T> items)
    {
        var json = JsonSerializer.Serialize(items, _options);

        // Асинхронная запись через StreamWriter
        using (var writer = new StreamWriter(_filePath, false, Encoding.UTF8))
        {
            await writer.WriteAsync(json);
        }
    }

    public async Task<List<T>> LoadAsync<T>()
    {
        if (!File.Exists(_filePath)) return new List<T>();

        // Асинхронное чтение через StreamReader
        using (var reader = new StreamReader(_filePath, Encoding.UTF8))
        {
            var json = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }
    }

    public Task ClearAsync()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
        return Task.CompletedTask;
    }
}