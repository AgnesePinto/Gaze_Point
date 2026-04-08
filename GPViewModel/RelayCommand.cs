using System;
using System.Windows.Input;

namespace Gaze_Point.GPViewModel
{
    /// <summary>
    /// A standard implementation of the ICommand interface for the MVVM pattern.
    /// Encapsulates a delegate (Action) to be executed by UI elements.
    /// </summary>
    /// <author>Agnese Pinto</author>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// Initializes a new instance of the RelayCommand.
        /// </summary>
        /// <param name="execute">The logic to be executed.</param>
        /// <param name="canExecute">The logic that determines if the command can execute.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}






//using System;
//using System.Windows.Input;

//namespace Gaze_Point.GPViewModel
//{

//    /// <summary>
//    /// A standard implementation of the ICommand interface for the MVVM pattern.
//    /// Encapsulates a delegate (Action) to be executed by UI elements.
//    /// </summary>
//    /// <author>Agnese Pinto</author>


//    public class RelayCommand : ICommand
//    {
//        private readonly Action<object> _execute;
//        private readonly Predicate<object> _canExecute;


//        /// <summary>
//        /// Initializes a new instance of the RelayCommand.
//        /// </summary>
//        /// <param name="execute">The logic to be executed.</param>
//        /// <param name="canExecute">The logic that determines if the command can execute.</param>
//        /// <exception cref="ArgumentNullException"></exception>
//        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
//        {
//            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
//            _canExecute = canExecute;
//        }

//        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

//        public void Execute(object parameter) => _execute(parameter);

//        public event EventHandler CanExecuteChanged
//        {
//            add => CommandManager.RequerySuggested += value;
//            remove => CommandManager.RequerySuggested -= value;
//        }
//    }
//}

