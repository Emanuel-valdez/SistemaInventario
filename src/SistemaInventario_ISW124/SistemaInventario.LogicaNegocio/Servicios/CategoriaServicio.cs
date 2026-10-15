using System;
using System.Collections.Generic;
using System.Text;
using SistemaInventario.AccesoDatos.Interfaces;
using SistemaInventario.AccesoDatos.Utilidades;
using SistemaInventario.Entidades;
using SistemaInventario.Entidades.Excepciones;
using SistemaInventario.LogicaNegocio.Comunes;
using SistemaInventario.LogicaNegocio.Interfaces;
using SistemaInventario.LogicaNegocio.Validaciones;

namespace SistemaInventario.LogicaNegocio.Servicios
{
    /// <summary>
    /// Reglas de negocio de las categorías.
    /// </summary>
    public class CategoriaServicio : ICategoriaServicio
    {
        private const int MaximoFotos = 5;
        private readonly ICategoriaRepositorio _repositorio;

        /// <summary>Crea el servicio con el repositorio inyectado.</summary>
        public CategoriaServicio(ICategoriaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        /// <inheritdoc />
        public IEnumerable<Categoria> ObtenerActivas() => _repositorio.ObtenerActivos();

        /// <inheritdoc />
        public IEnumerable<Categoria> ObtenerEliminadas() => _repositorio.ObtenerEliminados();

        /// <inheritdoc />
        public ResultadoOperacion<int> Guardar(Categoria categoria)
        {
            categoria.Nombre = (categoria.Nombre ?? string.Empty).Trim();
            categoria.Descripcion = (categoria.Descripcion ?? string.Empty).Trim();

            Dictionary<string, string> errores = Validar(categoria);
            if (errores.Count > 0)
            {
                return ResultadoOperacion<int>.ConErrores(errores);
            }

            try
            {
                int? excluirId = categoria.Id == 0 ? null : categoria.Id;
                if (_repositorio.ExisteNombre(categoria.Nombre, excluirId))
                {
                    return ResultadoOperacion<int>.ConErrores(new Dictionary<string, string>
                    {
                        { "Nombre", "Ya existe una categoría con ese nombre." }
                    });
                }

                CopiarFotosNuevas(categoria);

                if (categoria.Id == 0)
                {
                    int id = _repositorio.Insertar(categoria);
                    return ResultadoOperacion<int>.Correcto(id, "Categoría creada correctamente.");
                }

                return _repositorio.Actualizar(categoria)
                    ? ResultadoOperacion<int>.Correcto(categoria.Id, "Categoría actualizada correctamente.")
                    : ResultadoOperacion<int>.Fallido("No se encontró la categoría.");
            }
            catch (AccesoDatosException ex)
            {
                return ResultadoOperacion<int>.Fallido(ex.Message);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Logger.RegistrarError("CategoriaServicio.Guardar", ex);
                return ResultadoOperacion<int>.Fallido("No se pudieron copiar las fotos de la categoría.");
            }
        }

        /// <inheritdoc />
        public ResultadoOperacion<bool> Eliminar(int id)
        {
            try
            {
                if (_repositorio.TieneProductosActivos(id))
                {
                    return ResultadoOperacion<bool>.Fallido(
                        "No se puede eliminar la categoría porque tiene productos activos asociados.");
                }

                return _repositorio.Eliminar(id)
                    ? ResultadoOperacion<bool>.Correcto(true, "Categoría enviada a la papelera.")
                    : ResultadoOperacion<bool>.Fallido("No se encontró la categoría.");
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
                return _repositorio.Restaurar(id)
                    ? ResultadoOperacion<bool>.Correcto(true, "Categoría restaurada.")
                    : ResultadoOperacion<bool>.Fallido("No se encontró la categoría.");
            }
            catch (AccesoDatosException ex)
            {
                return ResultadoOperacion<bool>.Fallido(ex.Message);
            }
        }

        private static Dictionary<string, string> Validar(Categoria categoria)
        {
            var errores = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(categoria.Nombre))
            {
                errores["Nombre"] = "El nombre es obligatorio.";
            }
            else if (categoria.Nombre.Length < 3 || categoria.Nombre.Length > 100)
            {
                errores["Nombre"] = "El nombre debe tener entre 3 y 100 caracteres.";
            }

            if (string.IsNullOrWhiteSpace(categoria.Descripcion))
            {
                errores["Descripcion"] = "La descripción es obligatoria.";
            }
            else if (categoria.Descripcion.Length > 500)
            {
                errores["Descripcion"] = "La descripción no puede pasar de 500 caracteres.";
            }

            if (categoria.Fotos.Count > MaximoFotos)
            {
                errores["Fotos"] = "Una categoría puede tener como máximo 5 fotos.";
            }
            else
            {
                foreach (CategoriaFoto foto in categoria.Fotos)
                {
                    string? error = ValidadorCampos.ValidarFoto(RutaFotos.Resolver(foto.RutaArchivo));
                    if (error != null)
                    {
                        errores["Fotos"] = error;
                        break;
                    }
                }
            }

            return errores;
        }

        /// <summary>
        /// Copia a la carpeta Fotos las imágenes recién elegidas (ruta completa)
        /// y deja en la entidad la ruta relativa. Las que ya están guardadas no se tocan.
        /// </summary>
        private static void CopiarFotosNuevas(Categoria categoria)
        {
            Directory.CreateDirectory(RutaFotos.CarpetaAbsoluta);

            foreach (CategoriaFoto foto in categoria.Fotos)
            {
                if (!Path.IsPathRooted(foto.RutaArchivo))
                {
                    continue;
                }

                string extension = Path.GetExtension(foto.RutaArchivo).ToLowerInvariant();
                string nombreNuevo = Guid.NewGuid().ToString("N") + extension;
                string destino = Path.Combine(RutaFotos.CarpetaAbsoluta, nombreNuevo);

                File.Copy(foto.RutaArchivo, destino);
                foto.RutaArchivo = Path.Combine(RutaFotos.NombreCarpeta, nombreNuevo);
            }
        }
    }
}