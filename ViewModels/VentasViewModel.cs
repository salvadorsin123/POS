using System.Collections.ObjectModel;
using POS.Helpers;
using POS.Models;
using POS.Services;

namespace POS.ViewModels;

public class VentasViewModel : BaseViewModel
{
    private readonly ProductoService _productoSvc = new();
    private readonly ClienteService  _clienteSvc  = new();
    private readonly VentaService    _ventaSvc    = new();
    private readonly PdfService      _pdfSvc      = new();
    private readonly PrintService    _printSvc    = new();

    private ObservableCollection<ItemCarrito> _carrito = new();
    private string _codigoBarras = string.Empty;
    private string _busquedaProducto = string.Empty;
    private Cliente? _clienteSeleccionado;
    private decimal _efectivoRecibido;
    private List<Producto> _productosDisponibles = new();
    private Producto? _productoSeleccionado;

    public ObservableCollection<ItemCarrito> Carrito
    {
        get => _carrito;
        set { SetProperty(ref _carrito, value); RecalcularTotales(); }
    }

    public string CodigoBarras
    {
        get => _codigoBarras;
        set { SetProperty(ref _codigoBarras, value); }
    }

    public string BusquedaProducto
    {
        get => _busquedaProducto;
        set { SetProperty(ref _busquedaProducto, value); BuscarProductos(); }
    }

    public Cliente? ClienteSeleccionado
    {
        get => _clienteSeleccionado;
        set => SetProperty(ref _clienteSeleccionado, value);
    }

    public decimal EfectivoRecibido
    {
        get => _efectivoRecibido;
        set { SetProperty(ref _efectivoRecibido, value); OnPropertyChanged(nameof(Cambio)); }
    }

    public Producto? ProductoSeleccionado
    {
        get => _productoSeleccionado;
        set => SetProperty(ref _productoSeleccionado, value);
    }

    public List<Producto> ProductosDisponibles
    {
        get => _productosDisponibles;
        set => SetProperty(ref _productosDisponibles, value);
    }

    public List<Cliente> ClientesDisponibles { get; private set; } = new();

    // Totales
    private decimal _subtotal, _descuento, _iva, _total;
    public decimal Subtotal { get => _subtotal; private set => SetProperty(ref _subtotal, value); }
    public decimal Descuento { get => _descuento; private set => SetProperty(ref _descuento, value); }
    public decimal IVA { get => _iva; private set => SetProperty(ref _iva, value); }
    public decimal Total { get => _total; private set { SetProperty(ref _total, value); OnPropertyChanged(nameof(Cambio)); } }
    public decimal Cambio => EfectivoRecibido - Total;
    public int TotalItems => Carrito.Sum(i => i.Cantidad);
    public bool CarritoVacio => Carrito.Count == 0;

    // Flags
    private bool _aplicarIva;
    public bool AplicarIva
    {
        get => _aplicarIva;
        set { SetProperty(ref _aplicarIva, value); RecalcularTotales(); }
    }

    public AsyncRelayCommand EscanearCommand { get; }
    public RelayCommand AgregarProductoCommand { get; }
    public RelayCommand<ItemCarrito> QuitarItemCommand { get; }
    public RelayCommand LimpiarCarritoCommand { get; }
    public AsyncRelayCommand ConfirmarVentaCommand { get; }

    public event Action<Venta>? VentaCompletada;
    public event Func<string, bool>? Confirmar;
    public event Action<string>? MostrarError;

    public VentasViewModel()
    {
        EscanearCommand       = new AsyncRelayCommand(EscanearProducto);
        AgregarProductoCommand = new RelayCommand(AgregarProductoSeleccionado,
                                                  () => ProductoSeleccionado != null);
        QuitarItemCommand     = new RelayCommand<ItemCarrito>(QuitarItem);
        LimpiarCarritoCommand = new RelayCommand(LimpiarCarrito, () => !CarritoVacio);
        ConfirmarVentaCommand = new AsyncRelayCommand(ConfirmarVenta, () => !CarritoVacio && !IsBusy);

        Carrito.CollectionChanged += (_, _) =>
        {
            RecalcularTotales();
            OnPropertyChanged(nameof(TotalItems));
            OnPropertyChanged(nameof(CarritoVacio));
        };

        CargarClientes();
    }

    public void CargarClientes()
    {
        try { ClientesDisponibles = _clienteSvc.GetAll(); }
        catch (Exception ex) { LoggerService.Error("Error al cargar clientes.", ex); }
    }

    private void BuscarProductos()
    {
        if (string.IsNullOrWhiteSpace(BusquedaProducto))
        {
            ProductosDisponibles = new();
            return;
        }
        try { ProductosDisponibles = _productoSvc.Search(BusquedaProducto); }
        catch (Exception ex) { LoggerService.Error("Error al buscar productos.", ex); }
    }

    private async Task EscanearProducto()
    {
        if (string.IsNullOrWhiteSpace(CodigoBarras)) return;
        var prod = await Task.Run(() => _productoSvc.GetByCodigoBarras(CodigoBarras));
        CodigoBarras = string.Empty;
        if (prod == null)
        {
            MostrarError?.Invoke("Producto no encontrado.");
            return;
        }
        AgregarAlCarrito(prod);
    }

    private void AgregarProductoSeleccionado()
    {
        if (ProductoSeleccionado == null) return;
        AgregarAlCarrito(ProductoSeleccionado);
        ProductoSeleccionado = null;
        BusquedaProducto = string.Empty;
    }

    public void AgregarAlCarrito(Producto prod)
    {
        if (prod.Cantidad <= 0)
        {
            MostrarError?.Invoke($"Sin stock: '{prod.Modelo}'.");
            return;
        }

        var item = Carrito.FirstOrDefault(i => i.IdProducto == prod.IdProducto);
        if (item != null)
        {
            if (item.Cantidad >= prod.Cantidad)
            {
                MostrarError?.Invoke($"Stock máximo alcanzado: {prod.Cantidad}");
                return;
            }
            item.Cantidad++;
        }
        else
        {
            Carrito.Add(new ItemCarrito
            {
                IdProducto      = prod.IdProducto,
                NombreProducto  = prod.Modelo,
                CodigoBarras    = prod.CodigoBarras,
                Cantidad        = 1,
                PrecioUnitario  = prod.PrecioFinal,
                Descuento       = prod.Descuento,
            });
        }
        RecalcularTotales();
    }

    private void QuitarItem(ItemCarrito? item)
    {
        if (item == null) return;
        if (item.Cantidad > 1) item.Cantidad--;
        else Carrito.Remove(item);
        RecalcularTotales();
    }

    private void LimpiarCarrito()
    {
        bool ok = Confirmar?.Invoke("¿Limpiar el carrito?") ?? true;
        if (ok) Carrito.Clear();
    }

    private void RecalcularTotales()
    {
        decimal sub = Carrito.Sum(i => i.Subtotal);
        decimal desc = Carrito.Sum(i => i.Cantidad * (i.PrecioUnitario * i.Descuento / 100m));
        decimal iva = AplicarIva ? sub * 0.16m : 0;
        Subtotal = sub;
        Descuento = desc;
        IVA = iva;
        Total = sub + iva;
        OnPropertyChanged(nameof(TotalItems));
        OnPropertyChanged(nameof(CarritoVacio));
        OnPropertyChanged(nameof(Cambio));
    }

    private async Task ConfirmarVenta()
    {
        if (CarritoVacio) return;
        if (EfectivoRecibido < Total)
        {
            MostrarError?.Invoke("El efectivo recibido es insuficiente.");
            return;
        }

        bool ok = Confirmar?.Invoke(
            $"¿Confirmar venta por {Total:C2}?") ?? false;
        if (!ok) return;

        IsBusy = true;
        try
        {
            var venta = new Venta
            {
                IdEmpleado = SessionManager.CurrentUser!.IdEmpleado,
                IdCliente  = ClienteSeleccionado?.IdCliente,
                Subtotal   = Subtotal,
                Descuento  = Descuento,
                IVA        = IVA,
                Total      = Total,
                Detalles   = Carrito.Select(i => new DetalleVenta
                {
                    IdProducto     = i.IdProducto,
                    NombreProducto = i.NombreProducto,
                    Cantidad       = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario,
                    Descuento      = i.Descuento,
                    Subtotal       = i.Subtotal,
                }).ToList()
            };

            int id = await Task.Run(() => _ventaSvc.RegistrarVenta(venta));
            venta = await Task.Run(() => _ventaSvc.GetById(id))!;

            string pdfPath = await Task.Run(() => _pdfSvc.GenerarTicket(venta));

            try { _printSvc.ImprimirTicket(venta); }
            catch { /* impresión opcional */ }

            VentaCompletada?.Invoke(venta);
            Carrito.Clear();
            ClienteSeleccionado = null;
            EfectivoRecibido = 0;
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error al registrar venta.", ex);
            MostrarError?.Invoke(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}

