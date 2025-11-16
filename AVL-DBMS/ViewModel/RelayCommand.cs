using System.Windows.Input;

namespace AVL_DBMS.ViewModel
{
    /// <summary>
    /// Універсальна реалізація ICommand
    /// для зв'язування методів ViewModel з View.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// Викликається, коли змінюються умови, що впливають на
        /// можливість виконання команди (напр., валідація).
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Створює нову команду.
        /// </summary>
        /// <param name="execute">Логіка, яка має виконатися.</param>
        /// <param name="canExecute">Логіка, що перевіряє, чи можна виконати.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Перевіряє, чи може команда бути виконана.
        /// Використовується WPF для ввімкнення/вимкнення кнопок.
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Виконує логіку команди.
        /// </summary>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
