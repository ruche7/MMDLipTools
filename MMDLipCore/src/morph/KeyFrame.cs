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
        /// <param name="frame">フレーム位置。</param>
        /// <param name="weight">ウェイト値。</param>
        public KeyFrame(long frame, float weight)
        {
            this.Frame = frame;
            this.Weight = weight;
        }

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
