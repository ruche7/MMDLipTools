using System;
using System.Runtime.Serialization;
using ruche.mmd.morph;

namespace ruche.mmd.service.lip
{
    /// <summary>
    /// KeyFrames コマンドの情報を保持するクラス。
    /// </summary>
    [DataContract(Namespace = "")]
    [KnownType(typeof(MorphTimelineTable))]
    public class KeyFramesCommandParameter
    {
        /// <summary>
        /// モーフ別タイムラインを取得または設定する。
        /// </summary>
        [DataMember]
        public MorphTimelineTable TimelineTable { get; set; }

        /// <summary>
        /// ユニット基準長(「ア」の長さ)に相当する秒数値を取得または設定する。
        /// </summary>
        [DataMember]
        public decimal UnitSeconds { get; set; }
    }
}
