using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POS.ConexionBD;
using POS.Modelos.Productos;
using POS.Productos;
using POS.Proveedores;

namespace POS.Productos
{
    public class ProductosDAL3
    {
        public List<tproductos> ObtenerProductos()
        {
            List<tproductos> productos = new List<tproductos>();
            Conexion conexionBD = new Conexion();

            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                string query = "SELECT nProductoID, cModelo, nPrecio, nDescuento, nCantidad, cTipo FROM TProductos";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    productos.Add(new tproductos
                    {
                        nProductoID = Convert.ToInt32(reader["nProductoID"]),
                        cModelo = reader["cModelo"].ToString(),
                        nPrecio = Convert.ToDouble(reader["nPrecio"]),
                        nDescuento = Convert.ToInt32(reader["nDescuento"]),
                        nCantidad = Convert.ToInt32(reader["nCantidad"]),
                        cTipo = reader["cTipo"].ToString()
                    });
                }

                reader.Close();
            }

            return productos;
        }

        public bool InsertarProducto(tproductos producto)
        {
            using (SqlConnection conn = new Conexion().ObtenerConexion())
            {
                string query = "INSERT INTO TProductos (cModelo, nPrecio, nDescuento, nCantidad, cTipo) " +
                               "VALUES (@Modelo, @Precio, @Descuento, @Cantidad, @Tipo)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Modelo", producto.cModelo);
                cmd.Parameters.AddWithValue("@Precio", producto.nPrecio);
                cmd.Parameters.AddWithValue("@Descuento", producto.nDescuento);
                cmd.Parameters.AddWithValue("@Cantidad", producto.nCantidad);
                cmd.Parameters.AddWithValue("@Tipo", producto.cTipo);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool ActualizarProducto(tproductos producto)
        {
            using (SqlConnection conn = new Conexion().ObtenerConexion())
            {
                string query = "UPDATE TProductos SET cModelo=@Modelo, nPrecio=@Precio, nDescuento=@Descuento, nCantidad=@Cantidad, cTipo=@Tipo " +
                               "WHERE nProductoID=@ID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID", producto.nProductoID);
                cmd.Parameters.AddWithValue("@Modelo", producto.cModelo);
                cmd.Parameters.AddWithValue("@Precio", producto.nPrecio);
                cmd.Parameters.AddWithValue("@Descuento", producto.nDescuento);
                cmd.Parameters.AddWithValue("@Cantidad", producto.nCantidad);
                cmd.Parameters.AddWithValue("@Tipo", producto.cTipo);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool EliminarProducto(int idProducto)
        {
            using (SqlConnection conn = new Conexion().ObtenerConexion())
            {
                string query = "DELETE FROM TProductos WHERE nProductoID = @ID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID", idProducto);

                return cmd.ExecuteNonQuery() > 0;
            }
        }


    }

}
