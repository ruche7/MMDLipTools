using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ruche.mmd.morph;
using ruche.wpf.viewModel;
using dlg = ruche.wpf.dialogs;

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
        /// <param name="icon">アイコン種別。</param>
        /// <returns>選択結果。</returns>
        public delegate dlg.MessageBox.Result MessageBoxDelegate(
            string message,
            string caption,
            dlg.MessageBox.Button button,
            dlg.MessageBox.Icon icon);

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MorphPresetDialogViewModel()
        {
            // モーフ情報セット ViewModel 初期化
            this.EditingMorphInfoSet = new MorphInfoSetViewModel();

            // コマンド作成
            this.PresetUpCommand =
                new DelegateCommand(
                    this.ExecutePresetUpCommand,
                    p => (p is int) && (int)p >= 1);
            this.PresetDownCommand =
                new DelegateCommand(
                    this.ExecutePresetDownCommand,
                    p =>
                        (p is int) &&
                        (int)p >= 0 &&
                        (int)p + 1 < this.Presets.Count);
            this.EditCommand =
                new DelegateCommand(
                    this.ExecuteEditCommand,
                    p => (p is int) && (int)p >= 0);
            this.ApplyCommand = new DelegateCommand(this.ExecuteApplyCommand);
            this.DeleteCommand =
                new DelegateCommand(
                    this.ExecuteDeleteCommand,
                    p => (p is int) && (int)p >= 0);
            this.MorphWeightsSendCommand =
                new DelegateCommand(
                    this.ExecuteMorphWeightsSendCommand,
                    _ =>
                        this.MorphWeightsSender != null &&
                        this.EditingMorphInfoSet.SelectedItemIndex >= 0);
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
                    this.NotifyPropertyChanged(nameof(MessageBoxShower));
                }
            }
        }
        private MessageBoxDelegate _messageBoxShower = null;

        /// <summary>
        /// 口パクモーフプリセットリストを取得または設定する。
        /// </summary>
        public MorphPresetList Presets
        {
            get { return _presets; }
            set
            {
                var v = value ?? (new MorphPresetList());
                if (v != _presets)
                {
                    _presets = v;
                    this.NotifyPropertyChanged(nameof(Presets));
                }
            }
        }
        private MorphPresetList _presets =
            new MorphPresetList(new[] { new MorphPreset() });

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
                    this.NotifyPropertyChanged(nameof(EditingPresetName));
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
                    this.NotifyPropertyChanged(nameof(EditingMorphInfoSet));
                }
            }
        }
        private MorphInfoSetViewModel _editingMorphInfoSet = null;

        /// <summary>
        /// モーフウェイトリストの送信を行うデリゲートを取得または設定する。
        /// </summary>
        public Action<IEnumerable<MorphWeightData>> MorphWeightsSender
        {
            get { return _morphWeightsSender; }
            set
            {
                if (value != _morphWeightsSender)
                {
                    bool oldEnabled = this.IsMorphWeightsSenderEnabled;

                    _morphWeightsSender = value;
                    this.NotifyPropertyChanged(nameof(MorphWeightsSender));

                    if (this.IsMorphWeightsSenderEnabled != oldEnabled)
                    {
                        this.NotifyPropertyChanged(
                            nameof(IsMorphWeightsSenderEnabled));
                    }
                }
            }
        }
        private Action<IEnumerable<MorphWeightData>> _morphWeightsSender = null;

        /// <summary>
        /// MorphWeightsSender に有効な値が設定されているか否かを取得する。
        /// </summary>
        public bool IsMorphWeightsSenderEnabled => (this.MorphWeightsSender != null);

        /// <summary>
        /// 選択中のプリセットを上へ移動するコマンドを取得する。
        /// </summary>
        public ICommand PresetUpCommand { get; }

        /// <summary>
        /// 選択中のプリセットを下へ移動するコマンドを取得する。
        /// </summary>
        public ICommand PresetDownCommand { get; }

        /// <summary>
        /// 選択中のプリセットを編集開始するコマンドを取得する。
        /// </summary>
        public ICommand EditCommand { get; }

        /// <summary>
        /// 編集中のプリセットをプリセットリストへ適用するコマンドを取得する。
        /// </summary>
        public ICommand ApplyCommand { get; }

        /// <summary>
        /// 選択中のプリセットを削除するコマンドを取得する。
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// 編集中のモーフウェイトリストを送信するコマンドを取得する。
        /// </summary>
        public ICommand MorphWeightsSendCommand { get; }

        /// <summary>
        /// PresetUpCommand を実行する。
        /// </summary>
        /// <param name="param">選択中のプリセットインデックス。</param>
        private void ExecutePresetUpCommand(object param)
        {
            var index = (param is int) ? (int)param : -1;
            if (index <= 0 || index >= this.Presets.Count)
            {
                return;
            }

            this.Presets.Move(index, index - 1);
        }

        /// <summary>
        /// PresetDownCommand を実行する。
        /// </summary>
        /// <param name="param">選択中のプリセットインデックス。</param>
        private void ExecutePresetDownCommand(object param)
        {
            var index = (param is int) ? (int)param : -1;
            if (index < 0 || index + 1 >= this.Presets.Count)
            {
                return;
            }

            this.Presets.Move(index, index + 1);
        }

        /// <summary>
        /// EditCommand を実行する。
        /// </summary>
        /// <param name="param">選択中のプリセットインデックス。</param>
        private void ExecuteEditCommand(object param)
        {
            var index = (param is int) ? (int)param : -1;
            if (index < 0 || index >= this.Presets.Count)
            {
                return;
            }

            var preset = this.Presets[index];

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
                    dlg.MessageBox.Icon.Warning);
                return;
            }

            var preset =
                new MorphPreset(
                    this.EditingPresetName,
                    this.EditingMorphInfoSet.Source.Clone());

            // 同名のプリセットを検索
            int sameNameIndex = this.Presets.FindIndex(preset.Name);
            if (sameNameIndex < 0)
            {
                // 新規追加
                this.Presets.Add(preset);
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
                    this.Presets[sameNameIndex] = preset;
                }
            }
        }

        /// <summary>
        /// DeleteCommand を実行する。
        /// </summary>
        /// <param name="param">選択中のプリセットインデックス。</param>
        private void ExecuteDeleteCommand(object param)
        {
            var index = (param is int) ? (int)param : -1;
            if (index < 0 || index >= this.Presets.Count)
            {
                return;
            }

            bool yes =
                ShowOkCancelDialog(
                    @"プリセット " + this.Presets[index].Name +
                    @" を削除します。" + Environment.NewLine + @"よろしいですか？");
            if (yes)
            {
                // 削除
                this.Presets.RemoveAt(index);
            }
        }

        /// <summary>
        /// MorphWeightsSendCommand を実行する。
        /// </summary>
        private void ExecuteMorphWeightsSendCommand(object param)
        {
            var sender = this.MorphWeightsSender;
            if (sender == null)
            {
                return;
            }

            var vm = this.EditingMorphInfoSet;
            var index = vm.SelectedItemIndex;
            if (index < 0 || index >= vm.Items.Count)
            {
                return;
            }

            // 対象口形状のモーフウェイトリスト取得
            var targetMorphWeights = vm.Items[index].MorphWeights;

            // 対象口形状に含まれないモーフをウェイト値 0 で作成
            var zeroMorphWeights =
                vm.Items
                    .SelectMany(i => i.MorphWeights)
                    .Select(mw => mw.MorphName)
                    .Distinct()
                    .Where(n => targetMorphWeights.All(mw => mw.MorphName != n))
                    .Select(n => new MorphWeightData { MorphName = n, Weight = 0 });

            // 連結して送信
            sender(targetMorphWeights.Concat(zeroMorphWeights));
        }

        /// <summary>
        /// OK/キャンセルダイアログを表示する。
        /// </summary>
        /// <param name="message">表示するメッセージ。</param>
        /// <returns>Yesが選択されたならば true 。</returns>
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

        /// <summary>
        /// エラーダイアログを表示する。
        /// </summary>
        /// <param name="message">エラーメッセージ。</param>
        /// <param name="image">アイコン種別。</param>
        private void ShowErrorDialog(string message, dlg.MessageBox.Icon icon)
        {
            var shower = this.MessageBoxShower ?? dlg.MessageBox.Show;
            shower(message, @"エラー", dlg.MessageBox.Button.Ok, icon);
        }
    }
}
