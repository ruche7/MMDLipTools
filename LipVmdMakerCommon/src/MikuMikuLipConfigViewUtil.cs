using System;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ruche.mmd.tools
{
    /// <summary>
    /// MikuMikuLipConfigViewModel を用いる View 用の静的ユーティリティクラス。
    /// </summary>
    public static class MikuMikuLipConfigViewUtil
    {
        /// <summary>
        /// DataContextChanged イベントハンドラーにアタッチすることで
        /// MikuMikuLipConfigViewModel のデリゲートプロパティを設定する。
        /// </summary>
        /// <param name="sender">呼び出し元の View 。</param>
        /// <param name="e">イベント引数。</param>
        public static void OnDataContextChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as MikuMikuLipConfigViewModel;
            if (vm != null)
            {
                var self = sender as DependencyObject;
                var window = (self == null) ? null : Window.GetWindow(self);

                if (vm.MessageBoxShower == null)
                {
                    // メッセージボックス表示処理デリゲートを設定
                    vm.MessageBoxShower =
                        (message, caption, button, icon) =>
                            MessageBox.Show(window, message, caption, button, icon);
                }
                if (vm.CommonFileDialogShower == null)
                {
                    // コモンファイルダイアログ表示処理デリゲートを設定
                    vm.CommonFileDialogShower =
                        dialog =>
                            (dialog.ShowDialog(window) == CommonFileDialogResult.Ok);
                }
            }
        }
    }
}
