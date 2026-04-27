using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DutyPlanner.Presentation.Behaviors
{
    public static class EndEditBehavior
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(EndEditBehavior),
                new PropertyMetadata(null, OnChanged));

        public static ICommand? GetCommand(DependencyObject obj)
            => (ICommand?)obj.GetValue(CommandProperty);

        public static void SetCommand(DependencyObject obj, ICommand value)
            => obj.SetValue(CommandProperty, value);

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.LostFocus -= TextBox_LostFocus;
                textBox.LostFocus += TextBox_LostFocus;
            }
        }

        private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            var command = GetCommand(textBox);
            var parameter = textBox.DataContext;

            if (command?.CanExecute(parameter) == true)
                command.Execute(parameter);
        }
    }
}
