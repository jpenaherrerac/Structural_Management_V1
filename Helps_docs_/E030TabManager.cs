using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using E030App.Models;
using Analisis_estructuralv1.BIBLIOTECA; // Diafragmas

namespace E030App.Models
{
    public enum ZonaSismica { Z1, Z2, Z3, Z4 }
    public enum PerfilSuelo { S0, S1, S2, S3, S4 }
    public enum CategoriaEdificacion { A1, A2, B, C }
    public enum SistemaEstructural
    {
        SMF, IMF, OMF,
        SCBF, OCBF, EBF,
        PorticosCA,
        Dual,
        Muros,
        MurosDuctilidadLimitada,
        Albanileria,
        Madera
    }
    public class EntradaE030
    {
        public ZonaSismica? Zona { get; set; }
        public PerfilSuelo? Suelo { get; set; }
        public double? Ct { get; set; }
        public double? C { get; set; }
        public double? T { get; set; }
        public CategoriaEdificacion? Categoria { get; set; }
        public double? U { get; set; }
        public SistemaEstructural? Sistema { get; set; }
        public bool SistemaIrregular { get; set; }
        public double Ia { get; set; } = 1.0;
        public double Ip { get; set; } = 1.0;
        public double R0 { get; set; }
        public double R => Ia * Ip * R0;
    }
}

namespace Analisis_estructuralv1.UI.TABS
{
    internal static class E030TablaLoader
    {
        private static bool _loaded;
        private static readonly Dictionary<string, Dictionary<string, double>> _S = new Dictionary<string, Dictionary<string, double>>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, (double TP, double TL)> _PT = new Dictionary<string, (double TP, double TL)>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, double> _R0 = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _lock = new object();
        public static void EnsureLoaded(string baseDir)
        { if (_loaded) return; lock (_lock) { if (_loaded) return; try { string path = Path.Combine(baseDir ?? "", "Docs", "E030_Tablas.md"); if (File.Exists(path)) { string md = File.ReadAllText(path); ParseS(md); ParsePeriodos(md); ParseR0(md); } _loaded = true; } catch { _loaded = true; } } }
        private static void ParseS(string md)
        {
            int idx = md.IndexOf("Tabla_003_FactorSueloS", StringComparison.OrdinalIgnoreCase); if (idx < 0) return; int brace = md.IndexOf('[', idx); int end = md.IndexOf(']', brace); if (brace < 0 || end < 0) return; string slice = md.Substring(brace, end - brace + 1);
            foreach (var row in slice.Split(new[] { '{' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!row.Contains("Zona")) continue; var zone = Extract(row, "Zona"); if (string.IsNullOrEmpty(zone)) continue; var dict = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                foreach (var perfil in new[] { "S0", "S1", "S2", "S3" }) { string valStr = Extract(row, perfil); if (double.TryParse(valStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double v)) dict[perfil] = v; }
                _S[zone] = dict;
            }
        }
        private static void ParsePeriodos(string md)
        {
            int idx = md.IndexOf("Tabla_004_TP_TL", StringComparison.OrdinalIgnoreCase); if (idx < 0) return; int brace = md.IndexOf('[', idx); int end = md.IndexOf(']', brace); if (brace < 0 || end < 0) return; string slice = md.Substring(brace, end - brace + 1);
            foreach (var row in slice.Split(new[] { '{' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!row.Contains("Perfil")) continue; var perfil = Extract(row, "Perfil"); if (string.IsNullOrEmpty(perfil)) return; string tpStr = Extract(row, "TP"); string tlStr = Extract(row, "TL"); if (double.TryParse(tpStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double tp) && double.TryParse(tlStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double tl)) _PT[perfil] = (tp, tl);
            }
        }
        private static void ParseR0(string md)
        {
            int idx = md.IndexOf("Tabla_007_SistemasEstructurales", StringComparison.OrdinalIgnoreCase); if (idx < 0) return; int brace = md.IndexOf('[', idx); int end = md.IndexOf(']', brace); if (brace < 0 || end < 0) return; string slice = md.Substring(brace, end - brace + 1);
            foreach (var row in slice.Split(new[] { '{' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!row.Contains("Sistema")) continue; var sistema = Extract(row, "Sistema"); if (string.IsNullOrEmpty(sistema)) continue; string r0Str = Extract(row, "R0"); if (double.TryParse(r0Str, NumberStyles.Float, CultureInfo.InvariantCulture, out double r0)) _R0[sistema] = r0;
            }
        }
        private static string Extract(string row, string key)
        {
            int k = row.IndexOf("\"" + key + "\"", StringComparison.OrdinalIgnoreCase); if (k < 0) return null; int colon = row.IndexOf(':', k); if (colon < 0) return null; int comma = row.IndexOf(',', colon + 1); if (comma < 0) comma = row.IndexOf('}', colon + 1); if (comma < 0) return null; string raw = row.Substring(colon + 1, comma - colon - 1).Trim(); raw = raw.Trim('"'); return raw;
        }
        public static double? GetS(ZonaSismica zona, PerfilSuelo perfil)
        { if (_S.TryGetValue(zona.ToString(), out var dict) && dict.TryGetValue(perfil.ToString(), out var v)) return v; return FallbackS(zona, perfil); }
        private static double? FallbackS(ZonaSismica zona, PerfilSuelo perfil)
        {
            switch (zona)
            {
                case ZonaSismica.Z4:
                    switch (perfil) { case PerfilSuelo.S0: return 0.80; case PerfilSuelo.S1: return 1.00; case PerfilSuelo.S2: return 1.05; case PerfilSuelo.S3: return 1.10; default: return null; }
                case ZonaSismica.Z3:
                    switch (perfil) { case PerfilSuelo.S0: return 0.80; case PerfilSuelo.S1: return 1.00; case PerfilSuelo.S2: return 1.15; case PerfilSuelo.S3: return 1.20; default: return null; }
                case ZonaSismica.Z2:
                    switch (perfil) { case PerfilSuelo.S0: return 0.80; case PerfilSuelo.S1: return 1.00; case PerfilSuelo.S2: return 1.20; case PerfilSuelo.S3: return 1.40; default: return null; }
                case ZonaSismica.Z1:
                    switch (perfil) { case PerfilSuelo.S0: return 0.80; case PerfilSuelo.S1: return 1.00; case PerfilSuelo.S2: return 1.60; case PerfilSuelo.S3: return 2.00; default: return null; }
                default: return null;
            }
        }
        public static (double TP, double TL)? GetPeriodos(PerfilSuelo perfil) { if (_PT.TryGetValue(perfil.ToString(), out var t)) return t; return FallbackPeriodos(perfil); }
        private static (double TP, double TL)? FallbackPeriodos(PerfilSuelo perfil) { switch (perfil) { case PerfilSuelo.S0: return (0.30, 3.00); case PerfilSuelo.S1: return (0.40, 2.50); case PerfilSuelo.S2: return (0.60, 2.00); case PerfilSuelo.S3: return (1.00, 1.60); default: return null; } }
        public static double? GetR0(SistemaEstructural sistema) { string key = sistema.ToString(); if (sistema == SistemaEstructural.PorticosCA) key = "Porticos_CA"; if (sistema == SistemaEstructural.MurosDuctilidadLimitada) key = "Muros_Duct_Lim"; if (_R0.TryGetValue(key, out var v)) return v; return null; }
    }

    // GroupBox sin borde (solo texto) para ocultar contorno gris manteniendo hijos.
    internal class BorderlessGroupBox : GroupBox
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            // No dibuja borde; solo el texto.
            e.Graphics.Clear(Parent?.BackColor ?? BackColor);
            using (var brush = new SolidBrush(ForeColor))
            {
                // Posición similar a GroupBox estándar
                var textLocation = new Point(8, 0);
                e.Graphics.DrawString(Text, Font, brush, textLocation);
            }
            // Ajuste visual opcional de fondo para área hijos (dejar transparente)
        }
    }

    public class E030TabManager
    {
        private Panel _scrollPanel;
        private Label _lblCtMsg;
        private Label _lblSueloMsg;
        private const double _defaultCt = 35.0; // respaldo Ct si no definido
        private readonly TabPage _host; private bool _built; private EntradaE030 _entrada = new EntradaE030(); // X
        private EntradaE030 _entradaY = new EntradaE030(); // Y
        private ComboBox _cmbZona; private ComboBox _cmbSuelo; private ComboBox _cmbAmpl; private ComboBox _cmbCategoria; private TextBox _txtRo; private TextBox _txtIa; private TextBox _txtIp; private TextBox _txtR; private Label _lblZ; private Label _lblS; private Label _lblTP; private Label _lblTL; private Label _lblC; private Label _lblT; private Label _lblU; private FlowLayoutPanel _panelRadios; private Dictionary<SistemaEstructural, RadioButton> _radioMap = new Dictionary<SistemaEstructural, RadioButton>();
        private FlowLayoutPanel _panelRadiosY; private Dictionary<SistemaEstructural, RadioButton> _radioMapY = new Dictionary<SistemaEstructural, RadioButton>(); private TextBox _txtRoY; private TextBox _txtIaY; private TextBox _txtIpY; private TextBox _txtRY;
        private readonly Dictionary<SistemaEstructural, double> _fallbackR0 = new Dictionary<SistemaEstructural, double> { { SistemaEstructural.SMF, 8 }, { SistemaEstructural.IMF, 5 }, { SistemaEstructural.OMF, 4 }, { SistemaEstructural.SCBF, 7 }, { SistemaEstructural.OCBF, 4 }, { SistemaEstructural.EBF, 8 }, { SistemaEstructural.PorticosCA, 8 }, { SistemaEstructural.Dual, 7 }, { SistemaEstructural.Muros, 6 }, { SistemaEstructural.MurosDuctilidadLimitada, 4 }, { SistemaEstructural.Albanileria, 3 }, { SistemaEstructural.Madera, 7 } };
        private FlowLayoutPanel _panelIrregAltura; private FlowLayoutPanel _panelIrregPlanta; private readonly List<CheckBox> _chkIa = new List<CheckBox>(); private readonly List<CheckBox> _chkIp = new List<CheckBox>();
        private FlowLayoutPanel _panelIrregAlturaY; private FlowLayoutPanel _panelIrregPlantaY; private readonly List<CheckBox> _chkIaY = new List<CheckBox>(); private readonly List<CheckBox> _chkIpY = new List<CheckBox>();
        private struct IrregularItem { public string Nombre; public double Factor; public string Descripcion; }
        private static readonly IrregularItem[] _irregAltura = new[]{
            new IrregularItem{ Nombre="Irregularidad de Rigidez – Piso Blando", Factor=0.75, Descripcion="Rigidez piso <70% superior o <80% promedio tres superiores."},
            new IrregularItem{ Nombre="Irregularidades de Resistencia – Piso Débil", Factor=0.75, Descripcion="Resistencia cortante piso <80% piso superior."},
            new IrregularItem{ Nombre="Irregularidad Extrema de Rigidez", Factor=0.50, Descripcion="Rigidez piso <60% superior o <70% promedio tres superiores."},
            new IrregularItem{ Nombre="Irregularidad Extrema de Resistencia", Factor=0.50, Descripcion="Resistencia cortante piso <65% piso superior."},
            new IrregularItem{ Nombre="Irregularidad de Masa o Peso", Factor=0.90, Descripcion="Peso piso >1.5× peso piso adyacente."},
            new IrregularItem{ Nombre="Irregularidad Geométrica Vertical", Factor=0.90, Descripcion="Dimensión planta >1.3× dimensión piso adyacente."},
            new IrregularItem{ Nombre="Discontinuidad en los Sistemas Resistentes", Factor=0.80, Descripcion="Elemento >10% cortante con desalineamiento vertical >25% dim elemento."},
            new IrregularItem{ Nombre="Discontinuidad Extrema de los Sistemas Resistentes", Factor=0.60, Descripcion="Elementos discontinuos >25% cortante total."}
        };
        private static readonly IrregularItem[] _irregPlanta = new[]{
            new IrregularItem{ Nombre="Irregularidad Torsional", Factor=0.75, Descripcion="Δmax >1.3 Δprom con excentricidad accidental."},
            new IrregularItem{ Nombre="Irregularidad Torsional Extrema", Factor=0.60, Descripcion="Δmax >1.5 Δprom con excentricidad accidental."},
            new IrregularItem{ Nombre="Esquinas Entrantes", Factor=0.90, Descripcion="Esquinas entrantes >20% dimensión total."},
            new IrregularItem{ Nombre="Discontinuidad del Diafragma", Factor=0.85, Descripcion="Aberturas >50% área o sección <25% área neta."},
            new IrregularItem{ Nombre="Sistemas no Paralelos", Factor=0.90, Descripcion="Elementos no paralelos (ángulo >30° y >10% cortante)."}
        };
        private double GetZFactor(ZonaSismica zona) { switch (zona) { case ZonaSismica.Z1: return 0.10; case ZonaSismica.Z2: return 0.25; case ZonaSismica.Z3: return 0.35; case ZonaSismica.Z4: return 0.45; default: return 0.0; } }
        private double GetUsoFactor(CategoriaEdificacion categoria) { switch (categoria) { case CategoriaEdificacion.A1: return 1.50; case CategoriaEdificacion.A2: return 1.50; case CategoriaEdificacion.B: return 1.30; case CategoriaEdificacion.C: return 1.0; default: return 1.00; } }
        private readonly Dictionary<string, double> _valores = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        // Flags para evitar recursión al desmarcar radios
        private bool _suppressRadioX;
        private bool _suppressRadioY;

        // NUEVO: evento para avisar selección de sistema estructural por dirección
        public class SistemaSeleccionChangedEventArgs : EventArgs
        {
            public char Direccion { get; private set; } // 'X' o 'Y'
            public SistemaEstructural? Sistema { get; private set; }
            public SistemaSeleccionChangedEventArgs(char dir, SistemaEstructural? sis){ Direccion = dir; Sistema = sis; }
        }
        public event EventHandler<SistemaSeleccionChangedEventArgs> SistemaSeleccionChanged;

        // Accesores públicos para consulta externa
        public SistemaEstructural? SistemaSeleccionadoX => _entrada?.Sistema;
        public SistemaEstructural? SistemaSeleccionadoY => _entradaY?.Sistema;
        public (SistemaEstructural? X, SistemaEstructural? Y) GetSistemasSeleccionados() => (SistemaSeleccionadoX, SistemaSeleccionadoY);

        public E030TabManager(TabPage host) { _host = host ?? throw new ArgumentNullException(nameof(host)); ConstruirUIInmediata(); }
        private void ConstruirUIInmediata(){ if(_built) return; _host.Controls.Clear();
            _scrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White }; _host.Controls.Add(_scrollPanel);
            _host.AutoScroll = false; E030TablaLoader.EnsureLoaded(AppDomain.CurrentDomain.BaseDirectory);
            var root = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 5, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(8) }; for (int i = 0; i < 5; i++) root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); _scrollPanel.Controls.Add(root);
            root.ColumnStyles[3] = new ColumnStyle(SizeType.Absolute, 380f); root.ColumnStyles[4] = new ColumnStyle(SizeType.Absolute, 1f);
            var fLabel = new Font(SystemFonts.DefaultFont.FontFamily, 9f, FontStyle.Regular);
            root.Controls.Add(new Label { Text = "Factor de Zona :", AutoSize = false, Font = fLabel, Margin = new Padding(4, 6, 4, 4) }, 0, 0);
            _cmbZona = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 60 }; _cmbZona.Items.AddRange(Enum.GetNames(typeof(ZonaSismica))); _cmbZona.SelectedIndexChanged += (s, e) => { if (_cmbZona.SelectedIndex >= 0) { _entrada.Zona = (ZonaSismica)_cmbZona.SelectedIndex; InternalRefresh(); } }; root.Controls.Add(_cmbZona, 1, 0);
            _lblZ = new Label { Text = "Z =", AutoSize = true, Font = fLabel, ForeColor = Color.DimGray, Margin = new Padding(18, 6, 4, 4) }; root.Controls.Add(_lblZ, 2, 0);
            root.Controls.Add(new Label { Text = "Parámetro de Suelo :", AutoSize = false, Width = 220, Font = fLabel, Margin = new Padding(4, 6, 4, 4) }, 0, 1);
            _cmbSuelo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 60 }; _cmbSuelo.Items.AddRange(Enum.GetNames(typeof(PerfilSuelo))); _cmbSuelo.SelectedIndexChanged += (s, e) => { if (_cmbSuelo.SelectedIndex >= 0) { _entrada.Suelo = (PerfilSuelo)_cmbSuelo.SelectedIndex; InternalRefresh(); } }; root.Controls.Add(_cmbSuelo, 1, 1);
            var soilPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Margin = new Padding(12, 2, 4, 2) }; _lblS = new Label { Text = "S =", AutoSize = false, Width = 55, Font = fLabel, ForeColor = Color.DimGray, Margin = new Padding(5, 0, 0, 0) }; _lblTP = new Label { Text = "TP =", AutoSize = false, Width = 55, Font = fLabel, ForeColor = Color.DimGray }; _lblTL = new Label { Text = "TL =", AutoSize = false, Width = 55, Font = fLabel, ForeColor = Color.DimGray, Margin = new Padding(10, 0, 0, 0) }; soilPanel.Controls.Add(_lblS); soilPanel.Controls.Add(_lblTP); soilPanel.Controls.Add(_lblTL); root.Controls.Add(soilPanel, 2, 1);
            _lblSueloMsg = new Label { AutoSize = false, Size = new Size(360, fLabel.Height - 280), ForeColor = Color.Maroon, Font = new Font(fLabel, FontStyle.Italic), Text = "", Margin = new Padding(24, 4, 4, 4) }; root.Controls.Add(_lblSueloMsg, 3, 1); root.SetColumnSpan(_lblSueloMsg, 2);
            root.Controls.Add(new Label { Text = "Factor de Amplificacion Sismica :", AutoSize = false, Width = 220, Font = fLabel, Margin = new Padding(4, 6, 4, 4) }, 0, 2);
            _cmbAmpl = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90 }; _cmbAmpl.Items.Clear(); _cmbAmpl.Items.AddRange(new[] { "Ct=35", "Ct=45", "Ct=60" }); _cmbAmpl.SelectedIndexChanged += (s, e) => { UpdateR(); InternalRefresh(); }; root.Controls.Add(_cmbAmpl, 1, 2);
            var cPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Margin = new Padding(12, 2, 4, 2) }; _lblC = new Label { Text = "C =", AutoSize = false, Width = 55, Font = fLabel, ForeColor = Color.DimGray, Margin = new Padding(5, 0, 0, 0) }; _lblT = new Label { Text = "T =", AutoSize = false, Width = 80, Font = fLabel, ForeColor = Color.DimGray, Margin = new Padding(0, 0, 10, 0) }; cPanel.Controls.Add(_lblC); cPanel.Controls.Add(_lblT); root.Controls.Add(cPanel, 2, 2);
            _lblCtMsg = new Label { AutoSize = false, Size = new Size(600, fLabel.Height + 18), ForeColor = Color.Maroon, Font = new Font(fLabel, FontStyle.Italic), Text = "", Margin = new Padding(0, 4, 4, 4) }; root.Controls.Add(_lblCtMsg, 3, 2); root.SetColumnSpan(_lblCtMsg, 2);
            root.Controls.Add(new Label { Text = "Categoría de la Edificación :", AutoSize = true, Font = fLabel, Margin = new Padding(4, 6, 4, 4) }, 0, 3); _cmbCategoria = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 60 }; _cmbCategoria.Items.AddRange(Enum.GetNames(typeof(CategoriaEdificacion))); _cmbCategoria.SelectedIndexChanged += (s, e) => { if (_cmbCategoria.SelectedIndex >= 0) { _entrada.Categoria = (CategoriaEdificacion)_cmbCategoria.SelectedIndex; _entrada.U = GetUsoFactor(_entrada.Categoria.Value); _lblU.Text = "U = " + _entrada.U.Value.ToString("G", CultureInfo.InvariantCulture); InternalRefresh(); } }; root.Controls.Add(_cmbCategoria, 1, 3); _lblU = new Label { Text = "U =", AutoSize = true, Font = fLabel, ForeColor = Color.DimGray, Margin = new Padding(18, 6, 4, 4) }; root.Controls.Add(_lblU, 2, 3);
            var lblSistemaX = new Label { Text = "Sistemas Estructurales en X y en Y :", AutoSize = true, Font = new Font(fLabel, FontStyle.Bold), Margin = new Padding(4, 10, 4, 4) }; root.Controls.Add(lblSistemaX, 0, 4);
            _panelRadios = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(4), Margin = new Padding(4, 4, 8, 2) }; var grpAcero = new BorderlessGroupBox { Text = "Acero :", AutoSize = true, Font = fLabel, Padding = new Padding(8), Margin = new Padding(0, 0, 0, 2) }; var pnlAcero = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false }; AddRadio(pnlAcero, "Pórticos Especiales Resistentes a Momento (SMF)", SistemaEstructural.SMF); AddRadio(pnlAcero, "Pórticos Intermedios Resistentes a Momento (IMF)", SistemaEstructural.IMF); AddRadio(pnlAcero, "Pórticos Ordinarios Resistentes a Momento (OMF)", SistemaEstructural.OMF); AddRadio(pnlAcero, "Pórticos Especiales Concéntricamente Arriostrados (SCBF)", SistemaEstructural.SCBF); AddRadio(pnlAcero, "Pórticos Ordinarios Concéntricamente Arriostrados (OCBF)", SistemaEstructural.OCBF); AddRadio(pnlAcero, "Pórticos Excéntricamente Arriostrados (EBF)", SistemaEstructural.EBF); grpAcero.Controls.Add(pnlAcero); _panelRadios.Controls.Add(grpAcero);
            var grpCA = new GroupBox { Text = "Concreto Armado :", AutoSize = true, Font = fLabel, Padding = new Padding(8), Margin = new Padding(0, 0, 0, 2) }; var pnlCA = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false }; AddRadio(pnlCA, "Pórticos", SistemaEstructural.PorticosCA); AddRadio(pnlCA, "Dual", SistemaEstructural.Dual); AddRadio(pnlCA, "Muros Estructurales", SistemaEstructural.Muros); AddRadio(pnlCA, "Muros de Ductibilidad Limitada", SistemaEstructural.MurosDuctilidadLimitada); grpCA.Controls.Add(pnlCA); _panelRadios.Controls.Add(grpCA); grpCA.AutoSize = false; grpCA.Height = Math.Max(0, pnlCA.PreferredSize.Height + grpCA.Padding.Vertical - 19);
            var grpOtros = new GroupBox { Text = "Otros :", AutoSize = false, Font = fLabel, Padding = new Padding(8, 4, 8, 4), Margin = new Padding(0, 0, 0, 0) }; var pnlOtros = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Margin = new Padding(4, 2, 4, 2) }; AddRadio(pnlOtros, "Albañilería Armada o Confinada", SistemaEstructural.Albanileria); AddRadio(pnlOtros, "Madera (Esfuerzos Admisibles)", SistemaEstructural.Madera); grpOtros.Controls.Add(pnlOtros); grpOtros.Width = grpCA.Width; grpOtros.Height = pnlOtros.PreferredSize.Height + grpOtros.Padding.Vertical + 4; _panelRadios.Controls.Add(grpOtros); root.Controls.Add(_panelRadios, 0, 5); root.SetColumnSpan(_panelRadios, 3);
            var irregLayoutX = new TableLayoutPanel { AutoSize = true, ColumnCount = 1, Dock = DockStyle.Top, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(0), Margin = new Padding(4, 0, 8, 4) }; irregLayoutX.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _panelIrregAltura = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(4), Margin = new Padding(0) }; _panelIrregAltura.Controls.Add(new Label { Text = "Irregularidades en Altura (Ia)", AutoSize = true, Font = new Font(fLabel, FontStyle.Bold), ForeColor = Color.DarkSlateBlue, Margin = new Padding(2, 0, 2, 4) }); foreach (var item in _irregAltura) { var cbX = new CheckBox { Text = $"{item.Nombre} (Ia={item.Factor:G2})", AutoSize = true, Tag = item, Font = new Font(fLabel.FontFamily, 8.3f, FontStyle.Regular), Margin = new Padding(4, 2, 4, 2) }; cbX.CheckedChanged += (s, e) => { RecalcIrregularidades(); InternalRefresh(); }; _panelIrregAltura.Controls.Add(cbX); _chkIa.Add(cbX); } irregLayoutX.Controls.Add(_panelIrregAltura, 0, 0);
            _panelIrregPlanta = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(4), Margin = new Padding(0, 4, 0, 0) }; _panelIrregPlanta.Controls.Add(new Label { Text = "Irregularidades en Planta (Ip)", AutoSize = true, Font = new Font(fLabel, FontStyle.Bold), ForeColor = Color.DarkSlateBlue, Margin = new Padding(2, 0, 2, 4) }); foreach (var item in _irregPlanta) { var cbXP = new CheckBox { Text = $"{item.Nombre} (Ip={item.Factor:G2})", AutoSize = true, Tag = item, Font = new Font(fLabel.FontFamily, 8.3f, FontStyle.Regular), Margin = new Padding(4, 2, 4, 2) }; cbXP.CheckedChanged += (s, e) => { RecalcIrregularidades(); InternalRefresh(); }; _panelIrregPlanta.Controls.Add(cbXP); _chkIp.Add(cbXP); } irregLayoutX.Controls.Add(_panelIrregPlanta, 0, 1);
            irregLayoutX.Controls.Add(new Label { Text = "──────── Factores X (Ro, Ia, Ip, R) ────────", AutoSize = true, Font = new Font(fLabel, FontStyle.Bold), ForeColor = Color.DimGray, Margin = new Padding(6, 10, 6, 4) }, 0, 2);
            var factoresX = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(4), Margin = new Padding(4, 2, 4, 4) }; factoresX.Controls.Add(new Label { Text = "Ro:", AutoSize = true, Font = fLabel, Margin = new Padding(2, 6, 4, 6) }); _txtRo = new TextBox { Width = 70, ReadOnly = true, Margin = new Padding(2, 4, 8, 4) }; factoresX.Controls.Add(_txtRo); factoresX.Controls.Add(new Label { Text = "Ia:", AutoSize = true, Font = fLabel, Margin = new Padding(2, 6, 4, 6) }); _txtIa = new TextBox { Width = 40, ReadOnly = true, Margin = new Padding(2, 4, 8, 4), Text = _entrada.Ia.ToString("G") }; factoresX.Controls.Add(_txtIa); factoresX.Controls.Add(new Label { Text = "Ip:", AutoSize = true, Font = fLabel, Margin = new Padding(2, 6, 4, 6) }); _txtIp = new TextBox { Width = 40, ReadOnly = true, Margin = new Padding(2, 4, 8, 4), Text = _entrada.Ip.ToString("G") }; factoresX.Controls.Add(_txtIp); factoresX.Controls.Add(new Label { Text = "R:", AutoSize = true, Font = fLabel, Margin = new Padding(2, 6, 4, 6) }); _txtR = new TextBox { Width = 50, ReadOnly = true, Margin = new Padding(2, 4, 8, 4) }; factoresX.Controls.Add(_txtR); irregLayoutX.Controls.Add(factoresX, 0, 3); root.Controls.Add(irregLayoutX, 0, 6); root.SetColumnSpan(irregLayoutX, 2);
            _panelRadiosY = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(4), Margin = new Padding(4, 4, 8, 2) }; var grpAceroY = new BorderlessGroupBox { Text = "Acero :", AutoSize = true, Font = fLabel, Padding = new Padding(8), Margin = new Padding(0, 0, 0, 2) }; var pnlAceroY = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false }; AddRadioY(pnlAceroY, "Pórticos Especiales Resistentes a Momento (SMF)", SistemaEstructural.SMF); AddRadioY(pnlAceroY, "Pórticos Intermedios Resistentes a Momento (IMF)", SistemaEstructural.IMF); AddRadioY(pnlAceroY, "Pórticos Ordinarios Resistentes a Momento (OMF)", SistemaEstructural.OMF); AddRadioY(pnlAceroY, "Pórticos Especiales Concéntricamente Arriostrados (SCBF)", SistemaEstructural.SCBF); AddRadioY(pnlAceroY, "Pórticos Ordinarios Concéntricamente Arriostrados (OCBF)", SistemaEstructural.OCBF); AddRadioY(pnlAceroY, "Pórticos Excéntricamente Arriostrados (EBF)", SistemaEstructural.EBF); grpAceroY.Controls.Add(pnlAceroY); _panelRadiosY.Controls.Add(grpAceroY);
            var grpCAY = new GroupBox { Text = "Concreto Armado :", AutoSize = true, Font = fLabel, Padding = new Padding(8), Margin = new Padding(0, 0, 0, 2) }; var pnlCAY = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false }; AddRadioY(pnlCAY, "Pórticos", SistemaEstructural.PorticosCA); AddRadioY(pnlCAY, "Dual", SistemaEstructural.Dual); AddRadioY(pnlCAY, "Muros Estructurales", SistemaEstructural.Muros); AddRadioY(pnlCAY, "Muros de Ductibilidad Limitada", SistemaEstructural.MurosDuctilidadLimitada); grpCAY.Controls.Add(pnlCAY); _panelRadiosY.Controls.Add(grpCAY); grpCAY.AutoSize = false; grpCAY.Height = Math.Max(0, pnlCAY.PreferredSize.Height + grpCAY.Padding.Vertical - 19);
            var grpOtrosY = new GroupBox { Text = "Otros :", AutoSize = true, Font = fLabel, Padding = new Padding(8, 4, 8, 4), Margin = new Padding(0, 0, 0, 0) }; var pnlOtrosY = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Margin = new Padding(4, 2, 40, 2) }; AddRadioY(pnlOtrosY, "Albañilería Armada o Confinada", SistemaEstructural.Albanileria); AddRadioY(pnlOtrosY, "Madera (Esfuerzos Admisibles)", SistemaEstructural.Madera); grpOtrosY.Controls.Add(pnlOtrosY); _panelRadiosY.Controls.Add(grpOtrosY); grpOtrosY.AutoSize = false; grpOtrosY.Width = grpCAY.Width; grpOtrosY.Height = pnlOtrosY.PreferredSize.Height + grpOtrosY.Padding.Vertical + 4; root.Controls.Add(_panelRadiosY, 3, 5); root.SetColumnSpan(_panelRadiosY, 2);
            var irregLayoutY = new TableLayoutPanel { AutoSize = true, ColumnCount = 1, Dock = DockStyle.Top, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(0), Margin = new Padding(4, 0, 4, 4) }; irregLayoutY.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _panelIrregAlturaY = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(4), Margin = new Padding(0) }; _panelIrregAlturaY.Controls.Add(new Label { Text = "Irregularidades en Altura (Ia)", AutoSize = true, Font = new Font(fLabel, FontStyle.Bold), ForeColor = Color.DarkSlateBlue, Margin = new Padding(2, 0, 2, 4) }); foreach (var item in _irregAltura) { var cbY = new CheckBox { Text = $"{item.Nombre} (Ia={item.Factor:G2})", AutoSize = true, Tag = item, Font = new Font(fLabel.FontFamily, 8.3f, FontStyle.Regular), Margin = new Padding(4, 2, 4, 2) }; cbY.CheckedChanged += (s, e) => { RecalcIrregularidadesY(); InternalRefresh(); }; _panelIrregAlturaY.Controls.Add(cbY); _chkIaY.Add(cbY); } irregLayoutY.Controls.Add(_panelIrregAlturaY, 0, 0);
            _panelIrregPlantaY = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(4), Margin = new Padding(0, 4, 0, 0) }; _panelIrregPlantaY.Controls.Add(new Label { Text = "Irregularidades en Planta (Ip)", AutoSize = true, Font = new Font(fLabel, FontStyle.Bold), ForeColor = Color.DarkSlateBlue, Margin = new Padding(2, 0, 2, 4) }); foreach (var item in _irregPlanta) { var cbYP = new CheckBox { Text = $"{item.Nombre} (Ip={item.Factor:G2})", AutoSize = true, Tag = item, Font = new Font(fLabel.FontFamily, 8.3f, FontStyle.Regular), Margin = new Padding(4, 2, 4, 2) }; cbYP.CheckedChanged += (s, e) => { RecalcIrregularidadesY(); InternalRefresh(); }; _panelIrregPlantaY.Controls.Add(cbYP); _chkIpY.Add(cbYP); } irregLayoutY.Controls.Add(_panelIrregPlantaY, 0, 1);
            irregLayoutY.Controls.Add(new Label { Text = "──────── Factores Y (Ro, Ia, Ip, R) ────────", AutoSize = true, Font = new Font(fLabel, FontStyle.Bold), ForeColor = Color.DimGray, Margin = new Padding(6, 10, 6, 4) }, 0, 2);
            var factoresY = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(4), Margin = new Padding(4, 2, 4, 4) }; factoresY.Controls.Add(new Label { Text = "Ro:", AutoSize = true, Font = fLabel, Margin = new Padding(2, 6, 4, 6) }); _txtRoY = new TextBox { Width = 70, ReadOnly = true, Margin = new Padding(2, 4, 8, 4) }; factoresY.Controls.Add(_txtRoY); factoresY.Controls.Add(new Label { Text = "Ia:", AutoSize = true, Font = fLabel, Margin = new Padding(2, 6, 4, 6) }); _txtIaY = new TextBox { Width = 40, ReadOnly = true, Margin = new Padding(2, 4, 8, 4), Text = _entradaY.Ia.ToString("G") }; factoresY.Controls.Add(_txtIaY); factoresY.Controls.Add(new Label { Text = "Ip:", AutoSize = true, Font = fLabel, Margin = new Padding(2, 6, 4, 6) }); _txtIpY = new TextBox { Width = 40, ReadOnly = true, Margin = new Padding(2, 4, 8, 4), Text = _entradaY.Ip.ToString("G") }; factoresY.Controls.Add(_txtIpY); factoresY.Controls.Add(new Label { Text = "R:", AutoSize = true, Font = fLabel, Margin = new Padding(2, 6, 4, 6) }); _txtRY = new TextBox { Width = 50, ReadOnly = true, Margin = new Padding(2, 4, 8, 4) }; factoresY.Controls.Add(_txtRY); irregLayoutY.Controls.Add(factoresY, 0, 3); root.Controls.Add(irregLayoutY, 3, 6); root.SetColumnSpan(irregLayoutY, 2);
            if (_radioMap.TryGetValue(SistemaEstructural.PorticosCA, out var rbDefX)) rbDefX.Checked = true; if (_radioMapY.TryGetValue(SistemaEstructural.PorticosCA, out var rbDefY)) rbDefY.Checked = true;
            if (_cmbZona != null && _cmbZona.SelectedIndex < 0 && _cmbZona.Items.Count > 3) _cmbZona.SelectedIndex = 3; if (_cmbSuelo != null && _cmbSuelo.SelectedIndex < 0 && _cmbSuelo.Items.Count > 1) _cmbSuelo.SelectedIndex = 1; if (_cmbAmpl != null && _cmbAmpl.SelectedIndex < 0 && _cmbAmpl.Items.Count > 0) _cmbAmpl.SelectedIndex = 0; if (_cmbCategoria != null && _cmbCategoria.SelectedIndex < 0 && _cmbCategoria.Items.Count > 2) _cmbCategoria.SelectedIndex = 2;
            _built = true; InternalRefresh(); }
        public EntradaE030 GetEntrada() => _entrada;
        public void RefreshData(){ if(_built) { InternalRefresh(); ValoresActualesChanged?.Invoke(this, EventArgs.Empty); } }
        private void AddRadio(FlowLayoutPanel parent, string text, SistemaEstructural sistema)
        {
            var rb = new RadioButton
            {
                Text = text,
                AutoSize = true,
                Tag = sistema,
                Font = new Font(SystemFonts.DefaultFont.FontFamily, 8.5f, FontStyle.Regular),
                Margin = new Padding(4, 2, 4, 2)
            };

            rb.CheckedChanged += (s, e) =>
            {
                if (!rb.Checked) return;
                if (_suppressRadioX) return;

                try
                {
                    _suppressRadioX = true;
                    foreach (var kv in _radioMap)
                    {
                        if (!ReferenceEquals(kv.Value, rb) && kv.Value.Checked)
                            kv.Value.Checked = false;
                    }
                    _entrada.Sistema = sistema;
                    var r0 = E030TablaLoader.GetR0(sistema) ?? (_fallbackR0.TryGetValue(sistema, out var v) ? v : 0.0); // FIX: use null-coalescing instead of logical OR
                    _entrada.R0 = r0;
                    if(_txtRo!=null) _txtRo.Text = _entrada.R0.ToString("G", CultureInfo.InvariantCulture);
                    UpdateR();
                    // Disparar evento selección X
                    SistemaSeleccionChanged?.Invoke(this, new SistemaSeleccionChangedEventArgs('X', _entrada.Sistema));
                }
                finally
                {
                    _suppressRadioX = false;
                }
            };

            parent.Controls.Add(rb);
            _radioMap[sistema] = rb;
        }
        private void AddRadioY(FlowLayoutPanel parent, string text, SistemaEstructural sistema)
        {
            var rb = new RadioButton
            {
                Text = text,
                AutoSize = true,
                Tag = sistema,
                Font = new Font(SystemFonts.DefaultFont.FontFamily, 8.5f, FontStyle.Regular),
                Margin = new Padding(4, 2, 4, 2)
            };

            rb.CheckedChanged += (s, e) =>
            {
                if (!rb.Checked) return;
                if (_suppressRadioY) return;

                try
                {
                    _suppressRadioY = true;
                    foreach (var kv in _radioMapY)
                    {
                        if (!ReferenceEquals(kv.Value, rb) && kv.Value.Checked)
                            kv.Value.Checked = false;
                    }
                    _entradaY.Sistema = sistema;
                    var r0 = E030TablaLoader.GetR0(sistema) ?? (_fallbackR0.TryGetValue(sistema, out var v) ? v : 0.0); // FIX: use null-coalescing instead of logical OR
                    _entradaY.R0 = r0;
                    if(_txtRoY!=null) _txtRoY.Text = _entradaY.R0.ToString("G", CultureInfo.InvariantCulture);
                    UpdateRY();
                    // Disparar evento selección Y
                    SistemaSeleccionChanged?.Invoke(this, new SistemaSeleccionChangedEventArgs('Y', _entradaY.Sistema));
                }
                finally
                {
                    _suppressRadioY = false;
                }
            };

            parent.Controls.Add(rb);
            _radioMapY[sistema] = rb;
        }
        private void RecalcSuelo() { if (_entrada.Zona.HasValue) _lblZ.Text = "Z = " + GetZFactor(_entrada.Zona.Value).ToString("G", CultureInfo.InvariantCulture); if (_entrada.Suelo.HasValue) { switch (_entrada.Suelo.Value) { case PerfilSuelo.S0: _lblSueloMsg.Text = "S0: Roca dura"; break; case PerfilSuelo.S1: _lblSueloMsg.Text = "S1: Roca o suelos muy rígidos"; break; case PerfilSuelo.S2: _lblSueloMsg.Text = "S2: Suelos intermedios"; break; case PerfilSuelo.S3: _lblSueloMsg.Text = "S3: Suelos blandos"; break; case PerfilSuelo.S4: _lblSueloMsg.Text = "S4: Condición excepcional. Ingresar S, TP, TL"; break; default: _lblSueloMsg.Text = string.Empty; break; } } if (_entrada.Zona.HasValue && _entrada.Suelo.HasValue) { var sVal = E030TablaLoader.GetS(_entrada.Zona.Value, _entrada.Suelo.Value); _lblS.Text = sVal.HasValue ? "S = " + sVal.Value.ToString("G", CultureInfo.InvariantCulture) : "S = (definir)"; var per = E030TablaLoader.GetPeriodos(_entrada.Suelo.Value); if (per.HasValue) { _lblTP.Text = "TP = " + per.Value.TP.ToString("G", CultureInfo.InvariantCulture); _lblTL.Text = "TL = " + per.Value.TL.ToString("G", CultureInfo.InvariantCulture); } else { _lblTP.Text = "TP = (definir)"; _lblTL.Text = "TL = (definir)"; } } }
        private void RecalcIrregularidades() { var iaVals = _chkIa.Where(cb => cb.Checked).Select(cb => ((IrregularItem)cb.Tag).Factor).ToList(); var ipVals = _chkIp.Where(cb => cb.Checked).Select(cb => ((IrregularItem)cb.Tag).Factor).ToList(); _entrada.Ia = iaVals.Count > 0 ? iaVals.Min() : 1.0; _entrada.Ip = ipVals.Count > 0 ? ipVals.Min() : 1.0; if (_txtIa != null) _txtIa.Text = _entrada.Ia.ToString("G", CultureInfo.InvariantCulture); if (_txtIp != null) _txtIp.Text = _entrada.Ip.ToString("G", CultureInfo.InvariantCulture); }
        private void RecalcIrregularidadesY() { var iaVals = _chkIaY.Where(cb => cb.Checked).Select(cb => ((IrregularItem)cb.Tag).Factor).ToList(); var ipVals = _chkIpY.Where(cb => cb.Checked).Select(cb => ((IrregularItem)cb.Tag).Factor).ToList(); _entradaY.Ia = iaVals.Count > 0 ? iaVals.Min() : 1.0; _entradaY.Ip = ipVals.Count > 0 ? ipVals.Min() : 1.0; _txtIaY.Text = _entradaY.Ia.ToString("G", CultureInfo.InvariantCulture); _txtIpY.Text = _entradaY.Ip.ToString("G", CultureInfo.InvariantCulture); }
        private void UpdateR() { if (_txtR != null) _txtR.Text = _entrada.R.ToString("G", CultureInfo.InvariantCulture); }
        private void UpdateRY() { if (_txtRY != null) _txtRY.Text = _entradaY.R.ToString("G", CultureInfo.InvariantCulture); }
        // Nueva función unificada CALCULAR_C según fórmula solicitada
        public double CALCULAR_C()
        {
            double T = _entrada.T ?? 0.0;
            double TP = 0.30, TL = 3.00;
            if (_entrada.Suelo.HasValue)
            {
                var per = E030TablaLoader.GetPeriodos(_entrada.Suelo.Value);
                if (per.HasValue){ TP = per.Value.TP; TL = per.Value.TL; }
            }
            if (T <= 0){ _entrada.C = null; if (_lblC != null) _lblC.Text = "C = 0"; ActualizarDiccionario(TP, TL); return 0.0; }
            double Cval;
            if (T <= TP) Cval = 2.5; else if (T <= TL) Cval = 2.5 * TP / T; else Cval = 2.5 * TP * TL / (T * T);
            _entrada.C = Cval; if (_lblC != null) _lblC.Text = "C = " + Cval.ToString("G4", CultureInfo.InvariantCulture);
            ActualizarDiccionario(TP, TL); return Cval;
        }
        private void ActualizarDiccionario(double TP, double TL)
        {
            double z = _entrada.Zona.HasValue ? GetZFactor(_entrada.Zona.Value) : 0.0;
            double u = _entrada.U ?? 0.0;
            double s = (_entrada.Zona.HasValue && _entrada.Suelo.HasValue) ? (E030TablaLoader.GetS(_entrada.Zona.Value, _entrada.Suelo.Value) ?? 0.0) : 0.0;
            double t = _entrada.T ?? 0.0;
            double rX = _entrada.R;
            double roX = _entrada.R0;
            double iaX = _entrada.Ia;
            double ipX = _entrada.Ip;
            double rY = _entradaY.R;
            double roY = _entradaY.R0;
            double iaY = _entradaY.Ia;
            double ipY = _entradaY.Ip;
            // Base (sin duplicar R, Ro, Ip) -> solo guardar Ia base si se desea referencia global
            _valores["Z"] = z; _valores["U"] = u; _valores["S"] = s; _valores["T"] = t; _valores["TP"] = TP; _valores["TL"] = TL; _valores["Ia"] = iaX; _valores["C"] = _entrada.C ?? 0.0;
            // Direccional X
            _valores["R_x"] = rX; _valores["Ro_x"] = roX; _valores["Ia_x"] = iaX; _valores["Ip_x"] = ipX;
            // Direccional Y
            _valores["R_y"] = rY; _valores["Ro_y"] = roY; _valores["Ia_y"] = iaY; _valores["Ip_y"] = ipY;
            // Eliminar claves base antiguas si existieran
            _valores.Remove("R"); _valores.Remove("Ro"); _valores.Remove("Ip");
        }
        public IReadOnlyDictionary<string,double> GetValoresActuales() => _valores;
        public event EventHandler ValoresActualesChanged; // nuevo evento notificacion
        private void InternalRefresh()
        {
            RecalcSuelo();
            RecalcIrregularidades();
            RecalcIrregularidadesY();
            UpdateR();
            UpdateRY();
            CalcularT();
            CALCULAR_C();
            ValoresActualesChanged?.Invoke(this, EventArgs.Empty); // disparar evento
        }
        private void CalcularT()
        {
            if (_cmbAmpl == null || _lblT == null) return;
            double ct = 0.0;
            if (_cmbAmpl.SelectedIndex >= 0)
            {
                var txt = _cmbAmpl.SelectedItem?.ToString() ?? string.Empty;
                int eq = txt.IndexOf('=');
                if (eq >= 0 && double.TryParse(txt.Substring(eq + 1), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)) ct = parsed;
            }
            if (ct <= 0) ct = _defaultCt;
            _entrada.Ct = ct;
            double zTop = Diafragmas.DiafragmaPorPiso.Values.Where(d => d.Valido).Select(d => d.Zprom).DefaultIfEmpty(0.0).Max();
            double T = (ct > 0.0 && zTop > 0.0) ? (zTop / ct) : 0.0;
            _entrada.T = T;
            _lblT.Text = "T = " + T.ToString("G4", CultureInfo.InvariantCulture);
        }
        public void SetAlturaMax(double zTop)
        {
            CalcularT();
            CALCULAR_C();
            ValoresActualesChanged?.Invoke(this, EventArgs.Empty); // disparar evento
        }
    }
}
