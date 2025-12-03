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
using BlockManager.IPC.DTOs;
using BlockManager.UI.ViewModels;

namespace BlockManager.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
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
        if (DataContext is MainWindowViewModel viewModel && 
            FileTreeView.SelectedItem is TreeNodeDto selectedNode)
        {
            // 执行双击命令
            if (viewModel.FileDoubleClickCommand.CanExecute(selectedNode))
            {
                viewModel.FileDoubleClickCommand.Execute(selectedNode);
            }
        }
    }
}