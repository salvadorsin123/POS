using POS.Helpers;
using POS.Services;

namespace POS.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private string _username = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _hasError;

    private readonly AuthService _authService = new();

    public string Username
    {
        get => _username;
        set { SetProperty(ref _username, value); ErrorMessage = string.Empty; HasError = false; }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { SetProperty(ref _errorMessage, value); }
    }

    public bool HasError
    {
        get => _hasError;
        set { SetProperty(ref _hasError, value); }
    }

    public event Action? LoginSuccessful;
    public event Action<string>? LoginFailed;

    public AsyncRelayCommand LoginCommand { get; }

    public LoginViewModel()
    {
        LoginCommand = new AsyncRelayCommand(
            ExecuteLogin,
            () => !IsBusy && !string.IsNullOrWhiteSpace(Username));
    }

    public async Task ExecuteLoginWithPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            ShowError("Ingrese su nombre de usuario.");
            return;
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            ShowError("Ingrese su contraseña.");
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        HasError = false;

        try
        {
            var empleado = await Task.Run(() => _authService.Login(Username, password));
            if (empleado == null)
            {
                ShowError("Usuario o contraseña incorrectos.");
                LoginFailed?.Invoke("Credenciales inválidas.");
                return;
            }

            int sessionId = await Task.Run(() => _authService.AbrirSesion(empleado.IdEmpleado));
            SessionManager.SetSession(empleado, sessionId);
            LoggerService.Info($"Login exitoso: {empleado.Username} ({empleado.NombreTipo})");
            LoginSuccessful?.Invoke();
        }
        catch (Exception ex)
        {
            LoggerService.Error("Error en Login.", ex);
            ShowError("Error inesperado. Revise el log.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task ExecuteLogin() => Task.CompletedTask;

    private void ShowError(string msg)
    {
        ErrorMessage = msg;
        HasError = true;
    }
}
