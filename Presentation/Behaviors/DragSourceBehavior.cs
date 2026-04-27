using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DutyPlanner.Presentation.Behaviors
{
    public static class DragSourceBehavior
    {
        private static object? _dragItem;

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(DragSourceBehavior),
                new PropertyMetadata(false, OnChanged));

        public static bool GetIsEnabled(DependencyObject obj)
            => (bool)obj.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(DependencyObject obj, bool value)
            => obj.SetValue(IsEnabledProperty, value);

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBox listBox)
                return;

            if ((bool)e.NewValue)
            {
                listBox.PreviewMouseLeftButtonDown += OnMouseDown;
                listBox.PreviewMouseMove += OnMouseMove;
            }
            else
            {
                listBox.PreviewMouseLeftButtonDown -= OnMouseDown;
                listBox.PreviewMouseMove -= OnMouseMove;
            }
        }

        private static void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ListBox listBox)
                return;

            _dragItem = listBox.SelectedItem;
        }

        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (_dragItem == null)
                return;

            if (sender is not ListBox listBox)
                return;

            DragDrop.DoDragDrop(
                listBox,
                _dragItem,
                DragDropEffects.Copy);

            _dragItem = null;
        }
    }
}
