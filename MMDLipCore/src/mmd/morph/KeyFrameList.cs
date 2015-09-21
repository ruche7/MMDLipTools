using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ruche.mmd.morph
{
    /// <summary>
    /// KeyFrame リストクラス。
    /// </summary>
    [CollectionDataContract(ItemName = "Item", Namespace = "")]
    public class KeyFrameList : List<KeyFrame>
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public KeyFrameList() : base()
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="src">初期値列挙。</param>
        public KeyFrameList(IEnumerable<KeyFrame> src) : base(src)
        {
        }
    }
}
