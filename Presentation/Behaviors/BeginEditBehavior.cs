using System.Windows;
using System.Windows.Input;

namespace DutyPlanner.Presentation.Behaviors
{
    public static class BeginEditBehavior
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(BeginEditBehavior),
                new PropertyMetadata(null, OnChanged));

        public static ICommand? GetCommand(DependencyObject obj)
            => (ICommand?)obj.GetValue(CommandProperty);

        public static void SetCommand(DependencyObject obj, ICommand value)
            => obj.SetValue(CommandProperty, value);

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                element.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
                element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            }
        }

        private static void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement element) return;

            var command = GetCommand(element);
            var parameter = element.DataContext;

            if (command?.CanExecute(parameter) == true)
                command.Execute(parameter);
        }
    }
}
