using System.Collections.ObjectModel;
using POS.Helpers;
using POS.Models;
using POS.Services;

namespace POS.ViewModels;

public class ProductosViewModel : BaseViewModel
{
    private readonly ProductoService _service = new();
    private readonly CategoriaService _catService = new();

    private ObservableCollection<Producto> _productos = new();
    private Producto? _selectedProducto;
    private string _searchText = string.Empty;

    public ObservableCollection<Producto> Productos
    {
        get => _productos;
        set => SetProperty(ref _productos, value);
    }

    public Producto? SelectedProducto
    {
        get => _selectedProducto;
        set { SetProperty(ref _selectedProducto, value); UpdateCommands(); }
    }

    public string SearchText
    {
        get => _searchText;
        set { SetProperty(ref _searchText, value); CargarProductos(); }
    }

    public List<Categoria> Categorias { get; private set; } = new();

    public RelayCommand NuevoCommand { get; }
    public RelayCommand EditarCommand { get; }
    public RelayCommand EliminarCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public event Action<Producto?>? AbrirDialogo;
    public event Func<string, bool>? Confirmar;

    public ProductosViewModel()
    {
        NuevoCommand    = new RelayCommand(() => AbrirDialogo?.Invoke(null));
        EditarCommand   = new RelayCommand(() => AbrirDialogo?.Invoke(SelectedProducto),
                                           () => SelectedProducto != null);
        EliminarCommand = new RelayCommand(EjecutarEliminar,
                                           () => SelectedProducto != null);
        RefreshCommand  = new RelayCommand(CargarProductos);
        CargarDatos();
    }

    public void CargarDatos()
    {
        Categorias = _catService.GetAll();
        CargarProductos();
    }

    public void CargarProductos()
    {
        try
        {
            var lista = string.IsNullOrWhiteSpace(SearchText)
                ? _service.GetAll()
                : _service.Search(SearchText);
            Productos = new ObservableCollection<Producto>(lista);
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error al cargar productos.", ex);
            StatusMessage = "Error al cargar productos.";
        }
    }

    public bool GuardarProducto(Producto p)
    {
        try
        {
            if (p.IdProducto == 0)
                _service.Insert(p);
            else
                _service.Update(p);
            CargarProductos();
            return true;
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error al guardar producto.", ex);
            StatusMessage = "Error al guardar el producto.";
            return false;
        }
    }

    private void EjecutarEliminar()
    {
        if (SelectedProducto == null) return;
        bool confirm = Confirmar?.Invoke(
            $"¿Eliminar el producto '{SelectedProducto.Modelo}'?") ?? false;
        if (!confirm) return;

        try
        {
            _service.Delete(SelectedProducto.IdProducto);
            CargarProductos();
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error al eliminar producto.", ex);
            StatusMessage = "Error al eliminar el producto.";
        }
    }

    private void UpdateCommands()
    {
        EditarCommand.RaiseCanExecuteChanged();
        EliminarCommand.RaiseCanExecuteChanged();
    }
}
