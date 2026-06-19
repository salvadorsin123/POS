using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using POS.ConexionBD;
using POS.Ventas;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace POS.Ventas
{
    public class VentaDAL

    {
        public List<Producto> ObtenerProductos()
        {
            List<Producto> productos = new List<Producto>();

            Conexion conexionBD = new Conexion();

            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                string query = "SELECT nProductoID, cModelo, nPrecio, nDescuento, nCantidad, cTipo FROM TProductos";

                SqlCommand cmd = new SqlCommand(query, conn);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    double precio = Convert.ToDouble(reader["nPrecio"]);
                    int descuento = Convert.ToInt32(reader["nDescuento"]);
                    double precioConDescuento = precio - (precio * descuento / 100);

                    productos.Add(new Producto
                    {
                        ID = Convert.ToInt32(reader["nProductoID"]),
                        Nombre = reader["cModelo"].ToString(),
                        Precio = precio, // Precio original
                        Descuento = descuento, // Descuento en porcentaje
                        PrecioConDescuento = precioConDescuento, // Nuevo precio con descuento
                        Stock = Convert.ToInt32(reader["nCantidad"]),
                        cTipo = reader["cTipo"].ToString()
                    });
                }

                reader.Close();
            }

            return productos;
        }

        public int InsertarVenta(int total, DateTime fecha, int clienteID, int empleadoID)
        {
            using (SqlConnection conn = new Conexion().ObtenerConexion())
            {
                // Iniciar transacción para asegurar la integridad de los datos
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 1. Insertar la venta
                    string queryVenta = @"INSERT INTO TVenta (nTotal, dFecha, nClienteID, nEmpleadoID)
                         VALUES (@Total, @Fecha, @ClienteID, @EmpleadoID);
                         SELECT SCOPE_IDENTITY();";

                    SqlCommand cmdVenta = new SqlCommand(queryVenta, conn, transaction);
                    cmdVenta.Parameters.AddWithValue("@Total", total);
                    cmdVenta.Parameters.AddWithValue("@Fecha", fecha);
                    cmdVenta.Parameters.AddWithValue("@ClienteID", clienteID);
                    cmdVenta.Parameters.AddWithValue("@EmpleadoID", empleadoID);

                    int ventaID = Convert.ToInt32(cmdVenta.ExecuteScalar());

                    // 2. Actualizar el contador de ventas del empleado
                    string queryEmpleado = @"UPDATE TEmpleados 
                               SET nVentas = nVentas + 1 
                               WHERE nEmpleadoID = @EmpleadoID";

                    SqlCommand cmdEmpleado = new SqlCommand(queryEmpleado, conn, transaction);
                    cmdEmpleado.Parameters.AddWithValue("@EmpleadoID", empleadoID);
                    cmdEmpleado.ExecuteNonQuery();

                    // Confirmar transacción si todo fue exitoso
                    transaction.Commit();

                    return ventaID;
                }
                catch (Exception ex)
                {
                    // Revertir transacción en caso de error
                    transaction.Rollback();
                    throw new Exception("Error al registrar la venta: " + ex.Message);
                }
            }
        }

        public void InsertarDetalleVenta(int ventaID, ItemCarrito item)
        {
            using (SqlConnection conn = new Conexion().ObtenerConexion())
            using (SqlTransaction transaccion = conn.BeginTransaction())
            {
                try
                {
                    // Insertar detalle de venta
                    string query = @"INSERT INTO TDetalleVenta (nVentaID, nProductoID, nCantidad, nPrecio, nSubtotal)
                             VALUES (@VentaID, @ProductoID, @Cantidad, @Precio, @Subtotal)";

                    SqlCommand cmd = new SqlCommand(query, conn, transaccion);
                    cmd.Parameters.AddWithValue("@VentaID", ventaID);
                    cmd.Parameters.AddWithValue("@ProductoID", item.ProductoID);
                    cmd.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                    cmd.Parameters.AddWithValue("@Precio", item.Precio);
                    cmd.Parameters.AddWithValue("@Subtotal", item.Subtotal);
                    cmd.ExecuteNonQuery();

                    // Actualizar stock del producto
                    string actualizar = "UPDATE TProductos SET nCantidad = nCantidad - @Cantidad WHERE nProductoID = @ProductoID";
                    SqlCommand cmdStock = new SqlCommand(actualizar, conn, transaccion);
                    cmdStock.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                    cmdStock.Parameters.AddWithValue("@ProductoID", item.ProductoID);
                    cmdStock.ExecuteNonQuery();

                    // Confirmar transacción
                    transaccion.Commit();
                }
                catch (Exception ex)
                {
                    transaccion.Rollback();
                    MessageBox.Show("Error al insertar el detalle de venta: " + ex.Message);
                }
            }
        }

    }
}