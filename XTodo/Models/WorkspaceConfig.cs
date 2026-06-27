namespace XTodo.Models;

/// <summary>
/// workspace.json 的内容——全局配置（不属于任何工作区）。
/// </summary>
public class WorkspaceConfig : ObservableObject
{
    private string _activeWorkspace = "默认";
    private double _windowWidth = 640;
    private double _windowHeight = 500;
    private bool _autoStartEnabled;

    public string ActiveWorkspace { get => _activeWorkspace; set => SetField(ref _activeWorkspace, value); }
    public double WindowWidth { get => _windowWidth; set => SetField(ref _windowWidth, value); }
    public double WindowHeight { get => _windowHeight; set => SetField(ref _windowHeight, value); }
    public bool AutoStartEnabled { get => _autoStartEnabled; set => SetField(ref _autoStartEnabled, value); }
}
