using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Expr = System.Linq.Expressions;

namespace ruche.wpf.converters
{
    /// <summary>
    /// 任意の数値型の値を、範囲内に丸めつつ文字列に変換するクラス。
    /// </summary>
    /// <typeparam name="T">数値型。</typeparam>
    public abstract class RangedNumberStringConverter<T> : IValueConverter
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
        /// <param name="toStringDelegate">
        /// 数値を文字列に単純変換するデリゲート。
        /// </param>
        protected RangedNumberStringConverter(Func<T, string> toStringDelegate)
        {
            if (toStringDelegate == null)
            {
                throw new ArgumentNullException("toStringDelegate");
            }

            this.toStringDelegate = toStringDelegate;
        }

        /// <summary>
        /// 値の有効範囲の最小値。
        /// </summary>
        public T MinValue
        {
            get { return minValue; }
            set
            {
                minValue = value;
                if (value.CompareTo(MaxValue) > 0)
                {
                    MaxValue = value;
                }
            }
        }
        private T minValue = GetStaticField<T>("MinValue");

        /// <summary>
        /// 値の有効範囲の最大値。
        /// </summary>
        public T MaxValue
        {
            get { return maxValue; }
            set
            {
                maxValue = value;
                if (value.CompareTo(MinValue) < 0)
                {
                    MinValue = value;
                }
            }
        }
        private T maxValue = GetStaticField<T>("MaxValue");

        /// <summary>
        /// 小数点以下の最小桁数。
        /// </summary>
        public int MinDecimalPlaces
        {
            get { return minDecimalPlaces; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "value",
                        value,
                        "MinDecimalPlaces requires the value of 0 or more.");
                }
                minDecimalPlaces = value;
            }
        }
        private int minDecimalPlaces = 0;

        /// <summary>
        /// 値を MinValue 以上かつ MaxValue 以下になるように丸める。
        /// </summary>
        /// <param name="value">値。</param>
        /// <returns>丸められた値。</returns>
        public T Round(T value)
        {
            return
                (value.CompareTo(MinValue) < 0) ? MinValue :
                (value.CompareTo(MaxValue) > 0) ? MaxValue : value;
        }

        /// <summary>
        /// T.TryParse 静的メソッドを呼び出すデリゲート定義。
        /// </summary>
        /// <param name="value">変換元の文字列。</param>
        /// <param name="result">変換結果の設定先。</param>
        /// <returns>変換に成功したならば true 。そうでなければ false 。</returns>
        private delegate bool TryParseDelegate(string value, out T result);

        /// <summary>
        /// T.TryParse 静的メソッドを呼び出すデリゲートを作成する。
        /// </summary>
        /// <returns></returns>
        private static TryParseDelegate MakeTryParseDelegate()
        {
            var valueExp = Expr.Expression.Parameter(typeof(string), "value");
            var resultExp =
                Expr.Expression.Parameter(typeof(T).MakeByRefType(), "result");

            var tryParseCallExp =
                Expr.Expression.Call(typeof(T), "TryParse", null, valueExp, resultExp);
            var tryParseLambda =
                Expr.Expression.Lambda<TryParseDelegate>(
                    tryParseCallExp,
                    valueExp,
                    resultExp);

            return tryParseLambda.Compile();
        }

        /// <summary>
        /// T.TryParse 静的メソッドを呼び出すデリゲート。
        /// </summary>
        private static readonly TryParseDelegate tryParseDelegate = MakeTryParseDelegate();

        /// <summary>
        /// T の静的フィールド値を取得する。
        /// </summary>
        /// <typeparam name="U">静的フィールドの型。</typeparam>
        /// <param name="name">静的フィールド名。</param>
        /// <returns>静的フィールド値。</returns>
        private static U GetStaticField<U>(string name)
        {
            return (U)typeof(T).GetField(name).GetValue(null);
        }

        /// <summary>
        /// 数値を文字列に単純変換するデリゲート。
        /// </summary>
        private readonly Func<T, string> toStringDelegate;

        #region IValueConverter 実装

        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            try
            {
                var s = toStringDelegate(Round((T)value));

                if (MinDecimalPlaces > 0)
                {
                    // 小数点文字列取得
                    var dsep =
                        CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

                    // 最も後方にある小数点を探す
                    int dpos = s.LastIndexOf(dsep);
                    if (dpos < 0)
                    {
                        // 見つからなければくっつける
                        s += dsep;
                        dpos = s.Length - 1;
                    }

                    // 足りない桁数分だけ '0' をくっつける
                    int curPlaces = s.Length - (dpos + dsep.Length);
                    int needCount = MinDecimalPlaces - curPlaces;
                    if (needCount > 0)
                    {
                        s += new string('0', needCount);
                    }
                }

                return s;
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
                T v = default(T);
                if (!tryParseDelegate(value as string, out v))
                {
                    return DependencyProperty.UnsetValue;
                }

                return Round(v);
            }
            catch { }

            return DependencyProperty.UnsetValue;
        }

        #endregion
    }
}
