using System.Windows.Data;

namespace ruche.wpf.converters
{
    /// <summary>
    /// System.Single 型の値を、範囲内に丸めつつ文字列に変換するクラス。
    /// </summary>
    [ValueConversion(typeof(float), typeof(string))]
    public class RangedSingleStringConverter : RangedRealStringConverter<float>
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public RangedSingleStringConverter() { }
    }
}
