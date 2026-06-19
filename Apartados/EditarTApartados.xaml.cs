using System.Windows;
using System.Linq;
using POS.Apartados;
using System.Collections.Generic;

namespace POS.Apartados
{
    public partial class EditarTApartado : Window
    {

        // Indicador para saber si estamos editando o insertando un nuevo apartado
        public tapartados Apartado { get; private set; }

        public List<string> ListaEstados { get; } = new List<string>
    {
        "Pagado", "Pendiente", "Cancelado", "En Proceso", "Entregado"
    };

        public bool IsEditEnabled => true; // Puedes personalizar según el modo

        private List<tapartados> _listaApartados;

        public EditarTApartado(List<tapartados> listaApartados, tapartados existente = null)
        {
            InitializeComponent();
            _listaApartados = listaApartados;

            if (existente != null)
            {
                Apartado = existente;
            }
            else
            {
                Apartado = new tapartados();
                Apartado.nApartadoID = ObtenerSiguienteID();
                Apartado.cEstado = "Pendiente"; // Valor predeterminado
            }

            DataContext = this; // Para que funcione el binding con ListaEstados y Apartado
        }


        private int ObtenerSiguienteID()
        {
            if (_listaApartados == null || _listaApartados.Count == 0)
                return 1;

            return _listaApartados.Max(a => a.nApartadoID) + 1;
        }

        private void Btn_Guardaar_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(Txt_Abono.Text, out double abono) && abono > 0)
            {
                if (abono > Apartado.nSaldoPendiente)
                {
                    MessageBox.Show("El abono no puede ser mayor al saldo pendiente.", "Abono inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Apartado.nAnticipo += abono;
                Apartado.nSaldoPendiente -= abono;

                if (Apartado.nSaldoPendiente <= 0)
                {
                    Apartado.nSaldoPendiente = 0;
                    Apartado.cEstado = "Pagado";
                }

                ApartadosDAL.ActualizarApartado(Apartado);

                MessageBox.Show("Abono guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Ingresa una cantidad válida para abonar.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        private void Btn_Cancelaar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
