using System;
using System.Collections.Generic;
using System.Text;
using SistemaInventario.AccesoDatos;
using SistemaInventario.AccesoDatos.Repositorios;
using SistemaInventario.LogicaNegocio.Interfaces;
using SistemaInventario.LogicaNegocio.Servicios;

namespace SistemaInventario.LogicaNegocio
{
    /// <summary>
    /// Crea los servicios con todas sus dependencias ya inyectadas.
    /// La capa de Presentación usa esta clase en lugar de conocer el acceso a datos.
    /// </summary>
    public static class FabricaServicios
    {
        /// <summary>Crea el servicio de categorías.</summary>
        public static ICategoriaServicio CrearCategoriaServicio() =>
            new CategoriaServicio(new CategoriaRepositorio(ConexionBD.Instancia));

        /// <summary>Crea el servicio de suplidores.</summary>
        public static ISuplidorServicio CrearSuplidorServicio() =>
            new SuplidorServicio(new SuplidorRepositorio(ConexionBD.Instancia));

        /// <summary>Crea el servicio de productos.</summary>
        public static IProductoServicio CrearProductoServicio()
        {
            ConexionBD conexion = ConexionBD.Instancia;
            return new ProductoServicio(
                new ProductoRepositorio(conexion),
                new CategoriaRepositorio(conexion),
                new SuplidorRepositorio(conexion));
        }

        /// <summary>Crea el servicio de reportes y consultas LINQ.</summary>
        public static IReporteServicio CrearReporteServicio()
        {
            ConexionBD conexion = ConexionBD.Instancia;
            return new ReporteServicio(
                new ProductoRepositorio(conexion),
                new SuplidorRepositorio(conexion));
        }
    }
}