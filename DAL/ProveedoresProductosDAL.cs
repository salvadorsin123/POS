using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POS.ConexionBD;
using POS.Modelos.Productos;
using POS.Productos;
using Microsoft.Data.SqlClient;

namespace POS.Proveedores
{
    public class ProveedoresProductosDAL
    {
        public List<TProveedores_Productos> ObtenerProductosPorProveedor(int proveedorId)
        {
            List<TProveedores_Productos> productos = new List<TProveedores_Productos>();
            using (SqlConnection conn = new Conexion().ObtenerConexion())
            {
                string query = "SELECT nProveedorProductoID, nProveedorID, cProducto, nPrecioUnitario " +
                              "FROM TProveedores_Productos WHERE nProveedorID = @ProveedorID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProveedorID", proveedorId);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    productos.Add(new TProveedores_Productos
                    {
                        nProveedorProductoID = Convert.ToInt32(reader["nProveedorProductoID"]),
                        nProveedorID = Convert.ToInt32(reader["nProveedorID"]),
                        cProducto = reader["cProducto"].ToString(),
                        nPrecioUnitario = Convert.ToDouble(reader["nPrecioUnitario"])
                    });
                }
                reader.Close();
            }
            return productos;
        }

        public bool InsertarProductoProveedor(TProveedores_Productos producto)
        {
            using (SqlConnection conn = new Conexion().ObtenerConexion())
            {
                string query = "INSERT INTO TProveedores_Productos (nProveedorID, cProducto, nPrecioUnitario) " +
                              "VALUES (@ProveedorID, @Producto, @Precio)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProveedorID", producto.nProveedorID);
                cmd.Parameters.AddWithValue("@Producto", producto.cProducto);
                cmd.Parameters.AddWithValue("@Precio", producto.nPrecioUnitario);

                return cmd.ExecuteNonQuery() > 0;
            }
        }
        public TProveedores_Productos ObtenerProducto(int productoId)
        {
            using (SqlConnection conn = new Conexion().ObtenerConexion())
            {
                string query = "SELECT nProveedorProductoID, nProveedorID, cProducto, nPrecioUnitario " +
                              "FROM TProveedores_Productos WHERE nProveedorProductoID = @ID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ID", productoId);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new TProveedores_Productos
                    {
                        nProveedorProductoID = Convert.ToInt32(reader["nProveedorProductoID"]),
                        nProveedorID = Convert.ToInt32(reader["nProveedorID"]),
                        cProducto = reader["cProducto"].ToString(),
                        nPrecioUnitario = Convert.ToDouble(reader["nPrecioUnitario"])
                    };
                }
                return null;
            }
        }
    }
}
