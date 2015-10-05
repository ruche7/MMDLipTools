using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ruche.wpf.converters
{
    /// <summary>
    /// 任意の列挙型の値を、指定した列挙値と等しいか否かを表す真偽値に変換するクラス。
    /// </summary>
    public class EnumValueEquationConverter : DependencyObject, IValueConverter
    {
        /// <summary>
        /// Value 依存関係プロパティ。
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyPropertyFactory<EnumValueEquationConverter>.Register<object>(
                "Value",
                null);

        /// <summary>
        /// パラメータ未指定時に列挙値名として用いられる値を取得または設定する。
        /// </summary>
        public object Value
        {
            get { return this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// コンバータパラメータを基に、列挙値名として用いる値を決定する。
        /// </summary>
        /// <param name="parameter">コンバータパラメータ。</param>
        /// <returns>列挙値名として用いる値。</returns>
        private string DecideValueName(object parameter) =>
            (parameter ?? this.Value)?.ToString();

        #region IValueConverter 実装

        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            try
            {
                // 列挙値名を取得
                var param = DecideValueName(parameter);
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

                // 列挙値名を取得
                var param = DecideValueName(parameter);
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

        #endregion
    }
}
