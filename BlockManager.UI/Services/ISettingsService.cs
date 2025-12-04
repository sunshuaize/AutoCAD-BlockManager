using BlockManager.UI.Models;

namespace BlockManager.UI.Services;

/// <summary>
/// 设置服务接口
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// 加载设置
    /// </summary>
    Task<AppSettings> LoadSettingsAsync();

    /// <summary>
    /// 保存设置
    /// </summary>
    Task SaveSettingsAsync(AppSettings settings);

    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    string GetSettingsFilePath();

    /// <summary>
    /// 初始化默认Block目录
    /// </summary>
    Task InitializeDefaultBlockDirectoryAsync();
}
