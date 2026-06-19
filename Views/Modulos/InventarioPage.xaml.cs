using System.Windows;
using System.Windows.Controls;
using POS.ViewModels;

namespace POS.Views.Modulos;

public partial class InventarioPage : Page
{
    private readonly InventarioViewModel _vm = new();

    public InventarioPage()
    {
        InitializeComponent();
        DataContext = _vm;
        _vm.Confirmar += OnConfirmar;
    }

    private bool OnConfirmar(string msg)
        => MessageBox.Show(msg, "Confirmar",
               MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}
