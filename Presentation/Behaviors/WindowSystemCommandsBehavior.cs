using System.Windows;
using System.Windows.Controls;

namespace DutyPlanner.Presentation.Behaviors
{
    public static class WindowSystemCommandsBehavior
    {
        public static readonly DependencyProperty CloseProperty =
        DependencyProperty.RegisterAttached(
            "Close",
            typeof(bool),
            typeof(WindowSystemCommandsBehavior),
            new PropertyMetadata(false, OnCloseChanged));

        public static void SetClose(Button button, bool value) =>
            button.SetValue(CloseProperty, value);

        public static bool GetClose(Button button) =>
            (bool)button.GetValue(CloseProperty);

        private static void OnCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Button button || !(bool)e.NewValue)
                return;

            button.Click += (_, _) =>
            {
                var window = Window.GetWindow(button);
                window?.Close();
            };
        }

        // ---------- Minimize ----------
        public static readonly DependencyProperty MinimizeProperty =
            DependencyProperty.RegisterAttached(
                "Minimize",
                typeof(bool),
                typeof(WindowSystemCommandsBehavior),
                new PropertyMetadata(false, OnMinimizeChanged));

        public static void SetMinimize(Button button, bool value) =>
            button.SetValue(MinimizeProperty, value);

        public static bool GetMinimize(Button button) =>
            (bool)button.GetValue(MinimizeProperty);

        private static void OnMinimizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Button button || !(bool)e.NewValue)
                return;

            button.Click += (_, _) =>
            {
                var window = Window.GetWindow(button);
                if (window != null)
                    window.WindowState = WindowState.Minimized;
            };
        }

        // ---------- Toggle maximize ----------
        public static readonly DependencyProperty ToggleMaximizeProperty =
            DependencyProperty.RegisterAttached(
                "ToggleMaximize",
                typeof(bool),
                typeof(WindowSystemCommandsBehavior),
                new PropertyMetadata(false, OnToggleMaximizeChanged));

        public static void SetToggleMaximize(Button button, bool value) =>
            button.SetValue(ToggleMaximizeProperty, value);

        public static bool GetToggleMaximize(Button button) =>
            (bool)button.GetValue(ToggleMaximizeProperty);

        private static void OnToggleMaximizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Button button || !(bool)e.NewValue)
                return;

            button.Click += (_, _) =>
            {
                var window = Window.GetWindow(button);
                if (window == null)
                    return;

                window.WindowState =
                    window.WindowState == WindowState.Maximized
                        ? WindowState.Normal
                        : WindowState.Maximized;
            };

        }
    }
}
