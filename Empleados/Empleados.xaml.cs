using POS.Empleados;
using POS.Principal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace POS.Empleados
{
    public partial class Empleados : Window
    {
        private List<templeados> listaEmpleados;

        public Empleados()
        {
            InitializeComponent();
            CargarEmpleados();
        }
        private EmpleadoDAL empleadoDAL = new EmpleadoDAL();

        private void CargarEmpleados()
        {
            listaEmpleados = empleadoDAL.ObtenerEmpleados();
            DataGrid_Empleados.ItemsSource = listaEmpleados;
        }
        private void Txt_BuscarEmpleado_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = Txt_BuscarEmpleado.Text.ToLower();
            var filtrados = listaEmpleados.Where(emp =>
                emp.cNombreUsuario.ToLower().Contains(filtro) ||
                emp.cContrasena.ToLower().Contains(filtro)).ToList();

            DataGrid_Empleados.ItemsSource = filtrados;
        }

        private void Btn_AgregarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            templeados nuevoEmpleado = new templeados(); // Sin ID

            EditarTEmpleado ventanaEditar = new EditarTEmpleado(nuevoEmpleado);
            bool? resultado = ventanaEditar.ShowDialog();

            if (resultado == true)
            {
                try
                {
                    empleadoDAL.InsertarEmpleado(nuevoEmpleado);
                    CargarEmpleados();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al insertar: " + ex.Message);
                }
            }

        }

        private void Btn_EditarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            var empleadoSeleccionado = (templeados)DataGrid_Empleados.SelectedItem;
            if (empleadoSeleccionado != null)
            {
                // Guardar la contraseña actual antes de editar
                string contrasenaActual = empleadoSeleccionado.cContrasena;

                // Usar clon para evitar modificar directamente el objeto original
                templeados copiaEmpleado = empleadoSeleccionado.Clonar();

                EditarTEmpleado ventanaEditar = new EditarTEmpleado(copiaEmpleado);
                bool? resultado = ventanaEditar.ShowDialog();

                if (resultado == true)
                {
                    // Actualizar datos básicos del empleado
                    empleadoDAL.ActualizarEmpleado(copiaEmpleado);

                    // Solo actualizar contraseña si fue modificada
                    if (!string.IsNullOrEmpty(copiaEmpleado.cContrasena) &&
                        copiaEmpleado.cContrasena != contrasenaActual)
                    {
                        empleadoDAL.ActualizarContrasena(
                            copiaEmpleado.nEmpleadoID,
                            copiaEmpleado.cNombreUsuario,
                            copiaEmpleado.cContrasena);
                    }

                    CargarEmpleados();
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un empleado para editar.");
            }
        }


        private void Btn_EliminarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            var empleadoSeleccionado = (templeados)DataGrid_Empleados.SelectedItem;
            if (empleadoSeleccionado != null)
            {
                var confirmacion = MessageBox.Show("¿Estás seguro de eliminar este empleado?", "Confirmar", MessageBoxButton.YesNo);
                if (confirmacion == MessageBoxResult.Yes)
                {
                    empleadoDAL.EliminarEmpleado(empleadoSeleccionado.nEmpleadoID);
                    CargarEmpleados();
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un empleado para eliminar.");
            }
        }

        private void Btn_Regresar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        } 
    }
}
