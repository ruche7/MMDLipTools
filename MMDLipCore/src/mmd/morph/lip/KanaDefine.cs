using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ruche.mmd.morph.lip
{
    /// <summary>
    /// カナ文字に関する定義をまとめた静的クラス。
    /// </summary>
    public static class KanaDefine
    {
        /// <summary>
        /// カタカナテーブル。非発音文字を除く。
        /// </summary>
        private static readonly char[] KatakanaTable = (
            "アイウエオカキクケコサシスセソタチツテトナニヌネノ" +
            "ハヒフヘホマミムメモヤ\0ユ\0ヨラリルレロワヰ\0ヱヲ" +
            "ガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポ" +
            "ヷヸヴヹヺァィゥェォャ\0ュ\0ョヮ\0\0\0\0ヵ\0\0ヶ\0").ToCharArray();

        /// <summary>
        /// 母音文字の配列。
        /// </summary>
        private static readonly char[] VowelLetters = "アイウエオ".ToCharArray();

        /// <summary>
        /// 一旦口を閉じるカタカナ文字の配列。
        /// </summary>
        private static readonly char[] PreCloseLetters =
            "マミムメモバビブベボパピプペポヷヸヴヹヺ".ToCharArray();

        /// <summary>
        /// 一旦半分口を閉じるカタカナ文字の配列。
        /// </summary>
        private static readonly char[] PreHalfCloseLetters =
            "ワヰヱヲヮ".ToCharArray();

        /// <summary>
        /// 促音として扱う文字の配列。
        /// </summary>
        private static readonly char[] SmallTsuLetters = "ッｯ！!".ToCharArray();

        /// <summary>
        /// 長音として扱う文字の配列。
        /// </summary>
        private static readonly char[] LongSoundLetters = "ーｰ～〜".ToCharArray();

        /// <summary>
        /// 指定したカタカナ文字の段位置を取得する。
        /// </summary>
        /// <param name="c">カタカナ文字。</param>
        /// <returns>
        /// 段位置。あ段～お段が 0 ～ 4 に対応する。
        /// 対応する段が無い場合は -1 。
        /// </returns>
        public static int GetRow(char c)
        {
            int index = (c == '\0') ? -1 : Array.IndexOf(KatakanaTable, c);
            return (index < 0) ? -1 : (index % VowelLetters.Length);
        }

        /// <summary>
        /// 指定したカタカナ文字と同じ行で指定した段位置のカタカナ文字を取得する。
        /// </summary>
        /// <param name="c">カタカナ文字。</param>
        /// <param name="row">段位置。 0 ～ 4 。</param>
        /// <returns>カタカナ文字。該当する文字が無い場合は '\0' 。</returns>
        public static char GetSameColumnLetter(char c, int row)
        {
            int index = (c == '\0' || row < 0 || row >= VowelLetters.Length) ?
                -1 :
                Array.IndexOf(KatakanaTable, c);
            return (index < 0) ?
                '\0' :
                KatakanaTable[index - (index % VowelLetters.Length) + row];
        }

        /// <summary>
        /// 母音文字の配列を取得する。
        /// </summary>
        /// <returns>母音文字の配列。</returns>
        public static char[] GetVowelLetters()
        {
            return (char[])VowelLetters.Clone();
        }

        /// <summary>
        /// 一旦口を閉じる文字の配列を取得する。
        /// </summary>
        /// <returns>一旦口を閉じる文字の配列。</returns>
        public static char[] GetPreCloseLetters()
        {
            return (char[])PreCloseLetters.Clone();
        }

        /// <summary>
        /// 一旦半分口を閉じる文字の配列を取得する。
        /// </summary>
        /// <returns>一旦半分口を閉じる文字の配列。</returns>
        public static char[] GetPreHalfCloseLetters()
        {
            return (char[])PreHalfCloseLetters.Clone();
        }

        /// <summary>
        /// 促音として扱う文字の配列を取得する。
        /// </summary>
        /// <returns>促音として扱う文字の配列。 0 番目が代表文字。</returns>
        public static char[] GetSmallTsuLetters()
        {
            return (char[])SmallTsuLetters.Clone();
        }

        /// <summary>
        /// 長音として扱う文字の配列を取得する。
        /// </summary>
        /// <returns>長音として扱う文字の配列。 0 番目が代表文字。</returns>
        public static char[] GetLongSoundLetters()
        {
            return (char[])LongSoundLetters.Clone();
        }

        /// <summary>
        /// 母音文字であるか否かを取得する。
        /// </summary>
        /// <param name="c">文字。</param>
        /// <returns>母音文字ならば true 。</returns>
        public static bool IsVowel(char c)
        {
            return (Array.IndexOf(VowelLetters, c) >= 0);
        }

        /// <summary>
        /// 一旦口を閉じる文字であるか否かを取得する。
        /// </summary>
        /// <param name="c">文字。</param>
        /// <returns>一旦口を閉じる文字ならば true 。</returns>
        public static bool IsPreClose(char c)
        {
            return (Array.IndexOf(PreCloseLetters, c) >= 0);
        }

        /// <summary>
        /// 一旦半分口を閉じる文字であるか否かを取得する。
        /// </summary>
        /// <param name="c">文字。</param>
        /// <returns>一旦口を閉じる文字ならば true 。</returns>
        public static bool IsPreHalfClose(char c)
        {
            return (Array.IndexOf(PreHalfCloseLetters, c) >= 0);
        }

        /// <summary>
        /// 長音として扱う文字であるか否かを取得する。
        /// </summary>
        /// <param name="c">文字。</param>
        /// <returns>長音として扱う文字ならば true 。</returns>
        public static bool IsLongSound(char c)
        {
            return (Array.IndexOf(LongSoundLetters, c) >= 0);
        }
    }
}
