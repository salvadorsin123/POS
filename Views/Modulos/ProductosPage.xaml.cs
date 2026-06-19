using System.Windows;
using System.Windows.Controls;
using POS.Models;
using POS.ViewModels;
using POS.Views.Dialogs;

namespace POS.Views.Modulos;

public partial class ProductosPage : Page
{
    private readonly ProductosViewModel _vm = new();

    public ProductosPage()
    {
        InitializeComponent();
        DataContext = _vm;
        _vm.AbrirDialogo += OnAbrirDialogo;
        _vm.Confirmar    += OnConfirmar;
    }

    private void OnAbrirDialogo(Producto? producto)
    {
        var dlg = new EditProductoDialog(producto, _vm.Categorias)
        {
            Owner = Window.GetWindow(this)
        };
        if (dlg.ShowDialog() == true && dlg.Resultado != null)
            _vm.GuardarProducto(dlg.Resultado);
    }

    private bool OnConfirmar(string msg)
        => MessageBox.Show(msg, "Confirmar",
               MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}
