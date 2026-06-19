using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POS.ConexionBD;
using Microsoft.Data.SqlClient;

namespace POS.Proveedores
{
    public class ServiciosDAL
    {
        public List<TProveedores_Servicio> ObtenerServicios()
        {
            var servicios = new List<TProveedores_Servicio>();
            using (SqlConnection conn = new Conexion().ObtenerConexion())
            {
                string query = @"SELECT s.*, p.cNombreP as cNombreProveedor 
                       FROM TProveedores_Servicio s
                       INNER JOIN TProveedores p ON s.nProveedorID = p.nProveedorID";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var servicio = new TProveedores_Servicio
                    {
                        nServicioID = Convert.ToInt32(reader["nServicioID"]),
                        nProveedorID = Convert.ToInt32(reader["nProveedorID"]),
                        nDeudaTotal = Convert.ToDouble(reader["nDeudaTotal"]),
                        nAnticipo = Convert.ToDouble(reader["nAnticipo"]),
                        dPedido = Convert.ToDateTime(reader["dPedido"]),
                        cEstado = reader["cEstado"].ToString(),
                        cNombreProveedor = reader["cNombreProveedor"].ToString(),
                        Detalles = new ObservableCollection<TServicio_DetalleProductos>(),
                        cObservaciones = reader["cObservaciones"].ToString()
                    };
                    servicios.Add(servicio);
                }
                reader.Close();
            }
            return servicios;
        }


        public int InsertarServicio(TProveedores_Servicio servicio)
        {
            using (SqlConnection conn = new Conexion().ObtenerConexion())
            {
                string query = "INSERT INTO TProveedores_Servicio (nProveedorID, nDeudaTotal, nAnticipo, dPedido, cEstado, cObservaciones) " +
                              "OUTPUT INSERTED.nServicioID " +
                              "VALUES (@ProveedorID, @DeudaTotal, @Anticipo, @Pedido, @Estado, @Observaciones)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProveedorID", servicio.nProveedorID);
                cmd.Parameters.AddWithValue("@DeudaTotal", servicio.nDeudaTotal);
                cmd.Parameters.AddWithValue("@Anticipo", servicio.nAnticipo);
                cmd.Parameters.AddWithValue("@Pedido", servicio.dPedido);
                cmd.Parameters.AddWithValue("@Estado", servicio.cEstado);
                cmd.Parameters.AddWithValue("@Observaciones", servicio.cObservaciones ?? (object)DBNull.Value);

                return (int)cmd.ExecuteScalar();
            }
        }

        public bool InsertarDetalleServicio(TServicio_DetalleProductos detalle)
        {
            using (SqlConnection conn = new Conexion().ObtenerConexion())
            {
                string query = "INSERT INTO TServicio_DetalleProductos (nServicioID, nProveedorProductoID, nCantidad) " +
                              "VALUES (@ServicioID, @ProductoID, @Cantidad)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ServicioID", detalle.nServicioID);
                cmd.Parameters.AddWithValue("@ProductoID", detalle.nProveedorProductoID);
                cmd.Parameters.AddWithValue("@Cantidad", detalle.nCantidad);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool ActualizarEstadoServicio(int servicioId, string estado)
        {
            using (SqlConnection conn = new Conexion().ObtenerConexion())
            {
                string query = "UPDATE TProveedores_Servicio SET cEstado = @Estado WHERE nServicioID = @ID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Estado", estado);
                cmd.Parameters.AddWithValue("@ID", servicioId);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public List<TServicio_DetalleProductos> ObtenerDetallesServicio(int servicioId)
        {
            List<TServicio_DetalleProductos> detalles = new List<TServicio_DetalleProductos>();
            using (SqlConnection conn = new Conexion().ObtenerConexion())
            {
                string query = @"SELECT sd.nServicioDetalleID, sd.nServicioID, sd.nProveedorProductoID, 
                           sd.nCantidad, pp.cProducto, pp.nPrecioUnitario
                           FROM TServicio_DetalleProductos sd
                           INNER JOIN TProveedores_Productos pp ON sd.nProveedorProductoID = pp.nProveedorProductoID
                           WHERE sd.nServicioID = @ServicioID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ServicioID", servicioId);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    double precio = Convert.ToDouble(reader["nPrecioUnitario"]);
                    int cantidad = Convert.ToInt32(reader["nCantidad"]);

                    detalles.Add(new TServicio_DetalleProductos
                    {
                        nServicioDetalleID = Convert.ToInt32(reader["nServicioDetalleID"]),
                        nServicioID = Convert.ToInt32(reader["nServicioID"]),
                        nProveedorProductoID = Convert.ToInt32(reader["nProveedorProductoID"]),
                        nCantidad = cantidad,
                        cProducto = reader["cProducto"].ToString(),
                        nPrecioUnitario = precio,
                        //nSubtotal = precio * cantidad
                    });
                }
                reader.Close();
            }
            return detalles;
        }
        public void RegistrarAbono(int servicioId, double monto)
        {
            using (SqlConnection conn = new Conexion().ObtenerConexion())
            {
                string query = @"UPDATE TProveedores_Servicio 
                           SET nAnticipo = nAnticipo + @Monto
                           WHERE nServicioID = @ServicioID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Monto", monto);
                cmd.Parameters.AddWithValue("@ServicioID", servicioId);

                cmd.ExecuteNonQuery();
            }
        }

        public TProveedores_Servicio ObtenerServicioPorId(int servicioId)
        {
            using (SqlConnection conn = new Conexion().ObtenerConexion())
            {
                string query = @"SELECT s.*, p.cNombreP as cNombreProveedor 
               FROM TProveedores_Servicio s
               INNER JOIN TProveedores p ON s.nProveedorID = p.nProveedorID
               WHERE s.nServicioID = @servicioId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@servicioId", servicioId);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    var servicio = new TProveedores_Servicio
                    {
                        nServicioID = Convert.ToInt32(reader["nServicioID"]),
                        nProveedorID = Convert.ToInt32(reader["nProveedorID"]),
                        nDeudaTotal = Convert.ToDouble(reader["nDeudaTotal"]),
                        nAnticipo = Convert.ToDouble(reader["nAnticipo"]),
                        dPedido = Convert.ToDateTime(reader["dPedido"]),
                        cEstado = reader["cEstado"].ToString(),
                        cNombreProveedor = reader["cNombreProveedor"].ToString(),
                        Detalles = new ObservableCollection<TServicio_DetalleProductos>(),
                        cObservaciones = reader["cObservaciones"].ToString()
                    };

                    reader.Close();
                    return servicio;
                }

                reader.Close();
                return null; // Si no encuentra el servicio
            }
        }

        public bool ActualizarEstadoServicio(int servicioId, string nuevoEstado, DateTime? fechaCancelacion = null)
        {
            using (SqlConnection conn = new Conexion().ObtenerConexion())
            {
                string query = @"UPDATE TProveedores_Servicio 
                        SET cEstado = @estado
                        WHERE nServicioID = @servicioId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@estado", nuevoEstado);
                cmd.Parameters.AddWithValue("@servicioId", servicioId);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
    }
}
