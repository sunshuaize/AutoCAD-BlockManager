using System;
using System.Globalization;
using System.Windows.Data;

namespace BlockManager.UI.Converters;

/// <summary>
/// 时间显示转换器，将DateTime转换为友好的显示文本
/// </summary>
public class TimeDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;
            
            if (timeSpan.TotalMinutes < 1)
                return "刚刚";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}分钟前";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}小时前";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}天前";
            
            return dateTime.ToString("MM-dd HH:mm");
        }
        
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
