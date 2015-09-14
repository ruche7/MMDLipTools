using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace ruche.mmd.morph.lip.converters
{
    #region NumberPhoneType 定義

    /// <summary>
    /// 整数部の読み仮名の構成要素。
    /// </summary>
    internal enum NumberPhoneType
    {
        Begin = 0,

        None = Begin,
        Point,

        DigitBegin,
        Digit0 = DigitBegin,
        Digit1,
        Digit2,
        Digit3,
        Digit4,
        Digit5,
        Digit6,
        Digit7,
        Digit8,
        Digit9,
        DigitEnd,

        BaseBegin = DigitEnd,
        Ten = BaseBegin,
        Hundred,
        Thousand,
        BaseEnd,

        BigBegin = BaseEnd,
        Man = BigBegin,
        Oku,
        Cyou,
        Kei,
        Gai,
        Jo,
        Jou,
        Kou,
        Kan,
        Sei,
        Sai,
        Goku,
        Gougasya,
        Asougi,
        Nayuta,
        Fukashigi,
        Infinity,
        BigEnd,

        End = BigEnd,
    }

    /// <summary>
    /// NumberPhoneType の拡張メソッドを定義するクラス。
    /// </summary>
    internal static class NumberPhoneTypeExtension
    {
        /// <summary>
        /// 自身が有効な値であるか否かを取得する。
        /// </summary>
        /// <param name="self">自分自身。</param>
        /// <returns>有効な値ならば true 。</returns>
        public static bool IsDefined(this NumberPhoneType self)
        {
            return (
                self >= NumberPhoneType.Begin &&
                self < NumberPhoneType.End);
        }

        /// <summary>
        /// 自身が None を表すか否かを取得する。
        /// </summary>
        /// <param name="self">自分自身。</param>
        /// <returns>None ならば true 。</returns>
        public static bool IsNone(this NumberPhoneType self)
        {
            return (self == NumberPhoneType.None);
        }

        /// <summary>
        /// 自身が Point を表すか否かを取得する。
        /// </summary>
        /// <param name="self">自分自身。</param>
        /// <returns>Point ならば true 。</returns>
        public static bool IsPoint(this NumberPhoneType self)
        {
            return (self == NumberPhoneType.Point);
        }

        /// <summary>
        /// 自身が None または Point を表すか否かを取得する。
        /// </summary>
        /// <param name="self">自分自身。</param>
        /// <returns>None または Point ならば true 。</returns>
        public static bool IsNoneOrPoint(this NumberPhoneType self)
        {
            return (self.IsNone() || self.IsPoint());
        }

        /// <summary>
        /// 自身が 0 ～ 9 の数字を表すか否かを取得する拡張メソッド。
        /// </summary>
        /// <param name="self">自分自身。</param>
        /// <returns>Digit0 ～ Digit9 ならば true 。</returns>
        public static bool IsDigit(this NumberPhoneType self)
        {
            return (
                self >= NumberPhoneType.DigitBegin &&
                self < NumberPhoneType.DigitEnd);
        }

        /// <summary>
        /// 自身が十の位～千の位を表すか否かを取得する拡張メソッド。
        /// </summary>
        /// <param name="self">自分自身。</param>
        /// <returns>Ten, Hundred, Thousand のいずれかならば true 。</returns>
        public static bool IsBase(this NumberPhoneType self)
        {
            return (
                self >= NumberPhoneType.BaseBegin &&
                self < NumberPhoneType.BaseEnd);
        }

        /// <summary>
        /// 自身が10000の乗算桁を表すか否かを取得する拡張メソッド。
        /// </summary>
        /// <param name="self">自分自身。</param>
        /// <returns>Man ～ Infinity のいずれかならば true 。</returns>
        public static bool IsBig(this NumberPhoneType self)
        {
            return (
                self >= NumberPhoneType.BigBegin &&
                self < NumberPhoneType.BigEnd);
        }
    }

    #endregion

    /// <summary>
    /// 文字列中の数字列を読み仮名に変換するクラス。
    /// </summary>
    public class DigitKanaConverter
    {
        #region 読み仮名定義

        /// <summary>
        /// NumberPhoneType に対応する規定の読み仮名。
        /// </summary>
        private static readonly string[] DefaultNumberKanas =
            {
                "",
                "てん",

                "ぜろ",
                "いち",
                "に",
                "さん",
                "よん",
                "ご",
                "ろく",
                "なな",
                "はち",
                "きゅう",

                "じゅう",
                "ひゃく",
                "せん",

                "まん",
                "おく",
                "ちょう",
                "けい",
                "がい",
                "じょ",
                "じょう",
                "こう",
                "かん",
                "せい",
                "さい",
                "ごく",
                "ごうがしゃ",
                "あそうぎ",
                "なゆた",
                "ふかしぎ",
                "むりょうたいすう",
            };

        /// <summary>
        /// 整数部の構成要素の読み仮名を決定するデリゲート。
        /// </summary>
        /// <param name="type">対象の種別。</param>
        /// <param name="prevType">先行要素の種別。</param>
        /// <param name="nextType">後続要素の種別。</param>
        /// <returns>読み仮名。</returns>
        private delegate string NumberKanaDecider(
            NumberPhoneType type,
            NumberPhoneType prevType,
            NumberPhoneType nextType);

        /// <summary>
        /// 整数部の構成要素の既定の読みを取得する。
        /// </summary>
        /// <param name="type">対象の種別。</param>
        /// <returns>読み仮名。</returns>
        private static string GetDefaultNumberKana(NumberPhoneType type)
        {
            return DefaultNumberKanas[(int)type];
        }

        /// <summary>
        /// NumberPhoneType に対応する読み仮名決定デリゲート。
        /// </summary>
        private static readonly NumberKanaDecider[] NumberKanaDeciders =
            {
                // None
                (t, prev, next) => GetDefaultNumberKana(t),

                // Point
                (t, prev, next) => GetDefaultNumberKana(t),

                // Digit0
                (t, prev, next) =>
                {
                    if (!prev.IsNone() || !next.IsNoneOrPoint())
                    {
                        return string.Empty;
                    }
                    if (next.IsPoint())
                    {
                        return "れー";
                    }
                    return GetDefaultNumberKana(t);
                },

                // Digit1
                (t, prev, next) =>
                {
                    if (next.IsBase())
                    {
                        return string.Empty;
                    }
                    if (
                        next.IsPoint() ||
                        next == NumberPhoneType.Cyou ||
                        next == NumberPhoneType.Kei)
                    {
                        return "いっ";
                    }
                    return GetDefaultNumberKana(t);
                },

                // Digit2
                (t, prev, next) =>
                {
                    if (next.IsPoint())
                    {
                        return "にー";
                    }
                    return GetDefaultNumberKana(t);
                },

                // Digit3
                (t, prev, next) => GetDefaultNumberKana(t),

                // Digit4
                (t, prev, next) => GetDefaultNumberKana(t),

                // Digit5
                (t, prev, next) =>
                {
                    if (next.IsPoint())
                    {
                        return "ごー";
                    }
                    return GetDefaultNumberKana(t);
                },

                // Digit6
                (t, prev, next) =>
                {
                    if (
                        next == NumberPhoneType.Hundred ||
                        next == NumberPhoneType.Kei)
                    {
                        return "ろっ";
                    }
                    return GetDefaultNumberKana(t);
                },

                // Digit7
                (t, prev, next) => GetDefaultNumberKana(t),

                // Digit8
                (t, prev, next) =>
                {
                    if (
                        next.IsPoint() ||
                        next == NumberPhoneType.Hundred ||
                        next == NumberPhoneType.Thousand ||
                        next == NumberPhoneType.Cyou ||
                        next == NumberPhoneType.Kei)
                    {
                        return "はっ";
                    }
                    return GetDefaultNumberKana(t);
                },

                // Digit9
                (t, prev, next) => GetDefaultNumberKana(t),

                // Ten
                (t, prev, next) =>
                {
                    if (
                        next.IsPoint() ||
                        next == NumberPhoneType.Cyou ||
                        next == NumberPhoneType.Kei ||
                        next == NumberPhoneType.Kou ||
                        next == NumberPhoneType.Kan ||
                        next == NumberPhoneType.Sei ||
                        next == NumberPhoneType.Sai)
                    {
                        return "じゅっ";
                    }
                    return GetDefaultNumberKana(t);
                },
                
                // Hundred
                (t, prev, next) =>
                {
                    if (prev == NumberPhoneType.Digit3)
                    {
                        return "びゃく";
                    }
                    if (
                        prev == NumberPhoneType.Digit6 ||
                        prev == NumberPhoneType.Digit8)
                    {
                        return "ぴゃく";
                    }
                    return GetDefaultNumberKana(t);
                },

                // Thousand
                (t, prev, next) =>
                {
                    if (prev == NumberPhoneType.Digit3)
                    {
                        return "ぜん";
                    }
                    return GetDefaultNumberKana(t);
                },

                // Man
                (t, prev, next) => GetDefaultNumberKana(t),

                // Oku
                (t, prev, next) => GetDefaultNumberKana(t),

                // Cyou
                (t, prev, next) => GetDefaultNumberKana(t),

                // Kei
                (t, prev, next) => GetDefaultNumberKana(t),

                // Gai
                (t, prev, next) => GetDefaultNumberKana(t),

                // Jo
                (t, prev, next) => GetDefaultNumberKana(t),

                // Jou
                (t, prev, next) => GetDefaultNumberKana(t),

                // Kou
                (t, prev, next) => GetDefaultNumberKana(t),

                // Kan
                (t, prev, next) => GetDefaultNumberKana(t),

                // Sei
                (t, prev, next) => GetDefaultNumberKana(t),

                // Sai
                (t, prev, next) => GetDefaultNumberKana(t),

                // Goku
                (t, prev, next) => GetDefaultNumberKana(t),

                // Gougasya
                (t, prev, next) => GetDefaultNumberKana(t),

                // Asougi
                (t, prev, next) => GetDefaultNumberKana(t),

                // Nayuta
                (t, prev, next) => GetDefaultNumberKana(t),

                // Fukashigi
                (t, prev, next) => GetDefaultNumberKana(t),

                // Infinity
                (t, prev, next) => GetDefaultNumberKana(t),
            };

        /// <summary>
        /// 小数部の各桁数字(最終桁を除く)の読み仮名。
        /// </summary>
        private static readonly string[] RemainKanas =
            {
                "ぜろ",
                "いち",
                "にー",
                "さん",
                "よん",
                "ごー",
                "ろく",
                "なな",
                "はち",
                "きゅう",
            };

        /// <summary>
        /// 小数部の最終桁数字の読み仮名。小数部がちょうど2桁の場合は用いない。
        /// </summary>
        private static readonly string[] LastRemainKanas =
            {
                "ぜろ",
                "いち",
                "に",
                "さん",
                "よん",
                "ご",
                "ろく",
                "なな",
                "はち",
                "きゅう",
            };

        #endregion

        /// <summary>
        /// 数字にマッチする正規表現。
        /// </summary>
        private static readonly Regex rexNumber =
            new Regex(@"([1-9１-９][0-9０-９]*|[0０])([\.\．][0-9０-９]+)?");

        /// <summary>
        /// 整数部要素の読み仮名を決定する。
        /// </summary>
        /// <param name="type">対象種別。</param>
        /// <param name="prevType">先行要素の種別。</param>
        /// <param name="nextType">後続要素の種別。</param>
        /// <returns>読み仮名。</returns>
        private static string DecideNumberKana(
            NumberPhoneType type,
            NumberPhoneType prevType,
            NumberPhoneType nextType)
        {
            if (!type.IsDefined())
            {
                return string.Empty;
            }
            return NumberKanaDeciders[(int)type](type, prevType, nextType);
        }

        /// <summary>
        /// 文字列中の数字列を読み仮名に変換する。
        /// </summary>
        /// <param name="src">文字列。</param>
        /// <returns>変換された文字列。</returns>
        public string ConvertFrom(string src)
        {
            return rexNumber.Replace(src, m => GetPhonetic(m.Value));
        }

        /// <summary>
        /// 数字を表す文字列の読み仮名を取得する。
        /// </summary>
        /// <param name="src">数字を表す文字列。</param>
        /// <returns>文字列の読み仮名。</returns>
        private string GetPhonetic(string src)
        {
            var dest = new StringBuilder();

            // 整数部と小数部の半角文字列を取得
            var numParts = GetNumberParts(src);
            if (numParts == null || numParts.Length == 0)
            {
                return string.Empty;
            }

            // 小数部の有無
            bool remainExist =
                (numParts.Length >= 2 && numParts[1].Length > 0);

            // 整数部の読み仮名をバッファに追加
            AppendNumberPhonetic(numParts[0], remainExist, dest);

            if (remainExist)
            {
                // 小数部の読み仮名をバッファに追加
                AppednRemainPhonetic(numParts[1], dest);
            }

            return dest.ToString();
        }

        /// <summary>
        /// 数字を表す文字列を半角化、カンマ除去し、整数部と小数部に分ける。
        /// </summary>
        /// <param name="src">数字を表す文字列。</param>
        /// <returns>整数部文字列と小数部文字列の配列。</returns>
        private string[] GetNumberParts(string src)
        {
            // 全角を半角に変換
            string text = Strings.StrConv(src, VbStrConv.Narrow, 0x411);

            // カンマを削除
            text = text.Replace(",", "");

            // ピリオドで区切る
            return text.Split('.');
        }

        /// <summary>
        /// 整数部を表す文字列の読み仮名をバッファに追加する。
        /// </summary>
        /// <param name="src">数字列。</param>
        /// <param name="remainExist">小数部に続くならば true 。</param>
        /// <param name="dest">追加先の文字列バッファ。</param>
        /// <returns>追加された文字列長。</returns>
        private int AppendNumberPhonetic(
            string src,
            bool remainExist,
            StringBuilder dest)
        {
            int appendLen = 0;

            int len = src.Length;
            NumberPhoneType prevType = NumberPhoneType.None;

            for (int i = 0; i < len; )
            {
                // 4桁単位の位置を算出
                int bigPos = (len - 1 - i) / 4;

                // 4桁単位で区切って取得
                string part = src.Substring(i, len - i - bigPos * 4);

                // 取得した文字数分だけ位置を進める
                i += part.Length;

                // 4桁単位種別を決定
                NumberPhoneType baseType;
                if (bigPos == 0)
                {
                    baseType = remainExist ?
                        NumberPhoneType.Point :
                        NumberPhoneType.None;
                }
                else
                {
                    baseType =
                        (NumberPhoneType)(NumberPhoneType.BigBegin + bigPos - 1);
                    if (!baseType.IsBig())
                    {
                        // 大きすぎるのでスキップ
                        continue;
                    }
                }

                // 読みを追加
                NumberPhoneType lastType;
                appendLen +=
                    AppendThousandPartPhonetic(
                        part,
                        prevType,
                        baseType,
                        dest,
                        out lastType);

                // 先行種別を更新
                prevType = lastType;
            }

            return appendLen;
        }

        /// <summary>
        /// 小数部を表す文字列の読み仮名をバッファに追加する。
        /// </summary>
        /// <param name="src">数字列。</param>
        /// <param name="dest">追加先の文字列バッファ。</param>
        /// <returns>追加された文字列長。</returns>
        private int AppednRemainPhonetic(string src, StringBuilder dest)
        {
            int oldLen = dest.Length;

            int len = src.Length;
            for (int i = 0; i < len; ++i)
            {
                // 2桁以外の最終桁は読み方が異なる
                // 0.2   … れーてんに
                // 0.22  … れーてんにーにー
                // 0.222 … れーてんにーにーに
                var kanas =
                    (len != 2 && i == len - 1) ? LastRemainKanas : RemainKanas;

                int d = ParseChar(src[i]);
                dest.Append(kanas[d]);
            }

            return (dest.Length - oldLen);
        }

        /// <summary>
        /// 4桁単位での読み仮名をバッファに追加する。
        /// </summary>
        /// <param name="src">数字列。1～4文字。</param>
        /// <param name="prevType">先行要素の種別。</param>
        /// <param name="baseType">4桁単位要素の種別。</param>
        /// <param name="dest">追加先の文字列バッファ。</param>
        /// <param name="lastType">
        /// 最後に追加された要素種別の設定先。
        /// 何も追加されなかった場合は prevType が設定される。
        /// </param>
        /// <returns>追加された文字列長。</returns>
        private int AppendThousandPartPhonetic(
            string src,
            NumberPhoneType prevType,
            NumberPhoneType baseType,
            StringBuilder dest,
            out NumberPhoneType lastType)
        {
            int oldLen = dest.Length;

            // とりあえず prevType を設定しておく
            lastType = prevType;

            // 頭の "0" を削除
            string text = src.TrimStart('0');

            // 長さを算出
            int len = text.Length;

            if (len == 0)
            {
                // "0" のみだった場合
                string zero =
                    DecideNumberKana(
                        NumberPhoneType.Digit0,
                        lastType,
                        baseType);
                if (zero.Length > 0)
                {
                    dest.Append(zero);
                    lastType = NumberPhoneType.Digit0;
                }
            }
            else
            {
                // 種別配列を作成(baseType は含めない)
                var types = new List<NumberPhoneType>();
                for (int i = 0; i < len; ++i)
                {
                    // 数字
                    int d = ParseChar(text[i]);
                    if (d == 0)
                    {
                        // "0" ならば後続も含めてスキップ
                        continue;
                    }
                    types.Add(
                        (NumberPhoneType)(NumberPhoneType.DigitBegin + d));

                    // 数字の後続
                    int pos = (len - 1) - i - 1; // 0:十, 1:百, 2:千
                    if (pos >= 0)
                    {
                        types.Add(
                            (NumberPhoneType)(NumberPhoneType.BaseBegin + pos));
                    }
                }

                // 順番に処理
                for (int i = 0; i < types.Count; ++i)
                {
                    NumberPhoneType t = types[i];
                    NumberPhoneType next =
                        (i + 1 < types.Count) ? types[i + 1] : baseType;
                    string s = DecideNumberKana(t, lastType, next);
                    if (s.Length > 0)
                    {
                        dest.Append(s);
                        lastType = t;
                    }
                }
            }

            // 以下のいずれかの場合は baseType を処理
            // - baseType == NumberPhoneType.Point
            // - 1文字以上追加があった
            if (baseType.IsPoint() || dest.Length > oldLen)
            {
                string baseText =
                    DecideNumberKana(baseType, lastType, NumberPhoneType.None);
                if (baseText.Length > 0)
                {
                    dest.Append(baseText);
                    lastType = baseType;
                }
            }

            return (dest.Length - oldLen);
        }

        /// <summary>
        /// 文字を数値に変換する。
        /// </summary>
        /// <param name="c">文字。</param>
        /// <returns>数値。変換できなければ 0 。</returns>
        private int ParseChar(char c)
        {
            return char.IsDigit(c) ? int.Parse(c.ToString()) : 0;
        }
    }
}
