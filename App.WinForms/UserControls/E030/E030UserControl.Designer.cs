using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace App.WinForms.UserControls.E030
{
    partial class E030UserControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.BackColor = Color.White;

            var scrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
            this.Controls.Add(scrollPanel);

            var fLabel = new Font(SystemFonts.DefaultFont.FontFamily, 9f);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Top, ColumnCount = 5,
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(8)
            };
            for (int i = 0; i < 5; i++) root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            root.ColumnStyles[3] = new ColumnStyle(SizeType.Absolute, 380f);
            root.ColumnStyles[4] = new ColumnStyle(SizeType.Absolute, 1f);
            scrollPanel.Controls.Add(root);

            // ── Row 0: Factor de Zona ────────────────────────────────────────
            root.Controls.Add(new Label
            {
                Text = "Factor de Zona :", AutoSize = false, Width = 220,
                Font = fLabel, Margin = new Padding(4, 6, 4, 4)
            }, 0, 0);

            _cmbZona = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 60 };
            _cmbZona.Items.AddRange(Enum.GetNames(typeof(ZonaSismica)));
            _cmbZona.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbZona.SelectedIndex >= 0)
                { _entrada.Zona = (ZonaSismica)_cmbZona.SelectedIndex; InternalRefresh(); }
            };
            root.Controls.Add(_cmbZona, 1, 0);

            _lblZ = new Label
            {
                Text = "Z =", AutoSize = true, Font = fLabel,
                ForeColor = Color.DimGray, Margin = new Padding(18, 6, 4, 4)
            };
            root.Controls.Add(_lblZ, 2, 0);

            // ── Row 1: Parámetro de Suelo ────────────────────────────────────
            root.Controls.Add(new Label
            {
                Text = "Parámetro de Suelo :", AutoSize = false, Width = 220,
                Font = fLabel, Margin = new Padding(4, 6, 4, 4)
            }, 0, 1);

            _cmbSuelo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 60 };
            _cmbSuelo.Items.AddRange(Enum.GetNames(typeof(PerfilSuelo)));
            _cmbSuelo.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbSuelo.SelectedIndex >= 0)
                { _entrada.Suelo = (PerfilSuelo)_cmbSuelo.SelectedIndex; InternalRefresh(); }
            };
            root.Controls.Add(_cmbSuelo, 1, 1);

            var soilPanel = new FlowLayoutPanel
            {
                AutoSize = true, FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false, Margin = new Padding(12, 2, 4, 2)
            };
            _lblS = new Label { Text = "S =", AutoSize = false, Width = 55, Font = fLabel, ForeColor = Color.DimGray, Margin = new Padding(5, 0, 0, 0) };
            _lblTP = new Label { Text = "TP =", AutoSize = false, Width = 55, Font = fLabel, ForeColor = Color.DimGray };
            _lblTL = new Label { Text = "TL =", AutoSize = false, Width = 55, Font = fLabel, ForeColor = Color.DimGray, Margin = new Padding(10, 0, 0, 0) };
            soilPanel.Controls.Add(_lblS);
            soilPanel.Controls.Add(_lblTP);
            soilPanel.Controls.Add(_lblTL);
            root.Controls.Add(soilPanel, 2, 1);

            _lblSueloMsg = new Label
            {
                AutoSize = false, Size = new Size(360, 20),
                ForeColor = Color.Maroon, Font = new Font(fLabel, FontStyle.Italic),
                Margin = new Padding(24, 4, 4, 4)
            };
            root.Controls.Add(_lblSueloMsg, 3, 1);
            root.SetColumnSpan(_lblSueloMsg, 2);

            // ── Row 2: Factor de Amplificación Sísmica (Ct) ──────────────────
            root.Controls.Add(new Label
            {
                Text = "Factor de Amplificación Sísmica :", AutoSize = false, Width = 220,
                Font = fLabel, Margin = new Padding(4, 6, 4, 4)
            }, 0, 2);

            _cmbAmpl = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90 };
            _cmbAmpl.Items.AddRange(new object[] { "Ct=35", "Ct=45", "Ct=60" });
            _cmbAmpl.SelectedIndexChanged += (s, e) => InternalRefresh();
            root.Controls.Add(_cmbAmpl, 1, 2);

            var cPanel = new FlowLayoutPanel
            {
                AutoSize = true, FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false, Margin = new Padding(12, 2, 4, 2)
            };
            _lblC = new Label { Text = "C =", AutoSize = false, Width = 80, Font = fLabel, ForeColor = Color.DimGray, Margin = new Padding(5, 0, 0, 0) };
            _lblT = new Label { Text = "T =", AutoSize = false, Width = 80, Font = fLabel, ForeColor = Color.DimGray };
            cPanel.Controls.Add(_lblC);
            cPanel.Controls.Add(_lblT);
            root.Controls.Add(cPanel, 2, 2);

            // ── Row 3: Altura del Edificio (hn) ──────────────────────────────
            root.Controls.Add(new Label
            {
                Text = "Altura del Edificio hn (m) :", AutoSize = false, Width = 220,
                Font = fLabel, Margin = new Padding(4, 6, 4, 4)
            }, 0, 3);

            _nudAltura = new NumericUpDown
            {
                Width = 90, Minimum = 0, Maximum = 500, DecimalPlaces = 2,
                Increment = 0.50M, Value = 15.00M
            };
            _nudAltura.ValueChanged += (s, e) => InternalRefresh();
            root.Controls.Add(_nudAltura, 1, 3);

            // ── Row 4: Categoría de la Edificación ───────────────────────────
            root.Controls.Add(new Label
            {
                Text = "Categoría de la Edificación :", AutoSize = true,
                Font = fLabel, Margin = new Padding(4, 6, 4, 4)
            }, 0, 4);

            _cmbCategoria = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 60 };
            _cmbCategoria.Items.AddRange(Enum.GetNames(typeof(CategoriaEdificacion)));
            _cmbCategoria.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbCategoria.SelectedIndex >= 0)
                {
                    _entrada.Categoria = (CategoriaEdificacion)_cmbCategoria.SelectedIndex;
                    _entrada.U = E030Tables.GetUsoFactor(_entrada.Categoria.Value);
                    _lblU.Text = "U = " + _entrada.U.Value.ToString("G", CultureInfo.InvariantCulture);
                    InternalRefresh();
                }
            };
            root.Controls.Add(_cmbCategoria, 1, 4);

            _lblU = new Label
            {
                Text = "U =", AutoSize = true, Font = fLabel,
                ForeColor = Color.DimGray, Margin = new Padding(18, 6, 4, 4)
            };
            root.Controls.Add(_lblU, 2, 4);

            // ── Row 5: Título sistemas estructurales ─────────────────────────
            root.Controls.Add(new Label
            {
                Text = "Sistemas Estructurales en X y en Y :", AutoSize = true,
                Font = new Font(fLabel, FontStyle.Bold), Margin = new Padding(4, 10, 4, 4)
            }, 0, 5);

            // ── Row 6: Radio buttons for X (left) and Y (right) ─────────────
            var panelRadiosX = BuildSystemRadioPanel(addRadio: AddRadioX);
            root.Controls.Add(panelRadiosX, 0, 6);
            root.SetColumnSpan(panelRadiosX, 3);

            var panelRadiosY = BuildSystemRadioPanel(addRadio: AddRadioY);
            root.Controls.Add(panelRadiosY, 3, 6);
            root.SetColumnSpan(panelRadiosY, 2);

            // ── Row 7: Irregularities + Factors X (left) and Y (right) ──────
            var irregX = BuildIrregularitiesPanel(
                _chkIa, _chkIp, "X",
                out _txtRo, out _txtIa, out _txtIp, out _txtR,
                _entrada, () => { RecalcIrregularidades(); InternalRefresh(); });
            root.Controls.Add(irregX, 0, 7);
            root.SetColumnSpan(irregX, 2);

            var irregY = BuildIrregularitiesPanel(
                _chkIaY, _chkIpY, "Y",
                out _txtRoY, out _txtIaY, out _txtIpY, out _txtRY,
                _entradaY, () => { RecalcIrregularidadesY(); InternalRefresh(); });
            root.Controls.Add(irregY, 3, 7);
            root.SetColumnSpan(irregY, 2);

            // ── Set defaults ─────────────────────────────────────────────────
            if (_radioMap.TryGetValue(SistemaEstructural.PorticosCA, out var rbDefX))
                rbDefX.Checked = true;
            if (_radioMapY.TryGetValue(SistemaEstructural.PorticosCA, out var rbDefY))
                rbDefY.Checked = true;

            if (_cmbZona.Items.Count > 3) _cmbZona.SelectedIndex = 3;       // Z4
            if (_cmbSuelo.Items.Count > 1) _cmbSuelo.SelectedIndex = 1;     // S1
            if (_cmbAmpl.Items.Count > 0) _cmbAmpl.SelectedIndex = 0;       // Ct=35
            if (_cmbCategoria.Items.Count > 2) _cmbCategoria.SelectedIndex = 2; // B

            InternalRefresh();
        }

        // ── Helper: Build structural system radio panel ──────────────────────
        private FlowLayoutPanel BuildSystemRadioPanel(Action<FlowLayoutPanel, string, SistemaEstructural> addRadio)
        {
            var fLabel = new Font(SystemFonts.DefaultFont.FontFamily, 9f);

            var panel = new FlowLayoutPanel
            {
                AutoSize = true, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, Padding = new Padding(4), Margin = new Padding(4, 4, 8, 2)
            };

            // Acero group
            var grpAcero = new GroupBox
            {
                Text = "Acero :", AutoSize = true, Font = fLabel,
                Padding = new Padding(8), Margin = new Padding(0, 0, 0, 2)
            };
            var pnlAcero = new FlowLayoutPanel
            {
                AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false
            };
            addRadio(pnlAcero, "Pórticos Especiales Resistentes a Momento (SMF)", SistemaEstructural.SMF);
            addRadio(pnlAcero, "Pórticos Intermedios Resistentes a Momento (IMF)", SistemaEstructural.IMF);
            addRadio(pnlAcero, "Pórticos Ordinarios Resistentes a Momento (OMF)", SistemaEstructural.OMF);
            addRadio(pnlAcero, "Pórticos Especiales Concéntricamente Arriostrados (SCBF)", SistemaEstructural.SCBF);
            addRadio(pnlAcero, "Pórticos Ordinarios Concéntricamente Arriostrados (OCBF)", SistemaEstructural.OCBF);
            addRadio(pnlAcero, "Pórticos Excéntricamente Arriostrados (EBF)", SistemaEstructural.EBF);
            grpAcero.Controls.Add(pnlAcero);
            panel.Controls.Add(grpAcero);

            // Concreto Armado group
            var grpCA = new GroupBox
            {
                Text = "Concreto Armado :", AutoSize = true, Font = fLabel,
                Padding = new Padding(8), Margin = new Padding(0, 0, 0, 2)
            };
            var pnlCA = new FlowLayoutPanel
            {
                AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false
            };
            addRadio(pnlCA, "Pórticos", SistemaEstructural.PorticosCA);
            addRadio(pnlCA, "Dual", SistemaEstructural.Dual);
            addRadio(pnlCA, "Muros Estructurales", SistemaEstructural.Muros);
            addRadio(pnlCA, "Muros de Ductilidad Limitada", SistemaEstructural.MurosDuctilidadLimitada);
            grpCA.Controls.Add(pnlCA);
            panel.Controls.Add(grpCA);

            // Otros group
            var grpOtros = new GroupBox
            {
                Text = "Otros :", AutoSize = true, Font = fLabel,
                Padding = new Padding(8, 4, 8, 4), Margin = new Padding(0)
            };
            var pnlOtros = new FlowLayoutPanel
            {
                AutoSize = true, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, Margin = new Padding(4, 2, 4, 2)
            };
            addRadio(pnlOtros, "Albañilería Armada o Confinada", SistemaEstructural.Albanileria);
            addRadio(pnlOtros, "Madera (Esfuerzos Admisibles)", SistemaEstructural.Madera);
            grpOtros.Controls.Add(pnlOtros);
            panel.Controls.Add(grpOtros);

            return panel;
        }

        // ── Helper: Build irregularities panel with factors ──────────────────
        private TableLayoutPanel BuildIrregularitiesPanel(
            List<CheckBox> chkIaList, List<CheckBox> chkIpList, string direction,
            out TextBox txtRo, out TextBox txtIa, out TextBox txtIp, out TextBox txtR,
            EntradaE030 entrada, Action onChanged)
        {
            var fLabel = new Font(SystemFonts.DefaultFont.FontFamily, 9f);

            var layout = new TableLayoutPanel
            {
                AutoSize = true, ColumnCount = 1, Dock = DockStyle.Top,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0), Margin = new Padding(4, 0, 8, 4)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            // Irregularidades en Altura (Ia)
            var panelAltura = new FlowLayoutPanel
            {
                AutoSize = true, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, Padding = new Padding(4), Margin = new Padding(0)
            };
            panelAltura.Controls.Add(new Label
            {
                Text = "Irregularidades en Altura (Ia)", AutoSize = true,
                Font = new Font(fLabel, FontStyle.Bold), ForeColor = Color.DarkSlateBlue,
                Margin = new Padding(2, 0, 2, 4)
            });
            foreach (var item in E030Tables.IrregularidadesAltura)
            {
                var cb = new CheckBox
                {
                    Text = $"{item.Nombre} (Ia={item.Factor:G2})", AutoSize = true,
                    Tag = item, Font = new Font(fLabel.FontFamily, 8.3f),
                    Margin = new Padding(4, 2, 4, 2)
                };
                cb.CheckedChanged += (s, e) => onChanged();
                panelAltura.Controls.Add(cb);
                chkIaList.Add(cb);
            }
            layout.Controls.Add(panelAltura, 0, 0);

            // Irregularidades en Planta (Ip)
            var panelPlanta = new FlowLayoutPanel
            {
                AutoSize = true, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, Padding = new Padding(4), Margin = new Padding(0, 4, 0, 0)
            };
            panelPlanta.Controls.Add(new Label
            {
                Text = "Irregularidades en Planta (Ip)", AutoSize = true,
                Font = new Font(fLabel, FontStyle.Bold), ForeColor = Color.DarkSlateBlue,
                Margin = new Padding(2, 0, 2, 4)
            });
            foreach (var item in E030Tables.IrregularidadesPlanta)
            {
                var cb = new CheckBox
                {
                    Text = $"{item.Nombre} (Ip={item.Factor:G2})", AutoSize = true,
                    Tag = item, Font = new Font(fLabel.FontFamily, 8.3f),
                    Margin = new Padding(4, 2, 4, 2)
                };
                cb.CheckedChanged += (s, e) => onChanged();
                panelPlanta.Controls.Add(cb);
                chkIpList.Add(cb);
            }
            layout.Controls.Add(panelPlanta, 0, 1);

            // Separator + Factors row
            layout.Controls.Add(new Label
            {
                Text = $"──────── Factores {direction} (Ro, Ia, Ip, R) ────────",
                AutoSize = true, Font = new Font(fLabel, FontStyle.Bold),
                ForeColor = Color.DimGray, Margin = new Padding(6, 10, 6, 4)
            }, 0, 2);

            var factoresPanel = new FlowLayoutPanel
            {
                AutoSize = true, FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false, Padding = new Padding(4), Margin = new Padding(4, 2, 4, 4)
            };

            factoresPanel.Controls.Add(new Label { Text = "Ro:", AutoSize = true, Font = fLabel, Margin = new Padding(2, 6, 4, 6) });
            txtRo = new TextBox { Width = 70, ReadOnly = true, Margin = new Padding(2, 4, 8, 4) };
            factoresPanel.Controls.Add(txtRo);

            factoresPanel.Controls.Add(new Label { Text = "Ia:", AutoSize = true, Font = fLabel, Margin = new Padding(2, 6, 4, 6) });
            txtIa = new TextBox { Width = 40, ReadOnly = true, Margin = new Padding(2, 4, 8, 4), Text = entrada.Ia.ToString("G") };
            factoresPanel.Controls.Add(txtIa);

            factoresPanel.Controls.Add(new Label { Text = "Ip:", AutoSize = true, Font = fLabel, Margin = new Padding(2, 6, 4, 6) });
            txtIp = new TextBox { Width = 40, ReadOnly = true, Margin = new Padding(2, 4, 8, 4), Text = entrada.Ip.ToString("G") };
            factoresPanel.Controls.Add(txtIp);

            factoresPanel.Controls.Add(new Label { Text = "R:", AutoSize = true, Font = fLabel, Margin = new Padding(2, 6, 4, 6) });
            txtR = new TextBox { Width = 50, ReadOnly = true, Margin = new Padding(2, 4, 8, 4) };
            factoresPanel.Controls.Add(txtR);

            layout.Controls.Add(factoresPanel, 0, 3);

            return layout;
        }
    }
}
