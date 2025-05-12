using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

public class HeadingAnalyzer
{
    public HeadingAnalysisResult AnalyzeHeadings(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var headings = doc.DocumentNode.SelectNodes("//h1|//h2|//h3|//h4|//h5|//h6")
            ?.Select(node => new Heading
            {
                Level = int.Parse(node.Name.Substring(1)),
                Text = node.InnerText.Trim(),
                Length = node.InnerText.Length
            })
            .ToList() ?? new List<Heading>();

        return new HeadingAnalysisResult
        {
            Headings = headings,
            HierarchyIssues = CheckHierarchy(headings)
        };
    }

    private List<string> CheckHierarchy(List<Heading> headings)
    {
        var issues = new List<string>();
        int lastLevel = 0;

        foreach (var heading in headings)
        {
            if (heading.Level > lastLevel + 1)
                issues.Add($"Некорректный переход с h{lastLevel} на h{heading.Level}");

            lastLevel = heading.Level;
        }

        var h1Count = headings.Count(h => h.Level == 1);
        if (h1Count == 0) issues.Add("Отсутствует H1 заголовок");
        if (h1Count > 1) issues.Add($"Найдено {h1Count} H1 заголовков");

        return issues;
    }
}

public class Heading
{
    public int Level { get; set; }
    public string Text { get; set; }
    public int Length { get; set; }
}

public class HeadingAnalysisResult
{
    public List<Heading> Headings { get; set; }
    public List<string> HierarchyIssues { get; set; }
}