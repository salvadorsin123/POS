namespace POS.Models;

public class Apartado
{
    public int IdApartado { get; set; }
    public int IdCliente { get; set; }
    public string? NombreCliente { get; set; }
    public int IdEmpleado { get; set; }
    public string? NombreEmpleado { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaLimite { get; set; }
    public decimal Total { get; set; }
    public decimal Anticipo { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string Estado { get; set; } = "Activo";
    public string? Notas { get; set; }
    public List<DetalleApartado> Detalles { get; set; } = new();
}

public class DetalleApartado
{
    public int IdDetalleApartado { get; set; }
    public int IdApartado { get; set; }
    public int IdProducto { get; set; }
    public string? NombreProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
