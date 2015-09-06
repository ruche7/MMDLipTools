using System;

namespace ruche.wpf.converters
{
    /// <summary>
    /// 任意の整数型の値を、範囲内に丸めつつ文字列に変換するクラス。
    /// </summary>
    /// <typeparam name="T">整数型。</typeparam>
    public class RangedIntegerStringConverter<T> : RangedNumberStringConverter<T>
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
        public RangedIntegerStringConverter() : base(v => v.ToString())
        {
        }
    }
}
