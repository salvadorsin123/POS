using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POS.Proveedores
{
    /// <summary>
    /// Lógica de interacción para ProductoProveedorFormWindow.xaml
    /// </summary>
    public partial class ProductoProveedorFormWindow : Window
    {
        public TProveedores_Productos Producto { get; private set; }

        public ProductoProveedorFormWindow(int proveedorId)
        {
            InitializeComponent();
            Producto = new TProveedores_Productos
            {
                nProveedorID = proveedorId
            };
        }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProducto.Text))
            {
                MessageBox.Show("El nombre del producto es obligatorio");
                return;
            }

            if (!double.TryParse(txtPrecio.Text, out double precio) || precio <= 0)
            {
                MessageBox.Show("Ingrese un precio válido");
                return;
            }

            Producto.cProducto = txtProducto.Text;
            Producto.nPrecioUnitario = precio;

            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
