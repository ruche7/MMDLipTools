using System;
using System.Runtime.Serialization;

namespace ruche.mmd.morph
{
    /// <summary>
    /// モーフのキーフレームを保持するクラス。
    /// </summary>
    [DataContract(Namespace = "")]
    public class KeyFrame
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public KeyFrame() : this("", 0, 0)
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="morphName">モーフ名。</param>
        /// <param name="frame">フレーム位置。</param>
        /// <param name="weight">ウェイト値。</param>
        public KeyFrame(string morphName, long frame, float weight)
        {
            this.MorphName = morphName;
            this.Frame = frame;
            this.Weight = weight;
        }

        /// <summary>
        /// モーフ名を取得または設定する。
        /// </summary>
        [DataMember]
        public string MorphName
        {
            get { return _morphName; }
            set { _morphName = value ?? ""; }
        }
        private string _morphName = "";

        /// <summary>
        /// フレーム位置を取得または設定する。
        /// </summary>
        [DataMember]
        public long Frame { get; set; }

        /// <summary>
        /// ウェイト値を取得または設定する。
        /// </summary>
        [DataMember]
        public float Weight { get; set; }

        /// <summary>
        /// このオブジェクトの文字列表現を作成する。
        /// </summary>
        /// <returns>文字列表現。</returns>
        public override string ToString()
        {
            return (this.Frame + " : \"" + this.MorphName + "\" = " + this.Weight);
        }
    }
}
