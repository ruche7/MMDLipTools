using System;
using System.ServiceModel;
using ruche.mmd.morph;
using ruche.mmd.service.lip;

namespace MikuMikuLipMaker
{
    /// <summary>
    /// 口パクサービスクラス。
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class LipService : ILipService
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public LipService()
        {
            this.Command = new LipServiceCommand();
        }

        /// <summary>
        /// コマンドを取得または設定する。
        /// </summary>
        private LipServiceCommand Command { get; set; }

        /// <summary>
        /// Command プロパティの排他制御用オブジェクト。
        /// </summary>
        private object commandLockObject = new object();

        /// <summary>
        /// 口パクサービスから提供されるコマンドを取得する。
        /// </summary>
        /// <returns>コマンド。</returns>
        public LipServiceCommand GetCommand()
        {
            lock (commandLockObject)
            {
                return this.Command;
            }
        }

        /// <summary>
        /// MorphWeights コマンドを設定する。
        /// </summary>
        /// <param name="morphWeights">モーフウェイトリスト。</param>
        public void SetMorphWeightsCommand(MorphWeightDataList morphWeights)
        {
            var param = morphWeights.Clone();

            lock (commandLockObject)
            {
                this.Command =
                    new LipServiceCommand(
                        this.Command.Counter + 1,
                        LipServiceCommandId.MorphWeights,
                        param);
            }
        }

        /// <summary>
        /// KeyFrames コマンドを設定する。
        /// </summary>
        /// <param name="tlTable">モーフ別タイムラインテーブル。</param>
        /// <param name="unitSeconds">
        /// ユニット基準長(「ア」の長さ)に相当する秒数値。
        /// </param>
        public void SetKeyFramesCommand(
            MorphTimelineTable tlTable,
            decimal unitSeconds)
        {
            var param =
                new KeyFramesCommandParameter
                {
                    TimelineTable = tlTable,
                    UnitSeconds = unitSeconds,
                };

            lock (commandLockObject)
            {
                this.Command =
                    new LipServiceCommand(
                        this.Command.Counter + 1,
                        LipServiceCommandId.KeyFrames,
                        param);
            }
        }
    }
}
