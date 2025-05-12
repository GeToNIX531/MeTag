using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

public class ImageAnalyzer
{
    public ImageAnalysisResult AnalyzeImages(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var images = doc.DocumentNode.SelectNodes("//img")
            ?.Select(img => new ImageInfo
            {
                Src = img.GetAttributeValue("src", ""),
                Alt = img.GetAttributeValue("alt", ""),
                Width = ParseSize(img.GetAttributeValue("width", "")),
                Height = ParseSize(img.GetAttributeValue("height", "")),
                HasSrcset = !string.IsNullOrEmpty(img.GetAttributeValue("srcset", ""))
            })
            .ToList() ?? new List<ImageInfo>();

        return new ImageAnalysisResult
        {
            TotalImages = images.Count,
            ImagesWithoutAlt = images.Count(i => string.IsNullOrEmpty(i.Alt)),
            ImagesWithoutDimensions = images.Count(i => i.Width == 0 || i.Height == 0),
            ResponsiveImages = images.Count(i => i.HasSrcset)
        };
    }

    private int ParseSize(string value)
    {
        return int.TryParse(value, out int size) ? size : 0;
    }
}

public class ImageInfo
{
    public string Src { get; set; }       // Путь к изображению
    public string Alt { get; set; }        // Альтернативный текст
    public int Width { get; set; }         // Ширина в пикселях
    public int Height { get; set; }        // Высота в пикселях
    public bool HasSrcset { get; set; }    // Наличие адаптивных версий
}

public class ImageAnalysisResult
{
    public int TotalImages { get; set; }          // Всего изображений
    public int ImagesWithoutAlt { get; set; }     // Без alt-текста
    public int ImagesWithoutDimensions { get; set; } // Без размеров
    public int ResponsiveImages { get; set; }     // Адаптивные изображения
}