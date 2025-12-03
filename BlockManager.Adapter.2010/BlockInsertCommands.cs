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
            _ipcServer = new SimpleIPCServer(_blockLibraryService);
        }

        [CommandMethod("BLOCKVIEWER")]
        public void ShowBlockViewer()
        {
            var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
            try
            {
                ed?.WriteMessage("\n=== 开始执行BLOCKVIEWER命令 ===");
                ed?.WriteMessage("\n[调试] 正在启动现代化块库浏览器...");

                // 启动IPC服务器（如果尚未启动）
                ed?.WriteMessage($"\n[调试] IPC服务器运行状态: {_ipcServer.IsRunning}");
                if (!_ipcServer.IsRunning)
                {
                    ed?.WriteMessage("\n[调试] 正在启动IPC服务器...");
                    _ipcServer.Start();
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
                        Arguments = "--pipe BlockManager_IPC", // 指定2010版本的管道
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(uiProcessPath)
                    };
                    
                    var process = Process.Start(processInfo);
                    ed?.WriteMessage($"\n[调试] 进程已启动，PID: {process?.Id}");
                    ed?.WriteMessage($"\n[调试] 使用管道: BlockManager_IPC");
                    ed?.WriteMessage("\n✅ 现代化块库浏览器已启动");
                    ed?.WriteMessage("\n✅ AutoCAD 2010现在支持完整的现代化UI功能！");
                }
                else
                {
                    // 未找到现代化UI
                    ed?.WriteMessage("\n错误：未找到现代化UI应用程序");
                    ed?.WriteMessage("\n请确保BlockManager.UI.exe存在于正确的路径中");
                    ed?.WriteMessage("\n可能的路径：");
                    ed?.WriteMessage("\n  - " + Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "BlockManager.UI.exe"));
                    ed?.WriteMessage("\n  - " + CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BlockManager", "BlockManager.UI", "bin", "Debug", "net8.0-windows", "BlockManager.UI.exe"));
                }
            }
            catch (Exception ex)
            {
                 ed = Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage($"\n启动块库浏览器时发生错误: {ex.Message}");
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
                    CombinePaths(currentDirectory, "..", "BlockManager.UI", "bin", "Debug", "net8.0-windows", "BlockManager.UI.exe"),
                    CombinePaths(currentDirectory, "..", "BlockManager.UI", "bin", "Release", "net8.0-windows", "BlockManager.UI.exe"),
                    CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BlockManager", "BlockManager.UI", "bin", "Debug", "net8.0-windows", "BlockManager.UI.exe")
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

        [CommandMethod("BMTEST")]
        public void TestLogging()
        {
            var ed = Application.DocumentManager.MdiActiveDocument?.Editor;
            try
            {
                ed?.WriteMessage("\n=== BlockManager 调试测试 ===");
                ed?.WriteMessage("\n[测试] 开始测试日志功能...");
                
                // 测试IPC服务器状态
                ed?.WriteMessage($"\n[测试] IPC服务器是否运行: {_ipcServer?.IsRunning ?? false}");
                
                // 测试文件路径
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                ed?.WriteMessage($"\n[测试] 桌面路径: {desktop}");
                
                var blockPath = CombinePaths(desktop, "BlockManager", "Block");

                ed?.WriteMessage($"\n[测试] Block路径: {blockPath}");
                ed?.WriteMessage($"\n[测试] Block路径存在: {Directory.Exists(blockPath)}");
                
                // 测试UI路径
                var uiPath = GetUIProcessPath();
                ed?.WriteMessage($"\n[测试] UI程序路径: {uiPath}");
                
                // 测试日志文件
                var logPath = @"c:\temp\blockmgr_test.log";
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
            }
            catch (Exception ex)
            {
                ed?.WriteMessage($"\n[测试错误] {ex.Message}");
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