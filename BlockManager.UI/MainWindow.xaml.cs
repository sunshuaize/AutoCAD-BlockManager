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
    /// TreeView双击事件处理
    /// </summary>
    private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is TreeView treeView && treeView.SelectedItem is TreeNodeDto selectedNode)
        {
            _viewModel.FileDoubleClickCommand?.Execute(selectedNode);
        }
    }

    private void DwgFile_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext is TreeNodeDto dwgFile)
        {
            _viewModel.SelectDwgFileCommand?.Execute(dwgFile);
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