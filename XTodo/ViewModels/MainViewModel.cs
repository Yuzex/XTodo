using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using XTodo.Models;
using XTodo.Services;

namespace XTodo.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly DataService _dataService = new();
    private AppData _appData = null!;
    private Category? _currentCategory;
    private ObservableCollection<TaskViewModel> _displayTasks = new();
    private readonly Dictionary<string, TaskViewModel> _vmCache = new();

    public MainViewModel()
    {
        _appData = _dataService.Load();

        var actualAutoStart = StartupService.IsAutoStartEnabled();
        if (_appData.AutoStartEnabled != actualAutoStart)
            _appData.AutoStartEnabled = actualAutoStart;

        SwitchToCategory(_appData.ActiveCategoryId);
    }

    // ---- 公开属性 ----

    public ObservableCollection<TaskViewModel> DisplayTasks
    {
        get => _displayTasks;
        set => SetField(ref _displayTasks, value);
    }

    public Category? CurrentCategory
    {
        get => _currentCategory;
        set => SetField(ref _currentCategory, value);
    }

    public ObservableCollection<Category> Categories => _appData.Categories;

    public string WindowTitle => CurrentCategory != null
        ? $"XTodo — {CurrentCategory.Name}"
        : "XTodo";

    // ---- 分类操作 ----

    public void SwitchToCategory(string categoryId)
    {
        var cat = _appData.Categories.FirstOrDefault(c => c.Id == categoryId);
        if (cat == null && _appData.Categories.Count > 0)
            cat = _appData.Categories[0];

        CurrentCategory = cat;
        _appData.ActiveCategoryId = cat?.Id ?? "";
        _vmCache.Clear();
        if (cat != null) RebuildFlatList();
    }

    // ---- 任务列表 ----

    public void RebuildFlatList()
    {
        var flat = new ObservableCollection<TaskViewModel>();

        if (CurrentCategory != null)
        {
            foreach (var rootTask in CurrentCategory.RootTasks)
                AddTaskAndChildren(flat, rootTask, depth: 0);
        }

        DisplayTasks = flat;
        OnPropertyChanged(nameof(WindowTitle));
        RefreshCollapsedBarInfo();
    }

    private void AddTaskAndChildren(ObservableCollection<TaskViewModel> flat, TaskItem task, int depth)
    {
        if (!_vmCache.TryGetValue(task.Id, out var vm))
        {
            vm = new TaskViewModel(task, depth, OnTaskChanged, OnTaskDelete);
            _vmCache[task.Id] = vm;
        }
        else
        {
            vm.UpdateDepth(depth);
        }

        if (task.Id == _pendingEditTaskId)
        {
            vm.IsEditing = true;
            _pendingEditTaskId = null;
        }

        flat.Add(vm);

        if (vm.IsExpanded)
        {
            foreach (var child in task.Children)
                AddTaskAndChildren(flat, child, depth + 1);
        }
    }

    private void OnTaskChanged() => RebuildFlatList();

    private void OnTaskDelete(TaskItem target)
    {
        RemoveTask(target);
        RemoveFromCacheRecursive(target);
        RebuildFlatList();
    }

    private void RemoveFromCacheRecursive(TaskItem task)
    {
        _vmCache.Remove(task.Id);
        foreach (var child in task.Children)
            RemoveFromCacheRecursive(child);
    }

    private bool RemoveTask(TaskItem target)
    {
        if (CurrentCategory == null) return false;
        if (CurrentCategory.RootTasks.Remove(target)) return true;
        foreach (var root in CurrentCategory.RootTasks)
        {
            if (RemoveFromChildren(root, target)) return true;
        }
        return false;
    }

    private static bool RemoveFromChildren(TaskItem parent, TaskItem target)
    {
        if (parent.Children.Remove(target)) return true;
        foreach (var child in parent.Children)
        {
            if (RemoveFromChildren(child, target)) return true;
        }
        return false;
    }

    // ---- 分类排序 ----

    public void MoveCategory(string sourceId, string targetId)
    {
        var srcIdx = _appData.Categories.IndexOf(
            _appData.Categories.FirstOrDefault(c => c.Id == sourceId)!);
        var tgtIdx = _appData.Categories.IndexOf(
            _appData.Categories.FirstOrDefault(c => c.Id == targetId)!);
        if (srcIdx < 0 || tgtIdx < 0 || srcIdx == tgtIdx) return;
        _appData.Categories.Move(srcIdx, tgtIdx);
    }

    // ---- 分类操作命令 ----

    private ICommand? _switchCategoryCommand;
    public ICommand SwitchCategoryCommand =>
        _switchCategoryCommand ??= new RelayCommand<string?>(categoryId =>
        {
            if (!string.IsNullOrEmpty(categoryId))
                SwitchToCategory(categoryId);
        });

    private string? _pendingEditCategoryId;
    public string? PendingEditCategoryId
    {
        get => _pendingEditCategoryId;
        set => SetField(ref _pendingEditCategoryId, value);
    }

    private ICommand? _addCategoryCommand;
    public ICommand AddCategoryCommand =>
        _addCategoryCommand ??= new RelayCommand(() =>
        {
            var cat = new Category { Name = "新分类" };
            _appData.Categories.Add(cat);
            SwitchToCategory(cat.Id);
            PendingEditCategoryId = cat.Id;
        });

    private ICommand? _renameCategoryCommand;
    public ICommand RenameCategoryCommand =>
        _renameCategoryCommand ??= new RelayCommand<string?>(categoryId =>
        {
            if (string.IsNullOrEmpty(categoryId)) return;
            var cat = _appData.Categories.FirstOrDefault(c => c.Id == categoryId);
            if (cat == null) return;
            var name = ShowInputDialog("重命名分类", $"新名称（当前：{cat.Name}）：");
            if (string.IsNullOrWhiteSpace(name)) return;
            cat.Name = name.Trim();
            OnPropertyChanged(nameof(Categories));
        });

    private ICommand? _deleteCategoryCommand;
    public ICommand DeleteCategoryCommand =>
        _deleteCategoryCommand ??= new RelayCommand<string?>(categoryId =>
        {
            if (string.IsNullOrEmpty(categoryId)) return;
            if (_appData.Categories.Count <= 1)
            {
                MessageBox.Show("至少保留一个分类。", "无法删除",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var cat = _appData.Categories.FirstOrDefault(c => c.Id == categoryId);
            if (cat == null) return;
            var result = MessageBox.Show(
                $"确定删除分类「{cat.Name}」吗？\n该分类下的所有任务都会被删除。",
                "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            _appData.Categories.Remove(cat);
            if (CurrentCategory?.Id == categoryId)
                SwitchToCategory(_appData.Categories[0].Id);
            else
                OnPropertyChanged(nameof(Categories));
        });

    // ---- 添加根任务 ----

    private string? _pendingEditTaskId;

    public void SetPendingEditTask(string taskId) => _pendingEditTaskId = taskId;

    private ICommand? _addRootTaskCommand;
    public ICommand AddRootTaskCommand =>
        _addRootTaskCommand ??= new RelayCommand(() =>
        {
            if (CurrentCategory == null) return;
            var newTask = new TaskItem { Name = "新任务", Depth = 0 };
            CurrentCategory.RootTasks.Add(newTask);
            _pendingEditTaskId = newTask.Id;
            RebuildFlatList();
        });

    public static string? ShowInputDialog(string title, string prompt)
    {
        var dialog = new InputDialog(title, prompt);
        dialog.Owner = Application.Current.MainWindow;
        return dialog.ShowDialog() == true ? dialog.Answer : null;
    }

    // ---- 开机自启动 ----

    public bool AutoStartEnabled
    {
        get => _appData.AutoStartEnabled;
        set
        {
            if (_appData.AutoStartEnabled == value) return;
            _appData.AutoStartEnabled = value;
            StartupService.SetAutoStart(value);
            OnPropertyChanged();
        }
    }

    private ICommand? _toggleAutoStartCommand;
    public ICommand ToggleAutoStartCommand =>
        _toggleAutoStartCommand ??= new RelayCommand(() => { AutoStartEnabled = !AutoStartEnabled; });

    // ---- 收起态信息 ----

    private bool _isCollapsed;
    public bool IsCollapsed
    {
        get => _isCollapsed;
        set
        {
            if (SetField(ref _isCollapsed, value))
                RefreshCollapsedBarInfo();
        }
    }

    public int PendingCount =>
        CurrentCategory?.RootTasks.Sum(rt => CountIncomplete(rt)) ?? 0;

    public TaskItem? CurrentRootTask
    {
        get
        {
            if (CurrentCategory == null) return null;
            var starred = CurrentCategory.RootTasks.FirstOrDefault(t => t.IsActiveRoot && !t.IsCompleted);
            if (starred != null) return starred;
            var urgent = CurrentCategory.RootTasks
                .Where(t => !t.IsCompleted && t.EstimatedCompletionTime.HasValue)
                .OrderBy(t => t.EstimatedCompletionTime)
                .FirstOrDefault();
            if (urgent != null) return urgent;
            return CurrentCategory.RootTasks.FirstOrDefault(t => !t.IsCompleted);
        }
    }

    public string CurrentRootTaskName => CurrentRootTask?.Name ?? "无待办";
    public string CurrentRootTaskTime =>
        CurrentRootTask?.EstimatedCompletionTime.HasValue == true
            ? CurrentRootTask.EstimatedCompletionTime.Value.ToString("MM/dd")
            : "";
    public bool HasCurrentRootTaskTime => !string.IsNullOrEmpty(CurrentRootTaskTime);

    public void RefreshCollapsedBarInfo()
    {
        OnPropertyChanged(nameof(PendingCount));
        OnPropertyChanged(nameof(CurrentRootTaskName));
        OnPropertyChanged(nameof(CurrentRootTaskTime));
        OnPropertyChanged(nameof(HasCurrentRootTaskTime));
    }

    private static int CountIncomplete(TaskItem task)
    {
        int count = task.IsCompleted ? 0 : 1;
        foreach (var child in task.Children)
            count += CountIncomplete(child);
        return count;
    }

    // ---- 数据持久化 ----

    public void Save() => _dataService.Save(_appData);

    private ICommand? _saveCommand;
    public ICommand SaveCommand => _saveCommand ??= new RelayCommand(() => Save());

    public double SavedWindowWidth => _appData.WindowWidth;
    public double SavedWindowHeight => _appData.WindowHeight;

    public void UpdateWindowSize(double width, double height)
    {
        _appData.WindowWidth = width;
        _appData.WindowHeight = height;
    }
}
