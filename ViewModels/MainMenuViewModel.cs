using POS.Helpers;
using POS.Services;

namespace POS.ViewModels;

public class MainMenuViewModel : BaseViewModel
{
    public string NombreUsuario =>
        SessionManager.CurrentUser?.NombreCompleto ?? "Usuario";

    public string RolUsuario =>
        SessionManager.CurrentUser?.NombreTipo ?? "—";

    public bool EsAdministrador => SessionManager.IsAdmin;

    public event Action? CerrarSesionRequested;

    public RelayCommand CerrarSesionCommand { get; }

    public MainMenuViewModel()
    {
        CerrarSesionCommand = new RelayCommand(EjecutarCerrarSesion);
    }

    private void EjecutarCerrarSesion()
    {
        try
        {
            var auth = new AuthService();
            auth.CerrarSesion(SessionManager.CurrentSessionId);
            LoggerService.Info($"Sesión cerrada: {SessionManager.CurrentUser?.Username}");
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error al cerrar sesión.", ex);
        }
        finally
        {
            SessionManager.ClearSession();
            CerrarSesionRequested?.Invoke();
        }
    }
}
