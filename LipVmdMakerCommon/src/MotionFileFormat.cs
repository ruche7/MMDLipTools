using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ruche.mmd.tools
{
    /// <summary>
    /// モーションデータファイルフォーマット列挙。
    /// </summary>
    [DataContract(Namespace = "")]
    public enum MotionFileFormat
    {
        /// <summary>
        /// VMDフォーマット。
        /// </summary>
        [Display(
            Name = "(_1) MikuMikuDance 標準形式 (*.vmd)",
            ShortName = "MikuMikuDance 標準形式")]
        [EnumMember]
        Vmd,

        /// <summary>
        /// MVDフォーマット。
        /// </summary>
        [Display(
            Name = "(_2) MikuMikuMoving 標準形式 (*.mvd)",
            ShortName = "MikuMikuMoving 標準形式")]
        [EnumMember]
        Mvd,
    }
}
