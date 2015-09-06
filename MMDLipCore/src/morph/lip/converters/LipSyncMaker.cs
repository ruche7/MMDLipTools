using System;
using System.Collections.Generic;
using ruche.mmd.util;

namespace ruche.mmd.morph.lip.converters
{
    /// <summary>
    /// リップシンクユニットリストを作成するクラス。
    /// </summary>
    public class LipSyncMaker
    {
        /// <summary>
        /// リップシンクユニットとその適用対象文字を保持する構造体。
        /// </summary>
        private struct UnitTargetData
        {
            /// <summary>
            /// ベースとなるリップシンクユニット。
            /// </summary>
            public LipSyncUnit Unit { get; set; }

            /// <summary>
            /// 適用対象となるカタカナ文字の配列。
            /// </summary>
            public char[] TargetLetters { get; set; }
        }

        /// <summary>
        /// 適用対象のない文字で用いるリップシンクユニット。
        /// </summary>
        private static readonly LipSyncUnit DefaultUnit =
            new LipSyncUnit(LipId.Closed, LinkType.Normal, 100);

        /// <summary>
        /// リップシンクユニットとその適用対象文字の配列。
        /// </summary>
        private static readonly UnitTargetData[] UnitTargets =
            {
                // ア
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.A, LinkType.Normal, 100),
                    TargetLetters = "アカサタナハヤラガザダァャヵ".ToCharArray(),
                },
                // イ
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.I, LinkType.Normal, 100),
                    TargetLetters = "イキシチニヒリギジヂィ".ToCharArray(),
                },
                // ウ
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.U, LinkType.Normal, 100),
                    TargetLetters = "ウクスツヌフユルグズヅゥュ".ToCharArray(),
                },
                // エ
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.E, LinkType.Normal, 100),
                    TargetLetters = "エケセテネヘレゲゼデェヶ".ToCharArray(),
                },
                // オ
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.O, LinkType.Normal, 100),
                    TargetLetters = "オコソトノホヨロゴゾドォョ".ToCharArray(),
                },

                // マ
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.A, LinkType.PreClose, 100),
                    TargetLetters = "マバパヷ".ToCharArray(),
                },
                // ミ
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.I, LinkType.PreClose, 100),
                    TargetLetters = "ミビピヸ".ToCharArray(),
                },
                // ム
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.U, LinkType.PreClose, 100),
                    TargetLetters = "ムブプヴ".ToCharArray(),
                },
                // メ
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.E, LinkType.PreClose, 100),
                    TargetLetters = "メベペヹ".ToCharArray(),
                },
                // モ
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.O, LinkType.PreClose, 100),
                    TargetLetters = "モボポヺ".ToCharArray(),
                },

                // ワ
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.A, LinkType.PreHalfClose, 100),
                    TargetLetters = "ワヮ".ToCharArray(),
                },
                // ヰ
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.I, LinkType.PreHalfClose, 100),
                    TargetLetters = "ヰ".ToCharArray(),
                },
                // ヱ
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.E, LinkType.PreHalfClose, 100),
                    TargetLetters = "ヱ".ToCharArray(),
                },
                // ヲ
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.O, LinkType.PreHalfClose, 100),
                    TargetLetters = "ヲ".ToCharArray(),
                },

                // 長音(口形状は先行文字によって決まる)
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.Closed, LinkType.LongSound, 100),
                    TargetLetters = KanaDefine.GetLongSoundLetters(),
                },

                // 促音(口形状は先行文字によって決まる)
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.Closed, LinkType.Tsu, 100),
                    TargetLetters = KanaDefine.GetSmallTsuLetters(),
                },

                // 持続(長さ100％; 口形状は先行文字によって決まる)
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.Closed, LinkType.Keep, 100),
                    TargetLetters = "‥…―".ToCharArray(),
                },
                // 持続(長さ50％; 口形状は先行文字によって決まる)
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.Closed, LinkType.Keep, 50),
                    TargetLetters = "・･".ToCharArray(),
                },

                // 口閉じ(長さ150％)
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.Closed, LinkType.Normal, 150),
                    TargetLetters = "。．？；;：:＿".ToCharArray(),
                },
                // 口閉じ(長さ100％)
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.Closed, LinkType.Normal, 100),
                    TargetLetters = "ン　、，".ToCharArray(),
                },
                // 口閉じ(長さ50％)
                new UnitTargetData
                {
                    Unit = new LipSyncUnit(LipId.Closed, LinkType.Normal, 50),
                    TargetLetters = " .,\r\n\u30A0\u30FB".ToCharArray(),
                },
            };

        /// <summary>
        /// カタカナ文字に対応するリップシンクユニットを検索する。
        /// </summary>
        /// <param name="c">カタカナ文字。</param>
        /// <returns>対応するリップシンクユニット。</returns>
        private static LipSyncUnit FindLipSyncUnit(char c)
        {
            var unit = DefaultUnit;

            foreach (var ut in UnitTargets)
            {
                if (Array.IndexOf(ut.TargetLetters, c) >= 0)
                {
                    unit = ut.Unit;
                    break;
                }
            }

            // 呼び出し元でいじられてもいいようにクローンを返す
            return unit.Clone();
        }

        /// <summary>
        /// 口パク用カタカナコンバータ。
        /// </summary>
        private readonly KatakanaLipConverter _kataLipConv = new KatakanaLipConverter();

        /// <summary>
        /// カタカナの読み仮名文字列からリップシンクユニットリストを作成する。
        /// </summary>
        /// <param name="src">カタカナの読み仮名文字列。</param>
        /// <returns>リップシンクユニットリスト。</returns>
        public List<LipSyncUnit> Make(string src)
        {
            // 口パク用カタカナに変換。
            string text = _kataLipConv.ConvertFrom(src);

            // ユニットリスト作成
            List<LipSyncUnit> dests = new List<LipSyncUnit>(text.Length);
            foreach (var elem in new TextElementEnumerable(text))
            {
                // 文字に対応するユニットを決定
                // 文字がサロゲートペアなら既定のユニットにしておく
                var unit =
                    (elem.Length == 1) ?
                        FindLipSyncUnit(elem[0]) :
                        DefaultUnit.Clone();

                // 口形状を先行文字によって決めるべきか？
                if (
                    dests.Count > 0 &&
                    unit.LipId == LipId.Closed &&
                    !unit.LinkType.HasSingleSound())
                {
                    var last = dests[dests.Count - 1];

                    // 先行文字が促音および持続ではない または
                    // 自身が先行文字と同じ接続タイプ
                    if (
                        (last.LinkType != LinkType.Tsu &&
                         last.LinkType != LinkType.Keep) ||
                        unit.LinkType == last.LinkType)
                    {
                        // 口形状を先行文字と同じにする
                        unit.LipId = last.LipId;
                    }
                }

                // 口を閉じるなら接続タイプを通常にしておく
                if (unit.LipId == LipId.Closed)
                {
                    unit.LinkType = LinkType.Normal;
                }

                dests.Add(unit);
            }

            return dests;
        }
    }
}
