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

namespace POS.Principal
{
    /// <summary>
    /// Lógica de interacción para ConfirmacionSalidaDialog.xaml
    /// </summary>
    public partial class ConfirmacionSalidaDialog : Window
    {
        public enum AccionSalida { CerrarSesion, SalirAplicacion, Continuar }
        public AccionSalida AccionSeleccionada { get; private set; } = AccionSalida.Continuar;

        public ConfirmacionSalidaDialog()
        {
            InitializeComponent();
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            AccionSeleccionada = AccionSalida.CerrarSesion;
            this.Close();
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            AccionSeleccionada = AccionSalida.SalirAplicacion;
            this.Close();
        }

        private void BtnContinuar_Click(object sender, RoutedEventArgs e)
        {
            AccionSeleccionada = AccionSalida.Continuar;
            this.Close();
        }
    }
}
