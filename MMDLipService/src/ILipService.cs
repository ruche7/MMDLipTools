using System;
using System.ServiceModel;

namespace ruche.mmd.service.lip
{
    /// <summary>
    /// 口パクサービスインタフェース。
    /// </summary>
    [ServiceContract]
    public interface ILipService
    {
        /// <summary>
        /// 口パクサービスから提供されるコマンドを取得する。
        /// </summary>
        /// <returns>コマンド。</returns>
        [OperationContract]
        LipServiceCommand GetCommand();
    }
}
