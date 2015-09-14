using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ruche.mmd.morph.lip
{
    /// <summary>
    /// 口形状種別をキーとする MorphInfo テーブルクラス。
    /// </summary>
    [CollectionDataContract(ItemName = "Item", Namespace = "")]
    [KnownType(typeof(MorphInfo))]
    internal sealed class MorphInfoTable : Dictionary<LipId, MorphInfo>
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MorphInfoTable() : base() { }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="capacity">初期キャパシティ。</param>
        public MorphInfoTable(int capacity) : base(capacity) { }
    }
}
