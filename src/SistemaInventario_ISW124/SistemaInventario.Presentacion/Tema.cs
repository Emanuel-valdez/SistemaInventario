using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaInventario.Presentacion
{
    /// <summary>
    /// Colores, fuentes y métodos de ayuda para que toda la aplicación tenga el mismo diseño.
    /// </summary>
    public static class Tema
    {
        /// <summary>Azul marino de los encabezados.</summary>
        public static readonly Color Navy = ColorTranslator.FromHtml("#1E293B");

        /// <summary>Color de acento principal.</summary>
        public static readonly Color Teal = ColorTranslator.FromHtml("#14B8A6");

        /// <summary>Teal oscuro para textos resaltados.</summary>
        public static readonly Color TealOscuro = ColorTranslator.FromHtml("#0F766E");

        /// <summary>Teal claro para pestañas y filas seleccionadas.</summary>
        public static readonly Color TealClaro = ColorTranslator.FromHtml("#CCFBF1");

        /// <summary>Fondo gris claro de las ventanas.</summary>
        public static readonly Color Fondo = ColorTranslator.FromHtml("#F1F5F9");

        /// <summary>Color de los bordes suaves.</summary>
        public static readonly Color Borde = ColorTranslator.FromHtml("#E2E8F0");

        /// <summary>Color del texto principal.</summary>
        public static readonly Color Texto = ColorTranslator.FromHtml("#1E293B");

        /// <summary>Color del texto secundario.</summary>
        public static readonly Color Muted = ColorTranslator.FromHtml("#64748B");

        /// <summary>Rojo para errores y acciones peligrosas.</summary>
        public static readonly Color Rojo = ColorTranslator.FromHtml("#DC2626");

        /// <summary>Verde para mensajes de éxito.</summary>
        public static readonly Color Verde = ColorTranslator.FromHtml("#16A34A");

        /// <summary>Fuente base de la aplicación.</summary>
        public static readonly Font FuenteBase = new Font("Segoe UI", 10F);

        /// <summary>Crea un botón plano con el estilo indicado.</summary>
        public static Button Boton(string texto, EstiloBoton estilo, int ancho, Action accion, int alto = 38)
        {
            Color fondo = estilo switch
            {
                EstiloBoton.Primario => Teal,
                EstiloBoton.Oscuro => Navy,
                EstiloBoton.Peligro => Rojo,
                _ => Muted
            };

            var boton = new Button
            {
                Text = texto,
                Size = new Size(ancho, alto),
                FlatStyle = FlatStyle.Flat,
                BackColor = fondo,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 10, 0),
                UseVisualStyleBackColor = false
            };
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(fondo, 0.1f);
            boton.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(fondo, 0.2f);
            boton.Click += (s, e) => accion();
            return boton;
        }

        /// <summary>Crea un título en negrita para las tarjetas.</summary>
        public static Label Titulo(string texto, float tamano = 14F)
        {
            return new Label
            {
                Text = texto,
                AutoSize = true,
                ForeColor = Navy,
                Font = new Font("Segoe UI", tamano, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 4)
            };
        }

        /// <summary>Crea la etiqueta gris que va encima de cada campo de un formulario.</summary>
        public static Label EtiquetaCampo(string texto)
        {
            return new Label
            {
                Text = texto,
                AutoSize = true,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(0, 12, 0, 3)
            };
        }

        /// <summary>Aplica el estilo de la aplicación a un cuadro de texto.</summary>
        public static void EstilizarTextBox(TextBox cuadro)
        {
            cuadro.BorderStyle = BorderStyle.FixedSingle;
            cuadro.Font = new Font("Segoe UI", 11F);
            cuadro.ForeColor = Texto;
        }

        /// <summary>Aplica el estilo de la aplicación a una tabla.</summary>
        public static void EstilizarGrid(DataGridView grid)
        {
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.MultiSelect = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 42;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Navy;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Navy;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);

            grid.RowTemplate.Height = 38;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Borde;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            grid.DefaultCellStyle.ForeColor = Texto;
            grid.DefaultCellStyle.SelectionBackColor = TealClaro;
            grid.DefaultCellStyle.SelectionForeColor = TealOscuro;
            grid.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            grid.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#F8FAFC");
        }
    }
}