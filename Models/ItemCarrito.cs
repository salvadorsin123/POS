using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POS.Models;

public class ItemCarrito : INotifyPropertyChanged
{
    private int _cantidad;
    private decimal _precioUnitario;
    private decimal _descuento;

    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }

    public int Cantidad
    {
        get => _cantidad;
        set { _cantidad = value; OnPropertyChanged(); OnPropertyChanged(nameof(Subtotal)); }
    }

    public decimal PrecioUnitario
    {
        get => _precioUnitario;
        set { _precioUnitario = value; OnPropertyChanged(); OnPropertyChanged(nameof(Subtotal)); }
    }

    public decimal Descuento
    {
        get => _descuento;
        set { _descuento = value; OnPropertyChanged(); OnPropertyChanged(nameof(Subtotal)); }
    }

    public decimal Subtotal => Cantidad * (PrecioUnitario - (PrecioUnitario * Descuento / 100m));

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
