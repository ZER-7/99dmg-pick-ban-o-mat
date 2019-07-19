using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PickBan_o_mat.Converter
{
    public class BoolToVisible : IValueConverter
    {
        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            object firstObject2Convert = values;

            try
            {
                bool visible = firstObject2Convert != null && (bool) firstObject2Convert;
                return visible ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception)
            {
                return false;
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