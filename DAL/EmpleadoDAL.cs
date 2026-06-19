using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using POS.ConexionBD;
using POS.Empleados; // si ahí está la clase templeados
using POS.Inicio_Sesion;
using Microsoft.Data.SqlClient;

namespace POS.Empleados
{
    public class EmpleadoDAL
    {
        private Conexion conexionBD = new Conexion();

        public List<templeados> ObtenerEmpleados()
        {
            List<templeados> empleados = new List<templeados>();

            using (SqlConnection con = conexionBD.ObtenerConexion())
            {
                string query = "SELECT e.nEmpleadoID, e.cNombreUsuario, e.cContrasena, e.nSalario, e.nVentas, e.dUltimoLogin, t.cNombre AS cTipo FROM TEmpleados e INNER JOIN TTipoEmpleado t ON e.nTipoID = t.nTipoID;";
                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    empleados.Add(new templeados
                    {
                        nEmpleadoID = Convert.ToInt32(reader["nEmpleadoID"]),
                        cNombreUsuario = reader["cNombreUsuario"].ToString(),
                        cContrasena = reader["cContrasena"].ToString(),
                        nSalario = Convert.ToInt32(reader["nSalario"]),
                        nVentas = Convert.ToInt32(reader["nVentas"]),
                        dUltimoLogin = reader["dUltimoLogin"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["dUltimoLogin"]) : null,
                        cTipo = reader["cTipo"].ToString()
                    });
                }
            }

            return empleados;
        }

        public void InsertarEmpleado(templeados empleado)
        {
            using (SqlConnection con = conexionBD.ObtenerConexion())
            {
                // Consulta para obtener el nTipoID
                string queryTipo = "SELECT nTipoID FROM TTipoEmpleado WHERE cNombre = @tipo";
                SqlCommand cmdTipo = new SqlCommand(queryTipo, con);
                cmdTipo.Parameters.AddWithValue("@tipo", empleado.cTipo);

                int tipoId = Convert.ToInt32(cmdTipo.ExecuteScalar());

                // Generar hash combinando nombre de usuario y contraseña
                string contrasenaHash = MD5Helper.GenerarHashCombinado(
                    empleado.cNombreUsuario,
                    empleado.cContrasena);

                // Consulta para insertar el empleado
                string query = @"INSERT INTO TEmpleados 
                (cNombreUsuario, cContrasena, nSalario, nVentas, nTipoID, dUltimoLogin)
                VALUES 
                (@nombre, @contrasena, @salario, @ventas, @tipoId, NULL)";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@nombre", empleado.cNombreUsuario);
                cmd.Parameters.AddWithValue("@contrasena", contrasenaHash);
                cmd.Parameters.AddWithValue("@salario", empleado.nSalario);
                cmd.Parameters.AddWithValue("@ventas", empleado.nVentas);
                cmd.Parameters.AddWithValue("@tipoId", tipoId);

                cmd.ExecuteNonQuery();
            }
        }
        public void ActualizarEmpleado(templeados empleado)
        {
            using (SqlConnection con = conexionBD.ObtenerConexion())
            {
                // Consulta para obtener el nTipoID basado en el nombre del tipo
                string queryTipo = "SELECT nTipoID FROM TTipoEmpleado WHERE cNombre = @tipo";
                SqlCommand cmdTipo = new SqlCommand(queryTipo, con);
                cmdTipo.Parameters.AddWithValue("@tipo", empleado.cTipo);
                int tipoId = Convert.ToInt32(cmdTipo.ExecuteScalar());

                string query = @"UPDATE TEmpleados SET 
                cNombreUsuario = @nombre,
                nSalario = @salario,
                nVentas = @ventas,
                nTipoID = @tipoId
                WHERE nEmpleadoID = @id";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", empleado.nEmpleadoID);
                cmd.Parameters.AddWithValue("@nombre", empleado.cNombreUsuario);
                cmd.Parameters.AddWithValue("@salario", empleado.nSalario);
                cmd.Parameters.AddWithValue("@ventas", empleado.nVentas);
                cmd.Parameters.AddWithValue("@tipoId", tipoId);

                cmd.ExecuteNonQuery();
            }
        }
        public void ActualizarContrasena(int empleadoId, string nombreUsuario, string nuevaContrasena)
        {
            if (string.IsNullOrWhiteSpace(nuevaContrasena))
                return;

            using (SqlConnection con = conexionBD.ObtenerConexion())
            {
                string contrasenaHash = MD5Helper.GenerarHashCombinado(nombreUsuario, nuevaContrasena);

                string query = @"UPDATE TEmpleados SET 
                cContrasena = @contrasena
                WHERE nEmpleadoID = @id";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", empleadoId);
                cmd.Parameters.AddWithValue("@contrasena", contrasenaHash);

                cmd.ExecuteNonQuery();
            }
        }

        public void EliminarEmpleado(int id)
        {
            using (SqlConnection con = conexionBD.ObtenerConexion())
            {
                string query = "DELETE FROM TEmpleados WHERE nEmpleadoID = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}

//namespace Aplicacion_Sistema_D2.Empleados
//{
//    public class EmpleadoDAL
//    {
//        private ConexionBD.Conexion conexionBD = new ConexionBD.Conexion();

//        public List<templeados> ObtenerEmpleados()
//        {
//            List<templeados> empleados = new List<templeados>();
//            using (SqlConnection con = conexionBD.AbrirConexion())
//            {
//                string query = "SELECT * FROM TEmpleados";
//                SqlCommand cmd = new SqlCommand(query, con);
//                SqlDataReader reader = cmd.ExecuteReader();

//                while (reader.Read())
//                {
//                    empleados.Add(new templeados
//                    {
//                        nEmpleadoID = Convert.ToInt32(reader["nEmpleadoID"]),
//                        cNombreUsuario = reader["cNombreUsuario"].ToString(),
//                        cContrasena = reader["cContrasena"].ToString(),
//                        nSalario = Convert.ToInt32(reader["nSalario"]),
//                        nVentas = Convert.ToInt32(reader["nVentas"]),
//                        cTipo = reader["cTipo"].ToString()
//                    });
//                }
//                reader.Close();
//            }
//            return empleados; // No es necesario cerrar la conexión explícitamente aquí.
//        }

//        public void InsertarEmpleado(templeados empleado)
//        {
//            using (SqlConnection con = conexionBD.AbrirConexion())
//            {
//                string query = @"INSERT INTO TEmpleados (cNombreUsuario, cContrasena, nSalario, nVentas, cTipo)
//                             VALUES (@nombre, @contrasena, @salario, @ventas, @tipo)";
//                SqlCommand cmd = new SqlCommand(query, con);
//                cmd.Parameters.AddWithValue("@nombre", empleado.cNombreUsuario);
//                cmd.Parameters.AddWithValue("@contrasena", empleado.cContrasena);
//                cmd.Parameters.AddWithValue("@salario", empleado.nSalario);
//                cmd.Parameters.AddWithValue("@ventas", empleado.nVentas);
//                cmd.Parameters.AddWithValue("@tipo", empleado.cTipo);
//                cmd.ExecuteNonQuery();
//            }
//            // La conexión se cierra automáticamente después del bloque using.
//        }

//        public void ActualizarEmpleado(templeados empleado)
//        {
//            using (SqlConnection con = conexionBD.AbrirConexion())
//            {
//                string query = @"UPDATE TEmpleados SET 
//                                cNombreUsuario = @nombre,
//                                cContrasena = @contrasena,
//                                nSalario = @salario,
//                                nVentas = @ventas,
//                                cTipo = @tipo
//                             WHERE nEmpleadoID = @id";
//                SqlCommand cmd = new SqlCommand(query, con);
//                cmd.Parameters.AddWithValue("@id", empleado.nEmpleadoID);
//                cmd.Parameters.AddWithValue("@nombre", empleado.cNombreUsuario);
//                cmd.Parameters.AddWithValue("@contrasena", empleado.cContrasena);
//                cmd.Parameters.AddWithValue("@salario", empleado.nSalario);
//                cmd.Parameters.AddWithValue("@ventas", empleado.nVentas);
//                cmd.Parameters.AddWithValue("@tipo", empleado.cTipo);
//                cmd.ExecuteNonQuery();
//            }
//        }

//        public void EliminarEmpleado(int id)
//        {
//            using (SqlConnection con = conexionBD.AbrirConexion())
//            {
//                string query = "DELETE FROM TEmpleados WHERE nEmpleadoID = @id";
//                SqlCommand cmd = new SqlCommand(query, con);
//                cmd.Parameters.AddWithValue("@id", id);
//                cmd.ExecuteNonQuery();
//            }
//        }
//    }
//}
