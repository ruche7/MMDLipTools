using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ruche.mmd.tools
{
    /// <summary>
    /// 自動命名フォーマット列挙。
    /// </summary>
    [DataContract(Namespace = "")]
    public enum AutoNamingFormat
    {
        /// <summary>
        /// 入力文。
        /// </summary>
        [Display(Name = "(_1) 入力文")]
        [EnumMember]
        Text,

        [Display(Name = "(_2) 時刻_入力文")]
        [EnumMember]
        TimeText,

        [Display(Name = "(_3) 日付_時刻_入力文")]
        [EnumMember]
        DateTimeText,

        [Display(Name = "(_4) 読み仮名")]
        [EnumMember]
        Kana,

        [Display(Name = "(_5) 時刻_読み仮名")]
        [EnumMember]
        TimeKana,

        [Display(Name = "(_6) 日付_時刻_読み仮名")]
        [EnumMember]
        DateTimeKana,

        [Display(Name = "(_7) 日付_時刻")]
        [EnumMember]
        DateTime,
    }
}
