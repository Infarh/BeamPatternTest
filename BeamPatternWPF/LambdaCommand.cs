using System;
using System.Windows.Input;

namespace BeamPatternWPF
{
    class LambdaCommand : ICommand
    {
        private readonly Action<object> f_Execute;
        private readonly Func<object, bool> f_CanExecute;

        public string Name { get; }

        public LambdaCommand(string Name, Action<object> Execute, Func<object, bool> CanExecute = null)
            : this(Execute, CanExecute)
        {
            this.Name = Name;
        }

        public LambdaCommand(Action<object> Execute, Func<object, bool> CanExecute = null)
        {
            f_Execute = Execute;
            f_CanExecute = CanExecute;
        }

        #region Implementation of ICommand

        /// <inheritdoc />
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <inheritdoc />
        public bool CanExecute(object parameter) => f_CanExecute?.Invoke(parameter) ?? true;

        /// <inheritdoc />
        public void Execute(object parameter) => f_Execute?.Invoke(parameter);

        #endregion
    }
}