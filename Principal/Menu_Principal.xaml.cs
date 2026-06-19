using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
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
using POS.ConexionBD;
using POS.Inicio_Sesion;
using POS.Ventas;

namespace POS.Principal
{

    public partial class Menu_Principal : Window
    {

        public Menu_Principal()
        {
            InitializeComponent();
            ConfigurarMenu();
            MostrarNombreUsuario();
        }
        
        private void ConfigurarMenu()
        {
            if (Sesion.Tipo == "Empleado")
            {
                Boton_Lateral_Empleados.Visibility = Visibility.Collapsed;
                Panel_Empleado.Visibility = Visibility.Collapsed;

                Panel_Categoria.Visibility = Visibility.Collapsed;
                Boton_Lateral_Categorias.Visibility = Visibility.Collapsed;

                Panel_Proveedores.Visibility = Visibility.Collapsed;
                Boton_Lateral_Proveedores.Visibility = Visibility.Collapsed;

                Panel_Reportes.Visibility = Visibility.Collapsed;
                Boton_Lateral_Reportes.Visibility = Visibility.Collapsed;

                Panel_Detalle.Visibility = Visibility.Collapsed;
                Boton_Lateral_Detalle_Venta.Visibility = Visibility.Collapsed;
            }


            else if (Sesion.Tipo == "Administrador")
            {
            }
            else
            {
                MessageBox.Show("Tipo de usuario no reconocido.");
                this.Closing -= Window_Closing;

                try
                {
                    SesionHelper.CerrarSesion();
                    this.Close();
                }
                finally
                {
                    // Volver a agregar el manejador
                    this.Closing += Window_Closing;
                } // Cierra la ventana si el tipo de usuario no es válido
            }
        }

        private void MostrarNombreUsuario()
        {
            lblUsuario.Text = $"Bienvenido, {Sesion.NombreUsuario} ({Sesion.Tipo}) ";
        }
        private void Boton_Lateral_Empleados_Checked(object sender, RoutedEventArgs e)
        {
           // var empleadosWindow = new Window();
            var empleadosWindow = new POS.Empleados.Empleados(); 
            empleadosWindow.ShowDialog();
            //this.Close(); // Cierra la ventana Menu_Principal si deseas cerrarla
        }
        private void Boton_Lateral_Ventas_Checked(object sender, RoutedEventArgs e)
        {
            // Si es un UserControl

            Window ventasWindow = new Window();
            ventasWindow.Title = "Ventas";
            ventasWindow.Content = new POS.Ventas.Ventas(); 
            ventasWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ventasWindow.WindowState = WindowState.Maximized; // Maximiza la ventana
            ventasWindow.ShowDialog();
        }
        private void Boton_Lateral_Categorias_Checked(object sender, RoutedEventArgs e)
        {
            // Si es un UserControl
            Window inventarioWindow = new Window();
            inventarioWindow.Title = "Categoriass";
            inventarioWindow.Content = new POS.Inventario.Inventario(); 
            inventarioWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            inventarioWindow.ShowDialog();
            //this.Close(); // Cierra la ventana Menu_Principal si deseas cerrarla
        }
        private void Boton_Lateral_Clientes_Checked(object sender, RoutedEventArgs e)
        {
            // Si es un UserControl
            Window clientesWindow = new Window();
            clientesWindow.Title = "Clientes";
            clientesWindow.Content = new POS.Clientes.Clientes(); 
            clientesWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            clientesWindow.ShowDialog();
            //this.Close(); // Cierra la ventana Menu_Principal si deseas cerrarla
        }
        private void Boton_Lateral_Productos_Checked(object sender, RoutedEventArgs e)
        {
            // Si es un UserControl
            Window productosWindow = new Window();
            productosWindow.Title = "Productos";
            productosWindow.Content = new POS.Productos.Productos(); 
            productosWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            productosWindow.ShowDialog();
            //this.Close(); // Cierra la ventana Menu_Principal si deseas cerrarla
        }
        private void Boton_Lateral_Apartados_Checked(object sender, RoutedEventArgs e)
        {
            // Si es un UserControl
            Window apartadosWindow = new Window();
            apartadosWindow.Title = "Apartados";
            apartadosWindow.Content = new POS.Apartados.Apartados(); 
            apartadosWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            apartadosWindow.ShowDialog();
            //this.Close(); // Cierra la ventana Menu_Principal si deseas cerrarla
        }
        private void Boton_Lateral_Proveedores_Checked(object sender, RoutedEventArgs e)
        {
            // Si es un UserControl
            var proveedoresWindow = new POS.Proveedores.Proveedores();
            proveedoresWindow.Title = "Proveedores";
            proveedoresWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            proveedoresWindow.ShowDialog();
            //this.Close(); // Cierra la ventana Menu_Principal si deseas cerrarla
        }
        private void Boton_Lateral_Reportes_Checked(object sender, RoutedEventArgs e)
        {
            // Si es un UserControl
            var reportesWindow = new POS.Reporte.Reportes(Sesion.EmpleadoID);
            reportesWindow.Title = "Reportes";
            reportesWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            reportesWindow.ShowDialog();
            //this.Close(); // Cierra la ventana Menu_Principal si deseas cerrarla
        }
        private void Boton_Lateral_Detalle_Venta_Checked(object sender, RoutedEventArgs e)
        {
            var detalleVentaWindow = new POS.Ventas.Detalle_Venta();
            detalleVentaWindow.Title = "Detalles de Venta";
            detalleVentaWindow.ShowDialog();
        }

        private void RBtn_Salir_Click(object sender, RoutedEventArgs e)
        {
            // Remover el manejador temporalmente
            this.Closing -= Window_Closing;

            try
            {
                SesionHelper.CerrarSesion();
                this.Close();
            }
            finally
            {
                // Volver a agregar el manejador
                this.Closing += Window_Closing;
            }
        }
        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var dialog = new ConfirmacionSalidaDialog();
            dialog.ShowDialog();

            switch (dialog.AccionSeleccionada)
            {
                case ConfirmacionSalidaDialog.AccionSalida.CerrarSesion:
                    SesionHelper.CerrarSesion();
                    //new Aplicacion_Sistema_D2.Inicio_Sesion.Inicio().Show();
                    await Task.Delay(1000); // Espera 1 segundo
                    e.Cancel = true;
                    this.Close();
                    break;

                case ConfirmacionSalidaDialog.AccionSalida.SalirAplicacion:
                    SesionHelper.CerrarSesion(false);
                    Application.Current.Shutdown();
                    break;

                case ConfirmacionSalidaDialog.AccionSalida.Continuar:
                    e.Cancel = true;
                    break;
            }
        }
        public static class SesionHelper
        {
            public static void CerrarSesion(bool redirigirALogin = true)
            {
                try
                {
                    if (Sesion.EmpleadoID > 0)
                    {
                        RegistrarFinSesionEnBD();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al registrar fin de sesión: " + ex.Message);
                }
                finally
                {
                    Sesion.Limpiar();

                    if (redirigirALogin)
                    {
                        var inicioSesionWindow = new POS.Inicio_Sesion.Inicio();
                        inicioSesionWindow.Show();
                    }
                }
            }

            private static void RegistrarFinSesionEnBD()
            {
                var cnx = new Conexion();
                using (var connection = cnx.ObtenerConexion())
                {
                    string query = @"
                UPDATE TSesionesEmpleado 
                SET dFinSesion = GETDATE()
                WHERE nSesionID = (
                    SELECT TOP 1 nSesionID 
                    FROM TSesionesEmpleado 
                    WHERE nEmpleadoID = @empleadoID 
                    ORDER BY dInicioSesion DESC
                )";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@empleadoID", Sesion.EmpleadoID);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

    }
}
