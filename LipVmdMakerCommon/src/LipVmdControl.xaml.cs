using System;
using System.Windows;
using System.Windows.Controls;

namespace ruche.mmd.tools
{
    /// <summary>
    /// LipVmdControl の View クラス。
    /// </summary>
    public partial class LipVmdControl : UserControl
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public LipVmdControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// DataContext の変更時に呼び出される。
        /// </summary>
        private void OnDataContextChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as LipVmdControlViewModel;
            if (vm != null)
            {
                var owner = Window.GetWindow(this);

                if (vm.MessageBoxShower == null)
                {
                    // メッセージボックス表示処理デリゲートを設定
                    vm.MessageBoxShower =
                        (message, caption, button, icon) =>
                            MessageBox.Show(owner, message, caption, button, icon);
                }
                if (vm.CommonDialogShower == null)
                {
                    // コモンダイアログ表示処理デリゲートを設定
                    vm.CommonDialogShower = dialog => dialog.ShowDialog(owner);
                }
            }
        }
    }
}
