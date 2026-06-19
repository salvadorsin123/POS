using Microsoft.Data.Sqlite;
using POS.Data;
using POS.Models;

namespace POS.Services;

public class ProductoService
{
    public List<Producto> GetAll(bool soloActivos = true)
    {
        string where = soloActivos ? "WHERE p.Activo=1" : string.Empty;
        return DatabaseService.ExecuteReader(
            $@"SELECT p.*, c.Nombre AS NombreCategoria
               FROM TProductos p
               LEFT JOIN TCategorias c ON c.IdCategoria=p.IdCategoria
               {where}
               ORDER BY p.Modelo", MapProducto);
    }

    public Producto? GetById(int id)
    {
        return DatabaseService.ExecuteReaderSingle(
            @"SELECT p.*, c.Nombre AS NombreCategoria
              FROM TProductos p
              LEFT JOIN TCategorias c ON c.IdCategoria=p.IdCategoria
              WHERE p.IdProducto=@id",
            MapProducto, new() { ["@id"] = id });
    }

    public Producto? GetByCodigoBarras(string codigo)
    {
        return DatabaseService.ExecuteReaderSingle(
            @"SELECT p.*, c.Nombre AS NombreCategoria
              FROM TProductos p
              LEFT JOIN TCategorias c ON c.IdCategoria=p.IdCategoria
              WHERE p.CodigoBarras=@cod AND p.Activo=1",
            MapProducto, new() { ["@cod"] = codigo });
    }

    public List<Producto> Search(string term)
    {
        string like = $"%{term}%";
        return DatabaseService.ExecuteReader(
            @"SELECT p.*, c.Nombre AS NombreCategoria
              FROM TProductos p
              LEFT JOIN TCategorias c ON c.IdCategoria=p.IdCategoria
              WHERE p.Activo=1 AND (p.Modelo LIKE @t OR p.CodigoBarras LIKE @t OR p.Descripcion LIKE @t)
              ORDER BY p.Modelo",
            MapProducto, new() { ["@t"] = like });
    }

    public int Insert(Producto p)
    {
        return DatabaseService.InsertAndGetId(
            @"INSERT INTO TProductos(Modelo,Precio,Descuento,Cantidad,IdCategoria,CodigoBarras,Descripcion,Activo)
              VALUES(@m,@pr,@d,@c,@ic,@cb,@desc,1)",
            new()
            {
                ["@m"]    = p.Modelo,
                ["@pr"]   = (double)p.Precio,
                ["@d"]    = (double)p.Descuento,
                ["@c"]    = p.Cantidad,
                ["@ic"]   = (object?)p.IdCategoria ?? DBNull.Value,
                ["@cb"]   = (object?)p.CodigoBarras ?? DBNull.Value,
                ["@desc"] = (object?)p.Descripcion ?? DBNull.Value,
            });
    }

    public void Update(Producto p)
    {
        DatabaseService.ExecuteNonQuery(
            @"UPDATE TProductos SET Modelo=@m,Precio=@pr,Descuento=@d,Cantidad=@c,
              IdCategoria=@ic,CodigoBarras=@cb,Descripcion=@desc,
              FechaActualizacion=datetime('now','localtime')
              WHERE IdProducto=@id",
            new()
            {
                ["@m"]    = p.Modelo,
                ["@pr"]   = (double)p.Precio,
                ["@d"]    = (double)p.Descuento,
                ["@c"]    = p.Cantidad,
                ["@ic"]   = (object?)p.IdCategoria ?? DBNull.Value,
                ["@cb"]   = (object?)p.CodigoBarras ?? DBNull.Value,
                ["@desc"] = (object?)p.Descripcion ?? DBNull.Value,
                ["@id"]   = p.IdProducto,
            });
    }

    public void Delete(int id)
    {
        DatabaseService.ExecuteNonQuery(
            "UPDATE TProductos SET Activo=0 WHERE IdProducto=@id",
            new() { ["@id"] = id });
    }

    public void UpdateStock(int idProducto, int delta)
    {
        DatabaseService.ExecuteNonQuery(
            "UPDATE TProductos SET Cantidad=Cantidad+@delta WHERE IdProducto=@id",
            new() { ["@delta"] = delta, ["@id"] = idProducto });
    }

    private static Producto MapProducto(SqliteDataReader r) => new()
    {
        IdProducto          = DatabaseService.GetInt(r, "IdProducto"),
        Modelo              = DatabaseService.GetString(r, "Modelo"),
        Precio              = DatabaseService.GetDecimal(r, "Precio"),
        Descuento           = DatabaseService.GetDecimal(r, "Descuento"),
        Cantidad            = DatabaseService.GetInt(r, "Cantidad"),
        IdCategoria         = DatabaseService.GetIntNull(r, "IdCategoria"),
        NombreCategoria     = DatabaseService.GetStringNull(r, "NombreCategoria"),
        CodigoBarras        = DatabaseService.GetStringNull(r, "CodigoBarras"),
        Descripcion         = DatabaseService.GetStringNull(r, "Descripcion"),
        Activo              = DatabaseService.GetBool(r, "Activo"),
        FechaCreacion       = DatabaseService.GetDateTime(r, "FechaCreacion"),
        FechaActualizacion  = DatabaseService.GetDateTimeNull(r, "FechaActualizacion"),
    };
}
