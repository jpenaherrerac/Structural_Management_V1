using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Analisis_estructuralv1.UI.TABS
{
    internal class ESPECTR_TapManager : IDisposable
    {
        private readonly TabPage _host;
        private readonly Func<IReadOnlyDictionary<string,double>> _valProvider;
        private bool _built;
        private DataGridView _grid;            // parámetros (ahora horizontal)
        private DataGridView _gridEspectro;    // espectro Sa/Sv/Sd
        private ToolStrip _toolbar;
        private BindingSource _bs;
        private BindingSource _bsEspectro;
        private bool _pivotMode; // modo agrupado X/Y
        private ToolStripButton _btnToggleMode;
        private NumericUpDown _numDt;
        //private NumericUpDown _numTmax; // reemplazado por barra
        private TrackBar _tbTmax;            // barra Tmax
        private ToolStripLabel _lblTmaxVal;  // muestra valor actual Tmax
        private Button _btnRefreshEspectro;
        private PictureBox _pbSa; private PictureBox _pbSv; private PictureBox _pbSd;
        private IList<PuntoEspectro> _ultimaLista;
        private SplitContainer _espectroSplit; // referencia para ajuste dinámico

        public ESPECTR_TapManager(TabPage host, Func<IReadOnlyDictionary<string,double>> valProvider)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _valProvider = valProvider ?? (() => new Dictionary<string,double>());
        }

        public void Build()
        {
            if (_built) return; _built = true;
            _host.Controls.Clear(); _host.BackColor = Color.WhiteSmoke;
            _bs = new BindingSource(); _bsEspectro = new BindingSource();
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Color.WhiteSmoke, Padding = new Padding(6) };
            layout.RowStyles.Clear();
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1f));
            _host.Controls.Add(layout);

            _toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Fill, BackColor = Color.WhiteSmoke, ImageScalingSize = new Size(20,20) };
            var btnRefresh = new ToolStripButton("Refrescar Par"); btnRefresh.Click += (s, e) => { Refresh(); RefreshSpectrum(); }; _toolbar.Items.Add(btnRefresh);
            _btnToggleMode = new ToolStripButton("Vista: Lista"); _btnToggleMode.Click += (s,e)=> { _pivotMode = !_pivotMode; _btnToggleMode.Text = _pivotMode? "Vista: Pivot" : "Vista: Lista"; Refresh(); AjustarAnchoPanelEspectro(_espectroSplit); }; _toolbar.Items.Add(_btnToggleMode);
            var btnCopy = new ToolStripButton("Copiar Par"); btnCopy.Click += (s,e)=> CopyToClipboard(); _toolbar.Items.Add(btnCopy);
            _toolbar.Items.Add(new ToolStripSeparator());
            _toolbar.Items.Add(new ToolStripLabel("dt:"));
            _numDt = new NumericUpDown { Minimum = 0.001M, Maximum = 1M, DecimalPlaces = 3, Increment = 0.01M, Value = 0.01M, Width = 60 }; _numDt.ValueChanged += (s,e)=> { RefreshSpectrum(); AjustarAnchoPanelEspectro(_espectroSplit); };
            _toolbar.Items.Add(new ToolStripControlHost(_numDt){ AutoSize = false, Width = 60 });
            _toolbar.Items.Add(new ToolStripLabel("Tmax:"));
            // TrackBar para Tmax: escala *100 (0.10 -> 10, 30.00 -> 3000) valor por defecto 15.00 -> 1500
            _tbTmax = new TrackBar
            {
                Minimum = 10,
                Maximum = 3000,
                TickFrequency = 100, // cada 1.00 s
                SmallChange = 10,
                LargeChange = 100,
                Value = 1500, // 15.00 por defecto
                AutoSize = false,
                Width = 180
            };
            _tbTmax.Scroll += (s,e)=> { ActualizarLabelTmax(); RefreshSpectrum(); };
            _tbTmax.ValueChanged += (s,e)=> { ActualizarLabelTmax(); }; // muestra sin recalcular siempre
            _toolbar.Items.Add(new ToolStripControlHost(_tbTmax){ AutoSize = false, Width = 180 });
            _lblTmaxVal = new ToolStripLabel(); ActualizarLabelTmax(); _toolbar.Items.Add(_lblTmaxVal);
            _btnRefreshEspectro = new Button{ Text = "Actualizar Espectro", AutoSize = true }; _btnRefreshEspectro.Click += (s,e)=> { RefreshSpectrum(); AjustarAnchoPanelEspectro(_espectroSplit); };
            _toolbar.Items.Add(new ToolStripControlHost(_btnRefreshEspectro));
            layout.Controls.Add(_toolbar, 0, 0);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = true,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
            };
            layout.Controls.Add(_grid, 0, 1);

            _espectroSplit = new SplitContainer{ Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 220, BackColor = Color.WhiteSmoke };
            _espectroSplit.SizeChanged += (s,e)=> AjustarAnchoPanelEspectro(_espectroSplit); // ajusta al redimensionar
            layout.Controls.Add(_espectroSplit, 0, 2);

            _gridEspectro = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
            };
            _espectroSplit.Panel1.Controls.Add(_gridEspectro);

            var graphsPanel = new TableLayoutPanel{ Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Color.White };
            graphsPanel.RowStyles.Add(new RowStyle(SizeType.Percent,33f)); graphsPanel.RowStyles.Add(new RowStyle(SizeType.Percent,34f)); graphsPanel.RowStyles.Add(new RowStyle(SizeType.Percent,33f)); _espectroSplit.Panel2.Controls.Add(graphsPanel);
            _pbSa = CreateGraphBox("Pseudo Aceleración (cm/s²)", Color.RoyalBlue); graphsPanel.Controls.Add(_pbSa,0,0);
            _pbSv = CreateGraphBox("Pseudo Velocidad (cm/s)", Color.Firebrick); graphsPanel.Controls.Add(_pbSv,0,1);
            _pbSd = CreateGraphBox("Pseudo Desplazamiento (cm)", Color.ForestGreen); graphsPanel.Controls.Add(_pbSd,0,2);

            BuildColumnsEspectro();
            Refresh();
            RefreshSpectrum();
            AjustarAnchoPanelEspectro(_espectroSplit); // ajuste inicial
        }

        private void ActualizarLabelTmax()
        {
            if(_lblTmaxVal != null && _tbTmax != null)
            {
                _lblTmaxVal.Text = ($"{_tbTmax.Value/100.0:F2} s");
            }
        }

        private int PxFromCm(double cm)
        {
            using (var g = _host.CreateGraphics())
            {
                return (int)Math.Round(g.DpiX / 2.54 * cm);
            }
        }

        private PictureBox CreateGraphBox(string titulo, Color lineColor)
        {
            var pb = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Tag = new Tuple<string,Color>(titulo,lineColor) };
            pb.Paint += (s,e)=> DibujarCurva(pb, e.Graphics);
            return pb;
        }

        private void DibujarCurva(PictureBox pb, Graphics g)
        {
            var info = (Tuple<string,Color>)pb.Tag;
            g.Clear(Color.White);
            using(var fontTitle = new Font("Segoe UI",8f))
            using(var brush = new SolidBrush(Color.Black))
            { g.DrawString(info.Item1, fontTitle, brush, new PointF(4,4)); }
            if(_ultimaLista == null || _ultimaLista.Count == 0) return;
            Func<PuntoEspectro,double> selector;
            if (pb == _pbSa)
                selector = delegate(PuntoEspectro x) { return x.Sa; };
            else if (pb == _pbSv)
                selector = delegate(PuntoEspectro x) { return x.Sv; };
            else
                selector = delegate(PuntoEspectro x) { return x.Sd; };
            double maxY = _ultimaLista.Select(selector).DefaultIfEmpty(0.0).Max();
            if (maxY <= 0.0) return;
            var rect = pb.ClientRectangle;
            var plotRect = new Rectangle(50, 20, rect.Width - 60, rect.Height - 40);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int segundos = (int)Math.Ceiling(_ultimaLista.Select(x=> x.Periodo).DefaultIfEmpty(0.0).Max());
            int yDivs = 5;
            using(var gridPen = new Pen(Color.Gainsboro,1))
            using(var axisPen = new Pen(Color.Black,1))
            using(var fontAxis = new Font("Segoe UI",7f))
            using(var brushAxis = new SolidBrush(Color.Black))
            {
                for(int s=0; s<=segundos; s++)
                {
                    float x = (float)(plotRect.Left + (s / (double)segundos) * plotRect.Width);
                    g.DrawLine(gridPen, x, plotRect.Top, x, plotRect.Bottom);
                    g.DrawString(s.ToString(), fontAxis, brushAxis, new PointF(x-8, plotRect.Bottom+2));
                }
                for(int i=0;i<=yDivs;i++)
                {
                    double val = maxY * i / yDivs;
                    float y = (float)(plotRect.Bottom - (val / maxY) * plotRect.Height);
                    g.DrawLine(gridPen, plotRect.Left, y, plotRect.Right, y);
                    g.DrawString(val.ToString("G4"), fontAxis, brushAxis, new PointF(2, y-6));
                }
                g.DrawLine(axisPen, plotRect.Left, plotRect.Bottom, plotRect.Right, plotRect.Bottom);
                g.DrawLine(axisPen, plotRect.Left, plotRect.Bottom, plotRect.Left, plotRect.Top);
            }
            using(var pen = new Pen(info.Item2,1.6f))
            {
                PointF? prev = null;
                double maxT = _ultimaLista.Select(x=> x.Periodo).DefaultIfEmpty(0.0).Max();
                if(maxT <= 0.0) return;
                foreach(var p in _ultimaLista)
                {
                    float x = (float)(plotRect.Left + (p.Periodo / maxT) * plotRect.Width);
                    float y = (float)(plotRect.Bottom - (selector(p) / maxY) * plotRect.Height);
                    var pt = new PointF(x,y);
                    if(prev != null) g.DrawLine(pen, prev.Value, pt);
                    prev = pt;
                }
            }
        }

        private void BuildColumnsEspectro()
        {
            _gridEspectro.Columns.Clear();
            int colTWidth = PxFromCm(1.0);
            int colOthersWidth = PxFromCm(2.0);
            var colT = new DataGridViewTextBoxColumn{ DataPropertyName = nameof(PuntoEspectro.Periodo), HeaderText = "T", Width = colTWidth, DefaultCellStyle = { Format = "F3" } };
            var colSa = new DataGridViewTextBoxColumn{ DataPropertyName = nameof(PuntoEspectro.Sa), HeaderText = "Sa (cm/s²)", Width = colOthersWidth, DefaultCellStyle = { Format = "F3" } };
            var colSv = new DataGridViewTextBoxColumn{ DataPropertyName = nameof(PuntoEspectro.Sv), HeaderText = "Sv (cm/s)", Width = colOthersWidth, DefaultCellStyle = { Format = "F3" } };
            var colSd = new DataGridViewTextBoxColumn{ DataPropertyName = nameof(PuntoEspectro.Sd), HeaderText = "Sd (cm)", Width = colOthersWidth, DefaultCellStyle = { Format = "F3" } };
            _gridEspectro.Columns.AddRange(new DataGridViewColumn[]{ colT, colSa, colSv, colSd });
            _gridEspectro.DataSource = _bsEspectro;
        }

        private void AjustarAnchoPanelEspectro(SplitContainer sc)
        {
            if (sc == null || _gridEspectro == null) return;
            int totalCols = _gridEspectro.Columns.Cast<DataGridViewColumn>().Sum(c => c.Width);
            int extra = (_gridEspectro.RowHeadersVisible ? _gridEspectro.RowHeadersWidth : 0) + 0; // margen interno
            int panel1Deseado = totalCols + extra;
            // Limitar panel1 a como máximo 1/3 del ancho total para que gráficos tengan el doble (≈ 2/3)
           // int maxPanel1 = (int)(sc.Width / 3.0);
           // if (panel1Deseado > maxPanel1) panel1Deseado = maxPanel1;
            sc.SplitterDistance = Math.Max(panel1Deseado, 100); // mínimo razonable
        }

        public void Refresh()
        {
            if (!_built) return;
            var dict = _valProvider() ?? new Dictionary<string,double>();
            RebuildParametrosHorizontal(dict);
            AjustarAnchoPanelEspectro(_espectroSplit);
        }

        private void RebuildParametrosHorizontal(IReadOnlyDictionary<string,double> dict)
        {
            _grid.Columns.Clear();
            int width = PxFromCm(1.0);
            if(!_pivotMode)
            {
                var ordered = dict.OrderBy(k=> k.Key, StringComparer.OrdinalIgnoreCase).ToList();
                var dt = new DataTable();
                foreach(var kv in ordered) dt.Columns.Add(new DataColumn(kv.Key, typeof(string)));
                var row = dt.NewRow();
                foreach(var kv in ordered) row[kv.Key] = kv.Value.ToString("G6", CultureInfo.InvariantCulture);
                dt.Rows.Add(row);
                _grid.DataSource = dt;
                foreach(DataGridViewColumn c in _grid.Columns) c.Width = width;
            }
            else
            {
                var pivot = BuildPivot(dict);
                var bases = pivot.Select(p=> p.BaseNombre).OrderBy(s=> s, StringComparer.OrdinalIgnoreCase).ToList();
                var dt = new DataTable();
                dt.Columns.Add("Eje", typeof(string));
                foreach(var b in bases) dt.Columns.Add(b, typeof(string));
                var filaX = dt.NewRow(); filaX["Eje"] = "X";
                var filaY = dt.NewRow(); filaY["Eje"] = "Y";
                foreach(var p in pivot)
                {
                    filaX[p.BaseNombre] = p.ValorX ?? string.Empty;
                    filaY[p.BaseNombre] = p.ValorY ?? string.Empty;
                }
                dt.Rows.Add(filaX); dt.Rows.Add(filaY);
                _grid.DataSource = dt;
                foreach(DataGridViewColumn c in _grid.Columns) c.Width = width;
            }
        }

        private void RefreshSpectrum()
        {
            if (!_built) return;
            double dt = (double)_numDt.Value; 
            double Tmax = _tbTmax != null ? _tbTmax.Value / 100.0 : 15.0; // nuevo origen valor por defecto 15
            var dict = _valProvider() ?? new Dictionary<string,double>();
            dict.TryGetValue("Z", out double Z);
            dict.TryGetValue("U", out double U);
            dict.TryGetValue("S", out double S);
            dict.TryGetValue("TP", out double TP);
            dict.TryGetValue("TL", out double TL);
            double ZUCS = Z * U * S;
            var lista = new List<PuntoEspectro>();
            for(double p=0.0; p<=Tmax+1e-9; p+=dt){ double C = C_por_tramos(p, TP, TL); double Sa = ZUCS * C * 981.0; double Sv=0.0, Sd=0.0; if(p>0.0){ double w = 2.0*Math.PI/p; Sv = Sa / w; Sd = Sa / (w*w); } lista.Add(new PuntoEspectro{ Periodo=p, C=C, Sa=Sa, Sv=Sv, Sd=Sd }); }
            _bsEspectro.DataSource = lista;
            _ultimaLista = lista;
            _pbSa?.Invalidate(); _pbSv?.Invalidate(); _pbSd?.Invalidate();
        }

        private List<PivotRow> BuildPivot(IReadOnlyDictionary<string,double> dict)
        {
            var result = new List<PivotRow>();
            var grouped = new Dictionary<string, PivotRow>(StringComparer.OrdinalIgnoreCase);
            var basesConSufijos = new HashSet<string>(dict.Keys
                .Where(k => k.EndsWith("_x", StringComparison.OrdinalIgnoreCase) || k.EndsWith("_y", StringComparison.OrdinalIgnoreCase))
                .Select(k => k.Substring(0, k.Length - 2)), StringComparer.OrdinalIgnoreCase);
            var excluirBaseSiSufijo = new HashSet<string>(new[] { "R", "Ro", "Ip" }, StringComparer.OrdinalIgnoreCase);

            foreach(var kv in dict)
            {
                string key = kv.Key;
                if (key.EndsWith("_x", StringComparison.OrdinalIgnoreCase))
                {
                    string baseName = key.Substring(0,key.Length-2);
                    if(!grouped.TryGetValue(baseName,out var pr)) { pr = new PivotRow{ BaseNombre = baseName }; grouped[baseName] = pr; }
                    pr.ValorX = kv.Value.ToString("G6", CultureInfo.InvariantCulture);
                }
                else if (key.EndsWith("_y", StringComparison.OrdinalIgnoreCase))
                {
                    string baseName = key.Substring(0,key.Length-2);
                    if(!grouped.TryGetValue(baseName,out var pr)) { pr = new PivotRow{ BaseNombre = baseName }; grouped[baseName] = pr; }
                    pr.ValorY = kv.Value.ToString("G6", CultureInfo.InvariantCulture);
                }
                else
                {
                    if (basesConSufijos.Contains(key) && excluirBaseSiSufijo.Contains(key)) continue;
                    if(!grouped.TryGetValue(key,out var pr)) { pr = new PivotRow{ BaseNombre = key, ValorX = kv.Value.ToString("G6", CultureInfo.InvariantCulture) }; grouped[key] = pr; }
                }
            }
            result.AddRange(grouped.Values.OrderBy(r => r.BaseNombre));
            return result;
        }

        private void CopyToClipboard()
        {
            if (_grid?.DataSource == null) return;
            var sb = new System.Text.StringBuilder();
            var dt = _grid.DataSource as DataTable;
            if(dt == null) return;
            sb.AppendLine(string.Join("\t", dt.Columns.Cast<DataColumn>().Select(c=> c.ColumnName)));
            foreach(DataRow r in dt.Rows)
            {
                sb.AppendLine(string.Join("\t", dt.Columns.Cast<DataColumn>().Select(c=> Convert.ToString(r[c]) )));
            }
            try { Clipboard.SetText(sb.ToString()); } catch { }
        }

        public static double CalcularC(double T, double TP, double TL)
        {
            const double Cmax = 2.5;
            if (T <= 0.0) return 0.0;
            if (T <= TP) return Cmax;
            if (T <= TL) return Cmax * TP / T;
            return Cmax * TP * TL / (T * T);
        }

        public class PuntoEspectro
        {
            public double Periodo { get; set; }
            public double C { get; set; }
            public double Sa { get; set; }
            public double Sv { get; set; }
            public double Sd { get; set; }
        }
        private static double C_por_tramos(double periodo, double TP, double TL)
        {
            const double Cmax = 2.5;
            if (periodo <= 0.0) return 0.0;
            if (periodo <= TP) return Cmax;
            if (periodo <= TL) return Cmax * TP / periodo;
            return Cmax * TP * TL / (periodo * periodo);
        }
        public static List<PuntoEspectro> GenerarEspectro(double ZUCS, double TP, double TL, double dt = 0.01)
        {
            const double g = 981.0;
            var lista = new List<PuntoEspectro>();
            for (double p = 0.0; p <= 20.0 + 1e-9; p += dt)
            {
                double C = C_por_tramos(p, TP, TL);
                double Sa = ZUCS * C * g;
                double Sv = 0.0; double Sd = 0.0;
                if (p > 0.0)
                {
                    double w = (2.0 * Math.PI) / p;
                    Sv = Sa / w;
                    Sd = Sa / (w * w);
                }
                lista.Add(new PuntoEspectro { Periodo = p, C = C, Sa = Sa, Sv = Sv, Sd = Sd });
            }
            return lista;
        }
        public List<PuntoEspectro> GenerarEspectroDesdeValores(double dt = 0.01)
        {
            var dict = _valProvider() ?? new Dictionary<string,double>();
            dict.TryGetValue("Z", out double Z);
            dict.TryGetValue("U", out double U);
            dict.TryGetValue("S", out double S);
            dict.TryGetValue("TP", out double TP);
            dict.TryGetValue("TL", out double TL);
            double ZUCS = Z * U * S;
            return GenerarEspectro(ZUCS, TP, TL, dt);
        }

        private class PivotRow
        {
            public string BaseNombre { get; set; }
            public string ValorX { get; set; }
            public string ValorY { get; set; }
        }

        public void Dispose()
        {
            _grid?.Dispose();
            _gridEspectro?.Dispose();
            _toolbar?.Dispose();
            _pbSa?.Dispose(); _pbSv?.Dispose(); _pbSd?.Dispose();
        }
    }
}
