using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BlockManager.Abstractions;

namespace BlockManager.Core
{
    public partial class BlockLibraryViewer : Form
    {
        private string blockRootPath;
        private IBlockLibraryService _blockLibraryService;

        public BlockLibraryViewer() : this(null)
        {
        }

        public BlockLibraryViewer(IBlockLibraryService blockLibraryService)
        {
            _blockLibraryService = blockLibraryService;
            InitializeComponent();
            
            // 直接使用绝对路径，避免相对路径问题
            blockRootPath = @"c:\Users\PC\Desktop\BlockManager\BlockManager.Core\Block";
            
            // 如果绝对路径不存在，尝试其他可能的位置
            if (!Directory.Exists(blockRootPath))
            {
                // 尝试从当前程序集位置推断
                string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string assemblyDir = Path.GetDirectoryName(assemblyPath);
                blockRootPath = Path.Combine(assemblyDir, "Block");
                
                if (!Directory.Exists(blockRootPath))
                {
                    // 最后的备选方案
                    blockRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
                        @"BlockManager\BlockManager.Core\Block");
                }
            }
            
            LoadBlockLibrary();
        }

        private ImageList CreateImageList()
        {
            try
            {
                var imageList = new ImageList();
                imageList.ImageSize = new Size(16, 16);
                imageList.ColorDepth = ColorDepth.Depth32Bit;
                
                // 添加文件夹图标
                using (var folderIcon = SystemIcons.Shield.ToBitmap())
                {
                    var resizedFolder = new Bitmap(folderIcon, 16, 16);
                    imageList.Images.Add("folder", resizedFolder);
                }

                // 添加 DWG 文件图标
                using (var dwgIcon = SystemIcons.Application.ToBitmap())
                {
                    var resizedDwg = new Bitmap(dwgIcon, 16, 16);
                    imageList.Images.Add("dwg", resizedDwg);
                }

                // 添加图片文件图标
                using (var imageIcon = SystemIcons.Information.ToBitmap())
                {
                    var resizedImage = new Bitmap(imageIcon, 16, 16);
                    imageList.Images.Add("image", resizedImage);
                }

                return imageList;
            }
            catch (Exception)
            {
                // 如果创建图标失败，返回一个空的 ImageList
                return new ImageList() { ImageSize = new Size(16, 16) };
            }
        }

        private void LoadBlockLibrary()
        {
            try
            {
                treeView.BeginUpdate();
                treeView.Nodes.Clear();

                if (!Directory.Exists(blockRootPath))
                {
                    var errorNode = new TreeNode($"路径不存在: {blockRootPath}");
                    errorNode.ImageKey = "folder";
                    errorNode.SelectedImageKey = "folder";
                    treeView.Nodes.Add(errorNode);
                    return;
                }

                var rootNode = new TreeNode("块库 (Block Library)");
                rootNode.ImageKey = "folder";
                rootNode.SelectedImageKey = "folder";
                rootNode.Tag = blockRootPath;

                LoadDirectoryNodes(rootNode, blockRootPath);
                
                treeView.Nodes.Add(rootNode);
                rootNode.Expand();

                statusLabel.Text = $"已加载块库: {blockRootPath}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载块库时发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = $"加载失败: {ex.Message}";
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        private void LoadDirectoryNodes(TreeNode parentNode, string directoryPath)
        {
            try
            {
                // 添加子文件夹
                var subdirectories = Directory.GetDirectories(directoryPath);
                foreach (var subdir in subdirectories.OrderBy(d => d))
                {
                    var dirInfo = new DirectoryInfo(subdir);
                    var dirNode = new TreeNode(dirInfo.Name);
                    dirNode.ImageKey = "folder";
                    dirNode.SelectedImageKey = "folder";
                    dirNode.Tag = subdir;

                    LoadDirectoryNodes(dirNode, subdir);
                    parentNode.Nodes.Add(dirNode);
                }

                // 添加文件
                var files = Directory.GetFiles(directoryPath);
                foreach (var file in files.OrderBy(f => f))
                {
                    var fileInfo = new FileInfo(file);
                    var fileNode = new TreeNode(fileInfo.Name);
                    fileNode.Tag = file;

                    // 根据文件扩展名设置图标
                    string extension = fileInfo.Extension.ToLower();
                    switch (extension)
                    {
                        case ".dwg":
                            fileNode.ImageKey = "dwg";
                            fileNode.SelectedImageKey = "dwg";
                            break;
                        case ".png":
                        case ".jpg":
                        case ".jpeg":
                        case ".bmp":
                        case ".gif":
                            fileNode.ImageKey = "image";
                            fileNode.SelectedImageKey = "image";
                            break;
                        default:
                            fileNode.ImageKey = "dwg";
                            fileNode.SelectedImageKey = "dwg";
                            break;
                    }

                    parentNode.Nodes.Add(fileNode);
                }
            }
            catch (Exception ex)
            {
                var errorNode = new TreeNode($"错误: {ex.Message}");
                errorNode.ImageKey = "folder";
                errorNode.SelectedImageKey = "folder";
                parentNode.Nodes.Add(errorNode);
            }
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                var selectedPath = e.Node.Tag as string;
                if (string.IsNullOrEmpty(selectedPath))
                {
                    pictureBox.Image = null;
                    statusLabel.Text = "未选择文件";
                    return;
                }

                if (Directory.Exists(selectedPath))
                {
                    // 选择的是文件夹
                    pictureBox.Image = null;
                    var fileCount = Directory.GetFiles(selectedPath, "*", SearchOption.AllDirectories).Length;
                    var dirCount = Directory.GetDirectories(selectedPath, "*", SearchOption.AllDirectories).Length;
                    statusLabel.Text = $"文件夹: {selectedPath} | 包含 {fileCount} 个文件, {dirCount} 个子文件夹";
                }
                else if (File.Exists(selectedPath))
                {
                    // 选择的是文件
                    var fileInfo = new FileInfo(selectedPath);
                    string extension = fileInfo.Extension.ToLower();

                    if (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || 
                        extension == ".bmp" || extension == ".gif")
                    {
                        // 显示图片预览
                        try
                        {
                            using (var fs = new FileStream(selectedPath, FileMode.Open, FileAccess.Read))
                            {
                                pictureBox.Image = Image.FromStream(fs);
                            }
                            statusLabel.Text = $"图片预览: {fileInfo.Name} | 大小: {FormatFileSize(fileInfo.Length)}";
                        }
                        catch (Exception ex)
                        {
                            pictureBox.Image = null;
                            statusLabel.Text = $"无法加载图片: {ex.Message}";
                        }
                    }
                    else if (extension == ".dwg")
                    {
                        // DWG 文件，尝试查找对应的 PNG 预览图
                        var pngPath = Path.ChangeExtension(selectedPath, ".png");
                        if (File.Exists(pngPath))
                        {
                            try
                            {
                                using (var fs = new FileStream(pngPath, FileMode.Open, FileAccess.Read))
                                {
                                    pictureBox.Image = Image.FromStream(fs);
                                }
                                statusLabel.Text = $"DWG 文件预览: {fileInfo.Name} | 大小: {FormatFileSize(fileInfo.Length)} | 预览图: {Path.GetFileName(pngPath)}";
                            }
                            catch (Exception ex)
                            {
                                pictureBox.Image = null;
                                statusLabel.Text = $"DWG 文件: {fileInfo.Name} | 大小: {FormatFileSize(fileInfo.Length)} | 预览图加载失败: {ex.Message}";
                            }
                        }
                        else
                        {
                            pictureBox.Image = null;
                            statusLabel.Text = $"DWG 文件: {fileInfo.Name} | 大小: {FormatFileSize(fileInfo.Length)} | 无预览图";
                        }
                    }
                    else
                    {
                        pictureBox.Image = null;
                        statusLabel.Text = $"文件: {fileInfo.Name} | 大小: {FormatFileSize(fileInfo.Length)}";
                    }
                }
            }
            catch (Exception ex)
            {
                pictureBox.Image = null;
                statusLabel.Text = $"选择文件时发生错误: {ex.Message}";
            }
        }

        private string FormatFileSize(long bytes)
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

        private void TreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                var selectedPath = e.Node.Tag as string;
                statusLabel.Text = $"双击节点: {e.Node.Text}";
                
                if (string.IsNullOrEmpty(selectedPath) || !File.Exists(selectedPath))
                {
                    statusLabel.Text = $"文件不存在或路径无效: {selectedPath}";
                    return;
                }

                var fileInfo = new FileInfo(selectedPath);
                string extension = fileInfo.Extension.ToLower();

                // 只处理 DWG 文件的双击
                if (extension == ".dwg")
                {
                    statusLabel.Text = $"正在处理 DWG 文件: {fileInfo.Name}";
                    InsertBlockFromDwg(selectedPath);
                }
                else
                {
                    statusLabel.Text = $"不支持的文件类型: {extension}";
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"双击处理时发生错误: {ex.Message}";
            }
        }

        private void InsertBlockFromDwg(string dwgPath)
        {
            try
            {
                var fileInfo = new FileInfo(dwgPath);
                string blockName = Path.GetFileNameWithoutExtension(fileInfo.Name);

                // 直接启动 DWG 文件块插入流程
                StartDwgBlockInsertWorkflow(dwgPath, blockName);
                
                // 更新状态
                statusLabel.Text = $"已启动 DWG 块插入: {blockName}";
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"插入 DWG 块时发生错误: {ex.Message}";
            }
        }

       

        private void StartDwgBlockInsertWorkflow(string dwgPath, string blockName)
        {
            try
            {
                statusLabel.Text = $"准备启动插入流程: {blockName}";
                
                // 优先使用服务接口
                if (_blockLibraryService != null)
                {
                    statusLabel.Text = $"正在通过服务插入块: {blockName}";
                    _blockLibraryService.InsertDwgBlock(dwgPath, blockName);
                    statusLabel.Text = $"已请求插入块: {blockName}";
                }
                // 回退到事件处理
                else if (OnDwgBlockInsertRequested != null)
                {
                    statusLabel.Text = $"正在触发插入事件: {blockName}";
                    OnDwgBlockInsertRequested?.Invoke(dwgPath, blockName);
                    statusLabel.Text = $"已触发插入事件: {blockName}";
                }
                else
                {
                    statusLabel.Text = $"警告：没有可用的插入服务或事件处理器！块名: {blockName}";
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"启动 DWG 块插入流程时发生错误: {ex.Message}";
            }
        }

        // DWG 块插入请求事件（向后兼容）
        public static event Action<string, string> OnDwgBlockInsertRequested;
    }
}
