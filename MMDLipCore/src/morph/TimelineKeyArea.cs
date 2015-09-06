using System;
using System.Collections.Generic;
using System.Linq;

namespace ruche.mmd.morph
{
    /// <summary>
    /// キー位置とそのウェイト値のリストで構成されるモーフキー領域を保持するクラス。
    /// </summary>
    public class TimelineKeyArea
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public TimelineKeyArea()
        {
            this.Points = new SortedList<long, float>();
        }

        /// <summary>
        /// キー位置とそのウェイト値のリストを取得する。
        /// </summary>
        public SortedList<long, float> Points { get; private set; }

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
        public long BeginPlace
        {
            get { return this.IsEmpty ? 0 : this.Points.Keys[0]; }
        }

        /// <summary>
        /// 一番最後のキー位置を取得する。空ならば 0 を返す。
        /// </summary>
        public long EndPlace
        {
            get
            {
                return this.IsEmpty ? 0 : this.Points.Keys[this.Points.Count - 1];
            }
        }

        /// <summary>
        /// キー領域の長さを取得する。
        /// </summary>
        public long Length
        {
            get { return (this.EndPlace - this.BeginPlace); }
        }

        /// <summary>
        /// 指定したキー位置におけるウェイト値を取得する。
        /// </summary>
        /// <param name="place">キー位置。</param>
        /// <returns>ウェイト値。範囲外ならば 0 。</returns>
        public float GetWeight(long place)
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
            long placeDiff = gt.Key - lt.Key;
            float weightDiff = gt.Value - lt.Value;
            float weightInt = weightDiff * (place - lt.Key) / placeDiff;
            return (lt.Value + weightInt);
        }

        /// <summary>
        /// 指定したキー位置以降に新しいキー位置およびウェイト値を追加する。
        /// </summary>
        /// <param name="place">キー位置。</param>
        /// <param name="weight">ウェイト値。</param>
        /// <returns>実際に追加されたキー位置。</returns>
        public long AddPointAfter(long place, float weight)
        {
            while (this.Points.ContainsKey(place))
            {
                ++place;
            }
            this.Points.Add(place, weight);

            return place;
        }

        /// <summary>
        /// 条件を満たす登録キー位置の中で最も小さい値を返す。
        /// </summary>
        /// <param name="predicate">キー位置の条件判定関数。</param>
        /// <returns>
        /// 条件を満たす登録キー位置の中で最も小さい値。
        /// 条件を満たす登録キー位置が存在しなければ null 。
        /// </returns>
        public long? FindFirstPlace(Func<long, bool> predicate)
        {
            return (
                from p in this.Points.Keys
                where predicate(p)
                select (long?)p)
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
        public long? FindLastPlace(Func<long, bool> predicate)
        {
            return (
                from p in this.Points.Keys
                where predicate(p)
                select (long?)p)
                .LastOrDefault();
        }
    }
}
