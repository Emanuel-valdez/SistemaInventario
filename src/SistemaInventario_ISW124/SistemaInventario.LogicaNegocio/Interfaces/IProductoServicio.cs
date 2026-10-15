using System;
using System.Collections.Generic;
using System.Text;
using SistemaInventario.Entidades;
using SistemaInventario.LogicaNegocio.Comunes;

namespace SistemaInventario.LogicaNegocio.Interfaces
{
    /// <summary>
    /// Operaciones de negocio sobre los productos.
    /// </summary>
    public interface IProductoServicio
    {
        /// <summary>Lista los productos activos.</summary>
        IEnumerable<Producto> ObtenerActivos();

        /// <summary>Lista los productos eliminados (papelera).</summary>
        IEnumerable<Producto> ObtenerEliminados();

        /// <summary>Crea el producto (Id = 0) o lo edita (Id mayor a 0), validando antes.</summary>
        ResultadoOperacion<int> Guardar(Producto producto);

        /// <summary>Elimina lógicamente el producto.</summary>
        ResultadoOperacion<bool> Eliminar(int id);

        /// <summary>Restaura un producto si su categoría y su suplidor están activos.</summary>
        ResultadoOperacion<bool> Restaurar(int id);
    }
}