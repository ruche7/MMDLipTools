using System;
using System.Windows;

namespace ruche.wpf.dialogs
{
    /// <summary>
    /// ファイルダイアログの抽象基底クラス。
    /// </summary>
    public abstract class FileDialogBase
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public FileDialogBase()
        {
            this.FileName = "";
            this.Title = "";
            this.DefaultDirectory = "";
            this.IsValidNameRequired = false;
            this.IsFileExistsRequired = false;
            this.IsPathExistsRequired = false;
            this.IsWritableRequired = false;
        }

        /// <summary>
        /// 選択したファイルまたはディレクトリのパスを取得する。
        /// </summary>
        public string FileName { get; protected set; }

        /// <summary>
        /// タイトルを取得または設定する。
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 既定のディレクトリパスを取得または設定する。
        /// </summary>
        public string DefaultDirectory { get; set; }

        /// <summary>
        /// 存在するファイルのみ選択可能とするか否かを取得または設定する。
        /// </summary>
        public bool IsFileExistsRequired { get; set; }

        /// <summary>
        /// 存在するディレクトリのみ選択可能とするか否かを取得または設定する。
        /// </summary>
        public bool IsPathExistsRequired { get; set; }

        /// <summary>
        /// 正しいファイル名のみ指定可能とするか否かを取得または設定する。
        /// </summary>
        public bool IsValidNameRequired { get; set; }

        /// <summary>
        /// 書き出し可能なファイルのみ選択可能とするか否かを取得または設定する。
        /// </summary>
        public bool IsWritableRequired { get; set; }

        /// <summary>
        /// ダイアログを表示する。
        /// </summary>
        /// <param name="owner">オーナーウィンドウ。</param>
        /// <returns>選択が行われたならば true 。そうでなければ false 。</returns>
        public abstract bool Show(Window owner = null);
    }
}
