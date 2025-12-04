using System.IO;
using System.Text.Json;
using BlockManager.UI.Models;

namespace BlockManager.UI.Services;

/// <summary>
/// 设置服务实现
/// </summary>
public class SettingsService : ISettingsService
{
    private const string SettingsFileName = "appsettings.json";
    private readonly string _settingsFilePath;

    public SettingsService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "BlockManager");
        
        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }

        _settingsFilePath = Path.Combine(appFolder, SettingsFileName);
    }

    public async Task<AppSettings> LoadSettingsAsync()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                // 首次运行，创建默认设置并初始化Block目录
                var defaultSettings = AppSettings.Default;
                await InitializeDefaultBlockDirectoryAsync();
                await SaveSettingsAsync(defaultSettings);
                return defaultSettings;
            }

            var json = await File.ReadAllTextAsync(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            
            return settings ?? AppSettings.Default;
        }
        catch (Exception ex)
        {
            // 加载失败时返回默认设置
            System.Diagnostics.Debug.WriteLine($"加载设置失败: {ex.Message}");
            return AppSettings.Default;
        }
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(settings, options);
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"保存设置失败: {ex.Message}");
            throw;
        }
    }

    public string GetSettingsFilePath()
    {
        return _settingsFilePath;
    }

    public async Task InitializeDefaultBlockDirectoryAsync()
    {
        try
        {
            var sourceBlockPath = @"c:\Users\PC\Desktop\BlockManager\Block";
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var targetBlockPath = Path.Combine(appDir, "Block");

            // 如果源目录存在且目标目录不存在，则复制
            if (Directory.Exists(sourceBlockPath) && !Directory.Exists(targetBlockPath))
            {
                await Task.Run(() => CopyDirectory(sourceBlockPath, targetBlockPath));
                System.Diagnostics.Debug.WriteLine($"已复制Block目录到: {targetBlockPath}");
            }
            else if (!Directory.Exists(targetBlockPath))
            {
                // 如果源目录不存在，创建空的目标目录
                Directory.CreateDirectory(targetBlockPath);
                System.Diagnostics.Debug.WriteLine($"已创建空Block目录: {targetBlockPath}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"初始化Block目录失败: {ex.Message}");
        }
    }

    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        // 复制文件
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var targetFile = Path.Combine(targetDir, fileName);
            File.Copy(file, targetFile, true);
        }

        // 递归复制子目录
        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(subDir);
            var targetSubDir = Path.Combine(targetDir, dirName);
            CopyDirectory(subDir, targetSubDir);
        }
    }
}
