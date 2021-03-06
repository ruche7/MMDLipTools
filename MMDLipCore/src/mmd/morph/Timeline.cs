﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ruche.mmd.morph
{
    /// <summary>
    /// キー領域のリストで構成されるモーフタイムラインを表すクラス。
    /// </summary>
    [DataContract(Namespace = "")]
    [KnownType(typeof(KeyAreaList))]
    public class Timeline
    {
        /// <summary>
        /// キー領域リストクラス。
        /// </summary>
        [CollectionDataContract(ItemName = "Item", Namespace = "")]
        [KnownType(typeof(TimelineKeyArea))]
        public class KeyAreaList : List<TimelineKeyArea>
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public Timeline()
        {
        }

        /// <summary>
        /// キー領域のリストを取得する。
        /// </summary>
        [DataMember]
        public KeyAreaList KeyAreas { get; private set; } = new KeyAreaList();

        /// <summary>
        /// KeyAreas が空であるか否かを取得する。
        /// </summary>
        public bool IsEmpty => (this.KeyAreas.Count <= 0);

        /// <summary>
        /// 指定したキー位置におけるウェイト値を取得する。
        /// </summary>
        /// <param name="place">キー位置。</param>
        /// <returns>ウェイト値。</returns>
        public float GetWeight(decimal place) =>
            this.KeyAreas.Sum(a => a.GetWeight(place));

        /// <summary>
        /// 一番最初のキー位置を取得する。
        /// </summary>
        /// <returns>一番最初のキー位置。キーが1つも無いならば 0 。</returns>
        public decimal GetBeginPlace() =>
            this.IsEmpty ? 0 : this.KeyAreas.Min(a => a.BeginPlace);

        /// <summary>
        /// 一番最後のキー位置を取得する。
        /// </summary>
        /// <returns>一番最後のキー位置。キーが1つも無いならば 0 。</returns>
        public decimal GetEndPlace() =>
            this.IsEmpty ? 0 : this.KeyAreas.Max(a => a.EndPlace);

        /// <summary>
        /// すべてのキー位置を示す遅延列挙オブジェクトを取得する。
        /// </summary>
        /// <returns>すべてのキー位置を示す遅延列挙オブジェクト。</returns>
        public IEnumerable<decimal> GetAllPlaces() =>
            this.KeyAreas.SelectMany(a => a.Points.Keys).Distinct();

        /// <summary>
        /// 条件を満たす登録キー位置の中で最も小さい値を返す。
        /// </summary>
        /// <param name="predicate">キー位置の条件判定関数。</param>
        /// <returns>
        /// 条件を満たす登録キー位置の中で最も小さい値。
        /// 条件を満たす登録キー位置が存在しなければ null 。
        /// </returns>
        public decimal? FindFirstPlace(Func<decimal, bool> predicate) =>
            this.KeyAreas.Min(a => a.FindFirstPlace(predicate));

        /// <summary>
        /// 条件を満たす登録キー位置の中で最も大きい値を返す。
        /// </summary>
        /// <param name="predicate">キー位置の条件判定関数。</param>
        /// <returns>
        /// 条件を満たす登録キー位置の中で最も大きい値。
        /// 条件を満たす登録キー位置が存在しなければ null 。
        /// </returns>
        public decimal? FindLastPlace(Func<decimal, bool> predicate) =>
            this.KeyAreas.Max(a => a.FindLastPlace(predicate));
    }
}
