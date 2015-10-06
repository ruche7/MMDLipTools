using System;
using System.Collections.Generic;
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
        /// <param name="morphWeights">モーフウェイト列挙。</param>
        public void SetMorphWeightsCommand(IEnumerable<MorphWeightData> morphWeights)
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
        /// <param name="targetMorphNames">
        /// 操作対象モーフ名配列。 tlTable.MorphNames を用いるならば null 。
        /// </param>
        /// <param name="tlTable">モーフ別タイムラインテーブル。</param>
        /// <param name="unitSeconds">
        /// ユニット基準長(「ア」の長さ)に相当する秒数値。
        /// </param>
        /// <param name="edgeWeightZero">
        /// キーフレームリストの先頭と終端で、
        /// 含まれている全モーフのウェイト値をゼロ初期化するならば true 。
        /// </param>
        /// <param name="naturalLink">
        /// クライアント側が対応していれば、キーフレームリスト挿入位置前後の
        /// ウェイト値から自然に繋ぐならば true 。
        /// </param>
        /// <param name="keyFrameReplacing">
        /// クライアント側が対応していれば、キーフレームリスト挿入範囲の
        /// 既存キーフレームを削除して置き換えるならば true 。
        /// </param>
        public void SetKeyFramesCommand(
            string[] targetMorphNames,
            MorphTimelineTable tlTable,
            decimal unitSeconds,
            bool edgeWeightZero,
            bool naturalLink,
            bool keyFrameReplacing)
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
                    TargetMorphNames = targetMorphNames,
                    TimelineTable = tlTable,
                    UnitSeconds = unitSeconds,
                    IsEdgeWeightZero = edgeWeightZero,
                    IsNaturalLink = naturalLink,
                    IsKeyFrameReplacing = keyFrameReplacing,
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
