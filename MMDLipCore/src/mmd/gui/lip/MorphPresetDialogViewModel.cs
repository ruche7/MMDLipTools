using System;
using System.Windows;
using System.Windows.Input;
using ruche.wpf.viewModel;

namespace ruche.mmd.gui.lip
{
    /// <summary>
    /// MorphPresetDialog の ViewModel クラス。
    /// </summary>
    internal class MorphPresetDialogViewModel : ViewModelBase
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
        /// コンストラクタ。
        /// </summary>
        public MorphPresetDialogViewModel() : base()
        {
            // モーフ情報セット ViewModel 初期化
            this.EditingMorphInfoSet = new MorphInfoSetViewModel();

            // コマンド作成
            this.PresetUpCommand =
                new DelegateCommand(
                    this.ExecutePresetUpCommand,
                    _ => this.SelectedMorphPresetIndex >= 1);
            this.PresetDownCommand =
                new DelegateCommand(
                    this.ExecutePresetDownCommand,
                    _ =>
                        this.SelectedMorphPresetIndex >= 0 &&
                        this.SelectedMorphPresetIndex + 1 < this.MorphPresets.Count);
            this.EditCommand =
                new DelegateCommand(
                    this.ExecuteEditCommand,
                    _ => this.SelectedMorphPresetIndex >= 0);
            this.ApplyCommand = new DelegateCommand(this.ExecuteApplyCommand);
            this.DeleteCommand =
                new DelegateCommand(
                    this.ExecuteDeleteCommand,
                    _ => this.SelectedMorphPresetIndex >= 0);
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
        /// 口パクモーフプリセットリストを取得または設定する。
        /// </summary>
        public MorphPresetList MorphPresets
        {
            get { return _morphPresets; }
            set
            {
                var v = value ?? (new MorphPresetList());
                if (v != _morphPresets)
                {
                    _morphPresets = v;
                    this.NotifyPropertyChanged("MorphPresets");

                    // 選択中インデックスも必要に応じて更新
                    if (this.SelectedMorphPresetIndex >= v.Count)
                    {
                        this.SelectedMorphPresetIndex = v.Count - 1;
                    }
                }
            }
        }
        private MorphPresetList _morphPresets =
            new MorphPresetList(new[] { new MorphPreset() });

        /// <summary>
        /// 現在選択中の口パクモーフプリセットのインデックスを取得または設定する。
        /// </summary>
        public int SelectedMorphPresetIndex
        {
            get { return _selectedMorphPresetIndex; }
            set
            {
                var v = Math.Min(Math.Max(-1, value), this.MorphPresets.Count - 1);
                if (v != _selectedMorphPresetIndex)
                {
                    _selectedMorphPresetIndex = v;
                    this.NotifyPropertyChanged("SelectedMorphPresetIndex");
                }
            }
        }
        private int _selectedMorphPresetIndex = -1;

        /// <summary>
        /// 編集中のプリセット名を取得または設定する。
        /// </summary>
        public string EditingPresetName
        {
            get { return _editingPresetName; }
            set
            {
                var v = value ?? "";
                if (v != _editingPresetName)
                {
                    _editingPresetName = v;
                    this.NotifyPropertyChanged("EditingPresetName");
                }
            }
        }
        private string _editingPresetName = "";

        /// <summary>
        /// 編集中のモーフ情報セット ViewModel を取得または設定する。
        /// </summary>
        public MorphInfoSetViewModel EditingMorphInfoSet
        {
            get { return _editingMorphInfoSet; }
            set
            {
                var v = value ?? (new MorphInfoSetViewModel());
                if (v != _editingMorphInfoSet)
                {
                    _editingMorphInfoSet = v;
                    this.NotifyPropertyChanged("EditingMorphInfoSet");
                }
            }
        }
        private MorphInfoSetViewModel _editingMorphInfoSet =
                new MorphInfoSetViewModel();

        /// <summary>
        /// 選択中のプリセットを上へ移動するコマンドを取得する。
        /// </summary>
        public ICommand PresetUpCommand { get; private set; }

        /// <summary>
        /// 選択中のプリセットを下へ移動するコマンドを取得する。
        /// </summary>
        public ICommand PresetDownCommand { get; private set; }

        /// <summary>
        /// 選択中のプリセットを編集開始するコマンドを取得する。
        /// </summary>
        public ICommand EditCommand { get; private set; }

        /// <summary>
        /// 編集中のプリセットをプリセットリストへ適用するコマンドを取得する。
        /// </summary>
        public ICommand ApplyCommand { get; private set; }

        /// <summary>
        /// 選択中のプリセットを削除するコマンドを取得する。
        /// </summary>
        public ICommand DeleteCommand { get; private set; }

        /// <summary>
        /// PresetUpCommand を実行する。
        /// </summary>
        private void ExecutePresetUpCommand(object param)
        {
            var index = this.SelectedMorphPresetIndex;
            if (index <= 0 || index >= this.MorphPresets.Count)
            {
                return;
            }

            this.MorphPresets.Move(index, index - 1);
        }

        /// <summary>
        /// PresetDownCommand を実行する。
        /// </summary>
        private void ExecutePresetDownCommand(object param)
        {
            var index = this.SelectedMorphPresetIndex;
            if (index < 0 || index + 1 >= this.MorphPresets.Count)
            {
                return;
            }

            this.MorphPresets.Move(index, index + 1);
        }

        /// <summary>
        /// EditCommand を実行する。
        /// </summary>
        private void ExecuteEditCommand(object param)
        {
            var index = this.SelectedMorphPresetIndex;
            if (index < 0 || index >= this.MorphPresets.Count)
            {
                return;
            }

            var preset = this.MorphPresets[index];

            // 編集用プロパティへコピー
            this.EditingPresetName = preset.Name;
            this.EditingMorphInfoSet =
                new MorphInfoSetViewModel(preset.Value.Clone());
        }

        /// <summary>
        /// ApplyCommand を実行する。
        /// </summary>
        private void ExecuteApplyCommand(object param)
        {
            if (!MorphPreset.IsValidName(this.EditingPresetName))
            {
                ShowErrorDialog(
                    @"プリセット名が不正です。" + Environment.NewLine +
                    @"空の名前や空白文字のみの名前は付けられません。",
                    MessageBoxImage.Warning);
                return;
            }

            var preset =
                new MorphPreset(
                    this.EditingPresetName,
                    this.EditingMorphInfoSet.Source.Clone());

            // 同名のプリセットを検索
            int sameNameIndex = this.MorphPresets.FindIndex(preset.Name);
            if (sameNameIndex < 0)
            {
                // 新規追加
                this.MorphPresets.Add(preset);

                // 追加先を選択
                this.SelectedMorphPresetIndex = this.MorphPresets.Count - 1;
            }
            else
            {
                var yes =
                    ShowOkCancelDialog(
                        @"プリセット " + preset.Name + @" は既に存在します。" +
                        Environment.NewLine + @"置き換えますか？");
                if (yes)
                {
                    // 置換
                    this.MorphPresets[sameNameIndex] = preset;

                    // 置換先を選択
                    this.SelectedMorphPresetIndex = sameNameIndex;
                }
            }
        }

        /// <summary>
        /// DeleteCommand を実行する。
        /// </summary>
        private void ExecuteDeleteCommand(object param)
        {
            var index = this.SelectedMorphPresetIndex;
            if (index < 0 || index >= this.MorphPresets.Count)
            {
                return;
            }

            bool yes =
                ShowOkCancelDialog(
                    @"プリセット " + this.MorphPresets[index].Name +
                    @" を削除します。" + Environment.NewLine + @"よろしいですか？");
            if (yes)
            {
                // 削除
                this.MorphPresets.RemoveAt(index);

                // 選択インデックスを保持
                this.SelectedMorphPresetIndex = index;
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
