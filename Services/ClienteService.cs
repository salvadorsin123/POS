using Microsoft.Data.Sqlite;
using POS.Data;
using POS.Models;

namespace POS.Services;

public class ClienteService
{
    public List<Cliente> GetAll()
    {
        return DatabaseService.ExecuteReader(
            "SELECT * FROM TCliente WHERE Activo=1 ORDER BY Nombre", MapCliente);
    }

    public Cliente? GetById(int id)
    {
        return DatabaseService.ExecuteReaderSingle(
            "SELECT * FROM TCliente WHERE IdCliente=@id",
            MapCliente, new() { ["@id"] = id });
    }

    public List<Cliente> Search(string term)
    {
        string like = $"%{term}%";
        return DatabaseService.ExecuteReader(
            @"SELECT * FROM TCliente
              WHERE Activo=1 AND (Nombre LIKE @t OR ApellidoPaterno LIKE @t OR Telefono LIKE @t)
              ORDER BY Nombre",
            MapCliente, new() { ["@t"] = like });
    }

    public int Insert(Cliente c)
    {
        return DatabaseService.InsertAndGetId(
            @"INSERT INTO TCliente(Nombre,ApellidoPaterno,ApellidoMaterno,Correo,Telefono,Direccion)
              VALUES(@n,@ap,@am,@co,@tel,@dir)",
            new()
            {
                ["@n"]   = c.Nombre,
                ["@ap"]  = (object?)c.ApellidoPaterno  ?? DBNull.Value,
                ["@am"]  = (object?)c.ApellidoMaterno  ?? DBNull.Value,
                ["@co"]  = (object?)c.Correo           ?? DBNull.Value,
                ["@tel"] = (object?)c.Telefono         ?? DBNull.Value,
                ["@dir"] = (object?)c.Direccion        ?? DBNull.Value,
            });
    }

    public void Update(Cliente c)
    {
        DatabaseService.ExecuteNonQuery(
            @"UPDATE TCliente SET Nombre=@n,ApellidoPaterno=@ap,ApellidoMaterno=@am,
              Correo=@co,Telefono=@tel,Direccion=@dir WHERE IdCliente=@id",
            new()
            {
                ["@n"]   = c.Nombre,
                ["@ap"]  = (object?)c.ApellidoPaterno  ?? DBNull.Value,
                ["@am"]  = (object?)c.ApellidoMaterno  ?? DBNull.Value,
                ["@co"]  = (object?)c.Correo           ?? DBNull.Value,
                ["@tel"] = (object?)c.Telefono         ?? DBNull.Value,
                ["@dir"] = (object?)c.Direccion        ?? DBNull.Value,
                ["@id"]  = c.IdCliente,
            });
    }

    public void Delete(int id)
    {
        DatabaseService.ExecuteNonQuery(
            "UPDATE TCliente SET Activo=0 WHERE IdCliente=@id",
            new() { ["@id"] = id });
    }

    private static Cliente MapCliente(SqliteDataReader r) => new()
    {
        IdCliente       = DatabaseService.GetInt(r, "IdCliente"),
        Nombre          = DatabaseService.GetString(r, "Nombre"),
        ApellidoPaterno = DatabaseService.GetStringNull(r, "ApellidoPaterno"),
        ApellidoMaterno = DatabaseService.GetStringNull(r, "ApellidoMaterno"),
        Correo          = DatabaseService.GetStringNull(r, "Correo"),
        Telefono        = DatabaseService.GetStringNull(r, "Telefono"),
        Direccion       = DatabaseService.GetStringNull(r, "Direccion"),
        FechaRegistro   = DatabaseService.GetDateTime(r, "FechaRegistro"),
        Activo          = DatabaseService.GetBool(r, "Activo"),
    };
}
