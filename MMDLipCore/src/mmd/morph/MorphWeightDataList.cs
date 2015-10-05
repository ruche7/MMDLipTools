using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace ruche.mmd.morph
{
    /// <summary>
    /// MorphWeightData リストクラス。
    /// </summary>
    [CollectionDataContract(ItemName = "Item", Namespace = "")]
    [KnownType(typeof(MorphWeightData))]
    public sealed class MorphWeightDataList
        : ObservableCollection<MorphWeightData>, ICloneable
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MorphWeightDataList() : base() { }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="src">初期値列挙。</param>
        public MorphWeightDataList(IEnumerable<MorphWeightData> src)
            : base(src)
        {
        }

        /// <summary>
        /// 自身のクローンを作成する。
        /// </summary>
        /// <returns>自身のクローン。</returns>
        public MorphWeightDataList Clone() =>
            new MorphWeightDataList(this.Select(d => d.Clone()));

        #region ObservableCollection<MorphWeightData> のオーバライド

        protected override void SetItem(int index, MorphWeightData item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            base.SetItem(index, item);
        }

        protected override void InsertItem(int index, MorphWeightData item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            base.InsertItem(index, item);
        }

        #endregion

        #region ICloneable の明示的実装

        object ICloneable.Clone() => this.Clone();

        #endregion
    }
}
