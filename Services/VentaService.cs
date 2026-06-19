using Microsoft.Data.Sqlite;
using POS.Data;
using POS.Models;

namespace POS.Services;

public class VentaService
{
    private readonly ProductoService _productoService = new();

    public int RegistrarVenta(Venta venta)
    {
        using var conn = DatabaseService.GetConnection();
        using var tx = conn.BeginTransaction();
        try
        {
            // Validar stock
            foreach (var d in venta.Detalles)
            {
                var prod = _productoService.GetById(d.IdProducto)
                    ?? throw new InvalidOperationException($"Producto {d.IdProducto} no encontrado.");
                if (prod.Cantidad < d.Cantidad)
                    throw new InvalidOperationException(
                        $"Stock insuficiente para '{prod.Modelo}'. Disponible: {prod.Cantidad}");
            }

            string ticket = $"TKT-{DateTime.Now:yyyyMMddHHmmss}";
            venta.NumTicket = ticket;

            using var cmdVenta = new SqliteCommand(
                @"INSERT INTO TVenta(IdEmpleado,IdCliente,Subtotal,Descuento,IVA,Total,Estado,NumTicket)
                  VALUES(@emp,@cli,@sub,@desc,@iva,@tot,'Completada',@tick)", conn, tx);
            cmdVenta.Parameters.AddWithValue("@emp",  venta.IdEmpleado);
            cmdVenta.Parameters.AddWithValue("@cli",  (object?)venta.IdCliente ?? DBNull.Value);
            cmdVenta.Parameters.AddWithValue("@sub",  (double)venta.Subtotal);
            cmdVenta.Parameters.AddWithValue("@desc", (double)venta.Descuento);
            cmdVenta.Parameters.AddWithValue("@iva",  (double)venta.IVA);
            cmdVenta.Parameters.AddWithValue("@tot",  (double)venta.Total);
            cmdVenta.Parameters.AddWithValue("@tick", ticket);
            cmdVenta.ExecuteNonQuery();

            long idVenta;
            using (var lastId = new SqliteCommand("SELECT last_insert_rowid()", conn, tx))
                idVenta = (long)(lastId.ExecuteScalar() ?? 0L);
            venta.IdVenta = (int)idVenta;

            foreach (var d in venta.Detalles)
            {
                using var cmdDet = new SqliteCommand(
                    @"INSERT INTO TDetalleVenta(IdVenta,IdProducto,Cantidad,PrecioUnitario,Descuento,Subtotal)
                      VALUES(@iv,@ip,@c,@pu,@d,@sub)", conn, tx);
                cmdDet.Parameters.AddWithValue("@iv",  idVenta);
                cmdDet.Parameters.AddWithValue("@ip",  d.IdProducto);
                cmdDet.Parameters.AddWithValue("@c",   d.Cantidad);
                cmdDet.Parameters.AddWithValue("@pu",  (double)d.PrecioUnitario);
                cmdDet.Parameters.AddWithValue("@d",   (double)d.Descuento);
                cmdDet.Parameters.AddWithValue("@sub", (double)d.Subtotal);
                cmdDet.ExecuteNonQuery();

                // Descontar stock
                using var cmdStock = new SqliteCommand(
                    "UPDATE TProductos SET Cantidad=Cantidad-@c WHERE IdProducto=@ip", conn, tx);
                cmdStock.Parameters.AddWithValue("@c",  d.Cantidad);
                cmdStock.Parameters.AddWithValue("@ip", d.IdProducto);
                cmdStock.ExecuteNonQuery();
            }

            // Incrementar NumVentas del empleado
            using var cmdEmp = new SqliteCommand(
                "UPDATE TEmpleados SET NumVentas=NumVentas+1 WHERE IdEmpleado=@id", conn, tx);
            cmdEmp.Parameters.AddWithValue("@id", venta.IdEmpleado);
            cmdEmp.ExecuteNonQuery();

            tx.Commit();
            LoggerService.Info($"Venta registrada: Ticket={ticket}, Total={venta.Total:C2}");
            return (int)idVenta;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public List<Venta> GetAll(DateTime? desde = null, DateTime? hasta = null)
    {
        string where = "";
        var parms = new Dictionary<string, object?>();
        if (desde.HasValue)
        {
            where += " AND v.FechaVenta >= @desde";
            parms["@desde"] = desde.Value.ToString("yyyy-MM-dd");
        }
        if (hasta.HasValue)
        {
            where += " AND v.FechaVenta <= @hasta";
            parms["@hasta"] = hasta.Value.ToString("yyyy-MM-dd 23:59:59");
        }

        return DatabaseService.ExecuteReader(
            $@"SELECT v.*, e.Nombre||' '||COALESCE(e.ApellidoPaterno,'') AS NombreEmpleado,
               c.Nombre||' '||COALESCE(c.ApellidoPaterno,'') AS NombreCliente
               FROM TVenta v
               INNER JOIN TEmpleados e ON e.IdEmpleado=v.IdEmpleado
               LEFT JOIN TCliente c ON c.IdCliente=v.IdCliente
               WHERE 1=1 {where}
               ORDER BY v.FechaVenta DESC",
            MapVenta, parms);
    }

    public Venta? GetById(int id)
    {
        var venta = DatabaseService.ExecuteReaderSingle(
            @"SELECT v.*, e.Nombre||' '||COALESCE(e.ApellidoPaterno,'') AS NombreEmpleado,
              c.Nombre||' '||COALESCE(c.ApellidoPaterno,'') AS NombreCliente
              FROM TVenta v
              INNER JOIN TEmpleados e ON e.IdEmpleado=v.IdEmpleado
              LEFT JOIN TCliente c ON c.IdCliente=v.IdCliente
              WHERE v.IdVenta=@id",
            MapVenta, new() { ["@id"] = id });

        if (venta == null) return null;

        venta.Detalles = DatabaseService.ExecuteReader(
            @"SELECT d.*, p.Modelo AS NombreProducto
              FROM TDetalleVenta d
              INNER JOIN TProductos p ON p.IdProducto=d.IdProducto
              WHERE d.IdVenta=@id",
            MapDetalle, new() { ["@id"] = id });

        return venta;
    }

    private static Venta MapVenta(SqliteDataReader r) => new()
    {
        IdVenta        = DatabaseService.GetInt(r, "IdVenta"),
        IdEmpleado     = DatabaseService.GetInt(r, "IdEmpleado"),
        NombreEmpleado = DatabaseService.GetStringNull(r, "NombreEmpleado"),
        IdCliente      = DatabaseService.GetIntNull(r, "IdCliente"),
        NombreCliente  = DatabaseService.GetStringNull(r, "NombreCliente"),
        FechaVenta     = DatabaseService.GetDateTime(r, "FechaVenta"),
        Subtotal       = DatabaseService.GetDecimal(r, "Subtotal"),
        Descuento      = DatabaseService.GetDecimal(r, "Descuento"),
        IVA            = DatabaseService.GetDecimal(r, "IVA"),
        Total          = DatabaseService.GetDecimal(r, "Total"),
        Estado         = DatabaseService.GetString(r, "Estado"),
        NumTicket      = DatabaseService.GetStringNull(r, "NumTicket"),
    };

    private static DetalleVenta MapDetalle(SqliteDataReader r) => new()
    {
        IdDetalleVenta  = DatabaseService.GetInt(r, "IdDetalleVenta"),
        IdVenta         = DatabaseService.GetInt(r, "IdVenta"),
        IdProducto      = DatabaseService.GetInt(r, "IdProducto"),
        NombreProducto  = DatabaseService.GetStringNull(r, "NombreProducto"),
        Cantidad        = DatabaseService.GetInt(r, "Cantidad"),
        PrecioUnitario  = DatabaseService.GetDecimal(r, "PrecioUnitario"),
        Descuento       = DatabaseService.GetDecimal(r, "Descuento"),
        Subtotal        = DatabaseService.GetDecimal(r, "Subtotal"),
    };
}
