using System;
using System.Collections.Generic;
using System.Text;
using SistemaInventario.Entidades;
using SistemaInventario.LogicaNegocio.Comunes;

namespace SistemaInventario.LogicaNegocio.Interfaces
{
    /// <summary>
    /// Operaciones de negocio sobre los suplidores.
    /// </summary>
    public interface ISuplidorServicio
    {
        /// <summary>Lista los suplidores activos.</summary>
        IEnumerable<Suplidor> ObtenerActivos();

        /// <summary>Lista los suplidores eliminados (papelera).</summary>
        IEnumerable<Suplidor> ObtenerEliminados();

        /// <summary>Crea el suplidor (Id = 0) o lo edita (Id mayor a 0), validando antes.</summary>
        ResultadoOperacion<int> Guardar(Suplidor suplidor);

        /// <summary>Elimina lógicamente el suplidor si no tiene productos activos.</summary>
        ResultadoOperacion<bool> Eliminar(int id);

        /// <summary>Restaura un suplidor de la papelera.</summary>
        ResultadoOperacion<bool> Restaurar(int id);
    }
}