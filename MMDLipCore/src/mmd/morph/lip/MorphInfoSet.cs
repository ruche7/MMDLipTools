using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ruche.mmd.morph.lip
{
    /// <summary>
    /// 口形状種別ごとのモーフ情報を保持するクラス。
    /// </summary>
    [DataContract(Namespace = "")]
    [KnownType(typeof(MorphInfoTable))]
    [KnownType(typeof(MorphInfo))]
    public sealed class MorphInfoSet : IEquatable<MorphInfoSet>, ICloneable
    {
        /// <summary>
        /// 口形状ID配列。
        /// </summary>
        private static readonly LipId[] LipIds =
            (LipId[])Enum.GetValues(typeof(LipId));

        /// <summary>
        /// 口形状種別IDに対応する既定のモーフ情報を作成する。
        /// </summary>
        /// <param name="id">口形状種別ID。</param>
        /// <returns>既定のモーフ情報。</returns>
        public static MorphInfo CreateDefaultLipMorph(LipId id)
        {
            IEnumerable<MorphWeightData> weights = null;

            switch (id)
            {
            case LipId.Closed:
                weights = new MorphWeightData[0];
                break;

            case LipId.A:
                weights = new[] {
                    new MorphWeightData { MorphName = "あ", Weight = 1 } };
                break;

            case LipId.I:
                weights = new[] {
                    new MorphWeightData { MorphName = "い", Weight = 1 } };
                break;

            case LipId.U:
                weights = new[] {
                    new MorphWeightData { MorphName = "う", Weight = 1 } };
                break;

            case LipId.E:
                weights = new[] {
                    new MorphWeightData { MorphName = "え", Weight = 1 } };
                break;

            case LipId.O:
                weights = new[] {
                    new MorphWeightData { MorphName = "お", Weight = 1 } };
                break;

            default:
                throw new InvalidEnumArgumentException(
                    "id",
                    (int)id,
                    typeof(LipId));
            }

            return new MorphInfo(weights);
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MorphInfoSet() : this(null)
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="table">初期値テーブル。</param>
        public MorphInfoSet(IDictionary<LipId, MorphInfo> table)
        {
            this.Table = new MorphInfoTable(LipIds.Length);

            foreach (var id in LipIds)
            {
                MorphInfo value = null;

                if (table != null && table.ContainsKey(id))
                {
                    value = table[id];
                    if (value != null)
                    {
                        value = value.Clone();
                    }
                }
                if (value == null)
                {
                    value = CreateDefaultLipMorph(id);
                }

                this.Table.Add(id, value);
            }
        }

        /// <summary>
        /// モーフ情報を取得または設定するインデクサ。
        /// </summary>
        /// <param name="id">口形状種別ID。</param>
        /// <returns>モーフ情報。</returns>
        public MorphInfo this[LipId id]
        {
            get
            {
                ValidateLipId(id);
                return this.Table[id];
            }
            set
            {
                ValidateLipId(id);
                this.Table[id] = value ?? (new MorphInfo());
            }
        }

        /// <summary>
        /// 閉口のモーフ情報を取得または設定する。
        /// </summary>
        public MorphInfo Closed
        {
            get { return this[LipId.Closed]; }
            set { this[LipId.Closed] = value; }
        }

        /// <summary>
        /// 「あ」のモーフ情報を取得または設定する。
        /// </summary>
        public MorphInfo A
        {
            get { return this[LipId.A]; }
            set { this[LipId.A] = value; }
        }

        /// <summary>
        /// 「い」のモーフ情報を取得または設定する。
        /// </summary>
        public MorphInfo I
        {
            get { return this[LipId.I]; }
            set { this[LipId.I] = value; }
        }

        /// <summary>
        /// 「う」のモーフ情報を取得または設定する。
        /// </summary>
        public MorphInfo U
        {
            get { return this[LipId.U]; }
            set { this[LipId.U] = value; }
        }

        /// <summary>
        /// 「え」のモーフ情報を取得または設定する。
        /// </summary>
        public MorphInfo E
        {
            get { return this[LipId.E]; }
            set { this[LipId.E] = value; }
        }

        /// <summary>
        /// 「お」のモーフ情報を取得または設定する。
        /// </summary>
        public MorphInfo O
        {
            get { return this[LipId.O]; }
            set { this[LipId.O] = value; }
        }

        /// <summary>
        /// モーフ情報テーブルを取得または設定する。
        /// </summary>
        [DataMember]
        private MorphInfoTable Table { get; set; }

        /// <summary>
        /// このオブジェクトが別のオブジェクトと等しいか否かを取得する。
        /// </summary>
        /// <param name="other">調べるオブジェクト。</param>
        /// <returns>等しいならば true 。そうでなければ false 。</returns>
        public bool Equals(MorphInfoSet other)
        {
            return (
                other != null &&
                LipIds.All(id => this[id].Equals(other[id])));
        }

        /// <summary>
        /// このオブジェクトが別のオブジェクトと等しいか否かを取得する。
        /// </summary>
        /// <param name="obj">調べるオブジェクト。</param>
        /// <returns>等しいならば true 。そうでなければ false 。</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as MorphInfoSet);
        }

        /// <summary>
        /// ハッシュコードを取得する。
        /// </summary>
        /// <returns>ハッシュコード。</returns>
        public override int GetHashCode()
        {
            return this.A.GetHashCode();
        }

        /// <summary>
        /// 自身のクローンを作成する。
        /// </summary>
        /// <returns>自身のクローン。</returns>
        public MorphInfoSet Clone()
        {
            return new MorphInfoSet(this.Table);
        }

        /// <summary>
        /// 口形状種別IDが有効な値か検証する。
        /// </summary>
        /// <param name="id">口形状種別ID。</param>
        /// <exception cref="InvalidEnumArgumentException">
        /// id が有効な値ではない場合。
        /// </exception>
        private void ValidateLipId(LipId id)
        {
            if (!this.Table.ContainsKey(id))
            {
                throw new InvalidEnumArgumentException(
                    "id",
                    (int)id,
                    typeof(LipId));
            }
        }

        #region ICloneable の明示的実装

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion
    }
}
