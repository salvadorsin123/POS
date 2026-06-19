using System.Windows;
using System.Windows.Controls;
using POS.Models;
using POS.ViewModels;
using POS.Views.Dialogs;

namespace POS.Views.Modulos;

public partial class EmpleadosPage : Page
{
    private readonly EmpleadosViewModel _vm = new();

    public EmpleadosPage()
    {
        InitializeComponent();
        DataContext = _vm;
        _vm.AbrirDialogo += OnAbrirDialogo;
        _vm.Confirmar    += OnConfirmar;
    }

    private void OnAbrirDialogo(Empleado? empleado)
    {
        var dlg = new EditEmpleadoDialog(empleado, _vm.Tipos)
        {
            Owner = Window.GetWindow(this)
        };
        if (dlg.ShowDialog() == true && dlg.Resultado != null)
        {
            bool ok = _vm.GuardarEmpleado(dlg.Resultado, dlg.NuevaContrasena);
            if (!ok)
                MessageBox.Show(_vm.StatusMessage, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private bool OnConfirmar(string msg)
        => MessageBox.Show(msg, "Confirmar",
               MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}
