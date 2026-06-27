using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace XTodo.ViewModels;

public class InputDialog : Window
{
    public string? Answer { get; private set; }

    public InputDialog(string title, string prompt)
    {
        Title = title;
        WindowStyle = WindowStyle.ToolWindow;
        SizeToContent = SizeToContent.WidthAndHeight;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var grid = new Grid { Margin = new Thickness(16) };
        grid.RowDefinitions.Add(new RowDefinition());
        grid.RowDefinitions.Add(new RowDefinition());
        grid.RowDefinitions.Add(new RowDefinition());

        var label = new TextBlock
        {
            Text = prompt,
            FontFamily = new System.Windows.Media.FontFamily("Microsoft YaHei"),
            FontSize = 13,
            Margin = new Thickness(0, 0, 0, 8)
        };
        Grid.SetRow(label, 0);
        grid.Children.Add(label);

        var textBox = new TextBox
        {
            FontFamily = new System.Windows.Media.FontFamily("Microsoft YaHei"),
            FontSize = 13,
            MinWidth = 240,
            Padding = new Thickness(4),
            Margin = new Thickness(0, 0, 0, 12)
        };
        Grid.SetRow(textBox, 1);
        grid.Children.Add(textBox);

        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var cancelBtn = new Button
        {
            Content = "取消",
            Width = 72,
            Height = 28,
            Margin = new Thickness(0, 0, 8, 0),
            FontFamily = new System.Windows.Media.FontFamily("Microsoft YaHei"),
            FontSize = 12
        };
        cancelBtn.Click += (_, _) => { DialogResult = false; Close(); };
        btnPanel.Children.Add(cancelBtn);

        var okBtn = new Button
        {
            Content = "确定",
            Width = 72,
            Height = 28,
            FontFamily = new System.Windows.Media.FontFamily("Microsoft YaHei"),
            FontSize = 12
        };
        okBtn.Click += (_, _) =>
        {
            Answer = textBox.Text;
            DialogResult = true;
            Close();
        };
        btnPanel.Children.Add(okBtn);

        Grid.SetRow(btnPanel, 2);
        grid.Children.Add(btnPanel);

        Content = grid;

        Loaded += (_, _) => textBox.Focus();

        textBox.KeyDown += (_, args) =>
        {
            if (args.Key == Key.Enter)
            {
                Answer = textBox.Text;
                DialogResult = true;
                Close();
            }
            else if (args.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        };
    }
}
