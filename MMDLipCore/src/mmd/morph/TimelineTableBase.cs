using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ruche.mmd.morph
{
    /// <summary>
    /// ID別のモーフタイムラインを保持する抽象クラス。
    /// </summary>
    /// <typeparam name="TId">ID型。</typeparam>
    public abstract class TimelineTableBase<TId>
        : IEnumerable<KeyValuePair<TId, Timeline>>
    {
        /// <summary>
        /// タイムラインを取得または設定するインデクサ。
        /// </summary>
        /// <param name="id">ID値。</param>
        /// <returns>タイムライン。</returns>
        public abstract Timeline this[TId id] { get; set; }

        /// <summary>
        /// タイムラインテーブルを取得または設定する。
        /// </summary>
        protected Dictionary<TId, Timeline> Table
        {
            get { return _table; }
            set { _table = value ?? new Dictionary<TId, Timeline>(); }
        }
        private Dictionary<TId, Timeline> _table = new Dictionary<TId, Timeline>();

        /// <summary>
        /// タイムラインテーブルの列挙子を取得する。
        /// </summary>
        /// <returns>タイムラインテーブルの列挙子。</returns>
        public IEnumerator<KeyValuePair<TId, Timeline>> GetEnumerator()
        {
            return this.Table.GetEnumerator();
        }

        /// <summary>
        /// 一番最初のキー位置を取得する。
        /// </summary>
        /// <returns>一番最初のキー位置。キーが1つも無いならば 0 。</returns>
        public decimal GetBeginPlace()
        {
            return (this.Table.Count > 0) ?
                this.Table.Min(it => it.Value.GetBeginPlace()) :
                0;
        }

        /// <summary>
        /// 一番最後のキー位置を取得する。
        /// </summary>
        /// <returns>一番最後のキー位置。キーが1つも無いならば 0 。</returns>
        public decimal GetEndPlace()
        {
            return (this.Table.Count > 0) ?
                this.Table.Max(it => it.Value.GetEndPlace()) :
                0;
        }

        /// <summary>
        /// 条件を満たす登録キー位置の中で最も小さい値を返す。
        /// </summary>
        /// <param name="predicate">キー位置の条件判定関数。</param>
        /// <param name="ids">
        /// 返り値の登録キー位置をタイムラインに持つID値の設定先。
        /// 不要ならば null 。
        /// </param>
        /// <returns>
        /// 条件を満たす登録キー位置の中で最も小さい値。
        /// 条件を満たす登録キー位置が存在しなければ null 。
        /// </returns>
        public decimal? FindFirstPlace(
            Func<decimal, bool> predicate,
            List<TId> ids = null)
        {
            // 各タイムラインを検索し、値が見つかったものを抽出
            var table =
                from it in this.Table
                let p = it.Value.FindFirstPlace(predicate)
                where p.HasValue
                select new { Id = it.Key, Place = p };

            // 最小位置を取得
            var place = table.Min(ip => ip.Place);

            // ids が null でなければ対象IDリスト作成
            if (ids != null)
            {
                ids.Clear();
                if (place != null)
                {
                    ids.AddRange(
                        from ip in table
                        where ip.Place == place
                        select ip.Id);
                }
            }

            return place;
        }

        /// <summary>
        /// 条件を満たす登録キー位置の中で最も大きい値を返す。
        /// </summary>
        /// <param name="predicate">キー位置の条件判定関数。</param>
        /// <param name="lipIds">
        /// 返り値の登録キー位置をタイムラインに持つID値の設定先。
        /// 不要ならば null 。
        /// </param>
        /// <returns>
        /// 条件を満たす登録キー位置の中で最も大きい値。
        /// 条件を満たす登録キー位置が存在しなければ null 。
        /// </returns>
        public decimal? FindLastPlace(
            Func<decimal, bool> predicate,
            List<TId> ids = null)
        {
            // 各タイムラインを検索し、値が見つかったものを抽出
            var table =
                from it in this.Table
                let p = it.Value.FindLastPlace(predicate)
                where p.HasValue
                select new { Id = it.Key, Place = p };

            // 最大位置を取得
            var place = table.Max(ip => ip.Place);

            // ids が null でなければ対象IDリスト作成
            if (ids != null)
            {
                ids.Clear();
                if (place != null)
                {
                    ids.AddRange(
                        from ip in table
                        where ip.Place == place
                        select ip.Id);
                }
            }

            return place;
        }

        #region IEnumerable の明示的実装

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
