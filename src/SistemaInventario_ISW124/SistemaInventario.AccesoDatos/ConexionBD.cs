using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace SistemaInventario.AccesoDatos
{
    /// <summary>
    /// Singleton que guarda la cadena de conexión y entrega conexiones a SQL Server.
    /// </summary>
    public sealed class ConexionBD
    {
        private static readonly Lazy<ConexionBD> _instancia =
            new Lazy<ConexionBD>(() => new ConexionBD());

        private readonly string _cadenaConexion;

        private ConexionBD()
        {
            _cadenaConexion = LeerCadenaConexion();
        }

        /// <summary>Única instancia de la clase.</summary>
        public static ConexionBD Instancia => _instancia.Value;

        /// <summary>
        /// Crea una conexión nueva (sin abrir). Quien la use debe cerrarla con using.
        /// </summary>
        public SqlConnection ObtenerConexion() => new SqlConnection(_cadenaConexion);

        private static string LeerCadenaConexion()
        {
            string ruta = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

            if (!File.Exists(ruta))
            {
                throw new FileNotFoundException(
                    "No se encontró appsettings.json junto al ejecutable.", ruta);
            }

            using JsonDocument documento = JsonDocument.Parse(File.ReadAllText(ruta));
            string? cadena = documento.RootElement.GetProperty("CadenaConexion").GetString();

            if (string.IsNullOrWhiteSpace(cadena))
            {
                throw new InvalidOperationException("La cadena de conexión está vacía.");
            }

            return cadena;
        }
    }
}