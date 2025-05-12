using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class MetaTagView
{
    private readonly DataGridView _grid;

    public MetaTagView(DataGridView grid)
    {
        _grid = grid;
        ConfigureGrid();
    }

    private void ConfigureGrid()
    {
        _grid.Columns.Clear();
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Тип тега", Name = "TagType", Width = 150 },
            new DataGridViewTextBoxColumn { HeaderText = "Название", Name = "TagName", Width = 200 },
            new DataGridViewTextBoxColumn { HeaderText = "Значение", Name = "TagValue" },
            new DataGridViewTextBoxColumn { HeaderText = "Статус", Name = "Status", Width = 120 }
        );

        _grid.DefaultCellStyle.BackColor = Color.White;
        _grid.RowHeadersVisible = false;
    }

    public void UpdateView(IEnumerable<MetaTag> tags)
    {
        _grid.Rows.Clear();

        foreach (var tag in tags)
        {
            var row = new DataGridViewRow();
            row.CreateCells(_grid);

            row.Cells[0].Value = tag.Type;
            row.Cells[1].Value = tag.Name;
            row.Cells[2].Value = ShortenValue(tag.Value);
            row.Cells[3].Value = tag.Status;

            ApplyRowStyle(row, tag.Status);
            _grid.Rows.Add(row);
        }
    }

    private string ShortenValue(string value, int maxLength = 100)
    {
        return value.Length > maxLength
            ? value.Substring(0, maxLength) + "..."
            : value;
    }

    private void ApplyRowStyle(DataGridViewRow row, string status)
    {
        switch (status)
        {
            case ("❌ Отсутствует"):
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 238);
                break;
            case ("⚠️ Слишком короткий"):
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 244, 229);
                break;
            case ("⚠️ Требует настройки"):
                row.DefaultCellStyle.BackColor = Color.FromArgb(229, 246, 253);
                break;
            default:
                break;
        }
    }
}