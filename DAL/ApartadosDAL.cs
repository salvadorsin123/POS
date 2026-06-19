using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POS.ConexionBD;
using POS.Apartados;
using Microsoft.VisualBasic;
using POS.Ventas;

namespace POS.Apartados
{
    public class ApartadosDAL
    {
        private static Conexion conexionBD = new Conexion();
        public static List<tapartados> ObtenerApartados()
        {
            List<tapartados> lista = new List<tapartados>();

            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                string query = "SELECT a.nApartadoID, a.nClienteID, a.tFechaApartado, a.nAnticipo, a.nTotalApartado, a.nSaldoPendiente, a.dFechaLimite, a.cEstado, c.cNombre FROM TApartados a join TCliente c on a.nClienteID = c.nClienteID;";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new tapartados
                    {
                        nApartadoID = Convert.ToInt32(reader["nApartadoID"]),
                        nClienteID = Convert.ToInt32(reader["nClienteID"]),
                        cNombreCliente = reader["cNombre"].ToString(),
                        tFechaApartado = Convert.ToDateTime(reader["tFechaApartado"]),
                        nAnticipo = Convert.ToDouble(reader["nAnticipo"]),
                        nTotalApartado = Convert.ToDouble(reader["nTotalApartado"]),
                        nSaldoPendiente = Convert.ToDouble(reader["nSaldoPendiente"]),
                        dFechaLimite = Convert.ToDateTime(reader["dFechaLimite"]),
                        cEstado = reader["cEstado"].ToString()
                    });
                }
                reader.Close();
            }

            return lista;
        }

        public static int InsertarApartado(tapartados apartado)
        {
            int nuevoID = 0;
            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                // Calcular el total del apartado (suma de los precios de los productos en el detalle)
                double totalApartado = 0;

                string queryTotal = "SELECT SUM(p.nPrecio * d.nCantidad) FROM TProductos p " +
                                    "JOIN TDetalleApartado d ON p.nProductoID = d.nProductoID " +
                                    "WHERE d.nApartadoID = @apartadoID";
                SqlCommand cmdTotal = new SqlCommand(queryTotal, conn);
                cmdTotal.Parameters.AddWithValue("@apartadoID", apartado.nApartadoID);
                totalApartado = Convert.ToDouble(cmdTotal.ExecuteScalar());

                // Si el total es 0, calcularlo de otra manera, por ejemplo, por la cantidad de productos.
                if (totalApartado == 0)
                {
                    totalApartado = apartado.nAnticipo + apartado.nSaldoPendiente;
                }

                // Insertar el apartado con el nuevo total y estado
                string query = "INSERT INTO TApartados (nClienteID, tFechaApartado, nAnticipo, nTotalApartado, nSaldoPendiente, dFechaLimite, cEstado) " +
                               "OUTPUT INSERTED.nApartadoID VALUES (@cliente, GETDATE(), @anticipo, @total, @saldo, @fecha, @estado)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@cliente", apartado.nClienteID);
                cmd.Parameters.AddWithValue("@anticipo", apartado.nAnticipo);
                cmd.Parameters.AddWithValue("@total", totalApartado);  // Nuevo total calculado
                cmd.Parameters.AddWithValue("@saldo", apartado.nSaldoPendiente);
                cmd.Parameters.AddWithValue("@fecha", apartado.dFechaLimite);
                cmd.Parameters.AddWithValue("@estado", "Pendiente");  // Estado por defecto

                nuevoID = (int)cmd.ExecuteScalar(); // Recuperamos el ID del nuevo apartado
            }
            return nuevoID;
        }

        public static void InsertarDetalle(int apartadoID, List<TDetalleApartado> detalles)
        {
            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                foreach (var d in detalles)
                {
                    // Obtener el precio del producto
                    string queryPrecio = "SELECT nPrecio FROM TProductos WHERE nProductoID = @productoID";
                    SqlCommand cmdPrecio = new SqlCommand(queryPrecio, conn);
                    cmdPrecio.Parameters.AddWithValue("@productoID", d.nProductoID);
                    double precio = Convert.ToDouble(cmdPrecio.ExecuteScalar());

                    // Insertar el detalle del apartado
                    string query = "INSERT INTO TDetalleApartado (nApartadoID, nProductoID, nCantidad, nPrecio) " +
                                   "VALUES (@apartado, @producto, @cantidad, @precio)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@apartado", apartadoID);
                    cmd.Parameters.AddWithValue("@producto", d.nProductoID);
                    cmd.Parameters.AddWithValue("@cantidad", d.nCantidad);
                    cmd.Parameters.AddWithValue("@precio", precio);
                    cmd.ExecuteNonQuery();

                }

                // Luego de insertar todos los detalles, actualizar el total del apartado
                ActualizarTotalApartado(apartadoID, conn);
            }
        }

        private static void ActualizarTotalApartado(int apartadoID, SqlConnection conn)
        {
            // Calcular el nuevo total del apartado
            double totalApartado = 0;

            string queryTotal = "SELECT SUM(p.nPrecio * d.nCantidad) FROM TProductos p " +
                                "JOIN TDetalleApartado d ON p.nProductoID = d.nProductoID " +
                                "WHERE d.nApartadoID = @apartadoID";
            SqlCommand cmdTotal = new SqlCommand(queryTotal, conn);
            cmdTotal.Parameters.AddWithValue("@apartadoID", apartadoID);
            totalApartado = Convert.ToDouble(cmdTotal.ExecuteScalar());

            // Actualizar el total del apartado
            string queryActualizarTotal = "UPDATE TApartados SET nTotalApartado = @total WHERE nApartadoID = @apartadoID";
            SqlCommand cmdActualizarTotal = new SqlCommand(queryActualizarTotal, conn);
            cmdActualizarTotal.Parameters.AddWithValue("@total", totalApartado);
            cmdActualizarTotal.Parameters.AddWithValue("@apartadoID", apartadoID);
            cmdActualizarTotal.ExecuteNonQuery();
        }
        public static void ActualizarApartado(tapartados apartado)
        {
            using (SqlConnection connection = conexionBD.ObtenerConexion())
            {
                string query = @"UPDATE TApartados 
                         SET nAnticipo = @anticipo, 
                             nSaldoPendiente = @saldo, 
                             cEstado = @estado 
                         WHERE nApartadoID = @id";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@anticipo", apartado.nAnticipo);
                cmd.Parameters.AddWithValue("@saldo", apartado.nSaldoPendiente);
                cmd.Parameters.AddWithValue("@estado", apartado.cEstado);
                cmd.Parameters.AddWithValue("@id", apartado.nApartadoID);

                cmd.ExecuteNonQuery();
            }
        }



        public static void EliminarApartado(int apartadoID)
        {
            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                string query = "DELETE FROM TApartados WHERE nApartadoID = @apartadoID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@apartadoID", apartadoID);

                cmd.ExecuteNonQuery();
            }
        }


        public static void EliminarDetallesApartado(int apartadoID)
            {
                using (SqlConnection conn = conexionBD.ObtenerConexion())
                {
                string query = "DELETE FROM TDetalleApartado WHERE nApartadoID = @apartadoID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@apartadoID", apartadoID);
                cmd.ExecuteNonQuery();
                }
            }


        public static void InsertarApartadoConDetalles(int idCliente, DateTime fechaLimite, double abono, double total, double saldo, string estado, List<ItemCarrito> detalles)
        {
            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    // Insertar en TApartados
                    string sqlInsertApartado = @"
                INSERT INTO TApartados (nClienteID, tFechaApartado, nAnticipo, nTotalApartado, nSaldoPendiente, dFechaLimite, cEstado) 
                OUTPUT INSERTED.nApartadoID
                VALUES (@nClienteID, GETDATE(), @nAnticipo, @nTotalApartado, @nSaldoPendiente, @dFechaLimite, @cEstado)";

                    SqlCommand cmdApartado = new SqlCommand(sqlInsertApartado, conn, trans);
                    cmdApartado.Parameters.AddWithValue("@nClienteID", idCliente);
                    cmdApartado.Parameters.AddWithValue("@nAnticipo", abono);
                    cmdApartado.Parameters.AddWithValue("@nTotalApartado", total);
                    cmdApartado.Parameters.AddWithValue("@nSaldoPendiente", saldo);
                    cmdApartado.Parameters.AddWithValue("@dFechaLimite", fechaLimite);
                    cmdApartado.Parameters.AddWithValue("@cEstado", estado);

                    int idApartado = (int)cmdApartado.ExecuteScalar();

                    // Insertar detalles y actualizar stock por cada producto
                    foreach (var item in detalles)
                    {
                        // Insertar en TDetalleApartado
                        string sqlInsertDetalle = @"
                    INSERT INTO TDetalleApartado (nApartadoID, nProductoID, nCantidad, nPrecio)
                    VALUES (@nApartadoID, @nProductoID, @nCantidad, @nPrecio)";

                        SqlCommand cmdDetalle = new SqlCommand(sqlInsertDetalle, conn, trans);
                        cmdDetalle.Parameters.AddWithValue("@nApartadoID", idApartado);
                        cmdDetalle.Parameters.AddWithValue("@nProductoID", item.ProductoID);
                        cmdDetalle.Parameters.AddWithValue("@nCantidad", item.Cantidad);
                        cmdDetalle.Parameters.AddWithValue("@nPrecio", item.Precio);

                        cmdDetalle.ExecuteNonQuery();

                        // Actualizar stock del producto después de insertar el detalle
                        string sqlActualizarStock = @"
                    UPDATE TProductos
                    SET nCantidad = nCantidad - @Cantidad
                    WHERE nProductoID = @ProductoID";

                        SqlCommand cmdStock = new SqlCommand(sqlActualizarStock, conn, trans);
                        cmdStock.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                        cmdStock.Parameters.AddWithValue("@ProductoID", item.ProductoID);

                        cmdStock.ExecuteNonQuery();
                    }

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }


    }
}

