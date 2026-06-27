using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using XTodo.ViewModels;

namespace XTodo.Views;

public partial class TaskTreeView : UserControl
{
    public TaskTreeView()
    {
        InitializeComponent();
    }

    private void TaskName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount >= 2 && sender is TextBlock tb && tb.DataContext is TaskViewModel vm)
            vm.IsEditing = true;
    }

    private void EditBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            tb.Focus();
            tb.SelectAll();
        }
    }

    private void EditBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (sender is TextBox tb && tb.DataContext is TaskViewModel vm)
        {
            if (e.Key == Key.Enter)
            {
                vm.CommitEdit(tb.Text);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                vm.CancelEdit();
                e.Handled = true;
            }
        }
    }

    private void EditBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb && tb.DataContext is TaskViewModel vm)
            vm.CommitEdit(tb.Text);
    }

    private void TaskRow_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is Border border)
        {
            border.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xF0, 0xC0));
            var actionPanel = FindVisualChild<StackPanel>(border, "ActionButtons");
            if (actionPanel != null) actionPanel.Opacity = 1;
        }
    }

    private void TaskRow_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is Border border)
        {
            border.Background = Brushes.Transparent;
            var actionPanel = FindVisualChild<StackPanel>(border, "ActionButtons");
            if (actionPanel != null) actionPanel.Opacity = 0;
        }
    }

    private static T? FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T element && element.Name == name) return element;
            var found = FindVisualChild<T>(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
