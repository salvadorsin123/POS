using POS.Productos;
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


namespace POS.Inventario
{
    public partial class EditarProducto : Window
    {
        // Propiedad que almacena el producto a editar
        public Producto Producto { get; set; }

        // Constructor que recibe el producto a editar
        public EditarProducto(Producto producto)
        {
            InitializeComponent();
            Producto = producto;
            this.DataContext = Producto;  // Vincula el DataContext al producto recibido
        }

        // Evento al hacer clic en "Guardar"
        private void Btn_Guardar_Click(object sender, RoutedEventArgs e)
        {
            // Asegurémonos de que los campos no estén vacíos antes de guardar
            if (string.IsNullOrEmpty(Producto.Categoria))
            {
                MessageBox.Show("Por favor, completa todos los campos.");
                return;
            }

            // Al hacer clic en "Guardar", cerramos la ventana y pasamos el resultado como true
            this.DialogResult = true; // Esto indica que se ha guardado correctamente
            this.Close();
        }

        // Evento al hacer clic en "Cancelar"
        private void Btn_Cancelar_Click(object sender, RoutedEventArgs e)
        {
            // Si se cancela, cerramos la ventana sin guardar cambios
            this.DialogResult = false;
            this.Close();
        }
    }
}