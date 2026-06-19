using POS.ConexionBD;
using POS.Principal;
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
using System.Windows.Threading;
using Microsoft.Data.SqlClient;
using POS.Ventas;

namespace POS.Inicio_Sesion
{
    public partial class Inicio : Window
    {
        public Inicio()
        {
            InitializeComponent();
        }

        private async void Form_load(object sender, RoutedEventArgs e)
        {
            try
            {
                Conexion cnx = new Conexion();

                using (SqlConnection connection = await cnx.ObtenerConexionAsync())
                {
                    if (connection == null)
                        return;

                    string query = "SELECT cNombreUsuario FROM TEmpleados";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Cbo_Usuarios.Items.Add(new UsuarioItem {
                                usuarioNombre = reader["cNombreUsuario"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar usuarios: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btn_Iniciar_Click(object sender, RoutedEventArgs e)
        {
            string clave = Txt_Password.Password;
            UsuarioItem clienteSeleccionado = Cbo_Usuarios.SelectedItem as UsuarioItem;

            if (clienteSeleccionado == null)
            {
                MessageBox.Show("Por favor, selecciona un usuario válido.");
                return;
            }

            if (string.IsNullOrEmpty(clave))
            {
                MessageBox.Show("Ingrese contraseña", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Conexion cnx = new Conexion();
                using (SqlConnection connection = await cnx.ObtenerConexionAsync())
                {
                    if (connection == null)
                        return;
                    string usuario = "";
                    string tipo = "";
                    int empleadoID = 0;
                    bool credencialesCorrectas = false;
                    string contrasenaHash = "";

                    string query = @"SELECT e.nEmpleadoID, e.cNombreUsuario, e.cContrasena, t.cNombre AS cTipo 
                    FROM TEmpleados e 
                    INNER JOIN TTipoEmpleado t ON e.nTipoID = t.nTipoID 
                    WHERE e.cNombreUsuario = @usuario";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@usuario", clienteSeleccionado.usuarioNombre);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                usuario = reader["cNombreUsuario"].ToString();
                                contrasenaHash = reader["cContrasena"].ToString();
                                tipo = reader["cTipo"].ToString();
                                empleadoID = Convert.ToInt32(reader["nEmpleadoID"]);

                                // Verificación combinando usuario y contraseña
                                credencialesCorrectas = MD5Helper.VerificarHashCombinado(
                                    usuario,
                                    clave,
                                    contrasenaHash);
                            }
                        }
                    }

                    if (credencialesCorrectas)
                    {
                        // Guardar datos en la clase estática Sesion
                        Sesion.NombreUsuario = usuario;
                        Sesion.Tipo = tipo;
                        Sesion.EmpleadoID = empleadoID;

                        // Registrar la sesión y actualizar último login
                        using (SqlTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                await RegistrarSesion(empleadoID, connection, transaction);
                                await ActualizarUltimoLogin(empleadoID, connection, transaction);
                                transaction.Commit();

                                // Mostrar ventana principal
                                Menu_Principal menuPrincipal = new Menu_Principal();
                                menuPrincipal.Show();
                                this.Close();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                MessageBox.Show("Error al registrar la sesión: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Credenciales incorrectas", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con la base de datos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RegistrarSesion(int empleadoID, SqlConnection connection, SqlTransaction transaction)
        {
            string query = @"INSERT INTO TSesionesEmpleado (nEmpleadoID, dInicioSesion)
                    VALUES (@empleadoID, GETDATE())";

            using (SqlCommand cmd = new SqlCommand(query, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@empleadoID", empleadoID);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task ActualizarUltimoLogin(int empleadoID, SqlConnection connection, SqlTransaction transaction)
        {
            string query = @"UPDATE TEmpleados 
                    SET dUltimoLogin = GETDATE()
                    WHERE nEmpleadoID = @empleadoID";

            using (SqlCommand cmd = new SqlCommand(query, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@empleadoID", empleadoID);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}

