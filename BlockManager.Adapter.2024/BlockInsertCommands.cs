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
            _ipcImplementation = new Cad2024IPCServerImplementation(_blockLibraryService);
            _ipcServer = new NamedPipeServer(_ipcImplementation, "BlockManager_IPC_2024");
        }

        [CommandMethod("BLOCKVIEWER")]
        public async void ShowBlockViewer()
        {
            var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
            try
            {
                ed?.WriteMessage("\n=== 开始执行BLOCKVIEWER命令 (2024) ===");
                ed?.WriteMessage("\n[调试] 正在启动现代化块库浏览器...");

                // 启动IPC服务器（如果尚未启动）
                ed?.WriteMessage($"\n[调试] IPC服务器运行状态: {_ipcServer.IsRunning}");
                if (!_ipcServer.IsRunning)
                {
                    ed?.WriteMessage("\n[调试] 正在启动IPC服务器...");
                    await _ipcServer.StartAsync();
                    ed?.WriteMessage("\n[调试] IPC服务器启动完成");
                }
                else
                {
                    ed?.WriteMessage("\n[调试] IPC服务器已在运行");
                }

                // 启动WPF UI进程
                ed?.WriteMessage("\n[调试] 开始查找WPF UI程序...");
                var uiProcessPath = GetUIProcessPath();
                ed?.WriteMessage($"\n[调试] UI程序路径: {uiProcessPath}");
                
                if (File.Exists(uiProcessPath))
                {
                    ed?.WriteMessage("\n[调试] 找到UI程序，正在启动...");
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = uiProcessPath,
                        Arguments = "--pipe BlockManager_IPC_2024", // 指定2024版本的管道
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(uiProcessPath)
                    };
                    
                    var process = Process.Start(processInfo);
                    ed?.WriteMessage($"\n[调试] 进程已启动，PID: {process?.Id}");
                    ed?.WriteMessage($"\n[调试] 使用管道: BlockManager_IPC_2024");
                    ed?.WriteMessage("\n✅ 现代化块库浏览器已启动");
                    ed?.WriteMessage("\n✅ AutoCAD 2024现在支持完整的现代化UI功能！");
                }
                else
                {
                    // 未找到现代化UI
                    ed?.WriteMessage("\n错误：未找到现代化UI应用程序");
                    ed?.WriteMessage("\n请确保BlockManager.UI.exe存在于正确的路径中");
                    ed?.WriteMessage("\n可能的路径：");
                    ed?.WriteMessage("\n  - " + Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "BlockManager.UI.exe"));
                    ed?.WriteMessage("\n  - " + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BlockManager", "BlockManager.UI", "bin", "Debug", "net8.0-windows", "BlockManager.UI.exe"));
                }
            }
            catch (Exception ex)
            {
                ed = Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\n[错误] 启动块库浏览器时发生错误: {ex.Message}");
                ed?.WriteMessage($"\n[错误] 堆栈跟踪: {ex.StackTrace}");
            }
        }


        /// <summary>
        /// 获取UI进程的路径
        /// </summary>
        /// <returns>UI进程可执行文件的完整路径</returns>
        private string GetUIProcessPath()
        {
            var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
            try
            {
                // 获取当前程序集的目录
                var currentAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var currentDirectory = Path.GetDirectoryName(currentAssemblyPath);
                
                ed?.WriteMessage($"\n[调试] 当前程序集路径: {currentAssemblyPath}");
                ed?.WriteMessage($"\n[调试] 当前目录: {currentDirectory}");
                
                // 尝试几个可能的路径
                var possiblePaths = new[]
                {
                    Path.Combine(currentDirectory, "BlockManager.UI.exe"),
                    Path.Combine(currentDirectory, "..", "BlockManager.UI", "bin", "Debug", "net8.0-windows", "BlockManager.UI.exe"),
                    Path.Combine(currentDirectory, "..", "BlockManager.UI", "bin", "Release", "net8.0-windows", "BlockManager.UI.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BlockManager", "BlockManager.UI", "bin", "Debug", "net8.0-windows", "BlockManager.UI.exe")
                };

                ed?.WriteMessage("\n[调试] 检查以下路径:");
                for (int i = 0; i < possiblePaths.Length; i++)
                {
                    var path = possiblePaths[i];
                    var exists = File.Exists(path);
                    ed?.WriteMessage($"\n[调试] {i + 1}. {path} -> {(exists ? "存在" : "不存在")}");
                    
                    if (exists)
                    {
                        ed?.WriteMessage($"\n[调试] ✅ 找到UI程序: {path}");
                        return path;
                    }
                }

                ed?.WriteMessage("\n[调试] ❌ 未找到任何UI程序");
                return string.Empty;
            }
            catch (Exception ex)
            {
                ed?.WriteMessage($"\n[调试错误] GetUIProcessPath异常: {ex.Message}");
                return string.Empty;
            }
        }

        [CommandMethod("BMTEST2024")]
        public void TestLogging()
        {
            var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
            try
            {
                ed?.WriteMessage("\n=== BlockManager 调试测试 (2024) ===");
                ed?.WriteMessage("\n[测试] 开始测试日志功能...");
                
                // 测试IPC服务器状态
                ed?.WriteMessage($"\n[测试] IPC服务器是否运行: {_ipcServer?.IsRunning ?? false}");
                
                // 测试文件路径
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                ed?.WriteMessage($"\n[测试] 桌面路径: {desktop}");
                
                var blockPath = Path.Combine(desktop, "BlockManager", "Block");
                ed?.WriteMessage($"\n[测试] Block路径: {blockPath}");
                ed?.WriteMessage($"\n[测试] Block路径存在: {Directory.Exists(blockPath)}");
                
                // 测试UI路径
                var uiPath = GetUIProcessPath();
                ed?.WriteMessage($"\n[测试] UI程序路径: {uiPath}");
                
                // 测试IPC实现
                ed?.WriteMessage($"\n[测试] IPC实现类型: {_ipcImplementation?.GetType().Name}");
                
                // 测试日志文件
                var logPath = @"c:\temp\blockmgr_test_2024.log";
                try
                {
                    if (!Directory.Exists(@"c:\temp"))
                    {
                        Directory.CreateDirectory(@"c:\temp");
                        ed?.WriteMessage("\n[测试] 创建了c:\\temp目录");
                    }
                    
                    File.WriteAllText(logPath, $"测试时间: {DateTime.Now}\n");
                    ed?.WriteMessage($"\n[测试] 日志文件已创建: {logPath}");
                }
                catch (Exception ex)
                {
                    ed?.WriteMessage($"\n[测试错误] 无法创建日志文件: {ex.Message}");
                }
                
                ed?.WriteMessage("\n[测试] ✅ 测试完成");
                
                // 异步启动IPC服务器（不等待）
                if (!_ipcServer.IsRunning)
                {
                    ed?.WriteMessage("\n[测试] 尝试启动IPC服务器进行测试...");
                    StartIpcServerAsync();
                }
            }
            catch (Exception ex)
            {
                ed?.WriteMessage($"\n[测试错误] {ex.Message}");
                ed?.WriteMessage($"\n[测试错误] 堆栈: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 异步启动IPC服务器（不阻塞UI线程）
        /// </summary>
        private async void StartIpcServerAsync()
        {
            try
            {
                var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
                await _ipcServer.StartAsync();
                ed?.WriteMessage("\n[测试] IPC服务器启动完成");
            }
            catch (Exception ex)
            {
                var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\n[测试错误] IPC服务器启动失败: {ex.Message}");
            }
        }
    }
}
