using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.IO;
using System.Threading;
using System.Windows;
using ruche.mmd.tools;
using ruche.util;

namespace LipVmdMakerLite
{
    /// <summary>
    /// アプリケーションクラス。
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// LipVmdControlViewModel 設定保存ファイルパス。
        /// </summary>
        private static readonly string LipVmdControlViewModelConfigFilePath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                @"ruche-home\MMDLipTools\ruche.mmd.tools.LipVmdControlViewModel.xaml");

        /// <summary>
        /// MorphPresetList 設定保存ファイルパス。
        /// </summary>
        private static readonly string MorphPresetListConfigFilePath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                @"ruche-home\MMDLipTools\ruche.mmd.gui.lip.MorphPresetList.xaml");

        /// <summary>
        /// 多重起動防止用ミューテクス。
        /// </summary>
        private Mutex mutexForBoot =
            new Mutex(false, "{0975DE42-5DA7-460E-BEAC-D91909EDD7BC}");

        /// <summary>
        /// メインウィンドウの ViewModel を取得または設定する。
        /// </summary>
        private LipVmdControlViewModel WindowViewModel { get; set; }

        /// <summary>
        /// アプリケーションの開始時に呼び出される。
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 多重起動防止
            if (!this.mutexForBoot.WaitOne(0, false))
            {
                this.Shutdown(1);
                return;
            }

            // ViewModel 作成＆設定ロード
            // ロード失敗しても構わない
            this.WindowViewModel = new LipVmdControlViewModel();
            Config.Load(LipVmdControlViewModelConfigFilePath, this.WindowViewModel);

            // メインウィンドウ作成＆表示開始
            var window = new MainWindow();
            window.ViewModel = this.WindowViewModel;
            window.Show();
        }

        /// <summary>
        /// アプリケーションの終了時に呼び出される。
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // ViewModel 設定セーブ
            Config.Save(this.WindowViewModel, LipVmdControlViewModelConfigFilePath);

            // ミューテクス破棄
            if (this.mutexForBoot != null)
            {
                this.mutexForBoot.Dispose();
                this.mutexForBoot = null;
            }
        }
    }
}
