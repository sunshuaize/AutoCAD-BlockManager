using System.IO;
using System.Text.Json.Serialization;

namespace BlockManager.UI.Models;

/// <summary>
/// 历史记录项
/// </summary>
public class HistoryItem
{
    /// <summary>
    /// 文件路径（主键）
    /// </summary>
    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 文件名
    /// </summary>
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 添加时间
    /// </summary>
    [JsonPropertyName("addedTime")]
    public DateTime AddedTime { get; set; }

    /// <summary>
    /// 最后访问时间
    /// </summary>
    [JsonPropertyName("lastAccessTime")]
    public DateTime LastAccessTime { get; set; }

    /// <summary>
    /// 使用次数
    /// </summary>
    [JsonPropertyName("useCount")]
    public int UseCount { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    [JsonPropertyName("fileSize")]
    public long FileSize { get; set; }

    /// <summary>
    /// 创建历史记录项
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>历史记录项</returns>
    public static HistoryItem Create(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var fileInfo = new FileInfo(filePath);
        var now = DateTime.Now;

        return new HistoryItem
        {
            FilePath = filePath,
            FileName = fileName,
            AddedTime = now,
            LastAccessTime = now,
            UseCount = 1,
            FileSize = fileInfo.Exists ? fileInfo.Length : 0
        };
    }

    /// <summary>
    /// 更新访问信息
    /// </summary>
    public void UpdateAccess()
    {
        LastAccessTime = DateTime.Now;
        UseCount++;
    }

    /// <summary>
    /// 获取显示用的时间文本
    /// </summary>
    public string GetDisplayTime()
    {
        var timeSpan = DateTime.Now - LastAccessTime;
        
        if (timeSpan.TotalMinutes < 1)
            return "刚刚";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes}分钟前";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours}小时前";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays}天前";
        
        return LastAccessTime.ToString("MM-dd HH:mm");
    }
}
