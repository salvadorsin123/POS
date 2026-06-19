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

namespace POS.Empleados
{
    public partial class EditarTEmpleado : Window
    {
        private templeados empleado;

        // Constructor que recibe un objeto templeados
        public EditarTEmpleado(templeados empleado)
        {
            InitializeComponent();
            this.empleado = empleado;
            this.DataContext = empleado;  // Vincula los datos a la vista

            // Establecer las opciones del ComboBox
            Cbo_TipoEmpleado.ItemsSource = new List<string> { "Administrador", "Empleado" };
        }

        // Método para guardar los cambios en el empleado
        private void Btn_Guardar_Click(object sender, RoutedEventArgs e)
        {
            // Solo asignar nueva contraseña si el campo no está vacío
            if (!string.IsNullOrEmpty(Txt_NuevaContrasena.Text))
            {
                empleado.cContrasena = Txt_NuevaContrasena.Text;
            }

            this.DialogResult = true;
            this.Close();
        }

        // Método para cancelar la operación y cerrar la ventana
        private void Btn_Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;  // Indica que no se guardaron cambios
            this.Close();  // Cierra la ventana
        }
    }
}