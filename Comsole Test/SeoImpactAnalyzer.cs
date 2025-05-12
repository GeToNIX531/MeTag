using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

class SeoImpactAnalyzer
{
    public void CalculateSeoImpact(List<MetaTag> metaTags, string htmlContent)
    {
        var title = metaTags.Find(t => t.Name == "title")?.Content;
        var description = metaTags.Find(t => t.Name == "description")?.Content;

        // Анализ CTR (Click-Through Rate)
        double ctrImpact = CalculateCtrImpact(title, description);
        Console.WriteLine($"\n🏆 Прогнозируемый CTR: {ctrImpact:P0}");

        // Анализ релевантности контента
        double contentRelevance = AnalyzeContentRelevance(htmlContent, title);
        Console.WriteLine($"🎯 Релевантность контента: {contentRelevance}/10");

        // Оценка общего SEO-потенциала
        double seoScore = (ctrImpact * 100 + contentRelevance * 10) / 2;
        Console.WriteLine($"📈 SEO-потенциал страницы: {seoScore}/100");
    }

    private double CalculateCtrImpact(string title, string description)
    {
        double score = 0.5; // Базовый уровень

        // Оптимизация title
        if (!string.IsNullOrEmpty(title))
        {
            if (title.Length >= 30 && title.Length <= 60) score += 0.15;
            if (title.Contains("🔥") || title.Contains("⭐")) score += 0.05; // Эмодзи
            if (title.Split(' ').Length >= 5) score += 0.1;
        }

        // Оптимизация description
        if (!string.IsNullOrEmpty(description))
        {
            if (description.Length >= 120 && description.Length <= 160) score += 0.2;
            if (description.Contains("узнайте") || description.Contains("как")) score += 0.05;
        }

        return Math.Min(score, 0.95); // Максимум 95%
    }

    private double AnalyzeContentRelevance(string htmlContent, string title)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);
        var textContent = doc.DocumentNode.InnerText;

        double score = 0;

        // Проверка соответствия title и контента
        if (!string.IsNullOrEmpty(title))
        {
            var titleKeywords = title.Split(new[] { ' ', ',', '!' },
                StringSplitOptions.RemoveEmptyEntries);
            score += titleKeywords.Count(keyword =>
                textContent.Contains(keyword)) * 0.5;
        }

        // Анализ плотности ключевых слов
        var contentWords = textContent.Split(' ');
        int totalWords = contentWords.Length;
        int keywordMatches = contentWords.Count(w => w.Equals("SEO", StringComparison.OrdinalIgnoreCase));

        double keywordDensity = (double)keywordMatches / totalWords;
        if (keywordDensity >= 0.5 && keywordDensity <= 2.5) score += 2;

        return Math.Min(score, 10);
    }
}