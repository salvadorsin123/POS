using Microsoft.Data.Sqlite;
using POS.Data;
using POS.Models;

namespace POS.Services;

public class AuthService
{
    public Empleado? Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        const string sql = @"
SELECT e.IdEmpleado, e.Nombre, e.ApellidoPaterno, e.ApellidoMaterno,
       e.Username, e.PasswordHash, e.IdTipoEmpleado, t.Nombre AS NombreTipo,
       e.Salario, e.NumVentas, e.Activo, e.FechaRegistro, e.UltimoAcceso,
       e.Telefono, e.Correo
FROM TEmpleados e
INNER JOIN TTipoEmpleado t ON t.IdTipoEmpleado = e.IdTipoEmpleado
WHERE e.Username = @user AND e.Activo = 1";

        var empleado = DatabaseService.ExecuteReaderSingle(sql, MapEmpleado,
            new() { ["@user"] = username });

        if (empleado == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, empleado.PasswordHash)) return null;

        UpdateUltimoAcceso(empleado.IdEmpleado);
        return empleado;
    }

    public int AbrirSesion(int idEmpleado)
    {
        DatabaseService.ExecuteNonQuery(
            "INSERT INTO TSesionesEmpleado (IdEmpleado) VALUES (@id)",
            new() { ["@id"] = idEmpleado });

        return (int)(long)(DatabaseService.ExecuteScalar(
            "SELECT last_insert_rowid()") ?? 0L);
    }

    public void CerrarSesion(int idSesion)
    {
        DatabaseService.ExecuteNonQuery(
            "UPDATE TSesionesEmpleado SET FechaSalida=datetime('now','localtime') WHERE IdSesion=@id",
            new() { ["@id"] = idSesion });
    }

    private void UpdateUltimoAcceso(int idEmpleado)
    {
        DatabaseService.ExecuteNonQuery(
            "UPDATE TEmpleados SET UltimoAcceso=datetime('now','localtime') WHERE IdEmpleado=@id",
            new() { ["@id"] = idEmpleado });
    }

    private static Empleado MapEmpleado(SqliteDataReader r) => new()
    {
        IdEmpleado      = DatabaseService.GetInt(r, "IdEmpleado"),
        Nombre          = DatabaseService.GetString(r, "Nombre"),
        ApellidoPaterno = DatabaseService.GetStringNull(r, "ApellidoPaterno"),
        ApellidoMaterno = DatabaseService.GetStringNull(r, "ApellidoMaterno"),
        Username        = DatabaseService.GetString(r, "Username"),
        PasswordHash    = DatabaseService.GetString(r, "PasswordHash"),
        IdTipoEmpleado  = DatabaseService.GetInt(r, "IdTipoEmpleado"),
        NombreTipo      = DatabaseService.GetStringNull(r, "NombreTipo"),
        Salario         = DatabaseService.GetDecimal(r, "Salario"),
        NumVentas       = DatabaseService.GetInt(r, "NumVentas"),
        Activo          = DatabaseService.GetBool(r, "Activo"),
        Telefono        = DatabaseService.GetStringNull(r, "Telefono"),
        Correo          = DatabaseService.GetStringNull(r, "Correo"),
        UltimoAcceso    = DatabaseService.GetDateTimeNull(r, "UltimoAcceso"),
    };
}
