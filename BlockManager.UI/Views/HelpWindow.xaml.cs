using System.Windows;
using System.Windows.Input;

namespace BlockManager.UI.Views;

/// <summary>
/// HelpWindow.xaml 的交互逻辑
/// </summary>
public partial class HelpWindow : Window
{
    public HelpWindow()
    {
        InitializeComponent();
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
    /// 标题栏关闭按钮点击事件
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// 底部确认按钮点击事件
    /// </summary>
    private void ConfirmButton_Click(object sender, MouseButtonEventArgs e)
    {
        Close();
    }
}
