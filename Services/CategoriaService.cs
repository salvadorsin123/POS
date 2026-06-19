using Microsoft.Data.Sqlite;
using POS.Data;
using POS.Models;

namespace POS.Services;

public class CategoriaService
{
    public List<Categoria> GetAll()
    {
        return DatabaseService.ExecuteReader(
            "SELECT * FROM TCategorias ORDER BY Nombre", MapCategoria);
    }

    public Categoria? GetById(int id)
    {
        return DatabaseService.ExecuteReaderSingle(
            "SELECT * FROM TCategorias WHERE IdCategoria=@id",
            MapCategoria, new() { ["@id"] = id });
    }

    public int Insert(Categoria cat)
    {
        return DatabaseService.InsertAndGetId(
            "INSERT INTO TCategorias(Nombre,Descripcion) VALUES(@n,@d)",
            new() { ["@n"] = cat.Nombre, ["@d"] = cat.Descripcion });
    }

    public void Update(Categoria cat)
    {
        DatabaseService.ExecuteNonQuery(
            "UPDATE TCategorias SET Nombre=@n,Descripcion=@d WHERE IdCategoria=@id",
            new() { ["@n"] = cat.Nombre, ["@d"] = cat.Descripcion, ["@id"] = cat.IdCategoria });
    }

    public void Delete(int id)
    {
        DatabaseService.ExecuteNonQuery(
            "DELETE FROM TCategorias WHERE IdCategoria=@id",
            new() { ["@id"] = id });
    }

    private static Categoria MapCategoria(SqliteDataReader r) => new()
    {
        IdCategoria   = DatabaseService.GetInt(r, "IdCategoria"),
        Nombre        = DatabaseService.GetString(r, "Nombre"),
        Descripcion   = DatabaseService.GetStringNull(r, "Descripcion"),
        FechaCreacion = DatabaseService.GetDateTime(r, "FechaCreacion"),
    };
}
