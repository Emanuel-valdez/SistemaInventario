using System;
using System.Collections.Generic;
using System.Text;
using SistemaInventario.AccesoDatos.Interfaces;
using SistemaInventario.Entidades;
using SistemaInventario.LogicaNegocio.Interfaces;
using SistemaInventario.LogicaNegocio.Reportes;

namespace SistemaInventario.LogicaNegocio.Servicios
{
    /// <summary>
    /// Implementa las consultas LINQ de resumen y análisis de la aplicación.
    /// </summary>
    public class ReporteServicio : IReporteServicio
    {
        private const string SinCategoria = "Sin categoría";

        private readonly IProductoRepositorio _productos;
        private readonly ISuplidorRepositorio _suplidores;

        /// <summary>Crea el servicio con los repositorios inyectados.</summary>
        public ReporteServicio(IProductoRepositorio productos, ISuplidorRepositorio suplidores)
        {
            _productos = productos;
            _suplidores = suplidores;
        }

        /// <inheritdoc />
        /// <remarks>LINQ 1: GroupBy + Count (mediante el método genérico ResumirPor).</remarks>
        public IEnumerable<ResumenGrupo> ProductosPorCategoria()
        {
            return _productos.ObtenerActivos()
                .ResumirPor(p => p.Categoria?.Nombre ?? SinCategoria, p => p.PrecioUnitario)
                .OrderByDescending(r => r.Cantidad)
                .ThenBy(r => r.Nombre)
                .ToList();
        }

        /// <inheritdoc />
        /// <remarks>LINQ 2: GroupBy + OrderByDescending + Take.</remarks>
        public IEnumerable<ResumenGrupo> TopSuplidores(int cantidad = 5)
        {
            return _productos.ObtenerActivos()
                .ResumirPor(p => p.Suplidor?.NombreEmpresa ?? "Sin suplidor", p => p.PrecioUnitario)
                .OrderByDescending(r => r.Cantidad)
                .ThenBy(r => r.Nombre)
                .Take(cantidad)
                .ToList();
        }

        /// <inheritdoc />
        /// <remarks>LINQ 3: GroupBy + Average.</remarks>
        public IEnumerable<ResumenGrupo> PromedioPrecioPorCategoria()
        {
            return _productos.ObtenerActivos()
                .ResumirPor(p => p.Categoria?.Nombre ?? SinCategoria, p => p.PrecioUnitario)
                .OrderByDescending(r => r.Promedio)
                .ToList();
        }

        /// <inheritdoc />
        /// <remarks>LINQ 4: Where encadenados (búsqueda parcial sin mayúsculas, categoría y rango de precios).</remarks>
        public IEnumerable<Producto> BuscarProductos(string? texto, int? categoriaId,
                                                     decimal? precioMinimo, decimal? precioMaximo)
        {
            string parte = (texto ?? string.Empty).Trim();

            return _productos.ObtenerActivos()
                .Where(p => p.Nombre.Contains(parte, StringComparison.OrdinalIgnoreCase))
                .Where(p => categoriaId == null || p.CategoriaId == categoriaId)
                .Where(p => precioMinimo == null || p.PrecioUnitario >= precioMinimo)
                .Where(p => precioMaximo == null || p.PrecioUnitario <= precioMaximo)
                .OrderBy(p => p.PrecioUnitario)
                .ToList();
        }

        /// <inheritdoc />
        /// <remarks>LINQ 5: GroupBy + MaxBy.</remarks>
        public IEnumerable<Producto> ProductoMasCaroPorCategoria()
        {
            return _productos.ObtenerActivos()
                .GroupBy(p => p.Categoria?.Nombre ?? SinCategoria)
                .Select(grupo => grupo.MaxBy(p => p.PrecioUnitario)!)
                .OrderBy(p => p.Categoria?.Nombre)
                .ToList();
        }

        /// <inheritdoc />
        /// <remarks>LINQ 6: GroupJoin (unión externa izquierda) + Where.</remarks>
        public IEnumerable<Suplidor> SuplidoresSinProductos()
        {
            List<Producto> productosActivos = _productos.ObtenerActivos().ToList();

            return _suplidores.ObtenerActivos()
                .GroupJoin(productosActivos,
                           suplidor => suplidor.Id,
                           producto => producto.SuplidorId,
                           (suplidor, productos) => new { Suplidor = suplidor, Total = productos.Count() })
                .Where(x => x.Total == 0)
                .Select(x => x.Suplidor)
                .OrderBy(s => s.NombreEmpresa)
                .ToList();
        }
    }
}