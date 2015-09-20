using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ruche.wpf.converters
{
    /// <summary>
    /// 真偽値と表示状態値との変換および逆変換を行うクラス。
    /// </summary>
    //[ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanVisibilityConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            try
            {
                if (!(value is bool))
                {
                    return DependencyProperty.UnsetValue;
                }

                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }
            catch { }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            try
            {
                if (!(value is Visibility))
                {
                    return DependencyProperty.UnsetValue;
                }

                return ((Visibility)value == Visibility.Visible);
            }
            catch { }

            return DependencyProperty.UnsetValue;
        }
    }
}
