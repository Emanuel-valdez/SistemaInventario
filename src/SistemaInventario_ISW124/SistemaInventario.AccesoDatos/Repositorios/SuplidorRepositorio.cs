using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;
using SistemaInventario.AccesoDatos.Interfaces;
using SistemaInventario.Entidades;

namespace SistemaInventario.AccesoDatos.Repositorios
{
    /// <summary>
    /// Repositorio de suplidores.
    /// </summary>
    public class SuplidorRepositorio : RepositorioBase<Suplidor>, ISuplidorRepositorio
    {
        /// <summary>Crea el repositorio con la conexión inyectada.</summary>
        public SuplidorRepositorio(ConexionBD conexion) : base(conexion)
        {
        }

        /// <inheritdoc />
        protected override string NombreTabla => "Suplidores";

        /// <inheritdoc />
        protected override Suplidor Mapear(SqlDataReader lector)
        {
            int posSitioWeb = lector.GetOrdinal("SitioWeb");

            var suplidor = new Suplidor
            {
                NombreEmpresa = lector.GetString(lector.GetOrdinal("NombreEmpresa")),
                NombreContacto = lector.GetString(lector.GetOrdinal("NombreContacto")),
                Telefono = lector.GetString(lector.GetOrdinal("Telefono")),
                Correo = lector.GetString(lector.GetOrdinal("Correo")),
                SitioWeb = lector.IsDBNull(posSitioWeb) ? null : lector.GetString(posSitioWeb)
            };
            MapearCamposBase(lector, suplidor);
            return suplidor;
        }

        /// <inheritdoc />
        public override int Insertar(Suplidor suplidor)
        {
            const string sql =
                "INSERT INTO Suplidores (NombreEmpresa, NombreContacto, Telefono, Correo, SitioWeb) " +
                "OUTPUT INSERTED.Id " +
                "VALUES (@NombreEmpresa, @NombreContacto, @Telefono, @Correo, @SitioWeb)";

            object? id = EjecutarEscalar(sql, CrearParametros(suplidor));
            return Convert.ToInt32(id);
        }

        /// <inheritdoc />
        public override bool Actualizar(Suplidor suplidor)
        {
            const string sql =
                "UPDATE Suplidores SET NombreEmpresa = @NombreEmpresa, NombreContacto = @NombreContacto, " +
                "Telefono = @Telefono, Correo = @Correo, SitioWeb = @SitioWeb, " +
                "FechaModificacion = SYSDATETIME() WHERE Id = @Id";

            var parametros = CrearParametros(suplidor).Append(new SqlParameter("@Id", suplidor.Id));
            return EjecutarComando(sql, parametros.ToArray()) > 0;
        }

        /// <inheritdoc />
        public bool ExisteNombreEmpresa(string nombreEmpresa, int? excluirId = null)
        {
            const string sql =
                "SELECT COUNT(*) FROM Suplidores " +
                "WHERE NombreEmpresa = @Nombre AND (@Excluir IS NULL OR Id <> @Excluir)";

            object? resultado = EjecutarEscalar(sql,
                new SqlParameter("@Nombre", nombreEmpresa),
                new SqlParameter("@Excluir", (object?)excluirId ?? DBNull.Value));

            return Convert.ToInt32(resultado) > 0;
        }

        /// <inheritdoc />
        public bool TieneProductosActivos(int suplidorId)
        {
            const string sql =
                "SELECT COUNT(*) FROM Productos WHERE SuplidorId = @Id AND Activo = 1";

            object? resultado = EjecutarEscalar(sql, new SqlParameter("@Id", suplidorId));
            return Convert.ToInt32(resultado) > 0;
        }

        private static SqlParameter[] CrearParametros(Suplidor suplidor)
        {
            return new[]
            {
                new SqlParameter("@NombreEmpresa", suplidor.NombreEmpresa),
                new SqlParameter("@NombreContacto", suplidor.NombreContacto),
                new SqlParameter("@Telefono", suplidor.Telefono),
                new SqlParameter("@Correo", suplidor.Correo),
                new SqlParameter("@SitioWeb",
                    string.IsNullOrWhiteSpace(suplidor.SitioWeb) ? DBNull.Value : (object)suplidor.SitioWeb)
            };
        }
    }
}