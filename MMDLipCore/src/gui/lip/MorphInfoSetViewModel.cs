using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ruche.mmd.morph;
using ruche.mmd.morph.lip;
using ruche.wpf.viewModel;

namespace ruche.mmd.gui.lip
{
    /// <summary>
    /// MorphInfoSet の ViewModel クラス。
    /// </summary>
    public class MorphInfoSetViewModel : ViewModelBase
    {
        /// <summary>
        /// 口形状別のモーフ情報を保持する ViewModel クラス。
        /// </summary>
        public class Item : ViewModelBase
        {
            /// <summary>
            /// コンストラクタ。
            /// </summary>
            /// <param name="id">口形状ID。</param>
            /// <param name="info">モーフ情報。</param>
            public Item(LipId id, MorphInfo info)
            {
                this.Id = id;
                this.Info = info ?? (new MorphInfo());

                // コマンド作成
                this.AddCommand = new DelegateCommand(this.ExecuteAddCommand);
                this.DeleteCommand =
                    new DelegateCommand(
                        this.ExecuteDeleteCommand,
                        _ => this.SelectedMorphWeightIndex >= 0);
                this.UpCommand =
                    new DelegateCommand(
                        this.ExecuteUpCommand,
                        _ => this.SelectedMorphWeightIndex >= 1);
                this.DownCommand =
                    new DelegateCommand(
                        this.ExecuteDownCommand,
                        _ =>
                            this.SelectedMorphWeightIndex >= 0 &&
                            this.SelectedMorphWeightIndex + 1 <
                                this.Info.MorphWeights.Count);
            }

            /// <summary>
            /// 口形状IDを取得する。
            /// </summary>
            public LipId Id { get; private set; }

            /// <summary>
            /// モーフ情報を取得する。
            /// </summary>
            public MorphInfo Info { get; private set; }

            /// <summary>
            /// アクセス名を取得する。
            /// </summary>
            public string AccessName
            {
                get
                {
                    switch (this.Id)
                    {
                    case LipId.Closed: return "閉(_6)";
                    case LipId.A: return "あ(_1)";
                    case LipId.I: return "い(_2)";
                    case LipId.U: return "う(_3)";
                    case LipId.E: return "え(_4)";
                    case LipId.O: return "お(_5)";
                    }
                    return "";
                }
            }

            /// <summary>
            /// 選択中のモーフウェイト情報インデックスを取得または設定する。
            /// </summary>
            public int SelectedMorphWeightIndex
            {
                get { return _selectedMorphWeightIndex; }
                set
                {
                    var v =
                        Math.Min(
                            Math.Max(-1, value),
                            this.Info.MorphWeights.Count - 1);
                    if (v != _selectedMorphWeightIndex)
                    {
                        _selectedMorphWeightIndex = v;
                        this.NotifyPropertyChanged("SelectedMorphWeightIndex");
                    }
                }
            }
            private int _selectedMorphWeightIndex = -1;

            /// <summary>
            /// 空のモーフウェイト情報を新規追加するコマンドを取得する。
            /// </summary>
            public ICommand AddCommand { get; private set; }

            /// <summary>
            /// 選択中のモーフウェイト情報を削除するコマンドを取得する。
            /// </summary>
            public ICommand DeleteCommand { get; private set; }

            /// <summary>
            /// 選択中のモーフウェイト情報を上へ移動するコマンドを取得する。
            /// </summary>
            public ICommand UpCommand { get; private set; }

            /// <summary>
            /// 選択中のモーフウェイト情報を下へ移動するコマンドを取得する。
            /// </summary>
            public ICommand DownCommand { get; private set; }

            /// <summary>
            /// AddCommand を実行する。
            /// </summary>
            private void ExecuteAddCommand(object param)
            {
                // 追加
                this.Info.MorphWeights.Add(new MorphWeightData());

                // 追加された項目を選択
                this.SelectedMorphWeightIndex = this.Info.MorphWeights.Count - 1;
            }

            /// <summary>
            /// DeleteCommand を実行する。
            /// </summary>
            private void ExecuteDeleteCommand(object param)
            {
                var index = this.SelectedMorphWeightIndex;
                if (index < 0 || index >= this.Info.MorphWeights.Count)
                {
                    return;
                }

                // 削除
                this.Info.MorphWeights.RemoveAt(index);

                // 選択インデックスを保持
                this.SelectedMorphWeightIndex = index;
            }

            /// <summary>
            /// UpCommand を実行する。
            /// </summary>
            private void ExecuteUpCommand(object param)
            {
                var index = this.SelectedMorphWeightIndex;
                if (index <= 0 || index >= this.Info.MorphWeights.Count)
                {
                    return;
                }

                this.Info.MorphWeights.Move(index, index - 1);
            }

            /// <summary>
            /// DownCommand を実行する。
            /// </summary>
            private void ExecuteDownCommand(object param)
            {
                var index = this.SelectedMorphWeightIndex;
                if (index < 0 || index + 1 >= this.Info.MorphWeights.Count)
                {
                    return;
                }

                this.Info.MorphWeights.Move(index, index + 1);
            }
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MorphInfoSetViewModel() : this(new MorphInfoSet())
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="src">編集対象の MorphInfoSet 。</param>
        public MorphInfoSetViewModel(MorphInfoSet src)
        {
            if (src == null)
            {
                throw new ArgumentNullException("src");
            }

            this.Source = src;
            this.Items =
                new ReadOnlyObservableCollection<Item>(
                    new ObservableCollection<Item>
                    {
                        new Item(LipId.A, src.A),
                        new Item(LipId.I, src.I),
                        new Item(LipId.U, src.U),
                        new Item(LipId.E, src.E),
                        new Item(LipId.O, src.O),
                        new Item(LipId.Closed, src.Closed),
                    });
        }

        /// <summary>
        /// 編集対象の MorphInfoSet を取得する。
        /// </summary>
        public MorphInfoSet Source { get; private set; }

        /// <summary>
        /// 口形状別モーフ情報アイテムコレクションを取得する。
        /// </summary>
        public ReadOnlyObservableCollection<Item> Items { get; private set; }

        /// <summary>
        /// 現在選択中の口形状別モーフ情報アイテムのインデックスを取得または設定する。
        /// </summary>
        public int SelectedItemIndex
        {
            get { return _selectedItemIndex; }
            set
            {
                var v = (value < this.Items.Count) ? value : (this.Items.Count - 1);
                if (v != _selectedItemIndex)
                {
                    _selectedItemIndex = v;
                    this.NotifyPropertyChanged("SelectedItemIndex");
                }
            }
        }
        private int _selectedItemIndex = 0;
    }
}
