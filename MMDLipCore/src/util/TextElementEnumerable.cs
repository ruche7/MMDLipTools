using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace ruche.util
{
    /// <summary>
    /// 文字列に対するUnicode文字の列挙子を公開するクラス。
    /// </summary>
    public class TextElementEnumerable : IEnumerable<string>
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="source">列挙対象となる文字列。</param>
        public TextElementEnumerable(string source)
        {
            this.Source = source;
        }

        /// <summary>
        /// 列挙対象文字列を取得する。
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Unicode文字の列挙子を取得する。
        /// </summary>
        /// <returns>Unicode文字の列挙子。</returns>
        public IEnumerator<string> GetEnumerator()
        {
            var e = StringInfo.GetTextElementEnumerator(this.Source);
            while (e.MoveNext())
            {
                yield return e.GetTextElement();
            }
        }

        #region IEnumerable の明示的実装

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
