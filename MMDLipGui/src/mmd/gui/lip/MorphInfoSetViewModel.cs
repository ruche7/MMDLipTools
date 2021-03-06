﻿using System;
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
                        p => (p is int) && (int)p >= 0);
                this.UpCommand =
                    new DelegateCommand(
                        this.ExecuteUpCommand,
                        p => (p is int) && (int)p >= 1);
                this.DownCommand =
                    new DelegateCommand(
                        this.ExecuteDownCommand,
                        p =>
                            (p is int) &&
                            (int)p >= 0 &&
                            (int)p + 1 < this.MorphWeights.Count);
            }

            /// <summary>
            /// 口形状IDを取得する。
            /// </summary>
            public LipId Id { get; }

            /// <summary>
            /// モーフ名とそのウェイト値のリストを取得する。
            /// </summary>
            public MorphWeightDataList MorphWeights => this.Info.MorphWeights;

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
            /// モーフ情報を取得する。
            /// </summary>
            private MorphInfo Info { get; }

            /// <summary>
            /// 空のモーフウェイト情報を新規追加するコマンドを取得する。
            /// </summary>
            public ICommand AddCommand { get; }

            /// <summary>
            /// 選択中のモーフウェイト情報を削除するコマンドを取得する。
            /// </summary>
            public ICommand DeleteCommand { get; }

            /// <summary>
            /// 選択中のモーフウェイト情報を上へ移動するコマンドを取得する。
            /// </summary>
            public ICommand UpCommand { get; }

            /// <summary>
            /// 選択中のモーフウェイト情報を下へ移動するコマンドを取得する。
            /// </summary>
            public ICommand DownCommand { get; }

            /// <summary>
            /// AddCommand を実行する。
            /// </summary>
            private void ExecuteAddCommand(object param)
            {
                // 追加
                this.MorphWeights.Add(new MorphWeightData());
            }

            /// <summary>
            /// DeleteCommand を実行する。
            /// </summary>
            /// <param name="param">選択中のモーフウェイト情報インデックス。</param>
            private void ExecuteDeleteCommand(object param)
            {
                var index = (param is int) ? (int)param : -1;
                if (index < 0 || index >= this.MorphWeights.Count)
                {
                    return;
                }

                // 削除
                this.MorphWeights.RemoveAt(index);
            }

            /// <summary>
            /// UpCommand を実行する。
            /// </summary>
            /// <param name="param">選択中のモーフウェイト情報インデックス。</param>
            private void ExecuteUpCommand(object param)
            {
                var index = (param is int) ? (int)param : -1;
                if (index <= 0 || index >= this.MorphWeights.Count)
                {
                    return;
                }

                this.MorphWeights.Move(index, index - 1);
            }

            /// <summary>
            /// DownCommand を実行する。
            /// </summary>
            /// <param name="param">選択中のモーフウェイト情報インデックス。</param>
            private void ExecuteDownCommand(object param)
            {
                var index = (param is int) ? (int)param : -1;
                if (index < 0 || index + 1 >= this.MorphWeights.Count)
                {
                    return;
                }

                this.MorphWeights.Move(index, index + 1);
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
                throw new ArgumentNullException(nameof(src));
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
        public MorphInfoSet Source { get; }

        /// <summary>
        /// 口形状別モーフ情報アイテムコレクションを取得する。
        /// </summary>
        public ReadOnlyObservableCollection<Item> Items { get; }

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
                    this.NotifyPropertyChanged(nameof(SelectedItemIndex));
                }
            }
        }
        private int _selectedItemIndex = 0;
    }
}
