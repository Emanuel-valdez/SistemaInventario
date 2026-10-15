using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.LogicaNegocio.Comunes
{
    /// <summary>
    /// Resultado genérico de una operación de negocio.
    /// Indica si funcionó, el valor devuelto, un mensaje y los errores por campo.
    /// </summary>
    /// <typeparam name="T">Tipo del valor devuelto en caso de éxito.</typeparam>
    public class ResultadoOperacion<T>
    {
        /// <summary>True si la operación se realizó correctamente.</summary>
        public bool Exito { get; private set; }

        /// <summary>Valor devuelto por la operación (por ejemplo, el Id creado).</summary>
        public T? Valor { get; private set; }

        /// <summary>Mensaje para mostrar al usuario.</summary>
        public string Mensaje { get; private set; } = string.Empty;

        /// <summary>Errores de validación: clave = nombre del campo, valor = mensaje.</summary>
        public Dictionary<string, string> Errores { get; private set; } = new Dictionary<string, string>();

        private ResultadoOperacion()
        {
        }

        /// <summary>Crea un resultado exitoso.</summary>
        public static ResultadoOperacion<T> Correcto(T valor, string mensaje = "") =>
            new ResultadoOperacion<T> { Exito = true, Valor = valor, Mensaje = mensaje };

        /// <summary>Crea un resultado fallido con un mensaje general.</summary>
        public static ResultadoOperacion<T> Fallido(string mensaje) =>
            new ResultadoOperacion<T> { Exito = false, Mensaje = mensaje };

        /// <summary>Crea un resultado fallido con errores por campo (para ErrorProvider).</summary>
        public static ResultadoOperacion<T> ConErrores(Dictionary<string, string> errores) =>
            new ResultadoOperacion<T>
            {
                Exito = false,
                Mensaje = "Corrige los campos marcados.",
                Errores = errores
            };
    }
}