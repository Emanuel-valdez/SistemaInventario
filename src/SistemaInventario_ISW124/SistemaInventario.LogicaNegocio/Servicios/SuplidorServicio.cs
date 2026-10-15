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
    /// Reglas de negocio de los suplidores.
    /// </summary>
    public class SuplidorServicio : ISuplidorServicio
    {
        private readonly ISuplidorRepositorio _repositorio;

        /// <summary>Crea el servicio con el repositorio inyectado.</summary>
        public SuplidorServicio(ISuplidorRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        /// <inheritdoc />
        public IEnumerable<Suplidor> ObtenerActivos() => _repositorio.ObtenerActivos();

        /// <inheritdoc />
        public IEnumerable<Suplidor> ObtenerEliminados() => _repositorio.ObtenerEliminados();

        /// <inheritdoc />
        public ResultadoOperacion<int> Guardar(Suplidor suplidor)
        {
            suplidor.NombreEmpresa = (suplidor.NombreEmpresa ?? string.Empty).Trim();
            suplidor.NombreContacto = (suplidor.NombreContacto ?? string.Empty).Trim();
            suplidor.Telefono = (suplidor.Telefono ?? string.Empty).Trim();
            suplidor.Correo = (suplidor.Correo ?? string.Empty).Trim();
            suplidor.SitioWeb = string.IsNullOrWhiteSpace(suplidor.SitioWeb)
                ? null
                : suplidor.SitioWeb.Trim();

            Dictionary<string, string> errores = Validar(suplidor);
            if (errores.Count > 0)
            {
                return ResultadoOperacion<int>.ConErrores(errores);
            }

            try
            {
                int? excluirId = suplidor.Id == 0 ? null : suplidor.Id;
                if (_repositorio.ExisteNombreEmpresa(suplidor.NombreEmpresa, excluirId))
                {
                    return ResultadoOperacion<int>.ConErrores(new Dictionary<string, string>
                    {
                        { "NombreEmpresa", "Ya existe un suplidor con ese nombre de empresa." }
                    });
                }

                if (suplidor.Id == 0)
                {
                    int id = _repositorio.Insertar(suplidor);
                    return ResultadoOperacion<int>.Correcto(id, "Suplidor creado correctamente.");
                }

                return _repositorio.Actualizar(suplidor)
                    ? ResultadoOperacion<int>.Correcto(suplidor.Id, "Suplidor actualizado correctamente.")
                    : ResultadoOperacion<int>.Fallido("No se encontró el suplidor.");
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
                if (_repositorio.TieneProductosActivos(id))
                {
                    return ResultadoOperacion<bool>.Fallido(
                        "No se puede eliminar el suplidor porque tiene productos activos asociados.");
                }

                return _repositorio.Eliminar(id)
                    ? ResultadoOperacion<bool>.Correcto(true, "Suplidor enviado a la papelera.")
                    : ResultadoOperacion<bool>.Fallido("No se encontró el suplidor.");
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
                    ? ResultadoOperacion<bool>.Correcto(true, "Suplidor restaurado.")
                    : ResultadoOperacion<bool>.Fallido("No se encontró el suplidor.");
            }
            catch (AccesoDatosException ex)
            {
                return ResultadoOperacion<bool>.Fallido(ex.Message);
            }
        }

        private static Dictionary<string, string> Validar(Suplidor suplidor)
        {
            var errores = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(suplidor.NombreEmpresa))
            {
                errores["NombreEmpresa"] = "El nombre de la empresa es obligatorio.";
            }
            else if (suplidor.NombreEmpresa.Length > 150)
            {
                errores["NombreEmpresa"] = "El nombre de la empresa no puede pasar de 150 caracteres.";
            }

            if (string.IsNullOrWhiteSpace(suplidor.NombreContacto))
            {
                errores["NombreContacto"] = "El nombre del contacto es obligatorio.";
            }
            else if (suplidor.NombreContacto.Length > 100)
            {
                errores["NombreContacto"] = "El nombre del contacto no puede pasar de 100 caracteres.";
            }

            if (!ValidadorCampos.EsTelefonoValido(suplidor.Telefono))
            {
                errores["Telefono"] = "Formato inválido. Usa 809/829/849-XXX-XXXX.";
            }

            if (!ValidadorCampos.EsCorreoValido(suplidor.Correo))
            {
                errores["Correo"] = "Ingresa un correo electrónico válido.";
            }
            else if (suplidor.Correo.Length > 150)
            {
                errores["Correo"] = "El correo no puede pasar de 150 caracteres.";
            }

            if (suplidor.SitioWeb != null)
            {
                if (!ValidadorCampos.EsUrlValida(suplidor.SitioWeb))
                {
                    errores["SitioWeb"] = "Ingresa una URL válida (por ejemplo https://www.ejemplo.com).";
                }
                else if (suplidor.SitioWeb.Length > 200)
                {
                    errores["SitioWeb"] = "La URL no puede pasar de 200 caracteres.";
                }
            }

            return errores;
        }
    }
}