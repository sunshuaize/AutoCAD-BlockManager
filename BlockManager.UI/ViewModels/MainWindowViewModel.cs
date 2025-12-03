using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BlockManager.IPC.Contracts;
using BlockManager.IPC.DTOs;

namespace BlockManager.UI.ViewModels
{
    /// <summary>
    /// 主窗口ViewModel
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IBlockManagerClient _client;
        private TreeNodeDto? _rootNode;
        private TreeNodeDto? _selectedNode;
        private PreviewDto? _currentPreview;
        private string _statusText = "就绪";
        private bool _isLoading;

        public MainWindowViewModel(IBlockManagerClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            
            // 初始化命令
            LoadLibraryCommand = new AsyncRelayCommand(LoadLibraryAsync);
            FileDoubleClickCommand = new AsyncRelayCommand<TreeNodeDto>(HandleFileDoubleClickAsync);
            RefreshCommand = new AsyncRelayCommand(RefreshLibraryAsync);
            
            // 订阅文件变化事件
            _client.FileChanged += OnFileChanged;
        }

        #region 属性

        /// <summary>
        /// 根节点
        /// </summary>
        public TreeNodeDto? RootNode
        {
            get => _rootNode;
            set => SetProperty(ref _rootNode, value);
        }

        /// <summary>
        /// 选中的节点
        /// </summary>
        public TreeNodeDto? SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (SetProperty(ref _selectedNode, value))
                {
                    _ = LoadPreviewAsync(value);
                }
            }
        }

        /// <summary>
        /// 当前预览
        /// </summary>
        public PreviewDto? CurrentPreview
        {
            get => _currentPreview;
            set => SetProperty(ref _currentPreview, value);
        }

        /// <summary>
        /// 状态文本
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        #endregion

        #region 命令

        /// <summary>
        /// 加载库命令
        /// </summary>
        public ICommand LoadLibraryCommand { get; }

        /// <summary>
        /// 文件双击命令
        /// </summary>
        public ICommand FileDoubleClickCommand { get; }

        /// <summary>
        /// 刷新命令
        /// </summary>
        public ICommand RefreshCommand { get; }

        #endregion

        #region 私有方法

        /// <summary>
        /// 加载块库
        /// </summary>
        private async Task LoadLibraryAsync()
        {
            try
            {
                IsLoading = true;
                StatusText = "正在连接到CAD进程...";

                // 尝试连接到不同版本的AutoCAD
                if (!_client.IsConnected)
                {
                    await ConnectToAvailableServerAsync();
                }

                StatusText = "正在加载块库...";
                var rootPath = @"c:\Users\PC\Desktop\BlockManager\Block";
                StatusText = $"[调试] 请求加载路径: {rootPath}";
                
                RootNode = await _client.GetBlockLibraryTreeAsync(rootPath);
                StatusText = $"已加载块库: {rootPath} (节点数: {RootNode?.Children?.Count ?? 0})";
            }
            catch (Exception ex)
            {
                StatusText = $"连接失败: {ex.Message}";
                
                // 提供测试模式的提示
                if (ex.Message.Contains("无法连接到CAD进程") || ex.Message.Contains("All pipe instances are busy"))
                {
                    StatusText += "\n\n💡 提示：要测试完整功能，请：\n1. 启动AutoCAD\n2. 加载BlockManager插件\n3. 执行BLOCKVIEWER命令\n\n🔍 调试信息：\n- 检查AutoCAD是否运行\n- 检查插件是否加载\n- 检查IPC服务器是否启动";
                }
                
                RootNode = null;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 尝试连接到可用的服务器
        /// </summary>
        private async Task ConnectToAvailableServerAsync()
        {
            var pipeNames = new[] { "BlockManager_IPC_2024", "BlockManager_IPC" };
            
            foreach (var pipeName in pipeNames)
            {
                try
                {
                    StatusText = $"尝试连接到: {pipeName}";
                    
                    // 创建新的客户端实例
                    var testClient = new BlockManager.IPC.Client.NamedPipeClient(pipeName);
                    await testClient.ConnectAsync();
                    
                    if (testClient.IsConnected)
                    {
                        StatusText = $"成功连接到: {pipeName}";
                        
                        // 如果当前客户端使用不同的管道，需要重新创建
                        if (_client is BlockManager.IPC.Client.NamedPipeClient currentClient)
                        {
                            // 这里需要更新客户端引用，但由于架构限制，我们先记录成功的管道
                            testClient.Dispose();
                            await _client.ConnectAsync();
                            return;
                        }
                    }
                    
                    testClient.Dispose();
                }
                catch (Exception ex)
                {
                    StatusText = $"连接 {pipeName} 失败: {ex.Message}";
                    // 继续尝试下一个管道
                }
            }
            
            // 如果所有管道都失败，使用默认客户端
            await _client.ConnectAsync();
        }

        /// <summary>
        /// 刷新块库
        /// </summary>
        private async Task RefreshLibraryAsync()
        {
            CurrentPreview = null;
            await LoadLibraryAsync();
        }

        /// <summary>
        /// 加载预览
        /// </summary>
        /// <param name="node">节点</param>
        private async Task LoadPreviewAsync(TreeNodeDto? node)
        {
            if (node?.Type != "file")
            {
                CurrentPreview = null;
                StatusText = node?.Type == "folder" ? $"文件夹: {node.Name}" : "就绪";
                return;
            }

            try
            {
                StatusText = $"正在加载预览: {node.Name}";
                CurrentPreview = await _client.GetFilePreviewAsync(node.Path);

                if (CurrentPreview.IsSuccess && CurrentPreview.Metadata != null)
                {
                    var fileInfo = CurrentPreview.Metadata;
                    StatusText = $"文件: {fileInfo.Name} | 大小: {FormatFileSize(fileInfo.Size)} | 修改时间: {fileInfo.LastModified:yyyy-MM-dd HH:mm}";
                }
                else
                {
                    StatusText = $"预览加载失败: {CurrentPreview.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"预览加载错误: {ex.Message}";
                CurrentPreview = null;
            }
        }

        /// <summary>
        /// 处理文件双击
        /// </summary>
        /// <param name="node">节点</param>
        private async Task HandleFileDoubleClickAsync(TreeNodeDto? node)
        {
            if (node?.Type != "file" || string.IsNullOrEmpty(node.Path))
                return;

            try
            {
                StatusText = $"正在插入块: {node.Name}";

                var request = new InsertBlockRequest
                {
                    BlockPath = node.Path,
                    BlockName = System.IO.Path.GetFileNameWithoutExtension(node.Name)
                };

                bool success = await _client.InsertBlockAsync(request);
                
                if (success)
                {
                    StatusText = $"已成功插入块: {node.Name}";
                }
                else
                {
                    StatusText = $"插入块失败: {node.Name}";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"插入块时发生错误: {ex.Message}";
            }
        }

        /// <summary>
        /// 文件变化事件处理
        /// </summary>
        private async void OnFileChanged(object? sender, FileChangedEventArgs e)
        {
            // 在UI线程上更新状态
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                StatusText = $"文件已{e.ChangeType}: {System.IO.Path.GetFileName(e.FilePath)}";
            });

            // 如果需要，可以在这里刷新文件树
            // await RefreshLibraryAsync();
        }

        /// <summary>
        /// 格式化文件大小
        /// </summary>
        /// <param name="bytes">字节数</param>
        /// <returns>格式化的大小字符串</returns>
        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        #endregion
    }
}
