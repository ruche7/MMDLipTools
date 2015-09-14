using System;
using System.Runtime.Serialization;

namespace ruche.mmd.morph.lip
{
    /// <summary>
    /// 口形状種別IDを表す列挙。
    /// </summary>
    [DataContract(Namespace = "")]
    public enum LipId
    {
        /// <summary>
        /// 閉じた状態。
        /// </summary>
        [EnumMember]
        Closed,

        /// <summary>
        /// あ。
        /// </summary>
        [EnumMember]
        A,

        /// <summary>
        /// い。
        /// </summary>
        [EnumMember]
        I,

        /// <summary>
        /// う。
        /// </summary>
        [EnumMember]
        U,

        /// <summary>
        /// え。
        /// </summary>
        [EnumMember]
        E,

        /// <summary>
        /// お。
        /// </summary>
        [EnumMember]
        O,
    }
}
