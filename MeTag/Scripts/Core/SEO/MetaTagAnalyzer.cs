using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

public class MetaTagAnalyzer
{
    public AnalysisResult Analyze(string html, string url)
    {
        var doc = LoadHtmlDocument(html);
        var metaTags = ParseMetaTags(doc);

        return new AnalysisResult
        {
            Url = url,
            MetaTags = metaTags,
            CriticalIssues = ValidateCriticalTags(metaTags)
        };
    }

    private HtmlDocument LoadHtmlDocument(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc;
    }

    private List<MetaTag> ParseMetaTags(HtmlDocument doc)
    {
        return doc.DocumentNode.SelectNodes("//meta")
            ?.Select(meta => new MetaTag
            {
                Type = GetTagType(meta),
                Name = GetTagName(meta),
                Value = meta.GetAttributeValue("content", "")
            })
            .Where(tag => !string.IsNullOrEmpty(tag.Name))
            .ToList() ?? new List<MetaTag>();
    }

    private string GetTagType(HtmlNode meta)
    {
        return meta.Attributes["property"] != null ? "Open Graph" :
               meta.Attributes["itemprop"] != null ? "Schema.org" :
               meta.Attributes["http-equiv"] != null ? "HTTP-Header" : "Standard";
    }

    private string GetTagName(HtmlNode meta)
    {
        return meta.GetAttributeValue("name",
            meta.GetAttributeValue("property",
                meta.GetAttributeValue("itemprop", "")));
    }

    private List<string> ValidateCriticalTags(List<MetaTag> tags)
    {
        var criticalTags = new Dictionary<string, string>
        {
            { "title", "Отсутствует тег title" },
            { "description", "Отсутствует meta description" },
            { "viewport", "Не настроен viewport" }
        };

        return criticalTags
            .Where(kv => !tags.Any(t => t.Name.Equals(kv.Key, StringComparison.OrdinalIgnoreCase)))
            .Select(kv => kv.Value)
            .ToList();
    }
}