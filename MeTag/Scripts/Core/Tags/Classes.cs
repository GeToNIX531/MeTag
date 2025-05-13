using System;
using System.Collections.Generic;

public class MetaTag
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
    public string Status { get; set; }
}

public class AnalysisResult
{
    public string Url { get; set; }
    public DateTime Time { get; set; }
    public List<MetaTag> MetaTags { get; set; }
    public List<string> CriticalIssues { get; set; }

    public HeadingAnalysisResult Headings{ get; set; }
    public ImageAnalysisResult Images { get; set; }
    public LinkAnalysisResult Links { get; set; }
    public PageSpeedResult Speed { get; set; }
}