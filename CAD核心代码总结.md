# CAD 核心代码总结

## 项目概述

BlockManager 是一个基于适配器模式的 AutoCAD 插件项目，实现了块库浏览和管理功能。项目支持多个 AutoCAD 版本，通过抽象层和具体适配器实现版本兼容。

## 核心架构

### 1. 抽象层 (BlockManager.Abstractions)

#### IBlockLibraryService 接口
```csharp
public interface IBlockLibraryService
{
    void ShowBlockLibraryViewer();           // 显示块库浏览器
    void InsertDwgBlock(string dwgPath, string blockName);  // 插入DWG块
    bool FileExists(string filePath);        // 检查文件存在性
    void ShowMessage(string message);        // 显示消息
}
```

**核心功能**：
- 定义块库服务的标准接口
- 提供版本无关的抽象合同
- 支持依赖注入和解耦设计

### 2. 共享核心层 (BlockManager.Core)

#### BlockLibraryViewer - 主界面组件
```csharp
public partial class BlockLibraryViewer : Form
{
    private string blockRootPath;
    private IBlockLibraryService _blockLibraryService;
    
    // 构造函数支持依赖注入
    public BlockLibraryViewer(IBlockLibraryService blockLibraryService)
    {
        _blockLibraryService = blockLibraryService;
        InitializeComponent();
        blockRootPath = @"c:\Users\PC\Desktop\BlockManager\BlockManager.Core\Block";
        LoadBlockLibrary();
    }
}
```

**核心功能**：
- **TreeView 文件浏览**：递归加载文件夹结构和文件
- **图片预览**：支持 PNG、JPG 等格式的实时预览
- **DWG 文件支持**：自动查找对应的 PNG 预览图
- **双击插入**：双击 DWG 文件触发块插入流程
- **状态显示**：实时显示文件信息和操作状态

**关键方法**：
```csharp
private void LoadDirectoryNodes(TreeNode parentNode, string directoryPath)
private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
private void TreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
private void InsertBlockFromDwg(string dwgPath)
```

#### BlockLibraryCommands - 命令管理
```csharp
public class BlockLibraryCommands
{
    private static IBlockLibraryService _blockLibraryService;
    
    public static void SetBlockLibraryService(IBlockLibraryService service)
    {
        _blockLibraryService = service;
    }
    
    public static void ShowBlockLibraryViewer()
    {
        if (_blockLibraryService != null)
            _blockLibraryService.ShowBlockLibraryViewer();
        else
            new BlockLibraryViewer().Show();
    }
}
```

### 3. AutoCAD 2024 适配器 (BlockManager.Adapter.2024)

#### Cad2024BlockLibraryService - 服务实现
```csharp
public class Cad2024BlockLibraryService : IBlockLibraryService
{
    public void ShowBlockLibraryViewer()
    {
        var viewer = new BlockLibraryViewer(this);
        viewer.Show();
    }
    
    public void InsertDwgBlock(string dwgPath, string blockName)
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        var ed = doc.Editor;
        
        // 路径处理和验证
        string normalizedPath = Path.GetFullPath(dwgPath);
        if (!File.Exists(normalizedPath)) return;
        
        // 使用 LISP 命令执行 INSERT
        string escapedPath = normalizedPath.Replace("\\", "\\\\");
        string commandStr = $"(command \"_INSERT\" \"{escapedPath}\" pause \"\" \"\" \"\") \n";
        doc.SendStringToExecute(commandStr, false, false, false);
    }
}
```

#### BlockInsertCommands - AutoCAD 命令
```csharp
public class BlockInsertCommands
{
    [CommandMethod("BLOCKVIEWER")]
    public void ShowBlockViewer()
    {
        // 注册事件处理
        BlockLibraryViewer.OnDwgBlockInsertRequested += HandleDwgBlockInsertRequest;
        
        // 显示浏览器
        _blockLibraryService.ShowBlockLibraryViewer();
    }
    
    private void HandleDwgBlockInsertRequest(string dwgPath, string blockName)
    {
        _blockLibraryService.InsertDwgBlock(dwgPath, blockName);
    }
}
```

## 核心技术特性

### 1. 适配器模式实现
- **抽象层**：定义统一接口
- **具体适配器**：针对不同 AutoCAD 版本的具体实现
- **版本兼容**：支持 AutoCAD 2010 和 2024

### 2. 依赖注入设计
```csharp
// 静态初始化服务
static BlockInsertCommands()
{
    _blockLibraryService = new Cad2024BlockLibraryService();
    BlockLibraryCommands.SetBlockLibraryService(_blockLibraryService);
}
```

### 3. 事件驱动架构
```csharp
// 向后兼容的事件机制
public static event Action<string, string> OnDwgBlockInsertRequested;

// 双重处理机制：服务优先，事件备用
if (_blockLibraryService != null)
    _blockLibraryService.InsertDwgBlock(dwgPath, blockName);
else if (OnDwgBlockInsertRequested != null)
    OnDwgBlockInsertRequested?.Invoke(dwgPath, blockName);
```

### 4. 文件系统集成
- **智能路径处理**：支持绝对路径和相对路径
- **文件类型识别**：根据扩展名设置不同图标
- **预览图关联**：DWG 文件自动查找对应 PNG 预览

### 5. AutoCAD 集成
```csharp
// LISP 命令执行
string commandStr = $"(command \"_INSERT\" \"{escapedPath}\" pause \"\" \"\" \"\") \n";
doc.SendStringToExecute(commandStr, false, false, false);
```

## 用户交互流程

1. **启动**：用户执行 `BLOCKVIEWER` 命令
2. **浏览**：TreeView 显示块库文件结构
3. **预览**：选择文件显示预览图和信息
4. **插入**：双击 DWG 文件启动插入流程
5. **执行**：AutoCAD 执行 INSERT 命令

## 错误处理机制

- **异常捕获**：所有关键操作都有 try-catch 保护
- **状态反馈**：通过状态栏实时显示操作结果
- **优雅降级**：服务不可用时回退到基础功能
- **用户提示**：通过 MessageBox 和编辑器消息提供反馈

## 扩展性设计

- **接口抽象**：易于添加新的 AutoCAD 版本支持
- **插件化架构**：核心逻辑与 CAD 版本解耦
- **配置化路径**：支持自定义块库路径
- **事件机制**：支持外部扩展和集成

## 部署结构

```
BlockManager/
├── BlockManager.Abstractions/     # 抽象接口层
├── BlockManager.Core/             # 共享核心逻辑
├── BlockManager.Adapter.2010/     # AutoCAD 2010 适配器
├── BlockManager.Adapter.2024/     # AutoCAD 2024 适配器
└── Block/                         # 块库文件存储目录
```

这个架构确保了代码的可维护性、可扩展性和版本兼容性，是一个典型的企业级 AutoCAD 插件解决方案。
