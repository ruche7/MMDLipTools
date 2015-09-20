using System;
using System.Windows.Input;

namespace ruche.wpf.viewModel
{
    public class DelegateCommand : ICommand
    {
        public DelegateCommand() : this(null, null)
        {
        }

        public DelegateCommand(Action<object> executeDelegate)
            : this(executeDelegate, null)
        {
        }

        public DelegateCommand(
            Action<object> executeDelegate,
            Func<object, bool> canExecuteDelegate)
        {
            ExecuteDelegate = executeDelegate;
            CanExecuteDelegate = canExecuteDelegate;
        }

        public Action<object> ExecuteDelegate { get; set; }

        public Func<object, bool> CanExecuteDelegate { get; set; }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (ExecuteDelegate != null)
            {
                ExecuteDelegate(parameter);
            }
        }

        public bool CanExecute(object parameter)
        {
            return (CanExecuteDelegate == null) ? true : CanExecuteDelegate(parameter);
        }
    }
}
