using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
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
        private static Cad2010IPCServerImplementation _ipcServer;

        static BlockInsertCommands()
        {
            // 初始化服务实现
            _blockLibraryService = new Cad2010BlockLibraryService();
            _cadCommandService = new Cad2010CADCommandService();

            // 初始化IPC服务器
            _ipcServer = new Cad2010IPCServerImplementation(_blockLibraryService, _cadCommandService, "BlockManager_IPC");
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

        [CommandMethod("ExecuteCommand")]
        public void ExecuteCommand()
        {
            // 获取当前活动文档
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            if (acDoc == null)
            {
                return;
            }

            Editor acEd = acDoc.Editor;
            string blockFilePath = @"C:\Users\PC\Desktop\Block\围护结构\700x900支撑配筋断面.dwg";

            try
            {
                // 验证文件是否存在
                if (!File.Exists(blockFilePath))
                {
                    acEd.WriteMessage($"\n错误: 找不到指定的DWG文件: {blockFilePath}");
                    return;
                }

                // 构建INSERT命令字符串
                // 使用引号包围文件路径以处理包含空格和中文字符的路径
                string baseCommand = $"-INSERT \"{blockFilePath}\"";
                
                // 添加多个换行符来确保命令自动执行
                // 这模拟了用户输入命令后按回车的行为
                string commandToRun = baseCommand + "\n\n\n";

                acEd.WriteMessage($"\n正在执行INSERT命令: {baseCommand}");

                // 使用AutoCAD的SendStringToExecute方法
                // 参数说明：
                // 1. commandToRun: 格式化后的命令字符串（包含换行符）
                // 2. true: 激活AutoCAD文档窗口
                // 3. false: 不处理非活动文档
                // 4. false: 不在命令行回显（可能有助于自动执行）
                acDoc.SendStringToExecute(
                    commandToRun,           // 要执行的命令
                    true,                   // 执行后激活 AutoCAD 窗口
                    false,                  // 不处理非活动文档
                    true                   // 不在命令行显示（改为false可能有助于自动执行）
                );

                acEd.WriteMessage($"\nINSERT命令已发送，应该会自动执行");
                acEd.WriteMessage($"\n如果命令未自动执行，请检查AutoCAD窗口或手动按回车键");
            }
            catch (Exception ex)
            {
                acEd.WriteMessage($"\n执行命令时发生错误: {ex.Message}");
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