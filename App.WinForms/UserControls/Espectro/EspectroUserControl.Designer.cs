using System.Windows.Forms;

namespace App.WinForms.UserControls.Espectro
{
    partial class EspectroUserControl
    {
        private System.ComponentModel.IContainer components = null;

        private NumericUpDown _nudZ, _nudS, _nudI, _nudR, _nudTp, _nudTl, _nudEta, _nudTMax, _nudNPoints;
        private Button _btnGenerate;
        private DataGridView _dgvSpectrum;
        private Panel _panelChart;
        private Label _lblInfo;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 3,
                RowCount = 1
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // ── Left: parameters ─────────────────────────────────────────────
            var paramPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 12,
                Padding = new Padding(4)
            };
            paramPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
            paramPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var lblHeader = new Label
            {
                Text = "Parámetros del Espectro",
                Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold),
                Dock = DockStyle.Fill, Height = 26
            };
            paramPanel.Controls.Add(lblHeader, 0, 0);
            paramPanel.SetColumnSpan(lblHeader, 2);

            _nudZ = MakeNud(0.01M, 1M, 3, 0.01M, 0.35M);
            _nudS = MakeNud(0.5M, 3M, 2, 0.01M, 1.11M);
            _nudI = MakeNud(1M, 2M, 2, 0.25M, 1.00M);
            _nudR = MakeNud(1M, 12M, 1, 0.5M, 6M);
            _nudTp = MakeNud(0.05M, 3M, 3, 0.01M, 0.30M);
            _nudTl = MakeNud(1M, 10M, 2, 0.1M, 4M);
            _nudEta = MakeNud(0.5M, 3M, 2, 0.05M, 1.80M);
            _nudTMax = MakeNud(1M, 10M, 1, 0.5M, 4M);
            _nudNPoints = MakeNud(10, 500, 0, 10, 100);

            AddParamRow(paramPanel, 1, "Z:", _nudZ);
            AddParamRow(paramPanel, 2, "S:", _nudS);
            AddParamRow(paramPanel, 3, "I:", _nudI);
            AddParamRow(paramPanel, 4, "R:", _nudR);
            AddParamRow(paramPanel, 5, "Tp:", _nudTp);
            AddParamRow(paramPanel, 6, "Tl:", _nudTl);
            AddParamRow(paramPanel, 7, "η:", _nudEta);
            AddParamRow(paramPanel, 8, "T_max:", _nudTMax);
            AddParamRow(paramPanel, 9, "Puntos:", _nudNPoints);

            _btnGenerate = new Button { Text = "Generar Espectro", Dock = DockStyle.Fill, Height = 34 };
            _btnGenerate.Click += BtnGenerate_Click;
            paramPanel.Controls.Add(_btnGenerate, 0, 10);
            paramPanel.SetColumnSpan(_btnGenerate, 2);

            _lblInfo = new Label { Dock = DockStyle.Fill, Height = 36, Font = new System.Drawing.Font("Segoe UI", 8F), ForeColor = System.Drawing.Color.DimGray };
            paramPanel.Controls.Add(_lblInfo, 0, 11);
            paramPanel.SetColumnSpan(_lblInfo, 2);

            // ── Middle: data grid ────────────────────────────────────────────
            _dgvSpectrum = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Font = new System.Drawing.Font("Consolas", 8.5F),
                BackgroundColor = System.Drawing.Color.White
            };
            _dgvSpectrum.Columns.Add("T", "T (s)");
            _dgvSpectrum.Columns.Add("Sa", "Sa (g)");
            _dgvSpectrum.Columns.Add("SaR", "Sa/R (g)");

            // ── Right: chart ─────────────────────────────────────────────────
            _panelChart = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(240, 244, 248),
                BorderStyle = BorderStyle.FixedSingle
            };
            _panelChart.Paint += PanelChart_Paint;
            _panelChart.Resize += (s, e) => _panelChart.Invalidate();

            mainLayout.Controls.Add(paramPanel, 0, 0);
            mainLayout.Controls.Add(_dgvSpectrum, 1, 0);
            mainLayout.Controls.Add(_panelChart, 2, 0);

            this.Controls.Add(mainLayout);
        }

        private static NumericUpDown MakeNud(decimal min, decimal max, int decimals, decimal increment, decimal value)
        {
            return new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = min, Maximum = max,
                DecimalPlaces = decimals,
                Increment = increment,
                Value = value
            };
        }

        private static void AddParamRow(TableLayoutPanel panel, int row, string label, Control ctrl)
        {
            panel.Controls.Add(new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleRight
            }, 0, row);
            panel.Controls.Add(ctrl, 1, row);
        }
    }
}
