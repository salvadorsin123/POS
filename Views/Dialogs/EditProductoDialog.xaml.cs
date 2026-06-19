using System.Windows;
using POS.Models;

namespace POS.Views.Dialogs;

public partial class EditProductoDialog : Window
{
    public Producto? Resultado { get; private set; }
    private readonly Producto? _original;

    public EditProductoDialog(Producto? producto, List<Categoria> categorias)
    {
        InitializeComponent();
        _original = producto;

        CbxCategoria.ItemsSource = categorias;

        if (producto != null)
        {
            TxtTitulo.Text      = "Editar Producto";
            TxtModelo.Text      = producto.Modelo;
            TxtCodigo.Text      = producto.CodigoBarras ?? string.Empty;
            TxtPrecio.Text      = producto.Precio.ToString("N2");
            TxtDescuento.Text   = producto.Descuento.ToString("N1");
            TxtCantidad.Text    = producto.Cantidad.ToString();
            TxtDescripcion.Text = producto.Descripcion ?? string.Empty;
            CbxCategoria.SelectedItem = categorias.FirstOrDefault(
                c => c.IdCategoria == producto.IdCategoria);
        }
    }

    private void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        TxtError.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(TxtModelo.Text))
        {
            TxtError.Text = "El nombre del producto es requerido.";
            return;
        }
        if (!decimal.TryParse(TxtPrecio.Text.Replace(",", ""), out decimal precio) || precio < 0)
        {
            TxtError.Text = "Ingrese un precio válido.";
            return;
        }
        if (!decimal.TryParse(TxtDescuento.Text, out decimal desc) || desc < 0 || desc > 100)
        {
            TxtError.Text = "El descuento debe ser entre 0 y 100.";
            return;
        }
        if (!int.TryParse(TxtCantidad.Text, out int cant) || cant < 0)
        {
            TxtError.Text = "Ingrese una cantidad válida.";
            return;
        }

        Resultado = new Producto
        {
            IdProducto   = _original?.IdProducto ?? 0,
            Modelo       = TxtModelo.Text.Trim(),
            CodigoBarras = string.IsNullOrWhiteSpace(TxtCodigo.Text) ? null : TxtCodigo.Text.Trim(),
            Precio       = precio,
            Descuento    = desc,
            Cantidad     = cant,
            Descripcion  = string.IsNullOrWhiteSpace(TxtDescripcion.Text) ? null : TxtDescripcion.Text.Trim(),
            IdCategoria  = (CbxCategoria.SelectedItem as Categoria)?.IdCategoria,
            Activo       = true,
        };

        DialogResult = true;
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;
}
