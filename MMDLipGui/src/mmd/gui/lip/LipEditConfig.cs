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
        /// 口パクの秒数指定値の最小許容値。
        /// </summary>
        public static readonly decimal MinSpanSeconds = 0.001m;

        /// <summary>
        /// 口パクの秒数指定値の最大許容値。
        /// </summary>
        public static readonly decimal MaxSpanSeconds = 600m;

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
        }

        /// <summary>
        /// 読み仮名への自動変換を行うか否かを取得または設定する。
        /// </summary>
        [DataMember]
        public bool IsAutoLipKana { get; set; } = true;

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
                        nameof(value),
                        (int)value,
                        value.GetType());
                }

                _spanRange = value;
            }
        }
        private LipSpanRange _spanRange = LipSpanRange.Letter;

        /// <summary>
        /// 口パクの秒数指定値を取得または設定する。
        /// </summary>
        [DataMember]
        public decimal SpanSeconds
        {
            get { return _spanSeconds; }
            set
            {
                _spanSeconds =
                    Math.Min(Math.Max(MinSpanSeconds, value), MaxSpanSeconds);
            }
        }
        private decimal _spanSeconds = 0.2m;

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
                        nameof(value),
                        (int)value,
                        value.GetType());
                }

                _spanUnit = value;
            }
        }
        private LipSpanUnit _spanUnit = LipSpanUnit.MilliSeconds;

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
        /// キーフレームリストの先頭と終端で、含まれている全モーフのウェイト値を
        /// ゼロ初期化するか否かを取得または設定する。
        /// </summary>
        [DataMember]
        public bool IsEdgeWeightZero { get; set; } = true;

        /// <summary>
        /// キーフレームリストの先頭と終端に閉口時モーフ設定を適用するか否かを
        /// 取得または設定する。
        /// </summary>
        [DataMember]
        public bool IsEdgeClosed { get; set; } = true;

        /// <summary>
        /// "え" から "あ","い" へのモーフ変更を行うか否かを取得または設定する。
        /// </summary>
        [DataMember]
        public bool IsMorphEtoAI { get; set; } = false;

        #region IExtensibleDataObject の明示的実装

        ExtensionDataObject IExtensibleDataObject.ExtensionData { get; set; }

        #endregion
    }
}
