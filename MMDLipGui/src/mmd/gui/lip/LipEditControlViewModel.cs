using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ruche.mmd.morph;
using ruche.mmd.morph.converters;
using ruche.mmd.morph.lip;
using ruche.mmd.morph.lip.converters;
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
        /// <param name="presets">
        /// 編集対象の口パクモーフプリセットリスト。
        /// </param>
        /// <returns>
        /// 編集結果の口パクモーフプリセットリスト。
        /// 編集がキャンセルされた場合は null 。
        /// </returns>
        public delegate MorphPresetList MorphPresetDialogDelegate(
            MorphPresetList presets);

        /// <summary>
        /// 口パク時間指定単位種別変更による時間指定値の変換を行う。
        /// </summary>
        /// <param name="value">変更前の時間指定値。</param>
        /// <param name="oldUnit">変更前の時間指定単位種別。</param>
        /// <param name="newUnit">変更後の時間指定単位種別。</param>
        /// <param name="fps">FPS値。</param>
        /// <returns>変更後の時間指定値。</returns>
        private static decimal ConvertSpanValueUnit(
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
        /// FPS値変更によるフレーム数の変換を行う。
        /// </summary>
        /// <param name="frame">変更前のフレーム数。</param>
        /// <param name="oldFps">変更前のFPS値。</param>
        /// <param name="newFps">変更後のFPS値。</param>
        /// <returns>変更後のフレーム数。</returns>
        private static decimal ConvertSpanFrameFps(
            decimal frame,
            decimal oldFps,
            decimal newFps)
        {
            if (oldFps <= 0)
            {
                throw new ArgumentOutOfRangeException("oldFps");
            }
            if (newFps <= 0)
            {
                throw new ArgumentOutOfRangeException("newFps");
            }

            return (oldFps == newFps) ? frame : (frame * newFps / oldFps);
        }

        /// <summary>
        /// キーフレームリストを作成する。
        /// </summary>
        /// <returns>キーフレームリスト。</returns>
        private static KeyFrameList MakeKeyFrameListCore(
            IEnumerable<LipSyncUnit> units,
            decimal linkLengthPercent,
            float longSoundLastWeight,
            MorphInfoSet morphSet,
            bool morphEtoAI,
            LipSpanRange spanRange,
            decimal spanFrame,
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
            if (spanRange == LipSpanRange.All)
            {
                var end = tlTable.GetEndPlace();
                if (end > 0)
                {
                    spanFrame /= end;
                }
            }

            // キーフレームリスト作成
            KeyFrameList dest = null;
            {
                var maker = new KeyFrameListMaker();
                maker.UnitFrameLength = spanFrame;
                dest = maker.Make(tlTable, beginFrame);
            }

            return dest;
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public LipEditControlViewModel() : this(null, null)
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="editConfig">
        /// 口パク編集設定。既定値を用いるならば null 。
        /// </param>
        /// <param name="presetConfig">
        /// 口パクモーフプリセット編集設定。既定値を用いるならば null 。
        /// </param>
        public LipEditControlViewModel(
            LipEditConfig editConfig,
            MorphPresetConfig presetConfig)
        {
            // 設定初期化
            this.EditConfig = editConfig ?? (new LipEditConfig());
            this.PresetConfig = presetConfig ?? (new MorphPresetConfig());

            // 口パクモーフプリセットインデックス初期化
            var index = this.Presets.FindIndex(this.PresetConfig.ActivePresetName);
            this.SelectedPresetIndex = Math.Max(0, index);

            // コマンド作成
            this.TextToLipKanaCommand =
                new DelegateCommand(_ => this.StartLipKanaTask());
            this.PresetsEditCommand =
                new DelegateCommand(
                    this.ExecutePresetsEditCommand,
                    _ => this.IsPresetsEditable);
        }

        /// <summary>
        /// 口パク編集設定を取得する。
        /// </summary>
        /// <remarks>
        /// バインディング用のプロパティではない。
        /// </remarks>
        public LipEditConfig EditConfig { get; private set; }

        /// <summary>
        /// 口パクモーフプリセット編集設定を取得する。
        /// </summary>
        /// <remarks>
        /// バインディング用のプロパティではない。
        /// </remarks>
        public MorphPresetConfig PresetConfig { get; private set; }

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
            get { return this.EditConfig.IsAutoLipKana; }
            set
            {
                var old = this.IsAutoLipKana;
                this.EditConfig.IsAutoLipKana = value;
                if (this.IsAutoLipKana != old)
                {
                    this.NotifyPropertyChanged("IsAutoLipKana");

                    // true に変更されたら自動変換開始
                    if (this.IsAutoLipKana)
                    {
                        this.StartLipKanaTask();
                    }
                }
            }
        }

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
            get
            {
                lock (this.lipSyncUnitsLockObject)
                {
                    return _lipSyncUnits;
                }
            }
            private set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                bool changed = false;
                lock (this.lipSyncUnitsLockObject)
                {
                    if (value != _lipSyncUnits)
                    {
                        _lipSyncUnits = value;
                        changed = true;
                    }
                }

                if (changed)
                {
                    this.NotifyPropertyChanged("LipSyncUnits");

                    // 文字列表現値を作成
                    this.LipSyncText = string.Join("", value);
                }
            }
        }
        private ReadOnlyCollection<LipSyncUnit> _lipSyncUnits =
            (new List<LipSyncUnit>()).AsReadOnly();

        /// <summary>
        /// LipSyncUnits のロックオブジェクト。
        /// </summary>
        private object lipSyncUnitsLockObject = new object();

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
        public MorphPresetList Presets
        {
            get { return this.PresetConfig.Presets; }
            set
            {
                var old = this.Presets;
                this.PresetConfig.Presets = value;
                if (this.Presets != old)
                {
                    // プリセットインデックスの更新値を決定
                    var index =
                        this.Presets.FindIndex(this.PresetConfig.ActivePresetName);

                    this.NotifyPropertyChanged("Presets");

                    // プリセットインデックスを更新
                    // 選択中のプリセットが削除された場合は 0 にする
                    this.SelectedPresetIndex = Math.Max(0, index);

                    // アクティブなプリセット名を更新
                    var activePreset = this.SelectedPreset;
                    this.PresetConfig.ActivePresetName =
                        (activePreset == null) ? null : activePreset.Name;
                }
            }
        }

        /// <summary>
        /// 現在選択中の口パクモーフプリセットのインデックスを取得または設定する。
        /// </summary>
        public int SelectedPresetIndex
        {
            get { return _selectedPresetIndex; }
            set
            {
                var v = Math.Min(Math.Max(-1, value), this.Presets.Count - 1);
                if (v != _selectedPresetIndex)
                {
                    _selectedPresetIndex = v;
                    this.NotifyPropertyChanged("SelectedPresetIndex");

                    // アクティブなプリセット名を更新
                    var activePreset = this.SelectedPreset;
                    this.PresetConfig.ActivePresetName =
                        (activePreset == null) ? null : activePreset.Name;
                }
            }
        }
        private int _selectedPresetIndex = 0;

        /// <summary>
        /// 口パクモーフプリセット編集ダイアログの表示処理を行うデリゲートを
        /// 取得または設定する。
        /// </summary>
        public MorphPresetDialogDelegate PresetDialogShower
        {
            get { return _presetDialogShower; }
            set
            {
                if (value != _presetDialogShower)
                {
                    bool oldEditable = this.IsPresetsEditable;

                    _presetDialogShower = value;
                    this.NotifyPropertyChanged("PresetDialogShower");

                    if (this.IsPresetsEditable != oldEditable)
                    {
                        this.NotifyPropertyChanged("IsPresetsEditable");
                    }
                }
            }
        }
        private MorphPresetDialogDelegate _presetDialogShower;

        /// <summary>
        /// 口パクモーフプリセットリストの編集が可能であるか否かを取得する。
        /// </summary>
        /// <remarks>
        /// PresetDialogShower プロパティに有効なデリゲートを設定することで
        /// true を返すようになる。
        /// </remarks>
        public bool IsPresetsEditable
        {
            get { return (this.PresetDialogShower != null); }
        }

        /// <summary>
        /// 口パクの時間指定範囲種別を取得または設定する。
        /// </summary>
        public LipSpanRange SpanRange
        {
            get { return this.EditConfig.SpanRange; }
            set
            {
                var old = this.SpanRange;
                this.EditConfig.SpanRange = value;
                if (this.SpanRange != old)
                {
                    this.NotifyPropertyChanged("SpanRange");
                }
            }
        }

        /// <summary>
        /// 口パクの時間指定値を取得または設定する。
        /// </summary>
        public decimal SpanValue
        {
            get
            {
                return
                    ConvertSpanValueUnit(
                        this.EditConfig.SpanFrame,
                        LipSpanUnit.Frames,
                        this.SpanUnit,
                        this.Fps);
            }
            set
            {
                var old = this.SpanValue;
                this.EditConfig.SpanFrame =
                    ConvertSpanValueUnit(
                        value,
                        this.SpanUnit,
                        LipSpanUnit.Frames,
                        this.Fps);
                if (this.SpanValue != old)
                {
                    this.NotifyPropertyChanged("SpanValue");
                }
            }
        }

        /// <summary>
        /// 口パクの時間指定単位種別を取得または設定する。
        /// </summary>
        public LipSpanUnit SpanUnit
        {
            get { return this.EditConfig.SpanUnit; }
            set
            {
                var old = this.SpanUnit;
                this.EditConfig.SpanUnit = value;
                if (this.SpanUnit != old)
                {
                    this.NotifyPropertyChanged("SpanUnit");

                    // 関連プロパティの更新通知
                    this.NotifyPropertyChanged("MinSpanValue");
                    this.NotifyPropertyChanged("MaxSpanValue");
                    this.NotifyPropertyChanged("SpanValue");
                    this.NotifyPropertyChanged("SpanValueIncrement");
                    this.NotifyPropertyChanged("SpanValueFormat");
                }
            }
        }

        /// <summary>
        /// 口パクの時間指定値の最小許容値を取得する。
        /// </summary>
        public decimal MinSpanValue
        {
            get
            {
                return
                    ConvertSpanValueUnit(
                        LipEditConfig.MinSpanFrame,
                        LipSpanUnit.Frames,
                        this.SpanUnit,
                        this.Fps);
            }
        }

        /// <summary>
        /// 口パクの時間指定値の最大許容値を取得する。
        /// </summary>
        public decimal MaxSpanValue
        {
            get
            {
                return
                    ConvertSpanValueUnit(
                        LipEditConfig.MaxSpanFrame,
                        LipSpanUnit.Frames,
                        this.SpanUnit,
                        this.Fps);
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
        public decimal Fps
        {
            get { return this.EditConfig.Fps; }
            set
            {
                var old = this.Fps;
                this.EditConfig.Fps = value;
                if (this.Fps != old)
                {
                    this.NotifyPropertyChanged("Fps");

                    // 関連プロパティの更新通知
                    this.NotifyPropertyChanged("MinSpanValue");
                    this.NotifyPropertyChanged("MaxSpanValue");
                    this.NotifyPropertyChanged("SpanValue");
                }
            }
        }

        /// <summary>
        /// 実フレーム値への変換に用いるFPS値の最小許容値を取得する。
        /// </summary>
        public decimal MinFps
        {
            get { return LipEditConfig.MinFps; }
        }

        /// <summary>
        /// 実フレーム値への変換に用いるFPS値の最大許容値を取得する。
        /// </summary>
        public decimal MaxFps
        {
            get { return LipEditConfig.MaxFps; }
        }

        /// <summary>
        /// 前後のユニットとモーフ変化が重なる割合のパーセント値を取得または設定する。
        /// </summary>
        public decimal LinkLengthPercent
        {
            get { return this.EditConfig.LinkLengthPercent; }
            set
            {
                var old = this.LinkLengthPercent;
                this.EditConfig.LinkLengthPercent = value;
                if (this.LinkLengthPercent != old)
                {
                    this.NotifyPropertyChanged("LinkLengthPercent");
                }
            }
        }

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
        public float LongSoundLastWeightPercent
        {
            get { return this.EditConfig.LongSoundLastWeightPercent; }
            set
            {
                var old = this.LongSoundLastWeightPercent;
                this.EditConfig.LongSoundLastWeightPercent = value;
                if (this.LongSoundLastWeightPercent != old)
                {
                    this.NotifyPropertyChanged("LongSoundLastWeightPercent");
                }
            }
        }

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
        public bool IsMorphEtoAI
        {
            get { return this.EditConfig.IsMorphEtoAI; }
            set
            {
                var old = this.IsMorphEtoAI;
                this.EditConfig.IsMorphEtoAI = value;
                if (this.IsMorphEtoAI != old)
                {
                    this.NotifyPropertyChanged("IsMorphEtoAI");
                }
            }
        }

        /// <summary>
        /// 現在の設定値からキーフレームリストを作成する。
        /// </summary>
        /// <param name="fps">出力FPS値。</param>
        /// <param name="beginFrame">出力開始フレーム位置。出力FPS値基準。</param>
        /// <returns>キーフレームリスト。</returns>
        public KeyFrameList MakeKeyFrameList(decimal fps, long beginFrame)
        {
            return this.MakeKeyFrameListAsync(fps, beginFrame).Result;
        }

        /// <summary>
        /// 現在の設定値からキーフレームリストを非同期で作成する。
        /// </summary>
        /// <param name="fps">出力FPS値。</param>
        /// <param name="beginFrame">出力開始フレーム位置。出力FPS値基準。</param>
        /// <returns>キーフレームリスト作成タスク。</returns>
        /// <remarks>
        /// リップシンクユニットリストが作成途中であれば作成完了まで待機する。
        /// それ以外のパラメータはこのメソッドを呼び出した時点の値が利用される。
        /// </remarks>
        public Task<KeyFrameList> MakeKeyFrameListAsync(
            decimal fps,
            long beginFrame)
        {
            var linkLengthPercent = this.LinkLengthPercent;
            var longSoundLastWeight = this.LongSoundLastWeightPercent / 100;
            var preset = this.SelectedPreset;
            var morphSet =
                (preset == null) ? (new MorphInfoSet()) : preset.Value.Clone();
            var morphEtoAI = this.IsMorphEtoAI;
            var spanRange = this.SpanRange;
            var spanFrame =
                ConvertSpanFrameFps(this.EditConfig.SpanFrame, this.Fps, fps);

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

                            // 実処理
                            return
                                MakeKeyFrameListCore(
                                    this.LipSyncUnits,
                                    linkLengthPercent,
                                    longSoundLastWeight,
                                    morphSet,
                                    morphEtoAI,
                                    spanRange,
                                    spanFrame,
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
        public ICommand PresetsEditCommand { get; private set; }

        /// <summary>
        /// 現在選択中の口パクモーフプリセットを取得する。
        /// </summary>
        private MorphPreset SelectedPreset
        {
            get
            {
                var index = this.SelectedPresetIndex;
                return
                    (index >= 0 && index < this.Presets.Count) ?
                        this.Presets[index] : null;
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
        /// PresetsEditCommand を実行する。
        /// </summary>
        private void ExecutePresetsEditCommand(object param)
        {
            var shower = this.PresetDialogShower;
            if (shower == null)
            {
                return;
            }

            // 編集ダイアログ処理
            var presets = shower(this.Presets.Clone());
            if (presets != null)
            {
                // プリセットリスト更新
                this.Presets = presets;
            }
        }
    }
}
