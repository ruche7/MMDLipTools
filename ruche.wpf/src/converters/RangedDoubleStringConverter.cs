using System.Windows.Data;

namespace ruche.wpf.converters
{
    /// <summary>
    /// System.Double 型の値を、範囲内に丸めつつ文字列に変換するクラス。
    /// </summary>
    [ValueConversion(typeof(double), typeof(string))]
    public class RangedDoubleStringConverter : RangedRealStringConverter<double>
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public RangedDoubleStringConverter() { }
    }
}
