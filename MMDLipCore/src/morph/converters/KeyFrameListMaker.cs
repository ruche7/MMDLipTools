using System;
using System.Collections.Generic;

namespace ruche.mmd.morph.converters
{
    /// <summary>
    /// モーフのキーフレームリストを作成するクラス。
    /// </summary>
    public class KeyFrameListMaker
    {
        /// <summary>
        /// ユニット基準長(「ア」の長さ)に相当するフレーム長の既定値。
        /// </summary>
        public static readonly decimal DefaultUnitFrameLength = 10;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public KeyFrameListMaker()
        {
            this.UnitFrameLength = DefaultUnitFrameLength;
        }

        /// <summary>
        /// ユニット基準長(「ア」の長さ)に相当するフレーム長を取得または設定する。
        /// </summary>
        public decimal UnitFrameLength
        {
            get { return _unitFrameLength; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                _unitFrameLength = value;
            }
        }
        private decimal _unitFrameLength = DefaultUnitFrameLength;

        /// <summary>
        /// モーフ別タイムラインテーブルを基にキーフレームリストを作成する。
        /// </summary>
        /// <param name="tlTable">モーフ別タイムラインテーブル。</param>
        /// <param name="beginFrame">開始実フレーム位置。</param>
        /// <returns>キーフレームリスト。</returns>
        /// <remarks>
        /// モーフ別タイムラインテーブルは、ユニット基準長(「ア」の長さ)を
        /// 1.0 とする時間単位であるものとして処理する。
        /// </remarks>
        public List<KeyFrame> Make(
            MorphTimelineTable tlTable,
            long beginFrame)
        {
            var dest = new List<KeyFrame>();

            decimal? srcPos = decimal.MinValue;
            long destPos = long.MinValue;
            var names = new List<string>();

            while (srcPos.HasValue)
            {
                // 次の登録キー位置を検索
                srcPos = tlTable.FindFirstPlace(p => (p > srcPos), names);
                if (srcPos.HasValue)
                {
                    // 実フレーム位置決定
                    // 前回の位置より最低でも1フレームは進める
                    destPos = Math.Max(CalcFrame(srcPos.Value), destPos + 1);

                    // キーフレーム群追加
                    dest.AddRange(
                        names.ConvertAll(
                            name =>
                                new KeyFrame(
                                    name,
                                    beginFrame + destPos,
                                    tlTable[name].GetWeight(srcPos.Value))));
                }
            }

            return dest;
        }

        /// <summary>
        /// 実フレーム位置を算出する。
        /// </summary>
        /// <param name="place">タイムラインのキー位置。</param>
        /// <returns>実フレーム位置。</returns>
        private long CalcFrame(decimal place)
        {
            return (long)(place * this.UnitFrameLength + 0.5m);
        }
    }
}
