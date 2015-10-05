using System;
using System.Collections.Generic;
using System.Linq;

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
        /// ウェイト値が 0 のキーフレームをまとめて追加する。
        /// </summary>
        /// <param name="keyFrames">追加先のキーフレームリスト。</param>
        /// <param name="morphNames">追加対象のモーフ名列挙。</param>
        /// <param name="frame">フレーム位置。</param>
        /// <remarks>
        /// 既に同位置にキーフレームが存在する場合は追加しない。
        /// </remarks>
        private static void AddZeroWeightKeyFrames(
            KeyFrameList keyFrames,
            IEnumerable<string> morphNames,
            long frame)
        {
            keyFrames.AddRange(
                from name in morphNames
                where !keyFrames.Any(f => f.MorphName == name && f.Frame == frame)
                select new KeyFrame(name, frame, 0));
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public KeyFrameListMaker()
        {
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
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _unitFrameLength = value;
            }
        }
        private decimal _unitFrameLength = DefaultUnitFrameLength;

        /// <summary>
        /// キーフレームリストの先頭と終端で、
        /// 含まれている全モーフのウェイト値をゼロ初期化するか否かを取得する。
        /// </summary>
        /// <remarks>
        /// 既にキーフレーム登録されているモーフについては上書きしない。
        /// </remarks>
        public bool IsEdgeWeightZero { get; set; } = false;

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
        public KeyFrameList Make(
            MorphTimelineTable tlTable,
            long beginFrame)
        {
            var dest = new KeyFrameList();

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

            if (dest.Count > 0 && this.IsEdgeWeightZero)
            {
                // 先頭と末尾のウェイト値をゼロ初期化
                var morphNames = tlTable.MorphNames;
                AddZeroWeightKeyFrames(dest, morphNames, dest.Min(f => f.Frame));
                AddZeroWeightKeyFrames(dest, morphNames, dest.Max(f => f.Frame));
            }

            return dest;
        }

        /// <summary>
        /// 実フレーム位置を算出する。
        /// </summary>
        /// <param name="place">タイムラインのキー位置。</param>
        /// <returns>実フレーム位置。</returns>
        private long CalcFrame(decimal place) =>
            (long)(place * this.UnitFrameLength + 0.5m);
    }
}
