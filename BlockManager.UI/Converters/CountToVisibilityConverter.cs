using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BlockManager.UI.Converters
{
    /// <summary>
    /// 计数转可见性转换器，当计数大于0时显示，否则隐藏
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                bool isVisible = count > 0;
                
                // 检查是否需要反转
                if (parameter is string param && param.Equals("Invert", StringComparison.OrdinalIgnoreCase))
                {
                    isVisible = !isVisible;
                }
                
                return isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
