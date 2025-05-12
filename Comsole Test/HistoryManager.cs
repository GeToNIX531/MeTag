using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class HistoryManager
{
    const string HistoryFile = "seo_history.json";

    public static void SaveSnapshot(SeoSnapshot snapshot)
    {
        var history = LoadHistory();
        history.Add(snapshot);

        File.WriteAllText(HistoryFile,
            System.Text.Json.JsonSerializer.Serialize(history, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static List<SeoSnapshot> LoadHistory()
    {
        if (!File.Exists(HistoryFile)) return new List<SeoSnapshot>();

        return System.Text.Json.JsonSerializer.Deserialize<List<SeoSnapshot>>(
            File.ReadAllText(HistoryFile)) ?? new List<SeoSnapshot>();
    }
}

class SeoSnapshot
{
    public DateTime Date { get; set; }
    public string Url { get; set; }
    public List<MetaTag> MetaTags { get; set; }
    public TrafficStats Traffic { get; set; }
    public SeoScores Scores { get; set; }
}

class TrafficStats
{
    public int MonthlyVisitors { get; set; }
    public double BounceRate { get; set; }
    public double AvgVisitDuration { get; set; }
}

class SeoScores
{
    public double CtrScore { get; set; }
    public double RelevanceScore { get; set; }
    public double TotalScore { get; set; }
}