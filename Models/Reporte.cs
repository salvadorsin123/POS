namespace POS.Models;

public class Reporte
{
    public int IdReporte { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateTime FechaGeneracion { get; set; } = DateTime.Now;
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? RutaArchivo { get; set; }
    public int? IdEmpleado { get; set; }
    public string? NombreEmpleado { get; set; }
}

public class ResumenVenta
{
    public DateTime Fecha { get; set; }
    public int TotalTransacciones { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalDescuentos { get; set; }
}

public class MovimientoProducto
{
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal TotalGenerado { get; set; }
    public int StockActual { get; set; }
}

public class Devolucion
{
    public int IdDevolucion { get; set; }
    public int IdVenta { get; set; }
    public int IdEmpleado { get; set; }
    public string? NombreEmpleado { get; set; }
    public DateTime FechaDevolucion { get; set; } = DateTime.Now;
    public string? Motivo { get; set; }
    public decimal Total { get; set; }
}

public class MovimientoCaja
{
    public int IdMovimiento { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? Descripcion { get; set; }
    public DateTime FechaMovimiento { get; set; } = DateTime.Now;
    public int? IdEmpleado { get; set; }
}
