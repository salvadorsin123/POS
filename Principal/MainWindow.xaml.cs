using POS.Inicio_Sesion;
using POS.Principal;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static POS.Principal.Menu_Principal;

namespace POS
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MostrarEstado();
            ConfigurarMenuAsync();
        }

        private void MostrarEstado()
        {
            string rol = Properties.Settings.Default.EsServidor ? "SERVIDOR" : "CLIENTE";
            this.Title = $"Mi POS - Modo: {rol}";

            // Ejemplo de uso de la conexión
            string cadena = Properties.Settings.Default.CadenaConexion;

        }
        private async Task ConfigurarMenuAsync()
        {
            // Mostrar ventana principal
            try
            {
                await Task.Delay(2000);
                Inicio inicioS = new Inicio();
                inicioS.Show();
                this.Close();
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "¿Estás seguro? Esto borrará la configuración de IP y Base de Datos y reiniciará la aplicación como si fuera nueva.",
                "Confirmar Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // 1. Borrar toda la configuración guardada (vuelve a los valores default: False/False)
                Properties.Settings.Default.Reset();
                Properties.Settings.Default.Save();

                // 2. Obtener la ruta del ejecutable actual (.exe)
                string nombreEjecutable = Process.GetCurrentProcess().MainModule.FileName;

                // 3. Iniciar una nueva instancia de la aplicación
                Process.Start(nombreEjecutable);

                // 4. Cerrar la instancia actual
                Application.Current.Shutdown();
            }
        }
    }
}