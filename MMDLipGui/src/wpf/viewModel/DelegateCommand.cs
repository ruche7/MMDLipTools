using System;
using System.Windows.Input;

namespace ruche.wpf.viewModel
{
    /// <summary>
    /// デリゲートによるコマンドを定義するクラス。
    /// </summary>
    public class DelegateCommand : ICommand
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public DelegateCommand() : this(null, null)
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="executeDelegate">実行デリゲート。</param>
        public DelegateCommand(Action<object> executeDelegate)
            : this(executeDelegate, null)
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="executeDelegate">実行デリゲート。</param>
        /// <param name="canExecuteDelegate">実行可否判定デリゲート。</param>
        public DelegateCommand(
            Action<object> executeDelegate,
            Func<object, bool> canExecuteDelegate)
        {
            this.ExecuteDelegate = executeDelegate;
            this.CanExecuteDelegate = canExecuteDelegate;
        }

        /// <summary>
        /// 実行デリゲートを取得または設定する。
        /// </summary>
        public Action<object> ExecuteDelegate { get; set; }

        /// <summary>
        /// 実行可否判定デリゲートを取得または設定する。
        /// </summary>
        /// <remarks>
        /// null の場合、 ExecuteDelegate プロパティが
        /// null であるか否かによって実行可否判定される。
        /// </remarks>
        public Func<object, bool> CanExecuteDelegate { get; set; }

        #region ICommand の実装

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter) =>
            this.ExecuteDelegate?.Invoke(parameter);

        public bool CanExecute(object parameter) =>
            (this.CanExecuteDelegate == null) ?
                (this.ExecuteDelegate != null) :
                this.CanExecuteDelegate(parameter);

        #endregion
    }
}
