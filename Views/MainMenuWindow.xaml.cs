using System.Windows;
using System.Windows.Controls;
using POS.ViewModels;
using POS.Views.Modulos;

namespace POS.Views;

public partial class MainMenuWindow : Window
{
    private readonly MainMenuViewModel _vm = new();

    public MainMenuWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        _vm.CerrarSesionRequested += OnCerrarSesion;
        NavigateTo("Ventas");
    }

    private void NavButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
            NavigateTo(tag);
    }

    private void NavigateTo(string page)
    {
        TxtPageTitle.Text = page switch
        {
            "Ventas"      => "Punto de Venta",
            "Productos"   => "Gestión de Productos",
            "Clientes"    => "Gestión de Clientes",
            "Empleados"   => "Gestión de Empleados",
            "Inventario"  => "Categorías / Inventario",
            "Proveedores" => "Gestión de Proveedores",
            "Apartados"   => "Apartados",
            "Reportes"    => "Reportes y Estadísticas",
            _ => page
        };

        Page view = page switch
        {
            "Ventas"      => new VentasPage(),
            "Productos"   => new ProductosPage(),
            "Clientes"    => new ClientesPage(),
            "Empleados"   => new EmpleadosPage(),
            "Inventario"  => new InventarioPage(),
            "Proveedores" => new ProveedoresPage(),
            "Apartados"   => new ApartadosPage(),
            "Reportes"    => new ReportesPage(),
            _ => new VentasPage()
        };

        MainFrame.Navigate(view);
    }

    private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "¿Cerrar sesión?", "Confirmar",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
            _vm.CerrarSesionCommand.Execute(null);
    }

    private void OnCerrarSesion()
    {
        new LoginWindow().Show();
        Close();
    }
}
