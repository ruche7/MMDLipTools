using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace ruche.mmd.morph
{
    /// <summary>
    /// モーフ名とそのウェイト値を保持する構造体。
    /// </summary>
    [DataContract(Namespace = "")]
    public class MorphWeightData
        : INotifyPropertyChanged, IEquatable<MorphWeightData>, ICloneable
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MorphWeightData()
        {
        }

        /// <summary>
        /// モーフ名を取得または設定する。
        /// </summary>
        [DataMember]
        public string MorphName
        {
            get { return _morphName; }
            set
            {
                var v = value ?? "";
                if (v != _morphName)
                {
                    _morphName = v;
                    this.NotifyPropertyChanged(nameof(MorphName));
                }
            }
        }
        private string _morphName = "";

        /// <summary>
        /// ウェイト値を取得または設定する。
        /// </summary>
        [DataMember]
        public float Weight
        {
            get { return _weight; }
            set
            {
                var v = Math.Min(Math.Max(0.0f, value), 1.0f);
                if (v != _weight)
                {
                    _weight = v;
                    this.NotifyPropertyChanged(nameof(Weight));
                }
            }
        }
        private float _weight = 1.0f;

        /// <summary>
        /// プロパティの変更時に呼び出されるイベント。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// このオブジェクトが別のオブジェクトと等しいか否かを取得する。
        /// </summary>
        /// <param name="other">調べるオブジェクト。</param>
        /// <returns>等しいならば true 。そうでなければ false 。</returns>
        public bool Equals(MorphWeightData other) =>
            (this.MorphName == other?.MorphName && this.Weight == other?.Weight);

        /// <summary>
        /// このオブジェクトが別のオブジェクトと等しいか否かを取得する。
        /// </summary>
        /// <param name="obj">調べるオブジェクト。</param>
        /// <returns>等しいならば true 。そうでなければ false 。</returns>
        public override bool Equals(object obj) =>
            this.Equals(obj as MorphWeightData);

        /// <summary>
        /// ハッシュコードを取得する。
        /// </summary>
        /// <returns>ハッシュコード。</returns>
        public override int GetHashCode() =>
            (this.MorphName.GetHashCode() ^ this.Weight.GetHashCode());

        /// <summary>
        /// このオブジェクトの文字列表現を作成する。
        /// </summary>
        /// <returns>文字列表現。</returns>
        public override string ToString() =>
            ("\"" + this.MorphName + "\" = " + this.Weight);

        /// <summary>
        /// 自身のクローンを作成する。
        /// </summary>
        /// <returns>自身のクローン。</returns>
        public MorphWeightData Clone() =>
            new MorphWeightData
            {
                MorphName = this.MorphName,
                Weight = this.Weight,
            };

        /// <summary>
        /// プロパティの変更時に呼び出される。
        /// </summary>
        /// <param name="propertyName">プロパティ名。</param>
        private void NotifyPropertyChanged(string propertyName) =>
            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));

        #region ICloneable の明示的実装

        object ICloneable.Clone() => this.Clone();

        #endregion
    }
}
