using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MeTag
{
    partial class Main
    {
        private System.ComponentModel.IContainer components = null;

        // Основные цветовые константы
        private readonly Color MainColor = Color.FromArgb(51, 51, 76);
        private readonly Color SecondaryColor = Color.FromArgb(224, 224, 224);
        private readonly Color AccentColor = Color.FromArgb(0, 150, 136);

        // Основные элементы
        protected TextBox urlTextBox;
        protected Button analyzeButton;
        protected StatusStrip statusBar;
        protected TabControl contentTabs;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // Основные настройки формы
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(9F, 21F);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            this.BackColor = Color.FromArgb(245, 245, 248);
            this.ForeColor = Color.FromArgb(64, 64, 64);
            this.MinimumSize = new Size(1000, 700);

            // Основной контейнер
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(20),
                RowStyles = {
            new RowStyle(SizeType.Absolute, 80),
            new RowStyle(SizeType.Percent, 100),
            new RowStyle(SizeType.Absolute, 40)
        }
            };

            // Панель управления
            var controlPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15),
            };

            var inputContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                ColumnStyles = {
            new ColumnStyle(SizeType.Percent, 100),
            new ColumnStyle(SizeType.Absolute, 150)
        }
            };

            this.urlTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(44, 44, 46),
                Margin = new Padding(0, 5, 10, 5),
                SelectedText = "Введите URL сайта для анализа"
            };

            this.analyzeButton = new Button
            {
                Text = "Поиск",
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(15, 0, 15, 0)
            };
            analyzeButton.FlatAppearance.BorderSize = 0;
            analyzeButton.Click += analyzeButton_Click;

            inputContainer.Controls.Add(urlTextBox, 0, 0);
            inputContainer.Controls.Add(analyzeButton, 1, 0);
            controlPanel.Controls.Add(inputContainer);

            // Основная рабочая область
            var workspace = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 200,
                Panel1 = {
            BackColor = Color.White,
            Padding = new Padding(15)
        },
                Panel2 = {
            BackColor = Color.FromArgb(250, 250, 252)
        }
            };

            // Навигационная панель
            var navPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            var navItems = navigate.Keys;

            var yPos = 0;
            foreach (var item in navItems)
            {
                var btn = new Button
                {
                    Text = item,
                    Height = 45,
                    Width = 270,
                    Location = new Point(0, yPos),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = Color.FromArgb(64, 64, 64),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 10),
                    Padding = new Padding(20, 0, 0, 0),
                };
                btn.Click += NavButtonClick;
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 242);
                navPanel.Controls.Add(btn);
                yPos += 50;
            }

            workspace.Panel1.Controls.Add(navPanel);

            // Контентная область
            contentTabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Appearance = TabAppearance.Normal,
                ItemSize = new Size(120, 30),
                SizeMode = TabSizeMode.Fixed,
                Padding = new Point(3, 3),
                DrawMode = TabDrawMode.Normal
            };

            var metricsPanel = CreateMetricsPanel(new Metric[]
            {
                new Metric("Общий рейтинг", "92/100","Оптимизация выше среднего"),
                new Metric("Скорость загрузки", "4.2 сек", "Требуется оптимизация"),
                new Metric( "Позиции в поиске", "Топ-10 × 8","+2 за неделю"),

            });

            
            workspace.Panel2.Controls.Add(contentTabs);

            // Статус бар
            this.statusBar = new StatusStrip
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Renderer = new ToolStripProfessionalRenderer(new CustomColors())
            };
            statusBar.Items.AddRange(new ToolStripItem[]
            {
        new ToolStripStatusLabel("Готов к работе") {
            ForeColor = Color.FromArgb(128, 128, 128)
        },
        new ToolStripProgressBar {
            Style = ProgressBarStyle.Marquee,
            Visible = false
        }
            });

            mainContainer.Controls.Add(controlPanel, 0, 0);
            mainContainer.Controls.Add(workspace, 0, 1);
            mainContainer.Controls.Add(statusBar, 0, 2);

            this.Controls.Add(mainContainer);
            this.ResumeLayout(false);
        }

        private Panel CreateMetricsPanel(params Metric[] metrics)
        {
            var panel = new Panel { Dock = DockStyle.Fill };

            // Заголовок
            var header = new Label
            {
                Text = "Ключевые показатели",
                Dock = DockStyle.Top,
                Height = 60,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 44, 46),
                Padding = new Padding(20, 15, 0, 0)
            };

            // Контейнер метрик
            var metricsContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(20, 0, 20, 20),
                ColumnStyles = {
            new ColumnStyle(SizeType.Percent, 33.33F),
            new ColumnStyle(SizeType.Percent, 33.33F),
            new ColumnStyle(SizeType.Percent, 33.33F)
        }
            };



            foreach (var metric in metrics)
            {
                var metricCard = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Margin = new Padding(10),
                    Padding = new Padding(20)
                };

                var titleLabel = new Label
                {
                    Text = metric.Title,
                    Font = new Font("Segoe UI", 10, FontStyle.Regular),
                    ForeColor = Color.FromArgb(128, 128, 128),
                    Dock = DockStyle.Top,
                    Height = 30
                };

                var valueLabel = new Label
                {
                    Text = metric.Value,
                    Font = new Font("Segoe UI", 24, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 122, 204),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                var descLabel = new Label
                {
                    Text = metric.Description,
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(160, 160, 160),
                    Dock = DockStyle.Bottom,
                    Height = 25
                };

                metricCard.Controls.Add(descLabel);
                metricCard.Controls.Add(valueLabel);
                metricCard.Controls.Add(titleLabel);
                metricsContainer.Controls.Add(metricCard);
            }

            panel.Controls.Add(metricsContainer);
            panel.Controls.Add(header);

            contentTabs.TabPages.Add(new TabPage("Основные метрики")
            {
                Controls = { panel }
            });

            return panel;
        }

        public class Metric
        {
            public string Title;
            public string Value;
            public string Description;

            public Metric(string title, string value, string description)
            {
                Title = title;
                Value = value;
                Description = description;
            }
        }

        public Dictionary<string, Action> navigate = new Dictionary<string, Action>()
        {
            { "Обзор", null},
            { "Мета-данные", null},
            { "Производительность", null},
            { "Конкурентный анализ", null},
            { "История изменений", null}
        };

        public void NavButtonClick(object Object, System.EventArgs Args)
        {
            if (navigate.TryGetValue(((Button)Object).Text, out Action Value))
                Value?.Invoke();
        }


        // Кастомные цвета для элементов управления
        private class CustomColors : ProfessionalColorTable
        {
            public override Color MenuItemSelected => Color.FromArgb(240, 240, 242);
            public override Color MenuItemBorder => Color.Transparent;
            public override Color ToolStripBorder => Color.Transparent;
        }
    }
}