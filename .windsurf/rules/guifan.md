---
trigger: manual
---

# WinForms 开发规范

本文档为 AI 助手（如 Cursor）提供 WinForms 开发的通用规范和最佳实践。

---

## 1. 命名规范

### 1.1 控件命名

**必须使用标准前缀 + 描述性名称**

| 控件类型 | 前缀 | 示例 |
|---------|------|------|
| Button | btn | `btnSave`, `btnCancel`, `btnCalculate` |
| TextBox | txt | `txtUserName`, `txtPassword`, `txtAmount` |
| Label | lbl | `lblTitle`, `lblResult`, `lblStatus` |
| ComboBox | cbo | `cboLocomotiveType`, `cboVehicleType` |
| ListBox | lst | `lstItems`, `lstResults` |
| CheckBox | chk | `chkRememberMe`, `chkShowDetails` |
| RadioButton | rdb | `rdbOption1`, `rdbMale` |
| DataGridView | dgv | `dgvResults`, `dgvData` |
| Panel | pnl | `pnlMain`, `pnlSidebar` |
| GroupBox | grp | `grpOptions`, `grpParameters` |
| PictureBox | pic | `picLogo`, `picChart` |
| MenuStrip | mnu | `mnuMain` |
| ToolStrip | tls | `tlsMain` |
| StatusStrip | sts | `stsMain` |
| TabControl | tab | `tabMain`, `tabOptions` |
| TabPage | tpg | `tpgGeneral`, `tpgAdvanced` |
| TreeView | trv | `trvFolders`, `trvHierarchy` |
| ProgressBar | prg | `prgLoading`, `prgProgress` |
| NumericUpDown | nud | `nudQuantity`, `nudAge` |
| DateTimePicker | dtp | `dtpStartDate`, `dtpBirthday` |
| ToolTip | tip | `tipMain` |
| ErrorProvider | err | `errValidation` |
| Timer | tmr | `tmrRefresh`, `tmrAutoSave` |
| ListView | lvw | `lvwFiles`, `lvwUsers` |
| RichTextBox | rtb | `rtbContent`, `rtbDescription` |
| LinkLabel | lnk | `lnkWebsite`, `lnkHelp` |
| HScrollBar | hsb | `hsbHorizontal` |
| VScrollBar | vsb | `vsbVertical` |
| MaskedTextBox | mtb | `mtbPhoneNumber`, `mtbDate` |
| SplitContainer | spc | `spcMain`, `spcLeftRight` |
| WebBrowser | web | `webPreview`, `webContent` |
| NotifyIcon | nfy | `nfyTray` |
| ContextMenuStrip | cms | `cmsRightClick` |
| ToolStripButton | tsb | `tsbSave`, `tsbNew` |
| ToolStripMenuItem | tmi | `tmiFile`, `tmiEdit` |
| ToolStripStatusLabel | tsl | `tslStatus`, `tslMessage` |
| BindingSource | bds | `bdsData`, `bdsUsers` |
| BindingNavigator | bdn | `bdnMain`, `bdnRecords` |
| ImageList | iml | `imlIcons`, `imlToolbar` |

**Timer 控件说明：**
- **System.Windows.Forms.Timer**（`tmr` 前缀）：用于 UI 相关的定时任务（如定时刷新界面、自动保存）
  - 在 UI 线程上执行
  - 适合短时间、轻量级的 UI 更新
  - 示例：`tmrRefresh`, `tmrAutoSave`
  
- **System.Timers.Timer**：用于后台定时任务（如定期检查、后台处理）
  - 在线程池线程上执行
  - 更新 UI 时需要使用 Invoke
  - 通常定义为私有字段，使用 `_timer` 命名

**命名原则：**
- 名称必须有意义且描述控件用途
- 使用完整的英文单词，避免过度缩写
- 使用 PascalCase（首字母大写的驼峰命名）
- ✅ 好的命名：`btnCalculateTraction`, `txtLocomotiveWeight`
- ❌ 不好的命名：`button1`, `textBox3`, `btn`, `txt1`

### 1.2 窗体命名

窗体类名使用 `Form` 后缀：
- `LoginForm`
- `MainForm`
- `AccelerationTimeForm`
- `SettingsForm`

### 1.3 字段和属性命名

- **私有字段**：使用下划线前缀 + camelCase
  - `_userName`
  - `_connectionString`
  - `_isDataLoaded`

- **公共属性**：使用 PascalCase
  - `UserName`
  - `ConnectionString`
  - `IsDataLoaded`

- **常量**：使用 PascalCase 
  - `MaxRetryCount` 

### 1.4 方法命名

- **事件处理方法**：使用 `On[ControlName][EventName]` 格式
  - `OnSaveButtonClick`
  - `OnUserNameTextBoxTextChanged`
  - `OnDataGridViewCellValueChanged`
  - 注意：设计器自动生成的 `btnSave_Click` 也可接受，但新编写的事件处理应使用 On 前缀格式

- **业务方法**：使用动词开头的 PascalCase
  - `CalculateTraction()`
  - `LoadData()`
  - `ValidateInput()`
  - `SaveToDatabase()`

---

## 2. 控件声明和初始化

### 2.1 控件声明位置

**尽量在 Designer.cs 中声明和初始化控件**

- 通过设计器添加的所有控件应保留在 `Designer.cs` 中
- 控件的基本属性设置（Size、Location、Text、Font 等）应在 `Designer.cs` 中完成
- 控件的静态事件绑定应在 `Designer.cs` 中完成

### 2.2 动态控件和复杂初始化

**以下情况可以在后台代码（.cs 文件）中处理：**

1. **动态添加的控件**
   - 运行时根据数据动态创建的控件
   - 循环生成的控件

2. **复杂的控件初始化**
   - DataGridView 的列定义和格式设置（当列很多或配置复杂时）
   - Chart 控件的复杂配置
   - TreeView 的节点结构初始化
   - 需要从数据库或配置文件读取的初始化

3. **数据绑定**
   - ComboBox 的数据源设置
   - DataGridView 的数据源绑定
   - 需要异步加载的数据

**推荐做法：**
- 在窗体构造函数中调用 `InitializeComponent()` 后进行自定义初始化
- 或在 `Form_Load` 事件中进行需要数据的初始化

### 2.3 不要手动修改 Designer.cs

**原则：尽量避免直接编辑 Designer.cs 文件**

- 使用设计器的属性窗口进行修改
- 如果必须手动修改，要格外小心，确保不破坏设计器的代码生成逻辑
- 复杂逻辑应该放在 .cs 文件中，而不是试图修改 Designer.cs

---

## 3. 事件处理

### 3.1 事件订阅位置

**尽量在 Designer.cs 中订阅事件**

- 设计器中添加的事件（通过属性窗口的闪电图标）会自动在 Designer.cs 中订阅
- 这种方式更安全，设计器会自动管理事件的订阅和取消订阅

### 3.2 动态事件订阅

**以下情况在后台代码中订阅事件：**

1. **动态创建的控件的事件**
2. **需要条件判断的事件订阅**
3. **运行时才能确定的事件绑定**

**注意事项：**
- 动态订阅的事件要在适当时机取消订阅，防止内存泄漏
- 可以在 `Dispose` 方法或窗体关闭事件中取消订阅

### 3.3 事件处理方法

- 事件处理方法应简洁，复杂逻辑应提取到独立方法中
- 使用 XML 注释说明事件处理的目的

---

## 4. 代码组织和结构

### 4.1 代码文件结构

**推荐的代码块顺序（在 .cs 文件中）：**

```
1. using 语句
2. namespace 声明
3. 类声明和 XML 注释
4. 私有字段
5. 公共属性
6. 构造函数
7. 窗体生命周期事件（Load, Shown, FormClosing 等）
8. 控件事件处理方法
9. 公共方法
10. 私有辅助方法
11. 重写的方法（如 Dispose）
```

### 4.2 使用 #region 组织代码

**推荐使用 region 对代码进行分组：**

```csharp
#region 字段

#endregion

#region 属性

#endregion

#region 构造函数

#endregion

#region 事件处理

#endregion

#region 公共方法

#endregion

#region 私有方法

#endregion
```

### 4.3 代码复用

**提取公共控件和功能：**

1. **创建用户控件（UserControl）**
   - 当多个窗体需要相同的控件组合时
   - 当某个控件组具有独立的逻辑功能时
   - 命名：使用描述性名称 + `Control` 后缀，如 `ParameterInputControl`

2. **提取为辅助类或扩展方法**
   - 通用的验证逻辑
   - 通用的格式化逻辑
   - 通用的数据处理逻辑

3. **使用继承**
   - 创建基础窗体类，包含公共功能
   - 派生窗体继承基础窗体

---

## 5. XML 文档注释

### 5.1 必须添加注释的内容

**所有以下内容都需要 XML 文档注释：**

1. **公共类**
2. **公共方法**
3. **公共属性**
4. **事件处理方法**（如果逻辑复杂）
5. **复杂的私有方法**

### 5.2 注释格式

**类注释：**
```csharp
/// <summary>
/// 上坡牵引质量计算窗体
/// </summary>
public partial class UphillTractionForm : Form
```

**方法注释：**
```csharp
/// <summary>
/// 计算牵引质量
/// </summary>
/// <param name="weight">机车重量（吨）</param>
/// <param name="gradient">坡度（‰）</param>
/// <returns>计算结果</returns>
private double CalculateTractionMass(double weight, double gradient)
```

**属性注释：**
```csharp
/// <summary>
/// 获取或设置当前选择的机车类型
/// </summary>
public string SelectedLocomotiveType { get; set; }
```

**事件处理注释：**
```csharp
/// <summary>
/// 处理计算按钮点击事件，执行牵引质量计算
/// </summary>
private void OnCalculateButtonClick(object sender, EventArgs e)
```

### 5.3 注释原则

- 说明"做什么"而不是"怎么做"
- 对于复杂算法，说明业务含义
- 对于参数，说明单位和取值范围
- 对于返回值，说明含义和可能的特殊值

---

## 6. 异步操作和线程

### 6.1 使用 async/await

**对于长时间运行的操作，使用 async/await 模式：**

- 数据库查询
- 文件 I/O 操作
- 网络请求
- 复杂计算

### 6.2 使用 Task

**尽量使用 Task 而不是 BackgroundWorker：**

```csharp
/// <summary>
/// 异步加载数据
/// </summary>
private async Task LoadDataAsync()
{
    // 显示加载提示
    prgLoading.Visible = true;
    btnLoad.Enabled = false;
    
    try
    {
        // 异步操作
        var data = await Task.Run(() => DatabaseService.LoadData());
        
        // 更新 UI（这里已经在 UI 线程）
        dgvData.DataSource = data;
    }
    catch (Exception ex)
    {
        MessageBox.Show($"加载数据失败: {ex.Message}", "错误", 
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    finally
    {
        prgLoading.Visible = false;
        btnLoad.Enabled = true;
    }
}
```

### 6.3 UI 线程更新规则

**后台线程更新 UI 必须使用 Invoke/BeginInvoke：**

```csharp
// ❌ 错误：直接在后台线程更新 UI
Task.Run(() => 
{
    lblStatus.Text = "处理中...";  // 会抛出异常
});

// ✅ 正确：使用 Invoke
Task.Run(() => 
{
    this.Invoke(new Action(() => 
    {
        lblStatus.Text = "处理中...";
    }));
});

// ✅ 更好：使用 async/await
private async void OnProcessButtonClick(object sender, EventArgs e)
{
    lblStatus.Text = "开始处理...";
    
    await Task.Run(() => 
    {
        // 后台处理
        DoHeavyWork();
    });
    
    lblStatus.Text = "处理完成";  // 这里已经回到 UI 线程
}
```

### 6.4 显示进度和等待提示

**长时间操作必须提供用户反馈：**

1. 使用 ProgressBar 显示进度
2. 使用 Cursor.Current = Cursors.WaitCursor
3. 禁用相关操作按钮，防止重复操作
4. 在 finally 块中恢复 UI 状态

### 6.5 异步事件处理的陷阱

**避免在非事件处理器中使用 async void：**

```csharp
// ❌ 错误：async void 方法（事件处理器除外）
private async void LoadDataAsync()  // 异常无法被捕获
{
    var data = await DatabaseService.LoadAsync();
}

// ✅ 正确：使用 async Task
private async Task LoadDataAsync()
{
    var data = await DatabaseService.LoadAsync();
}

// ✅ 正确：事件处理器可以使用 async void
private async void OnLoadButtonClick(object sender, EventArgs e)
{
    try
    {
        await LoadDataAsync();
    }
    catch (Exception ex)
    {
        // 必须在事件处理器中捕获所有异常
        MessageBox.Show($"加载失败: {ex.Message}");
    }
}
```

**异步事件处理的最佳实践：**

1. **事件处理器中必须捕获所有异常**
   ```csharp
   private async void OnSaveButtonClick(object sender, EventArgs e)
   {
       try
       {
           await SaveDataAsync();
       }
       catch (Exception ex)
       {
           // 必须处理，否则异常会导致程序崩溃
           LogError(ex);
           MessageBox.Show("保存失败", "错误", 
               MessageBoxButtons.OK, MessageBoxIcon.Error);
       }
   }
   ```

2. **避免异步操作中的竞态条件**
   ```csharp
   private bool _isLoading = false;
   
   private async void OnLoadButtonClick(object sender, EventArgs e)
   {
       if (_isLoading) return;  // 防止重复点击
       
       _isLoading = true;
       btnLoad.Enabled = false;
       
       try
       {
           await LoadDataAsync();
       }
       finally
       {
           _isLoading = false;
           btnLoad.Enabled = true;
       }
   }
   ```

3. **处理窗体关闭时的异步操作**
   ```csharp
   private CancellationTokenSource _cts;
   
   private async void OnLoadButtonClick(object sender, EventArgs e)
   {
       _cts = new CancellationTokenSource();
       
       try
       {
           await LoadDataAsync(_cts.Token);
       }
       catch (OperationCanceledException)
       {
           // 操作已取消
       }
   }
   
   protected override void OnFormClosing(FormClosingEventArgs e)
   {
       _cts?.Cancel();  // 取消所有异步操作
       base.OnFormClosing(e);
   }
   ```

4. **async void 的唯一使用场景**
   - 仅在事件处理器中使用（如 Click、Load 等事件）
   - 其他任何情况都应该使用 `async Task` 或 `async Task<T>`

---

## 7. 数据验证和错误处理

### 7.1 输入验证

**验证时机：**

1. **即时验证（KeyPress）**：限制输入字符类型
2. **失去焦点验证（Validating）**：验证格式和范围
3. **提交验证**：执行业务规则验证

**使用 ErrorProvider 显示验证错误：**

```csharp
/// <summary>
/// 验证重量输入
/// </summary>
private bool ValidateWeightInput()
{
    if (string.IsNullOrWhiteSpace(txtWeight.Text))
    {
        errValidation.SetError(txtWeight, "请输入机车重量");
        return false;
    }
    
    if (!double.TryParse(txtWeight.Text, out double weight) || weight <= 0)
    {
        errValidation.SetError(txtWeight, "请输入有效的正数");
        return false;
    }
    
    errValidation.SetError(txtWeight, "");
    return true;
}
```

### 7.2 异常处理

**UI 层的异常处理原则：**

1. **捕获具体异常**，不要只捕获 Exception
2. **向用户显示友好的错误信息**
3. **记录详细的错误日志**（如果有日志系统）
4. **恢复 UI 状态**，不要让程序处于不一致状态

```csharp
/// <summary>
/// 保存数据到数据库
/// </summary>
private async Task SaveDataAsync()
{
    try
    {
        // 验证输入
        if (!ValidateInput())
        {
            return;
        }
        
        // 保存数据
        await DatabaseService.SaveAsync(data);
        
        MessageBox.Show("保存成功", "提示", 
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    catch (SqlException ex)
    {
        MessageBox.Show($"数据库操作失败: {ex.Message}", "错误", 
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"保存失败: {ex.Message}", "错误", 
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

### 7.3 消息提示

**使用 MessageBox 的规范：**

- **信息提示**：使用 `MessageBoxIcon.Information`
- **警告提示**：使用 `MessageBoxIcon.Warning`
- **错误提示**：使用 `MessageBoxIcon.Error`
- **确认对话框**：使用 `MessageBoxIcon.Question` 和 `MessageBoxButtons.YesNo`

**除非用户特别要求，否则使用系统默认的 MessageBox**

---

## 8. 布局和响应式设计

### 8.1 使用 Anchor 和 Dock

**合理使用控件的 Anchor 和 Dock 属性：**

- **Anchor**：适用于需要随窗体大小调整位置和大小的控件
  - 右下角的按钮：Anchor = Bottom, Right
  - 可拉伸的 TextBox：Anchor = Top, Left, Right

- **Dock**：适用于需要填充整个区域的控件
  - 主要内容区：Dock = Fill
  - 顶部工具栏：Dock = Top
  - 底部状态栏：Dock = Bottom

### 8.2 使用布局容器

**推荐使用布局容器管理复杂布局：**

- **TableLayoutPanel**：适合网格式布局
- **FlowLayoutPanel**：适合流式布局（如工具栏按钮）
- **SplitContainer**：适合可调整大小的分割布局

### 8.3 控件大小和间距

**遵循一致的设计原则：**

- 使用统一的控件间距（如 10 像素）
- 对齐相关控件
- 使用合适的字体大小（避免过小）
- 确保按钮大小足够点击

### 8.4 高 DPI 与缩放支持

**确保应用程序在不同 DPI 环境下正常显示：**

1. **设置 AutoScaleMode**
   ```csharp
   // 在窗体构造函数或设计器中设置
   this.AutoScaleMode = AutoScaleMode.Dpi;  // 推荐
   // 或
   this.AutoScaleMode = AutoScaleMode.Font;  // 备选
   ```

2. **在 Program.cs 中启用高 DPI 支持**
   ```csharp
   static void Main()
   {
       // .NET Framework 4.7+ 或 .NET Core/5+
       Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
       Application.EnableVisualStyles();
       Application.SetCompatibleTextRenderingDefault(false);
       Application.Run(new MainForm());
   }
   ```

3. **应用程序清单文件（app.manifest）**
   ```xml
   <application xmlns="urn:schemas-microsoft-com:asm.v3">
     <windowsSettings>
       <dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true</dpiAware>
       <dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">PerMonitorV2</dpiAwareness>
     </windowsSettings>
   </application>
   ```

4. **设计建议**
   - 优先使用 Anchor 和 Dock 而不是固定位置
   - 使用相对布局（TableLayoutPanel、FlowLayoutPanel）
   - 避免硬编码像素值
   - 使用矢量图标或多尺寸图片

5. **处理 DPI 变化事件**
   ```csharp
   /// <summary>
   /// 处理 DPI 变化（当窗体在不同 DPI 显示器间移动）
   /// </summary>
   protected override void OnDpiChanged(DpiChangedEventArgs e)
   {
       base.OnDpiChanged(e);
       
       // 调整字体大小、图标等
       AdjustForDpi(e.DeviceDpiNew);
   }
   ```

6. **常见问题**
   - 如果控件布局混乱，检查 AutoScaleMode 设置
   - 如果字体模糊，确保启用了 PerMonitorV2 模式
   - 测试多显示器环境（不同 DPI）

---

## 9. 资源管理

### 9.1 资源释放

**手动创建的资源必须正确释放：**

```csharp
/// <summary>
/// 清理资源
/// </summary>
protected override void Dispose(bool disposing)
{
    if (disposing)
    {
        // 释放托管资源
        if (components != null)
        {
            components.Dispose();
        }
        
        // 释放手动创建的资源
        if (_customBitmap != null)
        {
            _customBitmap.Dispose();
            _customBitmap = null;
        }
    }
    
    base.Dispose(disposing);
}
```

### 9.2 使用 using 语句

**对于临时资源，使用 using 语句：**

```csharp
using (var connection = new SqlConnection(connectionString))
{
    // 使用连接
}  // 自动释放

using (var bitmap = new Bitmap(filePath))
{
    // 处理图像
}  // 自动释放
```

### 9.3 IDisposable 对象的正确管理

**必须正确管理所有实现 IDisposable 的对象：**

1. **事件订阅导致的内存泄漏**
   ```csharp
   // ❌ 错误：未取消订阅
   public class ChildForm : Form
   {
       public ChildForm()
       {
           InitializeComponent();
           SomeStaticEvent.DataChanged += OnDataChanged;
       }
   }
   
   // ✅ 正确：在 Dispose 中取消订阅
   public class ChildForm : Form
   {
       protected override void Dispose(bool disposing)
       {
           if (disposing)
           {
               SomeStaticEvent.DataChanged -= OnDataChanged;
           }
           base.Dispose(disposing);
       }
   }
   ```

2. **静态引用导致的内存泄漏**
   ```csharp
   // ❌ 错误：静态字段持有窗体引用
   public static MainForm Instance;
   
   // ✅ 正确：使用弱引用或及时清理
   private static WeakReference<MainForm> _instanceRef;
   ```

3. **常见需要释放的对象**
   - Graphics 对象
   - Bitmap、Image 等图像对象
   - Font、Brush、Pen 等 GDI+ 对象
   - 数据库连接、DataReader
   - 文件流、网络流
   - Timer（System.Timers.Timer）
   - 手动创建的控件

4. **动态创建控件的释放**
   ```csharp
   /// <summary>
   /// 清理动态创建的控件
   /// </summary>
   protected override void Dispose(bool disposing)
   {
       if (disposing)
       {
           // 移除并释放动态控件
           foreach (Control control in _dynamicControls)
           {
               this.Controls.Remove(control);
               control.Dispose();
           }
           _dynamicControls.Clear();
       }
       
       base.Dispose(disposing);
   }
   ```

5. **检查是否已释放**
   ```csharp
   private bool _disposed = false;
   
   protected override void Dispose(bool disposing)
   {
       if (_disposed)
           return;
           
       if (disposing)
       {
           // 释放托管资源
       }
       
       _disposed = true;
       base.Dispose(disposing);
   }
   ```

---

## 10. 窗体生命周期

### 10.1 常用生命周期事件

**理解并正确使用窗体生命周期事件：**

1. **构造函数**：创建对象，调用 InitializeComponent()
2. **Load**：窗体首次显示前，初始化数据和状态
3. **Shown**：窗体首次显示后，适合获取焦点等操作
4. **Activated**：窗体获得焦点时
5. **Deactivate**：窗体失去焦点时
6. **FormClosing**：窗体关闭前，可以取消关闭
7. **FormClosed**：窗体已关闭，清理资源

### 10.2 初始化建议

**在合适的时机进行初始化：**

- **构造函数**：设置不依赖外部数据的属性
- **Load 事件**：加载数据、填充控件
- **Shown 事件**：设置焦点、显示提示

### 10.3 关闭窗体前的处理

```csharp
/// <summary>
/// 窗体关闭前检查未保存的更改
/// </summary>
private void OnFormClosing(object sender, FormClosingEventArgs e)
{
    if (_hasUnsavedChanges)
    {
        var result = MessageBox.Show(
            "有未保存的更改，确定要关闭吗？", 
            "确认", 
            MessageBoxButtons.YesNo, 
            MessageBoxIcon.Question);
            
        if (result == DialogResult.No)
        {
            e.Cancel = true;  // 取消关闭
        }
    }
}
```

---

## 11. 性能优化

### 11.1 批量更新 UI

**批量更新控件时使用 SuspendLayout/ResumeLayout：**

```csharp
/// <summary>
/// 批量添加控件
/// </summary>
private void AddManyControls()
{
    pnlContainer.SuspendLayout();
    
    try
    {
        for (int i = 0; i < 100; i++)
        {
            var button = new Button
            {
                Text = $"Button {i}",
                Location = new Point(10, i * 30)
            };
            pnlContainer.Controls.Add(button);
        }
    }
    finally
    {
        pnlContainer.ResumeLayout();
    }
}
```

### 11.2 DataGridView 优化

**处理大量数据时的优化：**

1. 使用虚拟模式（VirtualMode）
2. 禁用不必要的功能（如自动列生成）
3. 更新数据时使用 BeginUpdate/EndUpdate
4. 考虑分页显示

### 11.3 避免不必要的重绘

- 减少频繁修改控件属性
- 使用双缓冲（DoubleBuffered = true）
- 批量操作后再刷新界面

---

## 12. 数据绑定

### 12.1 简单数据绑定

**对于简单的属性绑定：**

```csharp
// 绑定到对象属性
txtUserName.DataBindings.Add("Text", user, "UserName");

// 双向绑定
txtAge.DataBindings.Add("Text", user, "Age", true, 
    DataSourceUpdateMode.OnPropertyChanged);
```

### 12.2 列表绑定

**ComboBox 和 ListBox 的数据绑定：**

```csharp
/// <summary>
/// 绑定机车类型下拉列表
/// </summary>
private void BindLocomotiveTypes()
{
    cboLocomotiveType.DataSource = locomotiveTypes;
    cboLocomotiveType.DisplayMember = "Name";
    cboLocomotiveType.ValueMember = "Id";
}
```

### 12.3 DataGridView 绑定

**推荐使用 BindingSource：**

```csharp
private BindingSource _dataBindingSource = new BindingSource();

/// <summary>
/// 绑定数据网格
/// </summary>
private void BindDataGrid()
{
    _dataBindingSource.DataSource = dataList;
    dgvData.DataSource = _dataBindingSource;
}
```

### 12.4 设计时数据绑定

**在设计器中使用 BindingSource 组件：**

1. **添加 BindingSource 组件**
   - 从工具箱拖拽 BindingSource 到窗体（会出现在组件托盘中）
   - 命名为 `bdsDataSource`（使用 `bds` 前缀）

2. **配置 BindingSource 属性**
   - **DataSource**：设置数据源类型（可在设计时选择项目中的类）
   - **DataMember**：设置数据成员（如果数据源是 DataSet）

3. **绑定控件到 BindingSource**
   - 在控件的 DataBindings 属性中设置绑定
   - 选择 BindingSource 作为数据源
   - 设置要绑定的属性路径

4. **Format 和 FormatString 设置**
   ```csharp
   // 在设计器的 DataBindings 编辑器中：
   // - FormattingEnabled: True
   // - FormatString: N2（数字格式）、d（日期格式）等
   // - NullValue: 空值时的默认显示
   ```

5. **设计时绑定的优势**
   - 可视化配置，减少代码量
   - 设计器自动管理绑定的生命周期
   - 更容易发现绑定错误

**注意事项：**
- 设计时绑定适合简单场景，复杂的数据处理建议在代码中完成
- BindingSource 需要在 Designer.cs 中声明和初始化
- 运行时可以通过代码修改 BindingSource 的 DataSource

---

## 13. 跨窗体通信

### 13.1 传递数据到新窗体

**通过构造函数传递：**

```csharp
/// <summary>
/// 打开详情窗体
/// </summary>
private void OpenDetailForm()
{
    var detailForm = new DetailForm(selectedData);
    detailForm.ShowDialog();
}
```

### 13.2 从子窗体获取数据

**使用公共属性：**

```csharp
/// <summary>
/// 打开选择窗体并获取结果
/// </summary>
private void SelectItem()
{
    var selectForm = new SelectForm();
    if (selectForm.ShowDialog() == DialogResult.OK)
    {
        var selectedItem = selectForm.SelectedItem;
        // 使用选择的项
    }
}
```

### 13.3 使用事件通信

**子窗体通知父窗体：**

```csharp
// 子窗体定义事件
public event EventHandler<DataEventArgs> DataChanged;

// 触发事件
protected virtual void OnDataChanged(DataEventArgs e)
{
    DataChanged?.Invoke(this, e);
}

// 父窗体订阅事件
childForm.DataChanged += OnChildFormDataChanged;
```

---

## 14. 禁止的做法

### 14.1 严格禁止

❌ **以下做法严格禁止：**

1. **过度使用 Application.DoEvents()**
   - 会导致重入问题
   - 应该使用 async/await

2. **在 UI 线程执行长时间操作**
   - 会导致界面冻结
   - 应该使用异步方法

3. **不处理异常**
   - 可能导致程序崩溃
   - 至少要捕获并记录

4. **硬编码字符串和魔法数字**
   - 应该使用常量或配置
   - 便于维护和修改

5. **在 Designer.cs 中编写业务逻辑**
   - Designer.cs 应该只包含设计器生成的代码
   - 业务逻辑放在 .cs 文件中

### 14.2 不推荐但不禁止

⚠️ **以下做法不推荐，但特殊情况下可以使用：**

1. **使用 Form.Tag 属性存储数据**
   - 优先使用强类型属性

2. **过多的全局变量**
   - 优先使用依赖注入或传参

3. **复杂的嵌套 if-else**
   - 考虑重构为多个方法

---

## 15. 第三方控件集成规范

### 15.1 选择第三方控件

**评估标准：**
- 稳定性和维护频率
- 文档和社区支持
- 许可证类型（商业/开源）
- 性能和兼容性

**常见控件库：**
- DevExpress WinForms
- Telerik UI for WinForms
- Syncfusion Windows Forms
- ComponentOne

### 15.2 命名规范

**第三方控件命名：**
- 使用控件库的标准前缀 + 描述性名称
- DevExpress：`dxe` 开头（如 `dxeUserName`、`dxgData`）
- Telerik：`rad` 开头（如 `radUserName`、`radGridView`）
- 或使用通用规则加供应商后缀（如 `txtUserName_DX`）

### 15.3 版本管理

**控件库版本管理：**

1. **使用 NuGet 包管理**
   - 统一团队的控件库版本
   - 在 packages.config 或 .csproj 中锁定版本

2. **记录版本信息**
   ```csharp
   // 在 AssemblyInfo.cs 或文档中记录
   // DevExpress v23.1.5
   // Telerik UI for WinForms R3 2023
   ```

3. **升级策略**
   - 在独立分支测试升级
   - 检查破坏性变更
   - 全面回归测试

### 15.4 自定义样式

**统一样式管理：**

1. **创建主题配置类**
   ```csharp
   /// <summary>
   /// 第三方控件主题配置
   /// </summary>
   public static class ThemeManager
   {
       public static void ApplyTheme()
       {
           // DevExpress 主题
           DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("Office 2019 Colorful");
           
           // Telerik 主题
           // ThemeResolutionService.ApplicationThemeName = "Office2019Light";
       }
   }
   ```

2. **在 Program.cs 中初始化**
   ```csharp
   static void Main()
   {
       Application.EnableVisualStyles();
       Application.SetCompatibleTextRenderingDefault(false);
       
       // 应用第三方控件主题
       ThemeManager.ApplyTheme();
       
       Application.Run(new MainForm());
   }
   ```

3. **自定义皮肤**
   - 将自定义皮肤文件放在 `Themes` 文件夹
   - 在版本控制中包含皮肤文件
   - 文档中说明如何切换主题

### 15.5 许可证管理

**保护许可证信息：**

1. 不要将许可证文件提交到公共仓库
2. 使用环境变量或加密配置存储许可证
3. 在 .gitignore 中排除许可证文件
4. 提供许可证配置说明文档

---

## 16. 日志与监控

### 16.1 集成日志系统

**推荐的日志框架：**

1. **NLog**
   ```csharp
   using NLog;
   
   public class MainForm : Form
   {
       private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
       
       private void OnSaveButtonClick(object sender, EventArgs e)
       {
           try
           {
               Logger.Info("开始保存数据");
               SaveData();
               Logger.Info("数据保存成功");
           }
           catch (Exception ex)
           {
               Logger.Error(ex, "保存数据失败");
               MessageBox.Show("保存失败", "错误");
           }
       }
   }
   ```

2. **Serilog**
   ```csharp
   using Serilog;
   
   // 在 Program.cs 中配置
   Log.Logger = new LoggerConfiguration()
       .MinimumLevel.Debug()
       .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
       .CreateLogger();
   ```

### 16.2 日志配置

**NLog.config 示例：**

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd">
  <targets>
    <target name="logfile" 
            xsi:type="File" 
            fileName="logs/${shortdate}.log"
            layout="${longdate} ${level:uppercase=true} ${logger} ${message} ${exception:format=tostring}" />
    
    <target name="console" 
            xsi:type="Console" 
            layout="${time} ${level} ${message}" />
  </targets>
  
  <rules>
    <logger name="*" minlevel="Info" writeTo="logfile,console" />
  </rules>
</nlog>
```

### 16.3 日志级别使用建议

**日志级别规范：**

- **Trace**：详细的调试信息（方法进入/退出）
- **Debug**：开发调试信息（变量值、流程状态）
- **Info**：关键业务操作（用户登录、数据保存）
- **Warn**：警告信息（性能慢、数据异常但可恢复）
- **Error**：错误信息（操作失败、异常捕获）
- **Fatal**：致命错误（程序崩溃、无法恢复）

### 16.4 异常监控

**集成异常监控服务（可选）：**

1. **Sentry**
   ```csharp
   using Sentry;
   
   // 在 Program.cs 中初始化
   static void Main()
   {
       using (SentrySdk.Init("your-dsn-here"))
       {
           Application.Run(new MainForm());
       }
   }
   
   // 捕获异常
   try
   {
       // 操作
   }
   catch (Exception ex)
   {
       SentrySdk.CaptureException(ex);
       throw;
   }
   ```

2. **全局异常处理**
   ```csharp
   static void Main()
   {
       // 捕获 UI 线程异常
       Application.ThreadException += (s, e) =>
       {
           Logger.Fatal(e.Exception, "UI 线程未处理异常");
           MessageBox.Show("程序发生严重错误，即将关闭");
       };
       
       // 捕获非 UI 线程异常
       AppDomain.CurrentDomain.UnhandledException += (s, e) =>
       {
           Logger.Fatal(e.ExceptionObject as Exception, "应用程序未处理异常");
       };
       
       Application.Run(new MainForm());
   }
   ```

---

## 17. 窗体和控件的自定义绘制

### 17.1 重写 OnPaint 方法

**何时使用自定义绘制：**
- 需要特殊的视觉效果
- 标准控件无法满足需求
- 性能优化（如自绘列表）
- 自定义图表、仪表盘等

**基本模式：**

```csharp
/// <summary>
/// 自定义绘制的面板
/// </summary>
public class CustomPanel : Panel
{
    /// <summary>
    /// 重写绘制方法
    /// </summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        
        Graphics g = e.Graphics;
        
        // 启用抗锯齿
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        
        // 自定义绘制
        using (var brush = new SolidBrush(Color.Blue))
        {
            g.FillRectangle(brush, 10, 10, 100, 50);
        }
    }
}
```

### 17.2 双缓冲

**启用双缓冲防止闪烁：**

```csharp
public class CustomControl : Control
{
    public CustomControl()
    {
        // 方法1：设置控件样式
        this.DoubleBuffered = true;
        
        // 方法2：设置控件样式标志
        this.SetStyle(
            ControlStyles.AllPaintingInWmPaint | 
            ControlStyles.UserPaint | 
            ControlStyles.OptimizedDoubleBuffer, 
            true);
            
        this.UpdateStyles();
    }
}
```

### 17.3 Graphics 对象管理

**正确使用 Graphics 对象：**

```csharp
protected override void OnPaint(PaintEventArgs e)
{
    base.OnPaint(e);
    
    Graphics g = e.Graphics;  // 使用事件提供的 Graphics，不要 Dispose
    
    // ✅ 正确：使用 using 释放自己创建的 GDI+ 对象
    using (var pen = new Pen(Color.Red, 2))
    using (var brush = new SolidBrush(Color.Blue))
    using (var font = new Font("Arial", 12))
    {
        g.DrawLine(pen, 0, 0, 100, 100);
        g.FillEllipse(brush, 50, 50, 100, 100);
        g.DrawString("文本", font, Brushes.Black, 10, 10);
    }
    
    // ❌ 错误：不要 Dispose 事件提供的 Graphics
    // g.Dispose();  // 会导致错误
}
```

### 17.4 性能优化

**自定义绘制的性能建议：**

1. **缓存 Brush、Pen、Font 等对象**
   ```csharp
   private readonly Pen _borderPen = new Pen(Color.Black, 1);
   private readonly Font _titleFont = new Font("Arial", 14, FontStyle.Bold);
   
   protected override void Dispose(bool disposing)
   {
       if (disposing)
       {
           _borderPen?.Dispose();
           _titleFont?.Dispose();
       }
       base.Dispose(disposing);
   }
   ```

2. **只绘制可见区域**
   ```csharp
   protected override void OnPaint(PaintEventArgs e)
   {
       var clipRect = e.ClipRectangle;
       
       // 只绘制可见区域内的内容
       foreach (var item in GetVisibleItems(clipRect))
       {
           DrawItem(e.Graphics, item);
       }
   }
   ```

3. **避免频繁 Invalidate**
   - 批量更新后再调用 Invalidate
   - 使用 Invalidate(Rectangle) 只刷新部分区域

### 17.5 常见自定义绘制场景

**自定义按钮：**
```csharp
public class RoundButton : Button
{
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        
        using (var path = GetRoundedRectanglePath(this.ClientRectangle, 10))
        using (var brush = new SolidBrush(this.BackColor))
        {
            e.Graphics.FillPath(brush, path);
            
            // 绘制文本
            TextRenderer.DrawText(e.Graphics, this.Text, this.Font, 
                this.ClientRectangle, this.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
```

---

## 18. 配置管理

### 18.1 使用配置文件

**推荐使用 appsettings.json（.NET Core/5+）：**

1. **添加配置文件**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=database.db"
     },
     "AppSettings": {
       "LogLevel": "Info",
       "MaxRetryCount": 3,
       "TimeoutSeconds": 30
     }
   }
   ```

2. **读取配置**
   ```csharp
   using Microsoft.Extensions.Configuration;
   
   public class ConfigManager
   {
       private static IConfiguration _configuration;
       
       static ConfigManager()
       {
           _configuration = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .Build();
       }
       
       public static string GetConnectionString()
       {
           return _configuration.GetConnectionString("DefaultConnection");
       }
       
       public static int GetMaxRetryCount()
       {
           return _configuration.GetValue<int>("AppSettings:MaxRetryCount");
       }
   }
   ```

### 18.2 使用 app.config（.NET Framework）

**app.config 配置：**

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Data Source=database.db" 
         providerName="System.Data.SqlClient" />
  </connectionStrings>
  
  <appSettings>
    <add key="LogLevel" value="Info" />
    <add key="MaxRetryCount" value="3" />
  </appSettings>
</configuration>
```

**读取配置：**
```csharp
using System.Configuration;

public class ConfigManager
{
    public static string GetConnectionString()
    {
        return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
    }
    
    public static int GetMaxRetryCount()
    {
        return int.Parse(ConfigurationManager.AppSettings["MaxRetryCount"]);
    }
}
```

### 18.3 用户设置

**使用 Settings.settings：**

1. **在项目属性中创建设置**
   - 右键项目 → 属性 → 设置
   - 添加用户设置（Scope: User）或应用程序设置（Scope: Application）

2. **访问设置**
   ```csharp
   // 读取
   string userName = Properties.Settings.Default.UserName;
   
   // 修改并保存
   Properties.Settings.Default.UserName = "NewUser";
   Properties.Settings.Default.Save();
   ```

### 18.4 配置管理最佳实践

**规范建议：**

1. **敏感信息保护**
   - 不要将密码等敏感信息明文存储
   - 使用加密或环境变量
   - 不要提交包含敏感信息的配置文件到版本控制

2. **环境区分**
   ```
   appsettings.json           // 基础配置
   appsettings.Development.json  // 开发环境
   appsettings.Production.json   // 生产环境
   ```

3. **类型安全的配置类**
   ```csharp
   public class AppSettings
   {
       public string LogLevel { get; set; }
       public int MaxRetryCount { get; set; }
       public int TimeoutSeconds { get; set; }
   }
   
   // 绑定配置
   var appSettings = new AppSettings();
   _configuration.GetSection("AppSettings").Bind(appSettings);
   ```

---

## 19. 版本控制注意事项

### 19.1 Designer.cs 和 resx 文件

**版本控制时的注意事项：**

1. **Designer.cs 应该纳入版本控制**
   - 它是窗体设计的一部分
   - 但要避免不必要的手动修改

2. **resx 文件应该纳入版本控制**
   - 包含资源和本地化字符串
   - 合并冲突时要小心

3. **合并冲突处理**
   - Designer.cs 的冲突通常很难手动解决
   - 建议使用设计器重新调整有冲突的控件
   - 或者接受一方的更改后重新调整

### 19.2 避免冲突的最佳实践

- 不同开发人员避免同时修改同一个窗体
- 将大窗体拆分为用户控件，减少冲突
- 及时提交和拉取代码

---

## 20. 可访问性

### 20.1 基本要求

**确保应用程序的可访问性：**

1. **设置 TabIndex**
   - 确保控件的 Tab 顺序符合逻辑
   - 从左到右，从上到下

2. **设置快捷键（AccessKey）**
   - 为常用按钮和菜单设置快捷键
   - 在 Text 属性中使用 & 符号（如 "保存(&S)"）

3. **提供工具提示（ToolTip）**
   - 为图标按钮和复杂控件提供工具提示
   - 说明控件的用途

4. **使用有意义的控件文本**
   - Label 文本要清晰描述相关输入框
   - Button 文本要明确说明操作

---

## 21. 总结

### 核心原则

1. **一致性**：整个项目使用统一的命名和编码风格
2. **可读性**：代码要易于理解和维护
3. **可维护性**：合理的结构和充分的注释
4. **用户体验**：响应快速，错误提示友好
5. **健壮性**：充分的验证和错误处理

### 开发流程建议

1. **设计优先**：在设计器中设计 UI
2. **命名规范**：立即为控件设置有意义的名称
3. **分离关注点**：UI 逻辑和业务逻辑分离
4. **逐步实现**：先实现核心功能，再优化
5. **测试验证**：测试各种边界情况和错误场景

---

**文档版本：** 2.0  
**最后更新：** 2024年10月

**更新内容：**
- 新增更多控件命名规范（ListView、RichTextBox、LinkLabel等）
- 新增 Timer 控件区分说明
- 新增设计时数据绑定详细说明
- 增强资源管理与 Disposable 模式说明
- 新增异步事件处理陷阱章节
- 新增高 DPI 与缩放支持章节
- 新增第三方控件集成规范章节
- 新增日志与监控章节
- 新增窗体和控件的自定义绘制章节
- 新增配置管理章节

本规范为通用 WinForms 开发规范，具体项目可以根据实际情况进行调整和扩展。

