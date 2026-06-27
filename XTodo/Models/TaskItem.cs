using System.Collections.ObjectModel;

namespace XTodo.Models;

public class TaskItem : ObservableObject
{
    private string _id = Guid.NewGuid().ToString("N")[..8];
    private string _name = "新任务";
    private bool _isCompleted;
    private ObservableCollection<TaskItem> _children = new();
    private ObservableCollection<SubCondition> _subConditions = new();
    private string _completionCriteria = string.Empty;
    private DateTime? _estimatedCompletionTime;
    private int _depth;
    private bool _isActiveRoot;

    public string Id { get => _id; set => SetField(ref _id, value); }
    public string Name { get => _name; set => SetField(ref _name, value); }

    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            if (SetField(ref _isCompleted, value))
            {
                if (value)
                {
                    foreach (var sc in SubConditions)
                        sc.IsChecked = true;
                    CompleteAllChildren();
                }
            }
        }
    }

    private void CompleteAllChildren()
    {
        foreach (var child in Children)
            child.IsCompleted = true;
    }

    public ObservableCollection<TaskItem> Children { get => _children; set => SetField(ref _children, value); }
    public ObservableCollection<SubCondition> SubConditions { get => _subConditions; set => SetField(ref _subConditions, value); }
    public string CompletionCriteria { get => _completionCriteria; set => SetField(ref _completionCriteria, value); }
    public DateTime? EstimatedCompletionTime { get => _estimatedCompletionTime; set => SetField(ref _estimatedCompletionTime, value); }
    public int Depth { get => _depth; set => SetField(ref _depth, value); }
    public bool IsActiveRoot { get => _isActiveRoot; set => SetField(ref _isActiveRoot, value); }
}
