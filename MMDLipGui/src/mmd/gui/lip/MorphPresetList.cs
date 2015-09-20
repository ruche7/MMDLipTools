using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace ruche.mmd.gui.lip
{
    /// <summary>
    /// 口パクモーフプリセットリストクラス。
    /// </summary>
    [CollectionDataContract(ItemName = "Item", Namespace = "")]
    [KnownType(typeof(MorphPreset))]
    public sealed class MorphPresetList
        : ObservableCollection<MorphPreset>, ICloneable
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MorphPresetList() : base() { }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="src">初期値列挙。</param>
        public MorphPresetList(IEnumerable<MorphPreset> src)
            : base(src)
        {
        }

        /// <summary>
        /// 指定したプリセット名を持つプリセットのインデックスを検索する。
        /// </summary>
        /// <param name="name">プリセット名。</param>
        /// <returns>インデックス。見つからなかった場合は -1 。</returns>
        public int FindIndex(string name)
        {
            var found =
                this
                    .Select((p, i) => new { Preset = p, Index = i })
                    .Where(v => v.Preset.Name == name)
                    .FirstOrDefault();

            return (found == null) ? -1 : found.Index;
        }

        /// <summary>
        /// 自身のクローンを作成する。
        /// </summary>
        /// <returns>自身のクローン。</returns>
        public MorphPresetList Clone()
        {
            return new MorphPresetList(this.Select(p => p.Clone()));
        }

        #region ObservableCollection<MorphPreset> のオーバライド

        protected override void SetItem(int index, MorphPreset item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            base.SetItem(index, item);
        }

        protected override void InsertItem(int index, MorphPreset item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            base.InsertItem(index, item);
        }

        #endregion

        #region ICloneable の明示的実装

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion
    }
}
