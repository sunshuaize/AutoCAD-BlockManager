using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BlockManager.UI.Converters;

/// <summary>
/// 字符串到可见性转换器
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return Visibility.Collapsed;

        var stringValue = value.ToString();
        var targetValue = parameter.ToString();

        return string.Equals(stringValue, targetValue, StringComparison.OrdinalIgnoreCase) 
            ? Visibility.Visible 
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
