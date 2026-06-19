using System.Collections.ObjectModel;
using POS.Helpers;
using POS.Models;
using POS.Services;

namespace POS.ViewModels;

public class ApartadosViewModel : BaseViewModel
{
    private readonly ApartadoService _service = new();
    private ObservableCollection<Apartado> _apartados = new();
    private Apartado? _selected;
    private decimal _montoPago;

    public ObservableCollection<Apartado> Apartados
    {
        get => _apartados;
        set => SetProperty(ref _apartados, value);
    }

    public Apartado? SelectedApartado
    {
        get => _selected;
        set { SetProperty(ref _selected, value); UpdateCommands(); }
    }

    public decimal MontoPago
    {
        get => _montoPago;
        set => SetProperty(ref _montoPago, value);
    }

    public RelayCommand NuevoCommand { get; }
    public RelayCommand AplicarPagoCommand { get; }
    public RelayCommand CancelarApartadoCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public event Action? AbrirNuevoApartado;
    public event Func<string, bool>? Confirmar;
    public event Action<string>? MostrarError;

    public ApartadosViewModel()
    {
        NuevoCommand           = new RelayCommand(() => AbrirNuevoApartado?.Invoke());
        AplicarPagoCommand     = new RelayCommand(EjecutarPago,
                                                  () => SelectedApartado?.Estado == "Activo");
        CancelarApartadoCommand = new RelayCommand(EjecutarCancelar,
                                                  () => SelectedApartado?.Estado == "Activo");
        RefreshCommand         = new RelayCommand(CargarApartados);
        CargarApartados();
    }

    public void CargarApartados()
    {
        try
        {
            Apartados = new ObservableCollection<Apartado>(_service.GetAll());
        }
        catch (Exception ex) { LoggerService.Error("Error al cargar apartados.", ex); }
    }

    private void EjecutarPago()
    {
        if (SelectedApartado == null) return;
        if (MontoPago <= 0)
        {
            MostrarError?.Invoke("Ingrese un monto válido.");
            return;
        }
        if (MontoPago > SelectedApartado.SaldoPendiente)
        {
            MostrarError?.Invoke("El monto excede el saldo pendiente.");
            return;
        }
        bool ok = Confirmar?.Invoke(
            $"¿Aplicar pago de {MontoPago:C2} al apartado #{SelectedApartado.IdApartado}?") ?? false;
        if (!ok) return;
        try
        {
            _service.AplicarPago(SelectedApartado.IdApartado, MontoPago);
            MontoPago = 0;
            CargarApartados();
        }
        catch (Exception ex) { LoggerService.Error("Error al aplicar pago.", ex); }
    }

    private void EjecutarCancelar()
    {
        if (SelectedApartado == null) return;
        bool ok = Confirmar?.Invoke(
            $"¿Cancelar el apartado #{SelectedApartado.IdApartado}? Se restaurará el stock.") ?? false;
        if (!ok) return;
        try
        {
            _service.Cancelar(SelectedApartado.IdApartado);
            CargarApartados();
        }
        catch (Exception ex) { LoggerService.Error("Error al cancelar apartado.", ex); }
    }

    private void UpdateCommands()
    {
        AplicarPagoCommand.RaiseCanExecuteChanged();
        CancelarApartadoCommand.RaiseCanExecuteChanged();
    }
}
