using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeTag
{
    public partial class Main : Form
    {
        private readonly WebAgent _webAgent;
        private readonly MetaTagAnalyzer _metaAnalyzer;
        private readonly MetaTagView _metaTagView;

        private DataGridView metaDataGrid;


        public Main()
        {
            InitializeComponent();
            InitializeCustomComponents();
            InitializeAdditionalComponents();

            _webAgent = new WebAgent();
            _metaAnalyzer = new MetaTagAnalyzer();
            _metaTagView = new MetaTagView(metaDataGrid);
        }

        // Добавляем новые элементы
        private TabPage linksTab;
        private TabPage speedTab;
        private DataGridView linksGrid;
        private Label lblPerformanceScore;

        // Добавляем элементы управления для вкладки заголовков
        private TabPage headingsTab;
        private DataGridView headingsGrid;

        private void InitializeAdditionalComponents()
        {
            // Создаем вкладку для ссылок
            var linksTab = new TabPage("Ссылки");
            linksGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 35
            };

            // Добавляем колонки
            linksGrid.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "Тип", Name = "Type", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "Количество", Name = "Count", Width = 100 }
            );

            linksTab.Controls.Add(linksGrid);
            contentTabs.TabPages.Add(linksTab);

            // Создаем вкладку для заголовков
            headingsTab = new TabPage("Заголовки");
            headingsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Columns =
            {
                new DataGridViewTextBoxColumn { HeaderText = "Уровень", Name = "Level", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "Текст", Name = "Text" },
                new DataGridViewTextBoxColumn { HeaderText = "Длина", Name = "Length", Width = 80 }
            }
            };
            headingsTab.Controls.Add(headingsGrid);

            // Вкладка для производительности
            speedTab = new TabPage("Производительность");
            lblPerformanceScore = new Label
            {
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 14),
                Padding = new Padding(20)
            };
            speedTab.Controls.Add(lblPerformanceScore);
            contentTabs.TabPages.Add(speedTab);
            contentTabs.TabPages.Add(headingsTab);
        }

        private void InitializeCustomComponents()
        {
            var metaTab = new TabPage("Мета-данные");

            metaDataGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                Name = "metaDataGrid",
                ColumnHeadersHeight = 40,
                BackgroundColor = Color.White
            };

            // Добавьте колонки
            metaDataGrid.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "Тип" },
                new DataGridViewTextBoxColumn { HeaderText = "Название" },
                new DataGridViewTextBoxColumn { HeaderText = "Значение" }
            );

            this.navigate["Мета-данные"] += () =>
            {
                var temp = new TabPage("Мета-данные");
                metaDataGrid.Parent = temp;
                metaTab.Dispose();
                this.contentTabs.TabPages.Add(temp);
                contentTabs.SelectTab(temp);

                metaTab = temp;

            };
        }

        private async void analyzeButton_Click(object sender, EventArgs e)
        {
            /*
            try
            {
            */
            ToggleLoadingState(true);

            var url = urlTextBox.Text.Trim();
            _webAgent.ValidateUrl(ref url);
            var html = await _webAgent.LoadPageHtml(url);

            // Основной анализ

            var metaResult = _metaAnalyzer.Analyze(html, url);
            var headings = new HeadingAnalyzer().AnalyzeHeadings(html);
            var images = new ImageAnalyzer().AnalyzeImages(html);
            var links = new LinkAnalyzer().AnalyzeLinks(html, url);
            var speedResult = await new PageSpeedAnalyzer("AIzaSyDDmdBVuKm8qyTbibP6DIaK-664dMerqPw").Analyze(url);
            var googleSearchs = new GoogleSearchService();
            int position = await googleSearchs.GetSearchPosition(url, url);

            // Обновление UI
            _metaTagView.UpdateView(metaResult.MetaTags);
            UpdateHeadingsView(headings);
            UpdateLinksView(links);

            CreateMetricsPanel(new Metric("Производительность", $"{speedResult.PerformanceScore:F1}", ""),
                               new Metric("Первая отрисовка", $"{speedResult.FirstContentfulPaint:F0} мс", ""),
                                new Metric("Позиция в поисковике", $"{position}", "Если -1, то не входит в топ-100"));

            /*
            // Сохранение истории
            new HistoryManager().SaveToHistory(new AnalysisResult
            {
                Url = url,
                MetaTags = metaResult.MetaTags,
                Headings = headings,
                Images = images,
                Links = links,
                Speed = speedResult
            });
            */

            UpdateStatusBar(metaResult.CriticalIssues);
            /*
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка анализа: {ex.Message}");
            }
            finally
            {
            */
            ToggleLoadingState(false);
            /*
            }
            */
        }

        private void UpdateStatusBar(List<string> issues)
        {
            statusBar.Items.Clear();
            foreach (var issue in issues)
            {
                statusBar.Items.Add(issue);
            }
        }

        private void UpdateHeadingsView(HeadingAnalysisResult result)
        {
            headingsGrid.Rows.Clear();

            foreach (var heading in result.Headings)
            {
                headingsGrid.Rows.Add(
                    $"H{heading.Level}",
                    heading.Text,
                    heading.Length
                );
            }
        }

        private void UpdateLinksView(LinkAnalysisResult result)
        {
            linksGrid.Rows.Clear();
            linksGrid.Rows.Add("Всего ссылок", result.TotalLinks);
            linksGrid.Rows.Add("Внутренние", result.InternalLinks);
            linksGrid.Rows.Add("Внешние", result.ExternalLinks);
            linksGrid.Rows.Add("NoFollow", result.NoFollowLinks);
        }

        private void ToggleLoadingState(bool isLoading)
        {
            analyzeButton.Enabled = !isLoading;
            urlTextBox.Enabled = !isLoading;
            statusBar.Items[0].Text = isLoading ? "Идет анализ..." : "Готово";
        }

        private void ShowError(string message)
        {
            statusBar.Items.Clear();
            statusBar.Items.Add(message);
            urlTextBox.BackColor = Color.FromArgb(255, 235, 238);
        }
    }
}
