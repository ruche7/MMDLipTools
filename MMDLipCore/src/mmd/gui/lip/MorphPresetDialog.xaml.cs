using System;
using System.Windows;
using System.Windows.Input;

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
        public MorphPresetList MorphPresets
        {
            get { return ((dynamic)this.DataContext).MorphPresets; }
            set { ((dynamic)this.DataContext).MorphPresets = value; }
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
                        MessageBox.Show(this, message, caption, button, icon);
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
