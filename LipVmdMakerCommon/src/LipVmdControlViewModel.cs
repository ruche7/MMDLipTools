using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using ruche.mmd.gui.lip;
using ruche.mmd.morph;
using ruche.mmd.morph.converters;
using ruche.wpf.viewModel;

namespace ruche.mmd.tools
{
    /// <summary>
    /// LipVmdControl の ViewModel クラス。
    /// </summary>
    public class LipVmdControlViewModel : ViewModelBase
    {
        /// <summary>
        /// ファイルフォーマット情報構造体。
        /// </summary>
        private class FileFormatInfo
        {
            /// <summary>
            /// コンストラクタ。
            /// </summary>
            /// <param name="name">名前。</param>
            /// <param name="extension">既定の拡張子。</param>
            /// <param name="fps">FPS値。負数ならば任意のFPS値で保存可能。</param>
            /// <param name="writerMaker">Writer を作成するデリゲート。</param>
            public FileFormatInfo(
                string name,
                string extension,
                decimal fps,
                Func<decimal, MotionDataWriterBase> writerMaker)
            {
                this.Name = name;
                this.Extension = extension;
                this.Fps = fps;
                this.WriterMaker = writerMaker;
            }

            /// <summary>
            /// 名前を取得する。
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// 既定の拡張子を取得する。
            /// </summary>
            public string Extension { get; private set; }

            /// <summary>
            /// FPS値を取得する。負数ならば任意のFPS値で保存可能。
            /// </summary>
            public decimal Fps { get; private set; }

            /// <summary>
            /// Writer を作成するデリゲートを取得する。
            /// </summary>
            public Func<decimal, MotionDataWriterBase> WriterMaker { get; private set; }
        }

        /// <summary>
        /// ファイルフォーマット別情報。
        /// </summary>
        private static readonly
        Dictionary<MotionFileFormat, FileFormatInfo> FileFormatInfos =
            new Dictionary<MotionFileFormat, FileFormatInfo>
            {
                {
                    MotionFileFormat.Vmd,
                    new FileFormatInfo(
                        @"MMD モーション ファイル",
                        @".vmd",
                        30,
                        fps => new VmdWriter())
                },
                {
                    MotionFileFormat.Mvd,
                    new FileFormatInfo(
                        @"MMM モーション ファイル",
                        @".mvd",
                        -1,
                        fps => new MvdWriter((float)fps))
                },
            };

        /// <summary>
        /// メッセージボックスの表示処理を提供するデリゲート。
        /// </summary>
        /// <param name="message">メッセージ。</param>
        /// <param name="caption">キャプション。</param>
        /// <param name="button">ボタン種別。</param>
        /// <param name="image">アイコン種別。</param>
        /// <returns>選択結果。</returns>
        public delegate MessageBoxResult MessageBoxDelegate(
            string message,
            string caption,
            MessageBoxButton button,
            MessageBoxImage image);

        /// <summary>
        /// コモンダイアログの表示処理を提供するデリゲート。
        /// </summary>
        /// <param name="dialog">コモンダイアログ。</param>
        /// <returns>操作結果。</returns>
        public delegate bool? CommonDialogDelegate(CommonDialog dialog);

        /// <summary>
        /// ファイルパスリストの最大要素数。
        /// </summary>
        public static readonly int MaxFilePathCount = 10;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public LipVmdControlViewModel() : this(null)
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="config">
        /// 口パクモーフモーションデータファイル保存設定。既定値を用いるならば null 。
        /// </param>
        public LipVmdControlViewModel(LipVmdConfig config)
        {
            // 設定初期化
            this.Config = config ?? (new LipVmdConfig());

            // コマンド作成
            this.FileSaveCommand = new DelegateCommand(this.ExecuteFileSaveCommand);
        }

        /// <summary>
        /// 口パクモーフモーションデータファイル保存設定を取得する。
        /// </summary>
        /// <remarks>
        /// バインディング用のプロパティではない。
        /// </remarks>
        public LipVmdConfig Config { get; private set; }

        /// <summary>
        /// メッセージボックスの表示処理を行うデリゲートを取得または設定する。
        /// </summary>
        public MessageBoxDelegate MessageBoxShower
        {
            get { return _messageBoxShower; }
            set
            {
                if (value != _messageBoxShower)
                {
                    _messageBoxShower = value;
                    this.NotifyPropertyChanged("MessageBoxShower");
                }
            }
        }
        private MessageBoxDelegate _messageBoxShower = null;

        /// <summary>
        /// コモンダイアログの表示処理を行うデリゲートを取得または設定する。
        /// </summary>
        public CommonDialogDelegate CommonDialogShower
        {
            get { return _commonDialogShower; }
            set
            {
                if (value != _commonDialogShower)
                {
                    _commonDialogShower = value;
                    this.NotifyPropertyChanged("CommonDialogShower");
                }
            }
        }
        private CommonDialogDelegate _commonDialogShower = null;

        /// <summary>
        /// LipEditControl の ViewModel を取得または設定する。
        /// </summary>
        public LipEditControlViewModel EditViewModel
        {
            get { return _editViewModel; }
            set
            {
                var v = value ?? (new LipEditControlViewModel());
                if (v != _editViewModel)
                {
                    _editViewModel = v;
                    this.NotifyPropertyChanged("EditViewModel");
                }
            }
        }
        private LipEditControlViewModel _editViewModel = new LipEditControlViewModel();

        /// <summary>
        /// ファイルの保存先ファイルパスリストを取得または設定する。
        /// </summary>
        public ObservableCollection<string> FilePathes
        {
            get { return this.Config.FilePathes; }
            set
            {
                var old = this.FilePathes;
                this.Config.FilePathes = value;
                if (this.FilePathes != old)
                {
                    this.NotifyPropertyChanged("FilePathes");
                }
            }
        }

        /// <summary>
        /// アクティブなファイルの保存先ファイルパスを取得または設定する。
        /// </summary>
        public string ActiveFilePath
        {
            get { return _activeFilePath; }
            set
            {
                var v = value ?? "";
                if (v != _activeFilePath)
                {
                    _activeFilePath = v;
                    this.NotifyPropertyChanged("ActiveFilePath");
                }
            }
        }
        private string _activeFilePath = "";

        /// <summary>
        /// ファイルの上書き確認表示を行うか否かを取得または設定する。
        /// </summary>
        public bool IsOverwriteConfirmed
        {
            get { return this.Config.IsOverwriteConfirmed; }
            set
            {
                var old = this.IsOverwriteConfirmed;
                this.Config.IsOverwriteConfirmed = value;
                if (this.IsOverwriteConfirmed != old)
                {
                    this.NotifyPropertyChanged("IsOverwriteConfirmed");
                }
            }
        }

        /// <summary>
        /// 直近のファイル保存ステータス文字列を取得または設定する。
        /// </summary>
        public string LastStatusText
        {
            get { return _lastStatusText; }
            set
            {
                var v = value ?? "";
                if (v != _lastStatusText)
                {
                    _lastStatusText = v;
                    this.NotifyPropertyChanged("LastStatusText");
                }
            }
        }
        private string _lastStatusText = "";

        /// <summary>
        /// 直近のファイル保存ステータス背景色を取得または設定する。
        /// </summary>
        public Brush LastStatusBackground
        {
            get { return _lastStatusBackground; }
            set
            {
                var v = value ?? Brushes.Transparent;
                if (v != _lastStatusBackground)
                {
                    _lastStatusBackground = v;
                    this.NotifyPropertyChanged("LastStatusBackground");
                }
            }
        }
        private Brush _lastStatusBackground = Brushes.Transparent;

        /// <summary>
        /// ファイル保存コマンドを取得する。
        /// </summary>
        public ICommand FileSaveCommand { get; private set; }

        /// <summary>
        /// FileSaveCommand を実行する。
        /// </summary>
        private void ExecuteFileSaveCommand(object param)
        {
            // 保存先ファイルパスとファイルフォーマットを決定する
            string path;
            MotionFileFormat format;
            if (this.DecideFile(out path, out format))
            {
                var info = FileFormatInfos[format];

                // 出力FPS値決定
                var fps = (info.Fps < 0) ? this.EditViewModel.Fps : info.Fps;

                // キーフレームリストと Writer 作成
                var keyFrames = this.EditViewModel.MakeKeyFrameList(fps, 0);
                var writer = info.WriterMaker(fps);

                // ファイル書き出し
                if (this.SaveFile(path, keyFrames, writer, fps))
                {
                    // ファイル関連プロパティ更新
                    this.UpdateFileProperties(path, format);
                }
            }
        }

        /// <summary>
        /// ファイルの保存先ファイルパスとファイルフォーマットを決定する。
        /// </summary>
        /// <param name="filePath">保存先ファイルパスの設定先。</param>
        /// <param name="fileFormat">ファイルフォーマットの設定先。</param>
        /// <returns>保存を実行するならば true 。そうでなければ false 。</returns>
        private bool DecideFile(out string filePath, out MotionFileFormat fileFormat)
        {
            filePath = null;
            fileFormat = MotionFileFormat.Vmd;

            string path = this.ActiveFilePath;
            MotionFileFormat? format = null;

            var formats = FileFormatInfos.Keys.ToList();

            if (string.IsNullOrWhiteSpace(path))
            {
                if (this.CommonDialogShower == null)
                {
                    this.ShowErrorDialog(
                        @"保存先のファイルパスを指定してください。",
                        MessageBoxImage.Warning);
                    return false;
                }

                // 保存先選択ダイアログ作成
                var dialog = new SaveFileDialog();
                dialog.Filter =
                    string.Join(
                        "|",
                        from i in FileFormatInfos.Values
                        select (i.Name + @" (*" + i.Extension + @")|*" + i.Extension));
                dialog.FilterIndex =
                    1 +
                    Math.Max(formats.FindIndex(f => f == this.Config.BaseFileFormat), 0);
                dialog.AddExtension = true;
                dialog.InitialDirectory = this.Config.BaseDirectoryPath;
                dialog.DereferenceLinks = true;
                dialog.OverwritePrompt = this.IsOverwriteConfirmed;
                dialog.ValidateNames = true;

                // 表示
                if (this.CommonDialogShower(dialog) != true)
                {
                    return false;
                }

                // ファイルパスを取得
                path = Path.GetFullPath(dialog.FileName);

                // ファイルフォーマットを決定
                var index = dialog.FilterIndex - 1;
                if (index >= 0 && index < formats.Count)
                {
                    format = formats[index];
                }
            }
            else if (this.IsOverwriteConfirmed && File.Exists(path))
            {
                // 上書き確認
                var message =
                    @"既にファイルが存在します。" + Environment.NewLine +
                    @"上書きしてもよろしいですか？";
                if (!this.ShowOkCancelDialog(message))
                {
                    return false;
                }
            }

            var ext = Path.GetExtension(path).ToLower();

            // ファイルフォーマット未決定なら決める
            if (!format.HasValue)
            {
                format = this.Config.BaseFileFormat;
                foreach (var fi in FileFormatInfos)
                {
                    if (ext == fi.Value.Extension)
                    {
                        format = fi.Key;
                        break;
                    }
                }
            }

            // 既定拡張子を付ける
            var defaultExt = FileFormatInfos[format.Value].Extension;
            if (ext != defaultExt)
            {
                path += defaultExt;
            }

            filePath = path;
            fileFormat = format.Value;

            return true;
        }

        /// <summary>
        /// ファイルを新規保存する。
        /// </summary>
        /// <param name="filePath">保存先ファイルパス。</param>
        /// <param name="keyFrames">キーフレーム列挙。</param>
        /// <param name="writer">Writer 。</param>
        /// <param name="fps">情報表示用のFPS値。</param>
        /// <returns>保存できたならば true 。そうでなければ false 。</returns>
        private bool SaveFile(
            string filePath,
            IEnumerable<KeyFrame> keyFrames,
            MotionDataWriterBase writer,
            decimal fps)
        {
            string errorMessage = null;
            try
            {
                // フルパス取得
                var path = Path.GetFullPath(filePath);

                // 親ディレクトリ作成
                var dirPath = Path.GetDirectoryName(path);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                // ファイル作成
                using (var fs = File.Create(path))
                {
                    // 書き出し
                    writer.Write(fs, keyFrames);
                }
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage = @"書き込み権限がありません。";
            }
            catch (ArgumentException)
            {
                errorMessage = @"パスに無効な文字が含まれています。";
            }
            catch (PathTooLongException)
            {
                errorMessage = @"パス文字数が多すぎます。";
            }
            catch (DirectoryNotFoundException)
            {
                errorMessage = @"保存先フォルダを作成できません。";
            }
            catch (NotSupportedException)
            {
                errorMessage = @"パスが不正です。";
            }
            catch (IOException)
            {
                errorMessage = @"書き込みエラーが発生しました。";
            }
            catch (Exception ex)
            {
                errorMessage = ex.GetType().Name;
            }

            var timeText = "[" + DateTime.Now.ToString("HH:mm:ss") + "] ";

            // エラー有無で切り分け
            if (errorMessage != null)
            {
                this.LastStatusText = timeText + @"失敗: " + errorMessage;
                this.LastStatusBackground = Brushes.LightPink;
                return false;
            }

            var frame = keyFrames.Any() ? keyFrames.Max(f => f.Frame) : 0;
            this.LastStatusText =
                timeText + @"成功: 長さ " + frame + @" フレーム (" + fps + @" fps.)";
            this.LastStatusBackground =
                (frame > 0) ? Brushes.PaleGreen : Brushes.LightYellow;
            return true;
        }

        /// <summary>
        /// 保存したファイルの情報を基にファイル関連プロパティを更新する。
        /// </summary>
        /// <param name="filePath">保存したファイルのパス。</param>
        /// <param name="fileFormat">保存したファイルのフォーマット。</param>
        private void UpdateFileProperties(string filePath, MotionFileFormat fileFormat)
        {
            var path = Path.GetFullPath(filePath);

            // 新しいファイルパスリストを作成
            var pathes =
                new[] { path }
                    .Concat(
                        from p in this.FilePathes
                        where
                            !string.IsNullOrWhiteSpace(p) &&
                            string.Compare(Path.GetFullPath(p), path, true) != 0
                        select p)
                    .Take(MaxFilePathCount);
            this.FilePathes = new ObservableCollection<string>(pathes);

            // パス関連更新
            this.ActiveFilePath = path;
            this.Config.BaseDirectoryPath = Path.GetDirectoryName(path);
            this.Config.BaseFileFormat = fileFormat;
        }

        /// <summary>
        /// OK/キャンセルダイアログを表示する。
        /// </summary>
        /// <param name="message">表示するメッセージ。</param>
        /// <returns>Yesが選択されたならば true 。</returns>
        private bool ShowOkCancelDialog(string message)
        {
            var shower = this.MessageBoxShower ?? MessageBox.Show;
            var result =
                shower(
                    message,
                    @"確認",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question);

            return (result == MessageBoxResult.OK);
        }

        /// <summary>
        /// エラーダイアログを表示する。
        /// </summary>
        /// <param name="message">エラーメッセージ。</param>
        /// <param name="image">アイコン種別。</param>
        private void ShowErrorDialog(string message, MessageBoxImage image)
        {
            var shower = this.MessageBoxShower ?? MessageBox.Show;
            shower(message, @"エラー", MessageBoxButton.OK, image);
        }
    }
}
