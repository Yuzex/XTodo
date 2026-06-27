using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using XTodo.Models;

namespace XTodo.ViewModels;

public class CriteriaEditDialog : Window
{
    private readonly TaskItem _task;
    private readonly TextBox _textBox;

    public CriteriaEditDialog(TaskItem task)
    {
        _task = task;
        Title = "编辑闭环条件";
        WindowStyle = WindowStyle.ToolWindow;
        SizeToContent = SizeToContent.WidthAndHeight;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var grid = new Grid { Margin = new Thickness(12), Width = 380 };
        grid.RowDefinitions.Add(new RowDefinition());
        grid.RowDefinitions.Add(new RowDefinition());

        var label = new TextBlock
        {
            Text = "描述任务的完成标准（可选）：",
            FontFamily = new FontFamily("Microsoft YaHei"),
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(0x5D, 0x40, 0x37)),
            Margin = new Thickness(0, 0, 0, 8)
        };
        grid.Children.Add(label);

        _textBox = new TextBox
        {
            Text = task.CompletionCriteria,
            FontFamily = new FontFamily("Microsoft YaHei"),
            FontSize = 13,
            MinHeight = 80,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Padding = new Thickness(6),
            Margin = new Thickness(0, 0, 0, 12)
        };
        Grid.SetRow(_textBox, 1);
        grid.Children.Add(_textBox);

        // 按钮行
        var btnRow = new Grid();
        btnRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        btnRow.ColumnDefinitions.Add(new ColumnDefinition());
        btnRow.ColumnDefinitions.Add(new ColumnDefinition());
        Grid.SetRow(btnRow, 2);
        grid.Children.Add(btnRow);

        var cancelBtn = new Button
        {
            Content = "取消",
            Width = 72,
            Height = 28,
            FontFamily = new FontFamily("Microsoft YaHei"),
            FontSize = 12
        };
        cancelBtn.Click += (_, _) => { DialogResult = false; Close(); };
        Grid.SetColumn(cancelBtn, 1);
        btnRow.Children.Add(cancelBtn);

        var okBtn = new Button
        {
            Content = "确定",
            Width = 72,
            Height = 28,
            Margin = new Thickness(8, 0, 0, 0),
            FontFamily = new FontFamily("Microsoft YaHei"),
            FontSize = 12
        };
        okBtn.Click += (_, _) =>
        {
            _task.CompletionCriteria = _textBox.Text;
            DialogResult = true;
            Close();
        };
        Grid.SetColumn(okBtn, 2);
        btnRow.Children.Add(okBtn);

        Content = grid;

        Loaded += (_, _) => _textBox.Focus();
    }
}
