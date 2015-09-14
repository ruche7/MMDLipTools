using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.VisualBasic;

namespace ruche.mmd.morph.lip.converters
{
    /// <summary>
    /// 英字と記号を読み仮名に変換するクラス。
    /// </summary>
    public class AlphaGlyphKanaConverter
    {
        /// <summary>
        /// 全角文字の読み仮名変換テーブル。
        /// </summary>
        private static readonly Dictionary<char, string> KanaTable =
            new Dictionary<char, string>
                {
                    { 'Ａ', "えー" },
                    { 'Ｂ', "びー" },
                    { 'Ｃ', "しー" },
                    { 'Ｄ', "でぃー" },
                    { 'Ｅ', "いー" },
                    { 'Ｆ', "えふ" },
                    { 'Ｇ', "じー" },
                    { 'Ｈ', "えっち" },
                    { 'Ｉ', "あい" },
                    { 'Ｊ', "じぇー" },
                    { 'Ｋ', "けー" },
                    { 'Ｌ', "える" },
                    { 'Ｍ', "えむ" },
                    { 'Ｎ', "えぬ" },
                    { 'Ｏ', "おー" },
                    { 'Ｐ', "ぴー" },
                    { 'Ｑ', "きゅー" },
                    { 'Ｒ', "あーる" },
                    { 'Ｓ', "えす" },
                    { 'Ｔ', "てぃー" },
                    { 'Ｕ', "ゆー" },
                    { 'Ｖ', "ぶい" },
                    { 'Ｗ', "だぶりゅー" },
                    { 'Ｘ', "えっくす" },
                    { 'Ｙ', "わい" },
                    { 'Ｚ', "ぜっと" },

                    { '＃', "しゃーぷ" },
                    { '♯', "しゃーぷ" },
                    { '＄', "どる" },
                    { '\\', "えん" },
                    { '￥', "えん" },
                    { '％', "ぱーせんと" },
                    { '＆', "あんど" },
                    { '＊', "あすたりすく" },
                    { '＋', "ぷらす" },
                    { '，', "、" },
                    { '－', "まいなす" },
                    { '．', "。" },
                    { '／', "すらっしゅ" },
                    { '＼', "ばっくすらっしゅ" },
                    { '＝', "いこーる" },
                    { '＠', "あっとまーく" },
                    { '＿', "あんだーばー" },

                    { '~',  "ちるだ" },
                    { '～', "ー" },

                    //{ '：', "：" },
                    //{ '；', "；" },
                    //{ '！', "！" },
                    //{ '？', "？" },
                    //{ '＾', "＾" },
                    //{ '｜', "｜" },
                    //{ '・', "・" },

                    { '”', "" },
                    { '“', "" },
                    { '’', "" },
                    { '‘', "" },
                    { '｀', "" },
                    { '（', "" },
                    { '）', "" },
                    { '＜', "" },
                    { '＞', "" },
                    { '［', "" },
                    { '］', "" },
                    { '｛', "" },
                    { '｝', "" },
                    { '〈', "" },
                    { '〉', "" },
                    { '《', "" },
                    { '》', "" },
                    { '〔', "" },
                    { '〕', "" },
                    { '「', "" },
                    { '」', "" },
                    { '『', "" },
                    { '』', "" },
                    { '【', "" },
                    { '】', "" },

                    { '\u30FF', "コト" },

                    // StrConv 対策
                    { 'ゕ', "カ" },
                    { 'ゖ', "ケ" },
                    { '\u3099', "゛" },
                    { '\u309A', "゜" },
                };

        /// <summary>
        /// 全角に変換して構わない文字列にマッチする正規表現。
        /// </summary>
        private static readonly Regex rexToZenkaku =
            new Regex("[^,\\.~ ゛ﾞ\u3099゜ﾟ\u309A]+");

        /// <summary>
        /// 英字と記号を読み仮名に変換する。
        /// </summary>
        /// <param name="src">文字列。</param>
        /// <returns>変換された文字列。</returns>
        public string ConvertFrom(string src)
        {
            var dest = new StringBuilder();

            // 半角を全角に変換(一部記号を除く)
            var text = rexToZenkaku.Replace(
                src,
                m => Strings.StrConv(m.Value, VbStrConv.Wide, 0x411));

            // 小文字を大文字に変換
            text = text.ToUpper(CultureInfo.CurrentCulture);

            // 1文字ずつ処理(サロゲートペア考慮)
            var e = StringInfo.GetTextElementEnumerator(text);
            while (e.MoveNext())
            {
                string c = e.Current.ToString();
                string r;
                if (c.Length > 1)
                {
                    // サロゲートペアは全角スペースにしておく
                    dest.Append("　");
                }
                else if (KanaTable.TryGetValue(c[0], out r))
                {
                    dest.Append(r);
                }
                else
                {
                    dest.Append(c);
                }
            }

            return dest.ToString();
        }
    }
}
