using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ruche.mmd.gui.lip
{
    /// <summary>
    /// 口パクの時間指定範囲を表す列挙。
    /// </summary>
    [DataContract(Namespace = "")]
    public enum LipSpanRange
    {
        /// <summary>
        /// 1音あたりの長さを指定する。
        /// </summary>
        [Display(Name = "1音あたり")]
        [EnumMember]
        Letter,

        /// <summary>
        /// 文章全体の長さを指定する。
        /// </summary>
        [Display(Name = "読み仮名全体で")]
        [EnumMember]
        All,
    }
}
