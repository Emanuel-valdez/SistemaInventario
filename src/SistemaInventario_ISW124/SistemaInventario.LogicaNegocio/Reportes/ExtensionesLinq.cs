using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.LogicaNegocio.Reportes
{
    /// <summary>
    /// Métodos de extensión genéricos propios para consultas LINQ.
    /// </summary>
    public static class ExtensionesLinq
    {
        /// <summary>
        /// Agrupa los elementos por una clave de texto y calcula, por cada grupo,
        /// la cantidad de elementos y el promedio de un valor numérico.
        /// </summary>
        /// <typeparam name="T">Tipo de los elementos de la colección.</typeparam>
        /// <param name="origen">Colección a agrupar.</param>
        /// <param name="clave">Función que devuelve el nombre del grupo de cada elemento.</param>
        /// <param name="valor">Función que devuelve el valor numérico a promediar.</param>
        /// <returns>Un resumen por cada grupo.</returns>
        public static IEnumerable<ResumenGrupo> ResumirPor<T>(this IEnumerable<T> origen,
                                                             Func<T, string> clave,
                                                             Func<T, decimal> valor)
        {
            return origen
                .GroupBy(clave)
                .Select(g => new ResumenGrupo(g.Key, g.Count(), g.Average(valor)));
        }
    }
}
