using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PickBan_o_mat.Converter
{
    public class VisibleToBool : IValueConverter
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
                            return true;

                        case Visibility.Collapsed:
                            return false;

                        case Visibility.Hidden:
                            return false;

                        default:
                            return false;
                    }
                }
                else
                {
                    return false;
                }
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