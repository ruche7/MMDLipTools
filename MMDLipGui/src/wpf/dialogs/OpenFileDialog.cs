using System;
using System.Windows;
using System.Windows.Interop;
using Microsoft.WindowsAPICodePack.Dialogs;
using Forms = System.Windows.Forms;

namespace ruche.wpf.dialogs
{
    /// <summary>
    /// ファイルやディレクトリを開くダイアログの表示を行うクラス。
    /// </summary>
    public class OpenFileDialog : FileDialogBase
    {
        /// <summary>
        /// IWin32Window ラッパクラス。
        /// </summary>
        private class Win32Window : Forms.IWin32Window
        {
            public IntPtr Handle { get; private set; }

            public Win32Window(Window window)
            {
                this.Handle = (new WindowInteropHelper(window)).Handle;
            }
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public OpenFileDialog()
        {
            this.IsFolderPicker = false;
        }

        /// <summary>
        /// ディレクトリを選択するか否かを取得または設定する。
        /// </summary>
        public bool IsFolderPicker { get; set; }

        /// <summary>
        /// ダイアログを表示する。
        /// </summary>
        /// <param name="owner">オーナーウィンドウ。</param>
        /// <returns>選択が行われたならば true 。そうでなければ false 。</returns>
        public override bool Show(Window owner = null)
        {
            if (CommonOpenFileDialog.IsPlatformSupported)
            {
                using (var dialog = new CommonOpenFileDialog())
                {
                    dialog.Title = this.Title;
                    dialog.DefaultDirectory = this.DefaultDirectory;
                    dialog.EnsureFileExists = this.IsFileExistsRequired;
                    dialog.EnsurePathExists = this.IsPathExistsRequired;
                    dialog.EnsureValidNames = this.IsValidNameRequired;
                    dialog.EnsureReadOnly = !this.IsWritableRequired;
                    dialog.IsFolderPicker = this.IsFolderPicker;
                    dialog.AllowNonFileSystemItems = false;

                    var result = dialog.ShowDialog(owner);
                    if (result != CommonFileDialogResult.Ok)
                    {
                        return false;
                    }

                    this.FileName = dialog.FileName;
                    this.DefaultDirectory = dialog.DefaultDirectory;
                }
            }
            else if (this.IsFolderPicker)
            {
                using (var dialog = new Forms.FolderBrowserDialog())
                {
                    dialog.Description = this.Title;
                    dialog.SelectedPath = this.DefaultDirectory;
                    dialog.ShowNewFolderButton = !this.IsPathExistsRequired;

                    var result =
                        (owner == null) ?
                            dialog.ShowDialog() :
                            dialog.ShowDialog(new Win32Window(owner));
                    if (result != Forms.DialogResult.OK)
                    {
                        return false;
                    }

                    this.FileName = dialog.SelectedPath;
                    this.DefaultDirectory = dialog.SelectedPath;
                }
            }
            else
            {
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.Title = this.Title;
                dialog.InitialDirectory = this.DefaultDirectory;
                dialog.CheckFileExists = this.IsFileExistsRequired;
                dialog.CheckPathExists = this.IsPathExistsRequired;
                dialog.ValidateNames = this.IsValidNameRequired;

                if (dialog.ShowDialog(owner) != true)
                {
                    return false;
                }

                this.FileName = dialog.FileName;
                this.DefaultDirectory = dialog.InitialDirectory;
            }

            return true;
        }
    }
}
