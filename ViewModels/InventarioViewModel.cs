using System.Collections.ObjectModel;
using POS.Helpers;
using POS.Models;
using POS.Services;

namespace POS.ViewModels;

public class InventarioViewModel : BaseViewModel
{
    private readonly CategoriaService _service = new();
    private ObservableCollection<Categoria> _categorias = new();
    private Categoria? _selected;
    private string _nombre = string.Empty;
    private string _descripcion = string.Empty;
    private bool _modoEdicion;

    public ObservableCollection<Categoria> Categorias
    {
        get => _categorias;
        set => SetProperty(ref _categorias, value);
    }

    public Categoria? SelectedCategoria
    {
        get => _selected;
        set { SetProperty(ref _selected, value); CargarFormulario(); UpdateCommands(); }
    }

    public string Nombre
    {
        get => _nombre;
        set => SetProperty(ref _nombre, value);
    }

    public string Descripcion
    {
        get => _descripcion;
        set => SetProperty(ref _descripcion, value);
    }

    public bool ModoEdicion
    {
        get => _modoEdicion;
        set => SetProperty(ref _modoEdicion, value);
    }

    public RelayCommand GuardarCommand { get; }
    public RelayCommand NuevoCommand { get; }
    public RelayCommand EliminarCommand { get; }

    public event Func<string, bool>? Confirmar;

    public InventarioViewModel()
    {
        GuardarCommand  = new RelayCommand(EjecutarGuardar,
                                           () => !string.IsNullOrWhiteSpace(Nombre));
        NuevoCommand    = new RelayCommand(LimpiarFormulario);
        EliminarCommand = new RelayCommand(EjecutarEliminar,
                                           () => SelectedCategoria != null);
        CargarCategorias();
    }

    public void CargarCategorias()
    {
        try { Categorias = new ObservableCollection<Categoria>(_service.GetAll()); }
        catch (Exception ex) { LoggerService.Error("Error al cargar categorías.", ex); }
    }

    private void CargarFormulario()
    {
        if (SelectedCategoria == null) return;
        Nombre      = SelectedCategoria.Nombre;
        Descripcion = SelectedCategoria.Descripcion ?? string.Empty;
        ModoEdicion = true;
    }

    private void LimpiarFormulario()
    {
        SelectedCategoria = null;
        Nombre      = string.Empty;
        Descripcion = string.Empty;
        ModoEdicion = false;
    }

    private void EjecutarGuardar()
    {
        try
        {
            var cat = new Categoria
            {
                IdCategoria = SelectedCategoria?.IdCategoria ?? 0,
                Nombre      = Nombre.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim(),
            };

            if (cat.IdCategoria == 0) _service.Insert(cat);
            else _service.Update(cat);

            LimpiarFormulario();
            CargarCategorias();
        }
        catch (Exception ex) { LoggerService.Error("Error al guardar categoría.", ex); }
    }

    private void EjecutarEliminar()
    {
        if (SelectedCategoria == null) return;
        bool ok = Confirmar?.Invoke(
            $"¿Eliminar la categoría '{SelectedCategoria.Nombre}'?") ?? false;
        if (!ok) return;
        try
        {
            _service.Delete(SelectedCategoria.IdCategoria);
            LimpiarFormulario();
            CargarCategorias();
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error al eliminar categoría.", ex);
            StatusMessage = "No se puede eliminar: tiene productos asociados.";
        }
    }

    private void UpdateCommands()
    {
        EliminarCommand.RaiseCanExecuteChanged();
        GuardarCommand.RaiseCanExecuteChanged();
    }
}
