using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

public class LinkAnalyzer
{
    public LinkAnalysisResult AnalyzeLinks(string html, string baseUrl)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var links = doc.DocumentNode.SelectNodes("//a[@href]")
            ?.Select(a => new LinkInfo
            {
                Href = a.GetAttributeValue("href", ""),
                Text = a.InnerText.Trim(),
                IsInternal = IsInternalLink(a.GetAttributeValue("href", ""), baseUrl),
                NoFollow = a.GetAttributeValue("rel", "").Contains("nofollow")
            })
            .ToList() ?? new List<LinkInfo>();

        return new LinkAnalysisResult
        {
            TotalLinks = links.Count,
            InternalLinks = links.Count(l => l.IsInternal),
            ExternalLinks = links.Count(l => !l.IsInternal),
            NoFollowLinks = links.Count(l => l.NoFollow)
        };
    }

    private bool IsInternalLink(string href, string baseUrl)
    {
        if (string.IsNullOrEmpty(href)) return false;
        if (href.StartsWith("#")) return true;

        Uri baseUri = new Uri(baseUrl);
        return Uri.TryCreate(baseUri, href, out Uri resultUri) &&
               resultUri.Authority == baseUri.Authority;
    }
}

public class LinkInfo
{
    public string Href { get; set; }
    public string Text { get; set; }
    public bool IsInternal { get; set; }
    public bool NoFollow { get; set; }
}

public class LinkAnalysisResult
{
    public int TotalLinks { get; set; }
    public int InternalLinks { get; set; }
    public int ExternalLinks { get; set; }
    public int NoFollowLinks { get; set; }
}