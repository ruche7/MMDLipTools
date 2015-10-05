using System;

namespace ruche.mmd.service.lip
{
    /// <summary>
    /// LipServiceCommand に関するイベントのイベントデータクラス。
    /// </summary>
    public class LipServiceCommandEventArgs : EventArgs
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="command">イベントに紐付くコマンド。</param>
        public LipServiceCommandEventArgs(LipServiceCommand command)
        {
            this.Command = command;
        }

        /// <summary>
        /// イベントに紐付くコマンドを取得する。
        /// </summary>
        public LipServiceCommand Command { get; }
    }
}
