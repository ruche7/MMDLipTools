using System;
using System.Collections.Generic;

namespace ruche.mmd.morph.converters
{
    /// <summary>
    /// モーフ別キーフレームテーブルを作成するクラス。
    /// </summary>
    public class KeyFrameTableMaker
    {
        /// <summary>
        /// ユニット基準長(「ア」の長さ)に相当するフレーム長の既定値。
        /// </summary>
        public static readonly decimal DefaultUnitFrameLength = 10;

        /// <summary>
        /// 指定したキーの値を取得または新規追加する。
        /// </summary>
        /// <typeparam name="TKey">キー型。</typeparam>
        /// <typeparam name="TValue">値型。</typeparam>
        /// <param name="table">テーブル。</param>
        /// <param name="key">キー。</param>
        /// <param name="createNew">
        /// 新規追加時のインスタンス生成デリゲート。
        /// null ならば default(TValue) を追加する。
        /// </param>
        /// <returns>取得または新規作成した値。</returns>
        private static TValue GetOrCreate<TKey, TValue>(
            IDictionary<TKey, TValue> table,
            TKey key,
            Func<TValue> createNew = null)
        {
            TValue value;
            if (!table.TryGetValue(key, out value))
            {
                value = (createNew == null) ? default(TValue) : createNew();
                table.Add(key, value);
            }
            return value;
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public KeyFrameTableMaker()
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
        /// モーフ別タイムラインテーブルを基にモーフ別キーフレームテーブルを作成する。
        /// </summary>
        /// <param name="tlTable">モーフ別タイムラインテーブル。</param>
        /// <param name="beginFrame">開始実フレーム位置。</param>
        /// <returns>モーフ別キーフレームテーブル。</returns>
        /// <remarks>
        /// モーフ別タイムラインテーブルは、ユニット基準長(「ア」の長さ)を
        /// 1.0 とする時間単位であるものとして処理する。
        /// </remarks>
        public Dictionary<string, List<KeyFrame>> Make(
            MorphTimelineTable tlTable,
            long beginFrame)
        {
            var dest = new Dictionary<string, List<KeyFrame>>();

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

                    // 登録キーのあるモーフ名ごとに処理
                    names.ForEach(
                        name =>
                        {
                            // フレームデータリスト取得or新規追加
                            var frames =
                                GetOrCreate(
                                    dest,
                                    name,
                                    () => new List<KeyFrame>());

                            // フレームデータ追加
                            frames.Add(
                                new KeyFrame(
                                    beginFrame + destPos,
                                    tlTable[name].GetWeight(srcPos.Value)));
                        });
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
