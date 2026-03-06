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

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 2,
                RowCount = 1
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // ── Left panel: input parameters ─────────────────────────────────
            var leftPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 14,
                Padding = new Padding(4)
            };
            leftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            leftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;

            // Header
            var lblHeader = new Label
            {
                Text = "Parámetros Sísmicos NEC / E030",
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold),
                Dock = DockStyle.Fill,
                Height = 28
            };
            leftPanel.Controls.Add(lblHeader, 0, row);
            leftPanel.SetColumnSpan(lblHeader, 2);
            row++;

            // Zone
            _cmbZone = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbZone.Items.AddRange(new object[]
            {
                "Zona I — Z = 0.15", "Zona II — Z = 0.25", "Zona III — Z = 0.30",
                "Zona IV — Z = 0.35", "Zona V — Z = 0.40", "Zona VI — Z = 0.50"
            });
            _cmbZone.SelectedIndex = 3;
            _cmbZone.SelectedIndexChanged += CmbZone_SelectedIndexChanged;
            AddRow(leftPanel, row++, "Factor de zona (Z):", _cmbZone);

            // Soil
            _cmbSoilType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbSoilType.Items.AddRange(new object[]
            {
                "A — Roca (S=1.00)", "B — Roca blanda (S=1.20)",
                "C — Suelo muy denso (S=1.11)", "D — Suelo blando (S=1.11 NEC / 1.35 E030)", "E — Suelo blando especial (S=1.30)"
            });
            _cmbSoilType.SelectedIndex = 3;
            _cmbSoilType.SelectedIndexChanged += CmbSoilType_SelectedIndexChanged;
            AddRow(leftPanel, row++, "Tipo de suelo (S):", _cmbSoilType);

            // Usage/Importance
            _cmbUsage = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbUsage.Items.AddRange(new object[]
            {
                "I — Residencial / Comercial (I=1.00)",
                "II — Comercial / Industrial (I=1.25)",
                "III — Esencial / Hospital (I=1.50)"
            });
            _cmbUsage.SelectedIndex = 0;
            AddRow(leftPanel, row++, "Categoría de uso (I):", _cmbUsage);

            // R
            _nudR = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1, Maximum = 12, DecimalPlaces = 1, Increment = 0.5M, Value = 6M };
            AddRow(leftPanel, row++, "Factor R:", _nudR);

            // Ct
            _nudCt = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0.01M, Maximum = 0.9M, DecimalPlaces = 3, Increment = 0.001M, Value = 0.055M };
            AddRow(leftPanel, row++, "Coeficiente Ct:", _nudCt);

            // Alpha
            _nudAlpha = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0.5M, Maximum = 1.2M, DecimalPlaces = 2, Increment = 0.01M, Value = 0.9M };
            AddRow(leftPanel, row++, "Exponente α:", _nudAlpha);

            // Height
            _nudHeight = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 3, Maximum = 300, DecimalPlaces = 1, Increment = 0.5M, Value = 15M };
            AddRow(leftPanel, row++, "Altura H (m):", _nudHeight);

            // Tp
            _nudTp = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0.05M, Maximum = 3M, DecimalPlaces = 3, Increment = 0.01M, Value = 0.30M };
            AddRow(leftPanel, row++, "Tp (s):", _nudTp);

            // Tl
            _nudTl = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1M, Maximum = 10M, DecimalPlaces = 2, Increment = 0.1M, Value = 4M };
            AddRow(leftPanel, row++, "Tl (s):", _nudTl);

            // Fa, Fd, Fs
            _nudFa = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0.5M, Maximum = 3M, DecimalPlaces = 2, Increment = 0.01M, Value = 1.20M };
            AddRow(leftPanel, row++, "Fa:", _nudFa);
            _nudFd = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0.5M, Maximum = 3M, DecimalPlaces = 2, Increment = 0.01M, Value = 1.19M };
            AddRow(leftPanel, row++, "Fd:", _nudFd);
            _nudFs = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0.5M, Maximum = 3M, DecimalPlaces = 2, Increment = 0.01M, Value = 1.28M };
            AddRow(leftPanel, row++, "Fs:", _nudFs);

            // Calculate button + period/Sa labels
            var calcPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            _btnCalculate = new Button { Text = "Calcular", Width = 100, Height = 30 };
            _btnCalculate.Click += BtnCalculate_Click;
            _lblComputedPeriod = new Label { Text = "T = —", Width = 90, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            _lblSa = new Label { Text = "Sa = —", Width = 100, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            calcPanel.Controls.Add(_btnCalculate);
            calcPanel.Controls.Add(_lblComputedPeriod);
            calcPanel.Controls.Add(_lblSa);
            leftPanel.Controls.Add(calcPanel, 0, row);
            leftPanel.SetColumnSpan(calcPanel, 2);

            // ── Right panel: results ──────────────────────────────────────────
            _rtbResults = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Consolas", 9.5F),
                ReadOnly = true,
                BackColor = System.Drawing.Color.FromArgb(30, 30, 30),
                ForeColor = System.Drawing.Color.FromArgb(220, 220, 180),
                Text = "[ Presione 'Calcular' para ver los resultados ]\n"
            };

            mainLayout.Controls.Add(leftPanel, 0, 0);
            mainLayout.Controls.Add(_rtbResults, 1, 0);
            this.Controls.Add(mainLayout);
        }

        private static void AddRow(TableLayoutPanel panel, int row, string labelText, Control ctrl)
        {
            panel.Controls.Add(new Label
            {
                Text = labelText,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 4, 0)
            }, 0, row);
            panel.Controls.Add(ctrl, 1, row);
        }
    }
}
