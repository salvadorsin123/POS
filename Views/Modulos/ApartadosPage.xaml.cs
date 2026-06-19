using System.Windows;
using System.Windows.Controls;
using POS.ViewModels;

namespace POS.Views.Modulos;

public partial class ApartadosPage : Page
{
    private readonly ApartadosViewModel _vm = new();

    public ApartadosPage()
    {
        InitializeComponent();
        DataContext = _vm;
        _vm.Confirmar    += OnConfirmar;
        _vm.MostrarError += OnError;
    }

    private bool OnConfirmar(string msg)
        => MessageBox.Show(msg, "Confirmar",
               MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    private void OnError(string msg)
        => MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
}
