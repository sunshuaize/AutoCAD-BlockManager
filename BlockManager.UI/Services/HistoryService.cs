using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BlockManager.UI.Models;

namespace BlockManager.UI.Services;



/// <summary>
/// 历史记录服务实现
/// </summary>
public class HistoryService : IHistoryService
{
    private const string HistoryFileName = "history.json";
    private readonly string _historyFilePath;
    private HistorySettings? _cachedSettings;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public HistoryService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "BlockManager");
        
        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }

        _historyFilePath = Path.Combine(appFolder, HistoryFileName);
    }

    /// <summary>
    /// 添加或更新历史记录
    /// </summary>
    public async Task AddOrUpdateHistoryAsync(string filePath)
    {
        System.Diagnostics.Debug.WriteLine($"[HistoryService] AddOrUpdateHistoryAsync 被调用: {filePath}");
        
        if (string.IsNullOrEmpty(filePath))
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryService] 文件路径为空，跳过");
            return;
        }
        
        if (!File.Exists(filePath))
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryService] 文件不存在，跳过: {filePath}");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[HistoryService] 等待文件锁...");
        await _fileLock.WaitAsync();
        try
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryService] 获得文件锁，加载设置...");
            var settings = await GetSettingsInternalAsync();
            System.Diagnostics.Debug.WriteLine($"[HistoryService] 当前历史记录数量: {settings.HistoryItems.Count}");
            
            settings.AddOrUpdateItem(filePath);
            System.Diagnostics.Debug.WriteLine($"[HistoryService] 添加后历史记录数量: {settings.HistoryItems.Count}");
            
            await SaveSettingsInternalAsync(settings);
            System.Diagnostics.Debug.WriteLine($"[HistoryService] 设置已保存到: {_historyFilePath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryService] ❌ 添加历史记录失败: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[HistoryService] 异常堆栈: {ex.StackTrace}");
        }
        finally
        {
            _fileLock.Release();
            System.Diagnostics.Debug.WriteLine($"[HistoryService] 释放文件锁");
        }
    }

    /// <summary>
    /// 获取历史记录列表
    /// </summary>
    public async Task<List<HistoryItem>> GetHistoryItemsAsync(int count = -1)
    {
        try
        {
            var settings = await GetSettingsAsync();
            return settings.GetRecentItems(count);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"获取历史记录失败: {ex.Message}");
            return new List<HistoryItem>();
        }
    }

    /// <summary>
    /// 移除历史记录项
    /// </summary>
    public async Task<bool> RemoveHistoryItemAsync(string filePath)
    {
        await _fileLock.WaitAsync();
        try
        {
            var settings = await GetSettingsInternalAsync();
            var removed = settings.RemoveItem(filePath);
            
            if (removed)
            {
                await SaveSettingsInternalAsync(settings);
            }
            
            return removed;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"移除历史记录失败: {ex.Message}");
            return false;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 清空所有历史记录
    /// </summary>
    public async Task ClearAllHistoryAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            var settings = await GetSettingsInternalAsync();
            settings.HistoryItems.Clear();
            await SaveSettingsInternalAsync(settings);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 清空所有历史记录
    /// </summary>
    public async Task ClearHistoryAsync()
    {
        System.Diagnostics.Debug.WriteLine($"[HistoryService] ClearHistoryAsync 被调用");
        
        await _fileLock.WaitAsync();
        try
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryService] 获得文件锁，开始清空历史记录");
            var settings = await GetSettingsInternalAsync();
            int originalCount = settings.HistoryItems.Count;
            
            settings.HistoryItems.Clear();
            await SaveSettingsInternalAsync(settings);
            
            // 清除缓存
            _cachedSettings = null;
            
            System.Diagnostics.Debug.WriteLine($"[HistoryService] 已清空 {originalCount} 条历史记录");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryService] ❌ 清空历史记录失败: {ex.Message}");
            throw;
        }
        finally
        {
            _fileLock.Release();
            System.Diagnostics.Debug.WriteLine($"[HistoryService] 释放文件锁");
        }
    }

    /// <summary>
    /// 检查文件是否在历史记录中
    /// </summary>
    public async Task<bool> ContainsAsync(string filePath)
    {
        try
        {
            var settings = await GetSettingsAsync();
            return settings.HistoryItems.Any(item => 
                string.Equals(item.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"检查历史记录失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 检查历史记录文件是否存在且有效
    /// </summary>
    public async Task<bool> IsHistoryValidAsync()
    {
        try
        {
            if (!File.Exists(_historyFilePath))
                return false;

            var settings = await GetSettingsAsync();
            return settings != null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"检查历史记录有效性失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 获取历史记录设置
    /// </summary>
    public async Task<HistorySettings> GetSettingsAsync()
    {
        if (_cachedSettings != null)
            return _cachedSettings;

        return await GetSettingsInternalAsync();
    }

    /// <summary>
    /// 保存历史记录设置
    /// </summary>
    public async Task SaveSettingsAsync(HistorySettings settings)
    {
        await _fileLock.WaitAsync();
        try
        {
            await SaveSettingsInternalAsync(settings);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 清理过期的历史记录
    /// </summary>
    public async Task CleanupExpiredItemsAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            var settings = await GetSettingsInternalAsync();
            var originalCount = settings.HistoryItems.Count;
            
            settings.CleanupExpiredItems();
            
            if (settings.HistoryItems.Count != originalCount)
            {
                await SaveSettingsInternalAsync(settings);
                System.Diagnostics.Debug.WriteLine($"清理了 {originalCount - settings.HistoryItems.Count} 条过期历史记录");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"清理过期历史记录失败: {ex.Message}");
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 内部获取设置方法（不使用缓存）
    /// </summary>
    private async Task<HistorySettings> GetSettingsInternalAsync()
    {
        try
        {
            if (!File.Exists(_historyFilePath))
            {
                System.Diagnostics.Debug.WriteLine($"[HistoryService] 历史记录文件不存在，创建默认设置");
                var defaultSettings = HistorySettings.Default;
                await SaveSettingsInternalAsync(defaultSettings);
                return defaultSettings;
            }

            var json = await File.ReadAllTextAsync(_historyFilePath);
            System.Diagnostics.Debug.WriteLine($"[HistoryService] 读取到JSON内容长度: {json.Length}");
            
            var settings = JsonSerializer.Deserialize<HistorySettings>(json);
            if (settings != null)
            {
                System.Diagnostics.Debug.WriteLine($"[HistoryService] 成功解析历史记录，数量: {settings.HistoryItems.Count}");
                _cachedSettings = settings;
                return _cachedSettings;
            }
            
            // 如果解析失败，返回默认设置
            System.Diagnostics.Debug.WriteLine($"[HistoryService] JSON解析返回null，使用默认设置");
            var fallbackSettings = HistorySettings.Default;
            _cachedSettings = fallbackSettings;
            return fallbackSettings;
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryService] JSON格式错误: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[HistoryService] 删除损坏的文件并创建新的默认设置");
            
            // 删除损坏的文件
            try
            {
                File.Delete(_historyFilePath);
            }
            catch { }
            
            var defaultSettings = HistorySettings.Default;
            await SaveSettingsInternalAsync(defaultSettings);
            _cachedSettings = defaultSettings;
            return defaultSettings;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryService] 加载历史记录设置失败: {ex.Message}");
            var defaultSettings = HistorySettings.Default;
            _cachedSettings = defaultSettings;
            return defaultSettings;
        }
    }

    /// <summary>
    /// 内部保存设置方法
    /// </summary>
    private async Task SaveSettingsInternalAsync(HistorySettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(settings, options);
            await File.WriteAllTextAsync(_historyFilePath, json);
            
            _cachedSettings = settings;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"保存历史记录设置失败: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _fileLock?.Dispose();
    }
}
