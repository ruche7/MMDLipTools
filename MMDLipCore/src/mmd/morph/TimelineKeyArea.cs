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
        /// コンストラクタ。
        /// </summary>
        public TimelineKeyArea()
        {
            this.Points = new PointList();
        }

        /// <summary>
        /// キー位置とそのウェイト値のリストを取得する。
        /// </summary>
        [DataMember]
        public PointList Points { get; private set; }

        /// <summary>
        /// Points が空であるか否かを取得する。
        /// </summary>
        public bool IsEmpty
        {
            get { return (this.Points.Count <= 0); }
        }

        /// <summary>
        /// 一番最初のキー位置を取得する。空ならば 0 を返す。
        /// </summary>
        public decimal BeginPlace
        {
            get { return this.IsEmpty ? 0 : this.Points.Keys[0]; }
        }

        /// <summary>
        /// 一番最後のキー位置を取得する。空ならば 0 を返す。
        /// </summary>
        public decimal EndPlace
        {
            get
            {
                return this.IsEmpty ? 0 : this.Points.Keys[this.Points.Count - 1];
            }
        }

        /// <summary>
        /// キー領域の長さを取得する。
        /// </summary>
        public decimal Length
        {
            get { return (this.EndPlace - this.BeginPlace); }
        }

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
            decimal placeDiff = gt.Key - lt.Key;
            float weightDiff = gt.Value - lt.Value;
            float weightInt =
                (float)((decimal)weightDiff * (place - lt.Key) / placeDiff);
            return (lt.Value + weightInt);
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
        public decimal? FindFirstPlace(Func<decimal, bool> predicate)
        {
            return (
                from p in this.Points.Keys
                where predicate(p)
                select (decimal?)p)
                .FirstOrDefault();
        }

        /// <summary>
        /// 条件を満たす登録キー位置の中で最も大きい値を返す。
        /// </summary>
        /// <param name="predicate">キー位置の条件判定関数。</param>
        /// <returns>
        /// 条件を満たす登録キー位置の中で最も大きい値。
        /// 条件を満たす登録キー位置が存在しなければ null 。
        /// </returns>
        public decimal? FindLastPlace(Func<decimal, bool> predicate)
        {
            return (
                from p in this.Points.Keys
                where predicate(p)
                select (decimal?)p)
                .LastOrDefault();
        }

        /// <summary>
        /// 自身のクローンを作成する。
        /// </summary>
        /// <returns>自身のクローン。</returns>
        public TimelineKeyArea Clone()
        {
            var dest = new TimelineKeyArea();
            dest.Points = new PointList(this.Points);
            return dest;
        }

        #region ICloneable の明示的実装

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion
    }
}
