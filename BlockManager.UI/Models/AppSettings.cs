using System.IO;
using System.Text.Json.Serialization;

namespace BlockManager.UI.Models;

/// <summary>
/// 应用程序设置
/// </summary>
public class AppSettings
{
    /// <summary>
    /// 块库根目录路径
    /// </summary>
    [JsonPropertyName("blockLibraryPath")]
    public string BlockLibraryPath { get; set; } = string.Empty;

    /// <summary>
    /// 窗口宽度
    /// </summary>
    [JsonPropertyName("windowWidth")]
    public double WindowWidth { get; set; } = 1120;

    /// <summary>
    /// 窗口高度
    /// </summary>
    [JsonPropertyName("windowHeight")]
    public double WindowHeight { get; set; } = 750;

    /// <summary>
    /// 是否最大化窗口
    /// </summary>
    [JsonPropertyName("isMaximized")]
    public bool IsMaximized { get; set; } = false;

    /// <summary>
    /// 左侧面板宽度
    /// </summary>
    [JsonPropertyName("leftPanelWidth")]
    public double LeftPanelWidth { get; set; } = 256;

    /// <summary>
    /// 获取默认设置
    /// </summary>
    public static AppSettings Default => new()
    {
        BlockLibraryPath = GetDefaultBlockPath(),
        WindowWidth = 1120,
        WindowHeight = 750,
        IsMaximized = false,
        LeftPanelWidth = 256
    };

    /// <summary>
    /// 获取默认Block路径
    /// </summary>
    private static string GetDefaultBlockPath()
    {
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(appDir, "Block");
    }
}
