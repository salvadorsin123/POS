using POS.Models;

namespace POS.Helpers;

public static class SessionManager
{
    public static Empleado? CurrentUser { get; private set; }
    public static int CurrentSessionId { get; private set; }

    public static bool IsLoggedIn => CurrentUser != null;
    public static bool IsAdmin => CurrentUser?.EsAdministrador ?? false;

    public static void SetSession(Empleado empleado, int sessionId)
    {
        CurrentUser = empleado;
        CurrentSessionId = sessionId;
    }

    public static void ClearSession()
    {
        CurrentUser = null;
        CurrentSessionId = 0;
    }
}
