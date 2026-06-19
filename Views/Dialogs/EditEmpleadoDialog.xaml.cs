using System.Windows;
using POS.Models;

namespace POS.Views.Dialogs;

public partial class EditEmpleadoDialog : Window
{
    public Empleado? Resultado { get; private set; }
    public string? NuevaContrasena { get; private set; }
    private readonly Empleado? _original;

    public EditEmpleadoDialog(Empleado? empleado, List<TipoEmpleado> tipos)
    {
        InitializeComponent();
        _original = empleado;
        CbxTipo.ItemsSource = tipos;

        if (empleado != null)
        {
            TxtTitulo.Text    = "Editar Empleado";
            TxtNombre.Text    = empleado.Nombre;
            TxtAP.Text        = empleado.ApellidoPaterno ?? string.Empty;
            TxtAM.Text        = empleado.ApellidoMaterno ?? string.Empty;
            TxtUsername.Text  = empleado.Username;
            TxtTelefono.Text  = empleado.Telefono ?? string.Empty;
            TxtCorreo.Text    = empleado.Correo ?? string.Empty;
            TxtSalario.Text   = empleado.Salario.ToString("N2");
            TxtPassHint.Visibility = Visibility.Visible;
            CbxTipo.SelectedItem = tipos.FirstOrDefault(t => t.IdTipoEmpleado == empleado.IdTipoEmpleado);
        }
        else
        {
            CbxTipo.SelectedIndex = 1; // Cajero por defecto
        }
    }

    private void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        TxtError.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(TxtNombre.Text))
        { TxtError.Text = "El nombre es requerido."; return; }
        if (string.IsNullOrWhiteSpace(TxtUsername.Text))
        { TxtError.Text = "El usuario es requerido."; return; }
        if (_original == null && string.IsNullOrEmpty(TxtPassword.Password))
        { TxtError.Text = "La contraseña es requerida para nuevos empleados."; return; }
        if (CbxTipo.SelectedItem == null)
        { TxtError.Text = "Seleccione un rol."; return; }
        if (!decimal.TryParse(TxtSalario.Text.Replace(",", ""), out decimal salario))
        { TxtError.Text = "Salario inválido."; return; }

        NuevaContrasena = string.IsNullOrEmpty(TxtPassword.Password) ? null : TxtPassword.Password;

        Resultado = new Empleado
        {
            IdEmpleado      = _original?.IdEmpleado ?? 0,
            Nombre          = TxtNombre.Text.Trim(),
            ApellidoPaterno = Nullable(TxtAP.Text),
            ApellidoMaterno = Nullable(TxtAM.Text),
            Username        = TxtUsername.Text.Trim(),
            Telefono        = Nullable(TxtTelefono.Text),
            Correo          = Nullable(TxtCorreo.Text),
            Salario         = salario,
            IdTipoEmpleado  = ((TipoEmpleado)CbxTipo.SelectedItem).IdTipoEmpleado,
            Activo          = true,
        };
        DialogResult = true;
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;

    private static string? Nullable(string s)
        => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
