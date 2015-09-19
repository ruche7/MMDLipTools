using System;

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
        public FileSaveResult() : this(null, null)
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="succeeded">処理の成否。</param>
        /// <param name="text">処理結果テキスト。</param>
        public FileSaveResult(bool? succeeded, string text)
        {
            this.IsSucceeded = succeeded;
            this.Text = text ?? "";
            this.Time = DateTime.Now;
        }

        /// <summary>
        /// 処理の成否を取得する。
        /// </summary>
        public bool? IsSucceeded { get; private set; }

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
