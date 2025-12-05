using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using BlockManager.Abstractions;
using BlockManager.IPC.Server;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace BlockManager.Adapter._2024
{
    public class BlockInsertCommands
    {
        private static IBlockLibraryService _blockLibraryService;
        private static ICADCommandService _cadCommandService;
        private static NamedPipeServer _ipcServer;
        private static Cad2024IPCServerImplementation _ipcImplementation;

        static BlockInsertCommands()
        {
            // 初始化服务实现
            _blockLibraryService = new Cad2024BlockLibraryService();
            _cadCommandService = new Cad2024CADCommandService();
            
            // 初始化IPC服务
            _ipcImplementation = new Cad2024IPCServerImplementation(_blockLibraryService, _cadCommandService);
            _ipcServer = new NamedPipeServer(_ipcImplementation, "BlockManager_IPC");
        }

        [CommandMethod("BLOCKVIEWER")]
        public async void ShowBlockViewer()
        {
            var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
            try
            {
                
                // 启动IPC服务器（如果尚未启动）
                if (!_ipcServer.IsRunning)
                {
                    await _ipcServer.StartAsync();
                    await Task.Delay(1000); // 等待服务器就绪
                    ed?.WriteMessage("\n[BlockViewer] IPC服务器已启动，管道名称: BlockManager_IPC");
                }
                else
                {
                    ed?.WriteMessage("\n[BlockViewer] IPC服务器已在运行，管道名称: BlockManager_IPC");
                }
                
               

                // 启动WPF UI进程
                var uiProcessPath = GetUIProcessPath();
                
                if (File.Exists(uiProcessPath))
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = uiProcessPath,
                        Arguments = "--pipe BlockManager_IPC",
                        UseShellExecute = false,
                        WorkingDirectory = Path.GetDirectoryName(uiProcessPath),
                        CreateNoWindow = false,
                        RedirectStandardOutput = false,
                        RedirectStandardError = false
                    };
                    
                    Process.Start(processInfo);
                                    }
                else
                {
                                                        }
            }
            catch (Exception ex)
            {
                ed = Application.DocumentManager.MdiActiveDocument?.Editor;
                            }
        }
        [CommandMethod("START")]
        public async void StartPipeClient()
        {
            var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
            // 启动IPC服务器（如果尚未启动）
            if (!_ipcServer.IsRunning)
            {
                await _ipcServer.StartAsync();
                await Task.Delay(1000); // 等待服务器就绪
                ed?.WriteMessage("\n[BlockViewer] IPC服务器已启动，管道名称: BlockManager_IPC");
            }
            else
            {
                ed?.WriteMessage("\n[BlockViewer] IPC服务器已在运行，管道名称: BlockManager_IPC");
            }

        }

             
        /// <summary>
        /// 获取UI进程的路径
        /// </summary>
        /// <returns>UI进程可执行文件的完整路径</returns>
        private string GetUIProcessPath()
        {
            try
            {
                // 获取当前程序集的目录
                var currentAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var currentDirectory = Path.GetDirectoryName(currentAssemblyPath);
                
                // 尝试几个可能的路径
                var possiblePaths = new[]
                {
                    Path.Combine(currentDirectory, "BlockManager.UI.exe"),
                    Path.Combine(currentDirectory, "..", "BlockManager.UI", "bin", "Debug", "net6.0-windows7.0", "BlockManager.UI.exe"),
                    Path.Combine(currentDirectory, "..", "BlockManager.UI", "bin", "Release", "net6.0-windows7.0", "BlockManager.UI.exe"),
                    Path.Combine(currentDirectory, "..", "BlockManager.UI", "bin", "Debug", "net8.0-windows", "BlockManager.UI.exe"),
                    Path.Combine(currentDirectory, "..", "BlockManager.UI", "bin", "Release", "net8.0-windows", "BlockManager.UI.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BlockManager", "BlockManager.UI", "bin", "Debug", "net6.0-windows7.0", "BlockManager.UI.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BlockManager", "BlockManager.UI", "bin", "Debug", "net8.0-windows", "BlockManager.UI.exe")
                };

                // 返回第一个存在的路径
                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }

                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        

    }
}
