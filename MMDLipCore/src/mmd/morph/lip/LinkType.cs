using System;
using System.Runtime.Serialization;

namespace ruche.mmd.morph.lip
{
    /// <summary>
    /// 直前の口形状からの繋ぎ方を表す列挙。
    /// </summary>
    [DataContract(Namespace = "")]
    public enum LinkType
    {
        /// <summary>
        /// 通常の繋ぎ方。
        /// </summary>
        [EnumMember]
        Normal,

        /// <summary>
        /// 一旦口を閉じる。「ま」「ば」等。
        /// </summary>
        [EnumMember]
        PreClose,

        /// <summary>
        /// 一旦半分口を閉じる。「わ」等。
        /// </summary>
        [EnumMember]
        PreHalfClose,

        /// <summary>
        /// 促音。「っ」等。
        /// </summary>
        [EnumMember]
        Tsu,

        /// <summary>
        /// 長音。「ー」等。
        /// </summary>
        [EnumMember]
        LongSound,

        /// <summary>
        /// 持続。「…」等。
        /// </summary>
        [EnumMember]
        Keep,
    }

    /// <summary>
    /// LinkType の拡張メソッドを提供する静的クラス。
    /// </summary>
    public static class LinkTypeExtension
    {
        /// <summary>
        /// 単独で音を持つか否かを取得する。
        /// </summary>
        /// <param name="self">LinkType 値。</param>
        /// <returns>単独で音を持つならば true 。</returns>
        public static bool HasSingleSound(this LinkType self) =>
            (self == LinkType.Normal ||
             self == LinkType.PreClose ||
             self == LinkType.PreHalfClose);
    }
}
