using System.Text.Json.Serialization;

namespace BlockManager.UI.Models;

/// <summary>
/// 历史记录设置
/// </summary>
public class HistorySettings
{
    /// <summary>
    /// 历史记录列表
    /// </summary>
    [JsonPropertyName("historyItems")]
    public List<HistoryItem> HistoryItems { get; set; } = new();

    /// <summary>
    /// 最大记录数量
    /// </summary>
    [JsonPropertyName("maxHistoryCount")]
    public int MaxHistoryCount { get; set; } = 20;

    /// <summary>
    /// 是否启用历史记录
    /// </summary>
    [JsonPropertyName("isHistoryEnabled")]
    public bool IsHistoryEnabled { get; set; } = true;

    /// <summary>
    /// 自动清理天数（超过此天数的记录将被自动删除）
    /// </summary>
    [JsonPropertyName("autoCleanupDays")]
    public int AutoCleanupDays { get; set; } = 30;

    /// <summary>
    /// 获取默认历史记录设置
    /// </summary>
    public static HistorySettings Default => new()
    {
        HistoryItems = new List<HistoryItem>(),
        MaxHistoryCount = 20,
        IsHistoryEnabled = true,
        AutoCleanupDays = 30
    };

    /// <summary>
    /// 添加或更新历史记录项
    /// </summary>
    /// <param name="filePath">文件路径</param>
    public void AddOrUpdateItem(string filePath)
    {
        System.Diagnostics.Debug.WriteLine($"[HistorySettings] AddOrUpdateItem 被调用: {filePath}");
        
        if (!IsHistoryEnabled)
        {
            System.Diagnostics.Debug.WriteLine($"[HistorySettings] 历史记录功能已禁用，跳过");
            return;
        }
        
        if (string.IsNullOrEmpty(filePath))
        {
            System.Diagnostics.Debug.WriteLine($"[HistorySettings] 文件路径为空，跳过");
            return;
        }

        // 查找现有项
        var existingItem = HistoryItems.FirstOrDefault(x => 
            string.Equals(x.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

        if (existingItem != null)
        {
            System.Diagnostics.Debug.WriteLine($"[HistorySettings] 找到现有项，更新访问时间: {existingItem.FileName}");
            // 更新现有项
            existingItem.UpdateAccess();
            
            // 移动到列表开头（最近使用）
            HistoryItems.Remove(existingItem);
            HistoryItems.Insert(0, existingItem);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[HistorySettings] 创建新的历史记录项");
            // 创建新项
            var newItem = HistoryItem.Create(filePath);
            HistoryItems.Insert(0, newItem);
            System.Diagnostics.Debug.WriteLine($"[HistorySettings] 新项已创建: {newItem.FileName}");
        }

        System.Diagnostics.Debug.WriteLine($"[HistorySettings] 当前列表数量: {HistoryItems.Count}");

        // 限制最大数量
        while (HistoryItems.Count > MaxHistoryCount)
        {
            var removedItem = HistoryItems[HistoryItems.Count - 1];
            HistoryItems.RemoveAt(HistoryItems.Count - 1);
            System.Diagnostics.Debug.WriteLine($"[HistorySettings] 移除旧项: {removedItem.FileName}");
        }

        // 自动清理过期项
        var beforeCleanup = HistoryItems.Count;
        CleanupExpiredItems();
        if (HistoryItems.Count != beforeCleanup)
        {
            System.Diagnostics.Debug.WriteLine($"[HistorySettings] 清理过期项: {beforeCleanup} -> {HistoryItems.Count}");
        }
    }

    /// <summary>
    /// 移除历史记录项
    /// </summary>
    /// <param name="filePath">文件路径</param>
    public bool RemoveItem(string filePath)
    {
        var item = HistoryItems.FirstOrDefault(x => 
            string.Equals(x.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
        
        if (item != null)
        {
            HistoryItems.Remove(item);
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// 清理过期的历史记录项
    /// </summary>
    public void CleanupExpiredItems()
    {
        if (AutoCleanupDays <= 0)
            return;

        var cutoffDate = DateTime.Now.AddDays(-AutoCleanupDays);
        HistoryItems.RemoveAll(x => x.LastAccessTime < cutoffDate);
    }

    /// <summary>
    /// 清空所有历史记录
    /// </summary>
    public void ClearAll()
    {
        HistoryItems.Clear();
    }

    /// <summary>
    /// 获取按最近使用时间排序的历史记录
    /// </summary>
    /// <param name="count">返回的数量，-1表示返回全部</param>
    /// <returns>历史记录列表</returns>
    public List<HistoryItem> GetRecentItems(int count = -1)
    {
        var sortedItems = HistoryItems
            .OrderByDescending(x => x.LastAccessTime)
            .ToList();

        return count > 0 ? sortedItems.Take(count).ToList() : sortedItems;
    }
}
