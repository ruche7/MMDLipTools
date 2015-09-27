using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ruche.wpf.dialogs
{
    /// <summary>
    /// ファイルの保存先を決定するダイアログの表示を行うクラス。
    /// </summary>
    public class SaveFileDialog : FileDialogBase
    {
        /// <summary>
        /// ファイルフィルタークラス。
        /// </summary>
        public class Filter
        {
            /// <summary>
            /// コンストラクタ。
            /// </summary>
            public Filter() : this(null, null)
            {
            }

            /// <summary>
            /// コンストラクタ。
            /// </summary>
            /// <param name="description">説明文。</param>
            /// <param name="extensions">";" または "," 区切りの拡張子リスト。</param>
            public Filter(string description, string extensions)
            {
                this.Description = description;
                this.Extensions =
                    string.IsNullOrWhiteSpace(extensions) ?
                        (new List<string>()) :
                        extensions.Split(';', ',')
                            .Select(
                                ext =>
                                    ext.Trim().Replace("*.", null).Replace(".", null))
                            .ToList();
            }

            /// <summary>
            /// 説明文を取得または設定する。
            /// </summary>
            public string Description
            {
                get { return _description; }
                set { _description = value ?? ""; }
            }
            private string _description = "";

            /// <summary>
            /// 拡張子リストを取得する。
            /// </summary>
            public List<string> Extensions { get; private set; }
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public SaveFileDialog()
        {
            this.Filters = new List<Filter>();
            this.FilterIndex = 0;
            this.IsExtensionAppended = true;
            this.IsOverwriteConfirmed = true;
        }

        /// <summary>
        /// フィルターリストを取得または設定する。
        /// </summary>
        public List<Filter> Filters { get; set; }

        /// <summary>
        /// 選択中のフィルターインデックスを取得または設定する。
        /// </summary>
        public int FilterIndex { get; set; }

        /// <summary>
        /// 選択されたパスに拡張子を付与するか否かを取得または設定する。
        /// </summary>
        public bool IsExtensionAppended { get; set; }

        /// <summary>
        /// 上書きの確認表示を行うか否かを取得または設定する。
        /// </summary>
        public bool IsOverwriteConfirmed { get; set; }

        /// <summary>
        /// ダイアログを表示する。
        /// </summary>
        /// <param name="owner">オーナーウィンドウ。</param>
        /// <returns>選択が行われたならば true 。そうでなければ false 。</returns>
        public override bool Show(Window owner = null)
        {
            if (CommonSaveFileDialog.IsPlatformSupported)
            {
                using (var dialog = new CommonSaveFileDialog())
                {
                    dialog.Title = this.Title;
                    dialog.DefaultDirectory = this.DefaultDirectory;
                    dialog.EnsureFileExists = this.IsFileExistsRequired;
                    dialog.EnsurePathExists = this.IsPathExistsRequired;
                    dialog.EnsureValidNames = this.IsValidNameRequired;
                    dialog.EnsureReadOnly = !this.IsWritableRequired;
                    this.Filters
                        .ConvertAll(
                            f =>
                                new CommonFileDialogFilter(
                                    f.Description,
                                    string.Join(";", f.Extensions))
                                {
                                    ShowExtensions = false,
                                })
                        .ForEach(f => dialog.Filters.Add(f));
                    if (this.FilterIndex >= 0 && this.FilterIndex < this.Filters.Count)
                    {
                        var exts = this.Filters[this.FilterIndex].Extensions;
                        if (exts.Count > 0)
                        {
                            dialog.DefaultExtension = exts[0];
                        }
                    }
                    dialog.AlwaysAppendDefaultExtension = this.IsExtensionAppended;
                    dialog.OverwritePrompt = this.IsOverwriteConfirmed;

                    if (dialog.ShowDialog(owner) != CommonFileDialogResult.Ok)
                    {
                        return false;
                    }

                    this.FileName = dialog.FileName;
                    this.DefaultDirectory = dialog.DefaultDirectory;
                    this.FilterIndex = dialog.SelectedFileTypeIndex - 1;
                }
            }
            else
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.Title = this.Title;
                dialog.InitialDirectory = this.DefaultDirectory;
                dialog.CheckFileExists = this.IsFileExistsRequired;
                dialog.CheckPathExists = this.IsPathExistsRequired;
                dialog.ValidateNames = this.IsValidNameRequired;
                dialog.Filter =
                    (this.Filters == null) ?
                        "" :
                        string.Join(
                            "|",
                            this.Filters.ConvertAll(
                                f =>
                                    f.Description + "|" +
                                    ((f.Extensions.Count > 0) ?
                                        string.Join(
                                            ";",
                                            f.Extensions.ConvertAll(ext => "*." + ext)) :
                                        "*.*")));
                dialog.FilterIndex = Math.Max(0, this.FilterIndex + 1);
                dialog.AddExtension = this.IsExtensionAppended;
                dialog.OverwritePrompt = this.IsOverwriteConfirmed;

                if (dialog.ShowDialog(owner) != true)
                {
                    return false;
                }

                this.FileName = dialog.FileName;
                this.DefaultDirectory = dialog.InitialDirectory;
                this.FilterIndex = dialog.FilterIndex - 1;
            }

            return true;
        }
    }
}
