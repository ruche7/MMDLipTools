﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using ruche.mmd.morph;
using dlg = ruche.wpf.dialogs;

namespace ruche.mmd.gui.lip
{
    /// <summary>
    /// MorphPresetDialog の View クラス。
    /// </summary>
    public partial class MorphPresetDialog : Window
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MorphPresetDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 口パクモーフプリセットリストを取得または設定する。
        /// </summary>
        public MorphPresetList Presets
        {
            get { return ((dynamic)this.DataContext).Presets; }
            set { ((dynamic)this.DataContext).Presets = value; }
        }

        /// <summary>
        /// モーフウェイトリストの送信を行うデリゲートを取得または設定する。
        /// </summary>
        public Action<IEnumerable<MorphWeightData>> MorphWeightsSender
        {
            get { return ((dynamic)this.DataContext).MorphWeightsSender; }
            set { ((dynamic)this.DataContext).MorphWeightsSender = value; }
        }

        /// <summary>
        /// DataContext の変更時に呼び出される。
        /// </summary>
        private void OnDataContextChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as MorphPresetDialogViewModel;
            if (vm != null && vm.MessageBoxShower == null)
            {
                // メッセージボックス表示処理デリゲートを設定
                vm.MessageBoxShower =
                    (message, caption, button, icon) =>
                        dlg.MessageBox.Show(this, message, caption, button, icon);
            }
        }

        /// <summary>
        /// OKボタンの押下時に呼び出される。
        /// </summary>
        private void OnCloseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
