using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.LogicaNegocio.Reportes
{
    /// <summary>
    /// Resultado de una agrupación: nombre del grupo, cantidad de elementos y promedio de un valor.
    /// </summary>
    /// <param name="Nombre">Nombre del grupo (por ejemplo, una categoría).</param>
    /// <param name="Cantidad">Cantidad de elementos del grupo.</param>
    /// <param name="Promedio">Promedio del valor medido en el grupo.</param>
    public record ResumenGrupo(string Nombre, int Cantidad, decimal Promedio);
}