# XTodo — 技术规格说明

> 本文档描述项目的技术选型、架构设计和关键实现细节。

---

## 1. 技术选型

| 决策项 | 选择 | 理由 |
|--------|------|------|
| 运行时 | .NET 10.0 | 使用已安装的 SDK 版本，WPF 支持完善 |
| UI 框架 | WPF (Windows Presentation Foundation) | 原生支持 TopMost、无边框窗口、透明度、动画、数据绑定，适合桌面悬浮面板 |
| 架构模式 | MVVM (Model-View-ViewModel) | WPF 的事实标准，数据驱动 UI 更新，解耦视图与逻辑 |
| 数据存储 | 本地 JSON 文件 | 用户不需要安装数据库，文件易于备份和迁移 |
| JSON 库 | System.Text.Json | .NET 内置，零依赖，性能足够 |
| 发布方式 | Self-contained publish | 用户无需安装 .NET 运行时，单文件夹即用 |

---

## 2. 项目结构

```
XTodo/                        ← .NET 项目文件夹
├── XTodo.csproj              ← 项目文件（依赖、发布配置）
├── App.xaml / App.xaml.cs           ← 应用程序入口
├── Models/                          ← 数据模型（纯数据，无逻辑）
│   ├── TaskItem.cs                  ← 单条任务
│   ├── SubCondition.cs              ← 闭环条件子检查项
│   ├── Category.cs                  ← 任务分类
│   └── AppData.cs                   ← 顶层数据容器
├── Services/                        ← 业务逻辑服务
│   └── DataService.cs               ← JSON 文件读写
├── ViewModels/                      ← 视图模型（连接 Model 和 View）
│   ├── ObservableObject.cs          ← INotifyPropertyChanged 基类
│   ├── TaskViewModel.cs             ← 任务的 VM（状态切换、层级操作）
│   ├── CategoryViewModel.cs         ← 分类的 VM
│   └── MainViewModel.cs             ← 主窗口 VM（全局状态）
├── Views/                           ← XAML 视图
│   ├── MainWindow.xaml/.cs          ← 主窗口
│   ├── CollapsedBar.xaml/.cs        ← 收起态窄条控件
│   ├── TaskTreeView.xaml/.cs        ← 任务树控件
│   └── TaskEditPanel.xaml/.cs       ← 任务编辑面板
├── Converters/                      ← 值转换器
│   ├── BoolToTextDecorationConverter.cs  ← 完成 → 删除线
│   ├── BoolToForegroundConverter.cs      ← 完成 → 灰色
│   └── InverseBoolConverter.cs           ← 布尔取反
└── Behaviors/                       ← 行为附加
    └── AutoHideBehavior.cs          ← 鼠标移入移出展开收起
```

---

## 3. 数据流

```
用户操作 (View)
    │ 数据绑定 (TwoWay)
    ▼
ViewModel (INotifyPropertyChanged)
    │ 方法调用
    ▼
Model (纯数据对象)
    │
    ▼
DataService (序列化/反序列化)
    │
    ▼
JSON 文件 (%APPDATA%\XTodo\data.json)
```

- **数据加载**：App 启动 → DataService.Load() → MainViewModel 初始化 → View 绑定更新
- **数据保存**：ViewModel 属性变更 → 触发 SaveCommand → DataService.Save() → 写入 JSON
- **自动保存**：每次数据变更后延迟 500ms 自动保存（防抖），窗口关闭时强制保存

---

## 4. 关键技术点

### 4.1 无边框悬浮窗口

```csharp
// MainWindow 关键属性设置
WindowStyle = "None"                // 无标题栏
AllowsTransparency = "True"         // 允许透明
Topmost = "True"                    // 置顶
Background = "Transparent"          // 窗口背景透明（面板自己画背景）
ResizeMode = "CanResizeWithGrip"    // 可拖拽调整大小
```

### 4.2 屏幕顶部居中 + 宽度自适应

```csharp
// 计算位置：取工作区宽度，设窗口为 1/3
var workArea = SystemParameters.WorkArea;
Width = workArea.Width / 3;
Left = (workArea.Width - Width) / 2;
Top = 0;
```

### 4.3 自动隐藏

- 使用 `MouseEnter` / `MouseLeave` 事件
- 收起态：设置窗口高度为 ~40px，隐藏主面板，显示窄条
- 展开态：恢复原始高度，显示主面板
- 可选 Storyboard 动画平滑过渡

### 4.4 MVVM 命令绑定

- 使用 `RelayCommand`（或 CommunityToolkit.Mvvm 的 `RelayCommand`）
- 增删改操作通过 Command 触发

---

## 5. JSON 数据格式

```json
{
  "categories": [
    {
      "id": "guid",
      "name": "工作",
      "rootTasks": [
        {
          "id": "guid",
          "name": "任务名",
          "isCompleted": false,
          "isActiveRoot": false,
          "completionCriteria": "完成标准描述",
          "subConditions": [
            { "id": "guid", "description": "子条件1", "isChecked": false }
          ],
          "estimatedCompletionTime": "2026-06-27T18:00:00",
          "children": [ ... ]
        }
      ]
    }
  ],
  "activeCategoryId": "guid",
  "windowWidth": 500,
  "windowHeight": 600
}
```

---

## 6. 发布配置

```xml
<!-- XTodo.csproj 关键配置 -->
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <TargetFramework>net8.0-windows</TargetFramework>
  <UseWPF>true</UseWPF>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <PublishSingleFile>false</PublishSingleFile>
</PropertyGroup>
```

发布命令：
```powershell
dotnet publish -c Release -o ./publish
```
