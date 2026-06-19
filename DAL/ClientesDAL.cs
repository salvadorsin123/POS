using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POS.ConexionBD;
using Microsoft.Data.SqlClient;

namespace POS.Clientes
{
    public class ClientesDAL
    {
        public List<Cliente> ObtenerClientes()
        {
            List<Cliente> clientes = new List<Cliente>();
            Conexion conexionBD = new Conexion();

            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                string query = "SELECT nClienteID, cNombre,cCorreo , cTelefono FROM TCliente";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    clientes.Add(new Cliente
                    {
                        Id = Convert.ToInt32(reader["nClienteID"]),
                        Nombre = reader["cNombre"].ToString(),
                        Correo = reader["cCorreo"].ToString(),
                        Telefono = reader["cTelefono"].ToString()
                    });
                }

                reader.Close();
            }

            return clientes;
        }
        public void InsertarCliente(Cliente cliente)
        {
            Conexion conexionBD = new Conexion();
            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                string query = "INSERT INTO TCliente (cNombre,cCorreo, cTelefono) VALUES (@Nombre, @Correo, @Telefono)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                cmd.Parameters.AddWithValue("@Correo", cliente.Correo);
                cmd.Parameters.AddWithValue("@Telefono", cliente.Telefono);
                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarCliente(Cliente cliente)
        {
            Conexion conexionBD = new Conexion();
            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                string query = "UPDATE TCliente SET cNombre = @Nombre,cCorreo = @Correo, cTelefono = @Telefono WHERE nClienteID = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                cmd.Parameters.AddWithValue("@Correo", cliente.Correo);
                cmd.Parameters.AddWithValue("@Telefono", cliente.Telefono);
                cmd.Parameters.AddWithValue("@Id", cliente.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public void EliminarCliente(int clienteId)
        {
            Conexion conexionBD = new Conexion();
            using (SqlConnection conn = conexionBD.ObtenerConexion())
            {
                string query = "DELETE FROM TCliente WHERE nClienteID = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", clienteId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
