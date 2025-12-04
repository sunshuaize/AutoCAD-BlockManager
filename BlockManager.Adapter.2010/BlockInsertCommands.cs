using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using BlockManager.Abstractions;
using System;
using System.Diagnostics;
using System.IO;
using Exception = System.Exception;

namespace BlockManager.Adapter._2010
{
    public class BlockInsertCommands
    {
        private static IBlockLibraryService _blockLibraryService;
        private static ICADCommandService _cadCommandService;
        private static SimpleIPCServer _ipcServer;

        static BlockInsertCommands()
        {
            // 初始化服务实现
            _blockLibraryService = new Cad2010BlockLibraryService();
            _cadCommandService = new Cad2010CADCommandService();

            // 初始化简化的IPC服务器
            _ipcServer = new SimpleIPCServer(_blockLibraryService, "BlockManager_IPC");
        }

        [CommandMethod("BLOCKVIEWER")]
        public void ShowBlockViewer()
        {
            var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
            try
            {
                // 启动IPC服务器（如果尚未启动）
                if (!_ipcServer.IsRunning)
                {
                    _ipcServer.Start();
                }
                else
                {
                }

                // 启动WPF UI进程
                var uiProcessPath = GetUIProcessPath();

                if (File.Exists(uiProcessPath))
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = uiProcessPath,
                        Arguments = "--pipe BlockManager_IPC", // 指定统一的管道名称
                        UseShellExecute = false,
                        WorkingDirectory = Path.GetDirectoryName(uiProcessPath),
                        CreateNoWindow = false,
                        RedirectStandardOutput = false,
                        RedirectStandardError = false
                    };

                    var process = Process.Start(processInfo);
                }
                else
                {
                    // 未找到现代化UI
                }
            }
            catch (Exception ex)
            {
                ed = Application.DocumentManager.MdiActiveDocument?.Editor;
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

                // 尝试几个可能的路径
                var possiblePaths = new[]
                {
                    Path.Combine(currentDirectory, "BlockManager.UI.exe"),
                    CombinePaths(currentDirectory, "..", "BlockManager.UI", "bin", "Debug", "net6.0-windows7.0", "BlockManager.UI.exe"),
                    CombinePaths(currentDirectory, "..", "BlockManager.UI", "bin", "Release", "net6.0-windows7.0", "BlockManager.UI.exe"),
                    CombinePaths(currentDirectory, "..", "BlockManager.UI", "bin", "Debug", "net8.0-windows", "BlockManager.UI.exe"),
                    CombinePaths(currentDirectory, "..", "BlockManager.UI", "bin", "Release", "net8.0-windows", "BlockManager.UI.exe"),
                    CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BlockManager", "BlockManager.UI", "bin", "Debug", "net6.0-windows7.0", "BlockManager.UI.exe"),
                    CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BlockManager", "BlockManager.UI", "bin", "Debug", "net8.0-windows", "BlockManager.UI.exe")
                };

                for (int i = 0; i < possiblePaths.Length; i++)
                {
                    var path = possiblePaths[i];
                    var exists = File.Exists(path);

                    if (exists)
                    {
                        return path;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        [CommandMethod("BMTEST")]
        public void TestLogging()
        {
            var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
            try
            {
                ed?.WriteMessage("\n=== BlockManager 2010 调试信息 ===");
                
                // 测试IPC服务器状态
                ed?.WriteMessage($"\nIPC服务器运行状态: {(_ipcServer?.IsRunning ?? false)}");
                
                // 测试文件路径
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                ed?.WriteMessage($"\n桌面路径: {desktop}");
                
                var blockPath = CombinePaths(desktop, "BlockManager", "Block");
                ed?.WriteMessage($"\n块文件夹路径: {blockPath}");
                ed?.WriteMessage($"\n块文件夹存在: {Directory.Exists(blockPath)}");
                
                // 测试UI路径
                var uiPath = GetUIProcessPath();
                ed?.WriteMessage($"\nUI程序路径: {uiPath}");
                ed?.WriteMessage($"\nUI程序存在: {File.Exists(uiPath)}");
                
                // 测试服务实例
                ed?.WriteMessage($"\n块库服务实例: {(_blockLibraryService != null ? "已创建" : "未创建")}");
                ed?.WriteMessage($"\nCAD命令服务实例: {(_cadCommandService != null ? "已创建" : "未创建")}");
                
                // 测试日志文件
                var logPath = @"c:\temp\blockmgr_test.log";
                try
                {
                    if (!Directory.Exists(@"c:\temp"))
                    {
                        Directory.CreateDirectory(@"c:\temp");
                    }

                    var testContent = $"测试时间: {DateTime.Now}\n" +
                                    $"IPC服务器状态: {(_ipcServer?.IsRunning ?? false)}\n" +
                                    $"UI路径: {uiPath}\n" +
                                    $"块路径: {blockPath}\n";
                    
                    File.WriteAllText(logPath, testContent);
                    ed?.WriteMessage($"\n测试日志已写入: {logPath}");
                }
                catch (Exception ex)
                {
                    ed?.WriteMessage($"\n写入日志失败: {ex.Message}");
                }
                
                // 测试插入功能
                ed?.WriteMessage("\n=== 测试插入功能 ===");
                var testFile = CombinePaths(blockPath, "围护结构", "1000x1000冠梁配筋断面.dwg");
                ed?.WriteMessage($"\n测试文件: {testFile}");
                ed?.WriteMessage($"\n测试文件存在: {File.Exists(testFile)}");
                
                if (File.Exists(testFile))
                {
                    ed?.WriteMessage("\n开始测试插入...");
                    _blockLibraryService?.InsertDwgBlock(testFile, "测试块");
                }
                
                ed?.WriteMessage("\n=== 调试信息结束 ===");
            }
            catch (Exception ex)
            {
                ed?.WriteMessage($"\n调试测试出错: {ex.Message}");
                ed?.WriteMessage($"\n错误详情: {ex.ToString()}");
            }
        }

        [CommandMethod("BMSTART")]
        public void StartIPCServer()
        {
            var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
            try
            {
                ed?.WriteMessage("\n=== 启动IPC服务器 ===");
                ed?.WriteMessage($"\n当前IPC服务器状态: {(_ipcServer?.IsRunning ?? false)}");
                
                if (_ipcServer == null)
                {
                    ed?.WriteMessage("\n错误：IPC服务器实例为null，重新创建...");
                    _ipcServer = new SimpleIPCServer(_blockLibraryService, "BlockManager_IPC");
                }
                
                if (!_ipcServer.IsRunning)
                {
                    ed?.WriteMessage("\n正在启动IPC服务器...");
                    _ipcServer.Start();
                    
                    // 等待一下让服务器启动
                    System.Threading.Thread.Sleep(1000);
                    
                    ed?.WriteMessage($"\nIPC服务器启动后状态: {_ipcServer.IsRunning}");
                }
                else
                {
                    ed?.WriteMessage("\nIPC服务器已在运行");
                }
                
                ed?.WriteMessage("\n=== IPC服务器启动完成 ===");
            }
            catch (Exception ex)
            {
                ed?.WriteMessage($"\n启动IPC服务器出错: {ex.Message}");
                ed?.WriteMessage($"\n错误详情: {ex.ToString()}");
            }
        }

        [CommandMethod("BMINSERT")]
        public void TestInsert()
        {
            var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
            try
            {
                ed?.WriteMessage("\n=== 测试INSERT命令 ===");
                
                // 测试文件路径
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var testFile = CombinePaths(desktop, "BlockManager", "Block", "围护结构", "1000x1000冠梁配筋断面.dwg");
                
                ed?.WriteMessage($"\n测试文件: {testFile}");
                ed?.WriteMessage($"\n文件存在: {File.Exists(testFile)}");
                
                if (!File.Exists(testFile))
                {
                    ed?.WriteMessage("\n错误：测试文件不存在！");
                    return;
                }
                
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    ed?.WriteMessage("\n错误：没有活动文档！");
                    return;
                }
                
                // 测试不同的INSERT命令格式
                string normalizedPath = Path.GetFullPath(testFile);
                string escapedPath = normalizedPath.Replace("\\", "/");
                
                ed?.WriteMessage($"\n标准化路径: {normalizedPath}");
                ed?.WriteMessage($"\n转义路径: {escapedPath}");
                
                // 方法1：简单INSERT
                ed?.WriteMessage("\n--- 方法1：简单INSERT ---");
                string cmd1 = "INSERT ";
                ed?.WriteMessage($"\n发送命令: {cmd1}");
                doc.SendStringToExecute(cmd1, true, false, false);
                
                System.Threading.Thread.Sleep(500);
                
                ed?.WriteMessage($"\n发送路径: \"{escapedPath}\"");
                doc.SendStringToExecute($"\"{escapedPath}\" ", true, false, false);
                
                ed?.WriteMessage("\n=== INSERT测试完成 ===");
            }
            catch (Exception ex)
            {
                ed?.WriteMessage($"\nINSERT测试出错: {ex.Message}");
                ed?.WriteMessage($"\n错误详情: {ex.ToString()}");
            }
        }

        /// <summary>
        /// 组合多个路径部分（兼容.NET Framework 3.5）
        /// </summary>
        /// <param name="paths">路径部分</param>
        /// <returns>组合后的完整路径</returns>
        private string CombinePaths(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return string.Empty;

            string result = paths[0];
            for (int i = 1; i < paths.Length; i++)
            {
                result = Path.Combine(result, paths[i]);
            }
            return result;
        }
    }
}