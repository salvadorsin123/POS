using System.Collections.ObjectModel;
using POS.Helpers;
using POS.Models;
using POS.Services;

namespace POS.ViewModels;

public class EmpleadosViewModel : BaseViewModel
{
    private readonly EmpleadoService _service = new();

    private ObservableCollection<Empleado> _empleados = new();
    private Empleado? _selected;

    public ObservableCollection<Empleado> Empleados
    {
        get => _empleados;
        set => SetProperty(ref _empleados, value);
    }

    public Empleado? SelectedEmpleado
    {
        get => _selected;
        set { SetProperty(ref _selected, value); UpdateCommands(); }
    }

    public List<TipoEmpleado> Tipos { get; private set; } = new();

    public RelayCommand NuevoCommand { get; }
    public RelayCommand EditarCommand { get; }
    public RelayCommand ToggleActivoCommand { get; }

    public event Action<Empleado?>? AbrirDialogo;
    public event Func<string, bool>? Confirmar;

    public EmpleadosViewModel()
    {
        NuevoCommand       = new RelayCommand(() => AbrirDialogo?.Invoke(null));
        EditarCommand      = new RelayCommand(() => AbrirDialogo?.Invoke(SelectedEmpleado),
                                              () => SelectedEmpleado != null);
        ToggleActivoCommand = new RelayCommand(EjecutarToggle,
                                              () => SelectedEmpleado != null);
        Tipos = _service.GetTipos();
        CargarEmpleados();
    }

    public void CargarEmpleados()
    {
        try
        {
            Empleados = new ObservableCollection<Empleado>(_service.GetAll());
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error al cargar empleados.", ex);
        }
    }

    public bool GuardarEmpleado(Empleado e, string? password)
    {
        try
        {
            if (e.IdEmpleado == 0)
            {
                if (string.IsNullOrEmpty(password))
                    throw new ArgumentException("La contraseña es requerida.");
                if (_service.UsernameExists(e.Username))
                    throw new InvalidOperationException("El nombre de usuario ya existe.");
                _service.Insert(e, password);
            }
            else
            {
                if (_service.UsernameExists(e.Username, e.IdEmpleado))
                    throw new InvalidOperationException("El nombre de usuario ya existe.");
                _service.Update(e, string.IsNullOrEmpty(password) ? null : password);
            }
            CargarEmpleados();
            return true;
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error al guardar empleado.", ex);
            StatusMessage = ex.Message;
            return false;
        }
    }

    private void EjecutarToggle()
    {
        if (SelectedEmpleado == null) return;
        string accion = SelectedEmpleado.Activo ? "desactivar" : "activar";
        bool ok = Confirmar?.Invoke(
            $"¿Desea {accion} a '{SelectedEmpleado.NombreCompleto}'?") ?? false;
        if (!ok) return;
        try
        {
            _service.ToggleActivo(SelectedEmpleado.IdEmpleado, !SelectedEmpleado.Activo);
            CargarEmpleados();
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error al cambiar estado de empleado.", ex);
        }
    }

    private void UpdateCommands()
    {
        EditarCommand.RaiseCanExecuteChanged();
        ToggleActivoCommand.RaiseCanExecuteChanged();
    }
}
