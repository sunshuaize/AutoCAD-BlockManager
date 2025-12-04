using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BlockManager.UI.Converters
{
    /// <summary>
    /// Null值到可见性的转换器
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNullOrEmpty = value == null || (value is string str && string.IsNullOrEmpty(str));
            
            // 检查是否需要反转
            bool invert = parameter is string param && param.Equals("Invert", StringComparison.OrdinalIgnoreCase);
            
            if (invert)
                isNullOrEmpty = !isNullOrEmpty;
                
            return isNullOrEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
