using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BlockManager.Abstractions;
using BlockManager.IPC.Contracts;
using BlockManager.IPC.DTOs;
using BlockManager.IPC.Server;

namespace BlockManager.Adapter._2024
{
    /// <summary>
    /// AutoCAD 2024 IPC服务端实现
    /// </summary>
    public class Cad2024IPCServerImplementation : IBlockManagerServer
    {
        private readonly IBlockLibraryService _blockLibraryService;
        private readonly ICADCommandService _cadCommandService;
        private readonly BlockManagerServerImplementation _baseImplementation;

        public Cad2024IPCServerImplementation(IBlockLibraryService blockLibraryService, ICADCommandService cadCommandService)
        {
            _blockLibraryService = blockLibraryService ?? throw new ArgumentNullException(nameof(blockLibraryService));
            _cadCommandService = cadCommandService ?? throw new ArgumentNullException(nameof(cadCommandService));
            
            // 创建基础实现
            _baseImplementation = new BlockManagerServerImplementation();
        }

        public bool IsRunning => _baseImplementation.IsRunning;

        public async Task StartAsync()
        {
            try
            {
                LogToAutoCAD("[2024 IPC] 开始启动IPC服务器...");
                await _baseImplementation.StartAsync();
                LogToAutoCAD("[2024 IPC] IPC服务器启动完成");
            }
            catch (Exception ex)
            {
                LogToAutoCAD($"[2024 IPC] 启动失败: {ex.Message}");
                throw;
            }
        }

        public async Task StopAsync()
        {
            await _baseImplementation.StopAsync();
        }

        public async Task<TreeNodeDto> GetBlockLibraryTreeAsync(string rootPath)
        {
            try
            {
                LogToAutoCAD($"[2024 IPC] 请求获取文件树，路径: {rootPath}");
                var result = await _baseImplementation.GetBlockLibraryTreeAsync(rootPath);
                LogToAutoCAD($"[2024 IPC] 文件树获取成功，根节点: {result?.Name}, 子节点数: {result?.Children?.Count ?? 0}");
                return result;
            }
            catch (Exception ex)
            {
                LogToAutoCAD($"[2024 IPC] 获取文件树失败: {ex.Message}");
                throw;
            }
        }

        public async Task<PreviewDto> GetFilePreviewAsync(string filePath)
        {
            return await _baseImplementation.GetFilePreviewAsync(filePath);
        }

        public async Task<CommandExecutionResponse> ExecuteCommandAsync(CommandExecutionRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                LogToAutoCAD($"[2024 IPC] 执行命令: {request.Command}");
                
                // 执行CAD命令
                _cadCommandService.ExecuteCommand(request.Command);
                
                stopwatch.Stop();
                
                var response = new CommandExecutionResponse
                {
                    IsSuccess = true,
                    Result = $"命令 '{request.Command}' 执行完成",
                    ExecutedAt = DateTime.UtcNow,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };
                
                LogToAutoCAD($"[2024 IPC] 命令执行成功，耗时: {stopwatch.ElapsedMilliseconds}ms");
                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                LogToAutoCAD($"[2024 IPC] 命令执行失败: {ex.Message}");
                
                return new CommandExecutionResponse
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    ExecutedAt = DateTime.UtcNow,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
        }
        
      

        /// <summary>
        /// 输出日志到AutoCAD命令行
        /// </summary>
        /// <param name="message">日志消息</param>
        private void LogToAutoCAD(string message)
        {
            try
            {
                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
                ed?.WriteMessage("\n" + message);
                
                // 同时输出到调试窗口和控制台
                System.Diagnostics.Debug.WriteLine(message);
                Console.WriteLine(message);
            }
            catch
            {
                // 如果AutoCAD不可用，至少输出到调试窗口
                System.Diagnostics.Debug.WriteLine(message);
            }
        }
    }
}
