using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using ruche.mmd.gui.lip;
using ruche.mmd.morph;
using ruche.mmd.morph.converters;
using ruche.util;
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
        /// コンストラクタ。
        /// </summary>
        public LipVmdControlViewModel()
        {
            // コマンド作成
            this.VmdFileSaveCommand =
                new DelegateCommand(this.ExecuteVmdFileSaveCommand);
        }

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
        [ConfigValueContainer]
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
        /// VMDファイルの保存先ファイルパスリストを取得または設定する。
        /// </summary>
        [ConfigValue]
        public ObservableStringCollection VmdFilePathes
        {
            get { return _vmdFilePathes; }
            set
            {
                var v = value ?? (new ObservableStringCollection());
                if (v != _vmdFilePathes)
                {
                    // 空文字列アイテムが存在しなければ先頭に追加
                    if (!v.Contains(""))
                    {
                        v.Insert(0, "");
                    }

                    _vmdFilePathes = v;
                    this.NotifyPropertyChanged("VmdFilePathes");
                }
            }
        }
        private ObservableStringCollection _vmdFilePathes =
            new ObservableStringCollection(new[] { "" });

        /// <summary>
        /// VMDファイルの保存先ファイルパスを取得または設定する。
        /// </summary>
        public string VmdFilePath
        {
            get { return _vmdFilePath; }
            set
            {
                var v = value ?? "";
                if (v != _vmdFilePath)
                {
                    _vmdFilePath = v;
                    this.NotifyPropertyChanged("VmdFilePath");
                }
            }
        }
        private string _vmdFilePath = "";

        /// <summary>
        /// VMDファイルの保存先選択時の既定のディレクトリパスを取得または設定する。
        /// </summary>
        [ConfigValue]
        public string VmdBaseDirectoryPath
        {
            get { return _vmdBaseDirectoryPath; }
            set
            {
                var v = value ?? "";
                if (v != _vmdBaseDirectoryPath)
                {
                    _vmdBaseDirectoryPath = v;
                    this.NotifyPropertyChanged("VmdBaseDirectoryPath");
                }
            }
        }
        private string _vmdBaseDirectoryPath = "";

        /// <summary>
        /// VMDファイルの上書き確認表示を行うか否かを取得または設定する。
        /// </summary>
        [ConfigValue]
        public bool IsVmdFileOverwriteConfirmed
        {
            get { return _vmdFileOverwriteConfirmed; }
            set
            {
                if (value != _vmdFileOverwriteConfirmed)
                {
                    _vmdFileOverwriteConfirmed = value;
                    this.NotifyPropertyChanged("IsVmdFileOverwriteConfirmed");
                }
            }
        }
        private bool _vmdFileOverwriteConfirmed = true;

        /// <summary>
        /// VMDファイル保存コマンドを取得する。
        /// </summary>
        public ICommand VmdFileSaveCommand { get; private set; }

        /// <summary>
        /// VmdFileSaveCommand を実行する。
        /// </summary>
        private void ExecuteVmdFileSaveCommand(object param)
        {
            // キーフレームリストを作成開始
            var task = this.EditViewModel.MakeKeyFrameListAsync(0);

            // 保存先ファイルパスを決定する
            var path = this.DecideVmdFilePath();
            if (path != null)
            {
                // ファイル書き出し
                this.SaveVmdFile(path, task.Result);
            }
        }

        /// <summary>
        /// VMDファイルの保存先ファイルパスを決定する。
        /// </summary>
        /// <returns>保存先ファイルパス。保存しないならば null 。</returns>
        private string DecideVmdFilePath()
        {
            string path = this.VmdFilePath.Trim();

            if (path == "")
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
                dialog.InitialDirectory = this.VmdBaseDirectoryPath;
                dialog.DereferenceLinks = true;
                dialog.OverwritePrompt = this.IsVmdFileOverwriteConfirmed;
                dialog.ValidateNames = true;

                // 表示
                if (this.CommonDialogShower(dialog) != true)
                {
                    return null;
                }

                // ファイルパスを取得
                path = Path.GetFullPath(dialog.FileName);

                // ベースディレクトリパス更新
                this.VmdBaseDirectoryPath = Path.GetDirectoryName(path);
            }
            else if (this.IsVmdFileOverwriteConfirmed && File.Exists(path))
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
        private void SaveVmdFile(string filePath, IEnumerable<KeyFrame> keyFrames)
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

                // 保存成功したらベースディレクトリパス更新
                this.VmdBaseDirectoryPath = dirPath;
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage = @"指定した保存先に書き出す権限がありません。";
            }
            catch (ArgumentException)
            {
                errorMessage = @"指定した保存先に無効な文字が含まれています。";
            }
            catch (PathTooLongException)
            {
                errorMessage = @"指定した保存先の文字数が多すぎます。";
            }
            catch (DirectoryNotFoundException)
            {
                errorMessage = @"指定した保存先フォルダを作成できません。";
            }
            catch (NotSupportedException)
            {
                errorMessage = @"指定した保存先が不正です。";
            }
            catch (IOException)
            {
                errorMessage = @"指定した保存先に書き出すことができませんでした。";
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
#if DEBUG
                errorMessage += Environment.NewLine + ex.GetType().Name;
#endif // DEBUG
            }

            if (errorMessage != null)
            {
                var message =
                    @"VMDファイルの保存に失敗しました。" + Environment.NewLine +
                    errorMessage;
                this.ShowErrorDialog(message, MessageBoxImage.Error);
                return;
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
