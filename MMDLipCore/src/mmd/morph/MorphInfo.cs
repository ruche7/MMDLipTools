using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ruche.mmd.morph
{
    /// <summary>
    /// モーフ情報を保持するクラス。
    /// </summary>
    [DataContract(Namespace = "")]
    [KnownType(typeof(MorphWeightDataList))]
    [KnownType(typeof(MorphWeightData))]
    public sealed class MorphInfo : ICloneable
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MorphInfo() : this(new MorphWeightData[0])
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="morphWeights">モーフ名とそのウェイト値の列挙。</param>
        public MorphInfo(IEnumerable<MorphWeightData> morphWeights)
        {
            if (morphWeights == null)
            {
                throw new ArgumentNullException(nameof(morphWeights));
            }

            this.MorphWeights = new MorphWeightDataList(morphWeights);
        }

        /// <summary>
        /// モーフ名とそのウェイト値のリストを取得する。
        /// </summary>
        [DataMember]
        public MorphWeightDataList MorphWeights { get; private set; } = null;

        /// <summary>
        /// このオブジェクトの文字列表現を作成する。
        /// </summary>
        /// <returns>文字列表現。</returns>
        public override string ToString() => string.Join(", ", this.MorphWeights);

        /// <summary>
        /// 自身のクローンを作成する。
        /// </summary>
        /// <returns>自身のクローン。</returns>
        public MorphInfo Clone() => new MorphInfo(this.MorphWeights.Clone());

        /// <summary>
        /// デシリアライズの直前に呼び出される。
        /// </summary>
        [OnDeserializing]
        void OnDeserializing(StreamingContext context)
        {
            // null 回避
            this.MorphWeights = new MorphWeightDataList();
        }

        #region ICloneable の明示的実装

        object ICloneable.Clone() => this.Clone();

        #endregion
    }
}
