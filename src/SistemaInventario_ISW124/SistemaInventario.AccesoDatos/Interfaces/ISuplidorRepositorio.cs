using System;
using System.Collections.Generic;
using System.Text;
using SistemaInventario.Entidades;

namespace SistemaInventario.AccesoDatos.Interfaces
{
    /// <summary>
    /// Operaciones de acceso a datos propias de los suplidores.
    /// </summary>
    public interface ISuplidorRepositorio : IRepositorio<Suplidor>
    {
        /// <summary>Indica si ya existe un suplidor con ese nombre de empresa (activo o eliminado).</summary>
        /// <param name="nombreEmpresa">Nombre a buscar.</param>
        /// <param name="excluirId">Id a ignorar (para cuando se edita el mismo suplidor).</param>
        bool ExisteNombreEmpresa(string nombreEmpresa, int? excluirId = null);

        /// <summary>Indica si el suplidor tiene productos activos asociados.</summary>
        bool TieneProductosActivos(int suplidorId);
    }
}
