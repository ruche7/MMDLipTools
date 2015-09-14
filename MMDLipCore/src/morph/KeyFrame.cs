using System;

namespace ruche.mmd.morph
{
    /// <summary>
    /// モーフのキーフレームを保持する構造体。
    /// </summary>
    public struct KeyFrame
    {
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
        public string MorphName { get; set; }

        /// <summary>
        /// フレーム位置を取得または設定する。
        /// </summary>
        public long Frame { get; set; }

        /// <summary>
        /// ウェイト値を取得または設定する。
        /// </summary>
        public float Weight { get; set; }
    }
}
