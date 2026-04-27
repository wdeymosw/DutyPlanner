using DutyPlanner.Models;
using DutyPlanner.Presentation.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace DutyPlanner.Presentation.Behaviors
{
    public static class DragTargetBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(DragTargetBehavior),
                new PropertyMetadata(false, OnChanged));

        public static bool GetIsEnabled(DependencyObject obj)
            => (bool)obj.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(DependencyObject obj, bool value)
            => obj.SetValue(IsEnabledProperty, value);

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement fe)
                return;

            if ((bool)e.NewValue)
            {
                fe.AllowDrop = true;
                fe.Drop += OnDrop;
            }
            else
            {
                fe.Drop -= OnDrop;
            }
        }

        private static void OnDrop(object sender, DragEventArgs e)
        {
            if (sender is not FrameworkElement fe)
                return;

            if (fe.DataContext is not DayViewModel dayVm)
                return;

            var data = e.Data.GetData(e.Data.GetFormats()[0]);
            var placement = fe.Tag?.ToString() == "Reserve"? DayUserPlacement.Reserve : DayUserPlacement.Active;

            dayVm.OnDrop(data, placement);
        }
    }
}
