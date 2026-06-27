using System.Collections.ObjectModel;

namespace XTodo.Models;

/// <summary>
/// 旧版数据容器——仅用于 v1.0 data.json 的迁移读取，不再作为运行时数据结构。
/// </summary>
public class AppData : ObservableObject
{
    private ObservableCollection<Category> _categories = new();
    private string _activeCategoryId = string.Empty;

    public ObservableCollection<Category> Categories { get => _categories; set => SetField(ref _categories, value); }
    public string ActiveCategoryId { get => _activeCategoryId; set => SetField(ref _activeCategoryId, value); }
}
