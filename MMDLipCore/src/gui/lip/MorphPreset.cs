using System;
using System.Runtime.Serialization;
using ruche.mmd.morph.lip;

namespace ruche.mmd.gui.lip
{
    /// <summary>
    /// 口パクモーフプリセットクラス。
    /// </summary>
    [DataContract(Name = "Preset", Namespace = "")]
    [KnownType(typeof(MorphInfoSet))]
    public sealed class MorphPreset : ICloneable
    {
        /// <summary>
        /// 既定のプリセット名。
        /// </summary>
        public static readonly string DefaultName = "デフォルト";

        /// <summary>
        /// プリセット名として使える文字列か否かを取得する。
        /// </summary>
        /// <param name="name">調べる文字列。</param>
        /// <returns>プリセット名として使えるならば true 。</returns>
        public static bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name);
        }

        /// <summary>
        /// 既定値で初期化するコンストラクタ。
        /// </summary>
        public MorphPreset() : this(null, null)
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="name">
        /// プリセット名。 null を指定すると既定値になる。
        /// </param>
        /// <param name="value">
        /// プリセットデータ。 null を指定すると既定値になる。
        /// </param>
        public MorphPreset(string name, MorphInfoSet value)
        {
            if (name != null && !IsValidName(name))
            {
                throw new ArgumentException("Invalid preset name.", "name");
            }

            this.Name = name ?? DefaultName;
            this.Value = value ?? new MorphInfoSet();
        }

        /// <summary>
        /// プリセット名を取得する。
        /// </summary>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// プリセットデータを取得する。
        /// </summary>
        [DataMember]
        public MorphInfoSet Value { get; private set; }

        /// <summary>
        /// 自身のクローンを作成する。
        /// </summary>
        /// <returns>自身のクローン。</returns>
        public MorphPreset Clone()
        {
            return new MorphPreset(this.Name, this.Value.Clone());
        }

        #region ICloneable の明示的実装

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion
    }
}
