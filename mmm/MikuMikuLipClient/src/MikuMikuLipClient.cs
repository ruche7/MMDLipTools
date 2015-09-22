using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MikuMikuPlugin;
using ruche.mmd.morph;
using ruche.mmd.morph.converters;
using ruche.mmd.service.lip;
using ruche.mmm.resources;

namespace ruche.mmm
{
    /// <summary>
    /// MikuMikuLipClient プラグインクラス。
    /// </summary>
    public class MikuMikuLipClient : IResidentPlugin
    {
        /// <summary>
        /// プラグインのGUID。
        /// </summary>
        private static readonly Guid PluginGuid =
            new Guid("6F583DB2-47D8-458B-8D5F-4F6D3D41C3D9");

        /// <summary>
        /// プラグインの表示テキスト。
        /// </summary>
        private static readonly string PluginText = @"MikuMikuLipクライアント";

        /// <summary>
        /// プラグインの英語表示テキスト。
        /// </summary>
        private static readonly string PluginEnglishText = @"MikuMikuLipClient";

        /// <summary>
        /// プラグインの説明文。
        /// </summary>
        private static readonly string PluginDescription =
            @"MikuMikuLipMaker の送信内容に従ってモデルを操作します。";

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MikuMikuLipClient()
        {
            this.Scene = null;
            this.Client = null;
            this.CommandQueue = new Queue<LipServiceCommand>();
        }

        /// <summary>
        /// シーンオブジェクトを取得する。
        /// </summary>
        public Scene Scene { get; private set; }

        /// <summary>
        /// 現在選択されているモデルを取得する。
        /// </summary>
        public Model ActiveModel
        {
            get { return (this.Scene == null) ? null : this.Scene.ActiveModel; }
        }

        /// <summary>
        /// プラグインが有効な状態であるか否かを取得する。
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// 口パクサービスクライアントを取得または設定する。
        /// </summary>
        private LipServiceClient Client { get; set; }

        /// <summary>
        /// コマンドキューを取得または設定する。
        /// </summary>
        private Queue<LipServiceCommand> CommandQueue { get; set; }

        /// <summary>
        /// 新規コマンド発行時に呼び出される。
        /// </summary>
        private void OnClientRaiseCommand(
            object sender,
            LipServiceCommandEventArgs e)
        {
            // キューに追加
            this.CommandQueue.Enqueue(e.Command);
        }

        /// <summary>
        /// MorphWeights コマンドによる更新処理を行う。
        /// </summary>
        /// <param name="param">コマンドパラメータ。</param>
        private void UpdateByMorphWeightsCommand(object param)
        {
            var morphWeights = param as MorphWeightDataList;
            var model = this.ActiveModel;
            if (morphWeights == null || model == null)
            {
                return;
            }

            // 一旦全モーフのウェイト値を 0 にする
            foreach (var morph in model.Morphs)
            {
                morph.CurrentWeight = 0;
            }

            // モーフ設定
            foreach (var mw in morphWeights)
            {
                var morph = model.Morphs[mw.MorphName];
                if (morph != null)
                {
                    morph.CurrentWeight = mw.Weight;
                }
            }
        }

        /// <summary>
        /// KeyFrames コマンドによる更新処理を行う。
        /// </summary>
        /// <param name="param">コマンドパラメータ。</param>
        private void UpdateByKeyFramesCommand(object param)
        {
            var p = param as KeyFramesCommandParameter;
            var model = this.ActiveModel;
            if (p == null || model == null)
            {
                return;
            }

            // キーフレームリスト作成
            var maker = new KeyFrameListMaker();
            maker.UnitFrameLength =
                p.UnitSeconds * (decimal)this.Scene.KeyFramePerSec;
            var keyFrames = maker.Make(p.TimelineTable, this.Scene.MarkerPosition);

            // モーフ名ごとにグループ化してキーフレーム設定
            foreach (var morphKeyFrames in keyFrames.GroupBy(f => f.MorphName))
            {
                var morph = model.Morphs[morphKeyFrames.Key];
                if (morph != null)
                {
                    morph.Frames.AddKeyFrame(
                        morphKeyFrames
                            .Select(f => new MorphFrameData(f.Frame, f.Weight))
                            .ToList());
                }
            }
        }

        #region IResidentPlugin の実装

        public Guid GUID
        {
            get { return PluginGuid; }
        }

        public string Description
        {
            get { return PluginDescription; }
        }

        public Image Image
        {
            get { return Resource.Icon32; }
        }

        public Image SmallImage
        {
            get { return Resource.Icon20; }
        }

        public string Text
        {
            get { return PluginText; }
        }

        public string EnglishText
        {
            get { return PluginEnglishText; }
        }

        public void Initialize()
        {
            this.Dispose();

            // クライアント作成
            this.Client = LipServiceClient.OpenNetNamedPipe("MikuMikuLipMaker");
        }

        public void Update(float Frame, float ElapsedTime)
        {
            // キューからコマンドを取得
            if (this.CommandQueue.Count <= 0)
            {
                return;
            }
            var command = this.CommandQueue.Dequeue();

            // コマンド処理
            switch (command.Id)
            {
            case LipServiceCommandId.MorphWeights:
                this.UpdateByMorphWeightsCommand(command.Parameter);
                break;

            case LipServiceCommandId.KeyFrames:
                this.UpdateByKeyFramesCommand(command.Parameter);
                break;

            case LipServiceCommandId.None:
            default:
                // 何もしない
                break;
            }
        }

        public void Enabled()
        {
            if (!this.IsEnabled)
            {
                // コマンド発行を受け付ける
                this.Client.RaiseCommand += this.OnClientRaiseCommand;

                this.IsEnabled = true;
            }
        }

        public void Disabled()
        {
            if (this.IsEnabled)
            {
                // コマンド発行を無視し、既存コマンドを破棄
                this.Client.RaiseCommand -= this.OnClientRaiseCommand;
                this.CommandQueue.Clear();

                this.IsEnabled = false;
            }
        }

        public void Dispose()
        {
            // クライアント破棄
            if (this.Client != null)
            {
                this.Client.Close();
                this.Client = null;
            }

            // キューをクリア
            this.CommandQueue.Clear();
        }

        #endregion

        #region IResidentPlugin の明示的実装

        IWin32Window IBasePlugin.ApplicationForm { get; set; }

        Scene IHaveScenePlugin.Scene
        {
            get { return this.Scene; }
            set { this.Scene = value; }
        }

        #endregion
    }
}
