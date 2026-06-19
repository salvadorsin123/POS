using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using POS.Helpers;
using POS.Models;
using POS.Services;

namespace POS.ViewModels;

public class ReportesViewModel : BaseViewModel
{
    private readonly ReporteService _reporteSvc = new();
    private readonly PdfService     _pdfSvc     = new();

    private DateTime _desde = DateTime.Today.AddMonths(-1);
    private DateTime _hasta = DateTime.Today;
    private ObservableCollection<ResumenVenta> _resumenVentas = new();
    private ObservableCollection<MovimientoProducto> _movimientos = new();
    private ObservableCollection<Reporte> _historial = new();
    private int _tabIndex;

    public DateTime Desde
    {
        get => _desde;
        set => SetProperty(ref _desde, value);
    }

    public DateTime Hasta
    {
        get => _hasta;
        set => SetProperty(ref _hasta, value);
    }

    public ObservableCollection<ResumenVenta> ResumenVentas
    {
        get => _resumenVentas;
        set => SetProperty(ref _resumenVentas, value);
    }

    public ObservableCollection<MovimientoProducto> Movimientos
    {
        get => _movimientos;
        set => SetProperty(ref _movimientos, value);
    }

    public ObservableCollection<Reporte> Historial
    {
        get => _historial;
        set => SetProperty(ref _historial, value);
    }

    public int TabIndex
    {
        get => _tabIndex;
        set => SetProperty(ref _tabIndex, value);
    }

    public decimal TotalPeriodo => ResumenVentas.Sum(r => r.TotalIngresos);
    public int TotalTransacciones => ResumenVentas.Sum(r => r.TotalTransacciones);

    public AsyncRelayCommand GenerarReporteCommand { get; }
    public AsyncRelayCommand ExportarPDFCommand { get; }
    public RelayCommand<Reporte> AbrirArchivoCommand { get; }

    public event Action<string>? MostrarMensaje;

    public ReportesViewModel()
    {
        GenerarReporteCommand = new AsyncRelayCommand(GenerarReporte, () => !IsBusy);
        ExportarPDFCommand    = new AsyncRelayCommand(ExportarPdf, () => ResumenVentas.Count > 0 && !IsBusy);
        AbrirArchivoCommand   = new RelayCommand<Reporte>(AbrirArchivo);
        CargarHistorial();
    }

    public void CargarHistorial()
    {
        try
        {
            Historial = new ObservableCollection<Reporte>(_reporteSvc.GetHistorial());
        }
        catch (Exception ex) { LoggerService.Error("Error al cargar historial.", ex); }
    }

    private async Task GenerarReporte()
    {
        IsBusy = true;
        StatusMessage = "Generando reporte...";
        try
        {
            var resumen  = await Task.Run(() => _reporteSvc.GetResumenVentas(Desde, Hasta));
            var movs     = await Task.Run(() => _reporteSvc.GetMovimientosProducto(Desde, Hasta));
            ResumenVentas = new ObservableCollection<ResumenVenta>(resumen);
            Movimientos   = new ObservableCollection<MovimientoProducto>(movs);
            OnPropertyChanged(nameof(TotalPeriodo));
            OnPropertyChanged(nameof(TotalTransacciones));
            ExportarPDFCommand.RaiseCanExecuteChanged();
            StatusMessage = $"Reporte generado: {resumen.Count} días";
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error al generar reporte.", ex);
            StatusMessage = "Error al generar reporte.";
        }
        finally { IsBusy = false; }
    }

    private async Task ExportarPdf()
    {
        IsBusy = true;
        StatusMessage = "Exportando PDF...";
        try
        {
            string path = await Task.Run(() =>
                _pdfSvc.GenerarReporteVentas(ResumenVentas.ToList(), Desde, Hasta));

            _reporteSvc.GuardarReporte(new Reporte
            {
                Tipo        = "Ventas por Periodo",
                FechaInicio = Desde,
                FechaFin    = Hasta,
                RutaArchivo = path,
                IdEmpleado  = SessionManager.CurrentUser?.IdEmpleado,
            });

            CargarHistorial();
            MostrarMensaje?.Invoke($"PDF guardado en:\n{path}");
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error al exportar PDF.", ex);
            StatusMessage = "Error al exportar PDF.";
        }
        finally { IsBusy = false; }
    }

    private void AbrirArchivo(Reporte? r)
    {
        if (r?.RutaArchivo == null || !File.Exists(r.RutaArchivo)) return;
        try { Process.Start(new ProcessStartInfo(r.RutaArchivo) { UseShellExecute = true }); }
        catch (Exception ex) { LoggerService.Error("Error al abrir archivo.", ex); }
    }
}
