using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using POS.Models;
using POS.ViewModels;

namespace POS.Views.Modulos;

public partial class VentasPage : Page
{
    private readonly VentasViewModel _vm = new();

    public VentasPage()
    {
        InitializeComponent();
        DataContext = _vm;
        _vm.VentaCompletada  += OnVentaCompletada;
        _vm.MostrarError     += OnError;
        _vm.Confirmar        += OnConfirmar;
        TxtCodigo.Focus();
    }

    private void TxtCodigo_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            _vm.EscanearCommand.Execute(null);
    }

    private void ProductoItem_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBoxItem { DataContext: POS.Models.Producto p })
            _vm.AgregarAlCarrito(p);
    }

    private void OnVentaCompletada(Venta v)
    {
        MessageBox.Show(
            $"Venta registrada exitosamente.\nTicket: {v.NumTicket}\nTotal: {v.Total:C2}",
            "Venta Completada", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OnError(string msg)
        => MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);

    private bool OnConfirmar(string msg)
        => MessageBox.Show(msg, "Confirmar",
               MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}
