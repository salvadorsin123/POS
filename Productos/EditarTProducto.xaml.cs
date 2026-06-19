using POS.Modelos.Productos;
using System.Windows;
using POS.ConexionBD;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
namespace POS.Productos
{
    public partial class EditarTProducto : Window
    {
        private tproductos producto;
        public List<CategoriaItem> listaProductos;

        public EditarTProducto(tproductos producto)
        {
            InitializeComponent();
            this.producto = producto;
            this.DataContext = producto;
            
        }



        private void CargarCategoria(object sender, RoutedEventArgs e)
        {
            try
            {
                Conexion cnx = new Conexion();

                using (SqlConnection connection = cnx.ObtenerConexion())
                {
                    string query = "SELECT cNombre FROM TCategorias";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Cbo_Categoria.Items.Add(new tproductos
                        {
                            cTipo = reader["cNombre"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar Categorias: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Btn_Guardarr_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Btn_Cancelarr_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
