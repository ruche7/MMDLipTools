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
        /// 口パクモーフVMDファイル保存設定。既定値を用いるならば null 。
        /// </param>
        public LipVmdControlViewModel(LipVmdConfig config)
        {
            // 設定初期化
            this.Config = config ?? (new LipVmdConfig());

            // コマンド作成
            this.FileSaveCommand = new DelegateCommand(this.ExecuteFileSaveCommand);
        }

        /// <summary>
        /// 口パクモーフVMDファイル保存設定を取得する。
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
                    // FPS値は 30 固定
                    v.Fps = 30;

                    _editViewModel = v;
                    this.NotifyPropertyChanged("EditViewModel");
                }
            }
        }
        private LipEditControlViewModel _editViewModel = new LipEditControlViewModel();

        /// <summary>
        /// VMDファイルの保存先ファイルパスリストを取得または設定する。
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
        /// アクティブなVMDファイルの保存先ファイルパスを取得または設定する。
        /// </summary>
        public string ActiveFilePath
        {
            get { return this.Config.ActiveFilePath; }
            set
            {
                var old = this.ActiveFilePath;
                this.Config.ActiveFilePath = value;
                if (this.ActiveFilePath != old)
                {
                    this.NotifyPropertyChanged("ActiveFilePath");
                }
            }
        }

        /// <summary>
        /// VMDファイルの保存先選択時の既定のディレクトリパスを取得または設定する。
        /// </summary>
        public string BaseDirectoryPath
        {
            get { return this.Config.BaseDirectoryPath; }
            set
            {
                var old = this.BaseDirectoryPath;
                this.Config.BaseDirectoryPath = value;
                if (this.BaseDirectoryPath != old)
                {
                    this.NotifyPropertyChanged("BaseDirectoryPath");
                }
            }
        }

        /// <summary>
        /// VMDファイルの上書き確認表示を行うか否かを取得または設定する。
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
        /// 直近のVMDファイル保存ステータス文字列を取得または設定する。
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
        /// 直近のVMDファイル保存ステータス背景色を取得または設定する。
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
        /// VMDファイル保存コマンドを取得する。
        /// </summary>
        public ICommand FileSaveCommand { get; private set; }

        /// <summary>
        /// VmdFileSaveCommand を実行する。
        /// </summary>
        private void ExecuteFileSaveCommand(object param)
        {
            // キーフレームリストを作成開始
            this.EditViewModel.Fps = 30;
            var task = this.EditViewModel.MakeKeyFrameListAsync(0);

            // 保存先ファイルパスを決定する
            var path = this.DecideFilePath();
            if (path != null)
            {
                // ファイル書き出し
                this.SaveFile(path, task.Result);
            }
        }

        /// <summary>
        /// VMDファイルの保存先ファイルパスを決定する。
        /// </summary>
        /// <returns>保存先ファイルパス。保存しないならば null 。</returns>
        private string DecideFilePath()
        {
            string path = this.ActiveFilePath;

            if (string.IsNullOrWhiteSpace(path))
            {
                if (this.CommonDialogShower == null)
                {
                    this.ShowErrorDialog(
                        @"保存先のファイルパスを指定してください。",
                        MessageBoxImage.Warning);
                    return null;
                }

                // 保存先選択ダイアログ作成
                var dialog = new SaveFileDialog();
                dialog.Filter = @"VMDファイル (*.vmd)|*.vmd";
                dialog.DefaultExt = "vmd";
                dialog.AddExtension = true;
                dialog.InitialDirectory = this.BaseDirectoryPath;
                dialog.DereferenceLinks = true;
                dialog.OverwritePrompt = this.IsOverwriteConfirmed;
                dialog.ValidateNames = true;

                // 表示
                if (this.CommonDialogShower(dialog) != true)
                {
                    return null;
                }

                // ファイルパスを取得
                path = Path.GetFullPath(dialog.FileName);
            }
            else if (this.IsOverwriteConfirmed && File.Exists(path))
            {
                // 上書き確認
                var message =
                    @"既にファイルが存在します。" + Environment.NewLine +
                    @"上書きしてもよろしいですか？";
                if (!this.ShowOkCancelDialog(message))
                {
                    return null;
                }
            }

            return path;
        }

        /// <summary>
        /// VMDファイルを新規保存する。
        /// </summary>
        /// <param name="filePath">VMDファイルパス。</param>
        /// <param name="keyFrames">キーフレーム列挙。</param>
        private void SaveFile(string filePath, IEnumerable<KeyFrame> keyFrames)
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
                    VmdWriter.Write(fs, keyFrames, @"");
                }

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
                this.BaseDirectoryPath = dirPath;
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
            if (errorMessage == null)
            {
                var frame = keyFrames.Any() ? keyFrames.Max(f => f.Frame) : 0;
                this.LastStatusText = timeText + @"成功: 長さ " + frame + @" フレーム";
                this.LastStatusBackground =
                    (frame > 0) ? Brushes.PaleGreen : Brushes.LightYellow;
            }
            else
            {
                this.LastStatusText = timeText + @"失敗: " + errorMessage;
                this.LastStatusBackground = Brushes.LightPink;
            }
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
