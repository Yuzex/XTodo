namespace XTodo.Models;

public class SubCondition : ObservableObject
{
    private string _id = Guid.NewGuid().ToString("N")[..8];
    private string _description = string.Empty;
    private bool _isChecked;

    public string Id { get => _id; set => SetField(ref _id, value); }
    public string Description { get => _description; set => SetField(ref _description, value); }
    public bool IsChecked { get => _isChecked; set => SetField(ref _isChecked, value); }
}
