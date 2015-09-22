using System;

namespace ruche.mmd.service.lip
{
    /// <summary>
    /// 口パクサービスに関する共通定義を提供する静的クラス。
    /// </summary>
    public static class LipServiceDefine
    {
        /// <summary>
        /// 名前付きパイプサーバのベースアドレス。
        /// </summary>
        public static readonly string NetNamedPipeBaseAddress =
            @"net.pipe://localhost/ruche/mmd/service/lip";
    }
}
