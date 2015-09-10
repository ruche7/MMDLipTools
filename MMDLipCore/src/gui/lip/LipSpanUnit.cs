using System;
using System.ComponentModel.DataAnnotations;

namespace ruche.mmd.gui.lip
{
    /// <summary>
    /// 口パクの時間指定単位を表す列挙。
    /// </summary>
    public enum LipSpanUnit
    {
        /// <summary>
        /// フレーム単位。
        /// </summary>
        [Display(Name = "フレーム")]
        Frames,

        /// <summary>
        /// ミリ秒単位。
        /// </summary>
        [Display(Name = "ミリ秒")]
        MilliSeconds,

        /// <summary>
        /// 秒単位。
        /// </summary>
        [Display(Name = "秒")]
        Seconds,
    }
}
