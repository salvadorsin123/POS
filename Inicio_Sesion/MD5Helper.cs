using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace POS.Inicio_Sesion
{

public static class MD5Helper
    {
        public static string GenerarHashCombinado(string usuario, string password)
        {
            // Combinar usuario y password con un separador
            string combinacion = $"{usuario}:{password}";

            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(combinacion));

                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }
        }

        public static bool VerificarHashCombinado(string usuario, string password, string hashAlmacenado)
        {
            string hashInput = GenerarHashCombinado(usuario, password);
            return hashInput.Equals(hashAlmacenado, StringComparison.OrdinalIgnoreCase);
        }
    }
}
