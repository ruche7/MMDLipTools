using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ruche.wpf.viewModel;

namespace ruche.mmd.gui.lip
{
    /// <summary>
    /// MorphTimelineTableControl の ViewModel クラス。
    /// </summary>
    internal class MorphTimelineTableControlViewModel : ViewModelBase
    {
        /// <summary>
        /// 口パクモーフプリセット編集ダイアログの表示処理を提供するデリゲート。
        /// </summary>
        /// <param name="morphPresets">
        /// 編集対象の口パクモーフプリセットリスト。
        /// </param>
        /// <returns>
        /// 編集結果の口パクモーフプリセットリスト。
        /// 編集がキャンセルされた場合は null 。
        /// </returns>
        public delegate MorphPresetList MorphPresetDialogDelegate(
            MorphPresetList morphPresets);

        /// <summary>
        /// 既定の口パクモーフプリセットリスト。
        /// </summary>
        private static readonly MorphPresetList DefaultMorphPresets =
            new MorphPresetList(new[] { new MorphPreset() });

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MorphTimelineTableControlViewModel() : base()
        {
        }

        /// <summary>
        /// 口パクモーフプリセットリストを取得または設定する。
        /// </summary>
        public MorphPresetList MorphPresets
        {
            get { return _morphPresets; }
            set
            {
                var v =
                    (value != null && value.Count >= 1) ?
                        value : DefaultMorphPresets.Clone();
                if (!v.SequenceEqual(_morphPresets))
                {
                    // 現在選択中のプリセット名を取得
                    string currentName = null;
                    if (
                        this.SelectedMorphPresetIndex >= 0 &&
                        this.SelectedMorphPresetIndex < _morphPresets.Count)
                    {
                        currentName =
                            _morphPresets[this.SelectedMorphPresetIndex].Name;
                    }

                    // プリセットリスト置換
                    _morphPresets = v;

                    // プリセットインデックスを更新
                    bool indexChanged = false;
                    var sameNameIndex = v.FindIndex(currentName);
                    if (
                        sameNameIndex > 0 &&
                        sameNameIndex != _selectedMorphPresetIndex)
                    {
                        // 同名プリセットがあればそちらを優先
                        _selectedMorphPresetIndex = sameNameIndex;
                        indexChanged = true;
                    }
                    else if (_selectedMorphPresetIndex >= v.Count)
                    {
                        // 範囲外なら 0 に
                        _selectedMorphPresetIndex = 0;
                        indexChanged = true;
                    }

                    this.NotifyPropertyChanged("MorphPresets");
                    if (indexChanged)
                    {
                        this.NotifyPropertyChanged("SelectedMorphPresetIndex");
                    }
                }
            }
        }
        private MorphPresetList _morphPresets = DefaultMorphPresets.Clone();

        /// <summary>
        /// 現在選択中の口パクモーフプリセットのインデックスを取得または設定する。
        /// </summary>
        public int SelectedMorphPresetIndex
        {
            get { return _selectedMorphPresetIndex; }
            set
            {
                var v =
                    (value < this.MorphPresets.Count) ?
                        value : (this.MorphPresets.Count - 1);
                if (v != _selectedMorphPresetIndex)
                {
                    _selectedMorphPresetIndex = v;
                    this.NotifyPropertyChanged("SelectedMorphPresetIndex");
                }
            }
        }
        private int _selectedMorphPresetIndex = 0;

        /// <summary>
        /// 口パクモーフプリセット編集ダイアログの表示処理を行うデリゲートを
        /// 取得または設定する。
        /// </summary>
        public MorphPresetDialogDelegate MorphPresetDialogShower
        {
            get { return _morphPresetDialogShower; }
            set
            {
                if (value != _morphPresetDialogShower)
                {
                    bool oldEditable = this.IsMorphPresetsEditable;

                    _morphPresetDialogShower = value;
                    this.NotifyPropertyChanged("MorphPresetDialogShower");

                    if (this.IsMorphPresetsEditable != oldEditable)
                    {
                        this.NotifyPropertyChanged("IsMorphPresetsEditable");
                    }
                }
            }
        }
        private MorphPresetDialogDelegate _morphPresetDialogShower;

        /// <summary>
        /// 口パクモーフプリセットリストの編集が可能であるか否かを取得する。
        /// </summary>
        /// <remarks>
        /// MorphPresetDialogShower プロパティに有効なデリゲートを設定することで
        /// true を返すようになる。
        /// </remarks>
        public bool IsMorphPresetsEditable
        {
            get { return (this.MorphPresetDialogShower != null); }
        }
    }
}
