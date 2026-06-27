using System.Collections.ObjectModel;

namespace XTodo.Models;

public class AppData : ObservableObject
{
    private ObservableCollection<Category> _categories = new();
    private string _activeCategoryId = string.Empty;
    private double _windowWidth = 640;
    private double _windowHeight = 500;
    private bool _autoStartEnabled;

    public ObservableCollection<Category> Categories { get => _categories; set => SetField(ref _categories, value); }
    public string ActiveCategoryId { get => _activeCategoryId; set => SetField(ref _activeCategoryId, value); }
    public double WindowWidth { get => _windowWidth; set => SetField(ref _windowWidth, value); }
    public double WindowHeight { get => _windowHeight; set => SetField(ref _windowHeight, value); }
    public bool AutoStartEnabled { get => _autoStartEnabled; set => SetField(ref _autoStartEnabled, value); }
}
