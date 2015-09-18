using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using ruche.mmd.morph.lip.converters;

namespace ruche.mmd.gui.lip
{
    /// <summary>
    /// 口パク編集設定クラス。
    /// </summary>
    [DataContract(Namespace = "")]
    public class LipEditConfig : IExtensibleDataObject
    {
        /// <summary>
        /// 口パクのフレーム数指定値の最小許容値。
        /// </summary>
        public static readonly decimal MinSpanFrame = 1;

        /// <summary>
        /// 口パクのフレーム数指定値の最大許容値。
        /// </summary>
        public static readonly decimal MaxSpanFrame = 99999.99m;

        /// <summary>
        /// FPSの最小許容値。
        /// </summary>
        public static readonly decimal MinFps = 0.01m;

        /// <summary>
        /// FPSの最大許容値。
        /// </summary>
        public static readonly decimal MaxFps = 9999.99m;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public LipEditConfig()
        {
            this.IsAutoLipKana = true;
            this.IsMorphEtoAI = false;
        }

        /// <summary>
        /// 読み仮名への自動変換を行うか否かを取得または設定する。
        /// </summary>
        [DataMember]
        public bool IsAutoLipKana { get; set; }

        /// <summary>
        /// 口パクの時間指定範囲種別を取得または設定する。
        /// </summary>
        [DataMember]
        public LipSpanRange SpanRange
        {
            get { return _spanRange; }
            set
            {
                if (!Enum.IsDefined(value.GetType(), value))
                {
                    throw new InvalidEnumArgumentException(
                        "value",
                        (int)value,
                        value.GetType());
                }

                _spanRange = value;
            }
        }
        private LipSpanRange _spanRange = LipSpanRange.Letter;

        /// <summary>
        /// 口パクのフレーム数指定値を取得または設定する。
        /// </summary>
        [DataMember]
        public decimal SpanFrame
        {
            get { return _spanFrame; }
            set { _spanFrame = Math.Min(Math.Max(MinSpanFrame, value), MaxSpanFrame); }
        }
        private decimal _spanFrame = 10;

        /// <summary>
        /// 口パクの時間指定単位種別を取得または設定する。
        /// </summary>
        [DataMember]
        public LipSpanUnit SpanUnit
        {
            get { return _spanUnit; }
            set
            {
                if (!Enum.IsDefined(value.GetType(), value))
                {
                    throw new InvalidEnumArgumentException(
                        "value",
                        (int)value,
                        value.GetType());
                }

                _spanUnit = value;
            }
        }
        private LipSpanUnit _spanUnit = LipSpanUnit.Frames;

        /// <summary>
        /// FPS値を取得または設定する。
        /// </summary>
        [DataMember]
        public decimal Fps
        {
            get { return _fps; }
            set { _fps = Math.Min(Math.Max(MinFps, value), MaxFps); }
        }
        private decimal _fps = 30;

        /// <summary>
        /// 前後のユニットとモーフ変化が重なる割合のパーセント値を取得または設定する。
        /// </summary>
        [DataMember]
        public decimal LinkLengthPercent
        {
            get { return _linkLengthPercent; }
            set
            {
                _linkLengthPercent =
                    Math.Min(
                        Math.Max(TimelineSetMaker.MinLinkLengthPercent, value),
                        TimelineSetMaker.MaxLinkLengthPercent);
            }
        }
        private decimal _linkLengthPercent = TimelineSetMaker.DefaultLinkLengthPercent;

        /// <summary>
        /// 長音の最大開口終端位置におけるウェイト値割合のパーセント値を
        /// 取得または設定する。
        /// </summary>
        [DataMember]
        public float LongSoundLastWeightPercent
        {
            get { return _longSoundLastWeightPercent; }
            set { _longSoundLastWeightPercent = Math.Min(Math.Max(0, value), 100); }
        }
        private float _longSoundLastWeightPercent =
            TimelineSetMaker.DefaultLongSoundLastWeight * 100;

        /// <summary>
        /// "え" から "あ","い" へのモーフ変更を行うか否かを取得または設定する。
        /// </summary>
        [DataMember]
        public bool IsMorphEtoAI { get; set; }

        #region IExtensibleDataObject の明示的実装

        ExtensionDataObject IExtensibleDataObject.ExtensionData { get; set; }

        #endregion
    }
}
