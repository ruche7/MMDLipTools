using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ruche.mmd.morph
{
    /// <summary>
    /// モーフ情報を保持するクラス。
    /// </summary>
    [DataContract(Namespace = "")]
    [KnownType(typeof(MorphWeightDataList))]
    [KnownType(typeof(MorphWeightData))]
    public sealed class MorphInfo : IEquatable<MorphInfo>, ICloneable
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
        public MorphWeightDataList MorphWeights { get; }

        /// <summary>
        /// このオブジェクトが別のオブジェクトと等しいか否かを取得する。
        /// </summary>
        /// <param name="other">調べるオブジェクト。</param>
        /// <returns>等しいならば true 。そうでなければ false 。</returns>
        public bool Equals(MorphInfo other) =>
            (other != null && this.MorphWeights.SequenceEqual(other.MorphWeights));

        /// <summary>
        /// このオブジェクトが別のオブジェクトと等しいか否かを取得する。
        /// </summary>
        /// <param name="obj">調べるオブジェクト。</param>
        /// <returns>等しいならば true 。そうでなければ false 。</returns>
        public override bool Equals(object obj) => this.Equals(obj as MorphInfo);

        /// <summary>
        /// ハッシュコードを取得する。
        /// </summary>
        /// <returns>ハッシュコード。</returns>
        public override int GetHashCode() =>
            (this.MorphWeights.Count == 0) ?
                0 :
                (this.MorphWeights[0].GetHashCode() + this.MorphWeights.Count);

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

        #region ICloneable の明示的実装

        object ICloneable.Clone() => this.Clone();

        #endregion
    }
}
