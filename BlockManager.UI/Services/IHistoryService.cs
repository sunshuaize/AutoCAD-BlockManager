using BlockManager.UI.Models;

namespace BlockManager.UI.Services;

/// <summary>
/// 历史记录服务接口
/// </summary>
public interface IHistoryService
{
    /// <summary>
    /// 添加或更新历史记录
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>操作任务</returns>
    Task AddOrUpdateHistoryAsync(string filePath);

    /// <summary>
    /// 获取历史记录列表
    /// </summary>
    /// <param name="count">获取数量，-1表示获取全部</param>
    /// <returns>历史记录列表</returns>
    Task<List<HistoryItem>> GetHistoryItemsAsync(int count = -1);

    /// <summary>
    /// 移除历史记录项
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否成功移除</returns>
    Task<bool> RemoveHistoryItemAsync(string filePath);

    /// <summary>
    /// 清空所有历史记录
    /// </summary>
    /// <returns>操作任务</returns>
    Task ClearAllHistoryAsync();

    /// <summary>
    /// 检查文件是否在历史记录中
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否存在</returns>
    Task<bool> ContainsAsync(string filePath);

    /// <summary>
    /// 检查历史记录文件是否存在且有效
    /// </summary>
    Task<bool> IsHistoryValidAsync();

    /// <summary>
    /// 清空所有历史记录
    /// </summary>
    Task ClearHistoryAsync();

    /// <summary>
    /// 获取历史记录设置
    /// </summary>
    /// <returns>历史记录设置</returns>
    Task<HistorySettings> GetSettingsAsync();

    /// <summary>
    /// 保存历史记录设置
    /// </summary>
    /// <param name="settings">历史记录设置</param>
    /// <returns>操作任务</returns>
    Task SaveSettingsAsync(HistorySettings settings);

    /// <summary>
    /// 清理过期的历史记录
    /// </summary>
    /// <returns>操作任务</returns>
    Task CleanupExpiredItemsAsync();
}
