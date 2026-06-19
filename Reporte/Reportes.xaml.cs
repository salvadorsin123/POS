using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using iTextSharp.text.pdf;
using iTextSharp.text;
using POS.ConexionBD;
using POS.Inicio_Sesion;
using Microsoft.Data.SqlClient;



namespace POS.Reporte
{
    /// <summary>
    /// Lógica de interacción para Reportes.xaml
    /// </summary>
    public partial class Reportes : Window
    {
        private Conexion conexionBD = new Conexion();
        private int empleadoID;
        private ReporteDAL reporteDA = new ReporteDAL();

        public Reportes(int empleadoID)
        {
            InitializeComponent();
            this.empleadoID = empleadoID;

            dpFechaInicio.SelectedDate = DateTime.Now;
            CargarReportesGenerados();
        }

        private void CargarReportesGenerados()
        {
            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                string query = "SELECT * FROM TReporte ORDER BY dFechaGeneracion DESC";
                SqlCommand cmd = new SqlCommand(query, conn);

                SqlDataReader reader = cmd.ExecuteReader();
                List<Reporte> reportes = new List<Reporte>();

                while (reader.Read())
                {
                    reportes.Add(new Reporte
                    {
                        ReporteID = Convert.ToInt32(reader["nReporteID"]),
                        TipoReporte = reader["cTipoReporte"].ToString(),
                        FechaGeneracion = Convert.ToDateTime(reader["dFechaGeneracion"]),
                        Periodo = reader["cPeriodo"].ToString(),
                        FechaInicio = Convert.ToDateTime(reader["dFechaInicio"]),
                        FechaFin = reader["dFechaFin"] != DBNull.Value ? Convert.ToDateTime(reader["dFechaFin"]) : (DateTime?)null,
                        NombreArchivo = reader["cNombreArchivo"].ToString(),
                        EmpleadoID = Convert.ToInt32(reader["nEmpleadoID"])
                    });
                }

                lstReportesGenerados.ItemsSource = reportes;
            }
        }

        private void chkRangoFechas_Checked(object sender, RoutedEventArgs e)
        {
            dpFechaFin.IsEnabled = true;
            dpFechaFin.SelectedDate = dpFechaInicio.SelectedDate?.AddDays(30);
        }

        private void chkRangoFechas_Unchecked(object sender, RoutedEventArgs e)
        {
            dpFechaFin.IsEnabled = false;
            dpFechaFin.SelectedDate = null;
        }

        private void GenerarReporte_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tipoReporte = ((ComboBoxItem)cmbTipoReporte.SelectedItem).Content.ToString();
                DateTime fechaInicio = dpFechaInicio.SelectedDate ?? DateTime.Now;
                DateTime? fechaFin = chkRangoFechas.IsChecked ?? false ? dpFechaFin.SelectedDate : null;

                string nombreArchivo = $"{tipoReporte.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string rutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reportes", nombreArchivo);

                // Crear directorio si no existe
                Directory.CreateDirectory(Path.GetDirectoryName(rutaArchivo));

                switch (tipoReporte)
                {
                    case "Ventas por Mes":
                        GenerarReporteVentasMes(fechaInicio, fechaFin, rutaArchivo);
                        break;

                    case "Compras a Proveedores":
                        GenerarReporteComprasProveedores(fechaInicio, fechaFin, rutaArchivo);
                        break;

                    case "Movimiento de Productos":
                        GenerarReporteMovimientoProductos(fechaInicio, fechaFin ?? fechaInicio.AddMonths(1).AddDays(-1), rutaArchivo);
                        break;

                    default:
                        MessageBox.Show("Tipo de reporte no implementado", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                }

                // Guardar registro del reporte
                var reporte = new Reporte
                {
                    TipoReporte = tipoReporte,
                    FechaGeneracion = DateTime.Now,
                    Periodo = fechaFin == null ? "Mes" : "Rango",
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    NombreArchivo = nombreArchivo,
                    EmpleadoID = empleadoID
                };

                reporteDA.GuardarReporte(reporte);
                CargarReportesGenerados();

                MessageBox.Show($"Reporte generado exitosamente: {rutaArchivo}", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerarReporteVentasMes(DateTime fechaInicio, DateTime? fechaFin, string rutaArchivo)
        {
            int mes = fechaInicio.Month;
            int año = fechaInicio.Year;

            var reporte = reporteDA.ObtenerVentasPorMes(mes, año);
            PdfGenerator.GenerarReporteVentasMes(reporte, rutaArchivo);
        }

        private void GenerarReporteComprasProveedores(DateTime fechaInicio, DateTime? fechaFin, string rutaArchivo)
        {
            int mes = fechaInicio.Month;
            int año = fechaInicio.Year;

            var reporte = reporteDA.ObtenerComprasProveedoresPorMes(mes, año);
            PdfGenerator.GenerarReporteComprasProveedores(reporte, rutaArchivo);
        }

        private void GenerarReporteMovimientoProductos(DateTime fechaInicio, DateTime fechaFin, string rutaArchivo)
        {
            var reporte = reporteDA.ObtenerMovimientoProductos(fechaInicio, fechaFin);
            PdfGenerator.GenerarReporteMovimientoProductos(reporte, rutaArchivo);
        }

        private void AbrirReporte_Click(object sender, RoutedEventArgs e)
        {
            if (lstReportesGenerados.SelectedItem is Reporte reporte)
            {
                string rutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reportes", reporte.NombreArchivo);

                if (File.Exists(rutaArchivo))
                {
                    Process.Start(rutaArchivo);
                }
                else
                {
                    MessageBox.Show("El archivo del reporte no se encuentra.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}