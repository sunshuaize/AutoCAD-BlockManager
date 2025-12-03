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
                        Arguments = "--pipe BlockManager_IPC", // 指定2010版本的管道
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(uiProcessPath)
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
                    CombinePaths(currentDirectory, "..", "BlockManager.UI", "bin", "Debug", "net8.0-windows", "BlockManager.UI.exe"),
                    CombinePaths(currentDirectory, "..", "BlockManager.UI", "bin", "Release", "net8.0-windows", "BlockManager.UI.exe"),
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
                // 测试IPC服务器状态

                // 测试文件路径
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                var blockPath = CombinePaths(desktop, "BlockManager", "Block");

                // 测试UI路径
                var uiPath = GetUIProcessPath();

                // 测试日志文件
                var logPath = @"c:\temp\blockmgr_test.log";
                try
                {
                    if (!Directory.Exists(@"c:\temp"))
                    {
                        Directory.CreateDirectory(@"c:\temp");
                    }

                    File.WriteAllText(logPath, $"测试时间: {DateTime.Now}\n");
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
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