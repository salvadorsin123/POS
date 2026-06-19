using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using POS.ConexionBD;
using POS.Detalle_Venta;
using Microsoft.Data.SqlClient;

namespace POS.Ventas
{
    public partial class Detalle_Venta : Window
    {
        public Detalle_Venta()
        {
            InitializeComponent();
            CargarDatosAgrupados();
        }

        private void CargarDatosAgrupados()
        {
            try
            {
                DataTable dt = Detalle_VentaDAL.ObtenerTodosLosDetallesDeVenta();
                CollectionViewSource cvs = (CollectionViewSource)this.FindResource("DetalleVentaCVS");
                cvs.Source = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los detalles de venta: " + ex.Message);
            }
        }


        private void Btn_GuardarDetalle_Click(object sender, RoutedEventArgs e)
        {
            // Implementación de guardar detalle si se desea agregar desde esta vista
        }

        private void Btn_CancelarDetalle_Click(object sender, RoutedEventArgs e)
        {
            // Opcional: lógica para cerrar o limpiar la vista
        }
    }
}
