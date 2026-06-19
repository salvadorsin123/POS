namespace POS.Models;

public class Servicio
{
    public int IdServicio { get; set; }
    public int IdProveedor { get; set; }
    public string? NombreProveedor { get; set; }
    public DateTime FechaPedido { get; set; } = DateTime.Now;
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string? Notas { get; set; }
    public List<DetalleServicio> Detalles { get; set; } = new();
}

public class DetalleServicio
{
    public int IdDetalle { get; set; }
    public int IdServicio { get; set; }
    public int IdProducto { get; set; }
    public string? NombreProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}
