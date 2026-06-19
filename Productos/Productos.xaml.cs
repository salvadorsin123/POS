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
using System.Windows.Navigation;
using System.Windows.Shapes;
using POS.ConexionBD;
using POS.Modelos.Productos;
using POS.Inicio_Sesion;
using POS.Proveedores;
using Microsoft.Data.SqlClient;
namespace POS.Productos
{
    public partial class Productos : UserControl
    {
        private List<tproductos> listaProductos;

        public Productos()
        {
            InitializeComponent();
            CargarProductos();
            ConfigurarMenu();
        }

        private void ConfigurarMenu()
        {
            if (Sesion.Tipo == "Empleado")
            {
                // Ocultar botones de agregar y eliminar
                Btn_EliminarProducto.Visibility = Visibility.Collapsed;
                Btn_AgregarProducto.Visibility = Visibility.Collapsed;
                Btn_EditarProducto.Visibility = Visibility.Collapsed;
            }
            else if (Sesion.Tipo == "Administrador")
            {
                // Mostrar todos los botones
            }
            else
            {
                MessageBox.Show("Tipo de usuario no reconocido.");
            }
        }

        private void CargarProductos()
        {
            ProductosDAL3 productoDAL = new ProductosDAL3();
            listaProductos = productoDAL.ObtenerProductos(); // Asignamos a la lista global
            DataGrid_Productos.ItemsSource = listaProductos; // Ahora se carga en el DataGrid
        }


        private void Txt_BuscarProducto_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = Txt_BuscarProducto.Text.ToLower();
            var filtrados = listaProductos.Where(p =>
                p.cModelo.ToLower().Contains(filtro) ||
                p.cTipo.ToLower().Contains(filtro)).ToList();

            DataGrid_Productos.ItemsSource = filtrados;
        }

        private void Btn_AgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            tproductos nuevoProducto = new tproductos
            {
                cModelo = "",
                nPrecio = 0,
                nDescuento = 0,
                nCantidad = 0,
                cTipo = ""
            };

            EditarTProducto ventanaEditar = new EditarTProducto(nuevoProducto);
            bool? resultado = ventanaEditar.ShowDialog();

            if (resultado == true)
            {
                ProductosDAL3 productoDAL = new ProductosDAL3();
                bool insertado = productoDAL.InsertarProducto(nuevoProducto);

                if (insertado)
                {
                    MessageBox.Show("Producto agregado correctamente.");
                    CargarProductos();
                }
                else
                {
                    MessageBox.Show("Error al agregar el producto.");
                }
            }
        }
        


        private void Btn_EditarProducto_Click(object sender, RoutedEventArgs e)
        {
            var productoSeleccionado = (tproductos)DataGrid_Productos.SelectedItem;
            if (productoSeleccionado != null)
            {
                EditarTProducto ventanaEditar = new EditarTProducto(productoSeleccionado);
                bool? resultado = ventanaEditar.ShowDialog();

                if (resultado == true)
                {
                    ProductosDAL3 productoDAL = new ProductosDAL3();
                    bool actualizado = productoDAL.ActualizarProducto(productoSeleccionado);

                    if (actualizado)
                    {
                        MessageBox.Show("Producto actualizado correctamente.");
                        CargarProductos();
                    }
                    else
                    {
                        MessageBox.Show("Error al actualizar el producto.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un producto para editar.");
            }
        }



        private void Btn_EliminarProducto_Click(object sender, RoutedEventArgs e)
        {
            var productoSeleccionado = (tproductos)DataGrid_Productos.SelectedItem;

            if (productoSeleccionado != null)
            {
                MessageBoxResult result = MessageBox.Show("¿Estás seguro de eliminar este producto?", "Confirmación", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    ProductosDAL3 productoDAL = new ProductosDAL3();
                    bool eliminado = productoDAL.EliminarProducto(productoSeleccionado.nProductoID);

                    if (eliminado)
                    {
                        MessageBox.Show("Producto eliminado correctamente.");
                        CargarProductos();
                    }
                    else
                    {
                        MessageBox.Show("Error al eliminar el producto.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un producto para eliminar.");
            }
        }

    }
}
