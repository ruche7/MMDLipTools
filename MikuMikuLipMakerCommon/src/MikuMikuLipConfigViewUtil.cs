using System;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using dlg = ruche.wpf.dialogs;

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
                    // メッセージボックス表示デリゲートを設定
                    vm.MessageBoxShower =
                        (message, caption, button, icon) =>
                            dlg.MessageBox.Show(window, message, caption, button, icon);
                }
                if (vm.SaveMotionFileDialogShower == null)
                {
                    // モーションファイル保存ダイアログ表示デリゲートを設定
                    vm.SaveMotionFileDialogShower =
                        (dirPath, filters, filterIndex) =>
                        {
                            var dialog = new dlg.SaveFileDialog();
                            dialog.DefaultDirectory = dirPath;
                            dialog.Filters = filters;
                            dialog.FilterIndex = filterIndex;
                            dialog.IsExtensionAppended = true;
                            dialog.IsOverwriteConfirmed = true;
                            dialog.IsValidNameRequired = true;
                            dialog.IsWritableRequired = true;

                            return
                                dialog.Show(window) ?
                                    Tuple.Create(dialog.FileName, dialog.FilterIndex) :
                                    null;
                        };
                }
                if (vm.SelectAutoNamingDirectoryDialogShower == null)
                {
                    // 自動命名保存先ディレクトリ選択ダイアログ表示デリゲートを設定
                    vm.SelectAutoNamingDirectoryDialogShower =
                        dirPath =>
                        {
                            var dialog = new dlg.OpenFileDialog();
                            dialog.IsFolderPicker = true;
                            dialog.Title = @"自動命名保存先フォルダーの選択";
                            dialog.DefaultDirectory = dirPath;
                            dialog.IsValidNameRequired = true;

                            return dialog.Show(window) ? dialog.FileName : null;
                        };
                }
            }
        }
    }
}
