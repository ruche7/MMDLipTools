﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.IO;
using System.Threading;
using System.Windows;
using ruche.mmd.tools;
using ruche.mmd.gui.lip;
using System.ComponentModel;

namespace LipVmdMakerLite
{
    /// <summary>
    /// アプリケーションクラス。
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 多重起動防止用ミューテクス。
        /// </summary>
        private Mutex mutexForBoot =
            new Mutex(false, "{0975DE42-5DA7-460E-BEAC-D91909EDD7BC}");

        private ConfigKeeper<MainWindowConfig> windowConfig =
            new ConfigKeeper<MainWindowConfig>();

        private ConfigKeeper<LipVmdConfig> vmdConfig =
            new ConfigKeeper<LipVmdConfig>();

        private ConfigKeeper<LipEditConfig> editConfig =
            new ConfigKeeper<LipEditConfig>();

        private ConfigKeeper<MorphPresetConfig> presetConfig =
            new ConfigKeeper<MorphPresetConfig>();

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

            // 設定をロード
            if (!this.windowConfig.Load())
            {
                this.windowConfig.Value = new MainWindowConfig();
            }
            if (!this.vmdConfig.Load())
            {
                this.vmdConfig.Value = new LipVmdConfig();
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
            var vm = new LipVmdControlViewModel(this.vmdConfig.Value);
            vm.EditViewModel =
                new LipEditControlViewModel(
                    this.editConfig.Value,
                    this.presetConfig.Value);

            // メインウィンドウ作成
            var window = new MainWindow();

            // パラメータ設定
            this.windowConfig.Value.ApplyTo(window);
            window.ViewModel = vm;
            window.Closing += this.OnMainWindowClosing;

            // メインウィンドウ表示開始
            window.Show();
        }

        /// <summary>
        /// アプリケーションの終了時に呼び出される。
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // 設定をセーブ
            this.vmdConfig.Save();
            this.editConfig.Save();
            this.presetConfig.Save();

            // ミューテクス破棄
            if (this.mutexForBoot != null)
            {
                this.mutexForBoot.Dispose();
                this.mutexForBoot = null;
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
