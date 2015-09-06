using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ruche.wpf.converters
{
    /// <summary>
    /// 任意の列挙型の値を、
    /// パラメータの示す列挙名と等しいか否かを表す真偽値に変換するクラス。
    /// </summary>
    public class EnumValueEquationConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            try
            {
                // パラメータに渡された列挙値名を取得
                var param = parameter as string;
                if (param == null || !Enum.IsDefined(value.GetType(), param))
                {
                    return DependencyProperty.UnsetValue;
                }

                // 列挙値に変換
                var paramValue = Enum.Parse(value.GetType(), param);

                // 等しいか否かを返す
                return value.Equals(paramValue);
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
                // value が true である場合のみ変換可能
                if (!(value is bool) || !(bool)value)
                {
                    return DependencyProperty.UnsetValue;
                }

                // パラメータに渡された列挙値名を取得
                var param = parameter as string;
                if (param == null || !Enum.IsDefined(targetType, param))
                {
                    return DependencyProperty.UnsetValue;
                }

                // 列挙値に変換して返す
                return Enum.Parse(targetType, param);
            }
            catch { }

            return DependencyProperty.UnsetValue;
        }
    }
}
