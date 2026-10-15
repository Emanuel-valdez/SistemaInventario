using System;
using System.Collections.Generic;
using System.Text;

using SistemaInventario.Entidades;
using SistemaInventario.LogicaNegocio.Reportes;

namespace SistemaInventario.LogicaNegocio.Interfaces
{
    /// <summary>
    /// Consultas de resumen y análisis sobre productos, categorías y suplidores.
    /// </summary>
    public interface IReporteServicio
    {
        /// <summary>Productos activos agrupados por categoría, con su conteo.</summary>
        IEnumerable<ResumenGrupo> ProductosPorCategoria();

        /// <summary>Los suplidores con más productos activos.</summary>
        /// <param name="cantidad">Cuántos suplidores devolver.</param>
        IEnumerable<ResumenGrupo> TopSuplidores(int cantidad = 5);

        /// <summary>Promedio de precios por categoría, del más alto al más bajo.</summary>
        IEnumerable<ResumenGrupo> PromedioPrecioPorCategoria();

        /// <summary>
        /// Busca productos por nombre (parcial, sin importar mayúsculas), categoría y rango de precios.
        /// </summary>
        /// <param name="texto">Parte del nombre (puede ser vacío).</param>
        /// <param name="categoriaId">Categoría a filtrar, o null para todas.</param>
        /// <param name="precioMinimo">Precio mínimo, o null.</param>
        /// <param name="precioMaximo">Precio máximo, o null.</param>
        IEnumerable<Producto> BuscarProductos(string? texto, int? categoriaId,
                                              decimal? precioMinimo, decimal? precioMaximo);

        /// <summary>El producto más caro de cada categoría.</summary>
        IEnumerable<Producto> ProductoMasCaroPorCategoria();

        /// <summary>Suplidores activos que no tienen ningún producto activo.</summary>
        IEnumerable<Suplidor> SuplidoresSinProductos();
    }
}