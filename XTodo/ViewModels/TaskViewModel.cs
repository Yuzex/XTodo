using System.Windows;
using System.Windows.Input;
using XTodo.Models;

namespace XTodo.ViewModels;

public class TaskViewModel : ObservableObject
{
    private readonly TaskItem _task;
    private readonly Action _onChanged;
    private readonly Action<TaskItem> _onDelete;
    private bool _isExpanded = true;
    private bool _isEditing;

    private ICommand? _toggleExpandCommand;
    private ICommand? _addChildCommand;
    private ICommand? _deleteCommand;
    private ICommand? _toggleActiveRootCommand;
    private ICommand? _beginEditCommand;

    public TaskViewModel(
        TaskItem task,
        int depth,
        Action onChanged,
        Action<TaskItem> onDelete)
    {
        _task = task;
        _onChanged = onChanged;
        _onDelete = onDelete;
        Depth = depth;
    }

    public TaskItem Task => _task;

    // ---- 基础属性 ----

    private int _depth;
    public int Depth
    {
        get => _depth;
        private set
        {
            if (SetField(ref _depth, value))
            {
                OnPropertyChanged(nameof(IndentMargin));
                OnPropertyChanged(nameof(IsRoot));
                OnPropertyChanged(nameof(CanAddChild));
                OnPropertyChanged(nameof(NameFontSize));
                OnPropertyChanged(nameof(NameFontWeight));
            }
        }
    }

    public void UpdateDepth(int newDepth) => Depth = newDepth;

    public bool IsRoot => Depth == 0;
    public Thickness IndentMargin => new(Depth * 24, 0, 0, 0);

    public double NameFontSize => Depth switch { 0 => 15, 1 => 14, _ => 13 };
    public FontWeight NameFontWeight => Depth == 0 ? FontWeights.SemiBold : FontWeights.Normal;

    // ---- 名称 ----

    public string Name
    {
        get => _task.Name;
        set
        {
            if (_task.Name != value)
            {
                _task.Name = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsEditing { get => _isEditing; set => SetField(ref _isEditing, value); }

    public ICommand BeginEditCommand =>
        _beginEditCommand ??= new RelayCommand(() => { IsEditing = true; });

    public void CommitEdit(string newName)
    {
        IsEditing = false;
        if (!string.IsNullOrWhiteSpace(newName))
            Name = newName.Trim();
        else
            OnPropertyChanged(nameof(Name));
    }

    public void CancelEdit()
    {
        IsEditing = false;
        OnPropertyChanged(nameof(Name));
    }

    // ---- 完成状态 ----

    public bool IsCompleted
    {
        get => _task.IsCompleted;
        set
        {
            if (_task.IsCompleted == value) return;
            _task.IsCompleted = value;
            OnPropertyChanged();
            if (value && HasChildren)
                IsExpanded = false;
            else
                _onChanged();
        }
    }

    // ---- 展开/折叠 ----

    public bool HasChildren => _task.Children.Count > 0;
    public int ChildCount => _task.Children.Count;

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (SetField(ref _isExpanded, value))
            {
                OnPropertyChanged(nameof(ExpandIcon));
                _onChanged();
            }
        }
    }

    public string ExpandIcon => HasChildren ? (IsExpanded ? "▼" : "▶") : "  ";

    public ICommand ToggleExpandCommand =>
        _toggleExpandCommand ??= new RelayCommand(() =>
        {
            if (HasChildren) IsExpanded = !IsExpanded;
        });

    // ---- 预计完成时间 ----

    public DateTime? EstimatedCompletionTime
    {
        get => _task.EstimatedCompletionTime;
        set { _task.EstimatedCompletionTime = value; OnPropertyChanged(); }
    }

    public string TimeDisplay =>
        EstimatedCompletionTime.HasValue
            ? EstimatedCompletionTime.Value.ToString("MM/dd HH:mm")
            : "";

    public string TimeTooltip =>
        EstimatedCompletionTime.HasValue
            ? $"预计完成：{EstimatedCompletionTime.Value:yyyy/MM/dd HH:mm}"
            : "点击设置预计完成时间";

    private ICommand? _setTimeCommand;
    public ICommand SetTimeCommand =>
        _setTimeCommand ??= new RelayCommand(() =>
        {
            var dialog = new DateTimeDialog(EstimatedCompletionTime);
            dialog.Owner = Application.Current.MainWindow;
            var result = dialog.ShowDialog();

            if (result == true)
            {
                EstimatedCompletionTime = dialog.SelectedDateTime;
                OnPropertyChanged(nameof(TimeDisplay));
                OnPropertyChanged(nameof(TimeTooltip));
                _onChanged();
            }
        });

    // ---- 当前进行中标记 ----

    public bool IsActiveRoot
    {
        get => _task.IsActiveRoot;
        set
        {
            if (_task.IsActiveRoot == value) return;
            _task.IsActiveRoot = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ActiveRootIcon));
        }
    }

    public string ActiveRootIcon => IsActiveRoot ? "★" : "☆";

    public ICommand ToggleActiveRootCommand =>
        _toggleActiveRootCommand ??= new RelayCommand(() =>
        {
            if (Depth == 0) IsActiveRoot = !IsActiveRoot;
        });

    // ---- 闭环条件 ----

    public string CompletionCriteria => _task.CompletionCriteria;

    public string CriteriaSnippet
    {
        get
        {
            var text = _task.CompletionCriteria;
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length > 20 ? text[..20] + "…" : text;
        }
    }

    public bool HasCriteria => !string.IsNullOrEmpty(_task.CompletionCriteria);

    public int CompletedChildrenCount => _task.Children.Count(c => c.IsCompleted);
    public int TotalChildrenCount => _task.Children.Count;

    public bool AreAllChildrenCompleted =>
        _task.Children.Count > 0 && _task.Children.All(c => c.IsCompleted);

    public string ChildProgressBadge
    {
        get
        {
            if (!HasChildren) return "";
            return AreAllChildrenCompleted
                ? $"✅ {TotalChildrenCount}/{TotalChildrenCount}"
                : $"☐ {CompletedChildrenCount}/{TotalChildrenCount}";
        }
    }

    public string CriteriaTooltip
    {
        get
        {
            if (!string.IsNullOrEmpty(_task.CompletionCriteria))
                return "📝 " + _task.CompletionCriteria;
            return "点击编辑闭环条件";
        }
    }

    private ICommand? _editCriteriaCommand;
    public ICommand EditCriteriaCommand =>
        _editCriteriaCommand ??= new RelayCommand(() =>
        {
            var dialog = new CriteriaEditDialog(_task);
            dialog.Owner = Application.Current.MainWindow;
            var result = dialog.ShowDialog();
            if (result == true) _onChanged();
        });

    // ---- 子任务操作 ----

    public bool CanAddChild => Depth < 2;

    public ICommand AddChildCommand =>
        _addChildCommand ??= new RelayCommand(() =>
        {
            if (!CanAddChild) return;

            var child = new TaskItem { Name = "新任务", Depth = Depth + 1 };
            _task.Children.Add(child);
            if (Application.Current.MainWindow?.DataContext is MainViewModel mvm)
                mvm.SetPendingEditTask(child.Id);
            IsExpanded = true;
            _onChanged();
        }, () => CanAddChild);

    // ---- 删除 ----

    public ICommand DeleteCommand =>
        _deleteCommand ??= new RelayCommand(() => { _onDelete(_task); });
}
