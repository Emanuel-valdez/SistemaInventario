using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.AccesoDatos.Utilidades
{
    /// <summary>
    /// Registra los errores de la aplicación en el archivo errores.log.
    /// </summary>
    public static class Logger
    {
        private static readonly object _bloqueo = new object();
        private static readonly string _rutaArchivo =
            Path.Combine(AppContext.BaseDirectory, "errores.log");

        /// <summary>
        /// Escribe un error en errores.log con fecha, origen y mensaje.
        /// </summary>
        /// <param name="origen">Clase o método donde ocurrió el error.</param>
        /// <param name="excepcion">Excepción capturada.</param>
        public static void RegistrarError(string origen, Exception excepcion)
        {
            try
            {
                lock (_bloqueo)
                {
                    string linea = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{origen}] " +
                                   $"{excepcion.GetType().Name}: {excepcion.Message}" +
                                   Environment.NewLine;
                    File.AppendAllText(_rutaArchivo, linea);
                }
            }
            catch (IOException)
            {
                // Si no se puede escribir el log, no se debe romper la aplicación.
            }
        }
    }
}