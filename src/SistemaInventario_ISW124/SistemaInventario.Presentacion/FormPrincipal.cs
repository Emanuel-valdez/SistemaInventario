using SistemaInventario.Entidades.Excepciones;
using SistemaInventario.LogicaNegocio.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SistemaInventario.Presentacion
{
    /// <summary>
    /// Ventana principal: encabezado, barra de pestañas y el contenido de cada módulo.
    /// Todas las pantallas se muestran dentro de esta misma ventana.
    /// </summary>
    public partial class FormPrincipal : Form
    {
        private readonly ICategoriaServicio _categorias;
        private readonly ISuplidorServicio _suplidores;
        private readonly IProductoServicio _productos;
        private readonly IReporteServicio _reportes;

        private readonly string[] _titulos = { "Categorías", "Suplidores", "Productos", "Papelera", "Reportes" };
        private readonly List<Button> _pestanas = new List<Button>();
        private readonly Panel _contenido = new Panel();
        private Control[] _vistas = Array.Empty<Control>();

        /// <summary>Crea la ventana con los servicios inyectados.</summary>
        public FormPrincipal(ICategoriaServicio categorias,
                             ISuplidorServicio suplidores,
                             IProductoServicio productos,
                             IReporteServicio reportes)
        {
            InitializeComponent();
            _categorias = categorias;
            _suplidores = suplidores;
            _productos = productos;
            _reportes = reportes;
            ConstruirInterfaz();
            Mostrar(0);
        }

        private void ConstruirInterfaz()
        {
            Text = "Sistema de Inventario";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1120, 700);
            MinimumSize = new Size(980, 640);
            BackColor = Tema.Fondo;
            Font = Tema.FuenteBase;

            // Encabezado azul marino
            var encabezado = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Tema.Navy };
            var lblTitulo = new Label
            {
                Text = "Sistema de Inventario",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 19F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(28, 16)
            };
            var lblDerecha = new Label
            {
                Text = "EV Solutions",
                ForeColor = ColorTranslator.FromHtml("#94A3B8"),
                Font = new Font("Segoe UI", 10F),
                AutoSize = true
            };
            encabezado.Controls.Add(lblTitulo);
            encabezado.Controls.Add(lblDerecha);

            void Reubicar() => lblDerecha.Location = new Point(
                encabezado.Width - lblDerecha.Width - 28,
                (encabezado.Height - lblDerecha.Height) / 2);

            encabezado.Resize += (s, e) => Reubicar();
            Load += (s, e) => Reubicar();

            // Barra de pestañas
            var barra = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                BackColor = Color.White,
                Padding = new Padding(20, 0, 20, 0)
            };
            var linea = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Tema.Borde };
            var flujo = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight
            };

            for (int i = 0; i < _titulos.Length; i++)
            {
                int indice = i;
                Button pestana = CrearPestana(_titulos[i], () => Mostrar(indice));
                _pestanas.Add(pestana);
                flujo.Controls.Add(pestana);
            }

            barra.Controls.Add(flujo);
            barra.Controls.Add(linea);

            // Contenido: una vista por pestaña, todas dentro del mismo panel
            _contenido.Dock = DockStyle.Fill;
            _contenido.BackColor = Tema.Fondo;

            _vistas = new Control[]
            {
                new VistaCategorias(_categorias),
                new VistaSuplidores(_suplidores),
                new VistaProductos(_productos, _categorias, _suplidores),
                new VistaPapelera(_categorias, _suplidores, _productos),
                new VistaReportes(_reportes, _categorias)
            };

            foreach (Control vista in _vistas)
            {
                vista.Dock = DockStyle.Fill;
                vista.Visible = false;
                _contenido.Controls.Add(vista);
            }

            // Primero el que llena el espacio, después los anclados arriba.
            Controls.Add(_contenido);
            Controls.Add(barra);
            Controls.Add(encabezado);
        }

        private static Button CrearPestana(string texto, Action accion)
        {
            var boton = new Button
            {
                Text = texto,
                Size = new Size(130, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Tema.Muted,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 8, 6, 0),
                TabStop = false,
                UseVisualStyleBackColor = false
            };
            boton.FlatAppearance.BorderSize = 0;
            boton.Click += (s, e) => accion();
            return boton;
        }

        /// <summary>Muestra la pestaña indicada dentro de la misma ventana y refresca sus datos.</summary>
        private void Mostrar(int indice)
        {
            for (int i = 0; i < _vistas.Length; i++)
            {
                bool activa = i == indice;
                _vistas[i].Visible = activa;

                Button pestana = _pestanas[i];
                pestana.BackColor = activa ? Tema.TealClaro : Color.White;
                pestana.ForeColor = activa ? Tema.TealOscuro : Tema.Muted;
                pestana.FlatAppearance.MouseOverBackColor = activa ? Tema.TealClaro : Tema.Fondo;
            }

            _vistas[indice].BringToFront();
            if (_vistas[indice] is IVista vista)
            {
                vista.Actualizar();
            }
        }
    }
}