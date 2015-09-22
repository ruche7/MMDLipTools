using System;
using System.Runtime.Serialization;

namespace ruche.mmd.service.lip
{
    /// <summary>
    /// 口パクサービスから各クライアントへ要求するコマンド種別ID列挙。
    /// </summary>
    [DataContract(Namespace = "")]
    public enum LipServiceCommandId
    {
        /// <summary>
        /// コマンドがないことを示す値。
        /// </summary>
        [EnumMember]
        None,

        /// <summary>
        /// パラメータで指定されたモーフウェイトリストを設定するコマンド。
        /// </summary>
        [EnumMember]
        MorphWeights,

        /// <summary>
        /// パラメータからキーフレームリストを作成してプロットするコマンド。
        /// </summary>
        [EnumMember]
        KeyFrames,
    }
}
