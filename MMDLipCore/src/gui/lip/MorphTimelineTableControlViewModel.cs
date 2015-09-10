using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ruche.mmd.morph.lip;
using ruche.mmd.morph.lip.converters;
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
        /// 読み仮名変換元の文字列を取得または設定する。
        /// </summary>
        public string Text
        {
            get { return _text; }
            set
            {
                var v = value ?? "";
                if (v != _text)
                {
                    _text = v;
                    this.NotifyPropertyChanged("Text");

                    // 自動変換開始
                    if (this.IsAutoLipKana)
                    {
                        this.StartLipKanaTask();
                    }
                }
            }
        }
        private string _text = "";

        /// <summary>
        /// 読み仮名への自動変換を行うか否かを取得または設定する。
        /// </summary>
        public bool IsAutoLipKana
        {
            get { return _autoLipKana; }
            set
            {
                if (value != _autoLipKana)
                {
                    _autoLipKana = value;
                    this.NotifyPropertyChanged("IsAutoLipKana");

                    // true に変更されたら自動変換開始
                    if (value)
                    {
                        this.StartLipKanaTask();
                    }
                }
            }
        }
        private bool _autoLipKana = false;

        /// <summary>
        /// カタカナの読み仮名文字列を取得または設定する。
        /// </summary>
        public string LipKana
        {
            get { return _lipKana; }
            set
            {
                var v = value ?? "";
                if (v != _lipKana)
                {
                    _lipKana = v;
                    this.NotifyPropertyChanged("LipKana");

                    // リップシンクユニットリスト更新開始
                    this.StartLipSyncUnitsTask();
                }
            }
        }
        private string _lipKana = "";

        /// <summary>
        /// リップシンクユニットリストを取得する。
        /// </summary>
        public ReadOnlyCollection<LipSyncUnit> LipSyncUnits
        {
            get { return _lipSyncUnits; }
            private set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _lipSyncUnits = value;
                this.NotifyPropertyChanged("LipSyncUnits");

                // 文字列表現値を作成
                this.LipSyncText = string.Join("", value);
            }
        }
        private ReadOnlyCollection<LipSyncUnit> _lipSyncUnits =
            (new List<LipSyncUnit>()).AsReadOnly();

        /// <summary>
        /// リップシンクユニットリストの文字列表現値を取得する。
        /// </summary>
        public string LipSyncText
        {
            get { return _lipSyncText; }
            private set
            {
                var v = value ?? "";
                if (v != _lipSyncText)
                {
                    _lipSyncText = v;
                    this.NotifyPropertyChanged("LipSyncText");
                }
            }
        }
        private string _lipSyncText = "";

        /// <summary>
        /// 口パクモーフプリセットリストを取得または設定する。
        /// </summary>
        public MorphPresetList MorphPresets
        {
            get { return _morphPresets; }
            set
            {
                var v = value ?? DefaultMorphPresets.Clone();

                // 空っぽならデフォルト値追加
                if (v.Count == 0)
                {
                    v.Add(new MorphPreset());
                }

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
                    this.NotifyPropertyChanged("MorphPresets");

                    // プリセットインデックスを更新
                    // 選択中のプリセットが削除された場合は 0 にする
                    var index = v.FindIndex(currentName);
                    this.SelectedMorphPresetIndex = Math.Max(0, index);
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

        /// <summary>
        /// 口パクの時間指定範囲種別を取得または設定する。
        /// </summary>
        public LipSpanRange SpanRange
        {
            get { return _spanRange; }
            set
            {
                if (!Enum.IsDefined(value.GetType(), value))
                {
                    throw new InvalidEnumArgumentException(
                        "value",
                        (int)value,
                        value.GetType());
                }

                if (value != _spanRange)
                {
                    _spanRange = value;
                    this.NotifyPropertyChanged("SpanRange");
                }
            }
        }
        private LipSpanRange _spanRange = LipSpanRange.Letter;

        /// <summary>
        /// 口パクの時間指定値を取得または設定する。
        /// </summary>
        public decimal SpanValue
        {
            get { return _spanValue; }
            set
            {
                var v =
                    Math.Min(Math.Max(this.MinSpanValue, value), this.MaxSpanValue);
                if (v != _spanValue)
                {
                    _spanValue = v;
                    this.NotifyPropertyChanged("SpanValue");
                }
            }
        }
        private decimal _spanValue = 1;

        /// <summary>
        /// 口パクの時間指定単位種別を取得または設定する。
        /// </summary>
        public LipSpanUnit SpanUnit
        {
            get { return _spanUnit; }
            set
            {
                if (!Enum.IsDefined(value.GetType(), value))
                {
                    throw new InvalidEnumArgumentException(
                        "value",
                        (int)value,
                        value.GetType());
                }

                if (value != _spanUnit)
                {
                    var oldValue = this.SpanValue;
                    var oldUnit = _spanUnit;

                    _spanUnit = value;
                    this.NotifyPropertyChanged("SpanUnit");

                    // 関連プロパティの更新通知
                    this.NotifyPropertyChanged("MinSpanValue");
                    this.NotifyPropertyChanged("MaxSpanValue");
                    this.NotifyPropertyChanged("SpanValueIncrement");

                    // 時間指定値を更新
                    this.SpanValue =
                        Math.Min(
                            Math.Max(
                                this.MinSpanValue,
                                this.CalcSpanValueByUnit(oldValue, oldUnit, value)),
                            this.MaxSpanValue);
                }
            }
        }
        private LipSpanUnit _spanUnit = LipSpanUnit.Frames;

        /// <summary>
        /// 口パクの時間指定値の最小許容値を取得する。
        /// </summary>
        public decimal MinSpanValue
        {
            get
            {
                switch (this.SpanUnit)
                {
                case LipSpanUnit.MilliSeconds: return 1;
                case LipSpanUnit.Seconds: return 0.001m;
                }
                return 1;
            }
        }

        /// <summary>
        /// 口パクの時間指定値の最大許容値を取得する。
        /// </summary>
        public decimal MaxSpanValue
        {
            get
            {
                switch (this.SpanUnit)
                {
                case LipSpanUnit.MilliSeconds: return 999999;
                case LipSpanUnit.Seconds: return 999.999m;
                }
                return 99999;
            }
        }

        /// <summary>
        /// 口パクの時間指定値の推奨インクリメント量を取得する。
        /// </summary>
        public decimal SpanValueIncrement
        {
            get
            {
                switch (this.SpanUnit)
                {
                case LipSpanUnit.MilliSeconds: return 100;
                case LipSpanUnit.Seconds: return 0.5m;
                }
                return 1;
            }
        }

        /// <summary>
        /// 実フレーム値への変換に用いるFPS値を取得または設定する。
        /// </summary>
        public decimal FramesPerSecond
        {
            get { return _framesPerSecond; }
            set
            {
                var v = Math.Min(Math.Max(0.001m, value), 999999);
                if (v != _framesPerSecond)
                {
                    _framesPerSecond = v;
                    this.NotifyPropertyChanged("FramesPerSecond");
                }
            }
        }
        private decimal _framesPerSecond = 30;

        /// <summary>
        /// 読み仮名文字列更新タスクのキャンセルトークンソースを取得または設定する。
        /// </summary>
        private CancellationTokenSource LipKanaTaskCanceller { get; set; }

        /// <summary>
        /// リップシンクユニットリスト更新タスクのキャンセルトークンソースを
        /// 取得または設定する。
        /// </summary>
        private CancellationTokenSource LipSyncUnitsTaskCanceller { get; set; }

        /// <summary>
        /// 読み仮名文字列更新タスクを開始する。
        /// </summary>
        private void StartLipKanaTask()
        {
            // 前回の処理が途中ならキャンセル
            if (this.LipKanaTaskCanceller != null)
            {
                this.LipKanaTaskCanceller.Cancel();
                this.LipKanaTaskCanceller.Dispose();
                this.LipKanaTaskCanceller = null;
            }

            // キャンセルソース生成
            this.LipKanaTaskCanceller = new CancellationTokenSource();

            var text = this.Text;

            // 読み仮名更新タスク開始
            Task.Factory
                .StartNew(
                    () => (new LipKanaMaker()).Make(text),
                    this.LipKanaTaskCanceller.Token)
                .ContinueWith(
                    t =>
                    {
                        // キャンセルされていなければ読み仮名文字列を更新
                        if (!t.IsCanceled)
                        {
                            this.LipKana = t.Result;
                        }

                        // キャンセルソースを破棄
                        this.LipKanaTaskCanceller.Dispose();
                        this.LipKanaTaskCanceller = null;
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        ///リップシンクユニットリスト更新タスクを開始する。
        /// </summary>
        private void StartLipSyncUnitsTask()
        {
            // 前回の処理が途中ならキャンセル
            if (this.LipSyncUnitsTaskCanceller != null)
            {
                this.LipSyncUnitsTaskCanceller.Cancel();
                this.LipSyncUnitsTaskCanceller.Dispose();
                this.LipSyncUnitsTaskCanceller = null;
            }

            // キャンセルソース生成
            this.LipSyncUnitsTaskCanceller = new CancellationTokenSource();

            var lipKana = this.LipKana;

            // 読み仮名更新タスク開始
            Task.Factory
                .StartNew(
                    () => (new LipSyncMaker()).Make(lipKana),
                    this.LipSyncUnitsTaskCanceller.Token)
                .ContinueWith(
                    t =>
                    {
                        // キャンセルされていなければリップシンクユニットリストを更新
                        if (!t.IsCanceled)
                        {
                            this.LipSyncUnits = t.Result.AsReadOnly();
                        }

                        // キャンセルソースを破棄
                        this.LipSyncUnitsTaskCanceller.Dispose();
                        this.LipSyncUnitsTaskCanceller = null;
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// 口パク時間指定単位種別変更による時間指定値の計算を行う。
        /// </summary>
        /// <param name="value">変更前の時間指定値。</param>
        /// <param name="oldUnit">変更前の時間指定単位種別。</param>
        /// <param name="newUnit">変更後の時間指定単位種別。</param>
        /// <returns>変更後の時間指定値。</returns>
        private decimal CalcSpanValueByUnit(
            decimal value,
            LipSpanUnit oldUnit,
            LipSpanUnit newUnit)
        {
            if (oldUnit == newUnit)
            {
                return value;
            }

            switch (oldUnit)
            {
            case LipSpanUnit.MilliSeconds:
                value *= this.FramesPerSecond;
                value /= 1000;
                break;

            case LipSpanUnit.Seconds:
                value *= this.FramesPerSecond;
                break;
            }

            switch (newUnit)
            {
            case LipSpanUnit.MilliSeconds:
                value *= 1000;
                value /= this.FramesPerSecond;
                break;

            case LipSpanUnit.Seconds:
                value /= this.FramesPerSecond;
                break;
            }

            return value;
        }
    }
}
