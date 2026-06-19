using System.Windows;
using System.Windows.Controls;
using POS.Models;
using POS.ViewModels;
using POS.Views.Dialogs;

namespace POS.Views.Modulos;

public partial class ProveedoresPage : Page
{
    private readonly ProveedoresViewModel _vm = new();

    public ProveedoresPage()
    {
        InitializeComponent();
        DataContext = _vm;
        _vm.AbrirDialogo += OnAbrirDialogo;
        _vm.Confirmar    += OnConfirmar;
    }

    private void OnAbrirDialogo(Proveedor? p)
    {
        var dlg = new EditProveedorDialog(p) { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true && dlg.Resultado != null)
            _vm.GuardarProveedor(dlg.Resultado);
    }

    private bool OnConfirmar(string msg)
        => MessageBox.Show(msg, "Confirmar",
               MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}
