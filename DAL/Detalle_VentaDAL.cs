using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POS.ConexionBD;
using Microsoft.Data.SqlClient;

namespace POS.Detalle_Venta
{
    internal class Detalle_VentaDAL
    {
        public static DataTable ObtenerTodosLosDetallesDeVenta()
        {
            DataTable dt = new DataTable();
            Conexion conexionBD = new Conexion();

            try
            {
                using (SqlConnection conn = conexionBD.ObtenerConexion())
                {

                    string query = @"
                SELECT 
                    dv.nDetalleVentaID, dv.nVentaID, dv.nProductoID,
                    p.cModelo AS ModeloProducto, dv.nCantidad, dv.nPrecio, dv.nSubtotal,
                    SUM(dv.nSubtotal) OVER (PARTITION BY dv.nVentaID) AS TotalVenta
                FROM TDetalleVenta dv
                INNER JOIN TProductos p ON dv.nProductoID = p.nProductoID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los detalles de venta: " + ex.Message);
            }

            return dt;
        }


    }
}
