using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POS.ConexionBD;

namespace POS.Reporte
{
    public class ReporteDAL
    {
        private Conexion conexionBD = new Conexion();


        public int GuardarReporte(Reporte reporte)
        {
            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                string query = @"
                INSERT INTO TReporte 
                (cTipoReporte, dFechaGeneracion, cPeriodo, dFechaInicio, dFechaFin, cNombreArchivo, nEmpleadoID)
                VALUES 
                (@TipoReporte, @FechaGeneracion, @Periodo, @FechaInicio, @FechaFin, @NombreArchivo, @EmpleadoID);
                SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TipoReporte", reporte.TipoReporte);
                cmd.Parameters.AddWithValue("@FechaGeneracion", reporte.FechaGeneracion);
                cmd.Parameters.AddWithValue("@Periodo", reporte.Periodo);
                cmd.Parameters.AddWithValue("@FechaInicio", reporte.FechaInicio);
                cmd.Parameters.AddWithValue("@FechaFin", (object)reporte.FechaFin ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NombreArchivo", reporte.NombreArchivo);
                cmd.Parameters.AddWithValue("@EmpleadoID", reporte.EmpleadoID);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        public ReporteVentasMes ObtenerVentasPorMes(int mes, int año)
        {
            var reporte = new ReporteVentasMes
            {
                Mes = mes,
                Año = año,
                Detalles = new List<DetalleVentaMes>()
            };

            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                string query = @"
        SELECT 
            DAY(v.dFecha) as Dia,
            SUM(v.nTotal) as TotalDia,
            COUNT(v.nVentaID) as CantidadVentas
        FROM TVenta v
        WHERE MONTH(v.dFecha) = @Mes AND YEAR(v.dFecha) = @Año
        GROUP BY DAY(v.dFecha)
        ORDER BY Dia";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Mes", mes);
                cmd.Parameters.AddWithValue("@Año", año);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    reporte.Detalles.Add(new DetalleVentaMes
                    {
                        Fecha = new DateTime(año, mes, Convert.ToInt32(reader["Dia"])),
                        TotalDia = Convert.ToDecimal(reader["TotalDia"]),
                        CantidadVentasDia = Convert.ToInt32(reader["CantidadVentas"])
                    });
                }
            }

            // Calcular totales
            reporte.TotalVentas = reporte.Detalles.Sum(d => d.TotalDia);
            reporte.CantidadVentas = reporte.Detalles.Sum(d => d.CantidadVentasDia);

            return reporte;
        }

        public ReporteComprasProveedores ObtenerComprasProveedoresPorMes(int mes, int año)
        {
            var reporte = new ReporteComprasProveedores
            {
                Mes = mes,
                Año = año,
                Detalles = new List<DetalleCompraProveedor>()
            };

            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                // Primero obtenemos todos los proveedores
                List<(int ProveedorId, string Proveedor, decimal TotalComprado, decimal DeudaFinal, int CantidadCompras)> proveedores = new List<(int, string, decimal, decimal, int)>();

                string queryProveedores = @"
                                            SELECT 
                                                p.nProveedorID,
                                                p.cNombreP as Proveedor,
                                                SUM(ps.nDeudaTotal - ps.nAnticipo) as TotalComprado,
                                                SUM(ps.nDeudaTotal) as DeudaFinal,
                                                COUNT(DISTINCT ps.nServicioID) as CantidadCompras
                                            FROM TProveedores_Servicio ps
                                            JOIN TProveedores p ON ps.nProveedorID = p.nProveedorID
                                            WHERE MONTH(ps.dPedido) = @Mes 
                                              AND YEAR(ps.dPedido) = @Año
                                              AND ps.cEstado != 'Cancelado'  -- Excluir pedidos cancelados
                                            GROUP BY p.nProveedorID, p.cNombreP
                                            ORDER BY TotalComprado DESC";

                using (SqlCommand cmd = new SqlCommand(queryProveedores, conn))
                {
                    cmd.Parameters.AddWithValue("@Mes", mes);
                    cmd.Parameters.AddWithValue("@Año", año);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            proveedores.Add((
                                Convert.ToInt32(reader["nProveedorID"]),
                                reader["Proveedor"].ToString(),
                                Convert.ToDecimal(reader["TotalComprado"]),
                                Convert.ToDecimal(reader["DeudaFinal"]),
                                Convert.ToInt32(reader["CantidadCompras"])
                            ));
                        }
                    }
                }

                // Luego para cada proveedor obtenemos los productos
                foreach (var prov in proveedores)
                {
                    var detalle = new DetalleCompraProveedor
                    {
                        Proveedor = prov.Proveedor,
                        TotalComprado = prov.TotalComprado,
                        DeudaFinal = prov.DeudaFinal,
                        CantidadCompras = prov.CantidadCompras,
                        Productos = new List<ProductoComprado>()
                    };

                    ObtenerProductosComprados(conn, prov.ProveedorId, mes, año, detalle.Productos);
                    reporte.Detalles.Add(detalle);
                }
            }

            reporte.TotalCompras = reporte.Detalles.Sum(d => d.TotalComprado);
            return reporte;
        }
        private void ObtenerProductosComprados(SqlConnection conn, int proveedorId, int mes, int año, List<ProductoComprado> productos)
        {
            string query = @"
                    SELECT 
                        pp.cProducto as NombreProducto,
                        SUM(sdp.nCantidad) as Cantidad,
                        pp.nPrecioUnitario,
                        SUM(sdp.nCantidad * pp.nPrecioUnitario) as Subtotal
                    FROM TServicio_DetalleProductos sdp
                    JOIN TProveedores_Productos pp ON sdp.nProveedorProductoID = pp.nProveedorProductoID
                    JOIN TProveedores_Servicio ps ON sdp.nServicioID = ps.nServicioID
                    WHERE ps.nProveedorID = @ProveedorId 
                      AND MONTH(ps.dPedido) = @Mes 
                      AND YEAR(ps.dPedido) = @Año
                      AND ps.cEstado != 'Cancelado'  -- Excluir pedidos cancelados
                    GROUP BY pp.cProducto, pp.nPrecioUnitario
                    ORDER BY Subtotal DESC";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ProveedorId", proveedorId);
            cmd.Parameters.AddWithValue("@Mes", mes);
            cmd.Parameters.AddWithValue("@Año", año);

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                productos.Add(new ProductoComprado
                {
                    NombreProducto = reader["NombreProducto"].ToString(),
                    Cantidad = Convert.ToInt32(reader["Cantidad"]),
                    PrecioUnitario = Convert.ToDecimal(reader["nPrecioUnitario"]),
                    Subtotal = Convert.ToDecimal(reader["Subtotal"])
                });
            }
            reader.Close();
        }
        public ReporteMovimientoProductos ObtenerMovimientoProductos(DateTime fechaInicio, DateTime fechaFin)
        {
            var reporte = new ReporteMovimientoProductos
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Productos = new List<MovimientoProducto>()
            };

            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                // Consulta para obtener movimiento de productos
                string query = @"
                        SELECT 
                            p.nProductoID,
                            p.cModelo as Modelo,
                            p.cTipo as Producto,
                            p.nCantidad as StockActual,
                            ISNULL((
                                SELECT SUM(sdp.nCantidad)
                                FROM TServicio_DetalleProductos sdp
                                JOIN TProveedores_Servicio ps ON sdp.nServicioID = ps.nServicioID
                                WHERE sdp.nProveedorProductoID IN (
                                    SELECT nProveedorProductoID 
                                    FROM TProveedores_Productos 
                                    WHERE cProducto = p.cModelo
                                )
                                AND ps.dPedido BETWEEN @FechaInicio AND @FechaFin
                                AND ps.cEstado != 'Cancelado'  -- Excluir pedidos cancelados
                            ), 0) as CantidadRecibida,
                            ISNULL((
                                SELECT SUM(dv.nCantidad)
                                FROM TDetalleVenta dv
                                JOIN TVenta v ON dv.nVentaID = v.nVentaID
                                WHERE dv.nProductoID = p.nProductoID
                                AND v.dFecha BETWEEN @FechaInicio AND @FechaFin
                            ), 0) as CantidadVendida
                        FROM TProductos p
                        ORDER BY p.cTipo, p.cModelo";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    reporte.Productos.Add(new MovimientoProducto
                    {
                        Producto = reader["Producto"].ToString(),
                        Modelo = reader["Modelo"].ToString(),
                        StockActual = Convert.ToInt32(reader["StockActual"]),
                        CantidadRecibida = Convert.ToInt32(reader["CantidadRecibida"]),
                        CantidadVendida = Convert.ToInt32(reader["CantidadVendida"])
                    });
                }
            }

            return reporte;
        }
    }
}