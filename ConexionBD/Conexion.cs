using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows;
using System.Data;
using Microsoft.Data.SqlClient;

namespace POS.ConexionBD
{
    public class Conexion
    {
        // Leer la cadena de conexión desde variable de entorno si está presente,
        // si no, usar Settings del proyecto.
        private string cadenaConexion =>
            Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? POS.Properties.Settings.Default.CadenaConexion;

        /// <summary>
        /// Devuelve una nueva conexión abierta basada en la configuración guardada.
        /// </summary>
        public SqlConnection ObtenerConexion()
        {
            try
            {
                // Verificamos si la cadena no está vacía (por si intentan usar la app sin configurar)
                if (string.IsNullOrEmpty(cadenaConexion))
                {
                    throw new Exception("La base de datos no ha sido configurada. Por favor, reinicie la aplicación.");
                }

                SqlConnection conexion = new SqlConnection(cadenaConexion);
                conexion.Open();
                return conexion;
            }
            catch (Exception ex)
            {
                // Es útil mostrar un mensaje si la red falla o el servidor está apagado
                MessageBox.Show($"Error de conexión con la base de datos:\n{ex.Message}",
                                "Error de Red", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Versión asíncrona de ObtenerConexion que abre la conexión sin bloquear el hilo UI.
        /// </summary>
        public async Task<SqlConnection> ObtenerConexionAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(cadenaConexion))
                {
                    throw new Exception("La base de datos no ha sido configurada. Por favor, reinicie la aplicación.");
                }

                SqlConnection conexion = new SqlConnection(cadenaConexion);
                await conexion.OpenAsync();
                return conexion;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión con la base de datos:\n{ex.Message}",
                                "Error de Red", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}



