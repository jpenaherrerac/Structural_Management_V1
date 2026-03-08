using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using App.Application.Interfaces;
using App.Application.UseCases;
using App.Domain.Entities.Seismic;

namespace App.WinForms.UserControls
{
    /// <summary>
    /// Tab control for "Desplazamientos y Derivas" inside the seismic results viewer.
    /// Layout: Left config panel | Center drift table | Bottom drift chart placeholder.
    /// </summary>
    public sealed class StoryDriftViewerControl : UserControl
    {
        private readonly ISapAdapter _sapAdapter;

        // Config panel controls
        private ComboBox _cmbLoadCase;
        private ComboBox _cmbMaterial;
        private NumericUpDown _nudR;
        private NumericUpDown _nudDriftLimit;
        private CheckBox _chkDriftX;
        private CheckBox _chkDriftY;
        private CheckBox _chkLimit;

        // Results
        private DataGridView _gridDrifts;
        private BindingSource _bsDrifts;
        private Panel _chartPanel;
        private Label _lblSummary;

        // Buttons
        private Button _btnCalculate;

        // State
        private DriftDataSet? _lastDataSet;

        public StoryDriftViewerControl(ISapAdapter sapAdapter)
        {
            _sapAdapter = sapAdapter ?? throw new ArgumentNullException(nameof(sapAdapter));
            InitializeLayout();
        }

        // ─── Layout ─────────────────────────────────────────────────────────────
        private void InitializeLayout()
        {
            Dock = DockStyle.Fill;

            // ── Left config panel ───────────────────────────────────────────────
            var configPanel = new Panel { Dock = DockStyle.Left, Width = 230, Padding = new Padding(10), BackColor = Color.FromArgb(245, 247, 250) };
            var fLabel = new Font("Segoe UI", 9F);
            var fBold = new Font("Segoe UI", 9F, FontStyle.Bold);
            int y = 10;

            // -- Configuración header
            configPanel.Controls.Add(new Label { Text = "Configuración", Font = fBold, Location = new Point(10, y), AutoSize = true });
            y += 28;

            // Load case
            configPanel.Controls.Add(new Label { Text = "Caso de Carga:", Font = fLabel, Location = new Point(10, y), AutoSize = true });
            y += 20;
            _cmbLoadCase = new ComboBox
            {
                Location = new Point(10, y), Width = 200, DropDownStyle = ComboBoxStyle.DropDown, Font = fLabel,
                Items = { "Sdx", "Sdy", "RSX", "RSY", "Sismo X", "Sismo Y" }
            };
            _cmbLoadCase.SelectedIndex = 0;
            configPanel.Controls.Add(_cmbLoadCase);
            y += 30;

            // Factor R
            configPanel.Controls.Add(new Label { Text = "Factor R:", Font = fLabel, Location = new Point(10, y), AutoSize = true });
            y += 20;
            _nudR = new NumericUpDown
            {
                Location = new Point(10, y), Width = 80, Minimum = 1, Maximum = 12, DecimalPlaces = 1, Value = 6, Font = fLabel
            };
            configPanel.Controls.Add(_nudR);
            y += 30;

            // -- Límites Normativos header
            configPanel.Controls.Add(new Label { Text = "Límites Normativos", Font = fBold, Location = new Point(10, y), AutoSize = true });
            y += 28;

            // Material
            configPanel.Controls.Add(new Label { Text = "Material:", Font = fLabel, Location = new Point(10, y), AutoSize = true });
            y += 20;
            _cmbMaterial = new ComboBox
            {
                Location = new Point(10, y), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = fLabel,
                Items = { "Concreto", "Acero", "Mampostería" }
            };
            _cmbMaterial.SelectedIndex = 0;
            _cmbMaterial.SelectedIndexChanged += (s, e) =>
            {
                string mat = _cmbMaterial.SelectedItem?.ToString() ?? "Concreto";
                _nudDriftLimit.Value = (decimal)StoryDriftCalculationParameters.GetDriftLimitForMaterial(mat);
            };
            configPanel.Controls.Add(_cmbMaterial);
            y += 30;

            // Drift limit
            configPanel.Controls.Add(new Label { Text = "Deriva máxima (Δ/h):", Font = fLabel, Location = new Point(10, y), AutoSize = true });
            y += 20;
            _nudDriftLimit = new NumericUpDown
            {
                Location = new Point(10, y), Width = 100, Minimum = 0.001m, Maximum = 0.020m,
                DecimalPlaces = 4, Increment = 0.001m, Value = 0.007m, Font = fLabel
            };
            configPanel.Controls.Add(_nudDriftLimit);
            y += 38;

            // -- Visualización header
            configPanel.Controls.Add(new Label { Text = "Visualización", Font = fBold, Location = new Point(10, y), AutoSize = true });
            y += 28;

            _chkDriftX = new CheckBox { Text = "Drift-X", Location = new Point(10, y), Checked = true, Font = fLabel, AutoSize = true };
            configPanel.Controls.Add(_chkDriftX);
            y += 22;

            _chkDriftY = new CheckBox { Text = "Drift-Y", Location = new Point(10, y), Checked = true, Font = fLabel, AutoSize = true };
            configPanel.Controls.Add(_chkDriftY);
            y += 22;

            _chkLimit = new CheckBox { Text = "Límite", Location = new Point(10, y), Checked = true, Font = fLabel, AutoSize = true };
            configPanel.Controls.Add(_chkLimit);
            y += 35;

            // Calculate button
            _btnCalculate = new Button { Text = "Calcular", Location = new Point(10, y), Size = new Size(200, 32), Font = fBold };
            _btnCalculate.Click += BtnCalculate_Click;
            configPanel.Controls.Add(_btnCalculate);

            // ── Center: split between grid and chart ────────────────────────────
            var centerSplitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 280
            };

            // Grid
            _bsDrifts = new BindingSource();
            _gridDrifts = new DataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = _bsDrifts,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false
            };
            centerSplitter.Panel1.Controls.Add(_gridDrifts);

            // Chart placeholder
            _chartPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            _lblSummary = new Label
            {
                Text = "Presione [Calcular] para obtener resultados de derivas.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.Gray
            };
            _chartPanel.Controls.Add(_lblSummary);
            centerSplitter.Panel2.Controls.Add(_chartPanel);

            // ── Assemble ────────────────────────────────────────────────────────
            Controls.Add(centerSplitter);
            Controls.Add(configPanel);
        }

        // ─── Calculate ──────────────────────────────────────────────────────────
        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            if (!_sapAdapter.IsConnected)
            {
                MessageBox.Show("Conecte a SAP2000 primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                var parameters = new StoryDriftCalculationParameters
                {
                    LoadCaseX = _cmbLoadCase.Text,
                    LoadCaseY = _cmbLoadCase.Text, // will use same case; user can switch
                    ReductionFactorR = (double)_nudR.Value,
                    Material = _cmbMaterial.SelectedItem?.ToString() ?? "Concreto",
                    DriftLimit = (double)_nudDriftLimit.Value
                };

                var calculator = new StoryDriftCalculator(_sapAdapter);
                _lastDataSet = calculator.Compute(parameters);
                DisplayResults(_lastDataSet, parameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al calcular derivas:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { Cursor = Cursors.Default; }
        }

        // ─── Display ────────────────────────────────────────────────────────────
        private void DisplayResults(DriftDataSet ds, StoryDriftCalculationParameters parms)
        {
            var rows = ds.Results.Select(r => new DriftGridRow
            {
                Piso = r.StoryName,
                Punto = r.ControlPoint ?? "-",
                DriftX_Elastico = r.ElasticDriftX,
                DriftY_Elastico = r.ElasticDriftY,
                DriftX_Inelastico = r.InelasticDriftX,
                DriftY_Inelastico = r.InelasticDriftY
            }).ToList();

            _bsDrifts.DataSource = null;
            _bsDrifts.DataSource = rows;

            // Format columns
            foreach (DataGridViewColumn col in _gridDrifts.Columns)
            {
                if (col.Name.Contains("Drift"))
                    col.DefaultCellStyle.Format = "E3";
            }

            // Summary
            double maxDrift = 0;
            string criticalStory = "-";
            if (ds.Results.Count > 0)
            {
                var critical = ds.Results.OrderByDescending(r => Math.Max(r.InelasticDriftX, r.InelasticDriftY)).First();
                maxDrift = Math.Max(critical.InelasticDriftX, critical.InelasticDriftY);
                criticalStory = critical.StoryName;
            }

            bool compliant = maxDrift <= parms.DriftLimit;
            _lblSummary.Text =
                $"Deriva máxima = {maxDrift:E4}   |   Límite = {parms.DriftLimit:F4}\n" +
                $"Piso crítico: {criticalStory}\n" +
                $"R = {parms.ReductionFactorR}   |   Material: {parms.Material}\n\n" +
                (compliant ? "✅ CUMPLE con el límite normativo" : "❌ NO CUMPLE – se excede la deriva permitida");
            _lblSummary.ForeColor = compliant ? Color.FromArgb(0, 120, 60) : Color.Red;
        }

        // ─── Grid row DTO ───────────────────────────────────────────────────────
        public class DriftGridRow
        {
            public string Piso { get; set; } = string.Empty;
            public string Punto { get; set; } = string.Empty;
            public double DriftX_Elastico { get; set; }
            public double DriftY_Elastico { get; set; }
            public double DriftX_Inelastico { get; set; }
            public double DriftY_Inelastico { get; set; }
        }
    }
}
