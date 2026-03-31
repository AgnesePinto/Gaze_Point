using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Gaze_Point.GPViewModel.Handlers
{
    public static class GazeHandlerFactory
    {
        private static readonly Dictionary<Type, IGazeActionHandler> _handlers = new Dictionary<Type, IGazeActionHandler>()
        {
            { typeof(TextBox), new TextBoxHandler() },
            { typeof(ComboBox), new ComboBoxHandler() },
            { typeof(ComboBoxItem), new ComboBoxHandler() },
            { typeof(Button), new StandardControlHandler() },
            { typeof(CheckBox), new StandardControlHandler() },
            { typeof(RadioButton), new StandardControlHandler() }
        };

        private static readonly IGazeActionHandler _defaultHandler = new StandardControlHandler();

        public static IGazeActionHandler GetHandler(Type type)
        {
            if (_handlers.TryGetValue(type, out var handler))
            {
                return handler;
            }
            return _defaultHandler;
        }
    }
}
