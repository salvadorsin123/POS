using POS.ConexionBD;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using POS.Ventas;
using Microsoft.Data.SqlClient;

namespace POS.Inventario
{
    public class ProductoDAL2
    {
        public List<Producto> ObtenerProductos()
        {
            List<Producto> productos = new List<Producto>();
            Conexion conexionBD = new Conexion();

            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                string query = "SELECT nCategoriaID, cNombre FROM TCategorias";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    productos.Add(new Producto
                    {
                        ID = Convert.ToInt32(reader["nCategoriaID"]),
                        Categoria = reader["cNombre"].ToString(),
                        
                    });
                }

                reader.Close();
            }

            return productos;
        }

        public void AgregarCategoria(Producto producto)
        {
            Conexion conexion = new Conexion();
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                string query = "Insert Into TCategorias (cNombre) Values (@nombre)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@nombre", producto.Categoria);

                cmd.ExecuteNonQuery();
            }
        }


        public void ActualizarProducto(Producto producto)
        {
            Conexion conexion = new Conexion();
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                string query = "UPDATE TCategorias  SET cNombre = @nombre WHERE nCategoriaID = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@nombre", producto.Categoria);
                cmd.Parameters.AddWithValue("@id", producto.ID);
                cmd.ExecuteNonQuery();
            }
        }

        public void EliminarProducto(int idProducto)
        {
            Conexion conexion = new Conexion();
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                string query = "DELETE FROM TCategorias WHERE nCategoriaID = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", idProducto);
                cmd.ExecuteNonQuery();
            }
        }

        //public int InsertarVenta(int total, DateTime fecha, int clienteID, int empleadoID)
        //{
        //    using (SqlConnection conn = new Conexion().ObtenerConexion())
        //    {
        //        string query = @"INSERT INTO TVenta (nTotal, dFecha, nClienteID, nEmpleadoID)
        //                 VALUES (@Total, @Fecha, @ClienteID, @EmpleadoID);
        //                 SELECT SCOPE_IDENTITY();";

        //        SqlCommand cmd = new SqlCommand(query, conn);
        //        cmd.Parameters.AddWithValue("@Total", total);
        //        cmd.Parameters.AddWithValue("@Fecha", fecha);
        //        cmd.Parameters.AddWithValue("@ClienteID", clienteID);
        //        cmd.Parameters.AddWithValue("@EmpleadoID", empleadoID);

        //        return Convert.ToInt32(cmd.ExecuteScalar());
        //    }
        //}

        //public void InsertarDetalleVenta(int ventaID, ItemCarrito item)
        //{
        //    using (SqlConnection conn = new Conexion().ObtenerConexion())
        //    using (SqlTransaction transaccion = conn.BeginTransaction())
        //    {
        //        try
        //        {
        //            // Insertar detalle de venta
        //            string query = @"INSERT INTO TDetalleVenta (nVentaID, nProductoID, nCantidad, nPrecio, nSubtotal)
        //                     VALUES (@VentaID, @ProductoID, @Cantidad, @Precio, @Subtotal)";

        //            SqlCommand cmd = new SqlCommand(query, conn, transaccion);
        //            cmd.Parameters.AddWithValue("@VentaID", ventaID);
        //            cmd.Parameters.AddWithValue("@ProductoID", item.ProductoID);
        //            cmd.Parameters.AddWithValue("@Cantidad", item.Cantidad);
        //            cmd.Parameters.AddWithValue("@Precio", item.Precio);
        //            cmd.Parameters.AddWithValue("@Subtotal", item.Subtotal);
        //            cmd.ExecuteNonQuery();

        //            // Actualizar stock del producto
        //            string actualizar = "UPDATE TProductos SET nCantidad = nCantidad - @Cantidad WHERE nProductoID = @ProductoID";
        //            SqlCommand cmdStock = new SqlCommand(actualizar, conn, transaccion);
        //            cmdStock.Parameters.AddWithValue("@Cantidad", item.Cantidad);
        //            cmdStock.Parameters.AddWithValue("@ProductoID", item.ProductoID);
        //            cmdStock.ExecuteNonQuery();

        //            // Confirmar transacción
        //            transaccion.Commit();
        //        }
        //        catch (Exception ex)
        //        {
        //            transaccion.Rollback();
        //            MessageBox.Show("Error al insertar el detalle de venta: " + ex.Message);
        //        }
        //    }
        //}

    }

}