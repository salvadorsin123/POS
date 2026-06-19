using System.Windows;
using POS.Models;

namespace POS.Views.Dialogs;

public partial class EditProveedorDialog : Window
{
    public Proveedor? Resultado { get; private set; }
    private readonly Proveedor? _original;

    public EditProveedorDialog(Proveedor? proveedor)
    {
        InitializeComponent();
        _original = proveedor;
        if (proveedor != null)
        {
            TxtTitulo.Text    = "Editar Proveedor";
            TxtNombre.Text    = proveedor.Nombre;
            TxtContacto.Text  = proveedor.Contacto ?? string.Empty;
            TxtTelefono.Text  = proveedor.Telefono ?? string.Empty;
            TxtRFC.Text       = proveedor.RFC ?? string.Empty;
            TxtCorreo.Text    = proveedor.Correo ?? string.Empty;
            TxtDireccion.Text = proveedor.Direccion ?? string.Empty;
        }
    }

    private void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        TxtError.Text = string.Empty;
        if (string.IsNullOrWhiteSpace(TxtNombre.Text))
        { TxtError.Text = "El nombre es requerido."; return; }

        Resultado = new Proveedor
        {
            IdProveedor = _original?.IdProveedor ?? 0,
            Nombre      = TxtNombre.Text.Trim(),
            Contacto    = Nullable(TxtContacto.Text),
            Telefono    = Nullable(TxtTelefono.Text),
            RFC         = Nullable(TxtRFC.Text),
            Correo      = Nullable(TxtCorreo.Text),
            Direccion   = Nullable(TxtDireccion.Text),
            Activo      = true,
        };
        DialogResult = true;
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;

    private static string? Nullable(string s)
        => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
