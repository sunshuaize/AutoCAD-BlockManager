using System;
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
        private readonly BlockManagerServerImplementation _baseImplementation;

        public Cad2024IPCServerImplementation(IBlockLibraryService blockLibraryService)
        {
            _blockLibraryService = blockLibraryService ?? throw new ArgumentNullException(nameof(blockLibraryService));
            
            // 创建基础实现，并传入插入块的处理函数
            _baseImplementation = new BlockManagerServerImplementation(HandleInsertBlockAsync);
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

        public async Task<bool> InsertBlockAsync(InsertBlockRequest request)
        {
            return await _baseImplementation.InsertBlockAsync(request);
        }

        /// <summary>
        /// 处理插入块的请求
        /// </summary>
        /// <param name="blockPath">块文件路径</param>
        /// <param name="blockName">块名称</param>
        /// <returns>操作是否成功</returns>
        private async Task<bool> HandleInsertBlockAsync(string blockPath, string blockName)
        {
            try
            {
                // 使用现有的块库服务来插入块
                _blockLibraryService.InsertDwgBlock(blockPath, blockName);
                await Task.CompletedTask;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"插入块时出错: {ex.Message}");
                return false;
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
