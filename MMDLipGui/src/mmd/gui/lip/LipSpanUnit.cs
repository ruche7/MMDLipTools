using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ruche.mmd.gui.lip
{
    /// <summary>
    /// 口パクの時間指定単位を表す列挙。
    /// </summary>
    [DataContract(Namespace = "")]
    public enum LipSpanUnit
    {
        /// <summary>
        /// フレーム単位。
        /// </summary>
        [Display(Name = "フレーム")]
        [EnumMember]
        Frames,

        /// <summary>
        /// ミリ秒単位。
        /// </summary>
        [Display(Name = "ミリ秒")]
        [EnumMember]
        MilliSeconds,

        /// <summary>
        /// 秒単位。
        /// </summary>
        [Display(Name = "秒")]
        [EnumMember]
        Seconds,
    }
}
