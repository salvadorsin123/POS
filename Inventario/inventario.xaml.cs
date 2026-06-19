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
using POS.Productos;
using POS.ConexionBD;
using System.Data.SqlClient;



namespace POS.Inventario
{
    public partial class Inventario : UserControl
    {
        private List<Producto> listaProductos = new List<Producto>(); // Inicializa la lista vacía

        public Inventario()
        {
            InitializeComponent();
            CargarProductos();
        }

        private void Txt_BuscarInventario_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Manejo seguro de nulos
                string filtro = Txt_BuscarInventario?.Text?.ToLower() ?? string.Empty;

                var filtrados = listaProductos?
                    .Where(i => (i?.Categoria?.ToLower()?.Contains(filtro) ?? false) ||
                               (i?.ID.ToString()?.Contains(filtro) ?? false))
                    .ToList() ?? new List<Producto>();

                DataGrid_Inventario.ItemsSource = filtrados;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar: {ex.Message}");
            }
        }

        private void CargarProductos()
        {
            try
            {
                ProductoDAL2 productoDAL = new ProductoDAL2();
                listaProductos = productoDAL.ObtenerProductos() ?? new List<Producto>();
                DataGrid_Inventario.ItemsSource = listaProductos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}");
                listaProductos = new List<Producto>();
            }
        }

        private void Btn_AgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            Producto nuevoProducto = new Producto
            {
                Categoria = ""
            };

            EditarProducto ventanaEditar = new EditarProducto(nuevoProducto);
            bool? resultado = ventanaEditar.ShowDialog();

            if (resultado == true)
            {
                ProductoDAL2 productoDAL = new ProductoDAL2();
                productoDAL.AgregarCategoria(nuevoProducto); // <-- Agrega a la BD
                RefrescarDataGrid();
            }
        }


        private void Btn_EditarProducto_Click(object sender, RoutedEventArgs e)
        {
            var productoSeleccionado = (Producto)DataGrid_Inventario.SelectedItem;
            if (productoSeleccionado != null)
            {
                EditarProducto ventanaEditar = new EditarProducto(productoSeleccionado);
                bool? resultado = ventanaEditar.ShowDialog();

                if (resultado == true)
                {
                    ProductoDAL2 productoDAL = new ProductoDAL2();
                    productoDAL.ActualizarProducto(productoSeleccionado); // <-- Editar en BD
                    RefrescarDataGrid();
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un producto para editar.");
            }
        }


        private void Btn_EliminarProducto_Click(object sender, RoutedEventArgs e)
        {
            var productoSeleccionado = (Producto)DataGrid_Inventario.SelectedItem;
            if (productoSeleccionado != null)
            {
                MessageBoxResult result = MessageBox.Show("¿Estás seguro de eliminar este producto?", "Confirmación", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    ProductoDAL2 productoDAL = new ProductoDAL2();
                    productoDAL.EliminarProducto(productoSeleccionado.ID); // <-- Eliminar en BD
                    RefrescarDataGrid();
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un producto para eliminar.");
            }
        }


        private void RefrescarDataGrid()
        {
            ProductoDAL2 productoDAL = new ProductoDAL2();
            listaProductos = productoDAL.ObtenerProductos(); // ← Actualiza desde la BD

            DataGrid_Inventario.ItemsSource = null;
            DataGrid_Inventario.ItemsSource = listaProductos;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
