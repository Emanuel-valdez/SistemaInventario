using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.LogicaNegocio.Comunes
{
    /// <summary>
    /// Utilidades para ubicar las fotos de las categorías.
    /// </summary>
    public static class RutaFotos
    {
        /// <summary>Nombre de la carpeta donde se guardan las fotos.</summary>
        public const string NombreCarpeta = "Fotos";

        /// <summary>Ruta completa de la carpeta de fotos.</summary>
        public static string CarpetaAbsoluta =>
            Path.Combine(AppContext.BaseDirectory, NombreCarpeta);

        /// <summary>
        /// Convierte una ruta guardada (relativa) en ruta completa.
        /// Si ya es completa, la devuelve igual.
        /// </summary>
        public static string Resolver(string ruta) =>
            Path.IsPathRooted(ruta) ? ruta : Path.Combine(AppContext.BaseDirectory, ruta);
    }
}