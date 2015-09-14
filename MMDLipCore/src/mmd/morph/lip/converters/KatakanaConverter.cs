using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace ruche.mmd.morph.lip.converters
{
    /// <summary>
    /// 文字列をカタカナに変換するクラス。
    /// </summary>
    public class KatakanaConverter
    {
        /// <summary>
        /// 濁点変換テーブル。
        /// </summary>
        private static readonly Dictionary<char, char> SonantTable =
            new Dictionary<char, char>
                {
                    { 'カ', 'ガ' },
                    { 'キ', 'ギ' },
                    { 'ク', 'グ' },
                    { 'ケ', 'ゲ' },
                    { 'コ', 'ゴ' },
                    { 'サ', 'ザ' },
                    { 'シ', 'ジ' },
                    { 'ス', 'ズ' },
                    { 'セ', 'ゼ' },
                    { 'ソ', 'ゾ' },
                    { 'タ', 'ダ' },
                    { 'チ', 'ヂ' },
                    { 'ツ', 'ヅ' },
                    { 'テ', 'デ' },
                    { 'ト', 'ド' },
                    { 'ハ', 'バ' },
                    { 'ヒ', 'ビ' },
                    { 'フ', 'ブ' },
                    { 'ヘ', 'ベ' },
                    { 'ホ', 'ボ' },
                    { 'ワ', 'ヷ' },
                    { 'ヰ', 'ヸ' },
                    { 'ウ', 'ヴ' },
                    { 'ヱ', 'ヹ' },
                    { 'ヲ', 'ヺ' },
                    { 'ヽ', 'ヾ' },
                };

        /// <summary>
        /// 半濁点変換テーブル。
        /// </summary>
        private static readonly Dictionary<char, char> PsoundTable =
            new Dictionary<char, char>
                {
                    { 'ハ', 'パ' },
                    { 'ヒ', 'ピ' },
                    { 'フ', 'プ' },
                    { 'ヘ', 'ペ' },
                    { 'ホ', 'ポ' },
                };

        /// <summary>
        /// カタカナのみで構成された文字列にマッチする正規表現。
        /// </summary>
        private static readonly Regex rexKatakanaOnly =
            new Regex(@"^\p{IsKatakana}+$");

        /// <summary>
        /// 文字がカタカナであるか否かを取得する。
        /// </summary>
        /// <param name="c">文字。</param>
        /// <returns>カタカナならば true 。</returns>
        private static bool IsKatakana(char c)
        {
            return rexKatakanaOnly.IsMatch(c.ToString());
        }

        /// <summary>
        /// 文字列をカタカナに変換する。
        /// </summary>
        /// <param name="src">文字列。</param>
        /// <returns>カタカナに変換された文字列。</returns>
        public string ConvertFrom(string src)
        {
            // ひらがなをカタカナに変換
            var text = Strings.StrConv(src, VbStrConv.Katakana, 0x411);

            // 濁点、半濁点を連結
            text = JoinSoundSymbol(text);

            // 繰り返し文字を変換
            text = ConvertRepeatLetter(text);

            return text;
        }

        /// <summary>
        /// カタカナ文字列中の濁点、半濁点を直前の文字と連結する。
        /// </summary>
        /// <param name="src">カタカナ文字列。</param>
        /// <returns>変換された文字列。</returns>
        private string JoinSoundSymbol(string src)
        {
            var dest = new StringBuilder();

            int len = src.Length;
            for (int i = 0; i < len; ++i)
            {
                char c = src[i];
                char next = (i < len - 1) ? src[i + 1] : '\0';

                char r;
                if (
                    (next == '゛' || next == 'ﾞ' || next == '\u3099') &&
                    SonantTable.TryGetValue(c, out r))
                {
                    dest.Append(r);
                    ++i;
                }
                else if (
                    (next == '゜' || next == 'ﾟ' || next == '\u309A') &&
                    PsoundTable.TryGetValue(c, out r))
                {
                    dest.Append(r);
                    ++i;
                }
                else
                {
                    dest.Append(c);
                }
            }

            return dest.ToString();
        }

        /// <summary>
        /// カタカナ文字列中の繰り返し文字を変換する。
        /// </summary>
        /// <param name="src">カタカナ文字列。</param>
        /// <returns>変換された文字列。</returns>
        private string ConvertRepeatLetter(string src)
        {
            var dest = new StringBuilder(src);

            int len = dest.Length;
            for (int i = 0; i < len - 1; ++i)
            {
                char c = dest[i];
                char next = dest[i + 1];

                if (next == 'ヽ' && IsKatakana(c))
                {
                    dest[i + 1] = c;
                }
                else if (next == 'ヾ' && IsKatakana(c))
                {
                    char r;
                    if (SonantTable.TryGetValue(c, out r))
                    {
                        dest[i + 1] = r;
                    }
                    else
                    {
                        dest[i + 1] = c;
                    }
                }
            }

            return dest.ToString();
        }
    }
}
