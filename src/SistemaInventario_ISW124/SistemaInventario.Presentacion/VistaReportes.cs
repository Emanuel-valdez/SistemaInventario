using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using SistemaInventario.Entidades;
using SistemaInventario.Entidades.Excepciones;
using SistemaInventario.LogicaNegocio.Interfaces;
using SistemaInventario.LogicaNegocio.Reportes;

namespace SistemaInventario.Presentacion
{
    /// <summary>
    /// Pantalla de reportes: muestra el resultado de las consultas LINQ de la aplicación.
    /// </summary>
    public class VistaReportes : UserControl, IVista
    {
        private const int AnchoMaximoBarra = 50;
        private const int AltoBarra = 14;
        private const int AnchoMensaje = 260;

        private static readonly Font FuenteFila = new Font("Segoe UI", 9.5F);
        private static readonly Font FuenteValor = new Font("Segoe UI", 9.5F, FontStyle.Bold);

        private readonly IReporteServicio _reportes;
        private readonly ICategoriaServicio _categorias;

        private readonly FlowLayoutPanel _barrasCategoria = CrearLista();
        private readonly FlowLayoutPanel _barrasSuplidores = CrearLista();
        private readonly FlowLayoutPanel _barrasPromedio = CrearLista();
        private readonly FlowLayoutPanel _listaSinProductos = CrearLista();
        private readonly DataGridView _gridMasCaro = new DataGridView();

        private readonly TextBox _txtTexto = new TextBox();
        private readonly ComboBox _cmbCategoria = new ComboBox();
        private readonly TextBox _txtMinimo = new TextBox();
        private readonly TextBox _txtMaximo = new TextBox();
        private readonly DataGridView _gridBusqueda = new DataGridView();
        private readonly Label _lblBusqueda = new Label();
        private bool _llenandoCombo;

        /// <summary>Crea la vista con los servicios inyectados.</summary>
        public VistaReportes(IReporteServicio reportes, ICategoriaServicio categorias)
        {
            _reportes = reportes;
            _categorias = categorias;
            ConstruirInterfaz();
        }

        // ---------- Construcción de la interfaz ----------

        private void ConstruirInterfaz()
        {
            Dock = DockStyle.Fill;
            BackColor = Tema.Fondo;
            Padding = new Padding(24);
            Font = Tema.FuenteBase;
            AutoScroll = true;

            var rejilla = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 660,
                ColumnCount = 3,
                RowCount = 2
            };
            for (int i = 0; i < 3; i++)
            {
                rejilla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3));
            }
            rejilla.RowStyles.Add(new RowStyle(SizeType.Absolute, 270));
            rejilla.RowStyles.Add(new RowStyle(SizeType.Absolute, 390));

            ConfigurarGrid(_gridMasCaro);
            ConfigurarGrid(_gridBusqueda);

            rejilla.Controls.Add(CrearTarjeta("Productos por categoría", _barrasCategoria, 0, 0), 0, 0);
            rejilla.Controls.Add(CrearTarjeta("Top 5 suplidores", _barrasSuplidores, 1, 0), 1, 0);
            rejilla.Controls.Add(CrearTarjeta("Precio promedio por categoría", _barrasPromedio, 2, 0), 2, 0);

            rejilla.Controls.Add(CrearTarjeta("Producto más caro por categoría", _gridMasCaro, 0, 1), 0, 1);
            rejilla.Controls.Add(CrearTarjeta("Suplidores sin productos", _listaSinProductos, 1, 1), 1, 1);
            rejilla.Controls.Add(CrearTarjeta("Búsqueda de productos", CrearBusqueda(), 2, 1), 2, 1);

            Controls.Add(rejilla);
        }

        private static FlowLayoutPanel CrearLista()
        {
            return new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
        }

        private static Tarjeta CrearTarjeta(string titulo, Control contenido, int columna, int fila)
        {
            var tarjeta = new Tarjeta
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, columna < 2 ? 12 : 0, fila == 0 ? 12 : 0),
                Padding = new Padding(18)
            };

            var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            tabla.Controls.Add(Tema.Titulo(titulo, 12.5F), 0, 0);

            contenido.Dock = DockStyle.Fill;
            contenido.Margin = new Padding(0, 8, 0, 0);
            tabla.Controls.Add(contenido, 0, 1);

            tarjeta.Controls.Add(tabla);
            return tarjeta;
        }

        private Control CrearBusqueda()
        {
            var tabla = new TableLayoutPanel { ColumnCount = 2, RowCount = 5 };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Tema.EstilizarTextBox(_txtTexto);
            _txtTexto.Dock = DockStyle.Fill;
            _txtTexto.Margin = new Padding(0, 0, 0, 6);
            _txtTexto.PlaceholderText = "Nombre del producto...";
            _txtTexto.TextChanged += (s, e) => ActualizarBusqueda();
            tabla.Controls.Add(_txtTexto, 0, 0);
            tabla.SetColumnSpan(_txtTexto, 2);

            _cmbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbCategoria.Font = new Font("Segoe UI", 11F);
            _cmbCategoria.Dock = DockStyle.Fill;
            _cmbCategoria.Margin = new Padding(0, 0, 0, 6);
            _cmbCategoria.SelectedIndexChanged += (s, e) =>
            {
                if (!_llenandoCombo)
                {
                    ActualizarBusqueda();
                }
            };
            tabla.Controls.Add(_cmbCategoria, 0, 1);
            tabla.SetColumnSpan(_cmbCategoria, 2);

            Tema.EstilizarTextBox(_txtMinimo);
            _txtMinimo.Dock = DockStyle.Fill;
            _txtMinimo.Margin = new Padding(0, 0, 6, 6);
            _txtMinimo.PlaceholderText = "Precio mín.";
            _txtMinimo.TextChanged += (s, e) => ActualizarBusqueda();
            tabla.Controls.Add(_txtMinimo, 0, 2);

            Tema.EstilizarTextBox(_txtMaximo);
            _txtMaximo.Dock = DockStyle.Fill;
            _txtMaximo.Margin = new Padding(6, 0, 0, 6);
            _txtMaximo.PlaceholderText = "Precio máx.";
            _txtMaximo.TextChanged += (s, e) => ActualizarBusqueda();
            tabla.Controls.Add(_txtMaximo, 1, 2);

            tabla.Controls.Add(_gridBusqueda, 0, 3);
            tabla.SetColumnSpan(_gridBusqueda, 2);

            _lblBusqueda.AutoSize = true;
            _lblBusqueda.ForeColor = Tema.Muted;
            _lblBusqueda.Margin = new Padding(0, 6, 0, 0);
            tabla.Controls.Add(_lblBusqueda, 0, 4);
            tabla.SetColumnSpan(_lblBusqueda, 2);

            return tabla;
        }

        private static void ConfigurarGrid(DataGridView grid)
        {
            Tema.EstilizarGrid(grid);
            grid.Dock = DockStyle.Fill;
            grid.ColumnHeadersHeight = 34;
            grid.RowTemplate.Height = 30;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        }

        // ---------- Datos ----------

        /// <inheritdoc />
        public void Actualizar()
        {
            try
            {
                LlenarBarras(_barrasCategoria, _reportes.ProductosPorCategoria(),
                    r => r.Cantidad, "N0", Tema.Teal);

                LlenarBarras(_barrasSuplidores, _reportes.TopSuplidores(5),
                    r => r.Cantidad, "N0", ColorTranslator.FromHtml("#3B82F6"));

                LlenarBarras(_barrasPromedio, _reportes.PromedioPrecioPorCategoria(),
                    r => r.Promedio, "N2", ColorTranslator.FromHtml("#F59E0B"));

                ActualizarMasCaro();
                ActualizarSinProductos();
                CargarCombo();
                ActualizarBusqueda();
            }
            catch (AccesoDatosException ex)
            {
                _barrasCategoria.Controls.Clear();
                _barrasCategoria.Controls.Add(CrearMensaje(ex.Message));
            }
        }

        /// <summary>
        /// Dibuja una gráfica de barras con una tabla que se ajusta sola al contenido:
        /// columna de nombres, columna de barras y columna de valores.
        /// </summary>
        private void LlenarBarras(FlowLayoutPanel lista, IEnumerable<ResumenGrupo> datos,
                                  Func<ResumenGrupo, decimal> valor, string formato, Color color)
        {
            lista.Controls.Clear();
            List<ResumenGrupo> filas = datos.ToList();

            if (filas.Count == 0)
            {
                lista.Controls.Add(CrearMensaje("Aún no hay productos registrados."));
                return;
            }

            // Las barras se escalan según la configuración de pantalla de Windows.
            float escala = DeviceDpi / 96f;
            int anchoMaximo = (int)(AnchoMaximoBarra * escala);
            int alto = (int)(AltoBarra * escala);
            decimal maximo = filas.Max(valor);

            var tabla = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 3,
                RowCount = filas.Count,
                Margin = new Padding(0)
            };
            for (int c = 0; c < 3; c++)
            {
                tabla.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            }

            for (int i = 0; i < filas.Count; i++)
            {
                ResumenGrupo fila = filas[i];
                decimal actual = valor(fila);
                int ancho = maximo <= 0 ? 4 : Math.Max(4, (int)(anchoMaximo * actual / maximo));

                tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                tabla.Controls.Add(new Label
                {
                    Text = fila.Nombre,
                    AutoSize = true,
                    Font = FuenteFila,
                    ForeColor = Tema.Texto,
                    Anchor = AnchorStyles.Left,
                    Margin = new Padding(0, 5, 14, 5)
                }, 0, i);

                tabla.Controls.Add(new Panel
                {
                    BackColor = color,
                    Size = new Size(ancho, alto),
                    Anchor = AnchorStyles.Left,
                    Margin = new Padding(0, 0, 10, 0)
                }, 1, i);

                tabla.Controls.Add(new Label
                {
                    Text = actual.ToString(formato, CultureInfo.CurrentCulture),
                    AutoSize = true,
                    Font = FuenteValor,
                    ForeColor = Tema.Texto,
                    Anchor = AnchorStyles.Left,
                    Margin = new Padding(0, 5, 0, 5)
                }, 2, i);
            }

            lista.Controls.Add(tabla);
        }

        private static Label CrearMensaje(string texto)
        {
            return new Label
            {
                Text = texto,
                AutoSize = true,
                MaximumSize = new Size(AnchoMensaje, 0),
                ForeColor = Tema.Muted
            };
        }

        private void ActualizarMasCaro()
        {
            var filas = _reportes.ProductoMasCaroPorCategoria()
                .Select(p => new
                {
                    Categoria = p.Categoria?.Nombre ?? string.Empty,
                    Producto = p.Nombre,
                    Precio = p.PrecioUnitario
                })
                .ToList();

            _gridMasCaro.DataSource = filas;
            _gridMasCaro.Columns["Categoria"]!.HeaderText = "Categoría";
            _gridMasCaro.Columns["Categoria"]!.FillWeight = 80;
            _gridMasCaro.Columns["Producto"]!.FillWeight = 110;
            _gridMasCaro.Columns["Precio"]!.FillWeight = 65;
            _gridMasCaro.Columns["Precio"]!.DefaultCellStyle.Format = "N2";
            _gridMasCaro.Columns["Precio"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _gridMasCaro.ClearSelection();
            _gridMasCaro.CurrentCell = null;
        }

        private void ActualizarSinProductos()
        {
            _listaSinProductos.Controls.Clear();
            List<Suplidor> suplidores = _reportes.SuplidoresSinProductos().ToList();

            if (suplidores.Count == 0)
            {
                _listaSinProductos.Controls.Add(CrearMensaje("Todos los suplidores tienen productos activos."));
                return;
            }

            foreach (Suplidor suplidor in suplidores)
            {
                _listaSinProductos.Controls.Add(new Label
                {
                    Text = "•  " + suplidor.NombreEmpresa,
                    AutoSize = true,
                    ForeColor = Tema.Texto,
                    Margin = new Padding(0, 0, 0, 6)
                });
            }
        }

        private void CargarCombo()
        {
            int seleccionado = _cmbCategoria.SelectedValue is int id ? id : 0;

            var lista = new List<Categoria> { new Categoria { Id = 0, Nombre = "Todas las categorías" } };
            lista.AddRange(_categorias.ObtenerActivas());

            _llenandoCombo = true;
            _cmbCategoria.DataSource = lista;
            _cmbCategoria.DisplayMember = nameof(Categoria.Nombre);
            _cmbCategoria.ValueMember = nameof(Categoria.Id);
            _cmbCategoria.SelectedValue = seleccionado;
            if (_cmbCategoria.SelectedIndex < 0)
            {
                _cmbCategoria.SelectedIndex = 0;
            }
            _llenandoCombo = false;
        }

        private void ActualizarBusqueda()
        {
            try
            {
                int? categoriaId = _cmbCategoria.SelectedValue is int id && id > 0 ? id : null;

                var filas = _reportes
                    .BuscarProductos(_txtTexto.Text, categoriaId,
                                     LeerPrecio(_txtMinimo), LeerPrecio(_txtMaximo))
                    .Select(p => new { p.Nombre, Precio = p.PrecioUnitario })
                    .ToList();

                _gridBusqueda.DataSource = filas;
                _gridBusqueda.Columns["Nombre"]!.FillWeight = 130;
                _gridBusqueda.Columns["Precio"]!.FillWeight = 70;
                _gridBusqueda.Columns["Precio"]!.DefaultCellStyle.Format = "N2";
                _gridBusqueda.Columns["Precio"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                _gridBusqueda.ClearSelection();
                _gridBusqueda.CurrentCell = null;

                _lblBusqueda.ForeColor = Tema.Muted;
                _lblBusqueda.Text = $"{filas.Count} productos encontrados";
            }
            catch (AccesoDatosException ex)
            {
                _lblBusqueda.ForeColor = Tema.Rojo;
                _lblBusqueda.Text = ex.Message;
            }
        }

        private static decimal? LeerPrecio(TextBox cuadro)
        {
            return decimal.TryParse(cuadro.Text.Trim(), NumberStyles.Number,
                CultureInfo.CurrentCulture, out decimal valor)
                ? valor
                : null;
        }
    }
}