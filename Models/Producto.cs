namespace POS.Models;

public class Producto
{
    public int IdProducto { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public decimal Descuento { get; set; }
    public int Cantidad { get; set; }
    public int? IdCategoria { get; set; }
    public string? NombreCategoria { get; set; }
    public string? CodigoBarras { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }

    public decimal PrecioFinal => Precio - (Precio * Descuento / 100m);

    public override string ToString() => Modelo;
}
