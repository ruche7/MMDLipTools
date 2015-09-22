using System;
using System.Runtime.Serialization;
using ruche.mmd.morph;

namespace ruche.mmd.service.lip
{
    /// <summary>
    /// 口パクサービスから各クライアントへ提供されるコマンドを表すクラス。
    /// </summary>
    [DataContract(Namespace = "")]
    [KnownType(typeof(MorphWeightDataList))]
    [KnownType(typeof(KeyFramesCommandParameter))]
    public class LipServiceCommand
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public LipServiceCommand() : this(0, LipServiceCommandId.None, null)
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="counter">カウンタ値。</param>
        /// <param name="id">種別ID。</param>
        /// <param name="parameter">パラメータ。</param>
        public LipServiceCommand(
            long counter,
            LipServiceCommandId id,
            object parameter)
        {
            this.Counter = counter;
            this.Id = id;
            this.Parameter = parameter;
        }

        /// <summary>
        /// カウンタ値を取得する。
        /// </summary>
        /// <remarks>
        /// このプロパティが同じ値を返す間は同一のコマンドであることを示す。
        /// </remarks>
        [DataMember]
        public long Counter { get; private set; }

        /// <summary>
        /// 種別IDを取得する。
        /// </summary>
        [DataMember]
        public LipServiceCommandId Id { get; private set; }

        /// <summary>
        /// パラメータを取得する。
        /// </summary>
        /// <remarks>
        /// 種別IDによって内容は異なる。
        /// LipServiceCommandId.MorphWeights ならば
        /// MorphWeightDataList オブジェクト、
        /// LipServiceCommandId.KeyFrames ならば
        /// KeyFramesCommandParameter オブジェクトが設定される。
        /// </remarks>
        [DataMember]
        public object Parameter { get; private set; }
    }
}
