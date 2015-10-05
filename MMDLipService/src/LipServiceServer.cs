using System;
using System.ServiceModel;

namespace ruche.mmd.service.lip
{
    /// <summary>
    /// 口パクサービスサーバクラス。
    /// </summary>
    public class LipServiceServer : IDisposable
    {
        /// <summary>
        /// 名前付きパイプによるサービスサーバを作成し、サービス提供を開始する。
        /// </summary>
        /// <param name="service">
        /// ILipService シングルトンオブジェクト。
        /// 実装クラスには ServiceBehaviorAttribute 属性を用いて
        /// InstanceContextMode.Single を指定する必要がある。
        /// </param>
        /// <returns>LipServiceServer オブジェクト。</returns>
        /// <remarks>
        /// エンドポイントアドレスには空文字列が利用される。
        /// </remarks>
        public static LipServiceServer OpenNetNamedPipe(ILipService service) =>
            OpenNetNamedPipe(service, "");

        /// <summary>
        /// 名前付きパイプによるサービスサーバを作成し、サービス提供を開始する。
        /// </summary>
        /// <param name="service">
        /// ILipService シングルトンオブジェクト。
        /// 実装クラスには ServiceBehaviorAttribute 属性を用いて
        /// InstanceContextMode.Single を指定する必要がある。
        /// </param>
        /// <param name="endpointAddress">
        /// エンドポイントアドレス。ベースアドレスからの相対パス。
        /// </param>
        /// <returns>LipServiceServer オブジェクト。</returns>
        public static LipServiceServer OpenNetNamedPipe(
            ILipService service,
            string endpointAddress)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }
            if (endpointAddress == null)
            {
                throw new ArgumentNullException(nameof(endpointAddress));
            }

            var host =
                new ServiceHost(
                    service,
                    new Uri(LipServiceDefine.NetNamedPipeBaseAddress));
            host.AddServiceEndpoint(
                typeof(ILipService),
                new NetNamedPipeBinding(),
                endpointAddress);
            host.Open();

            return new LipServiceServer(host);
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="host">サービスホスト。</param>
        protected LipServiceServer(ServiceHost host)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }
            this.Host = host;
        }

        /// <summary>
        /// デストラクタ。
        /// </summary>
        ~LipServiceServer()
        {
            this.Close();
        }

        /// <summary>
        /// サービスホストを取得または設定する。
        /// </summary>
        protected ServiceHost Host { get; private set; }

        /// <summary>
        /// サービス提供を終了する。
        /// </summary>
        public void Close()
        {
            if (this.Host != null)
            {
                this.Host.Close();
                this.Host = null;
            }
        }

        #region IDisposable の明示的実装

        void IDisposable.Dispose()
        {
            this.Close();
        }

        #endregion
    }
}
