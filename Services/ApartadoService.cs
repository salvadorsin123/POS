using Microsoft.Data.Sqlite;
using POS.Data;
using POS.Models;

namespace POS.Services;

public class ApartadoService
{
    public int RegistrarApartado(Apartado apartado)
    {
        using var conn = DatabaseService.GetConnection();
        using var tx = conn.BeginTransaction();
        try
        {
            using var cmd = new SqliteCommand(
                @"INSERT INTO TApartados(IdCliente,IdEmpleado,FechaLimite,Total,Anticipo,SaldoPendiente,Estado,Notas)
                  VALUES(@cli,@emp,@fl,@tot,@ant,@sal,'Activo',@notas)", conn, tx);
            cmd.Parameters.AddWithValue("@cli",   apartado.IdCliente);
            cmd.Parameters.AddWithValue("@emp",   apartado.IdEmpleado);
            cmd.Parameters.AddWithValue("@fl",    apartado.FechaLimite?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@tot",   (double)apartado.Total);
            cmd.Parameters.AddWithValue("@ant",   (double)apartado.Anticipo);
            cmd.Parameters.AddWithValue("@sal",   (double)apartado.SaldoPendiente);
            cmd.Parameters.AddWithValue("@notas", (object?)apartado.Notas ?? DBNull.Value);
            cmd.ExecuteNonQuery();

            long idApartado;
            using (var lastId = new SqliteCommand("SELECT last_insert_rowid()", conn, tx))
                idApartado = (long)(lastId.ExecuteScalar() ?? 0L);
            apartado.IdApartado = (int)idApartado;

            foreach (var d in apartado.Detalles)
            {
                using var cmdDet = new SqliteCommand(
                    @"INSERT INTO TDetalleApartado(IdApartado,IdProducto,Cantidad,PrecioUnitario,Subtotal)
                      VALUES(@ia,@ip,@c,@pu,@sub)", conn, tx);
                cmdDet.Parameters.AddWithValue("@ia",  idApartado);
                cmdDet.Parameters.AddWithValue("@ip",  d.IdProducto);
                cmdDet.Parameters.AddWithValue("@c",   d.Cantidad);
                cmdDet.Parameters.AddWithValue("@pu",  (double)d.PrecioUnitario);
                cmdDet.Parameters.AddWithValue("@sub", (double)d.Subtotal);
                cmdDet.ExecuteNonQuery();

                // Reservar stock
                using var cmdStock = new SqliteCommand(
                    "UPDATE TProductos SET Cantidad=Cantidad-@c WHERE IdProducto=@ip", conn, tx);
                cmdStock.Parameters.AddWithValue("@c",  d.Cantidad);
                cmdStock.Parameters.AddWithValue("@ip", d.IdProducto);
                cmdStock.ExecuteNonQuery();
            }

            tx.Commit();
            return (int)idApartado;
        }
        catch { tx.Rollback(); throw; }
    }

    public List<Apartado> GetAll()
    {
        return DatabaseService.ExecuteReader(
            @"SELECT a.*,
              c.Nombre||' '||COALESCE(c.ApellidoPaterno,'') AS NombreCliente,
              e.Nombre||' '||COALESCE(e.ApellidoPaterno,'') AS NombreEmpleado
              FROM TApartados a
              INNER JOIN TCliente c ON c.IdCliente=a.IdCliente
              INNER JOIN TEmpleados e ON e.IdEmpleado=a.IdEmpleado
              ORDER BY a.FechaCreacion DESC", MapApartado);
    }

    public Apartado? GetById(int id)
    {
        var ap = DatabaseService.ExecuteReaderSingle(
            @"SELECT a.*,
              c.Nombre||' '||COALESCE(c.ApellidoPaterno,'') AS NombreCliente,
              e.Nombre||' '||COALESCE(e.ApellidoPaterno,'') AS NombreEmpleado
              FROM TApartados a
              INNER JOIN TCliente c ON c.IdCliente=a.IdCliente
              INNER JOIN TEmpleados e ON e.IdEmpleado=a.IdEmpleado
              WHERE a.IdApartado=@id",
            MapApartado, new() { ["@id"] = id });

        if (ap == null) return null;
        ap.Detalles = DatabaseService.ExecuteReader(
            @"SELECT d.*, p.Modelo AS NombreProducto
              FROM TDetalleApartado d
              INNER JOIN TProductos p ON p.IdProducto=d.IdProducto
              WHERE d.IdApartado=@id",
            MapDetalle, new() { ["@id"] = id });

        return ap;
    }

    public void AplicarPago(int idApartado, decimal monto)
    {
        DatabaseService.ExecuteNonQuery(
            @"UPDATE TApartados
              SET Anticipo=Anticipo+@m, SaldoPendiente=SaldoPendiente-@m,
                  Estado=CASE WHEN SaldoPendiente-@m<=0 THEN 'Liquidado' ELSE 'Activo' END
              WHERE IdApartado=@id",
            new() { ["@m"] = (double)monto, ["@id"] = idApartado });
    }

    public void Cancelar(int idApartado)
    {
        using var conn = DatabaseService.GetConnection();
        using var tx = conn.BeginTransaction();
        try
        {
            var detalles = DatabaseService.ExecuteReader(
                "SELECT * FROM TDetalleApartado WHERE IdApartado=@id",
                r => (IdProducto: DatabaseService.GetInt(r, "IdProducto"),
                       Cantidad: DatabaseService.GetInt(r, "Cantidad")),
                new() { ["@id"] = idApartado });

            foreach (var (idProd, cant) in detalles)
            {
                using var cmdStock = new SqliteCommand(
                    "UPDATE TProductos SET Cantidad=Cantidad+@c WHERE IdProducto=@ip", conn, tx);
                cmdStock.Parameters.AddWithValue("@c", cant);
                cmdStock.Parameters.AddWithValue("@ip", idProd);
                cmdStock.ExecuteNonQuery();
            }

            using var cmdAp = new SqliteCommand(
                "UPDATE TApartados SET Estado='Cancelado' WHERE IdApartado=@id", conn, tx);
            cmdAp.Parameters.AddWithValue("@id", idApartado);
            cmdAp.ExecuteNonQuery();

            tx.Commit();
        }
        catch { tx.Rollback(); throw; }
    }

    private static Apartado MapApartado(SqliteDataReader r) => new()
    {
        IdApartado     = DatabaseService.GetInt(r, "IdApartado"),
        IdCliente      = DatabaseService.GetInt(r, "IdCliente"),
        NombreCliente  = DatabaseService.GetStringNull(r, "NombreCliente"),
        IdEmpleado     = DatabaseService.GetInt(r, "IdEmpleado"),
        NombreEmpleado = DatabaseService.GetStringNull(r, "NombreEmpleado"),
        FechaCreacion  = DatabaseService.GetDateTime(r, "FechaCreacion"),
        FechaLimite    = DatabaseService.GetDateTimeNull(r, "FechaLimite"),
        Total          = DatabaseService.GetDecimal(r, "Total"),
        Anticipo       = DatabaseService.GetDecimal(r, "Anticipo"),
        SaldoPendiente = DatabaseService.GetDecimal(r, "SaldoPendiente"),
        Estado         = DatabaseService.GetString(r, "Estado"),
        Notas          = DatabaseService.GetStringNull(r, "Notas"),
    };

    private static DetalleApartado MapDetalle(SqliteDataReader r) => new()
    {
        IdDetalleApartado = DatabaseService.GetInt(r, "IdDetalleApartado"),
        IdApartado        = DatabaseService.GetInt(r, "IdApartado"),
        IdProducto        = DatabaseService.GetInt(r, "IdProducto"),
        NombreProducto    = DatabaseService.GetStringNull(r, "NombreProducto"),
        Cantidad          = DatabaseService.GetInt(r, "Cantidad"),
        PrecioUnitario    = DatabaseService.GetDecimal(r, "PrecioUnitario"),
        Subtotal          = DatabaseService.GetDecimal(r, "Subtotal"),
    };
}
