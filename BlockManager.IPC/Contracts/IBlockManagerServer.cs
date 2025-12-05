using System.Threading.Tasks;
using BlockManager.IPC.DTOs;

namespace BlockManager.IPC.Contracts
{
    /// <summary>
    /// 块管理器服务端接口（CAD进程使用）
    /// </summary>
    public interface IBlockManagerServer
    {
        /// <summary>
        /// 获取块库文件树
        /// </summary>
        /// <param name="rootPath">根路径</param>
        /// <returns>文件树根节点</returns>
        Task<TreeNodeDto> GetBlockLibraryTreeAsync(string rootPath);

        /// <summary>
        /// 获取文件预览
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>预览数据</returns>
        Task<PreviewDto> GetFilePreviewAsync(string filePath);

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="request">命令执行请求</param>
        /// <returns>命令执行响应</returns>
        Task<CommandExecutionResponse> ExecuteCommandAsync(CommandExecutionRequest request);
   

        /// <summary>
        /// 启动服务器
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// 停止服务器
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// 服务器是否正在运行
        /// </summary>
        bool IsRunning { get; }
    }
}
