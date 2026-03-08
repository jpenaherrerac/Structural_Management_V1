using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using App.Application.Interfaces;
using App.Domain.Entities.Seismic;

namespace App.WinForms.Forms
{
    /// <summary>
    /// Seismic Results Viewer – Análisis → Ver Resultados Sísmicos.
    /// Tabs: Cortante Basal | Irregularidad Estructural | Desplazamientos y Derivas | Fuerzas Sísmicas
    /// </summary>
    public sealed class SeismicResultsForm : Form
    {
        private readonly ISapAdapter _sapAdapter;
        private TabControl _tabControl;

        public SeismicResultsForm(ISapAdapter sapAdapter)
        {
            _sapAdapter = sapAdapter ?? throw new ArgumentNullException(nameof(sapAdapter));
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            Text = "Resultados Sísmicos";
            Size = new Size(1100, 700);
            MinimumSize = new Size(900, 550);
            StartPosition = FormStartPosition.CenterParent;

            _tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };

            // Tab 1: Cortante Basal
            var tabBaseShear = new TabPage("Cortante Basal");
            BuildBaseShearTab(tabBaseShear);
            _tabControl.TabPages.Add(tabBaseShear);

            // Tab 2: Irregularidad Estructural
            var tabIrregularity = new TabPage("Irregularidad Estructural");
            BuildIrregularityTab(tabIrregularity);
            _tabControl.TabPages.Add(tabIrregularity);

            // Tab 3: Desplazamientos y Derivas
            var tabDrifts = new TabPage("Desplazamientos y Derivas");
            var driftViewer = new UserControls.StoryDriftViewerControl(_sapAdapter) { Dock = DockStyle.Fill };
            tabDrifts.Controls.Add(driftViewer);
            _tabControl.TabPages.Add(tabDrifts);

            // Tab 4: Fuerzas Sísmicas
            var tabForces = new TabPage("Fuerzas Sísmicas");
            BuildSeismicForcesTab(tabForces);
            _tabControl.TabPages.Add(tabForces);

            Controls.Add(_tabControl);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Tab 1: Cortante Basal
        // ═══════════════════════════════════════════════════════════════════════
        private void BuildBaseShearTab(TabPage tab)
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            // Toolbar
            var btnLoad = new Button { Text = "Obtener Cortante Basal", Size = new Size(200, 34), Margin = new Padding(8) };
            var cmbCase = new ComboBox
            {
                Width = 120, DropDownStyle = ComboBoxStyle.DropDown, Items = { "Sdx", "Sdy", "RSX", "RSY" },
                SelectedIndex = 0, Margin = new Padding(8)
            };
            var toolPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            toolPanel.Controls.Add(cmbCase);
            toolPanel.Controls.Add(btnLoad);
            layout.Controls.Add(toolPanel, 0, 0);

            // Grid
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White, RowHeadersVisible = false
            };
            layout.Controls.Add(grid, 0, 1);

            // Summary
            var lblSummary = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 10F) };
            layout.Controls.Add(lblSummary, 0, 2);

            btnLoad.Click += (s, e) =>
            {
                if (!_sapAdapter.IsConnected) { ShowNotConnected(); return; }
                Cursor = Cursors.WaitCursor;
                try
                {
                    string lc = cmbCase.Text;
                    var bs = _sapAdapter.GetBaseShear(lc);
                    var rows = new List<object> { new { LoadCase = bs.LoadCase, Fx_kN = bs.Fx, Fy_kN = bs.Fy, Fz_kN = bs.Fz, Mx_kNm = bs.Mx, My_kNm = bs.My, Mz_kNm = bs.Mz } };
                    grid.DataSource = rows;
                    lblSummary.Text = $"Resultante horizontal: {bs.TotalHorizontalResultant:F2} kN";
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                finally { Cursor = Cursors.Default; }
            };

            tab.Controls.Add(layout);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Tab 2: Irregularidad Estructural
        // ═══════════════════════════════════════════════════════════════════════
        private void BuildIrregularityTab(TabPage tab)
        {
            var lbl = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.Gray,
                Text = "Irregularidad Estructural\n\n" +
                       "Este módulo evaluará:\n" +
                       "• Irregularidad de Rigidez (Piso Blando)\n" +
                       "• Irregularidad de Resistencia (Piso Débil)\n" +
                       "• Irregularidad de Masa\n" +
                       "• Irregularidad Geométrica Vertical\n" +
                       "• Irregularidad Torsional\n" +
                       "• Esquinas Entrantes\n\n" +
                       "(Próximamente)"
            };
            tab.Controls.Add(lbl);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Tab 4: Fuerzas Sísmicas
        // ═══════════════════════════════════════════════════════════════════════
        private void BuildSeismicForcesTab(TabPage tab)
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Toolbar
            var btnLoad = new Button { Text = "Obtener Fuerzas por Piso", Size = new Size(200, 34), Margin = new Padding(8) };
            var cmbCase = new ComboBox
            {
                Width = 120, DropDownStyle = ComboBoxStyle.DropDown, Items = { "Sdx", "Sdy", "RSX", "RSY" },
                SelectedIndex = 0, Margin = new Padding(8)
            };
            var toolPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            toolPanel.Controls.Add(cmbCase);
            toolPanel.Controls.Add(btnLoad);
            layout.Controls.Add(toolPanel, 0, 0);

            // Grid
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White, RowHeadersVisible = false
            };
            layout.Controls.Add(grid, 0, 1);

            btnLoad.Click += (s, e) =>
            {
                if (!_sapAdapter.IsConnected) { ShowNotConnected(); return; }
                Cursor = Cursors.WaitCursor;
                try
                {
                    string lc = cmbCase.Text;
                    var stories = _sapAdapter.GetStoryShears(lc).ToList();
                    var rows = stories.Select(st => new
                    {
                        Piso = st.StoryName,
                        Nivel = st.StoryLevel,
                        Elevacion_m = st.ElevationMeters,
                        CortanteX_kN = st.ShearX,
                        CortanteY_kN = st.ShearY,
                        Peso_kN = st.WeightKN
                    }).ToList();
                    grid.DataSource = rows;
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                finally { Cursor = Cursors.Default; }
            };

            tab.Controls.Add(layout);
        }

        private static void ShowNotConnected()
        {
            MessageBox.Show("Conecte a SAP2000 primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
