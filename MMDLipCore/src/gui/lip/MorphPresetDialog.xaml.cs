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
            get { return GetOrBindViewModel().MorphPresets; }
            set { GetOrBindViewModel().MorphPresets = value; }
        }

        /// <summary>
        /// ViewModel を取得する。未バインドであればバインドする。
        /// </summary>
        /// <returns>ViewModel 。</returns>
        private MorphPresetDialogViewModel GetOrBindViewModel()
        {
            var vm = this.DataContext as MorphPresetDialogViewModel;

            if (vm == null)
            {
                vm = new MorphPresetDialogViewModel();
                this.DataContext = vm;
            }

            return vm;
        }

        /// <summary>
        /// ウィンドウの初期化完了時に呼び出される。
        /// </summary>
        private void Window_Initialized(object sender, EventArgs e)
        {
            // メッセージボックス表示処理デリゲートを設定
            var vm = this.GetOrBindViewModel();
            vm.MessageBoxShower =
                (message, caption, button, icon) =>
                    MessageBox.Show(this, message, caption, button, icon);
        }

        /// <summary>
        /// OKボタンの押下時に呼び出される。
        /// </summary>
        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
