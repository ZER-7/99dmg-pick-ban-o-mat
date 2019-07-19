using System;
using System.Globalization;
using System.Windows.Data;

namespace PickBan_o_mat.Converter
{
    public class InvertBool : IValueConverter
    {
        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            object firstObject2Convert = values;

            try
            {
                bool visible = firstObject2Convert != null && (bool) firstObject2Convert;

                return !visible;
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

                return !visible;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}