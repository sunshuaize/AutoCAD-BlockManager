using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using BlockManager.UI.Models;
using BlockManager.UI.Services;
using System.IO;

namespace BlockManager.UI.Views;

/// <summary>
/// SettingsWindow.xaml 的交互逻辑
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly ISettingsService _settingsService;
    private AppSettings _currentSettings;

    public SettingsWindow(ISettingsService settingsService)
    {
        InitializeComponent();
        _settingsService = settingsService;
        _currentSettings = new AppSettings();
        
        Loaded += SettingsWindow_Loaded;
    }

    private async void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _currentSettings = await _settingsService.LoadSettingsAsync();
            LoadSettingsToUI();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载设置失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadSettingsToUI()
    {
        BlockPathTextBox.Text = _currentSettings.BlockLibraryPath;
    }

    private bool ValidateAndSaveSettings()
    {
        try
        {
            // 验证块库路径
            var blockPath = BlockPathTextBox.Text.Trim();
            if (string.IsNullOrEmpty(blockPath))
            {
                MessageBox.Show("请设置块库路径", "验证失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                BlockPathTextBox.Focus();
                return false;
            }

            if (!Directory.Exists(blockPath))
            {
                var result = MessageBox.Show($"目录 '{blockPath}' 不存在，是否创建？", "确认", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Directory.CreateDirectory(blockPath);
                }
                else
                {
                    return false;
                }
            }

            // 保存设置
            _currentSettings.BlockLibraryPath = blockPath;

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"验证设置时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    /// <summary>
    /// 标题栏拖拽移动窗口
    /// </summary>
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    /// <summary>
    /// 浏览按钮点击事件
    /// </summary>
    private void BrowseButton_Click(object sender, MouseButtonEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择块库目录",
            Filter = "文件夹|*.folder",
            FileName = "选择文件夹",
            ValidateNames = false,
            CheckFileExists = false,
            CheckPathExists = true,
            InitialDirectory = string.IsNullOrEmpty(BlockPathTextBox.Text) 
                ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                : BlockPathTextBox.Text
        };

        // 使用一个技巧：让用户选择文件夹而不是文件
        if (dialog.ShowDialog() == true)
        {
            var selectedPath = Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrEmpty(selectedPath))
            {
                BlockPathTextBox.Text = selectedPath;
            }
        }
    }

    /// <summary>
    /// 取消按钮点击事件
    /// </summary>
    private void CancelButton_Click(object sender, MouseButtonEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    /// <summary>
    /// 保存按钮点击事件
    /// </summary>
    private async void SaveButton_Click(object sender, MouseButtonEventArgs e)
    {
        if (!ValidateAndSaveSettings())
            return;

        try
        {
            await _settingsService.SaveSettingsAsync(_currentSettings);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存设置失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 获取当前设置
    /// </summary>
    public AppSettings GetCurrentSettings()
    {
        return _currentSettings;
    }
}
