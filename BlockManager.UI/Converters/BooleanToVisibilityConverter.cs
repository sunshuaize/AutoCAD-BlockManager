using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BlockManager.UI.Converters
{
    /// <summary>
    /// 布尔值到可见性的转换器
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = value is bool boolValue && boolValue;
            
            // 检查是否需要反转
            bool invert = parameter is string param && param.Equals("Invert", StringComparison.OrdinalIgnoreCase);
            
            if (invert)
                isVisible = !isVisible;
                
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = value is Visibility visibility && visibility == Visibility.Visible;
            
            // 检查是否需要反转
            bool invert = parameter is string param && param.Equals("Invert", StringComparison.OrdinalIgnoreCase);
            
            if (invert)
                isVisible = !isVisible;
                
            return isVisible;
        }
    }
}
