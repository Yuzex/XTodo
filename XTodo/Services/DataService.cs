using System.IO;
using System.Text.Json;
using XTodo.Models;

namespace XTodo.Services;

/// <summary>
/// v2.0 工作区感知的数据存取层。
/// 一个工作区 = 一个文件夹，一个 Tab = 一个 .json 文件。
/// </summary>
public class DataService
{
    private static readonly string RootDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "XTodo");

    private static readonly string ConfigFile = Path.Combine(RootDir, "workspace.json");
    private static readonly string LegacyFile = Path.Combine(RootDir, "data.json");
    private const string DefaultWorkspace = "默认";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    // ============ 配置 ============

    public WorkspaceConfig LoadConfig()
    {
        try
        {
            if (File.Exists(ConfigFile))
            {
                var json = File.ReadAllText(ConfigFile);
                var config = JsonSerializer.Deserialize<WorkspaceConfig>(json, JsonOptions);
                if (config != null)
                    return config;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"加载工作区配置失败: {ex.Message}");
        }

        return new WorkspaceConfig();
    }

    public void SaveConfig(WorkspaceConfig config)
    {
        try
        {
            if (!Directory.Exists(RootDir))
                Directory.CreateDirectory(RootDir);

            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(ConfigFile, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"保存工作区配置失败: {ex.Message}");
        }
    }

    // ============ 工作区枚举 ============

    public List<string> ListWorkspaces()
    {
        var workspaces = new List<string>();

        try
        {
            if (Directory.Exists(RootDir))
            {
                foreach (var dir in Directory.GetDirectories(RootDir))
                {
                    var name = Path.GetFileName(dir);
                    if (!string.IsNullOrEmpty(name))
                        workspaces.Add(name);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"枚举工作区失败: {ex.Message}");
        }

        // 确保"默认"存在且排第一
        if (!workspaces.Contains(DefaultWorkspace))
        {
            var defaultPath = Path.Combine(RootDir, DefaultWorkspace);
            try { Directory.CreateDirectory(defaultPath); }
            catch { /* 忽略创建失败 */ }
            workspaces.Insert(0, DefaultWorkspace);
        }
        else
        {
            workspaces.Remove(DefaultWorkspace);
            workspaces.Insert(0, DefaultWorkspace);
        }

        // 其余按字母排序
        var tail = workspaces.Skip(1).OrderBy(w => w).ToList();
        return new List<string> { DefaultWorkspace }.Concat(tail).ToList();
    }

    // ============ 工作区读写 ============

    public List<Category> LoadWorkspace(string workspaceName)
    {
        var categories = new List<Category>();
        var dir = Path.Combine(RootDir, workspaceName);

        try
        {
            if (!Directory.Exists(dir))
                return categories;

            foreach (var file in Directory.GetFiles(dir, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var cat = JsonSerializer.Deserialize<Category>(json, JsonOptions);
                    if (cat != null)
                        categories.Add(cat);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"加载分类失败 ({file}): {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"加载工作区失败 ({workspaceName}): {ex.Message}");
        }

        return categories;
    }

    public void SaveCategory(string workspaceName, Category category)
    {
        try
        {
            var dir = Path.Combine(RootDir, workspaceName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var fileName = SanitizeFileName(category.Name) + ".json";
            var filePath = Path.Combine(dir, fileName);

            var json = JsonSerializer.Serialize(category, JsonOptions);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"保存分类失败 ({category.Name}): {ex.Message}");
        }
    }

    public void DeleteCategoryFile(string workspaceName, string categoryName)
    {
        try
        {
            var fileName = SanitizeFileName(categoryName) + ".json";
            var filePath = Path.Combine(RootDir, workspaceName, fileName);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"删除分类文件失败 ({categoryName}): {ex.Message}");
        }
    }

    public Category? LoadCategory(string workspaceName, string categoryName)
    {
        try
        {
            var fileName = SanitizeFileName(categoryName) + ".json";
            var filePath = Path.Combine(RootDir, workspaceName, fileName);
            if (!File.Exists(filePath))
                return null;

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Category>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"加载分类失败 ({categoryName}): {ex.Message}");
            return null;
        }
    }

    // ============ 文件名处理 ============

    public static string SanitizeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "未命名";

        var invalid = Path.GetInvalidFileNameChars();
        var chars = name.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (invalid.Contains(chars[i]))
                chars[i] = '_';
        }

        var result = new string(chars).Trim();
        return string.IsNullOrWhiteSpace(result) ? "未命名" : result;
    }

    // ============ 迁移 ============

    public bool NeedsMigration()
    {
        return File.Exists(LegacyFile) && !File.Exists(ConfigFile);
    }

    public void Migrate()
    {
        try
        {
            var json = File.ReadAllText(LegacyFile);
            var legacy = JsonSerializer.Deserialize<AppData>(json, JsonOptions);
            if (legacy == null) return;

            // 确保默认工作区目录存在
            var defaultDir = Path.Combine(RootDir, DefaultWorkspace);
            if (!Directory.Exists(defaultDir))
                Directory.CreateDirectory(defaultDir);

            // 拆分为独立文件
            foreach (var cat in legacy.Categories)
            {
                SaveCategory(DefaultWorkspace, cat);
            }

            // 创建 workspace.json
            var config = new WorkspaceConfig
            {
                ActiveWorkspace = DefaultWorkspace
            };
            SaveConfig(config);

            // 旧文件备份
            var bakPath = LegacyFile + ".bak";
            if (File.Exists(bakPath))
                File.Delete(bakPath);
            File.Move(LegacyFile, bakPath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"数据迁移失败: {ex.Message}");
        }
    }

    // ============ 示例数据 ============

    /// <summary>确保"默认"工作区有操作指南分类。如果没有则免打扰创建。</summary>
    public static void EnsureGuideExists()
    {
        var defaultDir = Path.Combine(RootDir, DefaultWorkspace);
        var guideFile = Path.Combine(defaultDir, SanitizeFileName("操作指南") + ".json");

        if (File.Exists(guideFile))
            return;

        try
        {
            if (!Directory.Exists(defaultDir))
                Directory.CreateDirectory(defaultDir);

            var guide = CreateGuideCategory();
            var json = JsonSerializer.Serialize(guide, JsonOptions);
            File.WriteAllText(guideFile, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"创建操作指南失败: {ex.Message}");
        }
    }

    /// <summary>创建完整的示例数据（仅首次使用）。</summary>
    public static void CreateSampleDataIfNeeded()
    {
        var defaultDir = Path.Combine(RootDir, DefaultWorkspace);

        // 已有数据则跳过
        if (Directory.Exists(defaultDir) && Directory.GetFiles(defaultDir, "*.json").Length > 0)
            return;

        try
        {
            if (!Directory.Exists(defaultDir))
                Directory.CreateDirectory(defaultDir);

            var guide = CreateGuideCategory();
            var work = CreateWorkCategory();
            var personal = CreatePersonalCategory();

            var opts = JsonOptions;

            File.WriteAllText(
                Path.Combine(defaultDir, SanitizeFileName(guide.Name) + ".json"),
                JsonSerializer.Serialize(guide, opts));
            File.WriteAllText(
                Path.Combine(defaultDir, SanitizeFileName(work.Name) + ".json"),
                JsonSerializer.Serialize(work, opts));
            File.WriteAllText(
                Path.Combine(defaultDir, SanitizeFileName(personal.Name) + ".json"),
                JsonSerializer.Serialize(personal, opts));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"创建示例数据失败: {ex.Message}");
        }
    }

    private static Category CreateGuideCategory() => new()
    {
        Id = "cat-guide",
        Name = "操作指南",
        RootTasks = new()
        {
            new TaskItem { Id = "g-1", Name = "双击任务名 → 进入编辑，修改任务名称", IsCompleted = true },
            new TaskItem { Id = "g-2", Name = "点击勾选框 → 标记任务完成 / 取消完成" },
            new TaskItem
            {
                Id = "g-3", Name = "鼠标悬停任务 → 右侧出现 ＋（添加子任务）和 ✕（删除）",
                Children = new()
                {
                    new TaskItem
                    {
                        Id = "g-3-1", Name = "这是子任务（第 2 层）", Depth = 1,
                        Children = new()
                        {
                            new TaskItem { Id = "g-3-1-1", Name = "这是孙任务（第 3 层，最深）", Depth = 2 }
                        }
                    }
                }
            },
            new TaskItem { Id = "g-4", Name = "点击星标 → 标记为「当前进行中」根任务", IsActiveRoot = true },
            new TaskItem
            {
                Id = "g-5", Name = "点击时间按钮 → 设置预计完成时间",
                EstimatedCompletionTime = DateTime.Today.AddDays(3)
            },
            new TaskItem
            {
                Id = "g-6", Name = "点击编辑按钮 → 编辑闭环条件（完成标准描述）",
                CompletionCriteria = "示例：首页加载时间 < 2 秒即为完成"
            },
            new TaskItem { Id = "g-7", Name = "顶部 Tab：左键切换 · 双击重命名 · 右键删除" },
            new TaskItem { Id = "g-8", Name = "添加分类 / 添加任务 → 直接创建，内联编辑" },
            new TaskItem { Id = "g-9", Name = "鼠标移出窗口 → 自动收起为窄条，移入恢复" },
            new TaskItem { Id = "g-10", Name = "展开态可拖拽窗口右下角调整大小" },
            new TaskItem { Id = "g-11", Name = "⚙ 设置 → 切换工作区，归档整理分类" }
        }
    };

    private static Category CreateWorkCategory() => new()
    {
        Id = "cat-work",
        Name = "工作",
        RootTasks = new()
        {
            new TaskItem
            {
                Id = "t-1",
                Name = "优化首页加载速度",
                CompletionCriteria = "首页加载时间 < 2 秒",
                EstimatedCompletionTime = DateTime.Today.AddDays(1),
                SubConditions = new()
                {
                    new SubCondition { Id = "sc-1a", Description = "定位性能瓶颈" },
                    new SubCondition { Id = "sc-1b", Description = "实施优化方案" },
                    new SubCondition { Id = "sc-1c", Description = "上线验证通过" }
                },
                Children = new()
                {
                    new TaskItem
                    {
                        Id = "t-1-1",
                        Name = "分析前端打包体积",
                        Depth = 1,
                        Children = new()
                        {
                            new TaskItem { Id = "t-1-1-1", Name = "检查未使用的依赖", Depth = 2 }
                        }
                    }
                }
            },
            new TaskItem
            {
                Id = "t-2",
                Name = "完成 Q1 项目报告",
                EstimatedCompletionTime = DateTime.Today.AddDays(3)
            }
        }
    };

    private static Category CreatePersonalCategory() => new()
    {
        Id = "cat-personal",
        Name = "个人",
        RootTasks = new()
        {
            new TaskItem
            {
                Id = "t-3",
                Name = "阅读《深入理解计算机系统》",
                CompletionCriteria = "读完第 1-3 章并做笔记"
            },
            new TaskItem
            {
                Id = "t-4",
                Name = "每日运动 30 分钟",
                EstimatedCompletionTime = DateTime.Today,
                SubConditions = new()
                {
                    new SubCondition { Id = "sc-4a", Description = "热身 5 分钟" },
                    new SubCondition { Id = "sc-4b", Description = "有氧 20 分钟" },
                    new SubCondition { Id = "sc-4c", Description = "拉伸 5 分钟" }
                }
            }
        }
    };
}
