namespace POS.Models;

public class ProveedorProducto
{
    public int IdProveedorProducto { get; set; }
    public int IdProveedor { get; set; }
    public string? NombreProveedor { get; set; }
    public int IdProducto { get; set; }
    public string? NombreProducto { get; set; }
    public decimal PrecioCompra { get; set; }
    public DateTime FechaAsignacion { get; set; } = DateTime.Now;
}
