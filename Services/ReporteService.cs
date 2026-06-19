using Microsoft.Data.Sqlite;
using POS.Data;
using POS.Models;
using ReporteModel = POS.Models.Reporte;

namespace POS.Services;

public class ReporteService
{
    public List<ResumenVenta> GetResumenVentas(DateTime desde, DateTime hasta)
    {
        return DatabaseService.ExecuteReader(
            @"SELECT date(FechaVenta) AS Fecha,
              COUNT(*) AS TotalTransacciones,
              SUM(Total) AS TotalIngresos,
              SUM(Descuento) AS TotalDescuentos
              FROM TVenta
              WHERE FechaVenta BETWEEN @d AND @h AND Estado='Completada'
              GROUP BY date(FechaVenta)
              ORDER BY Fecha",
            r => new ResumenVenta
            {
                Fecha               = DatabaseService.GetDateTime(r, "Fecha"),
                TotalTransacciones  = DatabaseService.GetInt(r, "TotalTransacciones"),
                TotalIngresos       = DatabaseService.GetDecimal(r, "TotalIngresos"),
                TotalDescuentos     = DatabaseService.GetDecimal(r, "TotalDescuentos"),
            },
            new() { ["@d"] = desde.ToString("yyyy-MM-dd"), ["@h"] = hasta.ToString("yyyy-MM-dd 23:59:59") });
    }

    public List<MovimientoProducto> GetMovimientosProducto(DateTime desde, DateTime hasta)
    {
        return DatabaseService.ExecuteReader(
            @"SELECT p.IdProducto, p.Modelo AS NombreProducto, p.Cantidad AS StockActual,
              COALESCE(SUM(d.Cantidad),0) AS CantidadVendida,
              COALESCE(SUM(d.Subtotal),0) AS TotalGenerado
              FROM TProductos p
              LEFT JOIN TDetalleVenta d ON d.IdProducto=p.IdProducto
              LEFT JOIN TVenta v ON v.IdVenta=d.IdVenta
                AND v.FechaVenta BETWEEN @d AND @h AND v.Estado='Completada'
              WHERE p.Activo=1
              GROUP BY p.IdProducto, p.Modelo, p.Cantidad
              ORDER BY CantidadVendida DESC",
            r => new MovimientoProducto
            {
                IdProducto      = DatabaseService.GetInt(r, "IdProducto"),
                NombreProducto  = DatabaseService.GetString(r, "NombreProducto"),
                StockActual     = DatabaseService.GetInt(r, "StockActual"),
                CantidadVendida = DatabaseService.GetInt(r, "CantidadVendida"),
                TotalGenerado   = DatabaseService.GetDecimal(r, "TotalGenerado"),
            },
            new() { ["@d"] = desde.ToString("yyyy-MM-dd"), ["@h"] = hasta.ToString("yyyy-MM-dd 23:59:59") });
    }

    public List<Reporte> GetHistorial()
    {
        return DatabaseService.ExecuteReader(
            @"SELECT r.*, e.Nombre||' '||COALESCE(e.ApellidoPaterno,'') AS NombreEmpleado
              FROM TReporte r
              LEFT JOIN TEmpleados e ON e.IdEmpleado=r.IdEmpleado
              ORDER BY r.FechaGeneracion DESC",
            r => new Reporte
            {
                IdReporte        = DatabaseService.GetInt(r, "IdReporte"),
                Tipo             = DatabaseService.GetString(r, "Tipo"),
                FechaGeneracion  = DatabaseService.GetDateTime(r, "FechaGeneracion"),
                FechaInicio      = DatabaseService.GetDateTimeNull(r, "FechaInicio"),
                FechaFin         = DatabaseService.GetDateTimeNull(r, "FechaFin"),
                RutaArchivo      = DatabaseService.GetStringNull(r, "RutaArchivo"),
                NombreEmpleado   = DatabaseService.GetStringNull(r, "NombreEmpleado"),
            });
    }

    public void GuardarReporte(Reporte r)
    {
        DatabaseService.ExecuteNonQuery(
            @"INSERT INTO TReporte(Tipo,FechaInicio,FechaFin,RutaArchivo,IdEmpleado)
              VALUES(@tipo,@fi,@ff,@ruta,@emp)",
            new()
            {
                ["@tipo"] = r.Tipo,
                ["@fi"]   = r.FechaInicio?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value,
                ["@ff"]   = r.FechaFin?.ToString("yyyy-MM-dd")    ?? (object)DBNull.Value,
                ["@ruta"] = (object?)r.RutaArchivo ?? DBNull.Value,
                ["@emp"]  = (object?)r.IdEmpleado  ?? DBNull.Value,
            });
    }
}
