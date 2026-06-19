using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POS.ConexionBD;


namespace POS.Proveedores
{
    public class ProveedoresDAL
    {
            public List<TProveedores> ObtenerProveedores()
            {
                List<TProveedores> proveedores = new List<TProveedores>();
                using (SqlConnection conn = new Conexion().ObtenerConexion())
                {
                    string query = "SELECT nProveedorID, cNombreP, cContacto, cTelefono, cEmail, dRegistro FROM TProveedores";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        proveedores.Add(new TProveedores
                        {
                            nProveedorID = Convert.ToInt32(reader["nProveedorID"]),
                            cNombreP = reader["cNombreP"].ToString(),
                            cContacto = reader["cContacto"].ToString(),
                            cTelefono = reader["cTelefono"].ToString(),
                            cEmail = reader["cEmail"].ToString(),
                            dRegistro = Convert.ToDateTime(reader["dRegistro"])
                        });
                    }
                    reader.Close();
                }
                return proveedores;
            }

            public bool InsertarProveedor(TProveedores proveedor)
            {
                using (SqlConnection conn = new Conexion().ObtenerConexion())
                {
                    string query = "INSERT INTO TProveedores (cNombreP, cContacto, cTelefono, cEmail) " +
                                  "VALUES (@Nombre, @Contacto, @Telefono, @Email)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Nombre", proveedor.cNombreP);
                    cmd.Parameters.AddWithValue("@Contacto", proveedor.cContacto ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Telefono", proveedor.cTelefono ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", proveedor.cEmail ?? (object)DBNull.Value);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }

            public bool ActualizarProveedor(TProveedores proveedor)
            {
                using (SqlConnection conn = new Conexion().ObtenerConexion())
                {
                    string query = "UPDATE TProveedores SET cNombreP = @Nombre, cContacto = @Contacto, " +
                                    "cTelefono = @Telefono, cEmail = @Email WHERE nProveedorID = @ID";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Nombre", proveedor.cNombreP);
                    cmd.Parameters.AddWithValue("@Contacto", proveedor.cContacto ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Telefono", proveedor.cTelefono ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", proveedor.cEmail ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ID", proveedor.nProveedorID);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }

            public bool EliminarProveedor(int id)
            {
                using (SqlConnection conn = new Conexion().ObtenerConexion())
                {
                    string query = "DELETE FROM TProveedores WHERE nProveedorID = @ID";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ID", id);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
 }
