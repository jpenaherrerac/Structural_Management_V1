using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace App.WinForms.UserControls.E030
{
    public partial class E030UserControl : UserControl
    {
        // ── Events ──────────────────────────────────────────────────────────
        public event EventHandler? ValoresActualesChanged;

        // ── Internal state ──────────────────────────────────────────────────
        private readonly EntradaE030 _entrada = new EntradaE030();
        private readonly EntradaE030 _entradaY = new EntradaE030();
        private readonly Dictionary<string, double> _valores =
            new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        // ── Shared controls ─────────────────────────────────────────────────
        private ComboBox _cmbZona = null!;
        private ComboBox _cmbSuelo = null!;
        private ComboBox _cmbAmpl = null!;
        private ComboBox _cmbCategoria = null!;
        private NumericUpDown _nudAltura = null!;
        private Label _lblZ = null!, _lblS = null!, _lblTP = null!, _lblTL = null!;
        private Label _lblC = null!, _lblT = null!, _lblU = null!;
        private Label _lblSueloMsg = null!;

        // ── Direction X controls ────────────────────────────────────────────
        private readonly Dictionary<SistemaEstructural, RadioButton> _radioMap =
            new Dictionary<SistemaEstructural, RadioButton>();
        private TextBox _txtRo = null!, _txtIa = null!, _txtIp = null!, _txtR = null!;
        private readonly List<CheckBox> _chkIa = new List<CheckBox>();
        private readonly List<CheckBox> _chkIp = new List<CheckBox>();
        private bool _suppressRadioX;

        // ── Direction Y controls ────────────────────────────────────────────
        private readonly Dictionary<SistemaEstructural, RadioButton> _radioMapY =
            new Dictionary<SistemaEstructural, RadioButton>();
        private TextBox _txtRoY = null!, _txtIaY = null!, _txtIpY = null!, _txtRY = null!;
        private readonly List<CheckBox> _chkIaY = new List<CheckBox>();
        private readonly List<CheckBox> _chkIpY = new List<CheckBox>();
        private bool _suppressRadioY;

        public E030UserControl()
        {
            InitializeComponent();
        }

        // ── Public API ──────────────────────────────────────────────────────
        public IReadOnlyDictionary<string, double> GetValoresActuales() => _valores;

        public void SetBuildingHeight(double height)
        {
            if (height >= (double)_nudAltura.Minimum && height <= (double)_nudAltura.Maximum)
                _nudAltura.Value = (decimal)height;
        }

        // ── Structural system radio – direction X ───────────────────────────
        private void AddRadioX(FlowLayoutPanel parent, string text, SistemaEstructural sistema)
        {
            var rb = new RadioButton
            {
                Text = text, AutoSize = true, Tag = sistema,
                Font = new Font(SystemFonts.DefaultFont.FontFamily, 8.5f),
                Margin = new Padding(4, 2, 4, 2)
            };
            rb.CheckedChanged += (s, e) =>
            {
                if (!rb.Checked || _suppressRadioX) return;
                try
                {
                    _suppressRadioX = true;
                    foreach (var kv in _radioMap)
                        if (!ReferenceEquals(kv.Value, rb) && kv.Value.Checked)
                            kv.Value.Checked = false;

                    _entrada.Sistema = sistema;
                    _entrada.R0 = E030Tables.GetR0(sistema);
                    if (_txtRo != null) _txtRo.Text = _entrada.R0.ToString("G", CultureInfo.InvariantCulture);
                    UpdateRX();
                }
                finally { _suppressRadioX = false; }
            };
            parent.Controls.Add(rb);
            _radioMap[sistema] = rb;
        }

        // ── Structural system radio – direction Y ───────────────────────────
        private void AddRadioY(FlowLayoutPanel parent, string text, SistemaEstructural sistema)
        {
            var rb = new RadioButton
            {
                Text = text, AutoSize = true, Tag = sistema,
                Font = new Font(SystemFonts.DefaultFont.FontFamily, 8.5f),
                Margin = new Padding(4, 2, 4, 2)
            };
            rb.CheckedChanged += (s, e) =>
            {
                if (!rb.Checked || _suppressRadioY) return;
                try
                {
                    _suppressRadioY = true;
                    foreach (var kv in _radioMapY)
                        if (!ReferenceEquals(kv.Value, rb) && kv.Value.Checked)
                            kv.Value.Checked = false;

                    _entradaY.Sistema = sistema;
                    _entradaY.R0 = E030Tables.GetR0(sistema);
                    if (_txtRoY != null) _txtRoY.Text = _entradaY.R0.ToString("G", CultureInfo.InvariantCulture);
                    UpdateRY();
                }
                finally { _suppressRadioY = false; }
            };
            parent.Controls.Add(rb);
            _radioMapY[sistema] = rb;
        }

        // ── Recalculation methods ───────────────────────────────────────────
        private void RecalcSuelo()
        {
            if (_entrada.Zona.HasValue)
                _lblZ.Text = "Z = " + E030Tables.GetZFactor(_entrada.Zona.Value).ToString("G", CultureInfo.InvariantCulture);

            if (_entrada.Suelo.HasValue)
                _lblSueloMsg.Text = E030Tables.GetSoilDescription(_entrada.Suelo.Value);

            if (_entrada.Zona.HasValue && _entrada.Suelo.HasValue)
            {
                double sVal = E030Tables.GetS(_entrada.Zona.Value, _entrada.Suelo.Value);
                _lblS.Text = "S = " + sVal.ToString("G", CultureInfo.InvariantCulture);
                var (tp, tl) = E030Tables.GetPeriodos(_entrada.Suelo.Value);
                _lblTP.Text = "TP = " + tp.ToString("G", CultureInfo.InvariantCulture);
                _lblTL.Text = "TL = " + tl.ToString("G", CultureInfo.InvariantCulture);
            }
        }

        private void RecalcIrregularidades()
        {
            var iaVals = _chkIa.Where(cb => cb.Checked).Select(cb => ((IrregularItem)cb.Tag).Factor).ToList();
            var ipVals = _chkIp.Where(cb => cb.Checked).Select(cb => ((IrregularItem)cb.Tag).Factor).ToList();
            _entrada.Ia = iaVals.Count > 0 ? iaVals.Min() : 1.0;
            _entrada.Ip = ipVals.Count > 0 ? ipVals.Min() : 1.0;
            if (_txtIa != null) _txtIa.Text = _entrada.Ia.ToString("G", CultureInfo.InvariantCulture);
            if (_txtIp != null) _txtIp.Text = _entrada.Ip.ToString("G", CultureInfo.InvariantCulture);
        }

        private void RecalcIrregularidadesY()
        {
            var iaVals = _chkIaY.Where(cb => cb.Checked).Select(cb => ((IrregularItem)cb.Tag).Factor).ToList();
            var ipVals = _chkIpY.Where(cb => cb.Checked).Select(cb => ((IrregularItem)cb.Tag).Factor).ToList();
            _entradaY.Ia = iaVals.Count > 0 ? iaVals.Min() : 1.0;
            _entradaY.Ip = ipVals.Count > 0 ? ipVals.Min() : 1.0;
            if (_txtIaY != null) _txtIaY.Text = _entradaY.Ia.ToString("G", CultureInfo.InvariantCulture);
            if (_txtIpY != null) _txtIpY.Text = _entradaY.Ip.ToString("G", CultureInfo.InvariantCulture);
        }

        private void UpdateRX()
        {
            if (_txtR != null) _txtR.Text = _entrada.R.ToString("G", CultureInfo.InvariantCulture);
        }

        private void UpdateRY()
        {
            if (_txtRY != null) _txtRY.Text = _entradaY.R.ToString("G", CultureInfo.InvariantCulture);
        }

        private void CalcularT()
        {
            double ct = 0.0;
            if (_cmbAmpl != null && _cmbAmpl.SelectedIndex >= 0)
            {
                var txt = _cmbAmpl.SelectedItem?.ToString() ?? "";
                int eq = txt.IndexOf('=');
                if (eq >= 0 && double.TryParse(txt.Substring(eq + 1),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                    ct = parsed;
            }
            if (ct <= 0) ct = 35.0;
            _entrada.Ct = ct;

            double hn = (double)_nudAltura.Value;
            double T = (ct > 0 && hn > 0) ? (hn / ct) : 0.0;
            _entrada.T = T;
            if (_lblT != null) _lblT.Text = "T = " + T.ToString("G4", CultureInfo.InvariantCulture);
        }

        private double CalcularC()
        {
            double T = _entrada.T ?? 0.0;
            double TP = 0.30, TL = 3.00;
            if (_entrada.Suelo.HasValue)
            {
                var (tp, tl) = E030Tables.GetPeriodos(_entrada.Suelo.Value);
                TP = tp; TL = tl;
            }
            double Cval = E030Tables.CalcularC(T, TP, TL);
            _entrada.C = Cval;
            if (_lblC != null) _lblC.Text = "C = " + Cval.ToString("G4", CultureInfo.InvariantCulture);
            ActualizarDiccionario(TP, TL);
            return Cval;
        }

        private void ActualizarDiccionario(double TP, double TL)
        {
            double z = _entrada.Zona.HasValue ? E030Tables.GetZFactor(_entrada.Zona.Value) : 0.0;
            double u = _entrada.U ?? 0.0;
            double s = (_entrada.Zona.HasValue && _entrada.Suelo.HasValue)
                ? E030Tables.GetS(_entrada.Zona.Value, _entrada.Suelo.Value) : 0.0;
            double t = _entrada.T ?? 0.0;

            _valores["Z"] = z;
            _valores["U"] = u;
            _valores["S"] = s;
            _valores["T"] = t;
            _valores["TP"] = TP;
            _valores["TL"] = TL;
            _valores["C"] = _entrada.C ?? 0.0;
            _valores["Ia"] = _entrada.Ia;
            _valores["R_x"] = _entrada.R;
            _valores["Ro_x"] = _entrada.R0;
            _valores["Ia_x"] = _entrada.Ia;
            _valores["Ip_x"] = _entrada.Ip;
            _valores["R_y"] = _entradaY.R;
            _valores["Ro_y"] = _entradaY.R0;
            _valores["Ia_y"] = _entradaY.Ia;
            _valores["Ip_y"] = _entradaY.Ip;
        }

        private void InternalRefresh()
        {
            RecalcSuelo();
            RecalcIrregularidades();
            RecalcIrregularidadesY();
            UpdateRX();
            UpdateRY();
            CalcularT();
            CalcularC();
            ValoresActualesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
