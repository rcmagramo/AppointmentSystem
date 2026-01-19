using AppointmentSystem.WPF.ViewModels;
using System.Windows;

namespace AppointmentSystem.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}