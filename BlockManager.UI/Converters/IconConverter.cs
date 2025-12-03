using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BlockManager.UI.Converters
{
    /// <summary>
    /// 图标类型到图片的转换器
    /// </summary>
    public class IconConverter : IValueConverter
    {
        private static readonly Dictionary<string, BitmapImage> IconCache = new();

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string iconType)
            {
                // 先检查缓存
                if (IconCache.TryGetValue(iconType, out var cachedIcon))
                    return cachedIcon;

                // 根据图标类型选择对应的图标路径
                var iconPath = iconType switch
                {
                    "folder" => "pack://application:,,,/Resources/Icons/folder.png",
                    "dwg" => "pack://application:,,,/Resources/Icons/dwg.png",
                    "image" => "pack://application:,,,/Resources/Icons/image.png",
                    _ => "pack://application:,,,/Resources/Icons/file.png"
                };

                try
                {
                    var bitmap = new BitmapImage(new Uri(iconPath));
                    bitmap.Freeze(); // 使图片可以跨线程使用
                    
                    // 缓存图标
                    IconCache[iconType] = bitmap;
                    return bitmap;
                }
                catch
                {
                    // 如果图标加载失败，返回null
                    return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
