using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Reporte
{
    public class Reporte
    {
        public int ReporteID { get; set; }
        public string TipoReporte { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public string Periodo { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string NombreArchivo { get; set; }
        public int EmpleadoID { get; set; }
    }

    public class ReporteVentasMes
    {
        public int Mes { get; set; }
        public int Año { get; set; }
        public decimal TotalVentas { get; set; }
        public int CantidadVentas { get; set; }
        public List<DetalleVentaMes> Detalles { get; set; }
    }

    public class DetalleVentaMes
    {
        public DateTime Fecha { get; set; }
        public decimal TotalDia { get; set; }
        public int CantidadVentasDia { get; set; }
    }

    public class ReporteComprasProveedores
    {
        public int Mes { get; set; }
        public int Año { get; set; }
        public decimal TotalCompras { get; set; }

        public List<DetalleCompraProveedor> Detalles { get; set; }
    }

    public class DetalleCompraProveedor
    {
        public string Proveedor { get; set; }
        public decimal TotalComprado { get; set; }
        public int CantidadCompras { get; set; }
        public decimal DeudaFinal { get; set; }
        public List<ProductoComprado> Productos { get; set; }
    }

    public class ProductoComprado
    {
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
    public class ReporteMovimientoProductos
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public List<MovimientoProducto> Productos { get; set; }
    }

    public class MovimientoProducto
    {
        public string Producto { get; set; }
        public string Modelo { get; set; }
        public int CantidadRecibida { get; set; }
        public int CantidadVendida { get; set; }
        public int StockActual { get; set; }
    }
    // Similar para otros tipos de reportes...
}
