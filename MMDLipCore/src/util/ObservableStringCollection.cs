using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ruche.util
{
    /// <summary>
    /// 文字列を保持する ObservableCollection の特化クラス。
    /// </summary>
    /// <remarks>
    /// XAML要素として利用するための定義。
    /// </remarks>
    public class ObservableStringCollection : ObservableCollection<string>
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public ObservableStringCollection() : base()
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="src">初期値。</param>
        public ObservableStringCollection(IEnumerable<string> src) : base(src)
        {
        }
    }
}
