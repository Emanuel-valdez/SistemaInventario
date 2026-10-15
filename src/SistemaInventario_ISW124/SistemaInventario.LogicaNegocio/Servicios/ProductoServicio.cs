using System;
using System.Collections.Generic;
using System.Text;
using SistemaInventario.AccesoDatos.Interfaces;
using SistemaInventario.Entidades;
using SistemaInventario.Entidades.Excepciones;
using SistemaInventario.LogicaNegocio.Comunes;
using SistemaInventario.LogicaNegocio.Interfaces;
using SistemaInventario.LogicaNegocio.Validaciones;

namespace SistemaInventario.LogicaNegocio.Servicios
{
    /// <summary>
    /// Reglas de negocio de los productos.
    /// </summary>
    public class ProductoServicio : IProductoServicio
    {
        private const decimal PrecioMaximo = 99999999.99m;

        private readonly IProductoRepositorio _productos;
        private readonly ICategoriaRepositorio _categorias;
        private readonly ISuplidorRepositorio _suplidores;

        /// <summary>Crea el servicio con los repositorios inyectados.</summary>
        public ProductoServicio(IProductoRepositorio productos,
                                ICategoriaRepositorio categorias,
                                ISuplidorRepositorio suplidores)
        {
            _productos = productos;
            _categorias = categorias;
            _suplidores = suplidores;
        }

        /// <inheritdoc />
        public IEnumerable<Producto> ObtenerActivos() => _productos.ObtenerActivos();

        /// <inheritdoc />
        public IEnumerable<Producto> ObtenerEliminados() => _productos.ObtenerEliminados();

        /// <inheritdoc />
        public ResultadoOperacion<int> Guardar(Producto producto)
        {
            producto.Nombre = (producto.Nombre ?? string.Empty).Trim();

            try
            {
                Dictionary<string, string> errores = Validar(producto);
                if (errores.Count > 0)
                {
                    return ResultadoOperacion<int>.ConErrores(errores);
                }

                if (producto.Id == 0)
                {
                    int id = _productos.Insertar(producto);
                    return ResultadoOperacion<int>.Correcto(id, "Producto creado correctamente.");
                }

                return _productos.Actualizar(producto)
                    ? ResultadoOperacion<int>.Correcto(producto.Id, "Producto actualizado correctamente.")
                    : ResultadoOperacion<int>.Fallido("No se encontró el producto.");
            }
            catch (AccesoDatosException ex)
            {
                return ResultadoOperacion<int>.Fallido(ex.Message);
            }
        }

        /// <inheritdoc />
        public ResultadoOperacion<bool> Eliminar(int id)
        {
            try
            {
                return _productos.Eliminar(id)
                    ? ResultadoOperacion<bool>.Correcto(true, "Producto enviado a la papelera.")
                    : ResultadoOperacion<bool>.Fallido("No se encontró el producto.");
            }
            catch (AccesoDatosException ex)
            {
                return ResultadoOperacion<bool>.Fallido(ex.Message);
            }
        }

        /// <inheritdoc />
        public ResultadoOperacion<bool> Restaurar(int id)
        {
            try
            {
                Producto? producto = _productos.ObtenerPorId(id);
                if (producto == null)
                {
                    return ResultadoOperacion<bool>.Fallido("No se encontró el producto.");
                }

                Categoria? categoria = _categorias.ObtenerPorId(producto.CategoriaId);
                if (categoria == null || !categoria.Activo)
                {
                    return ResultadoOperacion<bool>.Fallido(
                        "No se puede restaurar el producto porque su categoría está eliminada. Restaura la categoría primero.");
                }

                Suplidor? suplidor = _suplidores.ObtenerPorId(producto.SuplidorId);
                if (suplidor == null || !suplidor.Activo)
                {
                    return ResultadoOperacion<bool>.Fallido(
                        "No se puede restaurar el producto porque su suplidor está eliminado. Restaura el suplidor primero.");
                }

                return _productos.Restaurar(id)
                    ? ResultadoOperacion<bool>.Correcto(true, "Producto restaurado.")
                    : ResultadoOperacion<bool>.Fallido("No se encontró el producto.");
            }
            catch (AccesoDatosException ex)
            {
                return ResultadoOperacion<bool>.Fallido(ex.Message);
            }
        }

        private Dictionary<string, string> Validar(Producto producto)
        {
            var errores = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(producto.Nombre))
            {
                errores["Nombre"] = "El nombre del producto es obligatorio.";
            }
            else if (producto.Nombre.Length > 150)
            {
                errores["Nombre"] = "El nombre no puede pasar de 150 caracteres.";
            }

            if (producto.PrecioUnitario <= 0)
            {
                errores["PrecioUnitario"] = "El precio debe ser mayor a 0.";
            }
            else if (!ValidadorCampos.TieneMaximoDosDecimales(producto.PrecioUnitario))
            {
                errores["PrecioUnitario"] = "El precio puede tener como máximo 2 decimales.";
            }
            else if (producto.PrecioUnitario > PrecioMaximo)
            {
                errores["PrecioUnitario"] = "El precio es demasiado alto.";
            }

            if (producto.CategoriaId <= 0)
            {
                errores["CategoriaId"] = "Selecciona una categoría.";
            }
            else
            {
                Categoria? categoria = _categorias.ObtenerPorId(producto.CategoriaId);
                if (categoria == null || !categoria.Activo)
                {
                    errores["CategoriaId"] = "La categoría debe existir y estar activa.";
                }
            }

            if (producto.SuplidorId <= 0)
            {
                errores["SuplidorId"] = "Selecciona un suplidor.";
            }
            else
            {
                Suplidor? suplidor = _suplidores.ObtenerPorId(producto.SuplidorId);
                if (suplidor == null || !suplidor.Activo)
                {
                    errores["SuplidorId"] = "El suplidor debe existir y estar activo.";
                }
            }

            return errores;
        }
    }
}