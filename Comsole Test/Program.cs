using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

class Program
{
    // Обновленный метод Main
    static async Task Main(string[] args)
    {
        Console.WriteLine("Введите URL сайта для анализа:");
        string url = Console.ReadLine();

        var similarWeb = new SimilarWebClient();
        var domain = new Uri(url).Host;

        try
        {
            // Загрузка текущих данных
            string html = await FetchHtml(url);
            List<MetaTag> metaTags = ParseMetaTags(html);
            var traffic = await similarWeb.GetTrafficStats(domain);

            // Анализ SEO
            var analyzer = new SeoImpactAnalyzer();
            var scores = analyzer.CalculateSeoImpact(metaTags, html);

            // Создание снимка
            var snapshot = new SeoSnapshot
            {
                Date = DateTime.Now,
                Url = url,
                MetaTags = metaTags,
                Traffic = traffic,
                Scores = scores
            };

            HistoryManager.SaveSnapshot(snapshot);

            // Показать историю
            ShowHistoryAnalysis(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    static void ShowHistoryAnalysis(string url)
    {
        var history = HistoryManager.LoadHistory()
            .Where(s => s.Url == url)
            .OrderBy(s => s.Date)
            .ToList();

        if (history.Count < 2)
        {
            Console.WriteLine("Недостаточно данных для анализа истории");
            return;
        }

        Console.WriteLine("\n📊 Исторический анализ изменений:");

        var first = history.First();
        var last = history.Last();

        // Анализ изменений в трафике
        Console.WriteLine($"\nПосетителей: {first.Traffic.MonthlyVisitors} → {last.Traffic.MonthlyVisitors} " +
                          $"({GetChangeSymbol(last.Traffic.MonthlyVisitors - first.Traffic.MonthlyVisitors)})");

        // Анализ изменений в мета-тегах
        var changedTags = CompareMetaTags(first.MetaTags, last.MetaTags);
        Console.WriteLine("\nИзмененные мета-теги:");
        foreach (var change in changedTags)
        {
            Console.WriteLine($"{change.TagName}:");
            Console.WriteLine($"Было: {change.OldValue}");
            Console.WriteLine($"Стало: {change.NewValue}\n");
        }

        // Корреляция изменений тегов и трафика
        AnalyzeTagImpactCorrelation(history);
    }

    static List<TagChange> CompareMetaTags(List<MetaTag> oldTags, List<MetaTag> newTags)
    {
        var changes = new List<TagChange>();

        foreach (var newTag in newTags)
        {
            var oldTag = oldTags.FirstOrDefault(t => t.Name == newTag.Name);
            if (oldTag == null)
            {
                changes.Add(new TagChange
                {
                    TagName = newTag.Name,
                    OldValue = "Не существовал",
                    NewValue = newTag.Content
                });
            }
            else if (oldTag.Content != newTag.Content)
            {
                changes.Add(new TagChange
                {
                    TagName = newTag.Name,
                    OldValue = oldTag.Content,
                    NewValue = newTag.Content
                });
            }
        }

        return changes;
    }

    static async Task<string> FetchHtml(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MetaTagAnalyzer/1.0)");

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }

    static List<MetaTag> ParseMetaTags(string html)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);

        var metaTags = new List<MetaTag>();
        var nodes = doc.DocumentNode.SelectNodes("//meta");

        if (nodes == null) return metaTags;

        foreach (var node in nodes)
        {
            string name = node.GetAttributeValue("name", null)
                        ?? node.GetAttributeValue("property", null)
                        ?? node.GetAttributeValue("itemprop", null);

            string content = node.GetAttributeValue("content", null);

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(content))
            {
                metaTags.Add(new MetaTag(name.ToLower(), content));
            }
        }

        return metaTags;
    }

    static void AnalyzeSeo(List<MetaTag> metaTags)
    {
        Console.WriteLine("\nSEO Анализ:");

        var title = metaTags.Find(t => t.Name == "title")?.Content;
        var description = metaTags.Find(t => t.Name == "description")?.Content;
        var keywords = metaTags.Find(t => t.Name == "keywords")?.Content;

        // Анализ title
        if (string.IsNullOrEmpty(title))
        {
            Console.WriteLine("❌ Отсутствует тег title");
        }
        else
        {
            Console.WriteLine(title.Length <= 60
                ? $"✅ Title хорошей длины ({title.Length} символов)"
                : $"⚠️ Title слишком длинный ({title.Length}/60)");
        }

        // Анализ description
        if (string.IsNullOrEmpty(description))
        {
            Console.WriteLine("❌ Отсутствует meta description");
        }
        else
        {
            Console.WriteLine(description.Length <= 200
                ? $"✅ Description хорошей длины ({description.Length} символов)"
                : $"⚠️ Description слишком длинный ({description.Length}/200)");
        }

        // Анализ keywords
        Console.WriteLine(string.IsNullOrEmpty(keywords)
            ? "ℹ️ Keywords не указаны (необязательный тег)"
            : "✅ Keywords присутствуют");
    }
}

class MetaTag
{
    public string Name { get; }
    public string Content { get; }

    public MetaTag(string name, string content)
    {
        Name = name;
        Content = content;
    }
}