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

namespace POS.Apartados
{
    /// <summary>
    /// Lógica de interacción para CrearTApartado.xaml
    /// </summary>
    public partial class CrearTApartado : Window
    {
        public double Abono { get; private set; }
        public double Total { get; set; }

        public CrearTApartado(double total)
        {
            InitializeComponent();
            Total = total;
        }

        private void Btn_Confirmar_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(Txt_Abono.Text, out double abono))
            {
                if (abono > Total)
                {
                    MessageBox.Show("El abono no puede ser mayor al total.");
                    return;
                }

                Abono = abono;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Ingresa un abono válido.");
            }
        }

        private void Btn_Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Txt_Abono_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !double.TryParse(((TextBox)sender).Text + e.Text, out _);
        }

        private void Txt_Abono_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(Txt_Abono.Text, out double abono))
                Lbl_SaldoPendiente.Content = $"Saldo pendiente: ${(Total - abono):0.00}";
        }
    }

}
