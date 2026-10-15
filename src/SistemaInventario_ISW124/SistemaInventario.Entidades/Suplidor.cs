using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.Entidades
{
    /// <summary>
    /// Representa un suplidor (proveedor) de productos.
    /// </summary>
    public class Suplidor : EntidadBase
    {
        /// <summary>Nombre único de la empresa.</summary>
        public string NombreEmpresa { get; set; } = string.Empty;

        /// <summary>Nombre de la persona de contacto.</summary>
        public string NombreContacto { get; set; } = string.Empty;

        /// <summary>Teléfono en formato dominicano (809/829/849-XXX-XXXX).</summary>
        public string Telefono { get; set; } = string.Empty;

        /// <summary>Correo electrónico del suplidor.</summary>
        public string Correo { get; set; } = string.Empty;

        /// <summary>Sitio web (opcional).</summary>
        public string? SitioWeb { get; set; }

        /// <summary>Devuelve el nombre de la empresa.</summary>
        public override string ToString() => NombreEmpresa;
    }
}