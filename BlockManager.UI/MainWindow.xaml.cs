using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using BlockManager.IPC.DTOs;
using BlockManager.UI.ViewModels;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using BlockManager.UI.Views;
using BlockManager.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BlockManager.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        
        // 添加窗体调整大小支持
        this.SourceInitialized += MainWindow_SourceInitialized;
        
        // 添加全局点击事件，用于隐藏搜索下拉列表
        this.PreviewMouseLeftButtonDown += MainWindow_PreviewMouseLeftButtonDown;
        
        // 移除Loaded事件，使用构造函数中的后台任务触发自动加载
        // Loaded += MainWindow_Loaded;
    }

    private void MainWindow_SourceInitialized(object sender, EventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        HwndSource.FromHwnd(hwnd)?.AddHook(HwndHook);
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_NCHITTEST = 0x0084;
        const int HTCLIENT = 1;
        const int HTCAPTION = 2;
        const int HTLEFT = 10;
        const int HTRIGHT = 11;
        const int HTTOP = 12;
        const int HTTOPLEFT = 13;
        const int HTTOPRIGHT = 14;
        const int HTBOTTOM = 15;
        const int HTBOTTOMLEFT = 16;
        const int HTBOTTOMRIGHT = 17;

        if (msg == WM_NCHITTEST)
        {
            var point = PointFromScreen(new Point(lParam.ToInt32() & 0xFFFF, lParam.ToInt32() >> 16));
            var resizeMargin = 8;

            // 检查是否在调整大小区域
            if (point.X <= resizeMargin && point.Y <= resizeMargin)
            {
                handled = true;
                return new IntPtr(HTTOPLEFT);
            }
            else if (point.X >= ActualWidth - resizeMargin && point.Y <= resizeMargin)
            {
                handled = true;
                return new IntPtr(HTTOPRIGHT);
            }
            else if (point.X <= resizeMargin && point.Y >= ActualHeight - resizeMargin)
            {
                handled = true;
                return new IntPtr(HTBOTTOMLEFT);
            }
            else if (point.X >= ActualWidth - resizeMargin && point.Y >= ActualHeight - resizeMargin)
            {
                handled = true;
                return new IntPtr(HTBOTTOMRIGHT);
            }
            else if (point.X <= resizeMargin)
            {
                handled = true;
                return new IntPtr(HTLEFT);
            }
            else if (point.X >= ActualWidth - resizeMargin)
            {
                handled = true;
                return new IntPtr(HTRIGHT);
            }
            else if (point.Y <= resizeMargin)
            {
                handled = true;
                return new IntPtr(HTTOP);
            }
            else if (point.Y >= ActualHeight - resizeMargin)
            {
                handled = true;
                return new IntPtr(HTBOTTOM);
            }
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// 全局点击事件处理，用于隐藏搜索下拉列表
    /// </summary>
    private void MainWindow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // 检查点击是否在搜索框区域外
        var searchTextBox = SearchTextBox;
        if (searchTextBox != null)
        {
            var mousePosition = e.GetPosition(this);
            var isOverSearchBox = IsMouseOverElement(searchTextBox, mousePosition);
            
            // 还需要检查是否点击在搜索结果区域
            var isOverSearchResults = _viewModel.IsSearchMode && IsClickInSearchResults(mousePosition);
            
            if (!isOverSearchBox && !isOverSearchResults)
            {
                // 点击在搜索相关区域外，隐藏搜索下拉列表
                _viewModel.IsSearchMode = false;
            }
        }
    }

    /// <summary>
    /// 检查鼠标是否在指定元素上
    /// </summary>
    private bool IsMouseOverElement(FrameworkElement element, Point mousePosition)
    {
        try
        {
            var elementPosition = element.TransformToAncestor(this).Transform(new Point(0, 0));
            var elementBounds = new Rect(elementPosition, element.RenderSize);
            return elementBounds.Contains(mousePosition);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 检查点击是否在搜索结果区域
    /// </summary>
    private bool IsClickInSearchResults(Point mousePosition)
    {
        // 简单的区域检查：搜索框下方的区域
        var searchTextBox = SearchTextBox;
        if (searchTextBox != null)
        {
            try
            {
                var searchBoxPosition = searchTextBox.TransformToAncestor(this).Transform(new Point(0, 0));
                var searchResultsArea = new Rect(
                    searchBoxPosition.X, 
                    searchBoxPosition.Y + searchTextBox.ActualHeight,
                    searchTextBox.ActualWidth, 
                    300); // 最大高度300
                return searchResultsArea.Contains(mousePosition);
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// 搜索框获得焦点事件
    /// </summary>
    private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        // 搜索框获得焦点时，如果有搜索结果则显示下拉列表
        if (_viewModel.SearchResults.Count > 0)
        {
            _viewModel.IsSearchMode = true;
        }
    }

    /// <summary>
    /// 搜索框失去焦点事件
    /// </summary>
    private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        // 延迟隐藏，给点击搜索结果的时间
        Task.Delay(100).ContinueWith(_ =>
        {
            Dispatcher.Invoke(() =>
            {
                if (!SearchTextBox.IsFocused)
                {
                    _viewModel.IsSearchMode = false;
                }
            });
        });
    }
    
    /// <summary>
    /// 窗口加载完成事件
    /// </summary>
    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[UI] MainWindow_Loaded事件被触发");
        
        // 写入日志文件确认调用
        try
        {
            var logPath = @"c:\temp\ui_autoload_debug.log";
            var logDir = System.IO.Path.GetDirectoryName(logPath);
            if (!System.IO.Directory.Exists(logDir))
            {
                System.IO.Directory.CreateDirectory(logDir);
            }
            System.IO.File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss.fff} MainWindow_Loaded事件被触发\n");
        }
        catch { }
        
        if (DataContext is MainWindowViewModel viewModel)
        {
            System.Diagnostics.Debug.WriteLine("[UI] 找到ViewModel，准备调用TriggerAutoLoadAsync");
            System.IO.File.AppendAllText(@"c:\temp\ui_autoload_debug.log", $"{DateTime.Now:HH:mm:ss.fff} 找到ViewModel，准备调用TriggerAutoLoadAsync\n");
            
            // 触发自动加载
            await viewModel.TriggerAutoLoadAsync();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[UI] 错误：DataContext不是MainWindowViewModel");
            System.IO.File.AppendAllText(@"c:\temp\ui_autoload_debug.log", $"{DateTime.Now:HH:mm:ss.fff} 错误：DataContext不是MainWindowViewModel\n");
        }
    }

    /// <summary>
    /// TreeView选择项变化事件处理
    /// </summary>
    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainWindowViewModel viewModel && e.NewValue is TreeNodeDto selectedNode)
        {
            viewModel.SelectedNode = selectedNode;
        }
    }

    /// <summary>
    /// TreeView预览鼠标按下事件 - 处理重复点击同一节点的情况
    /// </summary>
    private void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TreeView treeView)
        {
            // 获取点击的TreeViewItem
            var hitTest = VisualTreeHelper.HitTest(treeView, e.GetPosition(treeView));
            if (hitTest?.VisualHit != null)
            {
                var treeViewItem = FindParent<TreeViewItem>(hitTest.VisualHit);
                if (treeViewItem?.DataContext is TreeNodeDto clickedNode)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] TreeView点击节点: {clickedNode.Name}, 类型: {clickedNode.Type}");
                    
                    // 如果点击的是已选中的文件夹节点，强制触发切换
                    if (clickedNode.Type == "folder" && 
                        _viewModel.SelectedNode == clickedNode)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] 重复点击文件夹，强制切换到网格模式: {clickedNode.Name}");
                        
                        // 强制触发文件夹切换逻辑
                        _viewModel.CurrentPreview = null;
                        _viewModel.SelectedFile = null;
                        _viewModel.ShowDefaultHint = false;
                        _viewModel.ShowGrid = true;
                        
                        // 更新文件夹内容
                        if (_viewModel.SelectedNode?.Type == "folder")
                        {
                            // 通过反射或直接调用来更新文件夹文件
                            var method = typeof(MainWindowViewModel).GetMethod("UpdateCurrentFolderFiles", 
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            method?.Invoke(_viewModel, null);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 查找父级控件
    /// </summary>
    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        if (parent == null) return null;
        
        if (parent is T parentT)
            return parentT;
        
        return FindParent<T>(parent);
    }

    /// <summary>
    /// TreeView双击事件处理
    /// </summary>
    private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is TreeView treeView && treeView.SelectedItem is TreeNodeDto selectedNode)
        {
            _viewModel.FileDoubleClickCommand?.Execute(selectedNode);
        }
    }


    /// <summary>
    /// 搜索结果鼠标按下事件 - 单击定位
    /// </summary>
    private void SearchResult_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext is TreeNodeDto searchResult)
        {
            // 单击定位到文件（搜索结果现在只包含文件）
            System.Diagnostics.Debug.WriteLine($"[DEBUG] 搜索结果点击: {searchResult.Name}, 类型: {searchResult.Type}");
            
            if (searchResult.Type == "file")
            {
                // 文件：定位到其父文件夹并在网格中选中该文件
                NavigateToFileInGrid(searchResult);
            }
            
            // 隐藏搜索下拉列表
            _viewModel.IsSearchMode = false;
        }
    }

    /// <summary>
    /// 导航到文件在网格中的位置
    /// </summary>
    private void NavigateToFileInGrid(TreeNodeDto dwgFile)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] 导航到文件: {dwgFile.Name}");
            
            // 找到文件的父文件夹
            var parentFolder = FindParentFolder(dwgFile);
            if (parentFolder != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] 找到父文件夹: {parentFolder.Name}");
                
                // 选中父文件夹，这会触发网格显示
                _viewModel.SelectedNode = parentFolder;
                
                // 等待网格更新后选中目标文件
                Task.Delay(100).ContinueWith(_ =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 在网格中选中目标文件
                        _viewModel.SelectedFile = dwgFile;
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] 已在网格中选中文件: {dwgFile.Name}");
                    });
                });
            }
            else
            {
                _viewModel.StatusText = $"无法找到文件 {dwgFile.Name} 的父文件夹";
            }
        }
        catch (Exception ex)
        {
            _viewModel.StatusText = $"导航到文件失败: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[DEBUG] 导航失败: {ex}");
        }
    }

    /// <summary>
    /// 查找文件的父文件夹
    /// </summary>
    private TreeNodeDto? FindParentFolder(TreeNodeDto targetFile)
    {
        if (_viewModel.RootNode == null) return null;
        
        return FindParentFolderRecursive(_viewModel.RootNode, targetFile);
    }

    /// <summary>
    /// 递归查找父文件夹
    /// </summary>
    private TreeNodeDto? FindParentFolderRecursive(TreeNodeDto currentNode, TreeNodeDto targetFile)
    {
        // 检查当前节点的直接子节点
        foreach (var child in currentNode.Children)
        {
            if (child == targetFile)
            {
                return currentNode; // 找到了，当前节点就是父文件夹
            }
            
            // 如果子节点是文件夹，递归搜索
            if (child.Type == "folder")
            {
                var result = FindParentFolderRecursive(child, targetFile);
                if (result != null)
                {
                    return result;
                }
            }
        }
        
        return null;
    }


    /// <summary>
    /// TreeView右键菜单 - 预览
    /// </summary>
    private void TreeView_Preview(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && 
            menuItem.Parent is ContextMenu contextMenu &&
            contextMenu.PlacementTarget is Border border &&
            border.DataContext is TreeNodeDto treeNode)
        {
            // 预览DWG文件
            if (treeNode.Type == "file" && treeNode.IconType == "dwg")
            {
                _viewModel.SelectDwgFileCommand?.Execute(treeNode);
            }
            else if (treeNode.Type == "folder")
            {
                // 文件夹设置为选中状态，显示其内容
                _viewModel.SelectedNode = treeNode;
            }
        }
    }

    /// <summary>
    /// TreeView右键菜单 - 插入到CAD
    /// </summary>
    private void TreeView_Insert(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && 
            menuItem.Parent is ContextMenu contextMenu &&
            contextMenu.PlacementTarget is Border border &&
            border.DataContext is TreeNodeDto treeNode)
        {
            // 插入到CAD
            if (treeNode.Type == "file" && treeNode.IconType == "dwg")
            {
                _viewModel.InsertToCadCommand?.Execute(treeNode);
            }
        }
    }

    /// <summary>
    /// TreeView右键菜单 - 在文件夹中显示
    /// </summary>
    private void TreeView_ShowInFolder(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && 
            menuItem.Parent is ContextMenu contextMenu &&
            contextMenu.PlacementTarget is Border border &&
            border.DataContext is TreeNodeDto treeNode)
        {
            try
            {
                if (File.Exists(treeNode.Path))
                {
                    // 在文件资源管理器中选中文件
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{treeNode.Path}\"");
                }
                else if (Directory.Exists(treeNode.Path))
                {
                    // 打开文件夹
                    System.Diagnostics.Process.Start("explorer.exe", treeNode.Path);
                }
            }
            catch (Exception ex)
            {
                _viewModel.StatusText = $"打开文件夹失败: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// DWG文件网格右键菜单 - 在文件夹中显示
    /// </summary>
    private void Grid_ShowInFolder(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && 
            menuItem.Parent is ContextMenu contextMenu &&
            contextMenu.PlacementTarget is Border border &&
            border.DataContext is TreeNodeDto dwgFile)
        {
            try
            {
                if (File.Exists(dwgFile.Path))
                {
                    // 在文件资源管理器中选中文件
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{dwgFile.Path}\"");
                    _viewModel.StatusText = $"已在文件夹中显示: {dwgFile.Name}";
                }
                else
                {
                    _viewModel.StatusText = $"文件不存在: {dwgFile.Path}";
                }
            }
            catch (Exception ex)
            {
                _viewModel.StatusText = $"打开文件夹失败: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// DWG文件网格右键菜单 - 预览
    /// </summary>
    private void Grid_Preview(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && 
            menuItem.Parent is ContextMenu contextMenu &&
            contextMenu.PlacementTarget is Border border &&
            border.DataContext is TreeNodeDto dwgFile)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Grid_Preview被调用: {dwgFile.Name}");
            
            // 调用ViewModel的预览命令
            _viewModel.SelectDwgFileCommand?.Execute(dwgFile);
        }
    }

    /// <summary>
    /// DWG文件网格右键菜单 - 插入到CAD
    /// </summary>
    private void Grid_InsertToCad(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && 
            menuItem.Parent is ContextMenu contextMenu &&
            contextMenu.PlacementTarget is Border border &&
            border.DataContext is TreeNodeDto dwgFile)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Grid_InsertToCad被调用: {dwgFile.Name}");
            
            // 调用ViewModel的插入CAD命令
            _viewModel.InsertToCadCommand?.Execute(dwgFile);
        }
    }

    /// <summary>
    /// DWG文件网格双击事件处理 - 插入到CAD
    /// </summary>
    private void DwgFilesGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is TreeNodeDto selectedFile)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] DwgFilesGrid双击: {selectedFile.Name}");
            
            // 双击插入到CAD
            if (selectedFile.Type == "file" && selectedFile.IconType == "dwg")
            {
                _viewModel.InsertToCadCommand?.Execute(selectedFile);
            }
        }
    }

    /// <summary>
    /// 提示按钮点击事件
    /// </summary>
    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        var helpWindow = new HelpWindow
        {
            Owner = this
        };
        helpWindow.ShowDialog();
    }

    /// <summary>
    /// 设置按钮点击事件
    /// </summary>
    private async void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var settingsService = ((App)Application.Current).Services.GetRequiredService<ISettingsService>();
            var settingsWindow = new SettingsWindow(settingsService)
            {
                Owner = this
            };

            if (settingsWindow.ShowDialog() == true)
            {
                // 设置已保存，重新加载数据
                if (_viewModel.RefreshCommand.CanExecute(null))
                {
                    _viewModel.RefreshCommand.Execute(null);
                }
                
                // 设置已保存，无需应用窗口设置
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"打开设置窗口失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    /// <summary>
    /// 最小化按钮点击事件
    /// </summary>
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// 最大化/还原按钮点击事件
    /// </summary>
    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        // 简单的窗口状态切换，让系统处理所有细节
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// 标题栏鼠标按下事件 - 处理拖动和双击
    /// </summary>
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            if (e.ClickCount == 2)
            {
                // 双击 - 最大化/还原
                MaximizeButton_Click(sender, new RoutedEventArgs());
            }
            else if (e.ClickCount == 1)
            {
                // 单击 - 拖动窗口
                DragMove();
            }
        }
    }

}