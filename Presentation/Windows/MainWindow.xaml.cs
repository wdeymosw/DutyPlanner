using DutyPlanner.Presentation.ViewModels;
using System.Windows;

namespace DutyPlanner.Presentation.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowsViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            

        }

     

    }
}