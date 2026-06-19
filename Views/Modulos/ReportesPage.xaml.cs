using System.Windows;
using System.Windows.Controls;
using POS.ViewModels;

namespace POS.Views.Modulos;

public partial class ReportesPage : Page
{
    private readonly ReportesViewModel _vm = new();

    public ReportesPage()
    {
        InitializeComponent();
        DataContext = _vm;
        _vm.MostrarMensaje += msg =>
            MessageBox.Show(msg, "Información", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
