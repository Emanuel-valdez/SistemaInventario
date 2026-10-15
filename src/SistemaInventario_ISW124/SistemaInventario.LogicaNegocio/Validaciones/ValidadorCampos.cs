using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SistemaInventario.LogicaNegocio.Validaciones
{
    /// <summary>
    /// Reglas de validación reutilizables (correo, teléfono, URL, precio y fotos).
    /// </summary>
    public static class ValidadorCampos
    {
        /// <summary>Tamaño máximo permitido para una foto (2 MB).</summary>
        public const long TamanoMaximoFotoBytes = 2 * 1024 * 1024;

        private static readonly Regex _correo =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        private static readonly Regex _telefono =
            new Regex(@"^(809|829|849)-\d{3}-\d{4}$", RegexOptions.Compiled);

        private static readonly string[] _extensionesFoto = { ".jpg", ".jpeg", ".png" };

        /// <summary>Valida el formato de un correo electrónico.</summary>
        public static bool EsCorreoValido(string? valor) =>
            !string.IsNullOrWhiteSpace(valor) && _correo.IsMatch(valor);

        /// <summary>Valida el formato dominicano 809/829/849-XXX-XXXX.</summary>
        public static bool EsTelefonoValido(string? valor) =>
            !string.IsNullOrWhiteSpace(valor) && _telefono.IsMatch(valor);

        /// <summary>Valida que sea una URL http o https.</summary>
        public static bool EsUrlValida(string? valor) =>
            Uri.TryCreate(valor, UriKind.Absolute, out Uri? uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        /// <summary>Valida que el número tenga como máximo 2 decimales.</summary>
        public static bool TieneMaximoDosDecimales(decimal valor) =>
            decimal.Round(valor, 2) == valor;

        /// <summary>
        /// Valida una foto: extensión .jpg/.jpeg/.png, que exista y que pese máximo 2 MB.
        /// </summary>
        /// <param name="rutaCompleta">Ruta completa del archivo.</param>
        /// <returns>El mensaje de error, o null si la foto es válida.</returns>
        public static string? ValidarFoto(string rutaCompleta)
        {
            string nombre = Path.GetFileName(rutaCompleta);
            string extension = Path.GetExtension(rutaCompleta).ToLowerInvariant();

            if (!_extensionesFoto.Contains(extension))
            {
                return $"La foto {nombre} no es válida. Solo se permiten .jpg, .jpeg o .png.";
            }

            if (!File.Exists(rutaCompleta))
            {
                return $"No se encontró el archivo de la foto {nombre}.";
            }

            if (new FileInfo(rutaCompleta).Length > TamanoMaximoFotoBytes)
            {
                return $"La foto {nombre} supera el máximo de 2 MB.";
            }

            return null;
        }
    }
}