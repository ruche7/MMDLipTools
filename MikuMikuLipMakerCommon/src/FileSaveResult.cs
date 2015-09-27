using System;
using System.IO;

namespace ruche.mmd.tools
{
    /// <summary>
    /// ファイル保存結果を表すクラス。
    /// </summary>
    public class FileSaveResult
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public FileSaveResult() : this(null, null, null)
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="succeeded">処理の成否。</param>
        /// <param name="filePath">処理対象ファイルパス。</param>
        /// <param name="text">処理結果テキスト。</param>
        public FileSaveResult(
            bool? succeeded,
            string filePath,
            string text)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                this.DirectoryPath = "";
                this.FileName = "";
            }
            else
            {
                var path = Path.GetFullPath(filePath);
                this.DirectoryPath = Path.GetDirectoryName(path);
                this.FileName = Path.GetFileName(path);
            }

            this.IsSucceeded = succeeded;
            this.Text = text ?? "";
            this.Time = DateTime.Now;
        }

        /// <summary>
        /// 処理の成否を取得する。
        /// </summary>
        public bool? IsSucceeded { get; private set; }

        /// <summary>
        /// 処理対象ファイルパスを取得する。
        /// </summary>
        public string FilePath
        {
            get { return Path.Combine(this.DirectoryPath, this.FileName); }
        }

        /// <summary>
        /// 処理対象ファイルパスのディレクトリパス部分文字列を取得する。
        /// </summary>
        public string DirectoryPath { get; private set; }

        /// <summary>
        /// 処理対象ファイルパスのファイル名部分文字列を取得する。
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// 処理結果テキストを取得する。
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// この結果の作成日時を取得する。
        /// </summary>
        public DateTime Time { get; private set; }
    }
}
