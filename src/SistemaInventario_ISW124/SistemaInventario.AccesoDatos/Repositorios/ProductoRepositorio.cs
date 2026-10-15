using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;
using SistemaInventario.AccesoDatos.Interfaces;
using SistemaInventario.Entidades;

namespace SistemaInventario.AccesoDatos.Repositorios
{
    /// <summary>
    /// Repositorio de productos (carga también el nombre de su categoría y suplidor).
    /// </summary>
    public class ProductoRepositorio : RepositorioBase<Producto>, IProductoRepositorio
    {
        private const string SelectBase =
            "SELECT p.*, c.Nombre AS CategoriaNombre, s.NombreEmpresa AS SuplidorNombre " +
            "FROM Productos p " +
            "INNER JOIN Categorias c ON c.Id = p.CategoriaId " +
            "INNER JOIN Suplidores s ON s.Id = p.SuplidorId ";

        /// <summary>Crea el repositorio con la conexión inyectada.</summary>
        public ProductoRepositorio(ConexionBD conexion) : base(conexion)
        {
        }

        /// <inheritdoc />
        protected override string NombreTabla => "Productos";

        /// <inheritdoc />
        protected override Producto Mapear(SqlDataReader lector)
        {
            int categoriaId = lector.GetInt32(lector.GetOrdinal("CategoriaId"));
            int suplidorId = lector.GetInt32(lector.GetOrdinal("SuplidorId"));

            var producto = new Producto
            {
                Nombre = lector.GetString(lector.GetOrdinal("Nombre")),
                PrecioUnitario = lector.GetDecimal(lector.GetOrdinal("PrecioUnitario")),
                CategoriaId = categoriaId,
                SuplidorId = suplidorId,
                Categoria = new Categoria
                {
                    Id = categoriaId,
                    Nombre = lector.GetString(lector.GetOrdinal("CategoriaNombre"))
                },
                Suplidor = new Suplidor
                {
                    Id = suplidorId,
                    NombreEmpresa = lector.GetString(lector.GetOrdinal("SuplidorNombre"))
                }
            };
            MapearCamposBase(lector, producto);
            return producto;
        }

        /// <inheritdoc />
        public override IEnumerable<Producto> ObtenerActivos() =>
            ConsultarLista(SelectBase + "WHERE p.Activo = 1 ORDER BY p.Id");

        /// <inheritdoc />
        public override IEnumerable<Producto> ObtenerEliminados() =>
            ConsultarLista(SelectBase + "WHERE p.Activo = 0 ORDER BY p.Id");

        /// <inheritdoc />
        public override Producto? ObtenerPorId(int id) =>
            ConsultarLista(SelectBase + "WHERE p.Id = @Id",
                           new SqlParameter("@Id", id)).FirstOrDefault();

        /// <inheritdoc />
        public override int Insertar(Producto producto)
        {
            const string sql =
                "INSERT INTO Productos (Nombre, PrecioUnitario, CategoriaId, SuplidorId) " +
                "OUTPUT INSERTED.Id " +
                "VALUES (@Nombre, @Precio, @CategoriaId, @SuplidorId)";

            object? id = EjecutarEscalar(sql,
                new SqlParameter("@Nombre", producto.Nombre),
                new SqlParameter("@Precio", producto.PrecioUnitario),
                new SqlParameter("@CategoriaId", producto.CategoriaId),
                new SqlParameter("@SuplidorId", producto.SuplidorId));

            return Convert.ToInt32(id);
        }

        /// <inheritdoc />
        public override bool Actualizar(Producto producto)
        {
            const string sql =
                "UPDATE Productos SET Nombre = @Nombre, PrecioUnitario = @Precio, " +
                "CategoriaId = @CategoriaId, SuplidorId = @SuplidorId, " +
                "FechaModificacion = SYSDATETIME() WHERE Id = @Id";

            return EjecutarComando(sql,
                new SqlParameter("@Nombre", producto.Nombre),
                new SqlParameter("@Precio", producto.PrecioUnitario),
                new SqlParameter("@CategoriaId", producto.CategoriaId),
                new SqlParameter("@SuplidorId", producto.SuplidorId),
                new SqlParameter("@Id", producto.Id)) > 0;
        }
    }
}