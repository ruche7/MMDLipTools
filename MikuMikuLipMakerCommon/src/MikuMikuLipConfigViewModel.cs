using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using ruche.mmd.gui.lip;
using ruche.mmd.morph;
using ruche.mmd.morph.converters;
using ruche.mmd.morph.lip;
using ruche.util;
using ruche.wpf.viewModel;
using dlg = ruche.wpf.dialogs;

namespace ruche.mmd.tools
{
    /// <summary>
    /// MikuMikuLipControl の ViewModel クラス。
    /// </summary>
    public class MikuMikuLipConfigViewModel : ViewModelBase
    {
        /// <summary>
        /// メッセージボックスの表示処理を提供するデリゲート。
        /// </summary>
        /// <param name="message">メッセージ。</param>
        /// <param name="caption">キャプション。</param>
        /// <param name="button">ボタン種別。</param>
        /// <param name="icon">アイコン種別。</param>
        /// <returns>選択結果。</returns>
        public delegate dlg.MessageBox.Result MessageBoxDelegate(
            string message,
            string caption,
            dlg.MessageBox.Button button,
            dlg.MessageBox.Icon icon);

        /// <summary>
        /// ファイル保存ダイアログの表示処理を提供するデリゲート。
        /// </summary>
        /// <param name="defaultDirectory">既定のディレクトリパス。</param>
        /// <param name="filters">フィルターリスト。</param>
        /// <param name="filterIndex">初期フィルターインデックス。</param>
        /// <returns>
        /// 選択されたファイルパスとフィルターインデックス。
        /// 選択されなかった場合は null 。
        /// </returns>
        public delegate Tuple<string, int> SaveFileDialogDelegate(
            string defaultDirectory,
            List<dlg.SaveFileDialog.Filter> filters,
            int filterIndex);

        /// <summary>
        /// ディレクトリ選択ダイアログの表示処理を提供するデリゲート。
        /// </summary>
        /// <param name="defaultDirectory">既定のディレクトリパス。</param>
        /// <returns>
        /// 選択されたディレクトリパス。選択されなかった場合は null 。
        /// </returns>
        public delegate string SelectDirectoryDialogDelegate(string defaultDirectory);

        /// <summary>
        /// タイムラインテーブル情報の送信処理を提供するデリゲート。
        /// </summary>
        /// <param name="tlTable">モーフ別タイムラインテーブル。</param>
        /// <param name="unitSeconds">
        /// ユニット基準長(「ア」の長さ)に相当する秒数値。
        /// </param>
        /// <param name="edgeWeightZero">
        /// キーフレームリストの先頭と終端で、
        /// 含まれている全モーフのウェイト値をゼロ初期化するならば true 。
        /// </param>
        /// <param name="edgeWeightHeld">
        /// クライアント側が対応していれば、キーフレームリスト挿入位置前後の
        /// ウェイト値を保持するならば true 。
        /// </param>
        public delegate void TimelineTableSendDelegate(
            MorphTimelineTable tlTable,
            decimal unitSeconds,
            bool edgeWeightZero,
            bool edgeWeightHeld);

        /// <summary>
        /// ファイルフォーマット情報構造体。
        /// </summary>
        private class FileFormatInfo
        {
            /// <summary>
            /// コンストラクタ。
            /// </summary>
            /// <param name="format">ファイルフォーマット。</param>
            /// <param name="extension">既定の拡張子。</param>
            /// <param name="fps">FPS値。負数ならば任意のFPS値で保存可能。</param>
            /// <param name="writerMaker">Writer を作成するデリゲート。</param>
            public FileFormatInfo(
                MotionFileFormat format,
                string extension,
                decimal fps,
                Func<decimal, MotionDataWriterBase> writerMaker)
            {
                this.Description = "";

                // 列挙値のメタデータ取得
                var info = format.GetType().GetField(format.ToString());

                // DisplayAttribute 属性を取得
                var attrs =
                    info?.GetCustomAttributes(typeof(DisplayAttribute), false)
                        as DisplayAttribute[];
                if (attrs != null && attrs.Length > 0)
                {
                    // 説明文字列に設定
                    this.Description = attrs[0].GetShortName();
                }

                this.Format = format;
                this.Extension = extension;
                this.Fps = fps;
                this.WriterMaker = writerMaker;
            }

            /// <summary>
            /// ファイルフォーマットを取得する。
            /// </summary>
            public MotionFileFormat Format { get; }

            /// <summary>
            /// 説明文字列を取得する。
            /// </summary>
            public string Description { get; }

            /// <summary>
            /// 既定の拡張子を取得する。
            /// </summary>
            public string Extension { get; }

            /// <summary>
            /// FPS値を取得する。負数ならば任意のFPS値で保存可能。
            /// </summary>
            public decimal Fps { get; }

            /// <summary>
            /// Writer を作成するデリゲートを取得する。
            /// </summary>
            public Func<decimal, MotionDataWriterBase> WriterMaker { get; }
        }

        /// <summary>
        /// ファイルフォーマット情報コレクション。
        /// </summary>
        private static readonly ReadOnlyCollection<FileFormatInfo> FileFormatInfos =
            new ReadOnlyCollection<FileFormatInfo>(
                new[]
                {
                    new FileFormatInfo(
                        MotionFileFormat.Vmd,
                        @"vmd",
                        30,
                        fps => new VmdWriter()),
                    new FileFormatInfo(
                        MotionFileFormat.Mvd,
                        @"mvd",
                        -1,
                        fps => new MvdWriter((float)fps)),
                });

        /// <summary>
        /// 1文字以上の空白文字にマッチする正規表現。
        /// </summary>
        private static readonly Regex RegexBlank = new Regex(@"\s+");

        /// <summary>
        /// 選択可能な列挙値コレクションを作成する。
        /// </summary>
        /// <typeparam name="T">列挙型。</typeparam>
        /// <param name="first">初期選択値。</param>
        /// <param name="selectedChanged">
        /// 選択状態変更時に呼び出されるイベントデリゲート。
        /// </param>
        /// <returns>作成されたコレクション。</returns>
        private static
        SelectableValueCollection<T> MakeSelectableEnumValueCollection<T>(
            T first,
            EventHandler selectedChanged)
        {
            var dest =
                new SelectableValueCollection<T>(
                    ((T[])Enum.GetValues(typeof(T)))
                        .Select(
                            e =>
                                new SelectableValue<T>
                                {
                                    Value = e,
                                    IsSelected =
                                        EqualityComparer<T>.Default.Equals(e, first),
                                }),
                    true);

            foreach (var item in dest)
            {
                item.IsSelectedChanged += selectedChanged;
            }

            return dest;
        }

        /// <summary>
        /// ファイルフォーマット情報を取得する。
        /// </summary>
        /// <param name="format">ファイルフォーマット。</param>
        /// <returns>ファイルフォーマット情報</returns>
        private static FileFormatInfo GetFileFormatInfo(MotionFileFormat format) =>
            FileFormatInfos.First(info => info.Format == format);

        /// <summary>
        /// ファイル自動命名における入力文または読み仮名文字列を作成する。
        /// </summary>
        /// <param name="src">入力文または読み仮名。</param>
        /// <returns>作成した文字列。</returns>
        private static string MakeAutoNamingString(string src)
        {
            if (string.IsNullOrEmpty(src))
            {
                return "(blank)";
            }

            // 空白文字を半角スペース1文字に短縮
            // ファイル名に使えない文字を置換
            var invalidChars = Path.GetInvalidFileNameChars();
            var dest =
                string.Join(
                    "",
                    from c in RegexBlank.Replace(src, " ")
                    select (Array.IndexOf(invalidChars, c) < 0) ? c : '_');

            // 文字数制限
            int maxLength = 12;
            if (dest.Length > maxLength)
            {
                dest = dest.Substring(0, maxLength - 1) + "-";
            }

            return dest;
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MikuMikuLipConfigViewModel() : this(null)
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="config">
        /// 口パクモーフモーションデータファイル保存設定。既定値を用いるならば null 。
        /// </param>
        public MikuMikuLipConfigViewModel(MikuMikuLipConfig config)
        {
            // 設定初期化
            this.Config = config ?? (new MikuMikuLipConfig());

            // 選択可能コレクション作成
            this.FileFormats =
                MakeSelectableEnumValueCollection(
                    this.Config.ActiveFileFormat,
                    this.OnFileFormatsItemIsSelectedChanged);
            this.AutoNamingFormats =
                MakeSelectableEnumValueCollection(
                    this.Config.AutoNamingFormat,
                    this.OnAutoNamingFormatsItemIsSelectedChanged);

            // モーフウェイトリスト送信コマンドジェスチャ作成
            this.MorphWeightsSendGestures =
                new ReadOnlyObservableCollection<KeyGesture>(
                    new ObservableCollection<KeyGesture>
                    {
                        new KeyGesture(Key.F1),
                        new KeyGesture(Key.F2),
                        new KeyGesture(Key.F3),
                        new KeyGesture(Key.F4),
                        new KeyGesture(Key.F5),
                        new KeyGesture(Key.F6),
                    });

            // コマンド作成
            this.AutoNamingSaveCommand =
                new DelegateCommand(this.ExecuteAutoNamingSaveCommand);
            this.SaveAsCommand =
                new DelegateCommand(
                    this.ExecuteSaveAsCommand,
                    _ => this.SaveMotionFileDialogShower != null);
            this.AutoNamingDirectoryCommand =
                new DelegateCommand(
                    this.ExecuteAutoNamingDirectoryCommand,
                    _ => this.SelectAutoNamingDirectoryDialogShower != null);
            this.LastDirectoryOpenCommand =
                new DelegateCommand(
                    this.ExecuteLastDirectoryOpenCommand,
                    _ =>
                        this.LastSaveResult != null &&
                        !string.IsNullOrWhiteSpace(this.LastSaveResult.DirectoryPath));
            this.TimelineSendCommand =
                new DelegateCommand(
                    this.ExecuteTimelineSendCommand,
                    _ => this.TimelineTableSender != null);
            this.MorphWeightsSendCommand =
                new DelegateCommand(
                    this.ExecuteMorphWeightsSendCommand,
                    _ =>
                        this.MorphWeightsSender != null &&
                        this.EditViewModel.SelectedPresetIndex >= 0);
            this.VersionInfoCommand =
                new DelegateCommand(
                    _ => this.VersionShower(),
                    _ => this.IsVersionShowerEnabled);
            this.LicenseInfoCommand =
                new DelegateCommand(
                    _ => this.LicenseShower(),
                    _ => this.IsLicenseShowerEnabled);
        }

        /// <summary>
        /// 口パクモーフモーションデータファイル保存設定を取得する。
        /// </summary>
        /// <remarks>
        /// バインディング用のプロパティではない。
        /// </remarks>
        public MikuMikuLipConfig Config { get; }

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
                    this.NotifyPropertyChanged(nameof(MessageBoxShower));
                }
            }
        }
        private MessageBoxDelegate _messageBoxShower = null;

        /// <summary>
        /// モーションファイル保存ダイアログの表示処理を行うデリゲートを
        /// 取得または設定する。
        /// </summary>
        public SaveFileDialogDelegate SaveMotionFileDialogShower
        {
            get { return _saveMotionFileDialogShower; }
            set
            {
                if (value != _saveMotionFileDialogShower)
                {
                    _saveMotionFileDialogShower = value;
                    this.NotifyPropertyChanged(nameof(SaveMotionFileDialogShower));
                }
            }
        }
        private SaveFileDialogDelegate _saveMotionFileDialogShower = null;

        /// <summary>
        /// 自動命名保存先ディレクトリ選択ダイアログの表示処理を行うデリゲートを
        /// 取得または設定する。
        /// </summary>
        public SelectDirectoryDialogDelegate SelectAutoNamingDirectoryDialogShower
        {
            get { return _selectAutoNamingDirectoryDialogShower; }
            set
            {
                if (value != _selectAutoNamingDirectoryDialogShower)
                {
                    _selectAutoNamingDirectoryDialogShower = value;
                    this.NotifyPropertyChanged(nameof(SelectAutoNamingDirectoryDialogShower));
                }
            }
        }
        private SelectDirectoryDialogDelegate _selectAutoNamingDirectoryDialogShower =
            null;

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
                    var oldSender = this.MorphWeightsSender;
                    var oldEnabled = this.IsSomeSenderEnabled;

                    _editViewModel = v;
                    this.NotifyPropertyChanged(nameof(EditViewModel));

                    if (this.MorphWeightsSender != oldSender)
                    {
                        this.NotifyPropertyChanged(nameof(MorphWeightsSender));
                    }
                    if (this.IsSomeSenderEnabled != oldEnabled)
                    {
                        this.NotifyPropertyChanged(nameof(IsSomeSenderEnabled));
                    }
                }
            }
        }
        private LipEditControlViewModel _editViewModel =
            new LipEditControlViewModel();

        /// <summary>
        /// 選択可能なファイルフォーマットのコレクションを取得する。
        /// </summary>
        public SelectableValueCollection<MotionFileFormat> FileFormats { get; }

        /// <summary>
        /// アクティブなファイルフォーマットを取得または設定する。
        /// </summary>
        public MotionFileFormat ActiveFileFormat
        {
            get { return this.FileFormats.SelectedItem.Value; }
            set { this.FileFormats.SelectItemByValue(value); }
        }

        /// <summary>
        /// FileFormats アイテムの選択状態が変更された時に呼び出される。
        /// </summary>
        private void OnFileFormatsItemIsSelectedChanged(object sender, EventArgs e)
        {
            var item = sender as SelectableValue<MotionFileFormat>;
            if (item?.IsSelected == true)
            {
                var old = this.Config.ActiveFileFormat;
                this.Config.ActiveFileFormat = item.Value;
                if (this.Config.ActiveFileFormat != old)
                {
                    this.NotifyPropertyChanged(nameof(ActiveFileFormat));
                }
            }
        }

        /// <summary>
        /// 選択可能なファイル自動命名フォーマットのコレクションを取得する。
        /// </summary>
        public SelectableValueCollection<AutoNamingFormat> AutoNamingFormats { get; }

        /// <summary>
        /// ファイル自動命名フォーマットを取得または設定する。
        /// </summary>
        public AutoNamingFormat AutoNamingFormat
        {
            get { return this.AutoNamingFormats.SelectedItem.Value; }
            set { this.AutoNamingFormats.SelectItemByValue(value); }
        }

        /// <summary>
        /// AutoNamingFormats アイテムの選択状態が変更された時に呼び出される。
        /// </summary>
        private void OnAutoNamingFormatsItemIsSelectedChanged(
            object sender,
            EventArgs e)
        {
            var item = sender as SelectableValue<AutoNamingFormat>;
            if (item?.IsSelected == true)
            {
                var old = this.Config.AutoNamingFormat;
                this.Config.AutoNamingFormat = item.Value;
                if (this.Config.AutoNamingFormat != old)
                {
                    this.NotifyPropertyChanged(nameof(AutoNamingFormat));
                }
            }
        }

        /// <summary>
        /// ファイル自動命名時の保存先ディレクトリパスを取得または設定する。
        /// </summary>
        public string AutoNamingDirectoryPath
        {
            get { return this.Config.AutoNamingDirectoryPath; }
            set
            {
                var old = this.AutoNamingDirectoryPath;
                this.Config.AutoNamingDirectoryPath = value;
                if (this.AutoNamingDirectoryPath != old)
                {
                    this.NotifyPropertyChanged(nameof(AutoNamingDirectoryPath));
                }
            }
        }

        /// <summary>
        /// ファイル自動命名時に上書き確認表示を行うか否かを取得または設定する。
        /// </summary>
        public bool IsAutoNamingOverwriteConfirmed
        {
            get { return this.Config.IsAutoNamingOverwriteConfirmed; }
            set
            {
                var old = this.IsAutoNamingOverwriteConfirmed;
                this.Config.IsAutoNamingOverwriteConfirmed = value;
                if (this.IsAutoNamingOverwriteConfirmed != old)
                {
                    this.NotifyPropertyChanged(nameof(IsAutoNamingOverwriteConfirmed));
                }
            }
        }

        /// <summary>
        /// 名前を付けて保存する際の既定のディレクトリパスを取得または設定する。
        /// </summary>
        public string DefaultDirectoryPath
        {
            get { return this.Config.DefaultDirectoryPath; }
            set
            {
                var old = this.DefaultDirectoryPath;
                this.Config.DefaultDirectoryPath = value;
                if (this.DefaultDirectoryPath != old)
                {
                    this.NotifyPropertyChanged(nameof(DefaultDirectoryPath));
                }
            }
        }

        /// <summary>
        /// テキストファイルを同時に保存するか否かを取得または設定する。
        /// </summary>
        public bool IsSavingWithText
        {
            get { return this.Config.IsSavingWithText; }
            set
            {
                var old = this.IsSavingWithText;
                this.Config.IsSavingWithText = value;
                if (this.IsSavingWithText != old)
                {
                    this.NotifyPropertyChanged(nameof(IsSavingWithText));
                }
            }
        }

        /// <summary>
        /// 直近のファイル保存処理結果を取得する。
        /// </summary>
        public FileSaveResult LastSaveResult
        {
            get { return _lastSaveResult; }
            private set
            {
                var v = value ?? (new FileSaveResult());
                if (v != _lastSaveResult)
                {
                    _lastSaveResult = v;
                    this.NotifyPropertyChanged(nameof(LastSaveResult));
                }
            }
        }
        public FileSaveResult _lastSaveResult = new FileSaveResult();

        /// <summary>
        /// タイムラインテーブル情報の送信処理を行うデリゲートを取得または設定する。
        /// </summary>
        public TimelineTableSendDelegate TimelineTableSender
        {
            get { return _timelineTableSender; }
            set
            {
                if (value != _timelineTableSender)
                {
                    bool oldEnabled = this.IsSomeSenderEnabled;

                    _timelineTableSender = value;
                    this.NotifyPropertyChanged(nameof(TimelineTableSender));

                    if (this.IsSomeSenderEnabled != oldEnabled)
                    {
                        this.NotifyPropertyChanged(nameof(IsSomeSenderEnabled));
                    }
                }
            }
        }
        private TimelineTableSendDelegate _timelineTableSender = null;

        /// <summary>
        /// モーフウェイトリストの送信を行うデリゲートを取得または設定する。
        /// </summary>
        public Action<MorphWeightDataList> MorphWeightsSender
        {
            get { return this.EditViewModel.MorphWeightsSender; }
            set
            {
                var oldEnabled = this.IsSomeSenderEnabled;

                var old = this.MorphWeightsSender;
                this.EditViewModel.MorphWeightsSender = value;
                if (this.MorphWeightsSender != old)
                {
                    this.NotifyPropertyChanged(nameof(MorphWeightsSender));
                }

                if (this.IsSomeSenderEnabled != oldEnabled)
                {
                    this.NotifyPropertyChanged(nameof(IsSomeSenderEnabled));
                }
            }
        }

        /// <summary>
        /// いずれかの送信デリゲートに有効な値が設定されているか否かを取得する。
        /// </summary>
        public bool IsSomeSenderEnabled =>
            (this.TimelineTableSender != null || this.MorphWeightsSender != null);

        /// <summary>
        /// クライアント側が対応していれば、キーフレームリスト挿入位置前後の
        /// ウェイト値を保持するか否かを取得または設定する。
        /// </summary>
        public bool IsEdgeWeightHeld
        {
            get { return this.Config.IsEdgeWeightHeld; }
            set
            {
                var old = this.IsEdgeWeightHeld;
                this.Config.IsEdgeWeightHeld = value;
                if (this.IsEdgeWeightHeld != old)
                {
                    this.NotifyPropertyChanged(nameof(IsEdgeWeightHeld));
                }
            }
        }

        /// <summary>
        /// バージョン情報の表示を行うデリゲートを取得または設定する。
        /// </summary>
        public Action VersionShower
        {
            get { return _versionShower; }
            set
            {
                if (value != _versionShower)
                {
                    var oldEnabled = this.IsVersionShowerEnabled;
                    var oldSomeEnabled = this.IsSomeInfoShowerEnabled;

                    _versionShower = value;
                    this.NotifyPropertyChanged(nameof(VersionShower));

                    if (this.IsVersionShowerEnabled != oldEnabled)
                    {
                        this.NotifyPropertyChanged(nameof(IsVersionShowerEnabled));
                    }
                    if (this.IsSomeInfoShowerEnabled != oldSomeEnabled)
                    {
                        this.NotifyPropertyChanged(nameof(IsSomeInfoShowerEnabled));
                    }
                }
            }
        }
        private Action _versionShower = null;

        /// <summary>
        /// VersionShower プロパティに有効な値が設定されているか否かを取得する。
        /// </summary>
        public bool IsVersionShowerEnabled => (this.VersionShower != null);

        /// <summary>
        /// ライセンス表記の表示を行うデリゲートを取得または設定する。
        /// </summary>
        public Action LicenseShower
        {
            get { return _licenseShower; }
            set
            {
                if (value != _licenseShower)
                {
                    var oldEnabled = this.IsLicenseShowerEnabled;
                    var oldSomeEnabled = this.IsSomeInfoShowerEnabled;

                    _licenseShower = value;
                    this.NotifyPropertyChanged(nameof(LicenseShower));

                    if (this.IsLicenseShowerEnabled != oldEnabled)
                    {
                        this.NotifyPropertyChanged(nameof(IsLicenseShowerEnabled));
                    }
                    if (this.IsSomeInfoShowerEnabled != oldSomeEnabled)
                    {
                        this.NotifyPropertyChanged(nameof(IsSomeInfoShowerEnabled));
                    }
                }
            }
        }
        private Action _licenseShower = null;

        /// <summary>
        /// LicenseShower プロパティに有効な値が設定されているか否かを取得する。
        /// </summary>
        public bool IsLicenseShowerEnabled => (this.LicenseShower != null);

        /// <summary>
        /// VersionShower プロパティまたは LicenseShower プロパティに有効な値が
        /// 設定されているか否かを取得する。
        /// </summary>
        public bool IsSomeInfoShowerEnabled =>
            (this.IsVersionShowerEnabled || this.IsLicenseShowerEnabled);

        /// <summary>
        /// 自動命名保存コマンドを取得する。
        /// </summary>
        public ICommand AutoNamingSaveCommand { get; }

        /// <summary>
        /// 自動命名保存コマンドのキージェスチャを取得または設定する。
        /// </summary>
        public KeyGesture AutoNamingSaveGesture
        {
            get { return _autoNamingSaveGesture; }
            set
            {
                if (value != _autoNamingSaveGesture)
                {
                    _autoNamingSaveGesture = value;
                    this.NotifyPropertyChanged(nameof(AutoNamingSaveGesture));
                    this.NotifyPropertyChanged(nameof(AutoNamingSaveGestureText));
                }
            }
        }
        private KeyGesture _autoNamingSaveGesture =
            new KeyGesture(Key.S, ModifierKeys.Control);

        /// <summary>
        /// 自動命名保存コマンドのキージェスチャ文字列を取得する。
        /// </summary>
        public string AutoNamingSaveGestureText =>
            (this.AutoNamingSaveGesture == null) ?
                "" :
                this.AutoNamingSaveGesture.GetDisplayStringForCulture(
                    CultureInfo.CurrentUICulture);

        /// <summary>
        /// 名前を付けて保存コマンドを取得する。
        /// </summary>
        public ICommand SaveAsCommand { get; }

        /// <summary>
        /// 名前を付けて保存コマンドのキージェスチャを取得または設定する。
        /// </summary>
        public KeyGesture SaveAsGesture
        {
            get { return _saveAsGesture; }
            set
            {
                if (value != _saveAsGesture)
                {
                    _saveAsGesture = value;
                    this.NotifyPropertyChanged(nameof(SaveAsGesture));
                    this.NotifyPropertyChanged(nameof(SaveAsGestureText));
                }
            }
        }
        private KeyGesture _saveAsGesture =
            new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift);

        /// <summary>
        /// 名前を付けて保存コマンドのキージェスチャ文字列を取得する。
        /// </summary>
        public string SaveAsGestureText =>
            (this.SaveAsGesture == null) ?
                "" :
                this.SaveAsGesture.GetDisplayStringForCulture(
                    CultureInfo.CurrentUICulture);

        /// <summary>
        /// 自動命名保存先設定コマンドを取得する。
        /// </summary>
        public ICommand AutoNamingDirectoryCommand { get; }

        /// <summary>
        /// 自動命名保存先設定コマンドのキージェスチャを取得または設定する。
        /// </summary>
        public KeyGesture AutoNamingDirectoryGesture
        {
            get { return _autoNamingDirectoryGesture; }
            set
            {
                if (value != _autoNamingDirectoryGesture)
                {
                    _autoNamingDirectoryGesture = value;
                    this.NotifyPropertyChanged(nameof(AutoNamingDirectoryGesture));
                    this.NotifyPropertyChanged(nameof(AutoNamingDirectoryGestureText));
                }
            }
        }
        private KeyGesture _autoNamingDirectoryGesture =
            new KeyGesture(Key.D, ModifierKeys.Control);

        /// <summary>
        /// 自動命名保存先設定コマンドのキージェスチャ文字列を取得する。
        /// </summary>
        public string AutoNamingDirectoryGestureText =>
            (this.AutoNamingDirectoryGesture == null) ?
                "" :
                this.AutoNamingDirectoryGesture.GetDisplayStringForCulture(
                    CultureInfo.CurrentUICulture);

        /// <summary>
        /// 保存先フォルダーを開くコマンドを取得する。
        /// </summary>
        public ICommand LastDirectoryOpenCommand { get; }

        /// <summary>
        /// タイムライン送信コマンドを取得する。
        /// </summary>
        public ICommand TimelineSendCommand { get; }

        /// <summary>
        /// タイムライン送信コマンドのキージェスチャを取得または設定する。
        /// </summary>
        public KeyGesture TimelineSendGesture
        {
            get { return _timelineSendGesture; }
            set
            {
                if (value != _timelineSendGesture)
                {
                    _timelineSendGesture = value;
                    this.NotifyPropertyChanged(nameof(TimelineSendGesture));
                    this.NotifyPropertyChanged(nameof(TimelineSendGestureText));
                }
            }
        }
        private KeyGesture _timelineSendGesture = new KeyGesture(Key.F8);

        /// <summary>
        /// タイムライン送信コマンドのキージェスチャ文字列を取得する。
        /// </summary>
        public string TimelineSendGestureText =>
            (this.TimelineSendGesture == null) ?
                "" :
                this.TimelineSendGesture.GetDisplayStringForCulture(
                    CultureInfo.CurrentUICulture);

        /// <summary>
        /// モーフウェイトリスト送信コマンドを取得する。
        /// </summary>
        /// <remarks>
        /// コマンドパラメータで LipId 文字列を指定する。
        /// </remarks>
        public ICommand MorphWeightsSendCommand { get; }

        /// <summary>
        /// モーフウェイトリスト送信コマンドのキージェスチャコレクションを
        /// 取得または設定する。
        /// </summary>
        /// <remarks>
        /// インデックス 0 ～ 4 が "あ" ～ "お" 、インデックス 5 が閉口時に対応する。
        /// </remarks>
        public ReadOnlyObservableCollection<KeyGesture> MorphWeightsSendGestures
        {
            get { return _morphWeightsSendGestures; }
            set
            {
                if (value != _morphWeightsSendGestures)
                {
                    _morphWeightsSendGestures = value;
                    this.NotifyPropertyChanged(nameof(MorphWeightsSendGestures));

                    this.MorphWeightsSendGestureTexts =
                        (value == null) ?
                            null :
                            value.Select(
                                g =>
                                    (g == null) ?
                                        "" :
                                        g.GetDisplayStringForCulture(
                                            CultureInfo.CurrentUICulture))
                                .ToList()
                                .AsReadOnly();
                    this.NotifyPropertyChanged(nameof(MorphWeightsSendGestureTexts));
                }
            }
        }
        private ReadOnlyObservableCollection<KeyGesture> _morphWeightsSendGestures;

        /// <summary>
        /// モーフウェイトリスト送信コマンドのキージェスチャ文字列コレクションを
        /// 取得する。
        /// </summary>
        public ReadOnlyCollection<string> MorphWeightsSendGestureTexts
        {
            get;
            private set;
        }

        /// <summary>
        /// バージョン情報表示コマンドを取得する。
        /// </summary>
        public ICommand VersionInfoCommand { get; }

        /// <summary>
        /// ライセンス表記表示コマンドを取得する。
        /// </summary>
        public ICommand LicenseInfoCommand { get; }

        /// <summary>
        /// AutoNamingSaveCommand を実行する。
        /// </summary>
        private void ExecuteAutoNamingSaveCommand(object param)
        {
            // 保存先ディレクトリ用意
            if (!this.PrepareAutoNamingDirectory())
            {
                return;
            }

            // ファイルパス決定
            var path = this.DecideAutoNamingFilePath();
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            // 保存
            this.SaveFile(ref path, this.ActiveFileFormat, true);
        }

        /// <summary>
        /// SaveAsCommand を実行する。
        /// </summary>
        private void ExecuteSaveAsCommand(object param)
        {
            var shower = this.SaveMotionFileDialogShower;
            if (shower == null)
            {
                return;
            }

            var filters =
                FileFormatInfos
                    .Select(
                        info =>
                            new dlg.SaveFileDialog.Filter(
                                info.Description + " (*." + info.Extension + ")",
                                info.Extension))
                    .ToList();
            var filterIndex =
                FileFormatInfos.ToList().FindIndex(
                    info => info.Format == this.ActiveFileFormat);

            // ダイアログ表示
            var result = shower(this.DefaultDirectoryPath, filters, filterIndex);
            if (result == null)
            {
                return;
            }

            var path = result.Item1;
            var format = FileFormatInfos[result.Item2].Format;

            // 保存
            if (this.SaveFile(ref path, format, false))
            {
                // 既定ディレクトリパス更新
                this.Config.DefaultDirectoryPath = Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// AutoNamingDirectoryCommand を実行する。
        /// </summary>
        private void ExecuteAutoNamingDirectoryCommand(object param)
        {
            this.SelectAutoNamingDirectory();
        }

        /// <summary>
        /// LastDirectoryOpenCommand を実行する。
        /// </summary>
        private void ExecuteLastDirectoryOpenCommand(object param)
        {
            var result = this.LastSaveResult;
            if (result == null || string.IsNullOrWhiteSpace(result.DirectoryPath))
            {
                return;
            }

            if (Directory.Exists(result.DirectoryPath))
            {
                Process.Start(result.DirectoryPath);
            }
            else
            {
                this.ShowErrorDialog(
                    result.DirectoryPath + Environment.NewLine + Environment.NewLine +
                    @"フォルダーが見つかりません。",
                    dlg.MessageBox.Icon.Error);
            }
        }

        /// <summary>
        /// TimelineSendCommand を実行する。
        /// </summary>
        private void ExecuteTimelineSendCommand(object param)
        {
            var sender = this.TimelineTableSender;
            if (sender == null)
            {
                return;
            }

            var spanRange = this.EditViewModel.SpanRange;
            var spanSeconds = this.EditViewModel.EditConfig.SpanSeconds;
            bool edgeWeightZero = this.EditViewModel.IsEdgeWeightZero;
            bool edgeWeightHeld = this.IsEdgeWeightHeld;

            this.EditViewModel
                .StartMakeMorphTimelineTable()
                .ContinueWith(
                    t =>
                    {
                        var tlTable = t.Result;

                        // 基準秒数算出
                        var unitSeconds = spanSeconds;
                        if (spanRange == LipSpanRange.All)
                        {
                            var end = tlTable.GetEndPlace();
                            if (end > 0)
                            {
                                unitSeconds /= end;
                            }
                        }

                        // 送信
                        sender(tlTable, unitSeconds, edgeWeightZero, edgeWeightHeld);
                    });
        }

        /// <summary>
        /// MorphWeightsSendCommand を実行する。
        /// </summary>
        /// <param name="param">LipId 文字列。</param>
        private void ExecuteMorphWeightsSendCommand(object param)
        {
            var sender = this.MorphWeightsSender;
            if (sender == null || param == null)
            {
                return;
            }

            var presets = this.EditViewModel.Presets;
            var index = this.EditViewModel.SelectedPresetIndex;
            if (index < 0 || index >= presets.Count)
            {
                return;
            }
            var infoSet = presets[index].Value;

            LipId lipId;
            if (!Enum.TryParse(param.ToString(), true, out lipId))
            {
                return;
            }

            sender(infoSet[lipId].MorphWeights);
        }

        /// <summary>
        /// 自動命名保存先ディレクトリを選択する。
        /// </summary>
        /// <returns></returns>
        private bool SelectAutoNamingDirectory()
        {
            var shower = this.SelectAutoNamingDirectoryDialogShower;
            if (shower == null)
            {
                return false;
            }

            // ダイアログ表示
            var path = shower(this.AutoNamingDirectoryPath);
            if (path == null)
            {
                return false;
            }

            // パス更新
            this.AutoNamingDirectoryPath = Path.GetFullPath(path);

            return true;
        }

        /// <summary>
        /// 自動命名保存先ディレクトリを用意する。
        /// </summary>
        /// <returns>用意に成功したならば true 。そうでなければ false 。</returns>
        private bool PrepareAutoNamingDirectory()
        {
            while (true)
            {
                var dirPath = this.AutoNamingDirectoryPath;

                if (Directory.Exists(dirPath))
                {
                    break;
                }

                try
                {
                    Directory.CreateDirectory(dirPath);
                    break;
                }
                catch
                {
                    var ok =
                        this.ShowOkCancelDialog(
                            dirPath + Environment.NewLine + Environment.NewLine +
                            @"保存先フォルダーを作成できません。" +
                            Environment.NewLine +
                            @"保存先フォルダーを変更しますか？");
                    if (!ok || !this.SelectAutoNamingDirectory())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 自動命名保存ファイルパスを決定する。
        /// </summary>
        /// <returns>自動命名保存ファイルパス。</returns>
        private string DecideAutoNamingFilePath()
        {
            // ファイル名構成要素を用意
            var text = MakeAutoNamingString(this.EditViewModel.Text);
            var kana = MakeAutoNamingString(this.EditViewModel.LipKana);
            var now = DateTime.Now;
            var date = now.ToString("yyMMdd");
            var time = now.ToString("hhmmss");

            // ベースファイルパス決定
            string basePath = kana;
            switch (this.AutoNamingFormat)
            {
            case AutoNamingFormat.Text:
                basePath = text;
                break;
            case AutoNamingFormat.TimeText:
                basePath = string.Join("_", time, text);
                break;
            case AutoNamingFormat.DateTimeText:
                basePath = string.Join("_", date, time, text);
                break;
            case AutoNamingFormat.Kana:
                basePath = kana;
                break;
            case AutoNamingFormat.TimeKana:
                basePath = string.Join("_", time, kana);
                break;
            case AutoNamingFormat.DateTimeKana:
                basePath = string.Join("_", date, time, kana);
                break;
            case AutoNamingFormat.DateTime:
                basePath = string.Join("_", date, time);
                break;
            }
            basePath = Path.Combine(this.AutoNamingDirectoryPath, basePath);

            // 拡張子決定
            var info = GetFileFormatInfo(this.ActiveFileFormat);
            var ext = "." + info.Extension;

            // 既存のファイルを避けてファイルパス決定
            string path = basePath + ext;
            for (int i = 1; File.Exists(path); ++i)
            {
                path = basePath + "[" + i + "]" + ext;
            }

            return path;
        }

        /// <summary>
        /// ファイルを新規保存する。
        /// </summary>
        /// <param name="filePath">
        /// 保存先ファイルパス。実際に使われたパスが設定される。
        /// </param>
        /// <param name="fileFormat">ファイルフォーマット。</param>
        /// <param name="overwritePrompt">
        /// 上書き時に確認ダイアログを表示するならば true 。
        /// </param>
        /// <returns>保存できたならば true 。そうでなければ false 。</returns>
        private bool SaveFile(
            ref string filePath,
            MotionFileFormat fileFormat,
            bool overwritePrompt)
        {
            var info = GetFileFormatInfo(fileFormat);

            var fps = (info.Fps < 0) ? this.EditViewModel.Fps : info.Fps;
            var keyFrames = this.EditViewModel.MakeKeyFrameList(fps, 0);
            var writer = info.WriterMaker(fps);

            // パス作成
            filePath = Path.GetFullPath(filePath);
            if (Path.GetExtension(filePath).ToLower() != "." + info.Extension)
            {
                // 拡張子付与
                filePath = filePath.TrimEnd('.') + "." + info.Extension;
            }

            // 上書き確認
            if (overwritePrompt && File.Exists(filePath))
            {
                var ok =
                    this.ShowOkCancelDialog(
                        filePath + Environment.NewLine + Environment.NewLine +
                        @"既にファイルが存在します。上書きしますか？");
                if (!ok)
                {
                    return false;
                }
            }

            string errorMessage = null;
            bool withText = false;
            try
            {
                // 親ディレクトリ作成
                var dirPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                // ファイル作成
                using (var fs = File.Create(filePath))
                {
                    // 書き出し
                    writer.Write(fs, keyFrames);
                }

                // テキストを保存
                if (this.IsSavingWithText)
                {
                    var text = this.EditViewModel.Text;
                    if (text != "")
                    {
                        text += Environment.NewLine;
                        text += @"--------------------" + Environment.NewLine;
                    }
                    text += this.EditViewModel.LipKana + Environment.NewLine;

                    withText = this.SaveTextFile(filePath, text);
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
                errorMessage = @"保存先フォルダーを作成できません。";
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

#if DEBUG
            // エラー表示テスト用
            //errorMessage = @"エラーテストです。";
#endif // DEBUG

            // エラー有無で切り分けて結果作成
            if (errorMessage == null)
            {
                var lastFrame = keyFrames.Any() ? keyFrames.Max(f => f.Frame) : 0;
                var text =
                    @"保存成功 : 長さ " + lastFrame + @" フレーム (" + fps + @" fps.";
                if (fps != this.EditViewModel.Fps)
                {
                    text += @" 換算で作成";
                }
                text += @")";
                if (withText)
                {
                    text += " with 入力文";
                }

                this.LastSaveResult = new FileSaveResult(true, filePath, text);
            }
            else
            {
                var text = @"保存失敗 : " + errorMessage;
                this.LastSaveResult = new FileSaveResult(false, filePath, text);
            }

            return (this.LastSaveResult.IsSucceeded == true);
        }

        /// <summary>
        /// 入力文テキストファイルを新規保存する。
        /// </summary>
        /// <param name="motionFilePath">モーションファイルパス。</param>
        /// <param name="text">入力文。</param>
        /// <returns>保存できたならば true 。そうでなければ false 。</returns>
        private bool SaveTextFile(string motionFilePath, string text)
        {
            // テキストファイルパス決定
            var path = motionFilePath + ".txt";
            for (int i = 0; File.Exists(path); ++i)
            {
                path = motionFilePath + "(" + i + ").txt";
            }

            // Unicode(UTF-16LE)で保存
            try
            {
                File.WriteAllText(path, text, Encoding.Unicode);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// エラーダイアログを表示する。
        /// </summary>
        /// <param name="message">表示するメッセージ。</param>
        /// <param name="icon">表示するアイコン。</param>
        private void ShowErrorDialog(string message, dlg.MessageBox.Icon icon)
        {
            var shower = this.MessageBoxShower ?? dlg.MessageBox.Show;
            shower(message, @"エラー", dlg.MessageBox.Button.Ok, icon);
        }

        /// <summary>
        /// OK/キャンセルダイアログを表示する。
        /// </summary>
        /// <param name="message">表示するメッセージ。</param>
        /// <returns>OKが選択されたならば true 。</returns>
        private bool ShowOkCancelDialog(string message)
        {
            var shower = this.MessageBoxShower ?? dlg.MessageBox.Show;
            var result =
                shower(
                    message,
                    @"確認",
                    dlg.MessageBox.Button.OkCancel,
                    dlg.MessageBox.Icon.Information);

            return (result == dlg.MessageBox.Result.Ok);
        }
    }
}
