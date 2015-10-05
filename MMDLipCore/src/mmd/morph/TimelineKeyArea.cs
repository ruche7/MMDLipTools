using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ruche.mmd.morph
{
    /// <summary>
    /// キー位置とそのウェイト値のリストで構成されるモーフキー領域を保持するクラス。
    /// </summary>
    [DataContract(Namespace = "")]
    [KnownType(typeof(PointList))]
    public class TimelineKeyArea : ICloneable
    {
        /// <summary>
        /// キー位置とそのウェイト値のリストを保持するクラス。
        /// </summary>
        [CollectionDataContract(
            KeyName = "Position",
            ValueName = "Weight",
            Namespace = "")]
        public class PointList : SortedList<decimal, float>
        {
            /// <summary>
            /// コンストラクタ。
            /// </summary>
            public PointList() : base()
            {
            }

            /// <summary>
            /// コンストラクタ。
            /// </summary>
            /// <param name="src">初期値ディクショナリ。</param>
            public PointList(IDictionary<decimal, float> src) : base(src)
            {
            }
        }

        /// <summary>
        /// 線形補間したウェイト値を算出する。
        /// </summary>
        /// <param name="placeBegin">開始キー位置。</param>
        /// <param name="weightBegin">開始キーのウェイト値。</param>
        /// <param name="placeEnd">終端キー位置。</param>
        /// <param name="weightEnd">終端キーのウェイト値。</param>
        /// <param name="place">求めるキー位置。</param>
        /// <returns>求めるキー位置におけるウェイト値。</returns>
        private static float Interpolate(
            decimal placeBegin,
            float weightBegin,
            decimal placeEnd,
            float weightEnd,
            decimal place)
        {
            if (placeBegin == placeEnd)
            {
                throw new ArgumentException(
                    "The value of `" +
                    nameof(placeBegin) + "` and `" +
                    nameof(placeEnd) + "` are the same.");
            }

            var placeDiff = placeEnd - placeBegin;
            var weightDiff = weightEnd - weightBegin;
            var weightInt =
                (float)((decimal)weightDiff * (place - placeBegin) / placeDiff);

            return (weightBegin + weightInt);
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public TimelineKeyArea()
        {
        }

        /// <summary>
        /// キー位置とそのウェイト値のリストを取得する。
        /// </summary>
        [DataMember]
        public PointList Points { get; private set; } = new PointList();

        /// <summary>
        /// Points が空であるか否かを取得する。
        /// </summary>
        public bool IsEmpty => (this.Points.Count <= 0);

        /// <summary>
        /// 一番最初のキー位置を取得する。空ならば 0 を返す。
        /// </summary>
        public decimal BeginPlace => this.IsEmpty ? 0 : this.Points.Keys[0];

        /// <summary>
        /// 一番最後のキー位置を取得する。空ならば 0 を返す。
        /// </summary>
        public decimal EndPlace =>
            this.IsEmpty ? 0 : this.Points.Keys[this.Points.Count - 1];

        /// <summary>
        /// キー領域の長さを取得する。
        /// </summary>
        public decimal Length => (this.EndPlace - this.BeginPlace);

        /// <summary>
        /// 指定したキー位置におけるウェイト値を取得する。
        /// </summary>
        /// <param name="place">キー位置。</param>
        /// <returns>ウェイト値。範囲外ならば 0 。</returns>
        public float GetWeight(decimal place)
        {
            // 範囲外ならば 0 を返す
            if (this.IsEmpty || place < this.BeginPlace || place > this.EndPlace)
            {
                return 0;
            }

            // 指定位置以前で直近のポイントを取得
            var lt = this.Points.LastOrDefault(p => (p.Key <= place));

            // ちょうどポイント位置だったならばそのウェイト値を返す
            if (lt.Key == place)
            {
                return lt.Value;
            }

            // 指定位置以降で直近のポイントを取得
            var gt = this.Points.FirstOrDefault(p => (p.Key > place));

            // 補間したウェイト値を返す
            return Interpolate(lt.Key, lt.Value, gt.Key, gt.Value, place);
        }

        /// <summary>
        /// 指定したキー位置にウェイト値を設定する。
        /// </summary>
        /// <param name="place">キー位置。</param>
        /// <param name="weight">ウェイト値。</param>
        /// <returns>設定できたならば true 。そうでなければ false 。</returns>
        /// <remarks>
        /// 既にウェイト値が設定されている場合、より大きいウェイト値を優先する。
        /// </remarks>
        public bool SetWeight(decimal place, float weight)
        {
            float currentWeight = 0;
            if (
                this.Points.TryGetValue(place, out currentWeight) &&
                currentWeight >= weight)
            {
                return false;
            }

            this.Points[place] = weight;

            return true;
        }

        /// <summary>
        /// 条件を満たす登録キー位置の中で最も小さい値を返す。
        /// </summary>
        /// <param name="predicate">キー位置の条件判定関数。</param>
        /// <returns>
        /// 条件を満たす登録キー位置の中で最も小さい値。
        /// 条件を満たす登録キー位置が存在しなければ null 。
        /// </returns>
        public decimal? FindFirstPlace(Func<decimal, bool> predicate) =>
            this.Points.Keys
                .Where(p => predicate(p))
                .Cast<decimal?>()
                .FirstOrDefault();

        /// <summary>
        /// 条件を満たす登録キー位置の中で最も大きい値を返す。
        /// </summary>
        /// <param name="predicate">キー位置の条件判定関数。</param>
        /// <returns>
        /// 条件を満たす登録キー位置の中で最も大きい値。
        /// 条件を満たす登録キー位置が存在しなければ null 。
        /// </returns>
        public decimal? FindLastPlace(Func<decimal, bool> predicate) =>
            this.Points.Keys
                .Where(p => predicate(p))
                .Cast<decimal?>()
                .LastOrDefault();

        /// <summary>
        /// 無意味なキーを削除する。
        /// </summary>
        public void RemoveUselessPoints()
        {
            for (int i = 0; i + 2 < this.Points.Count; )
            {
                // 3つのキーを取得
                var p0 = this.Points.Keys[i];
                var w0 = this.Points.Values[i];
                var p1 = this.Points.Keys[i + 1];
                var w1 = this.Points.Values[i + 1];
                var p2 = this.Points.Keys[i + 2];
                var w2 = this.Points.Values[i + 2];

                // 挟まれているキーが補間に不要なら削除
                if (Interpolate(p0, w0, p2, w2, p1) == w1)
                {
                    this.Points.RemoveAt(i + 1);
                }
                else
                {
                    ++i;
                }
            }
        }

        /// <summary>
        /// 自身のクローンを作成する。
        /// </summary>
        /// <returns>自身のクローン。</returns>
        public TimelineKeyArea Clone() =>
            new TimelineKeyArea { Points = new PointList(this.Points) };

        #region ICloneable の明示的実装

        object ICloneable.Clone() => this.Clone();

        #endregion
    }
}
