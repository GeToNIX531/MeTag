using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFReport
{
    class Program
    {
        static void Main(string[] args)
        {
            // Пример использования
            var generator = new SeoReportGenerator("seo_history.json");
            generator.GenerateReport("SEO_Report.pdf");
            Console.ReadLine();
        }
    }
}
