using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using static System.Windows.Media.ColorConverter;

namespace PickBan_o_mat.Converter
{
    public class T1ToColor : IValueConverter
    {
        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            object firstObject2Convert = values;
            Color rtnColor;
            try
            {
                bool isT1 = firstObject2Convert != null && (bool) firstObject2Convert;

                if (isT1)
                {
                    rtnColor = (Color) ConvertFromString("#FFDF6027");
                    return new SolidColorBrush(rtnColor);
                }

                rtnColor = (Color) ConvertFromString("#DD000000");
                return new SolidColorBrush(rtnColor);
            }
            catch (Exception)
            {
                rtnColor = (Color) ConvertFromString("#DD000000");
                return new SolidColorBrush(rtnColor);
            }
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            object firstObject2Convert = value;

            try
            {
                bool visible = firstObject2Convert != null && (bool) firstObject2Convert;
                return visible ? Visibility.Visible : Visibility.Hidden;
            }
            catch (Exception)
            {
                return Visibility.Hidden;
            }
        }
    }
}