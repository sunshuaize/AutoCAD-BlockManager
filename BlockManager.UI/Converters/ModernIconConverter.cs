using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
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
                return iconType switch
                {
                    "folder" => CreateFolderIcon(),
                    "dwg" => CreateDwgIcon(),
                    "image" => new SymbolIcon
                    {
                        Symbol = SymbolRegular.Image24,
                        FontSize = 16
                    },
                    "file" => new SymbolIcon
                    {
                        Symbol = SymbolRegular.DocumentText24,
                        FontSize = 16
                    },
                    _ => new SymbolIcon
                    {
                        Symbol = SymbolRegular.Document24,
                        FontSize = 16
                    }
                };
            }
            return null;
        }

        /// <summary>
        /// 创建Windows 11风格的黄色文件夹图标
        /// </summary>
        private Canvas CreateFolderIcon()
        {
            var canvas = new Canvas
            {
                Width = 16,
                Height = 16
            };

            // 文件夹标签部分（顶部小矩形）
            var folderTab = new Rectangle
            {
                Width = 6,
                Height = 2,
                Fill = new SolidColorBrush(Color.FromRgb(255, 193, 7)), // 黄色
                RadiusX = 1,
                RadiusY = 1
            };
            Canvas.SetLeft(folderTab, 2);
            Canvas.SetTop(folderTab, 4);
            canvas.Children.Add(folderTab);

            // 文件夹主体
            var folderBody = new Rectangle
            {
                Width = 12,
                Height = 8,
                Fill = new SolidColorBrush(Color.FromRgb(255, 193, 7)), // 黄色
                Stroke = new SolidColorBrush(Color.FromRgb(255, 171, 0)), // 深一点的黄色边框
                StrokeThickness = 0.5,
                RadiusX = 2,
                RadiusY = 2
            };
            Canvas.SetLeft(folderBody, 2);
            Canvas.SetTop(folderBody, 6);
            canvas.Children.Add(folderBody);

            // 文件夹高光效果（Windows 11风格的渐变效果）
            var highlight = new Rectangle
            {
                Width = 10,
                Height = 2,
                Fill = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)), // 半透明白色高光
                RadiusX = 1,
                RadiusY = 1
            };
            Canvas.SetLeft(highlight, 3);
            Canvas.SetTop(highlight, 7);
            canvas.Children.Add(highlight);

            // 文件夹阴影效果
            var shadow = new Rectangle
            {
                Width = 12,
                Height = 1,
                Fill = new SolidColorBrush(Color.FromArgb(30, 0, 0, 0)), // 半透明黑色阴影
                RadiusX = 1,
                RadiusY = 1
            };
            Canvas.SetLeft(shadow, 2.5);
            Canvas.SetTop(shadow, 14.5);
            canvas.Children.Add(shadow);

            return canvas;
        }

        /// <summary>
        /// 创建DWG文件的自定义图标
        /// </summary>
        private Canvas CreateDwgIcon()
        {
            var canvas = new Canvas
            {
                Width = 16,
                Height = 16
            };

            // 文档背景
            var docBackground = new Rectangle
            {
                Width = 12,
                Height = 14,
                Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                Stroke = new SolidColorBrush(Color.FromRgb(107, 114, 128)),
                StrokeThickness = 1,
                RadiusX = 1,
                RadiusY = 1
            };
            Canvas.SetLeft(docBackground, 2);
            Canvas.SetTop(docBackground, 1);
            canvas.Children.Add(docBackground);

            // 文档折角
            var docCorner = new Polygon
            {
                Fill = new SolidColorBrush(Color.FromRgb(229, 231, 235)),
                Stroke = new SolidColorBrush(Color.FromRgb(107, 114, 128)),
                StrokeThickness = 1,
                Points = new PointCollection { new Point(11, 1), new Point(14, 4), new Point(11, 4) }
            };
            canvas.Children.Add(docCorner);

            // DWG标识背景
            var labelBackground = new Rectangle
            {
                Width = 10,
                Height = 4,
                Fill = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                RadiusX = 1,
                RadiusY = 1
            };
            Canvas.SetLeft(labelBackground, 3);
            Canvas.SetTop(labelBackground, 10);
            canvas.Children.Add(labelBackground);

            // DWG文字
            var dwgText = new System.Windows.Controls.TextBlock
            {
                Text = "DWG",
                FontSize = 6,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Canvas.SetLeft(dwgText, 4.5);
            Canvas.SetTop(dwgText, 10.5);
            canvas.Children.Add(dwgText);

            // 绘图线条装饰
            var line1 = new Line
            {
                X1 = 4, Y1 = 4,
                X2 = 10, Y2 = 4,
                Stroke = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                StrokeThickness = 0.5
            };
            canvas.Children.Add(line1);

            var line2 = new Line
            {
                X1 = 4, Y1 = 6,
                X2 = 8, Y2 = 6,
                Stroke = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                StrokeThickness = 0.5
            };
            canvas.Children.Add(line2);

            var line3 = new Line
            {
                X1 = 4, Y1 = 8,
                X2 = 9, Y2 = 8,
                Stroke = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                StrokeThickness = 0.5
            };
            canvas.Children.Add(line3);

            return canvas;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
