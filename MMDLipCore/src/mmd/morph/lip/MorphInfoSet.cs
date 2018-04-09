using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections;

namespace ruche.mmd.morph.lip
{
    /// <summary>
    /// 口形状種別ごとのモーフ情報を保持するクラス。
    /// </summary>
    [DataContract(Namespace = "")]
    [KnownType(typeof(MorphInfoTable))]
    [KnownType(typeof(MorphInfo))]
    public sealed class MorphInfoSet
        : IEnumerable<KeyValuePair<LipId, MorphInfo>>, ICloneable
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
                    nameof(id),
                    (int)id,
                    id.GetType());
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
            if (table != null)
            {
                foreach (var id in LipIds)
                {
                    MorphInfo value = null;
                    if (table.TryGetValue(id, out value) && value != null)
                    {
                        this.Table.Add(id, value.Clone());
                    }
                }
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

                MorphInfo result = null;
                if (!this.Table.TryGetValue(id, out result) || result == null)
                {
                    result = CreateDefaultLipMorph(id);
                    this.Table[id] = result;
                }
                return result;
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
        /// モーフ情報テーブルを取得する。
        /// </summary>
        [DataMember]
        private MorphInfoTable Table { get; set; } = new MorphInfoTable(LipIds.Length);

        /// <summary>
        /// 自身のクローンを作成する。
        /// </summary>
        /// <returns>自身のクローン。</returns>
        public MorphInfoSet Clone() => new MorphInfoSet(this.Table);

        /// <summary>
        /// モーフ情報テーブルの列挙子を取得する。
        /// </summary>
        /// <returns>モーフ情報テーブルの列挙子。</returns>
        public IEnumerator<KeyValuePair<LipId, MorphInfo>> GetEnumerator() =>
            this.Table.GetEnumerator();

        /// <summary>
        /// 口形状種別IDが有効な値か検証する。
        /// </summary>
        /// <param name="id">口形状種別ID。</param>
        /// <exception cref="InvalidEnumArgumentException">
        /// id が有効な値ではない場合。
        /// </exception>
        private void ValidateLipId(LipId id)
        {
            if (!Enum.IsDefined(typeof(LipId), id))
            {
                throw new InvalidEnumArgumentException(
                    nameof(id),
                    (int)id,
                    id.GetType());
            }
        }

        /// <summary>
        /// デシリアライズの直前に呼び出される。
        /// </summary>
        [OnDeserializing]
        void OnDeserializing(StreamingContext context)
        {
            // null 回避
            this.Table = new MorphInfoTable(LipIds.Length);
        }

        #region IEnumerable の明示的実装

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        #endregion

        #region ICloneable の明示的実装

        object ICloneable.Clone() => this.Clone();

        #endregion
    }
}
