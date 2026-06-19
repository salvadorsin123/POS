using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using POS.Apartados;
using POS.Inicio_Sesion;

namespace POS.Apartados
{
    public partial class Apartados : UserControl
    {
        private List<tapartados> listaApartados;
        private List<tapartados> listaApartadosCompleta;

        public Apartados()
        {
            InitializeComponent();
            CargarApartados();
            ConfigurarMenu();
        }

        private void ConfigurarMenu()
        {
            if (Sesion.Tipo == "Empleado")
            {
                // Ocultar botones de agregar y eliminar
                Btn_EliminarApartado.Visibility = Visibility.Collapsed;
                Btn_Abonar.Visibility = Visibility.Visible; // Mostrar solo el botón de abonar
            }


            else if (Sesion.Tipo == "Administrador")
            {
            }
            else
            {
                MessageBox.Show("Tipo de usuario no reconocido.");
            }
        }

        private void CargarApartados()
        {
            // Crear una instancia de la clase DAL
            ApartadosDAL ApartadosDAL = new ApartadosDAL();

            // Obtener los apartados desde la base de datos
            listaApartados = ApartadosDAL.ObtenerApartados();

            // Copiar la lista original a la lista completa
            listaApartadosCompleta = new List<tapartados>(listaApartados);

            // Asignar la lista de apartados al DataGrid
            DataGrid_Apartados.ItemsSource = listaApartados;
        }

        private void Txt_BuscarApartado_TextChanged(object sender, TextChangedEventArgs e)
        {
            FiltrarApartados();
        }
        private void FiltrarApartados()
        {
            string textoBusqueda = Txt_BuscarApartado.Text.Trim();

            if (string.IsNullOrEmpty(textoBusqueda))
            {
                // Si no hay texto, mostrar todos los apartados
                listaApartados = new List<tapartados>(listaApartadosCompleta);
            }
            else
            {
                // Filtrar SOLO por nClienteID (convertido a string para comparar)
                listaApartados = listaApartadosCompleta
                    .Where(a => a.cNombreCliente.ToLower().Contains(textoBusqueda.ToLower()) ||
                        a.nClienteID.ToString().Contains(textoBusqueda))
                    .ToList();
            }

            RefrescarDataGrid();
        }

        //private void Btn_AgregarApartado_Click(object sender, RoutedEventArgs e)
        //{
        //    var ventana = new EditarTApartado(listaApartados);
        //    if (ventana.ShowDialog() == true)
        //    {
        //        var nuevoApartado = ventana.Apartado;
        //        // Insertar el apartado en la base de datos y obtener el ID generado
        //        int nuevoID = ApartadosDAL.InsertarApartado(nuevoApartado);
        //        nuevoApartado.nApartadoID = nuevoID;

        //        // Insertar los productos asociados (detalles del apartado)
        //        if (nuevoApartado.Detalles != null && nuevoApartado.Detalles.Count > 0)
        //        {
        //            ApartadosDAL.InsertarDetalle(nuevoApartado.nApartadoID, nuevoApartado.Detalles);
        //        }

        //        listaApartados.Add(nuevoApartado);
        //        RefrescarDataGrid();
        //    }
        //}



        private void Btn_Abonar_Click(object sender, RoutedEventArgs e)
        {
            if (DataGrid_Apartados.SelectedItem is tapartados seleccionado)
            {
                var ventana = new EditarTApartado(listaApartados, seleccionado);
                if (ventana.ShowDialog() == true)
                {
                    var apartadoEditado = ventana.Apartado;
                    // Actualizar el apartado en la base de datos
                    ApartadosDAL.ActualizarApartado(apartadoEditado);

                    // Eliminar los detalles actuales y agregar los nuevos si es necesario
                    ApartadosDAL.EliminarDetallesApartado(apartadoEditado.nApartadoID);
                    if (apartadoEditado.Detalles != null && apartadoEditado.Detalles.Count > 0)
                    {
                        ApartadosDAL.InsertarDetalle(apartadoEditado.nApartadoID, apartadoEditado.Detalles);
                    }

                    RefrescarDataGrid();
                }
            }
            else
            {
                MessageBox.Show("Selecciona un apartado para editar.");
            }
        }


        private void Btn_EliminarApartado_Click(object sender, RoutedEventArgs e)
        {
            if (DataGrid_Apartados.SelectedItem is tapartados seleccionado)
            {
                if (MessageBox.Show("¿Seguro que deseas eliminar este apartado?", "Confirmación", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // Eliminar los detalles asociados
                    ApartadosDAL.EliminarDetallesApartado(seleccionado.nApartadoID);
                    // Eliminar el apartado de la base de datos
                    ApartadosDAL.EliminarApartado(seleccionado.nApartadoID);

                    // Eliminar el apartado de la lista
                    listaApartados.Remove(seleccionado);
                    RefrescarDataGrid();
                }
            }
            else
            {
                MessageBox.Show("Selecciona un apartado para eliminar.");
            }
        }


        private void RefrescarDataGrid()
        {
            DataGrid_Apartados.ItemsSource = null;
            DataGrid_Apartados.ItemsSource = listaApartados;
        }
    }
}
