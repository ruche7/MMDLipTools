using System;
using System.Collections.Generic;
using System.Text;

namespace ruche.mmd.morph.lip.converters
{
    /// <summary>
    /// 文字列を口パク用のカタカナに変換するクラス。
    /// </summary>
    /// <remarks>
    /// KatakanaConverter クラスの処理を内包し、
    /// 加えて「アアアア」を「アーーー」にしたり
    /// 「シャ」を「サ」にまとめる等、口パクを前提とした変換を行う。
    /// </remarks>
    public class KatakanaLipConverter
    {
        /// <summary>
        /// 拗音の情報を保持する構造体。
        /// </summary>
        private struct SmallKanaInfo
        {
            /// <summary>
            /// 対応する大文字。
            /// </summary>
            public char BigLetter { get; set; }

            /// <summary>
            /// 1音にまとめる対象となる文字の配列。
            /// </summary>
            public char[] JoinLetters { get; set; }
        }

        /// <summary>
        /// 拗音情報テーブル。
        /// </summary>
        private static readonly Dictionary<char, SmallKanaInfo> SmallKanaInfoTable =
            new Dictionary<char, SmallKanaInfo>
            {
                {
                    'ァ',
                    new SmallKanaInfo
                    {
                        BigLetter = 'ア',
                        JoinLetters = "フブヴ".ToCharArray(),
                    }
                },
                {
                    'ィ',
                    new SmallKanaInfo
                    {
                        BigLetter = 'イ',
                        JoinLetters = "フブヴ".ToCharArray(),
                    }
                },
                {
                    'ゥ',
                    new SmallKanaInfo
                    {
                        BigLetter = 'ウ',
                        JoinLetters = new char[0],
                    }
                },
                {
                    'ェ',
                    new SmallKanaInfo
                    {
                        BigLetter = 'エ',
                        JoinLetters = "フブヴ".ToCharArray(),
                    }
                },
                {
                    'ォ',
                    new SmallKanaInfo
                    {
                        BigLetter = 'オ',
                        JoinLetters = "フブヴ".ToCharArray(),
                    }
                },
                {
                    'ャ',
                    new SmallKanaInfo
                    {
                        BigLetter = 'ヤ',
                        JoinLetters = "キシチニヒミリギジヂビピ".ToCharArray(),
                    }
                },
                {
                    'ュ',
                    new SmallKanaInfo
                    {
                        BigLetter = 'ユ',
                        JoinLetters = "キシチニヒミリギジヂビピ".ToCharArray(),
                    }
                },
                {
                    'ョ',
                    new SmallKanaInfo
                    {
                        BigLetter = 'ヨ',
                        JoinLetters = "キシチニヒミリギジヂビピ".ToCharArray(),
                    }
                },
            };

        /// <summary>
        /// 長音文字。
        /// </summary>
        private static readonly char LongSoundLetter =
            KanaDefine.GetLongSoundLetters()[0];

        /// <summary>
        /// カタカナコンバータ。
        /// </summary>
        private readonly KatakanaConverter _kataConv = new KatakanaConverter();

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public KatakanaLipConverter()
        {
            this.IsColumnHaConverting = false;
        }

        /// <summary>
        /// 連続するハ行同段文字を長音化するか否かを取得または設定する。
        /// </summary>
        public bool IsColumnHaConverting { get; set; }

        /// <summary>
        /// 文字列を口パク前提のカタカナに変換する。
        /// </summary>
        /// <param name="src">文字列。</param>
        /// <returns>口パク前提のカタカナに変換された文字列。</returns>
        public string ConvertFrom(string src)
        {
            // カタカナに変換
            var text = _kataConv.ConvertFrom(src);

            // 拗音を処理する
            text = ConvertSmallChar(text);

            // 母音を処理する
            text = ConvertVowel(text);

            // ハ行を処理する
            if (this.IsColumnHaConverting)
            {
                text = ConvertColumnHa(text);
            }

            return text;
        }

        /// <summary>
        /// 拗音を処理する。
        /// </summary>
        /// <param name="src">カタカナ文字列。</param>
        /// <returns>変換された文字列。</returns>
        private string ConvertSmallChar(string src)
        {
            var dest = new StringBuilder();

            int len = src.Length;
            for (int i = 0; i < len; ++i)
            {
                char c = src[i];
                char next = (i < len - 1) ? src[i + 1] : '\0';

                // 後続文字が拗音か？
                SmallKanaInfo info;
                if (SmallKanaInfoTable.TryGetValue(next, out info))
                {
                    // 連結対象文字か？
                    char cc = KanaDefine.GetSameColumnLetter(
                        c,
                        KanaDefine.GetRow(next));
                    if (cc != '\0' && Array.IndexOf(info.JoinLetters, c) >= 0)
                    {
                        // 1音にまとめる
                        dest.Append(cc);
                    }
                    else
                    {
                        // 拗音を大文字化
                        dest.Append(c);
                        dest.Append(info.BigLetter);
                    }
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
        /// 母音を処理する。
        /// </summary>
        /// <param name="src">カタカナ文字列。</param>
        /// <returns>変換された文字列。</returns>
        /// <remarks>
        /// 連続する同段母音は長音として扱う。
        /// </remarks>
        private string ConvertVowel(string src)
        {
            var dest = new StringBuilder(src);

            int len = dest.Length;
            for (int i = 0; i < len - 1; ++i)
            {
                // 段位置取得
                var row = KanaDefine.GetRow(dest[i]);

                if (row >= 0)
                {
                    // 後続する同段母音文字および長音文字を長音に置換
                    for (int ni = i + 1; ni < len; ++ni)
                    {
                        char nc = dest[ni];

                        // 長音ではないか？(長音は問答無用でOK)
                        if (!KanaDefine.IsLongSound(nc))
                        {
                            // 母音以外は不可
                            if (!KanaDefine.IsVowel(nc))
                            {
                                break;
                            }

                            // 段位置が異なるならば不可
                            if (KanaDefine.GetRow(nc) != row)
                            {
                                break;
                            }
                        }

                        dest[ni] = LongSoundLetter;
                        i = ni;
                    }
                }
            }

            return dest.ToString();
        }

        /// <summary>
        /// ハ行を処理する。
        /// </summary>
        /// <param name="src">カタカナ文字列。</param>
        /// <returns>変換された文字列。</returns>
        /// <remarks>
        /// ハ行文字は直前の同段文字から続く場合に口の形が変わらないため、
        /// そのような場合は長音2つとして扱う。
        /// </remarks>
        private string ConvertColumnHa(string src)
        {
            var dest = new StringBuilder();

            int len = src.Length;
            for (int i = 0; i < len; ++i)
            {
                char c = src[i];

                // 文字追加
                dest.Append(c);

                // 段位置取得
                var row = KanaDefine.GetRow(c);

                if (row >= 0)
                {
                    // 後続する同段ハ行文字および長音文字を長音2つに置換
                    // 非対象文字か文字列終端になったらループを抜ける
                    for (++i; i < len; ++i)
                    {
                        char nc = src[i];

                        // 長音か？
                        if (KanaDefine.IsLongSound(nc))
                        {
                            // そのまま追加して次へ
                            dest.Append(nc);
                            continue;
                        }

                        // ハ行以外は不可
                        if (KanaDefine.GetSameColumnLetter(nc, 0) != 'ハ')
                        {
                            break;
                        }

                        // 段位置が異なるならば不可
                        if (KanaDefine.GetRow(nc) != row)
                        {
                            break;
                        }

                        // 長音2つ追加
                        dest.Append(LongSoundLetter, 2);
                    }

                    // 文字追加
                    if (i < len)
                    {
                        dest.Append(src[i]);
                    }
                }
            }

            return dest.ToString();
        }
    }
}
