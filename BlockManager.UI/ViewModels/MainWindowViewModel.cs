using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BlockManager.IPC.Client;
using BlockManager.IPC.DTOs;
using BlockManager.UI.Services;
using BlockManager.UI.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlockManager.IPC.Contracts;

namespace BlockManager.UI.ViewModels
{
    /// <summary>
    /// 主窗口ViewModel
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IBlockManagerClient _client;
        private readonly ISettingsService _settingsService;
        private TreeNodeDto? _rootNode;
        private TreeNodeDto? _selectedNode;
        private PreviewDto? _currentPreview;
        private string _statusText = "就绪";
        private bool _isLoading;
        private string _connectionStatus = "未连接";
        private string _connectionStatusColor = "#EF4444";
        private ObservableCollection<TreeNodeDto> _currentFolderFiles = new();
        private bool _showDefaultHint = true;
        private bool _showGrid = false;
        private string _searchText = string.Empty;
        private ObservableCollection<TreeNodeDto> _searchResults = new();
        private bool _isSearchMode = false;
        private TreeNodeDto? _selectedFile;

        public MainWindowViewModel(IBlockManagerClient client, ISettingsService settingsService)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            
            
            // 初始化命令
            LoadLibraryCommand = new AsyncRelayCommand(LoadLibraryAsync);
            FileDoubleClickCommand = new AsyncRelayCommand<TreeNodeDto>(HandleFileDoubleClickAsync);
            RefreshCommand = new AsyncRelayCommand(RefreshLibraryAsync);
            SelectDwgFileCommand = new AsyncRelayCommand<TreeNodeDto>(SelectDwgFileAsync);
            InsertToCadCommand = new AsyncRelayCommand<TreeNodeDto>(InsertToCadAsync);
            SearchCommand = new RelayCommand<string>(ExecuteSearch);
            ClearSearchCommand = new RelayCommand(ClearSearch);
            
            // 订阅文件变化事件
            _client.FileChanged += OnFileChanged;
            
            // 使用后台任务启动自动加载，避免阻塞UI
            Task.Run(async () =>
            {
                try
                {
                    // 等待UI完全初始化
                    await Task.Delay(3000);
                    
                    // 触发自动加载
                    await TriggerAutoLoadAsync();
                }
                catch
                {
                    // 自动加载失败，用户可以手动点击加载按钮
                }
            });
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
                    if (value?.Type == "folder")
                    {
                        // 选择文件夹时，显示网格
                        UpdateCurrentFolderFiles();
                        CurrentPreview = null;
                        SelectedFile = null; // 清除选中的文件
                        ShowDefaultHint = false;
                        ShowGrid = true;
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] 切换到文件夹网格模式: {value.Name}");
                    }
                    else if (value?.Type == "file" && value?.IconType == "dwg")
                    {
                        // 选择DWG文件时，定位到其父文件夹并在网格中选中该文件
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] TreeView选择DWG文件，准备定位到网格: {value.Name}");
                        NavigateToFileInGrid(value);
                    }
                    else
                    {
                        // 其他情况显示默认提示
                        CurrentFolderFiles.Clear();
                        CurrentPreview = null;
                        ShowDefaultHint = true;
                        ShowGrid = false;
                    }
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

        /// <summary>
        /// 连接状态文本
        /// </summary>
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        /// <summary>
        /// 连接状态颜色
        /// </summary>
        public string ConnectionStatusColor
        {
            get => _connectionStatusColor;
            set => SetProperty(ref _connectionStatusColor, value);
        }

        /// <summary>
        /// 当前文件夹中的DWG文件
        /// </summary>
        public ObservableCollection<TreeNodeDto> CurrentFolderFiles
        {
            get => _currentFolderFiles;
            set => SetProperty(ref _currentFolderFiles, value);
        }

        /// <summary>
        /// 是否显示默认提示
        /// </summary>
        public bool ShowDefaultHint
        {
            get => _showDefaultHint;
            set => SetProperty(ref _showDefaultHint, value);
        }

        /// <summary>
        /// 是否显示网格
        /// </summary>
        public bool ShowGrid
        {
            get => _showGrid;
            set => SetProperty(ref _showGrid, value);
        }

        /// <summary>
        /// 搜索文本
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    // 实时搜索
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        ClearSearch();
                    }
                    else
                    {
                        ExecuteSearch(value);
                    }
                }
            }
        }

        /// <summary>
        /// 搜索结果
        /// </summary>
        public ObservableCollection<TreeNodeDto> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        /// <summary>
        /// 是否处于搜索模式
        /// </summary>
        public bool IsSearchMode
        {
            get => _isSearchMode;
            set => SetProperty(ref _isSearchMode, value);
        }

        /// <summary>
        /// 选中的文件
        /// </summary>
        public TreeNodeDto? SelectedFile
        {
            get => _selectedFile;
            set
            {
                if (SetProperty(ref _selectedFile, value))
                {
                    // 当选中文件时，可以触发预览或其他操作
                    if (value != null)
                    {
                        StatusText = $"已选中: {value.Name}";
                        // 这里可以添加其他选中后的操作，比如自动预览
                        // _ = Task.Run(() => SelectDwgFileAsync(value));
                    }
                }
            }
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

        /// <summary>
        /// 选择DWG文件命令
        /// </summary>
        public ICommand SelectDwgFileCommand { get; }

        /// <summary>
        /// 插入到CAD命令
        /// </summary>
        public ICommand InsertToCadCommand { get; }

        /// <summary>
        /// 搜索命令
        /// </summary>
        public ICommand SearchCommand { get; }

        /// <summary>
        /// 清空搜索命令
        /// </summary>
        public ICommand ClearSearchCommand { get; }

        #endregion

        #region 公共方法

        /// <summary>
        /// 触发自动加载（由MainWindow在加载完成后调用）
        /// </summary>
        public async Task TriggerAutoLoadAsync()
        {
            await InitializeAsync();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 初始化ViewModel，自动加载块文件夹
        /// </summary>
        private async Task InitializeAsync()
        {
            try
            {
                // 先加载本地目录
                StatusText = "正在加载块文件夹...";
                await LoadLocalDirectoryAsync();
                
                // 然后尝试连接IPC（用于状态显示）
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(1000); // 延迟一秒再尝试连接
                        await CheckIpcConnectionAsync();
                    }
                    catch
                    {
                        // IPC连接失败不影响文件加载
                    }
                });
            }
            catch (Exception ex)
            {
                StatusText = $"加载失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 加载本地目录
        /// </summary>
        private async Task LoadLocalDirectoryAsync()
        {
            await Task.Run(async () =>
            {
                var settings = await _settingsService.LoadSettingsAsync();
                var rootPath = settings.BlockLibraryPath;
                
                if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
                {
                    throw new DirectoryNotFoundException($"块库目录不存在: {rootPath}，请在设置中配置正确的路径");
                }

                StatusText = "正在扫描本地目录...";
                
                var rootNode = new TreeNodeDto
                {
                    Name = "Block",
                    Path = rootPath,
                    Type = "folder",
                    IconType = "folder"
                };

                LoadDirectoryRecursive(rootNode, rootPath);
                
                RootNode = rootNode;
                StatusText = $"已加载本地目录 (节点数: {rootNode.Children.Count})";
                ConnectionStatus = "本地模式";
                ConnectionStatusColor = "#6B7280"; // 灰色表示本地模式
            });
        }

        /// <summary>
        /// 递归加载目录结构
        /// </summary>
        private void LoadDirectoryRecursive(TreeNodeDto parentNode, string directoryPath)
        {
            try
            {
                // 加载子目录
                var directories = Directory.GetDirectories(directoryPath)
                    .Where(d => !Path.GetFileName(d).StartsWith(".")) // 忽略隐藏目录
                    .OrderBy(d => Path.GetFileName(d));

                foreach (var dir in directories)
                {
                    var dirNode = new TreeNodeDto
                    {
                        Name = Path.GetFileName(dir),
                        Path = dir,
                        Type = "folder",
                        IconType = "folder"
                    };

                    LoadDirectoryRecursive(dirNode, dir);
                    parentNode.Children.Add(dirNode);
                }

                // 加载文件
                var files = Directory.GetFiles(directoryPath)
                    .Where(f => IsValidBlockFile(f))
                    .OrderBy(f => Path.GetFileName(f));

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var extension = Path.GetExtension(file).ToLowerInvariant();
                    
                    var fileNode = new TreeNodeDto
                    {
                        Name = fileName,
                        Path = file,
                        Type = "file",
                        IconType = GetIconType(extension),
                        FileInfo = new FileInfoDto
                        {
                            Name = fileName,
                            Size = new System.IO.FileInfo(file).Length,
                            LastModified = new System.IO.FileInfo(file).LastWriteTime
                        }
                    };

                    parentNode.Children.Add(fileNode);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // 忽略无权限访问的目录
            }
            catch (Exception)
            {
                // 忽略其他错误，继续处理其他目录
            }
        }

        /// <summary>
        /// 判断是否为有效的块文件
        /// </summary>
        private bool IsValidBlockFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".dwg";
        }

        /// <summary>
        /// 根据文件扩展名获取图标类型
        /// </summary>
        private string GetIconType(string extension)
        {
            return extension switch
            {
                ".dwg" => "dwg",
                _ => "file"
            };
        }

        /// <summary>
        /// 检查IPC连接状态
        /// </summary>
        private async Task CheckIpcConnectionAsync()
        {
            try
            {
                StatusText = "正在检查CAD连接...";
                
                if (!_client.IsConnected)
                {
                    await _client.ConnectAsync();
                }
                
                if (_client.IsConnected)
                {
                    ConnectionStatus = "已连接";
                    ConnectionStatusColor = "#10B981"; // 绿色
                    StatusText = "CAD连接正常，文件已加载";
                }
                else
                {
                    throw new Exception("无法建立连接");
                }
            }
            catch (Exception ex)
            {
                ConnectionStatus = "未连接";
                ConnectionStatusColor = "#EF4444"; // 红色
                StatusText = $"CAD未连接: {ex.Message}";
            }
        }

        /// <summary>
        /// 带重试机制的加载块库
        /// </summary>
        private async Task LoadLibraryWithRetryAsync()
        {
            const int maxRetries = 5;
            const int retryDelayMs = 1000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    StatusText = $"正在连接... (第{attempt}次)";
                    
                    await LoadLibraryAsync();
                    
                    return; // 成功则退出
                }
                catch (Exception ex)
                {
                    if (attempt == maxRetries)
                    {
                        // 最后一次尝试失败
                        StatusText = $"连接失败: {ex.Message}";
                        ConnectionStatus = "连接失败";
                        ConnectionStatusColor = "#EF4444";
                        
                        if (ex.Message.Contains("无法连接到CAD进程") || ex.Message.Contains("All pipe instances are busy"))
                        {
                            StatusText += "\n\n💡 提示：要测试完整功能，请：\n1. 启动AutoCAD\n2. 加载BlockManager插件\n3. 执行BLOCKVIEWER命令";
                        }
                        
                        throw;
                    }
                    else
                    {
                        // 等待后重试
                        StatusText = $"连接失败，{retryDelayMs/1000}秒后重试...";
                        await Task.Delay(retryDelayMs);
                    }
                }
            }
        }

        /// <summary>
        /// 加载块库
        /// </summary>
        private async Task LoadLibraryAsync()
        {
            try
            {
                IsLoading = true;
                
                // 直接加载本地目录
                await LoadLocalDirectoryAsync();
                
                // 尝试检查IPC连接状态
                try
                {
                    await CheckIpcConnectionAsync();
                }
                catch
                {
                    // IPC连接失败不影响文件加载，保持本地模式状态
                }
            }
            catch (Exception)
            {
                // 重置连接状态
                ConnectionStatus = "加载失败";
                ConnectionStatusColor = "#EF4444";
                RootNode = null;
                
                // 重新抛出异常，让重试机制处理
                throw;
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
            var pipeNames = new[] {  "BlockManager_IPC" };
            
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
                StatusText = $"文件已更新: {e.FilePath}";
            });
        }

        /// <summary>
        /// 执行搜索
        /// </summary>
        private void ExecuteSearch(string? searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText) || RootNode == null)
            {
                ClearSearch();
                return;
            }

            try
            {
                var results = new List<TreeNodeDto>();
                SearchInNode(RootNode, searchText.Trim(), results);

                SearchResults.Clear();
                foreach (var result in results.Take(50)) // 限制结果数量
                {
                    SearchResults.Add(result);
                }

                IsSearchMode = SearchResults.Count > 0;
                StatusText = $"找到 {SearchResults.Count} 个匹配项";
            }
            catch (Exception ex)
            {
                StatusText = $"搜索失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 在节点中递归搜索
        /// </summary>
        private void SearchInNode(TreeNodeDto node, string searchText, List<TreeNodeDto> results)
        {
            // 搜索当前节点，但只添加文件类型的节点（过滤掉文件夹）
            if (IsMatch(node, searchText) && node.Type == "file")
            {
                results.Add(node);
            }

            // 递归搜索子节点
            foreach (var child in node.Children)
            {
                SearchInNode(child, searchText, results);
            }
        }

        /// <summary>
        /// 判断节点是否匹配搜索条件
        /// </summary>
        private bool IsMatch(TreeNodeDto node, string searchText)
        {
            if (string.IsNullOrEmpty(node.Name))
                return false;

            // 文件名匹配（不区分大小写）
            if (node.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                return true;

            // 路径匹配
            if (!string.IsNullOrEmpty(node.Path) && 
                node.Path.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        /// <summary>
        /// 清空搜索
        /// </summary>
        private void ClearSearch()
        {
            SearchResults.Clear();
            IsSearchMode = false;
            if (!string.IsNullOrEmpty(SearchText))
            {
                StatusText = "已清空搜索";
            }
        }

        /// <summary>
        /// 更新当前文件夹文件列表
        /// </summary>
        private void UpdateCurrentFolderFiles()
        {
            CurrentFolderFiles.Clear();
            
            if (SelectedNode?.Type == "folder" && SelectedNode.Children != null)
            {
                var dwgFiles = SelectedNode.Children
                    .Where(child => child.Type == "file" && child.IconType == "dwg")
                    .ToList();

                foreach (var file in dwgFiles)
                {
                    // 查找对应的PNG预览图
                    var pngPath = Path.ChangeExtension(file.Path, ".png");
                    if (File.Exists(pngPath))
                    {
                        file.PreviewImagePath = pngPath;
                    }
                    
                    CurrentFolderFiles.Add(file);
                }
            }
        }

        /// <summary>
        /// 选择DWG文件
        /// </summary>
        private async Task SelectDwgFileAsync(TreeNodeDto? dwgFile)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] SelectDwgFileAsync被调用，参数: {dwgFile?.Name ?? "null"}");
            
            if (dwgFile == null) 
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] dwgFile为null，退出方法");
                StatusText = "错误: 未选择文件";
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SelectDwgFileAsync开始处理: {dwgFile.Name}");
                
                // 设置选中的文件
                SelectedFile = dwgFile;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SelectedFile已设置: {SelectedFile?.Name}");
                
                // 查找对应的PNG预览图
                var pngPath = Path.ChangeExtension(dwgFile.Path, ".png");
                StatusText = $"正在加载预览: {dwgFile.Name}";
                System.Diagnostics.Debug.WriteLine($"[DEBUG] PNG路径: {pngPath}, 存在: {File.Exists(pngPath)}");
                
                // 创建预览数据（无论是否有PNG图片）
                var previewData = new PreviewDto
                {
                    FileName = dwgFile.Name,
                    FilePath = dwgFile.Path,
                    PreviewImagePath = File.Exists(pngPath) ? pngPath : null,
                    FileSize = dwgFile.FileInfo?.Size ?? 0,
                    LastModified = dwgFile.FileInfo?.LastModified ?? DateTime.MinValue,
                    IsSuccess = File.Exists(pngPath),
                    ErrorMessage = File.Exists(pngPath) ? null : "未找到对应的PNG预览图"
                };

                CurrentPreview = previewData;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] CurrentPreview已设置: {CurrentPreview != null}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] PreviewImagePath: '{CurrentPreview.PreviewImagePath}'");
                
                if (File.Exists(pngPath))
                {
                    StatusText = $"预览已加载: {dwgFile.Name}";
                }
                else
                {
                    StatusText = $"已选中文件: {dwgFile.Name} (无预览图)";
                }
                
                // 切换到预览模式
                ShowGrid = false;
                ShowDefaultHint = false;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] 视图状态 - ShowGrid: {ShowGrid}, ShowDefaultHint: {ShowDefaultHint}");
            }
            catch (Exception ex)
            {
                StatusText = $"加载预览失败: {ex.Message}";
                CurrentPreview = null;
            }
        }

        /// <summary>
        /// 插入到CAD
        /// </summary>
        private async Task InsertToCadAsync(TreeNodeDto? dwgFile)
        {
            if (dwgFile == null) 
            {
                StatusText = "错误: 未选择文件";
                return;
            }

            try
            {
                StatusText = $"正在插入到CAD: {dwgFile.Name}";
                
                // 检查文件是否存在
                if (!File.Exists(dwgFile.Path))
                {
                    StatusText = $"错误: 文件不存在 - {dwgFile.Path}";
                    return;
                }
                
                // 检查客户端连接状态
                if (!_client.IsConnected)
                {
                    StatusText = "正在尝试连接CAD...";
                    await _client.ConnectAsync();
                    
                    if (!_client.IsConnected)
                    {
                        StatusText = "错误: 无法连接到CAD，请确保CAD已启动并加载了BlockManager插件";
                        return;
                    }
                }
                
                // 发送插入命令到CAD
                var insertRequest = new InsertBlockRequest
                {
                    BlockPath = dwgFile.Path,
                    BlockName = Path.GetFileNameWithoutExtension(dwgFile.Name)
                };

                StatusText = $"正在发送插入命令...";
                var result = await _client.InsertBlockAsync(insertRequest);
                
                if (result)
                {
                    StatusText = $"✅ 已成功插入到CAD: {dwgFile.Name}";
                }
                else
                {
                    StatusText = $"❌ 插入失败: {dwgFile.Name} (CAD返回失败)";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"❌ 插入到CAD失败: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"InsertToCadAsync异常: {ex}");
            }
        }

        /// <summary>
        /// 加载DWG文件预览
        /// </summary>
        private async Task LoadDwgPreviewAsync(TreeNodeDto dwgFile)
        {
            try
            {
                StatusText = $"正在加载预览: {dwgFile.Name}";
                
                // 查找对应的PNG预览图
                var pngPath = Path.ChangeExtension(dwgFile.Path, ".png");
                StatusText = $"查找PNG文件: {pngPath}";
                
                if (File.Exists(pngPath))
                {
                    // 创建预览数据
                    var previewData = new PreviewDto
                    {
                        FileName = dwgFile.Name,
                        FilePath = dwgFile.Path,
                        PreviewImagePath = pngPath,
                        FileSize = dwgFile.FileInfo?.Size ?? 0,
                        LastModified = dwgFile.FileInfo?.LastModified ?? DateTime.MinValue,
                        IsSuccess = true
                    };

                    CurrentPreview = previewData;
                    StatusText = $"已加载预览: {dwgFile.Name} -> {pngPath}";
                }
                else
                {
                    // 没有PNG预览图，创建基本信息
                    var previewData = new PreviewDto
                    {
                        FileName = dwgFile.Name,
                        FilePath = dwgFile.Path,
                        PreviewImagePath = null,
                        FileSize = dwgFile.FileInfo?.Size ?? 0,
                        LastModified = dwgFile.FileInfo?.LastModified ?? DateTime.MinValue,
                        IsSuccess = false,
                        ErrorMessage = "未找到对应的PNG预览图"
                    };

                    CurrentPreview = previewData;
                    StatusText = $"未找到预览图: {dwgFile.Name}";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"加载预览失败: {ex.Message}";
                CurrentPreview = null;
            }
        }

        /// <summary>
        /// 导航到文件在网格中的位置
        /// </summary>
        private void NavigateToFileInGrid(TreeNodeDto dwgFile)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] ViewModel导航到文件: {dwgFile.Name}");
                
                // 找到文件的父文件夹
                var parentFolder = FindParentFolder(dwgFile);
                if (parentFolder != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] 找到父文件夹: {parentFolder.Name}");
                    
                    // 选中父文件夹，这会触发网格显示
                    // 注意：这里需要避免递归调用，所以直接设置私有字段
                    _selectedNode = parentFolder;
                    OnPropertyChanged(nameof(SelectedNode));
                    
                    // 手动触发文件夹切换逻辑
                    UpdateCurrentFolderFiles();
                    CurrentPreview = null;
                    ShowDefaultHint = false;
                    ShowGrid = true;
                    
                    // 延迟选中目标文件
                    Task.Delay(100).ContinueWith(_ =>
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            // 在网格中选中目标文件
                            SelectedFile = dwgFile;
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] 已在网格中选中文件: {dwgFile.Name}");
                        });
                    });
                }
                else
                {
                    StatusText = $"无法找到文件 {dwgFile.Name} 的父文件夹";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"导航到文件失败: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[DEBUG] 导航失败: {ex}");
            }
        }

        /// <summary>
        /// 查找文件的父文件夹
        /// </summary>
        private TreeNodeDto? FindParentFolder(TreeNodeDto targetFile)
        {
            if (RootNode == null) return null;
            
            return FindParentFolderRecursive(RootNode, targetFile);
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
