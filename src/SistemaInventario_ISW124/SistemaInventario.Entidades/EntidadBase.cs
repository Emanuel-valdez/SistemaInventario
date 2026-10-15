using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.Entidades
{
    /// <summary>
    /// Clase base de todas las entidades del dominio.
    /// Contiene los campos comunes y el indicador de borrado lógico.
    /// </summary>
    public abstract class EntidadBase
    {
        /// <summary>Identificador único del registro.</summary>
        public int Id { get; set; }

        /// <summary>Indica si el registro está activo (true) o en la papelera (false).</summary>
        public bool Activo { get; set; } = true;

        /// <summary>Fecha en que se creó el registro.</summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>Fecha de la última modificación del registro.</summary>
        public DateTime? FechaModificacion { get; set; }
    }
}
