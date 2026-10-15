using SistemaInventario.Entidades;
using SistemaInventario.LogicaNegocio.Comunes;
using System;
using System.Collections.Generic;
using System.Text;


namespace SistemaInventario.LogicaNegocio.Interfaces
{
    /// <summary>
    /// Operaciones de negocio sobre las categorías.
    /// </summary>
    public interface ICategoriaServicio
    {
        /// <summary>Lista las categorías activas.</summary>
        IEnumerable<Categoria> ObtenerActivas();

        /// <summary>Lista las categorías eliminadas (papelera).</summary>
        IEnumerable<Categoria> ObtenerEliminadas();

        /// <summary>Crea la categoría (Id = 0) o la edita (Id mayor a 0), validando antes.</summary>
        ResultadoOperacion<int> Guardar(Categoria categoria);

        /// <summary>Elimina lógicamente la categoría si no tiene productos activos.</summary>
        ResultadoOperacion<bool> Eliminar(int id);

        /// <summary>Restaura una categoría de la papelera.</summary>
        ResultadoOperacion<bool> Restaurar(int id);
    }
}