using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Gaze_Point.GPModel.GPInteraction
{

    /// <summary>
    /// Global registry for identifying UI element types that the gaze system should consider inactive.
    /// </summary>
    /// <author>Agnese Pinto</author>
    

    public static class GPInteractiveElements
    {
        public static readonly HashSet<Type> InteractiveTypes = new HashSet<Type>
        {
            typeof(Button),
            typeof(TextBox),
            typeof(CheckBox),
            typeof(RadioButton),
            typeof(ComboBox),
            typeof(ComboBoxItem),
            typeof(MenuItem)
        };
    }
}