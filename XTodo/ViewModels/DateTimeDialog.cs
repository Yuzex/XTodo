using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace XTodo.ViewModels;

public class DateTimeDialog : Window
{
    public DateTime? SelectedDateTime { get; private set; }

    public DateTimeDialog(DateTime? initial = null)
    {
        Title = "设置预计完成时间";
        WindowStyle = WindowStyle.ToolWindow;
        SizeToContent = SizeToContent.WidthAndHeight;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var grid = new Grid { Margin = new Thickness(12) };
        grid.RowDefinitions.Add(new RowDefinition());
        grid.RowDefinitions.Add(new RowDefinition());
        grid.RowDefinitions.Add(new RowDefinition());

        // 日历
        var cal = new Calendar
        {
            SelectedDate = initial ?? DateTime.Today,
            Margin = new Thickness(0, 0, 0, 8)
        };
        grid.Children.Add(cal);

        // 时分输入
        var timePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        var timeLabel = new TextBlock
        {
            Text = "时间：",
            FontFamily = new FontFamily("Microsoft YaHei"),
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0)
        };
        timePanel.Children.Add(timeLabel);

        var hourBox = new TextBox
        {
            Text = initial?.Hour.ToString("D2") ?? "00",
            FontFamily = new FontFamily("Microsoft YaHei"),
            FontSize = 12,
            Width = 36,
            TextAlignment = TextAlignment.Center
        };
        timePanel.Children.Add(hourBox);

        var sep = new TextBlock
        {
            Text = " : ",
            FontFamily = new FontFamily("Microsoft YaHei"),
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center
        };
        timePanel.Children.Add(sep);

        var minuteBox = new TextBox
        {
            Text = initial?.Minute.ToString("D2") ?? "00",
            FontFamily = new FontFamily("Microsoft YaHei"),
            FontSize = 12,
            Width = 36,
            TextAlignment = TextAlignment.Center
        };
        timePanel.Children.Add(minuteBox);

        Grid.SetRow(timePanel, 1);
        grid.Children.Add(timePanel);

        // 按钮
        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var clearBtn = new Button
        {
            Content = "清除",
            Width = 72,
            Height = 28,
            Margin = new Thickness(0, 0, 8, 0),
            FontFamily = new FontFamily("Microsoft YaHei"),
            FontSize = 12
        };
        clearBtn.Click += (_, _) => { SelectedDateTime = null; DialogResult = true; Close(); };
        btnPanel.Children.Add(clearBtn);

        var cancelBtn = new Button
        {
            Content = "取消",
            Width = 72,
            Height = 28,
            Margin = new Thickness(0, 0, 8, 0),
            FontFamily = new FontFamily("Microsoft YaHei"),
            FontSize = 12
        };
        cancelBtn.Click += (_, _) => { DialogResult = false; Close(); };
        btnPanel.Children.Add(cancelBtn);

        var okBtn = new Button
        {
            Content = "确定",
            Width = 72,
            Height = 28,
            FontFamily = new FontFamily("Microsoft YaHei"),
            FontSize = 12
        };
        okBtn.Click += (_, _) =>
        {
            if (cal.SelectedDate.HasValue &&
                int.TryParse(hourBox.Text, out var h) &&
                int.TryParse(minuteBox.Text, out var m) &&
                h >= 0 && h <= 23 && m >= 0 && m <= 59)
            {
                SelectedDateTime = cal.SelectedDate.Value.Date.AddHours(h).AddMinutes(m);
            }
            else if (cal.SelectedDate.HasValue)
            {
                SelectedDateTime = cal.SelectedDate.Value.Date;
            }
            DialogResult = true;
            Close();
        };
        btnPanel.Children.Add(okBtn);

        Grid.SetRow(btnPanel, 2);
        grid.Children.Add(btnPanel);

        Content = grid;
    }
}
