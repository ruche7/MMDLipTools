using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace ruche.mmd.service.lip
{
    /// <summary>
    /// 口パクサービスクライアントクラス。
    /// </summary>
    public class LipServiceClient : IDisposable
    {
        /// <summary>
        /// 正常時のワーカスレッドループインターバル。
        /// </summary>
        private static readonly TimeSpan WorkInterval =
            TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// 不正時のワーカスレッドループインターバル。
        /// </summary>
        private static readonly TimeSpan WorkIntervalOnFail =
            TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// 名前付きパイプによるサービスクライアントを作成し、通信を開始する。
        /// </summary>
        /// <returns>LipServiceClient オブジェクト。</returns>
        /// <remarks>
        /// エンドポイントアドレスには空文字列が利用される。
        /// </remarks>
        public static LipServiceClient OpenNetNamedPipe() => OpenNetNamedPipe("");

        /// <summary>
        /// 名前付きパイプによるサービスクライアントを作成し、通信を開始する。
        /// </summary>
        /// <param name="endpointAddress">
        /// エンドポイントアドレス。ベースアドレスからの相対パス。
        /// </param>
        /// <returns>LipServiceClient オブジェクト。</returns>
        public static LipServiceClient OpenNetNamedPipe(string endpointAddress)
        {
            if (endpointAddress == null)
            {
                throw new ArgumentNullException(nameof(endpointAddress));
            }

            var address =
                LipServiceDefine.NetNamedPipeBaseAddress +
                ((endpointAddress == "") ? "" : ("/" + endpointAddress));
            var factory =
                new ChannelFactory<ILipService>(
                    new NetNamedPipeBinding(),
                    new EndpointAddress(address));

            return new LipServiceClient(factory);
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="factory">チャネルファクトリ。</param>
        protected LipServiceClient(ChannelFactory<ILipService> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }
            this.Factory = factory;

            // 同期コンテキストを保持
            // コンソールアプリ等では null
            this.MainThreadContext = SynchronizationContext.Current;

            // ワーカスレッドキャンセルソース作成
            var source = new CancellationTokenSource();
            this.WorkCancelSource = source;

            // ワーカスレッド開始
            this.WorkTask =
                Task.Factory.StartNew(
                    () => this.Run(factory, source),
                    source.Token);
        }

        /// <summary>
        /// デストラクタ。
        /// </summary>
        ~LipServiceClient()
        {
            this.Close();
        }

        /// <summary>
        /// 直近のコマンドを取得する。
        /// </summary>
        public LipServiceCommand LastCommand
        {
            get
            {
                lock (lastCommandLockObject)
                {
                    return _lastCommand;
                }
            }
            private set
            {
                bool changed = false;

                lock (lastCommandLockObject)
                {
                    // カウンタ値が更新されている場合のみ
                    if (value != null && value.Counter != _lastCommand.Counter)
                    {
                        _lastCommand = value;
                        changed = true;
                    }
                }

                if (changed)
                {
                    Action<LipServiceCommand> callback =
                        cmd =>
                            this.RaiseCommand?.Invoke(
                                this,
                                new LipServiceCommandEventArgs(cmd));

                    // 同期コンテキストが保持されているならそれを使う
                    if (this.MainThreadContext == null)
                    {
                        callback(value);
                    }
                    else
                    {
                        this.MainThreadContext.Post(_ => callback(value), null);
                    }
                }
            }
        }
        private LipServiceCommand _lastCommand = new LipServiceCommand();

        /// <summary>
        /// LastCommand プロパティの排他制御用オブジェクト。
        /// </summary>
        private object lastCommandLockObject = new object();

        /// <summary>
        /// 新規コマンド発行時に呼び出されるイベント。
        /// </summary>
        public event EventHandler<LipServiceCommandEventArgs> RaiseCommand;

        /// <summary>
        /// チャネルファクトリを取得する。
        /// </summary>
        private ChannelFactory<ILipService> Factory { get; set; }

        /// <summary>
        /// 同期コンテキストを取得または設定する。
        /// </summary>
        private SynchronizationContext MainThreadContext { get; set; }

        /// <summary>
        /// ワーカスレッドタスクを取得または設定する。
        /// </summary>
        private Task WorkTask { get; set; }

        /// <summary>
        /// ワーカスレッドのキャンセルトークンソースを取得または設定する。
        /// </summary>
        private CancellationTokenSource WorkCancelSource { get; set; }

        /// <summary>
        /// サーバとの通信を終了する。
        /// </summary>
        public void Close()
        {
            // ワーカスレッド終了指示
            if (this.WorkCancelSource != null)
            {
                this.WorkCancelSource.Cancel();
                if (this.WorkTask != null)
                {
                    this.WorkTask.Wait();
                    this.WorkTask.Dispose();
                    this.WorkTask = null;
                }
                this.WorkCancelSource.Dispose();
                this.WorkCancelSource = null;
            }

            // 通信終了
            if (
                this.Factory != null &&
                this.Factory.State == CommunicationState.Opened)
            {
                try
                {
                    this.Factory.Close();
                }
                catch
                {
                    this.Factory.Abort();
                }
                this.Factory = null;
            }
        }

        /// <summary>
        /// ワーカスレッド処理を行う。
        /// </summary>
        /// <param name="factory">チャネルファクトリ。</param>
        /// <param name="cancelSource">キャンセルトークンソース。</param>
        private void Run(
            ChannelFactory<ILipService> factory,
            CancellationTokenSource cancelSource)
        {
            // チャネル作成
            var service = factory.CreateChannel();
            var channel = service as IClientChannel;

            // Close して失敗したら Abort するデリゲート
            Action<IClientChannel> closeOrAbort =
                ch =>
                {
                    try
                    {
                        ch.Close();
                    }
                    catch
                    {
                        ch.Abort();
                    }
                };

            // メインループ用ストップウォッチ
            var loopSw = Stopwatch.StartNew();
            var lastElapsed = TimeSpan.Zero;
            var interval = TimeSpan.Zero;

            // メインループ
            for (; !cancelSource.Token.IsCancellationRequested; Thread.Sleep(1))
            {
                // インターバル制御
                var elapsed = loopSw.Elapsed;
                if (elapsed >= lastElapsed && elapsed - lastElapsed < interval)
                {
                    continue;
                }
                lastElapsed = elapsed;

                interval = WorkIntervalOnFail;
                try
                {
                    // 再接続
                    if (channel.State != CommunicationState.Opened)
                    {
                        closeOrAbort(channel);
                        service = factory.CreateChannel();
                        channel = service as IClientChannel;
                        channel.Open();
                    }

                    // コマンド取得
                    this.LastCommand = service.GetCommand();

                    interval = WorkInterval;
                }
                catch (EndpointNotFoundException)
                {
                }
                catch (CommunicationException)
                {
                }
            }

            closeOrAbort(channel);
        }

        #region IDisposable の明示的実装

        void IDisposable.Dispose() => this.Close();

        #endregion
    }
}
