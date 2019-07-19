using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PickBan_o_mat.Converter
{
    public class InvertBoolToVisible : IValueConverter
    {
        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            object firstObject2Convert = values;

            try
            {
                if (firstObject2Convert != null)
                {
                    Visibility visible = (Visibility) firstObject2Convert;
                    switch (visible)
                    {
                        case Visibility.Visible:
                            return false;

                        case Visibility.Collapsed:
                            return true;

                        case Visibility.Hidden:
                            return true;

                        default:
                            return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return true;
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