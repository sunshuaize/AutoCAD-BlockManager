using System;
using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Controls;
using Wpf.Ui;

namespace BlockManager.UI.Converters
{
    /// <summary>
    /// 现代化图标转换器，将图标类型转换为 WPF.UI 的 SymbolIcon
    /// </summary>
    public class ModernIconConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string iconType)
            {
                var symbol = iconType switch
                {
                    "folder" => SymbolRegular.Folder24,
                    "dwg" => SymbolRegular.Document24,
                    "image" => SymbolRegular.Image24,
                    "file" => SymbolRegular.DocumentText24,
                    _ => SymbolRegular.Document24
                };

                return new SymbolIcon
                {
                    Symbol = symbol,
                    FontSize = 16
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
