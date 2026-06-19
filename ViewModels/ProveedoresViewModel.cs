using System.Collections.ObjectModel;
using POS.Helpers;
using POS.Models;
using POS.Services;

namespace POS.ViewModels;

public class ProveedoresViewModel : BaseViewModel
{
    private readonly ProveedorService _service = new();
    private ObservableCollection<Proveedor> _proveedores = new();
    private Proveedor? _selected;

    public ObservableCollection<Proveedor> Proveedores
    {
        get => _proveedores;
        set => SetProperty(ref _proveedores, value);
    }

    public Proveedor? SelectedProveedor
    {
        get => _selected;
        set { SetProperty(ref _selected, value); UpdateCommands(); }
    }

    public RelayCommand NuevoCommand { get; }
    public RelayCommand EditarCommand { get; }
    public RelayCommand EliminarCommand { get; }

    public event Action<Proveedor?>? AbrirDialogo;
    public event Func<string, bool>? Confirmar;

    public ProveedoresViewModel()
    {
        NuevoCommand    = new RelayCommand(() => AbrirDialogo?.Invoke(null));
        EditarCommand   = new RelayCommand(() => AbrirDialogo?.Invoke(SelectedProveedor),
                                           () => SelectedProveedor != null);
        EliminarCommand = new RelayCommand(EjecutarEliminar,
                                           () => SelectedProveedor != null);
        CargarProveedores();
    }

    public void CargarProveedores()
    {
        try { Proveedores = new ObservableCollection<Proveedor>(_service.GetAll()); }
        catch (Exception ex) { LoggerService.Error("Error al cargar proveedores.", ex); }
    }

    public bool GuardarProveedor(Proveedor p)
    {
        try
        {
            if (p.IdProveedor == 0) _service.Insert(p);
            else _service.Update(p);
            CargarProveedores();
            return true;
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error al guardar proveedor.", ex);
            return false;
        }
    }

    private void EjecutarEliminar()
    {
        if (SelectedProveedor == null) return;
        bool ok = Confirmar?.Invoke(
            $"¿Eliminar al proveedor '{SelectedProveedor.Nombre}'?") ?? false;
        if (!ok) return;
        try
        {
            _service.Delete(SelectedProveedor.IdProveedor);
            CargarProveedores();
        }
        catch (Exception ex) { LoggerService.Error("Error al eliminar proveedor.", ex); }
    }

    private void UpdateCommands()
    {
        EditarCommand.RaiseCanExecuteChanged();
        EliminarCommand.RaiseCanExecuteChanged();
    }
}
