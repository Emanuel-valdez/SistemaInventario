using System;
using System.Collections.Generic;
using System.Text;
using SistemaInventario.Entidades;

namespace SistemaInventario.AccesoDatos.Interfaces
{
    /// <summary>
    /// Operaciones de acceso a datos propias de las categorías.
    /// </summary>
    public interface ICategoriaRepositorio : IRepositorio<Categoria>
    {
        /// <summary>Indica si ya existe una categoría con ese nombre (activa o eliminada).</summary>
        /// <param name="nombre">Nombre a buscar.</param>
        /// <param name="excluirId">Id a ignorar (para cuando se edita la misma categoría).</param>
        bool ExisteNombre(string nombre, int? excluirId = null);

        /// <summary>Indica si la categoría tiene productos activos asociados.</summary>
        bool TieneProductosActivos(int categoriaId);
    }
}