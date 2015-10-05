using System;
using System.Linq;
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
        }

        /// <summary>
        /// コマンドを取得または設定する。
        /// </summary>
        private LipServiceCommand Command { get; set; } = new LipServiceCommand();

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
            if (morphWeights == null)
            {
                throw new ArgumentNullException(nameof(morphWeights));
            }

            // モーフ名が空文字列のものは弾く
            var param =
                new MorphWeightDataList(
                    morphWeights.Where(mw => !string.IsNullOrEmpty(mw.MorphName)));

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
        /// <param name="edgeWeightZero">
        /// キーフレームリストの先頭と終端で、
        /// 含まれている全モーフのウェイト値をゼロ初期化するならば true 。
        /// </param>
        /// <param name="edgeWeightHeld">
        /// クライアント側が対応していれば、キーフレームリスト挿入位置前後の
        /// ウェイト値を保持するならば true 。
        /// </param>
        public void SetKeyFramesCommand(
            MorphTimelineTable tlTable,
            decimal unitSeconds,
            bool edgeWeightZero,
            bool edgeWeightHeld)
        {
            if (tlTable == null)
            {
                throw new ArgumentNullException(nameof(tlTable));
            }
            if (unitSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(unitSeconds));
            }

            var param =
                new KeyFramesCommandParameter
                {
                    TimelineTable = tlTable,
                    UnitSeconds = unitSeconds,
                    IsEdgeWeightZero = edgeWeightZero,
                    IsEdgeWeightHeld = edgeWeightHeld,
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
