using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using ScottPlot;

using Color = MigraDoc.DocumentObjectModel.Color;
using Colors = MigraDoc.DocumentObjectModel.Colors;

public class SeoReportGenerator
{
    private readonly List<SeoData> _seoHistory;

    public SeoReportGenerator(string jsonFilePath)
    {
        var json = File.ReadAllText(jsonFilePath);
        _seoHistory = JsonConvert.DeserializeObject<List<SeoData>>(json)
            ?? new List<SeoData>();
    }

    public void GenerateReport(string outputPath)
    {
        var document = new Document();
        DefineStyles(document);

        AddTitlePage(document);
        AddTableOfContents(document);
        AddSummarySection(document);
        AddDetailedAnalysis(document);
        AddHistoricalData(document);
        AddAppendix(document);

        RenderDocument(document, outputPath);
    }

    private void DefineStyles(Document document)
    {
        var normalStyle = document.Styles["Normal"];
        normalStyle.Font.Name = "Arial";
        normalStyle.Font.Size = 10;

        var titleStyle = document.Styles.AddStyle("Title", "Normal");
        titleStyle.Font.Size = 18;
        titleStyle.Font.Bold = true;
        titleStyle.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        titleStyle.ParagraphFormat.SpaceAfter = "2cm";

        var header1Style = document.Styles.AddStyle("Header1", "Normal");
        header1Style.Font.Size = 14;
        header1Style.Font.Bold = true;
        header1Style.ParagraphFormat.SpaceBefore = "1cm";
        header1Style.ParagraphFormat.SpaceAfter = "0.5cm";
        header1Style.ParagraphFormat.OutlineLevel = OutlineLevel.Level1;

        var header2Style = document.Styles.AddStyle("Header2", "Normal");
        header2Style.Font.Size = 12;
        header2Style.Font.Bold = true;
        header2Style.Font.Color = Colors.DarkBlue;
        header2Style.ParagraphFormat.SpaceBefore = "0.5cm";

        var tableHeaderStyle = document.Styles.AddStyle("TableHeader", "Normal");
        tableHeaderStyle.Font.Bold = true;
    }

    private void AddTitlePage(Document document)
    {
        var section = document.AddSection();
        var title = section.AddParagraph("SEO и Performance Отчёт");
        title.Style = "Title";

        var subtitle = section.AddParagraph($"Период данных: {GetDataPeriod()}");
        subtitle.Format.Alignment = ParagraphAlignment.Center;
        subtitle.Format.SpaceAfter = "1cm";

        var dateInfo = section.AddParagraph($"Сгенерировано: {DateTime.Now:yyyy-MM-dd HH:mm}");
        dateInfo.Format.Alignment = ParagraphAlignment.Center;

        section.AddPageBreak();
    }

    private string GetDataPeriod()
    {
        if (_seoHistory.Count == 0) return "Нет данных";
        var minDate = _seoHistory.Min(x => x.Time);
        var maxDate = _seoHistory.Max(x => x.Time);
        return $"{minDate:yyyy-MM-dd} — {maxDate:yyyy-MM-dd}";
    }

    private void AddTableOfContents(Document document)
    {
        var section = document.LastSection;
        var toc = section.AddParagraph("Оглавление");
        toc.Style = "Header1";

        var paragraph = section.AddParagraph();
        paragraph.AddTab();
        paragraph.AddText("Ключевые показатели \t 4");
        paragraph.AddLineBreak();
        paragraph.AddTab();
        paragraph.AddText("Детальный анализ \t 5");
        paragraph.AddLineBreak();
        paragraph.AddTab();
        paragraph.AddText("Исторические данные \t 8");

        section.AddPageBreak();
    }

    private void AddSummarySection(Document document)
    {
        var section = document.LastSection;
        section.AddParagraph("Ключевые показатели").Style = "Header1";

        if (_seoHistory.Count == 0)
        {
            section.AddParagraph("Нет данных для отображения").Format.Font.Color = Colors.Red;
            return;
        }

        var table = section.AddTable();
        table.Style = "Table";
        table.Borders.Width = 0.25;
        table.Rows.Alignment = RowAlignment.Center;

        // Автоподбор ширины столбцов
        table.AddColumn(Unit.FromCentimeter(4)).Format.Alignment = ParagraphAlignment.Center;
        table.AddColumn(Unit.FromCentimeter(4));
        table.AddColumn(Unit.FromCentimeter(4));
        table.AddColumn(Unit.FromCentimeter(4));

        var headerRow = table.AddRow();
        headerRow.Style = "TableHeader";
        headerRow.Cells[0].AddParagraph("Дата");
        headerRow.Cells[1].AddParagraph("Производительность");
        headerRow.Cells[2].AddParagraph("FCP (ms)");
        headerRow.Cells[3].AddParagraph("Ссылки");

        foreach (var entry in _seoHistory.OrderBy(x => x.Time))
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph(entry.Time.ToString("yyyy-MM-dd"));
            row.Cells[1].AddParagraph(entry.Speed.PerformanceScore.ToString());
            row.Cells[2].AddParagraph(entry.Speed.FirstContentfulPaint.ToString("N0"));
            row.Cells[3].AddParagraph(entry.Links?.TotalLinks.ToString() ?? "N/A");
        }

        AddPerformanceTrend(section);
    }

    private void AddPerformanceTrend(Section section)
    {
        var lastTwo = _seoHistory
            .OrderBy(x => x.Time).ToList();
        lastTwo.Reverse();
        lastTwo = lastTwo.Take(2).ToList();
        lastTwo.Reverse();

        if (lastTwo.Count == 2)
        {
            var diff = lastTwo[1].Speed.PerformanceScore - lastTwo[0].Speed.PerformanceScore;

            string trend;
            if (diff > 0)
                trend = "↑ Улучшение";
            else if (diff < 0)
                trend = "↓ Ухудшение";
            else trend = "→ Без изменений";


            var trendParagraph = section.AddParagraph($"Тренд производительности: {trend} ({diff:+0;-0})");
            trendParagraph.Format.Font.Color = diff >= 0 ? Colors.DarkGreen : Colors.Red;
        }
    }

    private void AddDetailedAnalysis(Document document)
    {
        var section = document.AddSection();
        section.AddParagraph("Детальный анализ").Style = "Header1";

        AddMetaTagsTable(section);
        AddImagesAnalysis(section);
        AddHeadingsAnalysis(section);
    }

    private void AddMetaTagsTable(Section section)
    {
        section.AddParagraph("Мета-теги").Style = "Header2";

        if (_seoHistory[0].MetaTags?.Any() != true)
        {
            section.AddParagraph("Мета-теги не найдены").Format.Font.Color = Colors.Red;
            return;
        }

        var table = CreateTableWithHeader(section,
            new[] { "Тег", "Значение", "Статус" },
            new[] { 3.5, 8.0, 3.5 }
        );

        foreach (var tag in _seoHistory[0].MetaTags)
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph(tag.Name);
            row.Cells[1].AddParagraph(tag.Value);

            var statusCell = row.Cells[2];
            var status = GetTagStatus(tag);
            statusCell.AddParagraph(status.Text);
            statusCell.Format.Shading.Color = status.Color;
        }
    }

    private (string Text, Color Color) GetTagStatus(MetaTag tag)
    {
        if (tag.Name.Equals("description", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(tag.Value))
                return ("❌ Отсутствует", Colors.LightPink);

            if (tag.Value.Length < 120 || tag.Value.Length > 320)
                return ("⚠️ Неоптимально", Colors.LightYellow);
        }

        return ("✅ OK", Colors.LightGreen);
    }

    private Table CreateTableWithHeader(Section section, string[] headers, double[] columnWidths)
    {
        var table = section.AddTable();
        table.Borders.Width = 0.25;

        foreach (var width in columnWidths)
            table.AddColumn(Unit.FromCentimeter(width));

        var headerRow = table.AddRow();
        headerRow.Style = "TableHeader";
        for (int i = 0; i < headers.Length; i++)
            headerRow.Cells[i].AddParagraph(headers[i]);

        return table;
    }

    private void AddImagesAnalysis(Section section)
    {
        section.AddParagraph("Анализ изображений").Style = "Header2";

        if (_seoHistory[0].Images?.Items?.Any() != true)
        {
            section.AddParagraph("Изображения не найдены").Format.Font.Color = Colors.Red;
            return;
        }

        var table = CreateTableWithHeader(section,
            new[] { "Источник", "Alt-текст", "Размеры", "Рекомендации" },
            new[] { 6.0, 3.0, 3.0, 4.0 }
        );

        foreach (var img in _seoHistory[0].Images.Items.Take(10)) // Ограничение для примера
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph(TruncateString(img.Src, 50));
            row.Cells[1].AddParagraph(string.IsNullOrEmpty(img.Alt) ? "❌ Отсутствует" : "✅ Присутствует");
            row.Cells[2].AddParagraph(img.Width > 0 && img.Height > 0
                ? $"{img.Width}x{img.Height}"
                : "❌ Не указаны");

            var recommendations = new List<string>();
            if (img.Width > 1920) recommendations.Add("Слишком широкое");
            if (img.Height > 1080) recommendations.Add("Слишком высокое");
            row.Cells[3].AddParagraph(string.Join(", ", recommendations));
        }

        if (_seoHistory[0].Images.Items.Count > 10)
            section.AddParagraph($"... Показано 10 из {_seoHistory[0].Images.Items.Count} изображений");
    }

    private string TruncateString(string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input;

        // Для версий ниже C# 8.0 используем Substring
        return input.Substring(0, maxLength - 3) + "...";

        // В C# 8.0+ можно использовать:
        // return input[..(maxLength - 3)] + "...";
    }

    private void AddHeadingsAnalysis(Section section)
    {
        section.AddParagraph("Анализ заголовков").Style = "Header2";

        var headings = _seoHistory[0].Headings?.Items;
        if (headings?.Any() != true)
        {
            section.AddParagraph("Заголовки не найдены").Format.Font.Color = Colors.Red;
            return;
        }

        // Проверка иерархии
        var levels = headings.Select(h => h.Level).ToList();
        if (!IsValidHeadingHierarchy(levels))
            section.AddParagraph("⚠️ Нарушена иерархия заголовков!")
                 .Format.Font.Color = Colors.Red;

        foreach (var heading in headings)
        {
            var p = section.AddParagraph($"H{heading.Level}: {heading.Text}");
            p.Format.Font.Size = 14 - heading.Level;
            p.Format.LeftIndent = $"{heading.Level * 0.5}cm";
        }
    }

    private bool IsValidHeadingHierarchy(List<int> levels)
    {
        int currentLevel = 0;
        foreach (var level in levels)
        {
            if (level > currentLevel + 1) return false;
            currentLevel = level;
        }
        return true;
    }

    private void AddHistoricalData(Document document)
    {
        var section = document.AddSection();
        section.AddParagraph("Исторические данные").Style = "Header1";

        if (_seoHistory.Count < 2)
        {
            section.AddParagraph("Недостаточно данных для анализа истории")
                  .Format.Font.Color = Colors.Gray;
            return;
        }

        // Таблица с ключевыми метриками по датам
        var table = CreateTableWithHeader(section,
            new[] { "Дата", "Производительность", "FCP (ms)", "Ссылки", "Изображения" },
            new[] { 3.0, 3.0, 3.0, 3.0, 3.0 }
        );

        foreach (var entry in _seoHistory.OrderBy(x => x.Time))
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph(entry.Time.ToString("yyyy-MM-dd"));
            row.Cells[1].AddParagraph(entry.Speed.PerformanceScore.ToString());
            row.Cells[2].AddParagraph(entry.Speed.FirstContentfulPaint.ToString("N0"));
            row.Cells[3].AddParagraph(entry.Links?.TotalLinks.ToString() ?? "N/A");
            row.Cells[4].AddParagraph(entry.Images?.Items.Count.ToString() ?? "0");
        }

        // График изменения производительности (псевдокод)
        AddPerformanceChart(section);
    }

    private void AddPerformanceChart(Section section)
    {
        if (_seoHistory == null || _seoHistory.Count < 2)
        {
            section.AddParagraph("Недостаточно данных для построения графика")
                  .Format.Font.Color = Colors.Gray;
            return;
        }

        try
        {
            // 1. Инициализация графика
            var plt = new ScottPlot.Plot();

            // 3. Подготовка данных
            var orderedData = _seoHistory
                .OrderBy(x => x.Time)
                .ToList();

            double[] dates = orderedData
                .Select(x => x.Time.ToOADate())
                .ToArray();

            double[] scores = orderedData
                .Select(x => (double)x.Speed.PerformanceScore)
                .ToArray();

            // 4. Добавление данных и стилизация
            var scatter = plt.Add.Scatter(dates, scores);
            scatter.LineWidth = 2;
            scatter.Color = ScottPlot.Colors.DarkBlue;

            // 5. Настройка оформления
            plt.Title("Динамика производительности");
            plt.XLabel("Дата");
            plt.YLabel("Оценка (%)");

            // 7. Сохранение изображения
            string tempPath = Path.Combine(
                Path.GetTempPath(),
                $"chart_{Guid.NewGuid()}.png");

            plt.SavePng(tempPath, 800, 400);

            // 8. Добавление в документ
            var image = section.AddImage(tempPath);
            image.Width = "16cm";
            image.LockAspectRatio = true;
            Console.WriteLine(tempPath);

            // 9. Очистка
            //File.Delete(tempPath);

        }
        catch (Exception ex)
        {
            section.AddParagraph($"Ошибка при построении графика: {ex.Message}")
                  .Format.Font.Color = Colors.Red;
        }
    }

    private void AddAppendix(Document document)
    {
        var section = document.AddSection();
        section.AddParagraph("Приложение").Style = "Header1";
        section.AddParagraph("Метрики собирались с использованием Lighthouse v9.6.8");
    }

    private void RenderDocument(Document document, string outputPath)
    {
        try
        {
            var renderer = new PdfDocumentRenderer(true) { Document = document };
            renderer.RenderDocument();

            // Добавление колонтитулов
            for (int i = 0; i < renderer.PdfDocument.Pages.Count; i++)
            {
                PdfPage page = renderer.PdfDocument.Pages[i];
                int number = i + 1;

                var xGraph = XGraphics.FromPdfPage(page);
                xGraph.DrawString($"Страница {number}",
                                    new XFont("Arial", 8),
                                    XBrushes.Gray,
                                    new XPoint(page.Width - 50, page.Height - 20)
                    );
            }

            renderer.PdfDocument.Save(outputPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Ошибка генерации PDF", ex);
        }
    }
}


// Классы для десериализации JSON
public class SeoData
{
    public DateTime Time { get; set; }
    public List<MetaTag> MetaTags { get; set; }
    public HeadingStructure Headings { get; set; }
    public Images Images { get; set; }
    public Links Links { get; set; }
    public Speed Speed { get; set; }
}

public class MetaTag
{
    public string Name { get; set; }
    public string Value { get; set; }
}

public class HeadingStructure
{
    [JsonProperty("Headings")]
    public List<Heading> Items { get; set; }

    [JsonProperty("HierarchyIssues")]
    public List<string> Issues { get; set; }
}

public class Heading
{
    public int Level { get; set; }
    public string Text { get; set; }
}

public class Images
{
    [JsonProperty("Images")]
    public List<ImageInfo> Items { get; set; }
}

public class ImageInfo
{
    public string Src { get; set; }
    public string Alt { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class Links
{
    public int TotalLinks { get; set; }
}

public class Speed
{
    public int PerformanceScore { get; set; }
    public double FirstContentfulPaint { get; set; }
}

