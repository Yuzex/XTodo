using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using XTodo.ViewModels;
using WinForms = System.Windows.Forms;

namespace XTodo;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();
    private DispatcherTimer? _collapseTimer;
    private double _expandedHeight;
    private WinForms.NotifyIcon? _notifyIcon;
    private bool _isExiting;

    public MainWindow()
    {
        InitializeComponent();

        var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "XTodo.ico");
        System.Drawing.Icon? appIcon = null;
        if (System.IO.File.Exists(iconPath))
        {
            appIcon = new System.Drawing.Icon(iconPath);
            Icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                appIcon.Handle,
                System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }

        SetupTrayIcon(appIcon);

        DataContext = _viewModel;

        Loaded += (_, _) =>
        {
            LoadDataAndPosition();
            SetupAutoHide();
        };
    }

    private void SetupTrayIcon(System.Drawing.Icon? icon)
    {
        _notifyIcon = new WinForms.NotifyIcon
        {
            Icon = icon ?? System.Drawing.SystemIcons.Application,
            Text = "XTodo",
            Visible = true
        };

        var trayMenu = new WinForms.ContextMenuStrip();

        var showHideItem = new WinForms.ToolStripMenuItem("显示/隐藏");
        showHideItem.Click += (_, _) => ToggleVisibility();
        trayMenu.Items.Add(showHideItem);

        trayMenu.Items.Add(new WinForms.ToolStripSeparator());

        var exitItem = new WinForms.ToolStripMenuItem("退出");
        exitItem.Click += (_, _) =>
        {
            _isExiting = true;
            Application.Current.Shutdown();
        };
        trayMenu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = trayMenu;
        _notifyIcon.DoubleClick += (_, _) => ToggleVisibility();
    }

    private void ToggleVisibility()
    {
        if (Visibility == Visibility.Visible)
            Hide();
        else
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }
    }

    private void LoadDataAndPosition()
    {
        var workArea = SystemParameters.WorkArea;

        var savedW = _viewModel.SavedWindowWidth;
        var savedH = _viewModel.SavedWindowHeight;

        Width = savedW > 0 ? savedW : Math.Clamp(workArea.Width / 3, 300, 800);
        Height = savedH > 0 ? savedH : 500;
        _expandedHeight = Height;

        Left = (workArea.Width - Width) / 2 + workArea.Left;
        Top = workArea.Top;
    }

    private void SetupAutoHide()
    {
        SizeChanged += (_, _) =>
        {
            if (!_viewModel.IsCollapsed && Height > 100)
                _expandedHeight = Height;
        };

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.IsCollapsed))
            {
                if (_viewModel.IsCollapsed)
                    CollapseWindow();
                else
                    ExpandWindow();
            }
            else if (e.PropertyName == nameof(MainViewModel.PendingEditCategoryId) &&
                     _viewModel.PendingEditCategoryId != null)
            {
                var catId = _viewModel.PendingEditCategoryId;
                Dispatcher.BeginInvoke(new Action(() => StartCategoryEdit(catId)),
                    DispatcherPriority.Loaded);
            }
        };
    }

    private void Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _collapseTimer?.Stop();
        if (_viewModel.IsCollapsed)
            _viewModel.IsCollapsed = false;
    }

    private void Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _collapseTimer?.Stop();
        _collapseTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _collapseTimer.Tick += (_, _) =>
        {
            _collapseTimer.Stop();
            if (!_viewModel.IsCollapsed)
                _viewModel.IsCollapsed = true;
        };
        _collapseTimer.Start();
    }

    private void CollapseWindow()
    {
        if (Height > 50) _expandedHeight = Height;
        MinHeight = 0;
        Height = 40;
    }

    private void ExpandWindow()
    {
        Height = _expandedHeight > 100 ? _expandedHeight : 500;
        MinHeight = 300;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_isExiting)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        _viewModel.UpdateWindowSize(Width, _expandedHeight);
        _viewModel.Save();

        _notifyIcon!.Visible = false;
        _notifyIcon.Dispose();
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;

        _viewModel.RefreshWorkspacesList();

        var menu = new ContextMenu
        {
            FontFamily = new System.Windows.Media.FontFamily("Microsoft YaHei"),
            FontSize = 12,
            Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0xFF, 0xF8, 0xE1)),
            BorderBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0xD7, 0xCC, 0xC8)),
            BorderThickness = new Thickness(1)
        };

        // ---- 工作区区域 ----
        var workspaceHeader = new MenuItem
        {
            Header = "工作区",
            IsEnabled = false,
            Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x8D, 0x6E, 0x63)),
            FontWeight = FontWeights.Bold,
            Padding = new Thickness(12, 4, 28, 2)
        };
        menu.Items.Add(workspaceHeader);

        foreach (var ws in _viewModel.AvailableWorkspaces)
        {
            var isCurrent = ws == _viewModel.ActiveWorkspace;
            var wsItem = new MenuItem
            {
                Header = ws,
                IsCheckable = true,
                IsChecked = isCurrent,
                ToolTip = "右键打开文件夹",
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x3E, 0x27, 0x23)),
                Padding = new Thickness(28, 4, 28, 4)
            };
            var captured = ws;
            wsItem.Click += (_, _) =>
            {
                if (captured != _viewModel.ActiveWorkspace)
                    _viewModel.SwitchToWorkspace(captured);
            };
            wsItem.PreviewMouseRightButtonDown += (_, args) =>
            {
                OpenWorkspaceInExplorer(captured);
                args.Handled = true;
            };
            menu.Items.Add(wsItem);
        }

        menu.Items.Add(new Separator());

        // ---- 开机自启动 ----
        var autoStartItem = new MenuItem
        {
            Header = "开机自启动",
            IsCheckable = true,
            IsChecked = _viewModel.AutoStartEnabled,
            Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x3E, 0x27, 0x23)),
            Padding = new Thickness(12, 6, 28, 6)
        };
        autoStartItem.Click += (_, _) =>
        {
            _viewModel.AutoStartEnabled = !_viewModel.AutoStartEnabled;
            autoStartItem.IsChecked = _viewModel.AutoStartEnabled;
        };

        menu.Items.Add(autoStartItem);
        menu.IsOpen = true;
    }

    private static void OpenWorkspaceInExplorer(string workspaceName)
    {
        var dir = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "XTodo", workspaceName);
        try
        {
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            System.Diagnostics.Process.Start("explorer.exe", dir);
        }
        catch { /* 忽略 */ }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void Tab_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Models.Category cat)
        {
            StartCategoryEdit(cat.Id);
            e.Handled = true;
        }
    }

    private void StartCategoryEdit(string categoryId)
    {
        var btn = FindTabButton(categoryId);
        var cat = _viewModel.Categories.FirstOrDefault(c => c.Id == categoryId);
        if (btn == null || cat == null) return;

        var oldText = cat.Name;
        var oldTextBlock = btn.Content as TextBlock;

        var editBox = new TextBox
        {
            Text = oldText,
            FontFamily = new System.Windows.Media.FontFamily("Microsoft YaHei"),
            FontSize = 12,
            MinWidth = 40,
            MaxWidth = 120,
            Padding = new Thickness(4, 2, 4, 2),
            VerticalAlignment = VerticalAlignment.Center
        };

        var committed = false;
        void Commit()
        {
            if (committed) return;
            committed = true;
            var newName = editBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(newName))
            {
                cat.Name = newName;
                _viewModel.OnCategoryRenamed(cat, oldText);
                _viewModel.PendingEditCategoryId = null;
            }
            btn.Content = oldTextBlock;
        }

        editBox.KeyDown += (s, args) =>
        {
            if (args.Key == System.Windows.Input.Key.Enter) Commit();
            else if (args.Key == System.Windows.Input.Key.Escape) Commit();
        };
        editBox.LostFocus += (_, _) => Commit();
        editBox.Loaded += (s, _) => { ((TextBox)s).Focus(); ((TextBox)s).SelectAll(); };

        btn.Content = editBox;
    }

    private Button? FindTabButton(string categoryId)
    {
        var scrollViewer = FindVisualChild<ScrollViewer>(OuterBorder);
        if (scrollViewer == null) return null;

        return FindVisualChild<Button>(scrollViewer,
            el => el.DataContext is Models.Category cat && cat.Id == categoryId);
    }

    private static T? FindVisualChild<T>(System.Windows.DependencyObject parent,
        Func<T, bool>? predicate = null) where T : System.Windows.DependencyObject
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T found && (predicate == null || predicate(found)))
                return found;
            var result = FindVisualChild<T>(child, predicate);
            if (result != null) return result;
        }
        return null;
    }

    private static T? FindVisualChild<T>(System.Windows.DependencyObject parent) where T : System.Windows.DependencyObject
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T found) return found;
            var result = FindVisualChild<T>(child);
            if (result != null) return result;
        }
        return null;
    }

    private void Tab_RightClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement el && el.DataContext is Models.Category cat)
        {
            _viewModel.DeleteCategoryCommand.Execute(cat.Id);
            e.Handled = true;
        }
    }

    // Tab 拖拽排序
    private Models.Category? _dragCategory;
    private System.Windows.Point _dragStartPoint;
    private bool _dragging;
    private Button? _dragButton;

    private void Tab_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Models.Category cat)
        {
            _dragCategory = cat;
            _dragButton = btn;
            _dragStartPoint = e.GetPosition(null);
            _dragging = false;
        }
    }

    private void Tab_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_dragCategory == null || _dragging) return;
        var pos = e.GetPosition(null);
        if (Math.Abs(pos.X - _dragStartPoint.X) > 5 || Math.Abs(pos.Y - _dragStartPoint.Y) > 5)
        {
            _dragging = true;
            if (_dragButton != null) _dragButton.Opacity = 0.4;
        }
    }

    private void Tab_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var swapped = false;
        if (_dragging && _dragButton != null && _dragCategory != null)
        {
            _dragButton.Opacity = 1.0;

            var hitResult = System.Windows.Media.VisualTreeHelper.HitTest(this, e.GetPosition(this));
            var targetCat = FindCategoryFromHitTest(hitResult);
            if (targetCat != null && targetCat.Id != _dragCategory.Id)
            {
                _viewModel.MoveCategory(_dragCategory.Id, targetCat.Id);
                swapped = true;
            }
        }
        _dragCategory = null;
        _dragging = false;
        _dragButton = null;
        if (swapped) e.Handled = true;
    }

    private static Models.Category? FindCategoryFromHitTest(System.Windows.Media.HitTestResult? hitResult)
    {
        var current = hitResult?.VisualHit as System.Windows.DependencyObject;
        while (current != null)
        {
            if (current is FrameworkElement fe && fe.DataContext is Models.Category cat)
                return cat;
            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
