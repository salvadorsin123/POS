using Microsoft.Data.Sqlite;
using POS.Data;
using POS.Models;

namespace POS.Services;

public class EmpleadoService
{
    public List<Empleado> GetAll()
    {
        return DatabaseService.ExecuteReader(
            @"SELECT e.*, t.Nombre AS NombreTipo
              FROM TEmpleados e
              INNER JOIN TTipoEmpleado t ON t.IdTipoEmpleado=e.IdTipoEmpleado
              ORDER BY e.Nombre", MapEmpleado);
    }

    public List<TipoEmpleado> GetTipos()
    {
        return DatabaseService.ExecuteReader(
            "SELECT * FROM TTipoEmpleado ORDER BY IdTipoEmpleado",
            r => new TipoEmpleado
            {
                IdTipoEmpleado = DatabaseService.GetInt(r, "IdTipoEmpleado"),
                Nombre         = DatabaseService.GetString(r, "Nombre"),
                Descripcion    = DatabaseService.GetStringNull(r, "Descripcion"),
            });
    }

    public Empleado? GetById(int id)
    {
        return DatabaseService.ExecuteReaderSingle(
            @"SELECT e.*, t.Nombre AS NombreTipo
              FROM TEmpleados e
              INNER JOIN TTipoEmpleado t ON t.IdTipoEmpleado=e.IdTipoEmpleado
              WHERE e.IdEmpleado=@id",
            MapEmpleado, new() { ["@id"] = id });
    }

    public bool UsernameExists(string username, int excludeId = 0)
    {
        var result = DatabaseService.ExecuteScalar(
            "SELECT COUNT(*) FROM TEmpleados WHERE Username=@u AND IdEmpleado!=@id",
            new() { ["@u"] = username, ["@id"] = excludeId });
        return (long)(result ?? 0L) > 0;
    }

    public int Insert(Empleado e, string plainPassword)
    {
        string hash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
        int id = DatabaseService.InsertAndGetId(
            @"INSERT INTO TEmpleados(Nombre,ApellidoPaterno,ApellidoMaterno,Telefono,Correo,
              Username,PasswordHash,IdTipoEmpleado,Salario)
              VALUES(@n,@ap,@am,@tel,@cor,@u,@ph,@tipo,@sal)",
            new()
            {
                ["@n"]    = e.Nombre,
                ["@ap"]   = (object?)e.ApellidoPaterno ?? DBNull.Value,
                ["@am"]   = (object?)e.ApellidoMaterno ?? DBNull.Value,
                ["@tel"]  = (object?)e.Telefono        ?? DBNull.Value,
                ["@cor"]  = (object?)e.Correo          ?? DBNull.Value,
                ["@u"]    = e.Username,
                ["@ph"]   = hash,
                ["@tipo"] = e.IdTipoEmpleado,
                ["@sal"]  = (double)e.Salario,
            });

        string rol = e.IdTipoEmpleado == 1 ? "Administrador" : "Cajero";
        DatabaseService.ExecuteNonQuery(
            "INSERT OR IGNORE INTO TUsuarios(IdEmpleado,Username,PasswordHash,Rol) VALUES(@id,@u,@ph,@rol)",
            new() { ["@id"] = id, ["@u"] = e.Username, ["@ph"] = hash, ["@rol"] = rol });

        return id;
    }

    public void Update(Empleado e, string? newPassword = null)
    {
        if (!string.IsNullOrEmpty(newPassword))
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            DatabaseService.ExecuteNonQuery(
                @"UPDATE TEmpleados SET Nombre=@n,ApellidoPaterno=@ap,ApellidoMaterno=@am,
                  Telefono=@tel,Correo=@cor,Username=@u,PasswordHash=@ph,
                  IdTipoEmpleado=@tipo,Salario=@sal WHERE IdEmpleado=@id",
                new()
                {
                    ["@n"]    = e.Nombre,
                    ["@ap"]   = (object?)e.ApellidoPaterno ?? DBNull.Value,
                    ["@am"]   = (object?)e.ApellidoMaterno ?? DBNull.Value,
                    ["@tel"]  = (object?)e.Telefono        ?? DBNull.Value,
                    ["@cor"]  = (object?)e.Correo          ?? DBNull.Value,
                    ["@u"]    = e.Username,
                    ["@ph"]   = hash,
                    ["@tipo"] = e.IdTipoEmpleado,
                    ["@sal"]  = (double)e.Salario,
                    ["@id"]   = e.IdEmpleado,
                });
            DatabaseService.ExecuteNonQuery(
                "UPDATE TUsuarios SET PasswordHash=@ph WHERE IdEmpleado=@id",
                new() { ["@ph"] = hash, ["@id"] = e.IdEmpleado });
        }
        else
        {
            DatabaseService.ExecuteNonQuery(
                @"UPDATE TEmpleados SET Nombre=@n,ApellidoPaterno=@ap,ApellidoMaterno=@am,
                  Telefono=@tel,Correo=@cor,Username=@u,IdTipoEmpleado=@tipo,Salario=@sal
                  WHERE IdEmpleado=@id",
                new()
                {
                    ["@n"]    = e.Nombre,
                    ["@ap"]   = (object?)e.ApellidoPaterno ?? DBNull.Value,
                    ["@am"]   = (object?)e.ApellidoMaterno ?? DBNull.Value,
                    ["@tel"]  = (object?)e.Telefono        ?? DBNull.Value,
                    ["@cor"]  = (object?)e.Correo          ?? DBNull.Value,
                    ["@u"]    = e.Username,
                    ["@tipo"] = e.IdTipoEmpleado,
                    ["@sal"]  = (double)e.Salario,
                    ["@id"]   = e.IdEmpleado,
                });
        }
    }

    public void ToggleActivo(int id, bool activo)
    {
        DatabaseService.ExecuteNonQuery(
            "UPDATE TEmpleados SET Activo=@a WHERE IdEmpleado=@id",
            new() { ["@a"] = activo ? 1 : 0, ["@id"] = id });
    }

    private static Empleado MapEmpleado(SqliteDataReader r) => new()
    {
        IdEmpleado      = DatabaseService.GetInt(r, "IdEmpleado"),
        Nombre          = DatabaseService.GetString(r, "Nombre"),
        ApellidoPaterno = DatabaseService.GetStringNull(r, "ApellidoPaterno"),
        ApellidoMaterno = DatabaseService.GetStringNull(r, "ApellidoMaterno"),
        Telefono        = DatabaseService.GetStringNull(r, "Telefono"),
        Correo          = DatabaseService.GetStringNull(r, "Correo"),
        Username        = DatabaseService.GetString(r, "Username"),
        PasswordHash    = DatabaseService.GetString(r, "PasswordHash"),
        IdTipoEmpleado  = DatabaseService.GetInt(r, "IdTipoEmpleado"),
        NombreTipo      = DatabaseService.GetStringNull(r, "NombreTipo"),
        Salario         = DatabaseService.GetDecimal(r, "Salario"),
        NumVentas       = DatabaseService.GetInt(r, "NumVentas"),
        Activo          = DatabaseService.GetBool(r, "Activo"),
        FechaRegistro   = DatabaseService.GetDateTime(r, "FechaRegistro"),
        UltimoAcceso    = DatabaseService.GetDateTimeNull(r, "UltimoAcceso"),
    };
}
