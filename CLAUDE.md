# CLAUDE.md — XTodo 项目 AI 助手指引

> 本文件是 AI 助手（Claude Code）的工作入口。每次新会话开始时，优先参考本文件。

---

## 项目简介

为 Windows 用户开发一个悬浮 Todo List 桌面应用。详情见需求文档。

**技术栈**：.NET 10 + WPF + MVVM
**目标用户**：非技术背景
**开发原则**：分阶段渐进式，每阶段产出可运行版本

---

## 标准文件索引

每次工作前，**必须按需阅读以下文件**：

| 优先级 | 文件路径 | 用途 | 何时读 |
|--------|----------|------|--------|
| ⭐ | `docs/development-phases.md` | 分阶段开发计划、进度跟踪 | **每次会话必须** |
| ⭐ | `devlog/` 目录 | 最近开发日志（按日期排序取最新） | **每次会话必须** |
| 📖 | `docs/requirements.md` | 用户需求（非技术语言） | 需求不明确时 |
| 📖 | `docs/technical-spec.md` | 技术架构、代码结构、关键实现 | 技术决策时 |
| 📖 | `docs/design-spec.md` | UI 配色、字体、间距、交互规范 | UI 相关改动时 |

---

## 标准工作流程

```
每次会话：
  1. 读取 devlog/ 下最新日志（了解上次做到哪了）
  2. 读取 docs/development-phases.md（确认当前 Phase）
  3. 按当前 Phase 的任务清单执行
  4. Phase 完成后更新 development-phases.md 的状态标记
  5. 会话结束前在 devlog/ 写入当日日志
```

---

## 开发原则（强制执行）

1. **一次只做一个 Phase**：不跨阶段开发
2. **每个 Phase 结束时必须可运行**：`dotnet run` 必须成功
3. **改动最小化**：每次编辑只服务于当前 Phase 的目标
4. **先读再写**：修改任何文件前，先 Read 确认当前内容
5. **遇到设计疑问**：先查 `docs/design-spec.md` 和 `docs/technical-spec.md`，无答案再问用户
6. **提交信息用中文**：便于用户理解

---

## 代码约定

### C# 命名
- 类名：PascalCase（`TaskItem`，`DataService`）
- 方法：PascalCase（`SaveData`，`LoadData`）
- 属性：PascalCase（`IsCompleted`，`TaskName`）
- 私有字段：`_camelCase`（`_dataService`，`_isExpanded`）
- 接口：`IPascalCase`（`INotifyPropertyChanged`）

### XAML 命名
- UserControl 文件：`XxxView.xaml`（`TaskTreeView.xaml`）
- ViewModel 绑定：`{Binding PropertyName}`
- 资源 Key：PascalCase（`PrimaryBackgroundBrush`）

### 文件组织
- 所有源码在 `XTodo/` 项目文件夹下
- Model 放 `Models/`，ViewModel 放 `ViewModels/`，View 放 `Views/`
- 一个文件一个类（除了简单的关联类如 `SubCondition`）

---

## 环境信息

- **项目根目录**：`d:\source\aiRepos\XTodo\`
- **.NET 项目目录**：`d:\source\aiRepos\XTodo\XTodo\`
- **操作系统**：Windows 11 Pro
- **终端**：PowerShell 5.1

---

## 当前状态

- **Phase 1** ✅ 已完成（2026-06-26）
- **Phase 2** ✅ 已完成（2026-06-26）
- **Phase 3** ✅ 已完成（2026-06-26）
- **Phase 4** ✅ 已完成（2026-06-26）
- **Phase 5** ✅ 已完成（2026-06-26）
- **Phase 6** ✅ 已完成（2026-06-26）
- **Phase 7** ✅ 已完成（2026-06-26）
- **Phase 8** ✅ 已完成（2026-06-26）
- **Phase 9** ✅ 已完成（2026-06-26）
- **Phase 10** ✅ 已完成（2026-06-27）— 工作区存档系统
- **最后的 devlog**：`devlog/2026-06-26.md`
