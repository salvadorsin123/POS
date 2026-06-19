using System.Windows;
using System.Windows.Controls;
using POS.Models;
using POS.ViewModels;
using POS.Views.Dialogs;

namespace POS.Views.Modulos;

public partial class ClientesPage : Page
{
    private readonly ClientesViewModel _vm = new();

    public ClientesPage()
    {
        InitializeComponent();
        DataContext = _vm;
        _vm.AbrirDialogo += OnAbrirDialogo;
        _vm.Confirmar    += OnConfirmar;
    }

    private void OnAbrirDialogo(Cliente? cliente)
    {
        var dlg = new EditClienteDialog(cliente) { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true && dlg.Resultado != null)
            _vm.GuardarCliente(dlg.Resultado);
    }

    private bool OnConfirmar(string msg)
        => MessageBox.Show(msg, "Confirmar",
               MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}
