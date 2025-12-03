using System;
using System.Threading.Tasks;
using BlockManager.IPC.DTOs;

namespace BlockManager.IPC.Contracts
{
    /// <summary>
    /// 块管理器客户端接口（UI进程使用）
    /// </summary>
    public interface IBlockManagerClient
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
        /// 插入块到CAD
        /// </summary>
        /// <param name="request">插入请求</param>
        /// <returns>操作是否成功</returns>
        Task<bool> InsertBlockAsync(InsertBlockRequest request);

        /// <summary>
        /// 文件变化事件
        /// </summary>
        event EventHandler<FileChangedEventArgs>? FileChanged;

        /// <summary>
        /// 连接到服务器
        /// </summary>
        Task ConnectAsync();

        /// <summary>
        /// 断开连接
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// 检查连接状态
        /// </summary>
        bool IsConnected { get; }
    }

    /// <summary>
    /// 文件变化事件参数
    /// </summary>
    public class FileChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 变化类型：Created、Modified、Deleted
        /// </summary>
        public string ChangeType { get; set; } = string.Empty;

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
