namespace POS.Models;

public class Venta
{
    public int IdVenta { get; set; }
    public int IdEmpleado { get; set; }
    public string? NombreEmpleado { get; set; }
    public int? IdCliente { get; set; }
    public string? NombreCliente { get; set; }
    public DateTime FechaVenta { get; set; } = DateTime.Now;
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal IVA { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Completada";
    public string? NumTicket { get; set; }
    public List<DetalleVenta> Detalles { get; set; } = new();
}

public class DetalleVenta
{
    public int IdDetalleVenta { get; set; }
    public int IdVenta { get; set; }
    public int IdProducto { get; set; }
    public string? NombreProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal Subtotal { get; set; }
}
