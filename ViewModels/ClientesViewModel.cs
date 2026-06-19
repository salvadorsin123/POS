using System.Collections.ObjectModel;
using POS.Helpers;
using POS.Models;
using POS.Services;

namespace POS.ViewModels;

public class ClientesViewModel : BaseViewModel
{
    private readonly ClienteService _service = new();

    private ObservableCollection<Cliente> _clientes = new();
    private Cliente? _selectedCliente;
    private string _searchText = string.Empty;

    public ObservableCollection<Cliente> Clientes
    {
        get => _clientes;
        set => SetProperty(ref _clientes, value);
    }

    public Cliente? SelectedCliente
    {
        get => _selectedCliente;
        set { SetProperty(ref _selectedCliente, value); UpdateCommands(); }
    }

    public string SearchText
    {
        get => _searchText;
        set { SetProperty(ref _searchText, value); CargarClientes(); }
    }

    public RelayCommand NuevoCommand { get; }
    public RelayCommand EditarCommand { get; }
    public RelayCommand EliminarCommand { get; }

    public event Action<Cliente?>? AbrirDialogo;
    public event Func<string, bool>? Confirmar;

    public ClientesViewModel()
    {
        NuevoCommand    = new RelayCommand(() => AbrirDialogo?.Invoke(null));
        EditarCommand   = new RelayCommand(() => AbrirDialogo?.Invoke(SelectedCliente),
                                           () => SelectedCliente != null);
        EliminarCommand = new RelayCommand(EjecutarEliminar,
                                           () => SelectedCliente != null);
        CargarClientes();
    }

    public void CargarClientes()
    {
        try
        {
            var lista = string.IsNullOrWhiteSpace(SearchText)
                ? _service.GetAll()
                : _service.Search(SearchText);
            Clientes = new ObservableCollection<Cliente>(lista);
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error al cargar clientes.", ex);
        }
    }

    public bool GuardarCliente(Cliente c)
    {
        try
        {
            if (c.IdCliente == 0) _service.Insert(c);
            else _service.Update(c);
            CargarClientes();
            return true;
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error al guardar cliente.", ex);
            return false;
        }
    }

    private void EjecutarEliminar()
    {
        if (SelectedCliente == null) return;
        bool ok = Confirmar?.Invoke(
            $"¿Eliminar al cliente '{SelectedCliente.NombreCompleto}'?") ?? false;
        if (!ok) return;
        try
        {
            _service.Delete(SelectedCliente.IdCliente);
            CargarClientes();
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error al eliminar cliente.", ex);
        }
    }

    private void UpdateCommands()
    {
        EditarCommand.RaiseCanExecuteChanged();
        EliminarCommand.RaiseCanExecuteChanged();
    }
}
