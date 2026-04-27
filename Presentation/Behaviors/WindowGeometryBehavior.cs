using System.Windows;

namespace DutyPlanner.Presentation.Behaviors
{
    public static class WindowGeometryBehavior
    {
        // ===== CornerRadius =====
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached(
                "CornerRadius",
                typeof(CornerRadius),
                typeof(WindowGeometryBehavior),
                new PropertyMetadata(new CornerRadius(8), OnGeometryPropertyChanged));

        public static CornerRadius GetCornerRadius(Window window) =>
            (CornerRadius)window.GetValue(CornerRadiusProperty);

        public static void SetCornerRadius(Window window, CornerRadius value) =>
            window.SetValue(CornerRadiusProperty, value);

        // ===== OuterMargin =====
        public static readonly DependencyProperty OuterMarginProperty =
            DependencyProperty.RegisterAttached(
                "OuterMargin",
                typeof(Thickness),
                typeof(WindowGeometryBehavior),
                new PropertyMetadata(new Thickness(10), OnGeometryPropertyChanged));

        public static Thickness GetOuterMargin(Window window) =>
            (Thickness)window.GetValue(OuterMarginProperty);

        public static void SetOuterMargin(Window window, Thickness value) =>
            window.SetValue(OuterMarginProperty, value);

        // ===== attach logic =====
        private static void OnGeometryPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is not Window window)
                return;

            // Подписываемся ОДИН РАЗ
            window.StateChanged -= OnWindowStateChanged;
            window.StateChanged += OnWindowStateChanged;

            ApplyGeometry(window);
        }

        private static void OnWindowStateChanged(object? sender, EventArgs e)
        {
            if (sender is not Window window)
                return;

            ApplyGeometry(window);
        }

        private static void ApplyGeometry(Window window)
        {
            if (window.WindowState == WindowState.Maximized)
            {
                SetCornerRadius(window, new CornerRadius(0));
                SetOuterMargin(window, new Thickness(0));
            }
            else
            {
                // значения по умолчанию
                SetCornerRadius(window, new CornerRadius(8));
                SetOuterMargin(window, new Thickness(10));
            }
        }
    }
}
