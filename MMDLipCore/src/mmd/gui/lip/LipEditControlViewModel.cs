using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ruche.mmd.morph;
using ruche.mmd.morph.converters;
using ruche.mmd.morph.lip;
using ruche.mmd.morph.lip.converters;
using ruche.util;
using ruche.wpf.viewModel;

namespace ruche.mmd.gui.lip
{
    /// <summary>
    /// LipEditControl の ViewModel クラス。
    /// </summary>
    public class LipEditControlViewModel : ViewModelBase
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
        /// 口パク時間指定単位種別変更による時間指定値の計算を行う。
        /// </summary>
        /// <param name="value">変更前の時間指定値。</param>
        /// <param name="oldUnit">変更前の時間指定単位種別。</param>
        /// <param name="newUnit">変更後の時間指定単位種別。</param>
        /// <param name="fps">FPS値。</param>
        /// <returns>変更後の時間指定値。</returns>
        private static decimal CalcSpanValueByUnit(
            decimal value,
            LipSpanUnit oldUnit,
            LipSpanUnit newUnit,
            decimal fps)
        {
            if (fps <= 0)
            {
                throw new ArgumentOutOfRangeException("fps");
            }

            if (oldUnit == newUnit)
            {
                return value;
            }

            switch (oldUnit)
            {
            case LipSpanUnit.MilliSeconds:
                value *= fps;
                value /= 1000;
                break;

            case LipSpanUnit.Seconds:
                value *= fps;
                break;
            }

            switch (newUnit)
            {
            case LipSpanUnit.MilliSeconds:
                value *= 1000;
                value /= fps;
                break;

            case LipSpanUnit.Seconds:
                value /= fps;
                break;
            }

            return value;
        }

        /// <summary>
        /// キーフレームリストを作成する。
        /// </summary>
        /// <returns>キーフレームリスト。</returns>
        private static List<KeyFrame> MakeKeyFrameListCore(
            IEnumerable<LipSyncUnit> units,
            decimal linkLengthPercent,
            float longSoundLastWeight,
            MorphInfoSet morphSet,
            bool morphEtoAI,
            LipSpanRange spanRange,
            decimal spanValue,
            LipSpanUnit spanUnit,
            decimal fps,
            long beginFrame)
        {
            // 口形状別タイムラインセット作成
            TimelineSet tlSet = null;
            {
                var maker = new TimelineSetMaker();
                maker.LinkLengthPercent = linkLengthPercent;
                maker.LongSoundLastWeight = longSoundLastWeight;
                tlSet = maker.Make(units);
            }

            // モーフ別タイムラインテーブル作成
            MorphTimelineTable tlTable = null;
            {
                var maker = new MorphTimelineTableMaker();
                tlTable = maker.Make(tlSet, morphSet, morphEtoAI);
            }

            // 基準フレーム長算出
            var frames =
                CalcSpanValueByUnit(spanValue, spanUnit, LipSpanUnit.Frames, fps);
            if (spanRange == LipSpanRange.All)
            {
                var end = tlTable.GetEndPlace();
                if (end > 0)
                {
                    frames /= end;
                }
            }

            // キーフレームリスト作成
            List<KeyFrame> dest = null;
            {
                var maker = new KeyFrameListMaker();
                maker.UnitFrameLength = frames;
                dest = maker.Make(tlTable, beginFrame);
            }

            return dest;
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public LipEditControlViewModel() : base()
        {
            // コマンド作成
            this.TextToLipKanaCommand =
                new DelegateCommand(_ => this.StartLipKanaTask());
            this.MorphPresetsEditCommand =
                new DelegateCommand(
                    this.ExecuteMorphPresetsEditCommand,
                    _ => this.IsMorphPresetsEditable);
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
                if (v != _morphPresets)
                {
                    // 空っぽならデフォルト値追加
                    if (v.Count == 0)
                    {
                        v.Add(new MorphPreset());
                    }

                    // 現在選択中のプリセット名を取得
                    var currentPreset = this.SelectedMorphPreset;
                    string currentName =
                        (currentPreset == null) ? null : currentPreset.Name;

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
                var v = Math.Min(Math.Max(-1, value), this.MorphPresets.Count - 1);
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
        [ConfigValue]
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
        [ConfigValue]
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
        private decimal _spanValue = 10;

        /// <summary>
        /// 口パクの時間指定単位種別を取得または設定する。
        /// </summary>
        [ConfigValue]
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
                    this.NotifyPropertyChanged("SpanValueFormat");

                    // 時間指定値を更新
                    this.SpanValue =
                        Math.Min(
                            Math.Max(
                                this.MinSpanValue,
                                CalcSpanValueByUnit(
                                    oldValue,
                                    oldUnit,
                                    value,
                                    this.Fps)),
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
        /// 口パクの時間指定値の推奨表示フォーマット指定を取得する。
        /// </summary>
        public string SpanValueFormat
        {
            get
            {
                switch (this.SpanUnit)
                {
                case LipSpanUnit.MilliSeconds: return "0.###";
                case LipSpanUnit.Seconds: return "0.000###";
                }
                return "0.###";
            }
        }

        /// <summary>
        /// 実フレーム値への変換に用いるFPS値を取得または設定する。
        /// </summary>
        [ConfigValue]
        public decimal Fps
        {
            get { return _fps; }
            set
            {
                var v = Math.Min(Math.Max(this.MinFps, value), this.MaxFps);
                if (v != _fps)
                {
                    _fps = v;
                    this.NotifyPropertyChanged("Fps");
                }
            }
        }
        private decimal _fps = 30;

        /// <summary>
        /// 実フレーム値への変換に用いるFPS値の最小許容値を取得する。
        /// </summary>
        public decimal MinFps
        {
            get { return 0.01m; }
        }

        /// <summary>
        /// 実フレーム値への変換に用いるFPS値の最大許容値を取得する。
        /// </summary>
        public decimal MaxFps
        {
            get { return 9999.99m; }
        }

        /// <summary>
        /// 前後のユニットとモーフ変化が重なる割合のパーセント値を取得または設定する。
        /// </summary>
        [ConfigValue]
        public decimal LinkLengthPercent
        {
            get { return _linkLengthPercent; }
            set
            {
                var v =
                    Math.Min(
                        Math.Max(this.MinLinkLengthPercent, value),
                        this.MaxLinkLengthPercent);
                if (v != _linkLengthPercent)
                {
                    _linkLengthPercent = v;
                    this.NotifyPropertyChanged("LinkLengthPercent");
                }
            }
        }
        private decimal _linkLengthPercent = TimelineSetMaker.DefaultLinkLengthPercent;

        /// <summary>
        /// 前後のユニットとモーフ変化が重なる割合の最小許容パーセント値を取得する。
        /// </summary>
        public decimal MinLinkLengthPercent
        {
            get { return TimelineSetMaker.MinLinkLengthPercent; }
        }

        /// <summary>
        /// 前後のユニットとモーフ変化が重なる割合の最大許容パーセント値を取得する。
        /// </summary>
        public decimal MaxLinkLengthPercent
        {
            get { return TimelineSetMaker.MaxLinkLengthPercent; }
        }

        /// <summary>
        /// 長音の最大開口終端位置におけるウェイト値割合のパーセント値を
        /// 取得または設定する。
        /// </summary>
        [ConfigValue]
        public float LongSoundLastWeightPercent
        {
            get { return _longSoundLastWeightPercent; }
            set
            {
                var v =
                    Math.Min(
                        Math.Max(this.MinLongSoundLastWeightPercent, value),
                        this.MaxLongSoundLastWeightPercent);
                if (v != _longSoundLastWeightPercent)
                {
                    _longSoundLastWeightPercent = v;
                    this.NotifyPropertyChanged("LongSoundLastWeightPercent");
                }
            }
        }
        private float _longSoundLastWeightPercent =
            TimelineSetMaker.DefaultLongSoundLastWeight * 100;

        /// <summary>
        /// 長音の最大開口終端位置におけるウェイト値割合の最小許容パーセント値を
        /// 取得する。
        /// </summary>
        public float MinLongSoundLastWeightPercent
        {
            get { return 0; }
        }

        /// <summary>
        /// 長音の最大開口終端位置におけるウェイト値割合の最大許容パーセント値を
        /// 取得する。
        /// </summary>
        public float MaxLongSoundLastWeightPercent
        {
            get { return 100; }
        }

        /// <summary>
        /// "え" から "あ","い" へのモーフ変更を行うか否かを取得または設定する。
        /// </summary>
        [ConfigValue]
        public bool IsMorphEtoAI
        {
            get { return _morphEtoAI; }
            set
            {
                if (value != _morphEtoAI)
                {
                    _morphEtoAI = value;
                    this.NotifyPropertyChanged("IsMorphEtoAI");
                }
            }
        }
        private bool _morphEtoAI = false;

        /// <summary>
        /// 現在の設定値からキーフレームリストを作成する。
        /// </summary>
        /// <param name="beginFrame">開始フレーム位置。</param>
        /// <returns>キーフレームリスト。</returns>
        public List<KeyFrame> MakeKeyFrameList(long beginFrame)
        {
            return this.MakeKeyFrameListAsync(beginFrame).Result;
        }

        /// <summary>
        /// 現在の設定値からキーフレームリストを非同期で作成する。
        /// </summary>
        /// <param name="beginFrame">開始フレーム位置。</param>
        /// <returns>キーフレームリスト作成タスク。</returns>
        /// <remarks>
        /// リップシンクユニットリストが作成途中であれば作成完了まで待機する。
        /// それ以外のパラメータはこのメソッドを呼び出した時点の値が利用される。
        /// </remarks>
        public Task<List<KeyFrame>> MakeKeyFrameListAsync(
            long beginFrame)
        {
            var linkLengthPercent = this.LinkLengthPercent;
            var longSoundLastWeight = this.LongSoundLastWeightPercent / 100;
            var preset = this.SelectedMorphPreset;
            var morphSet =
                (preset == null) ? (new MorphInfoSet()) : preset.Value.Clone();
            var morphEtoAI = this.IsMorphEtoAI;
            var spanRange = this.SpanRange;
            var spanValue = this.SpanValue;
            var spanUnit = this.SpanUnit;
            var fps = this.Fps;

            return
                Task.Factory
                    .StartNew(
                        () =>
                        {
                            // リップシンクユニットリスト作成途中なら待機
                            while (this.IsLipSyncUnitsTaskRunning)
                            {
                                Thread.Yield();
                            }
                        })
                    .ContinueWith(
                        _ =>
                        {
                            // メインスレッドでリップシンクユニットリストを取得
                            return this.LipSyncUnits.Select(u => u.Clone());
                        },
                        TaskScheduler.FromCurrentSynchronizationContext())
                    .ContinueWith(
                        t =>
                        {
                            // 実処理
                            return
                                MakeKeyFrameListCore(
                                    t.Result,
                                    linkLengthPercent,
                                    longSoundLastWeight,
                                    morphSet,
                                    morphEtoAI,
                                    spanRange,
                                    spanValue,
                                    spanUnit,
                                    fps,
                                    beginFrame);
                        });
        }

        /// <summary>
        /// 読み仮名文字列への変換を開始するコマンドを取得する。
        /// </summary>
        public ICommand TextToLipKanaCommand { get; private set; }

        /// <summary>
        /// 口パクモーフプリセットリストの編集を開始するコマンドを取得する。
        /// </summary>
        public ICommand MorphPresetsEditCommand { get; private set; }

        /// <summary>
        /// 現在選択中の口パクモーフプリセットを取得する。
        /// </summary>
        private MorphPreset SelectedMorphPreset
        {
            get
            {
                var index = this.SelectedMorphPresetIndex;
                return
                    (index >= 0 && index < this.MorphPresets.Count) ?
                        this.MorphPresets[index] : null;
            }
        }

        /// <summary>
        /// 読み仮名文字列更新タスクの実行カウンタを取得または設定する。
        /// </summary>
        private ulong LipKanaTaskCounter { get; set; }

        /// <summary>
        /// リップシンクユニットリスト更新タスクの実行カウンタを取得または設定する。
        /// </summary>
        private ulong LipSyncUnitsTaskCounter { get; set; }

        /// <summary>
        /// リップシンクユニットリスト更新タスク処理中であるか否かを
        /// 取得または設定する。
        /// </summary>
        private bool IsLipSyncUnitsTaskRunning
        {
            get { return _lipSyncUnitsTaskRunning; }
            set { _lipSyncUnitsTaskRunning = value; }
        }
        private volatile bool _lipSyncUnitsTaskRunning = false;

        /// <summary>
        /// 読み仮名文字列更新タスクを開始する。
        /// </summary>
        private void StartLipKanaTask()
        {
            var counter = ++this.LipKanaTaskCounter;
            var text = this.Text;

            // 読み仮名更新タスク開始
            // 後続タスクはメインスレッドで処理する
            (new LipKanaMaker())
                .MakeAsync(text)
                .ContinueWith(
                    t =>
                    {
                        // 完了前に再実行されていたら何もしない
                        if (this.LipKanaTaskCounter == counter)
                        {
                            // 値更新
                            this.LipKana = t.Result;
                        }
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// リップシンクユニットリスト更新タスクを開始する。
        /// </summary>
        private void StartLipSyncUnitsTask()
        {
            var counter = ++this.LipSyncUnitsTaskCounter;
            var lipKana = this.LipKana;

            // 処理中フラグを立てる
            this.IsLipSyncUnitsTaskRunning = true;

            // 読み仮名更新タスク開始
            // 後続タスクはメインスレッドで処理する
            Task.Factory
                .StartNew(() => (new LipSyncMaker()).Make(lipKana))
                .ContinueWith(
                    t =>
                    {
                        // 完了前に再実行されていたら何もしない
                        if (this.LipSyncUnitsTaskCounter == counter)
                        {
                            // 値更新
                            this.LipSyncUnits = t.Result.AsReadOnly();

                            // 処理中フラグをおろす
                            this.IsLipSyncUnitsTaskRunning = false;
                        }
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// MorphPresetsEditCommand を実行する。
        /// </summary>
        private void ExecuteMorphPresetsEditCommand(object param)
        {
            var shower = this.MorphPresetDialogShower;
            if (shower == null)
            {
                return;
            }

            // 編集ダイアログ処理
            var presets = shower(this.MorphPresets.Clone());
            if (presets != null)
            {
                // プリセットリスト更新
                this.MorphPresets = presets;
            }
        }
    }
}
