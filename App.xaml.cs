using System.Windows;
using System.Windows.Threading;
using POS.Data;
using POS.Services;
using POS.Views;

namespace POS;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Captura global de excepciones no manejadas
        DispatcherUnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainException;

        try
        {
            LoggerService.Info("=== Iniciando Sistema POS ===");
            DatabaseInitializer.Initialize();
            LoggerService.Info("Base de datos lista.");

            var login = new LoginWindow();
            login.Show();
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error crítico al iniciar.", ex);
            MessageBox.Show(
                $"Error crítico al iniciar el sistema:\n\n{ex.Message}\n\nRevise el archivo log.txt.",
                "Error de Inicio", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LoggerService.Error("Excepción no manejada en UI.", e.Exception);
        MessageBox.Show(
            $"Se produjo un error inesperado:\n\n{e.Exception.Message}",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnDomainException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            LoggerService.Error("Excepción de dominio no manejada.", ex);
    }
}
