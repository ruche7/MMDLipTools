using System;

namespace ruche.wpf.converters
{
    /// <summary>
    /// 任意の実数型の値を、範囲内に丸めつつ文字列に変換するクラス。
    /// </summary>
    /// <typeparam name="T">実数型。</typeparam>
    public class RangedRealStringConverter<T> : RangedNumberStringConverter<T>
        where T :
            struct,
            IComparable,
            IFormattable,
            IConvertible,
            IComparable<T>,
            IEquatable<T>
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public RangedRealStringConverter()
            : base(v => string.Format("{0:0.###############}", v))
        {
        }
    }
}
