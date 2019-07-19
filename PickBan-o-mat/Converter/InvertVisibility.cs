using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PickBan_o_mat.Converter
{
    public class InvertVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
            {
                throw new InvalidOperationException("Converter can only convert to value of type Visibility.");
            }

            if (value == null)
            {
                return Visibility.Visible;
            }

            Visibility vis = (Visibility) value;
            return vis == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("Invalid call - one way only");
        }
    }
}