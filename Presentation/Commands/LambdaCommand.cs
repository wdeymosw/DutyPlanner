namespace DutyPlanner.Presentation.Commands
{
    public class LambdaCommand : CommandBase
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public LambdaCommand(
            Action<object?> execute,
            Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public LambdaCommand(Action execute, Func<bool>? canExecute = null)
           : this(_ => execute(), canExecute is null ? null : _ => canExecute())
        {
        }

        public override bool CanExecute(object? parameter)
            => _canExecute?.Invoke(parameter) ?? true;

        public override void Execute(object? parameter)
            => _execute(parameter);

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

    }


    public class LambdaCommand<T> : CommandBase
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public LambdaCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public override bool CanExecute(object parameter)
        {
            if (parameter is not T)
                return false;

            return _canExecute?.Invoke((T)parameter) ?? true;
        }

        public override void Execute(object parameter)
        {
            if (parameter is T value)
                _execute(value);
        }
    }


}
