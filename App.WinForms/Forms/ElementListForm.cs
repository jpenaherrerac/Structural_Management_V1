using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using App.Application.Interfaces;
using App.Domain.Entities.Elements;
using App.Domain.Enums;

namespace App.WinForms.Forms
{
    /// <summary>
    /// Generic element-list window used for Vigas, Columnas, Muros de Corte, and Losas.
    /// Toolbar: [Groups] [Todas] [Seleccionar]
    /// Body: DataGridView showing retrieved elements.
    /// </summary>
    public sealed class ElementListForm : Form
    {
        private readonly ISapAdapter _sapAdapter;
        private readonly ElementType _elementType;
        private readonly string _groupPrefix;
        private readonly int _floorCount;

        private DataGridView _grid;
        private Button _btnGroups;
        private Button _btnAll;
        private Button _btnSelected;
        private Label _lblInfo;

        // In-memory element collection for the current session
        private readonly List<ElementRowData> _rows = new List<ElementRowData>();
        private BindingSource _bindingSource;

        public ElementListForm(ISapAdapter sapAdapter, ElementType elementType, string groupPrefix, int floorCount)
        {
            _sapAdapter = sapAdapter ?? throw new ArgumentNullException(nameof(sapAdapter));
            _elementType = elementType;
            _groupPrefix = groupPrefix ?? "Grupo";
            _floorCount = floorCount > 0 ? floorCount : 5;
            InitializeLayout();
        }

        // ─── Layout ─────────────────────────────────────────────────────────────
        private void InitializeLayout()
        {
            string title = _elementType switch
            {
                ElementType.Beam => "Vigas",
                ElementType.Column => "Columnas",
                ElementType.ShearWall => "Muros de Corte",
                ElementType.Slab => "Losas",
                _ => "Elementos"
            };

            Text = $"{title} – Listado de Elementos";
            Size = new Size(900, 600);
            MinimumSize = new Size(700, 400);
            StartPosition = FormStartPosition.CenterParent;

            // ── Toolbar ─────────────────────────────────────────────────────────
            var toolPanel = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(8) };

            _btnGroups = new Button { Text = "📁 Groups", Location = new Point(8, 10), Size = new Size(120, 30) };
            _btnGroups.Click += BtnGroups_Click;
            toolPanel.Controls.Add(_btnGroups);

            _btnAll = new Button { Text = "📋 Todas", Location = new Point(140, 10), Size = new Size(100, 30) };
            _btnAll.Click += BtnAll_Click;
            toolPanel.Controls.Add(_btnAll);

            _btnSelected = new Button { Text = "🔍 Seleccionar", Location = new Point(252, 10), Size = new Size(130, 30) };
            _btnSelected.Click += BtnSelected_Click;
            toolPanel.Controls.Add(_btnSelected);

            _lblInfo = new Label
            {
                Text = $"Prefijo: {_groupPrefix}  |  Pisos: {_floorCount}",
                Location = new Point(400, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 110, 130)
            };
            toolPanel.Controls.Add(_lblInfo);

            // ── Grid ────────────────────────────────────────────────────────────
            _bindingSource = new BindingSource();
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = _bindingSource,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false
            };
            _grid.CellDoubleClick += Grid_CellDoubleClick;

            Controls.Add(_grid);
            Controls.Add(toolPanel);
        }

        // ─── Groups button ──────────────────────────────────────────────────────
        private void BtnGroups_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            Cursor = Cursors.WaitCursor;

            try
            {
                var allGroups = _sapAdapter.GetGroupNames().ToList();
                var expectedNames = GroupPrefixConfiguration.BuildGroupNames(_groupPrefix, _floorCount);

                // Filter SAP groups that match the prefix pattern
                var matchingGroups = allGroups
                    .Where(g => expectedNames.Any(exp => g.Equals(exp, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (matchingGroups.Count == 0)
                {
                    MessageBox.Show(
                        $"No se encontraron grupos con prefijo '{_groupPrefix}' en SAP2000.\n" +
                        $"Se esperaban nombres como: {string.Join(", ", expectedNames.Take(3))}...",
                        "Grupos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _rows.Clear();
                foreach (var grp in matchingGroups)
                {
                    var elements = _sapAdapter.GetGroupElements(grp);
                    foreach (var elId in elements)
                    {
                        _rows.Add(new ElementRowData
                        {
                            ElementId = elId,
                            Group = grp,
                            Story = ExtractStory(grp),
                            Type = _elementType.ToString(),
                            Source = "Group"
                        });
                    }
                }

                RefreshGrid();
                _lblInfo.Text = $"Cargados {_rows.Count} elementos de {matchingGroups.Count} grupos";
            }
            finally { Cursor = Cursors.Default; }
        }

        // ─── All button ─────────────────────────────────────────────────────────
        private void BtnAll_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            Cursor = Cursors.WaitCursor;
            try
            {
                _rows.Clear();
                IEnumerable<string> ids;

                if (_elementType == ElementType.Beam || _elementType == ElementType.Column)
                    ids = _sapAdapter.GetFrameElementIds();
                else
                    ids = _sapAdapter.GetAreaElementIds();

                foreach (var id in ids)
                {
                    _rows.Add(new ElementRowData
                    {
                        ElementId = id,
                        Group = "-",
                        Story = "-",
                        Type = _elementType.ToString(),
                        Source = "Todas"
                    });
                }

                RefreshGrid();
                _lblInfo.Text = $"Cargados {_rows.Count} elementos (todos del modelo)";
            }
            finally { Cursor = Cursors.Default; }
        }

        // ─── Selected button ────────────────────────────────────────────────────
        private void BtnSelected_Click(object sender, EventArgs e)
        {
            if (!EnsureConnected()) return;
            Cursor = Cursors.WaitCursor;
            try
            {
                IEnumerable<string> ids;

                if (_elementType == ElementType.Beam || _elementType == ElementType.Column)
                    ids = _sapAdapter.GetSelectedFrameIds();
                else
                    ids = _sapAdapter.GetSelectedAreaIds();

                var idList = ids.ToList();
                if (idList.Count == 0)
                {
                    MessageBox.Show("No hay elementos seleccionados en SAP2000.", "Seleccionar",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _rows.Clear();
                foreach (var id in idList)
                {
                    _rows.Add(new ElementRowData
                    {
                        ElementId = id,
                        Group = "-",
                        Story = "-",
                        Type = _elementType.ToString(),
                        Source = "Seleccionado"
                    });
                }

                RefreshGrid();
                _lblInfo.Text = $"Cargados {_rows.Count} elementos seleccionados en SAP2000";
            }
            finally { Cursor = Cursors.Default; }
        }

        // ─── Double-click detail ────────────────────────────────────────────────
        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _rows.Count) return;
            var row = _rows[e.RowIndex];
            MessageBox.Show(
                $"Elemento: {row.ElementId}\n" +
                $"Tipo: {row.Type}\n" +
                $"Grupo: {row.Group}\n" +
                $"Piso: {row.Story}\n" +
                $"Fuente: {row.Source}\n\n" +
                "(Ventana de detalle próximamente)",
                "Detalle de Elemento", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ─── Helpers ────────────────────────────────────────────────────────────
        private bool EnsureConnected()
        {
            if (_sapAdapter.IsConnected) return true;
            MessageBox.Show("Conecte a SAP2000 primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private void RefreshGrid()
        {
            _bindingSource.DataSource = null;
            _bindingSource.DataSource = _rows;
        }

        private static string ExtractStory(string groupName)
        {
            // "Vigas_P3" → "P3"
            int idx = groupName.LastIndexOf('_');
            return idx >= 0 ? groupName.Substring(idx + 1) : groupName;
        }

        // ─── Row DTO ────────────────────────────────────────────────────────────
        public class ElementRowData
        {
            public string ElementId { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string Group { get; set; } = string.Empty;
            public string Story { get; set; } = string.Empty;
            public string Source { get; set; } = string.Empty;
        }
    }
}
