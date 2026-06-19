using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;

namespace POS
{
    public static class InstaladorDB
    {
        // Script SQL completo para crear las tablas
        public static void Instalar()
        {
            string nombreBD = "MiPosDB";
            string nombreArchivoSQL = "BD/instaladorDB.sql"; // El nombre exacto de tu archivo

            // 1. Verificar si el archivo existe junto al .exe
            string rutaScript = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nombreArchivoSQL);

            if (!File.Exists(rutaScript))
            {
                throw new Exception($"No se encontró el archivo de instalación: {nombreArchivoSQL}\nAsegúrate de marcar 'Copiar siempre' en sus propiedades.");
            }

            // 2. Crear la Base de Datos vacía (usando master)
            CrearBaseDatosSiNoExiste(nombreBD);

            // 3. Leer y Ejecutar el Script
            EjecutarScriptSQL(rutaScript, nombreBD);
        }

        private static void CrearBaseDatosSiNoExiste(string nombreBD)
        {
            string conexionMaster = "Server=localhost\\SQLEXPRESS; Database=master; Integrated Security=True; TrustServerCertificate=True;";

            using (SqlConnection conn = new SqlConnection(conexionMaster))
            {
                conn.Open();
                string check = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{nombreBD}'";
                SqlCommand cmd = new SqlCommand(check, conn);

                if ((int)cmd.ExecuteScalar() == 0)
                {
                    SqlCommand crear = new SqlCommand($"CREATE DATABASE {nombreBD}", conn);
                    crear.ExecuteNonQuery();
                    Thread.Sleep(3000); // Dar tiempo a Windows
                }
            }
        }

        private static void EjecutarScriptSQL(string rutaArchivo, string nombreBD)
        {
            string contenidoScript = File.ReadAllText(rutaArchivo);

            // --- TRUCO PARA ELIMINAR 'GO' ---
            // Dividimos el script en bloques cada vez que aparece "GO"
            // Usamos Regex para ignorar mayúsculas/minúsculas y espacios
            string[] comandos = Regex.Split(contenidoScript, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            string conexionPos = $"Server=localhost\\SQLEXPRESS; Database={nombreBD}; Integrated Security=True; TrustServerCertificate=True;";

            using (SqlConnection conn = new SqlConnection(conexionPos))
            {
                conn.Open();

                foreach (string cmdTexto in comandos)
                {
                    if (!string.IsNullOrWhiteSpace(cmdTexto))
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand(cmdTexto, conn))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            // Opcional: Saber qué parte falló
                            throw new Exception($"Error en el script SQL:\n{ex.Message}\n\nComando fallido:\n{cmdTexto}");
                        }
                    }
                }
            }
        }
    }
}
