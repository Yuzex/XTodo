using System.Collections.ObjectModel;

namespace XTodo.Models;

public class Category : ObservableObject
{
    private string _id = Guid.NewGuid().ToString("N")[..8];
    private string _name = "新分类";
    private ObservableCollection<TaskItem> _rootTasks = new();

    public string Id { get => _id; set => SetField(ref _id, value); }
    public string Name { get => _name; set => SetField(ref _name, value); }
    public ObservableCollection<TaskItem> RootTasks { get => _rootTasks; set => SetField(ref _rootTasks, value); }
}
