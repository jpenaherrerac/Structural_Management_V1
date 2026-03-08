using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using App.Application.Interfaces;
using App.Domain.Entities.Sap;

namespace App.WinForms.Forms
{
    /// <summary>
    /// Dialog that lists running SAP2000 instances and lets the user choose one,
    /// or create a new instance.
    /// </summary>
    public sealed class SapSessionDialog : Form
    {
        private readonly ISapConnectionManager _connectionManager;
        private ListView _listView;
        private Button _btnAttach;
        private Button _btnNewInstance;
        private Button _btnRefresh;
        private Button _btnCancel;
        private Label _lblInfo;

        /// <summary>The session created as a result of user action, or null if cancelled.</summary>
        public SapSession? ResultSession { get; private set; }

        public SapSessionDialog(ISapConnectionManager connectionManager)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            InitializeComponent();
            RefreshInstances();
        }

        private void InitializeComponent()
        {
            Text = "Conexión SAP2000 – Seleccionar Instancia";
            Size = new Size(620, 420);
            MinimumSize = new Size(500, 350);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // ── Info label ──────────────────────────────────────────────────────
            _lblInfo = new Label
            {
                Text = "Sesiones de SAP2000 detectadas:",
                Location = new Point(12, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            // ── ListView ────────────────────────────────────────────────────────
            _listView = new ListView
            {
                Location = new Point(12, 40),
                Size = new Size(580, 240),
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = false,
                GridLines = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            _listView.Columns.Add("PID", 70);
            _listView.Columns.Add("Título de ventana", 310);
            _listView.Columns.Add("Ruta del programa", 190);
            _listView.DoubleClick += (s, e) => DoAttach();

            // ── Buttons ─────────────────────────────────────────────────────────
            int btnY = 295;

            _btnRefresh = new Button
            {
                Text = "⟳ Refrescar",
                Location = new Point(12, btnY),
                Size = new Size(110, 32)
            };
            _btnRefresh.Click += (s, e) => RefreshInstances();

            _btnAttach = new Button
            {
                Text = "Conectar a seleccionada",
                Location = new Point(200, btnY),
                Size = new Size(170, 32),
                Enabled = false
            };
            _btnAttach.Click += (s, e) => DoAttach();

            _btnNewInstance = new Button
            {
                Text = "Crear nueva instancia",
                Location = new Point(380, btnY),
                Size = new Size(160, 32)
            };
            _btnNewInstance.Click += (s, e) => DoCreateNew();

            _btnCancel = new Button
            {
                Text = "Cancelar",
                Location = new Point(12, btnY + 42),
                Size = new Size(100, 32),
                DialogResult = DialogResult.Cancel
            };

            _listView.SelectedIndexChanged += (s, e) =>
            {
                _btnAttach.Enabled = _listView.SelectedItems.Count > 0;
            };

            Controls.AddRange(new Control[] { _lblInfo, _listView, _btnRefresh, _btnAttach, _btnNewInstance, _btnCancel });
            CancelButton = _btnCancel;
            AcceptButton = _btnAttach;
        }

        private void RefreshInstances()
        {
            _listView.Items.Clear();
            var instances = _connectionManager.DiscoverInstances();

            if (instances.Count == 0)
            {
                _lblInfo.Text = "No se encontraron sesiones de SAP2000 ejecutándose.";
                _btnAttach.Enabled = false;
            }
            else
            {
                _lblInfo.Text = $"Sesiones de SAP2000 detectadas: {instances.Count}";
            }

            foreach (var inst in instances)
            {
                var item = new ListViewItem(inst.ProcessId.ToString());
                item.SubItems.Add(inst.WindowTitle);
                item.SubItems.Add(inst.ProgramPath);
                item.Tag = inst;
                _listView.Items.Add(item);
            }

            if (_listView.Items.Count > 0)
                _listView.Items[0].Selected = true;
        }

        private void DoAttach()
        {
            if (_listView.SelectedItems.Count == 0) return;
            var info = (SapInstanceInfo)_listView.SelectedItems[0].Tag;
            try
            {
                Cursor = Cursors.WaitCursor;
                ResultSession = _connectionManager.AttachToInstance(info);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { Cursor = Cursors.Default; }
        }

        private void DoCreateNew()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                ResultSession = _connectionManager.CreateNewInstance();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear instancia:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { Cursor = Cursors.Default; }
        }
    }
}
