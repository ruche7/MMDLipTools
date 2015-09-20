using System;
using System.Threading;
using System.Windows;
using ruche.mmd.gui.lip;
using ruche.util;

namespace ruche.mmd.tools
{
    /// <summary>
    /// MikuMikuLipMaker アプリケーションの抽象基底クラス。
    /// </summary>
    public abstract class MikuMikuLipMakerAppBase : Application
    {
        /// <summary>
        /// 共通設定保存ディレクトリ名。
        /// </summary>
        public static readonly string CommonConfigDirectoryName = @"MMDLipTools";

        /// <summary>
        /// 設定保存ディレクトリ名。
        /// </summary>
        public static readonly string ConfigDirectoryName =
            CommonConfigDirectoryName + @"\MikuMikuLipMaker";

        /// <summary>
        /// 多重起動防止用ミューテクス。
        /// </summary>
        private Mutex mutexForBoot =
            new Mutex(false, "{0975DE42-5DA7-460E-BEAC-D91909EDD7BC}");

        /// <summary>
        /// 口パクモーフモーションデータファイル保存設定。
        /// </summary>
        private ConfigKeeper<MikuMikuLipConfig> mmlConfig =
            new ConfigKeeper<MikuMikuLipConfig>(ConfigDirectoryName);

        /// <summary>
        /// 口パク編集設定。
        /// </summary>
        private ConfigKeeper<LipEditConfig> editConfig =
            new ConfigKeeper<LipEditConfig>(ConfigDirectoryName);

        /// <summary>
        /// 口パクモーフプリセット編集設定。
        /// </summary>
        private ConfigKeeper<MorphPresetConfig> presetConfig =
            new ConfigKeeper<MorphPresetConfig>(CommonConfigDirectoryName);

        /// <summary>
        /// MikuMikuLipMaker アプリケーションの開始時に呼び出される。
        /// </summary>
        /// <param name="viewModel">ViewModel 。</param>
        protected abstract void OnAppStartup(MikuMikuLipConfigViewModel viewModel);

        /// <summary>
        /// MikuMikuLipMaker アプリケーションの終了時に呼び出される。
        /// </summary>
        protected virtual void OnAppExit()
        {
        }

        /// <summary>
        /// アプリケーションの開始時に呼び出される。
        /// </summary>
        protected override sealed void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 多重起動防止
            if (!this.mutexForBoot.WaitOne(0, false))
            {
                this.Shutdown(1);
                return;
            }

            // 設定をロード
            if (!this.mmlConfig.Load())
            {
                this.mmlConfig.Value = new MikuMikuLipConfig();
            }
            if (!this.editConfig.Load())
            {
                this.editConfig.Value = new LipEditConfig();
            }
            if (!this.presetConfig.Load())
            {
                this.presetConfig.Value = new MorphPresetConfig();
            }

            // ViewModel 作成
            var vm = new MikuMikuLipConfigViewModel(this.mmlConfig.Value);
            vm.EditViewModel =
                new LipEditControlViewModel(
                    this.editConfig.Value,
                    this.presetConfig.Value);

            // アプリケーション個別処理
            this.OnAppStartup(vm);
        }

        /// <summary>
        /// アプリケーションの終了時に呼び出される。
        /// </summary>
        protected override sealed void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // アプリケーション個別処理
            this.OnAppExit();

            // 設定をセーブ
            this.mmlConfig.Save();
            this.editConfig.Save();
            this.presetConfig.Save();

            // ミューテクス破棄
            if (this.mutexForBoot != null)
            {
                this.mutexForBoot.Dispose();
                this.mutexForBoot = null;
            }
        }
    }
}
