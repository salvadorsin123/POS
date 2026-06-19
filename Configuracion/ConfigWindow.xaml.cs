using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;
namespace POS
{
    /// <summary>
    /// Lógica de interacción para ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        // -------------------------------------------------------------
        // CONFIGURA AQUÍ TU USUARIO Y PASSWORD DE SQL SERVER (Paso 3 de la guía anterior)
        // -------------------------------------------------------------
        private const string DB_USER = "userpos2";
        private const string DB_PASS = "123";
        private const string DB_NAME = "MiPosDB";

        private string cadenaConexionTemporal = "";

        public ConfigWindow()
        {
            InitializeComponent();
        }

        // --- BOTONES DE MENÚ ---
        private void btnModoServidor_Click(object sender, RoutedEventArgs e)
        {
            PanelSeleccion.Visibility = Visibility.Collapsed;
            PanelServidor.Visibility = Visibility.Visible;

            // Lógica automática
            txtIPResult.Text = ObtenerIPLocal();
            AbrirPuertoFirewall();
        }

        private void btnModoCliente_Click(object sender, RoutedEventArgs e)
        {
            PanelSeleccion.Visibility = Visibility.Collapsed;
            PanelCliente.Visibility = Visibility.Visible;
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            PanelServidor.Visibility = Visibility.Collapsed;
            PanelCliente.Visibility = Visibility.Collapsed;
            PanelSeleccion.Visibility = Visibility.Visible;
            btnFinalizarCliente.IsEnabled = false;
        }

        // --- LÓGICA SERVIDOR ---
        private void btnGuardarServidor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Cambiar cursor a reloj de arena para indicar trabajo
                this.Cursor = System.Windows.Input.Cursors.Wait;

                // 2. EJECUTAR EL INSTALADOR
                // Esto crea la BD 'MiPosDB' y las tablas (Productos, Ventas, Usuarios)
                InstaladorDB.Instalar();

                // 3. Preparar la cadena de conexión final
                // Nota: Agregamos 'TrustServerCertificate=True' para evitar errores de SSL
                string cadenaFinal = "Server=localhost\\SQLEXPRESS; Database=MiPosDB; Integrated Security=True; Encrypt=True; TrustServerCertificate=True;";

                // 4. Guardar configuración y reiniciar app
                GuardarYReiniciar(cadenaFinal, true);
            }
            catch (Exception ex)
            {
                // Regresar cursor a normal y mostrar error
                this.Cursor = System.Windows.Input.Cursors.Arrow;
                MessageBox.Show($"Ocurrió un error al configurar la base de datos:\n{ex.Message}", "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // --- LÓGICA CLIENTE ---
        private void btnProbar_Click(object sender, RoutedEventArgs e)
        {
            string ip = txtIpServidorInput.Text.Trim();
            if (string.IsNullOrEmpty(ip))
            {
                MessageBox.Show("Escribe una IP primero.");
                return;
            }

            // Armamos la cadena remota
            cadenaConexionTemporal = $"Server={ip}\\SQLEXPRESS; Database={DB_NAME}; User Id={DB_USER}; Password={DB_PASS}; TrustServerCertificate=True;";

            if (ProbarConexion(cadenaConexionTemporal))
            {
                MessageBox.Show("¡Conexión Exitosa! Ya puedes guardar.");
                btnFinalizarCliente.IsEnabled = true;
            }
        }

        private void btnGuardarCliente_Click(object sender, RoutedEventArgs e)
        {
            GuardarYReiniciar(cadenaConexionTemporal, false);
        }

        // --- FUNCIONES AUXILIARES ---

        private void GuardarYReiniciar(string cadena, bool esServidor)
        {
            try
            {
                // Guardamos en Settings
                Properties.Settings.Default.CadenaConexion = cadena;
                Properties.Settings.Default.EsServidor = esServidor;
                Properties.Settings.Default.EstaConfigurado = true;
                Properties.Settings.Default.Save();

                MessageBox.Show("Configuración guardada correctamente.\nEl sistema se iniciará ahora.");

                // Abrimos la ventana principal
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar configuración: " + ex.Message);
            }
        }

        private bool ProbarConexion(string connStr)
        {
            try
            {
                // Timeout de 3 segundos para no congelar la app
                using (SqlConnection conn = new SqlConnection(connStr + ";Connection Timeout=3"))
                {
                    conn.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión:\n{ex.Message}\n\nVerifica:\n1. Que la IP sea correcta.\n2. Que el servidor tenga el Firewall configurado.");
                return false;
            }
        }

        private string ObtenerIPLocal()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        return ip.ToString();
                    }
                }
                return "No encontrada";
            }
            catch { return "Error"; }
        }

        private void AbrirPuertoFirewall()
        {
            // Solo funciona si el app corre como Administrador (app.manifest)
            try
            {
                string cmd = "netsh advfirewall firewall add rule name=\"POS SQL Server\" dir=in action=allow protocol=TCP localport=1433";
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + cmd);
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.Verb = "runas";
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            catch { /* Silencioso si falla o no es admin */ }
        }
    }
}

