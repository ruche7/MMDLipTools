using System;
using System.Diagnostics;
using System.Windows.Input;

namespace ruche.wpf.viewModel
{
    /// <summary>
    /// コマンドパラメータを Process.Start メソッドに渡すコマンドを定義するクラス。
    /// </summary>
    public class ProcessStartCommand : ICommand
    {
        /// <summary>
        /// インスタンス。 XAML から x:Static で参照する。
        /// </summary>
        public static readonly ProcessStartCommand Instance = new ProcessStartCommand();

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public ProcessStartCommand()
        {
        }

        #region ICommand の実装

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return (
                parameter != null &&
                !string.IsNullOrWhiteSpace(parameter.ToString()));
        }

        public void Execute(object parameter)
        {
            if (parameter == null)
            {
                return;
            }

            var cmd = parameter.ToString();
            if (string.IsNullOrWhiteSpace(cmd))
            {
                return;
            }

            Process.Start(cmd);
        }

        #endregion
    }
}
