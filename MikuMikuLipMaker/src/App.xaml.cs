using System;
using System.ComponentModel;
using System.Windows;
using ruche.mmd.service.lip;
using ruche.mmd.tools;
using ruche.util;

namespace MikuMikuLipMaker
{
    /// <summary>
    /// アプリケーションクラス。
    /// </summary>
    public partial class App : MikuMikuLipMakerAppBase
    {
        /// <summary>
        /// メインウィンドウ設定。
        /// </summary>
        private ConfigKeeper<MainWindowConfig> windowConfig =
            new ConfigKeeper<MainWindowConfig>(ConfigDirectoryName);

        /// <summary>
        /// 口パクサービス。
        /// </summary>
        private LipService lipService = new LipService();

        /// <summary>
        /// 口パクサービスサーバ。
        /// </summary>
        private LipServiceServer lipServiceServer = null;

        /// <summary>
        /// MikuMikuLipMaker アプリケーションの開始時に呼び出される。
        /// </summary>
        /// <param name="viewModel">ViewModel 。</param>
        protected override void OnAppStartup(MikuMikuLipConfigViewModel viewModel)
        {
            // 設定をロード
            if (!this.windowConfig.Load())
            {
                this.windowConfig.Value = new MainWindowConfig();
            }

            // 口パクサービスサーバ作成＆開始
            this.lipServiceServer =
                LipServiceServer.OpenNetNamedPipe(lipService, "MikuMikuLipMaker");

            // ViewModel にサービスを設定
            viewModel.TimelineTableSender = this.lipService.SetKeyFramesCommand;
            viewModel.MorphWeightsSender = this.lipService.SetMorphWeightsCommand;

            // メインウィンドウ作成
            var window = new MainWindow();

            // パラメータ設定
            this.windowConfig.Value.ApplyTo(window);
            window.ViewModel = viewModel;
            window.Closing += this.OnMainWindowClosing;

            // メインウィンドウ表示開始
            window.Show();
        }

        /// <summary>
        /// MikuMikuLipMaker アプリケーションの終了時に呼び出される。
        /// </summary>
        protected override void OnAppExit()
        {
            // 口パクサービスサーバ終了
            if (this.lipServiceServer != null)
            {
                this.lipServiceServer.Close();
                this.lipServiceServer = null;
            }
        }

        /// <summary>
        /// メインウィンドウが閉じようとしている時に呼び出される。
        /// </summary>
        private void OnMainWindowClosing(object sender, CancelEventArgs e)
        {
            var window = sender as Window;
            if (window != null)
            {
                // ウィンドウ状態を設定値に反映してセーブ
                this.windowConfig.Value.CopyFrom(window);
                this.windowConfig.Save();
            }
        }
    }
}
