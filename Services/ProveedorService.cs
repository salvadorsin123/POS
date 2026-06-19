using Microsoft.Data.Sqlite;
using POS.Data;
using POS.Models;

namespace POS.Services;

public class ProveedorService
{
    public List<Proveedor> GetAll()
    {
        return DatabaseService.ExecuteReader(
            "SELECT * FROM TProveedores WHERE Activo=1 ORDER BY Nombre", MapProveedor);
    }

    public Proveedor? GetById(int id)
    {
        return DatabaseService.ExecuteReaderSingle(
            "SELECT * FROM TProveedores WHERE IdProveedor=@id",
            MapProveedor, new() { ["@id"] = id });
    }

    public int Insert(Proveedor p)
    {
        return DatabaseService.InsertAndGetId(
            @"INSERT INTO TProveedores(Nombre,Contacto,Telefono,Correo,Direccion,RFC)
              VALUES(@n,@con,@tel,@cor,@dir,@rfc)",
            new()
            {
                ["@n"]   = p.Nombre,
                ["@con"] = (object?)p.Contacto  ?? DBNull.Value,
                ["@tel"] = (object?)p.Telefono  ?? DBNull.Value,
                ["@cor"] = (object?)p.Correo    ?? DBNull.Value,
                ["@dir"] = (object?)p.Direccion ?? DBNull.Value,
                ["@rfc"] = (object?)p.RFC       ?? DBNull.Value,
            });
    }

    public void Update(Proveedor p)
    {
        DatabaseService.ExecuteNonQuery(
            @"UPDATE TProveedores SET Nombre=@n,Contacto=@con,Telefono=@tel,
              Correo=@cor,Direccion=@dir,RFC=@rfc WHERE IdProveedor=@id",
            new()
            {
                ["@n"]   = p.Nombre,
                ["@con"] = (object?)p.Contacto  ?? DBNull.Value,
                ["@tel"] = (object?)p.Telefono  ?? DBNull.Value,
                ["@cor"] = (object?)p.Correo    ?? DBNull.Value,
                ["@dir"] = (object?)p.Direccion ?? DBNull.Value,
                ["@rfc"] = (object?)p.RFC       ?? DBNull.Value,
                ["@id"]  = p.IdProveedor,
            });
    }

    public void Delete(int id)
    {
        DatabaseService.ExecuteNonQuery(
            "UPDATE TProveedores SET Activo=0 WHERE IdProveedor=@id",
            new() { ["@id"] = id });
    }

    private static Proveedor MapProveedor(SqliteDataReader r) => new()
    {
        IdProveedor   = DatabaseService.GetInt(r, "IdProveedor"),
        Nombre        = DatabaseService.GetString(r, "Nombre"),
        Contacto      = DatabaseService.GetStringNull(r, "Contacto"),
        Telefono      = DatabaseService.GetStringNull(r, "Telefono"),
        Correo        = DatabaseService.GetStringNull(r, "Correo"),
        Direccion     = DatabaseService.GetStringNull(r, "Direccion"),
        RFC           = DatabaseService.GetStringNull(r, "RFC"),
        Activo        = DatabaseService.GetBool(r, "Activo"),
        FechaRegistro = DatabaseService.GetDateTime(r, "FechaRegistro"),
    };
}
