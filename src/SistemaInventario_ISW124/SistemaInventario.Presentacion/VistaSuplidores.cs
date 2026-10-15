using System;
using System.Collections.Generic;
using System.Text;
using SistemaInventario.Entidades;
using SistemaInventario.Entidades.Excepciones;
using SistemaInventario.LogicaNegocio.Comunes;
using SistemaInventario.LogicaNegocio.Interfaces;

namespace SistemaInventario.Presentacion
{
    /// <summary>
    /// Pantalla de suplidores: listado a la izquierda y editor a la derecha.
    /// </summary>
    public class VistaSuplidores : UserControl, IVista
    {
        private readonly ISuplidorServicio _servicio;
        private List<Suplidor> _todos = new List<Suplidor>();
        private int _suplidorId;
        private bool _cargando;

        private readonly ErrorProvider _error = new ErrorProvider();

        private readonly TextBox _txtBuscar = new TextBox();
        private readonly DataGridView _grid = new DataGridView();
        private readonly Label _lblResumen = new Label();

        private readonly Label _lblTituloEditor = Tema.Titulo("Nuevo suplidor");
        private readonly TextBox _txtEmpresa = new TextBox();
        private readonly TextBox _txtContacto = new TextBox();
        private readonly TextBox _txtTelefono = new TextBox();
        private readonly TextBox _txtCorreo = new TextBox();
        private readonly TextBox _txtSitioWeb = new TextBox();
        private readonly Label _lblFechas = new Label();
        private readonly Label _lblMensaje = new Label();
        private Button _btnEliminar = null!;

        /// <summary>Crea la vista con el servicio inyectado.</summary>
        public VistaSuplidores(ISuplidorServicio servicio)
        {
            _servicio = servicio;
            ConstruirInterfaz();
            LimpiarEditor();
        }

        /// <inheritdoc />
        public void Actualizar()
        {
            _lblMensaje.Text = string.Empty;
            Cargar();
        }

        // ---------- Construcción de la interfaz ----------

        private void ConstruirInterfaz()
        {
            Dock = DockStyle.Fill;
            BackColor = Tema.Fondo;
            Padding = new Padding(24);
            Font = Tema.FuenteBase;

            _error.ContainerControl = this;
            _error.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            var raiz = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            raiz.Controls.Add(ConstruirLista(), 0, 0);
            raiz.Controls.Add(ConstruirEditor(), 1, 0);

            Controls.Add(raiz);
        }

        private Control ConstruirLista()
        {
            var tarjeta = new Tarjeta { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0) };

            var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            tabla.Controls.Add(Tema.Titulo("Suplidores"), 0, 0);

            Tema.EstilizarTextBox(_txtBuscar);
            _txtBuscar.Dock = DockStyle.Fill;
            _txtBuscar.Margin = new Padding(0, 4, 0, 12);
            _txtBuscar.PlaceholderText = "Buscar por empresa, contacto, teléfono o correo...";
            _txtBuscar.TextChanged += (s, e) => Filtrar();
            tabla.Controls.Add(_txtBuscar, 0, 1);

            Tema.EstilizarGrid(_grid);
            _grid.Dock = DockStyle.Fill;
            _grid.SelectionChanged += (s, e) =>
            {
                if (_cargando)
                {
                    return;
                }
                Suplidor? suplidor = ObtenerSeleccionado();
                if (suplidor != null)
                {
                    CargarEnEditor(suplidor);
                }
            };
            tabla.Controls.Add(_grid, 0, 2);

            _lblResumen.AutoSize = true;
            _lblResumen.ForeColor = Tema.Muted;
            _lblResumen.Margin = new Padding(0, 8, 0, 0);
            tabla.Controls.Add(_lblResumen, 0, 3);

            tarjeta.Controls.Add(tabla);
            return tarjeta;
        }

        private Control ConstruirEditor()
        {
            var tarjeta = new Tarjeta { Dock = DockStyle.Fill, Margin = new Padding(12, 0, 0, 0) };

            var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 14 };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 14; i++)
            {
                tabla.RowStyles.Add(i == 11
                    ? new RowStyle(SizeType.Percent, 100)
                    : new RowStyle(SizeType.AutoSize));
            }

            tabla.Controls.Add(_lblTituloEditor, 0, 0);

            AgregarCampo(tabla, 1, "Nombre de la empresa (obligatorio)", _txtEmpresa, 150, null);
            AgregarCampo(tabla, 3, "Nombre del contacto (obligatorio)", _txtContacto, 100, null);
            AgregarCampo(tabla, 5, "Teléfono (809/829/849-XXX-XXXX)", _txtTelefono, 12, "809-555-0100");
            AgregarCampo(tabla, 7, "Correo electrónico", _txtCorreo, 150, "nombre@empresa.com");
            AgregarCampo(tabla, 9, "Sitio web (opcional)", _txtSitioWeb, 200, "https://www.ejemplo.com");

            // Fila 11: fechas (pegadas abajo)
            _lblFechas.AutoSize = true;
            _lblFechas.Dock = DockStyle.Bottom;
            _lblFechas.ForeColor = Tema.Muted;
            _lblFechas.Font = new Font("Segoe UI", 9F);
            _lblFechas.Margin = new Padding(0, 10, 0, 4);
            tabla.Controls.Add(_lblFechas, 0, 11);

            // Fila 12: mensaje de resultado
            _lblMensaje.AutoSize = true;
            _lblMensaje.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _lblMensaje.Margin = new Padding(0, 0, 0, 8);
            tabla.SizeChanged += (s, e) =>
                _lblMensaje.MaximumSize = new Size(Math.Max(tabla.Width - 10, 100), 0);
            tabla.Controls.Add(_lblMensaje, 0, 12);

            // Fila 13: acciones
            var acciones = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false
            };
            acciones.Controls.Add(Tema.Boton("Guardar", EstiloBoton.Primario, 130, Guardar));
            acciones.Controls.Add(Tema.Boton("Nuevo", EstiloBoton.Oscuro, 110, LimpiarEditor));
            _btnEliminar = Tema.Boton("Eliminar", EstiloBoton.Peligro, 110, Eliminar);
            acciones.Controls.Add(_btnEliminar);
            tabla.Controls.Add(acciones, 0, 13);

            tarjeta.Controls.Add(tabla);
            return tarjeta;
        }

        private void AgregarCampo(TableLayoutPanel tabla, int fila, string etiqueta,
                                  TextBox cuadro, int maximo, string? ejemplo)
        {
            tabla.Controls.Add(Tema.EtiquetaCampo(etiqueta), 0, fila);

            Tema.EstilizarTextBox(cuadro);
            cuadro.Dock = DockStyle.Fill;
            cuadro.MaxLength = maximo;
            if (ejemplo != null)
            {
                cuadro.PlaceholderText = ejemplo;
            }
            cuadro.TextChanged += (s, e) => _error.SetError(cuadro, string.Empty);
            _error.SetIconPadding(cuadro, -22);

            tabla.Controls.Add(cuadro, 0, fila + 1);
        }

        // ---------- Listado ----------

        private void Cargar()
        {
            try
            {
                _todos = _servicio.ObtenerActivos().ToList();
                Filtrar();
            }
            catch (AccesoDatosException ex)
            {
                MostrarMensaje(ex.Message, false);
            }
        }

        /// <summary>Filtra la lista en memoria mientras el usuario escribe (LINQ).</summary>
        private void Filtrar()
        {
            string texto = _txtBuscar.Text.Trim();

            var filas = _todos
                .Where(s => s.NombreEmpresa.Contains(texto, StringComparison.OrdinalIgnoreCase)
                         || s.NombreContacto.Contains(texto, StringComparison.OrdinalIgnoreCase)
                         || s.Correo.Contains(texto, StringComparison.OrdinalIgnoreCase)
                         || s.Telefono.Contains(texto, StringComparison.OrdinalIgnoreCase))
                .Select(s => new
                {
                    s.Id,
                    Empresa = s.NombreEmpresa,
                    Contacto = s.NombreContacto,
                    s.Telefono,
                    s.Correo
                })
                .ToList();

            _cargando = true;
            _grid.DataSource = filas;
            _grid.Columns["Id"]!.Visible = false;
            _grid.Columns["Empresa"]!.FillWeight = 110;
            _grid.Columns["Contacto"]!.FillWeight = 90;
            _grid.Columns["Telefono"]!.HeaderText = "Teléfono";
            _grid.Columns["Telefono"]!.FillWeight = 75;
            _grid.Columns["Correo"]!.FillWeight = 120;
            _grid.ClearSelection();
            _grid.CurrentCell = null;
            _cargando = false;

            _lblResumen.Text = $"Mostrando {filas.Count} de {_todos.Count} suplidores";
        }

        private Suplidor? ObtenerSeleccionado()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                return null;
            }
            if (_grid.SelectedRows[0].Cells["Id"].Value is not int id)
            {
                return null;
            }
            return _todos.FirstOrDefault(s => s.Id == id);
        }

        // ---------- Editor ----------

        private void CargarEnEditor(Suplidor suplidor)
        {
            _error.Clear();
            _lblMensaje.Text = string.Empty;

            _suplidorId = suplidor.Id;
            _lblTituloEditor.Text = "Editar suplidor";
            _txtEmpresa.Text = suplidor.NombreEmpresa;
            _txtContacto.Text = suplidor.NombreContacto;
            _txtTelefono.Text = suplidor.Telefono;
            _txtCorreo.Text = suplidor.Correo;
            _txtSitioWeb.Text = suplidor.SitioWeb ?? string.Empty;

            string modificada = suplidor.FechaModificacion?.ToString("dd/MM/yyyy HH:mm") ?? "—";
            _lblFechas.Text = $"Creado: {suplidor.FechaCreacion:dd/MM/yyyy HH:mm}   ·   Modificado: {modificada}";
            _btnEliminar.Enabled = true;
        }

        private void LimpiarEditor()
        {
            _error.Clear();
            _lblMensaje.Text = string.Empty;

            _suplidorId = 0;
            _lblTituloEditor.Text = "Nuevo suplidor";
            _txtEmpresa.Clear();
            _txtContacto.Clear();
            _txtTelefono.Clear();
            _txtCorreo.Clear();
            _txtSitioWeb.Clear();
            _lblFechas.Text = string.Empty;
            _btnEliminar.Enabled = false;

            _cargando = true;
            _grid.ClearSelection();
            _grid.CurrentCell = null;
            _cargando = false;
        }

        private void Guardar()
        {
            _error.Clear();
            _lblMensaje.Text = string.Empty;

            var suplidor = new Suplidor
            {
                Id = _suplidorId,
                NombreEmpresa = _txtEmpresa.Text,
                NombreContacto = _txtContacto.Text,
                Telefono = _txtTelefono.Text,
                Correo = _txtCorreo.Text,
                SitioWeb = _txtSitioWeb.Text
            };

            ResultadoOperacion<int> resultado = _servicio.Guardar(suplidor);

            if (resultado.Exito)
            {
                Cargar();
                LimpiarEditor();
                MostrarMensaje(resultado.Mensaje, true);
                return;
            }

            MostrarErrores(resultado.Errores);
            MostrarMensaje(resultado.Mensaje, false);
        }

        private void Eliminar()
        {
            if (_suplidorId == 0)
            {
                return;
            }

            DialogResult respuesta = MessageBox.Show(
                $"¿Enviar el suplidor \"{_txtEmpresa.Text}\" a la papelera?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
            {
                return;
            }

            ResultadoOperacion<bool> resultado = _servicio.Eliminar(_suplidorId);
            if (!resultado.Exito)
            {
                MostrarMensaje(resultado.Mensaje, false);
                return;
            }

            Cargar();
            LimpiarEditor();
            MostrarMensaje(resultado.Mensaje, true);
        }

        // ---------- Mensajes y errores ----------

        private void MostrarMensaje(string texto, bool exito)
        {
            _lblMensaje.ForeColor = exito ? Tema.Verde : Tema.Rojo;
            _lblMensaje.Text = texto;
        }

        private void MostrarErrores(Dictionary<string, string> errores)
        {
            foreach (KeyValuePair<string, string> error in errores)
            {
                Control? control = error.Key switch
                {
                    "NombreEmpresa" => _txtEmpresa,
                    "NombreContacto" => _txtContacto,
                    "Telefono" => _txtTelefono,
                    "Correo" => _txtCorreo,
                    "SitioWeb" => _txtSitioWeb,
                    _ => null
                };

                if (control != null)
                {
                    _error.SetError(control, error.Value);
                }
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _error.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}