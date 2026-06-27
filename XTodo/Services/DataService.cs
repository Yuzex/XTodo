using System.IO;
using System.Text.Json;
using XTodo.Models;

namespace XTodo.Services;

/// <summary>
/// 负责 AppData ↔ JSON 文件的序列化与反序列化。
/// </summary>
public class DataService
{
    private static readonly string DataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "XTodo");

    private static readonly string DataFile = Path.Combine(DataDir, "data.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public AppData Load()
    {
        try
        {
            if (File.Exists(DataFile))
            {
                var json = File.ReadAllText(DataFile);
                var data = JsonSerializer.Deserialize<AppData>(json, JsonOptions);
                if (data != null)
                {
                    EnsureGuideCategory(data);
                    return data;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"加载数据失败: {ex.Message}");
        }

        var sampleData = CreateSampleData();
        Save(sampleData);
        return sampleData;
    }

    public void Save(AppData data)
    {
        try
        {
            if (!Directory.Exists(DataDir))
                Directory.CreateDirectory(DataDir);

            var json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(DataFile, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"保存数据失败: {ex.Message}");
        }
    }

    private static void EnsureGuideCategory(AppData data)
    {
        if (data.Categories.Any(c => c.Id == "cat-guide"))
            return;

        var guide = CreateGuideCategory();
        data.Categories.Insert(0, guide);
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
            new TaskItem { Id = "g-11", Name = "收起态窄条显示：待办数 | 当前任务 | 截止时间" }
        }
    };

    private static AppData CreateSampleData()
    {
        var guide = CreateGuideCategory();

        var work = new Category
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

        var personal = new Category
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

        return new AppData
        {
            Categories = new() { guide, work, personal },
            ActiveCategoryId = "cat-guide"
        };
    }
}
