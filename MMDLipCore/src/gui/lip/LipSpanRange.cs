using System;
using System.ComponentModel.DataAnnotations;

namespace ruche.mmd.gui.lip
{
    /// <summary>
    /// 口パクの時間指定範囲を表す列挙。
    /// </summary>
    public enum LipSpanRange
    {
        /// <summary>
        /// 1音あたりの長さを指定する。
        /// </summary>
        [Display(Name = "1音あたり")]
        Letter,

        /// <summary>
        /// 文章全体の長さを指定する。
        /// </summary>
        [Display(Name = "読み仮名全体で")]
        All,
    }
}
