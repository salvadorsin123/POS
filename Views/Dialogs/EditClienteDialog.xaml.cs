using System.Windows;
using POS.Models;

namespace POS.Views.Dialogs;

public partial class EditClienteDialog : Window
{
    public Cliente? Resultado { get; private set; }
    private readonly Cliente? _original;

    public EditClienteDialog(Cliente? cliente)
    {
        InitializeComponent();
        _original = cliente;
        if (cliente != null)
        {
            TxtTitulo.Text   = "Editar Cliente";
            TxtNombre.Text   = cliente.Nombre;
            TxtAP.Text       = cliente.ApellidoPaterno ?? string.Empty;
            TxtAM.Text       = cliente.ApellidoMaterno ?? string.Empty;
            TxtTelefono.Text = cliente.Telefono ?? string.Empty;
            TxtCorreo.Text   = cliente.Correo ?? string.Empty;
            TxtDireccion.Text = cliente.Direccion ?? string.Empty;
        }
    }

    private void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        TxtError.Text = string.Empty;
        if (string.IsNullOrWhiteSpace(TxtNombre.Text))
        {
            TxtError.Text = "El nombre es requerido.";
            return;
        }

        Resultado = new Cliente
        {
            IdCliente       = _original?.IdCliente ?? 0,
            Nombre          = TxtNombre.Text.Trim(),
            ApellidoPaterno = Nullable(TxtAP.Text),
            ApellidoMaterno = Nullable(TxtAM.Text),
            Telefono        = Nullable(TxtTelefono.Text),
            Correo          = Nullable(TxtCorreo.Text),
            Direccion       = Nullable(TxtDireccion.Text),
            Activo          = true,
        };
        DialogResult = true;
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;

    private static string? Nullable(string s)
        => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
