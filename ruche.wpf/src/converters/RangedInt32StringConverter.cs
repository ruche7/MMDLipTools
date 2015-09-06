using System.Windows.Data;

namespace ruche.wpf.converters
{
    /// <summary>
    /// System.Int32 型の値を、範囲内に丸めつつ文字列に変換するクラス。
    /// </summary>
    [ValueConversion(typeof(int), typeof(string))]
    public class RangedInt32StringConverter : RangedIntegerStringConverter<int>
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public RangedInt32StringConverter() { }
    }
}
