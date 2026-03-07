using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using App.WinForms.UserControls.E030;

namespace App.WinForms.UserControls.Espectro
{
    /// <summary>
    /// Spectrum point data for one period value.
    /// </summary>
    public class PuntoEspectro
    {
        public double Periodo { get; set; }
        public double C { get; set; }
        public double Sa { get; set; }
        public double Sv { get; set; }
        public double Sd { get; set; }
    }

    /// <summary>
    /// Display unit system for spectrum values.
    /// </summary>
    public enum UnidadDisplay
    {
        /// <summary>Sa in cm/s², Sv in cm/s, Sd in cm (default).</summary>
        Cm,
        /// <summary>Sa in m/s², Sv in m/s, Sd in m.</summary>
        M,
        /// <summary>Sa as fraction of g (9.81 m/s²), Sv in m/s, Sd in m.</summary>
        G
    }

    public partial class EspectroUserControl : UserControl
    {
        // ── Conversion constants ─────────────────────────────────────────────
        private const double CmToM = 100.0;
        private const double CmS2ToG = 981.0;

        private Func<IReadOnlyDictionary<string, double>>? _valProvider;
        private bool _built;
        private bool _pivotMode;

        // Direction / unit state
        private bool _dirX = true;
        private UnidadDisplay _unidad = UnidadDisplay.Cm;

        // Controls
        private DataGridView _gridParams = null!;
        private DataGridView _gridEspectro = null!;
        private ToolStrip _toolbar = null!;
        private ToolStripButton _btnToggleMode = null!;
        private ToolStripLabel _lblTmaxVal = null!;
        private NumericUpDown _numDt = null!;
        private TrackBar _tbTmax = null!;
        private BindingSource _bsEspectro = null!;
        private PictureBox _pbSa = null!, _pbSv = null!, _pbSd = null!;
        /// <summary>Base (unreduced) spectrum; also selected-direction display list.</summary>
        private IList<PuntoEspectro>? _ultimaLista;
        /// <summary>Spectrum divided by R_x (cm-unit values).</summary>
        private IList<PuntoEspectro>? _ultimaListaX;
        /// <summary>Spectrum divided by R_y (cm-unit values).</summary>
        private IList<PuntoEspectro>? _ultimaListaY;
        private SplitContainer _espectroSplit = null!;

        // Direction / unit toolbar items
        private ToolStripButton _btnDirX = null!;
        private ToolStripButton _btnDirY = null!;
        private ToolStripComboBox _cmbUnidad = null!;

        public EspectroUserControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the parameter provider (from E030UserControl.GetValoresActuales).
        /// Must be called before Build().
        /// </summary>
        public void SetParameterProvider(Func<IReadOnlyDictionary<string, double>> provider)
        {
            _valProvider = provider;
        }

        /// <summary>
        /// Builds the UI. Call after SetParameterProvider.
        /// </summary>
        public void Build()
        {
            if (_built) return;
            _built = true;

            this.Controls.Clear();
            this.BackColor = Color.WhiteSmoke;

            _bsEspectro = new BindingSource();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4,
                BackColor = Color.WhiteSmoke, Padding = new Padding(6)
            };
            layout.RowStyles.Clear();
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1f));
            this.Controls.Add(layout);

            // ── Row 0: Toolbar ───────────────────────────────────────────────
            _toolbar = new ToolStrip
            {
                GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke, ImageScalingSize = new Size(20, 20)
            };

            var btnRefresh = new ToolStripButton("Refrescar Parámetros");
            btnRefresh.Click += (s, e) => { Refresh(); RefreshSpectrum(); };
            _toolbar.Items.Add(btnRefresh);

            _btnToggleMode = new ToolStripButton("Vista: Lista");
            _btnToggleMode.Click += (s, e) =>
            {
                _pivotMode = !_pivotMode;
                _btnToggleMode.Text = _pivotMode ? "Vista: Pivot" : "Vista: Lista";
                Refresh();
                AjustarAnchoPanelEspectro();
            };
            _toolbar.Items.Add(_btnToggleMode);

            var btnCopy = new ToolStripButton("Copiar Parámetros");
            btnCopy.Click += (s, e) => CopyToClipboard();
            _toolbar.Items.Add(btnCopy);

            _toolbar.Items.Add(new ToolStripSeparator());

            _toolbar.Items.Add(new ToolStripLabel("dt:"));
            _numDt = new NumericUpDown
            {
                Minimum = 0.001M, Maximum = 1M, DecimalPlaces = 3,
                Increment = 0.01M, Value = 0.01M, Width = 60
            };
            _numDt.ValueChanged += (s, e) => { RefreshSpectrum(); AjustarAnchoPanelEspectro(); };
            _toolbar.Items.Add(new ToolStripControlHost(_numDt) { AutoSize = false, Width = 60 });

            _toolbar.Items.Add(new ToolStripLabel("Tmax:"));
            _tbTmax = new TrackBar
            {
                Minimum = 10, Maximum = 3000, TickFrequency = 100,
                SmallChange = 10, LargeChange = 100,
                Value = 1500, AutoSize = false, Width = 180
            };
            _tbTmax.Scroll += (s, e) => { UpdateTmaxLabel(); RefreshSpectrum(); };
            _tbTmax.ValueChanged += (s, e) => UpdateTmaxLabel();
            _toolbar.Items.Add(new ToolStripControlHost(_tbTmax) { AutoSize = false, Width = 180 });

            _lblTmaxVal = new ToolStripLabel();
            UpdateTmaxLabel();
            _toolbar.Items.Add(_lblTmaxVal);

            var btnRefreshEspectro = new Button { Text = "Actualizar Espectro", AutoSize = true };
            btnRefreshEspectro.Click += (s, e) => { RefreshSpectrum(); AjustarAnchoPanelEspectro(); };
            _toolbar.Items.Add(new ToolStripControlHost(btnRefreshEspectro));

            // ── X / Y direction switch ────────────────────────────────────────
            _toolbar.Items.Add(new ToolStripSeparator());
            _toolbar.Items.Add(new ToolStripLabel("Dirección:"));

            _btnDirX = new ToolStripButton("X") { Checked = true, CheckOnClick = false, ToolTipText = "Mostrar espectro dirección X" };
            _btnDirX.Click += (s, e) => SetDirection(true);
            _toolbar.Items.Add(_btnDirX);

            _btnDirY = new ToolStripButton("Y") { Checked = false, CheckOnClick = false, ToolTipText = "Mostrar espectro dirección Y" };
            _btnDirY.Click += (s, e) => SetDirection(false);
            _toolbar.Items.Add(_btnDirY);

            // ── Unit selector ─────────────────────────────────────────────────
            _toolbar.Items.Add(new ToolStripSeparator());
            _toolbar.Items.Add(new ToolStripLabel("Unidades:"));

            _cmbUnidad = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130, ToolTipText = "Seleccionar sistema de unidades" };
            _cmbUnidad.Items.AddRange(new object[] { "cm/s², cm/s, cm", "m/s², m/s, m", "g (fracción de gravedad)" });
            _cmbUnidad.SelectedIndex = 0;
            _cmbUnidad.SelectedIndexChanged += (s, e) => { _unidad = (UnidadDisplay)_cmbUnidad.SelectedIndex; ReconstruirColumnasYRefrescar(); };
            _toolbar.Items.Add(_cmbUnidad);

            layout.Controls.Add(_toolbar, 0, 0);

            // ── Row 1: Parameter grid (horizontal) ───────────────────────────
            _gridParams = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                AutoGenerateColumns = true, RowHeadersVisible = false,
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
            };
            layout.Controls.Add(_gridParams, 0, 1);

            // ── Row 2: Spectrum split (grid + charts) ────────────────────────
            _espectroSplit = new SplitContainer
            {
                Dock = DockStyle.Fill, Orientation = Orientation.Vertical,
                SplitterDistance = 220, BackColor = Color.WhiteSmoke
            };
            _espectroSplit.SizeChanged += (s, e) => AjustarAnchoPanelEspectro();
            layout.Controls.Add(_espectroSplit, 0, 2);

            // Spectrum data grid
            _gridEspectro = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                AutoGenerateColumns = false, RowHeadersVisible = false,
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
            };
            _espectroSplit.Panel1.Controls.Add(_gridEspectro);

            // Charts panel (3 charts stacked vertically)
            var graphsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Color.White
            };
            graphsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33f));
            graphsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 34f));
            graphsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33f));
            _espectroSplit.Panel2.Controls.Add(graphsPanel);

            _pbSa = CreateGraphBox("Sa/R  (cm/s²)", Color.RoyalBlue);
            graphsPanel.Controls.Add(_pbSa, 0, 0);
            _pbSv = CreateGraphBox("Sv/R  (cm/s)", Color.Firebrick);
            graphsPanel.Controls.Add(_pbSv, 0, 1);
            _pbSd = CreateGraphBox("Sd/R  (cm)", Color.ForestGreen);
            graphsPanel.Controls.Add(_pbSd, 0, 2);

            BuildColumnsEspectro();
            Refresh();
            RefreshSpectrum();
            AjustarAnchoPanelEspectro();
        }

        // ── Public refresh ──────────────────────────────────────────────────
        public new void Refresh()
        {
            if (!_built) return;
            var dict = GetValues();
            RebuildParametrosHorizontal(dict);
            AjustarAnchoPanelEspectro();
        }

        // ── Spectrum generation ─────────────────────────────────────────────
        private void RefreshSpectrum()
        {
            if (!_built) return;
            double dt = (double)_numDt.Value;
            double Tmax = _tbTmax != null ? _tbTmax.Value / 100.0 : 15.0;
            var dict = GetValues();
            dict.TryGetValue("Z", out double Z);
            dict.TryGetValue("U", out double U);
            dict.TryGetValue("S", out double S);
            dict.TryGetValue("TP", out double TP);
            dict.TryGetValue("TL", out double TL);
            dict.TryGetValue("R_x", out double Rx);
            dict.TryGetValue("R_y", out double Ry);
            if (Rx <= 0) Rx = 1.0;
            if (Ry <= 0) Ry = 1.0;

            double ZUCS = Z * U * S;
            var listaX = new List<PuntoEspectro>();
            var listaY = new List<PuntoEspectro>();
            for (double p = 0.0; p <= Tmax + 1e-9; p += dt)
            {
                double C = E030Tables.CalcularC(p, TP, TL);
                double Sa = ZUCS * C * 981.0;
                double Sv = 0.0, Sd = 0.0;
                if (p > 0.0)
                {
                    double w = 2.0 * Math.PI / p;
                    Sv = Sa / w;
                    Sd = Sa / (w * w);
                }
                listaX.Add(new PuntoEspectro { Periodo = p, C = C, Sa = Sa / Rx, Sv = Sv / Rx, Sd = Sd / Rx });
                listaY.Add(new PuntoEspectro { Periodo = p, C = C, Sa = Sa / Ry, Sv = Sv / Ry, Sd = Sd / Ry });
            }

            _ultimaListaX = listaX;
            _ultimaListaY = listaY;
            ActualizarTablaYGraficas();
        }

        /// <summary>
        /// Refreshes the data table for the currently selected direction and repaints the charts.
        /// </summary>
        private void ActualizarTablaYGraficas()
        {
            _ultimaLista = _dirX ? _ultimaListaX : _ultimaListaY;
            if (_ultimaLista != null)
                _bsEspectro.DataSource = ConstruirListaDisplay(_ultimaLista);
            _pbSa?.Invalidate();
            _pbSv?.Invalidate();
            _pbSd?.Invalidate();
        }

        /// <summary>Sets the active direction and updates the table and charts.</summary>
        private void SetDirection(bool isX)
        {
            _dirX = isX;
            _btnDirX.Checked = isX;
            _btnDirY.Checked = !isX;
            ActualizarTablaYGraficas();
        }

        /// <summary>
        /// Rebuilds column headers with the current unit labels and refreshes the table.
        /// </summary>
        private void ReconstruirColumnasYRefrescar()
        {
            BuildColumnsEspectro();
            ActualizarTablaYGraficas();
            ActualizarTitulosGraficas();
        }

        /// <summary>
        /// Updates chart title text to match the selected unit.
        /// </summary>
        private void ActualizarTitulosGraficas()
        {
            if (_pbSa != null) _pbSa.Tag = new Tuple<string, Color>($"Sa/R  ({GetSaUnitLabel()})", Color.RoyalBlue);
            if (_pbSv != null) _pbSv.Tag = new Tuple<string, Color>($"Sv/R  ({GetSvUnitLabel()})", Color.Firebrick);
            if (_pbSd != null) _pbSd.Tag = new Tuple<string, Color>($"Sd/R  ({GetSdUnitLabel()})", Color.ForestGreen);
            _pbSa?.Invalidate();
            _pbSv?.Invalidate();
            _pbSd?.Invalidate();
        }

        // ── Unit label helpers ───────────────────────────────────────────────
        private string GetSaUnitLabel() =>
            _unidad == UnidadDisplay.M ? "m/s²" : _unidad == UnidadDisplay.G ? "g" : "cm/s²";

        private string GetSvUnitLabel() => _unidad == UnidadDisplay.Cm ? "cm/s" : "m/s";

        private string GetSdUnitLabel() => _unidad == UnidadDisplay.Cm ? "cm" : "m";

        /// <summary>
        /// Converts a list of cm-unit spectrum points to the currently selected display unit.
        /// </summary>
        private List<PuntoEspectro> ConstruirListaDisplay(IList<PuntoEspectro> source)
        {
            var result = new List<PuntoEspectro>(source.Count);
            foreach (var p in source)
            {
                result.Add(new PuntoEspectro
                {
                    Periodo = p.Periodo,
                    C = p.C,
                    Sa = ConvertirSa(p.Sa),
                    Sv = ConvertirSvSd(p.Sv),
                    Sd = ConvertirSvSd(p.Sd)
                });
            }
            return result;
        }

        private double ConvertirSa(double val) =>
            _unidad == UnidadDisplay.M ? val / CmToM :
            _unidad == UnidadDisplay.G ? val / CmS2ToG : val;

        private double ConvertirSvSd(double val) =>
            _unidad == UnidadDisplay.Cm ? val : val / CmToM;

        // ── Static spectrum generation (for external use) ───────────────────
        public static List<PuntoEspectro> GenerarEspectro(double ZUCS, double TP, double TL, double dt = 0.01)
        {
            const double g = 981.0;
            var lista = new List<PuntoEspectro>();
            for (double p = 0.0; p <= 20.0 + 1e-9; p += dt)
            {
                double C = E030Tables.CalcularC(p, TP, TL);
                double Sa = ZUCS * C * g;
                double Sv = 0.0, Sd = 0.0;
                if (p > 0.0)
                {
                    double w = 2.0 * Math.PI / p;
                    Sv = Sa / w;
                    Sd = Sa / (w * w);
                }
                lista.Add(new PuntoEspectro { Periodo = p, C = C, Sa = Sa, Sv = Sv, Sd = Sd });
            }
            return lista;
        }

        // ── Internal helpers ────────────────────────────────────────────────
        private IReadOnlyDictionary<string, double> GetValues()
        {
            return _valProvider?.Invoke() ?? new Dictionary<string, double>();
        }

        private void UpdateTmaxLabel()
        {
            if (_lblTmaxVal != null && _tbTmax != null)
                _lblTmaxVal.Text = $"{_tbTmax.Value / 100.0:F2} s";
        }

        private void BuildColumnsEspectro()
        {
            string saHdr = $"Sa/R ({GetSaUnitLabel()})";
            string svHdr = $"Sv/R ({GetSvUnitLabel()})";
            string sdHdr = $"Sd/R ({GetSdUnitLabel()})";

            _gridEspectro.Columns.Clear();
            var colT = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PuntoEspectro.Periodo),
                HeaderText = "T (s)", Width = 55,
                DefaultCellStyle = { Format = "F3" }
            };
            var colC = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PuntoEspectro.C),
                HeaderText = "C", Width = 55,
                DefaultCellStyle = { Format = "F4" }
            };
            var colSa = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PuntoEspectro.Sa),
                HeaderText = saHdr, Width = 95,
                DefaultCellStyle = { Format = "F4" }
            };
            var colSv = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PuntoEspectro.Sv),
                HeaderText = svHdr, Width = 90,
                DefaultCellStyle = { Format = "F4" }
            };
            var colSd = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PuntoEspectro.Sd),
                HeaderText = sdHdr, Width = 85,
                DefaultCellStyle = { Format = "F5" }
            };
            _gridEspectro.Columns.AddRange(new DataGridViewColumn[] { colT, colC, colSa, colSv, colSd });
            _gridEspectro.DataSource = _bsEspectro;
        }

        private void AjustarAnchoPanelEspectro()
        {
            if (_espectroSplit == null || _gridEspectro == null) return;
            int totalCols = _gridEspectro.Columns.Cast<DataGridViewColumn>().Sum(c => c.Width);
            int extra = _gridEspectro.RowHeadersVisible ? _gridEspectro.RowHeadersWidth : 0;
            int panel1Deseado = totalCols + extra;
            _espectroSplit.SplitterDistance = Math.Max(panel1Deseado, 100);
        }

        private void RebuildParametrosHorizontal(IReadOnlyDictionary<string, double> dict)
        {
            _gridParams.Columns.Clear();
            if (!_pivotMode)
            {
                var ordered = dict.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase).ToList();
                var dt = new DataTable();
                foreach (var kv in ordered)
                    dt.Columns.Add(new DataColumn(kv.Key, typeof(string)));
                var row = dt.NewRow();
                foreach (var kv in ordered)
                    row[kv.Key] = kv.Value.ToString("G6", CultureInfo.InvariantCulture);
                dt.Rows.Add(row);
                _gridParams.DataSource = dt;
                foreach (DataGridViewColumn c in _gridParams.Columns) c.Width = 55;
            }
            else
            {
                var pivot = BuildPivot(dict);
                var bases = pivot.Select(p => p.BaseNombre).OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
                var dt = new DataTable();
                dt.Columns.Add("Eje", typeof(string));
                foreach (var b in bases) dt.Columns.Add(b, typeof(string));
                var filaX = dt.NewRow(); filaX["Eje"] = "X";
                var filaY = dt.NewRow(); filaY["Eje"] = "Y";
                foreach (var p in pivot)
                {
                    filaX[p.BaseNombre] = p.ValorX ?? "";
                    filaY[p.BaseNombre] = p.ValorY ?? "";
                }
                dt.Rows.Add(filaX);
                dt.Rows.Add(filaY);
                _gridParams.DataSource = dt;
                foreach (DataGridViewColumn c in _gridParams.Columns) c.Width = 55;
            }
        }

        private List<PivotRow> BuildPivot(IReadOnlyDictionary<string, double> dict)
        {
            var grouped = new Dictionary<string, PivotRow>(StringComparer.OrdinalIgnoreCase);
            var basesConSufijos = new HashSet<string>(
                dict.Keys
                    .Where(k => k.EndsWith("_x", StringComparison.OrdinalIgnoreCase) ||
                                k.EndsWith("_y", StringComparison.OrdinalIgnoreCase))
                    .Select(k => k.Substring(0, k.Length - 2)),
                StringComparer.OrdinalIgnoreCase);
            var excluirBaseSiSufijo = new HashSet<string>(
                new[] { "R", "Ro", "Ip" }, StringComparer.OrdinalIgnoreCase);

            foreach (var kv in dict)
            {
                string key = kv.Key;
                if (key.EndsWith("_x", StringComparison.OrdinalIgnoreCase))
                {
                    string baseName = key.Substring(0, key.Length - 2);
                    if (!grouped.TryGetValue(baseName, out var pr))
                    { pr = new PivotRow { BaseNombre = baseName }; grouped[baseName] = pr; }
                    pr.ValorX = kv.Value.ToString("G6", CultureInfo.InvariantCulture);
                }
                else if (key.EndsWith("_y", StringComparison.OrdinalIgnoreCase))
                {
                    string baseName = key.Substring(0, key.Length - 2);
                    if (!grouped.TryGetValue(baseName, out var pr))
                    { pr = new PivotRow { BaseNombre = baseName }; grouped[baseName] = pr; }
                    pr.ValorY = kv.Value.ToString("G6", CultureInfo.InvariantCulture);
                }
                else
                {
                    if (basesConSufijos.Contains(key) && excluirBaseSiSufijo.Contains(key)) continue;
                    if (!grouped.TryGetValue(key, out var pr))
                    {
                        pr = new PivotRow
                        {
                            BaseNombre = key,
                            ValorX = kv.Value.ToString("G6", CultureInfo.InvariantCulture)
                        };
                        grouped[key] = pr;
                    }
                }
            }
            return grouped.Values.OrderBy(r => r.BaseNombre).ToList();
        }

        private void CopyToClipboard()
        {
            if (_gridParams?.DataSource == null) return;
            var dt = _gridParams.DataSource as DataTable;
            if (dt == null) return;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Join("\t", dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));
            foreach (DataRow r in dt.Rows)
                sb.AppendLine(string.Join("\t", dt.Columns.Cast<DataColumn>().Select(c => Convert.ToString(r[c]))));
            try { Clipboard.SetText(sb.ToString()); } catch { /* clipboard access may fail */ }
        }

        // ── Chart rendering ─────────────────────────────────────────────────
        private PictureBox CreateGraphBox(string titulo, Color lineColor)
        {
            var pb = new PictureBox
            {
                Dock = DockStyle.Fill, BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Tag = new Tuple<string, Color>(titulo, lineColor)
            };
            pb.Paint += (s, e) => DibujarCurva(pb, e.Graphics);
            return pb;
        }

        private void DibujarCurva(PictureBox pb, Graphics g)
        {
            var info = (Tuple<string, Color>)pb.Tag;
            g.Clear(Color.White);

            using (var fontTitle = new Font("Segoe UI", 8f, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(50, 50, 80)))
                g.DrawString(info.Item1, fontTitle, brush, new PointF(4, 4));

            if ((_ultimaListaX == null || _ultimaListaX.Count == 0) &&
                (_ultimaListaY == null || _ultimaListaY.Count == 0)) return;

            // Selector (raw cm values): convert during paint via factor
            Func<PuntoEspectro, double> rawSel;
            double convFactor;
            if (pb == _pbSa)
            {
                rawSel = x => x.Sa;
                convFactor = _unidad == UnidadDisplay.M ? 1.0 / CmToM :
                             _unidad == UnidadDisplay.G ? 1.0 / CmS2ToG : 1.0;
            }
            else if (pb == _pbSv)
            {
                rawSel = x => x.Sv;
                convFactor = _unidad == UnidadDisplay.Cm ? 1.0 : 1.0 / CmToM;
            }
            else
            {
                rawSel = x => x.Sd;
                convFactor = _unidad == UnidadDisplay.Cm ? 1.0 : 1.0 / CmToM;
            }

            Func<PuntoEspectro, double> sel = p => rawSel(p) * convFactor;

            // Determine joint max over both curves for consistent Y-axis scale
            double maxY = 0;
            if (_ultimaListaX != null) maxY = Math.Max(maxY, _ultimaListaX.Select(sel).DefaultIfEmpty(0.0).Max());
            if (_ultimaListaY != null) maxY = Math.Max(maxY, _ultimaListaY.Select(sel).DefaultIfEmpty(0.0).Max());
            if (maxY <= 0.0) return;

            double maxT = 0;
            if (_ultimaListaX != null) maxT = Math.Max(maxT, _ultimaListaX.Select(x => x.Periodo).DefaultIfEmpty(0.0).Max());
            if (_ultimaListaY != null) maxT = Math.Max(maxT, _ultimaListaY.Select(x => x.Periodo).DefaultIfEmpty(0.0).Max());
            if (maxT <= 0.0) return;

            var rect = pb.ClientRectangle;
            var plotRect = new Rectangle(52, 22, Math.Max(rect.Width - 65, 20), Math.Max(rect.Height - 44, 20));
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int segundos = (int)Math.Ceiling(maxT);
            if (segundos < 1) segundos = 1;
            int yDivs = 5;

            using (var gridPen = new Pen(Color.Gainsboro, 1))
            using (var axisPen = new Pen(Color.FromArgb(60, 60, 80), 1.5f))
            using (var fontAxis = new Font("Segoe UI", 6.5f))
            using (var brushAxis = new SolidBrush(Color.FromArgb(60, 60, 80)))
            {
                for (int s = 0; s <= segundos; s++)
                {
                    float x = (float)(plotRect.Left + (s / (double)segundos) * plotRect.Width);
                    g.DrawLine(gridPen, x, plotRect.Top, x, plotRect.Bottom);
                    g.DrawString(s.ToString(), fontAxis, brushAxis, new PointF(x - 6, plotRect.Bottom + 2));
                }
                for (int i = 0; i <= yDivs; i++)
                {
                    double val = maxY * i / yDivs;
                    float y = (float)(plotRect.Bottom - (val / maxY) * plotRect.Height);
                    g.DrawLine(gridPen, plotRect.Left, y, plotRect.Right, y);
                    g.DrawString(val.ToString("G4"), fontAxis, brushAxis, new PointF(1, y - 7));
                }
                g.DrawLine(axisPen, plotRect.Left, plotRect.Bottom, plotRect.Right, plotRect.Bottom);
                g.DrawLine(axisPen, plotRect.Left, plotRect.Bottom, plotRect.Left, plotRect.Top);
            }

            // Draw Y-direction curve (orange, dashed) behind X
            if (_ultimaListaY != null && _ultimaListaY.Count >= 2)
            {
                using var penY = new Pen(Color.DarkOrange, 1.5f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
                PointF? prev = null;
                foreach (var p in _ultimaListaY)
                {
                    float x = (float)(plotRect.Left + (p.Periodo / maxT) * plotRect.Width);
                    float y = (float)(plotRect.Bottom - (sel(p) / maxY) * plotRect.Height);
                    var pt = new PointF(x, y);
                    if (prev != null) g.DrawLine(penY, prev.Value, pt);
                    prev = pt;
                }
            }

            // Draw X-direction curve (primary color, solid) on top
            if (_ultimaListaX != null && _ultimaListaX.Count >= 2)
            {
                using var penX = new Pen(info.Item2, 1.8f);
                PointF? prev = null;
                foreach (var p in _ultimaListaX)
                {
                    float x = (float)(plotRect.Left + (p.Periodo / maxT) * plotRect.Width);
                    float y = (float)(plotRect.Bottom - (sel(p) / maxY) * plotRect.Height);
                    var pt = new PointF(x, y);
                    if (prev != null) g.DrawLine(penX, prev.Value, pt);
                    prev = pt;
                }
            }

            // Legend (top-right)
            using (var fontLeg = new Font("Segoe UI", 7f))
            using (var brushLeg = new SolidBrush(Color.FromArgb(50, 50, 80)))
            {
                int lx = plotRect.Right - 58;
                int ly = plotRect.Top + 4;
                g.FillRectangle(new SolidBrush(Color.FromArgb(230, Color.White)), lx - 2, ly - 2, 60, 30);
                using var penX2 = new Pen(info.Item2, 2f);
                g.DrawLine(penX2, lx, ly + 6, lx + 16, ly + 6);
                g.DrawString("X", fontLeg, brushLeg, lx + 18, ly + 1);
                using var penY2 = new Pen(Color.DarkOrange, 2f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
                g.DrawLine(penY2, lx, ly + 20, lx + 16, ly + 20);
                g.DrawString("Y", fontLeg, brushLeg, lx + 18, ly + 15);
            }
        }

        private class PivotRow
        {
            public string BaseNombre { get; set; } = "";
            public string? ValorX { get; set; }
            public string? ValorY { get; set; }
        }
    }
}
