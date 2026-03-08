using System;
using System.Windows.Forms;
using App.Application.Interfaces;
using App.Application.UseCases;
using App.Domain.Entities.Elements;
using App.Domain.Enums;

namespace App.WinForms
{
    public partial class MainForm : Form
    {
        private readonly ISapAdapter _sapAdapter;
        private readonly CreateProjectUseCase _createProject;
        private readonly HydrateSeismicSourceUseCase _hydrateSeismic;
        private readonly HydrateDesignSourceUseCase _hydrateDesign;

        private Guid? _activeProjectId;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;
        private ToolStripStatusLabel _sapStatusLabel;

        // Shared state for group prefixes (editable via Definir → Grupos)
        private GroupPrefixConfiguration _groupPrefixes = new GroupPrefixConfiguration();
        private int _floorCount = 5;

        public MainForm(
            ISapAdapter sapAdapter,
            CreateProjectUseCase createProject,
            HydrateSeismicSourceUseCase hydrateSeismic,
            HydrateDesignSourceUseCase hydrateDesign)
        {
            _sapAdapter = sapAdapter ?? throw new ArgumentNullException(nameof(sapAdapter));
            _createProject = createProject ?? throw new ArgumentNullException(nameof(createProject));
            _hydrateSeismic = hydrateSeismic ?? throw new ArgumentNullException(nameof(hydrateSeismic));
            _hydrateDesign = hydrateDesign ?? throw new ArgumentNullException(nameof(hydrateDesign));
            InitializeComponent();
            UpdateSapStatus();
        }

        private void UpdateSapStatus()
        {
            if (_sapAdapter.IsConnected)
            {
                _sapStatusLabel.Text = $"SAP2000: Conectado ({_sapAdapter.GetSapVersion()})";
                _sapStatusLabel.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                _sapStatusLabel.Text = "SAP2000: Desconectado";
                _sapStatusLabel.ForeColor = System.Drawing.Color.Red;
            }
        }

        // ─── Archivo menu ───────────────────────────────────────────────────────
        private void menuNuevoProyecto_Click(object sender, EventArgs e)
        {
            using var dlg = new Forms.NewProjectDialog(_createProject);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _activeProjectId = dlg.CreatedProjectId;
                _statusLabel.Text = $"Proyecto activo: {dlg.ProjectName}";
                MessageBox.Show($"Proyecto '{dlg.ProjectName}' creado correctamente.",
                    "Nuevo Proyecto", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void menuAbrir_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "SAP2000 Models (*.sdb)|*.sdb|All files (*.*)|*.*",
                Title = "Abrir modelo SAP2000"
            };
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                if (_sapAdapter.IsConnected)
                {
                    bool opened = _sapAdapter.OpenModel(ofd.FileName);
                    _statusLabel.Text = opened ? $"Modelo abierto: {ofd.FileName}" : "Error al abrir modelo.";
                }
                else
                {
                    MessageBox.Show("Conecte a SAP2000 primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void menuGuardar_Click(object sender, EventArgs e)
        {
            if (!_sapAdapter.IsConnected)
            {
                MessageBox.Show("No hay conexión a SAP2000.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            bool saved = _sapAdapter.SaveModel();
            _statusLabel.Text = saved ? "Modelo guardado." : "Error al guardar.";
        }

        private void menuSalir_Click(object sender, EventArgs e) => Close();

        // ─── Conectar menu ──────────────────────────────────────────────────────
        private void menuConectarSAP_Click(object sender, EventArgs e)
        {
            try
            {
                using var dlg = new Forms.SapSessionDialog(_sapAdapter.ConnectionManager);
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.ResultSession != null)
                {
                    UpdateSapStatus();
                    _statusLabel.Text = $"Conectado a SAP2000 versión {_sapAdapter.GetSapVersion()}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuDesconectarSAP_Click(object sender, EventArgs e)
        {
            _sapAdapter.Disconnect();
            UpdateSapStatus();
            _statusLabel.Text = "Desconectado de SAP2000.";
        }

        private void menuCambiarSesionSAP_Click(object sender, EventArgs e)
        {
            try
            {
                using var dlg = new Forms.SapSessionDialog(_sapAdapter.ConnectionManager);
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.ResultSession != null)
                {
                    UpdateSapStatus();
                    _statusLabel.Text = $"Sesión cambiada – SAP2000 versión {_sapAdapter.GetSapVersion()}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar sesión: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── Sismicidad menu ────────────────────────────────────────────────────
        private void menuSismicidad_Click(object sender, EventArgs e)
        {
            using var frm = new Forms.Seismicity.SeismicityForm();
            frm.ShowDialog(this);
        }

        // ─── Run menu ───────────────────────────────────────────────────────────
        private void menuRunAnalysis_Click(object sender, EventArgs e)
        {
            if (!_sapAdapter.IsConnected)
            {
                MessageBox.Show("Conecte a SAP2000 primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Cursor = Cursors.WaitCursor;
            bool ok = _sapAdapter.RunAnalysis();
            Cursor = Cursors.Default;
            _statusLabel.Text = ok ? "Análisis completado." : "Error en análisis.";
            MessageBox.Show(ok ? "Análisis ejecutado exitosamente." : "Error al ejecutar análisis.",
                "Run Analysis", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }

        private void menuRunDesign_Click(object sender, EventArgs e)
        {
            if (!_sapAdapter.IsConnected)
            {
                MessageBox.Show("Conecte a SAP2000 primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Cursor = Cursors.WaitCursor;
            bool ok = _sapAdapter.RunDesign();
            Cursor = Cursors.Default;
            _statusLabel.Text = ok ? "Diseño completado." : "Error en diseño.";
        }

        // ─── Obtener menu ───────────────────────────────────────────────────────
        private void menuObtenerSismico_Click(object sender, EventArgs e)
        {
            if (!EnsureActiveProject()) return;
            if (!_sapAdapter.IsConnected)
            {
                MessageBox.Show("Conecte a SAP2000 primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var request = new HydrateSeismicSourceRequest
            {
                ProjectId = _activeProjectId!.Value,
                Label = $"Sismico-{DateTime.Now:yyyyMMdd-HHmm}",
                SeismicLoadCaseX = "Sx",
                SeismicLoadCaseY = "Sy",
                RunAnalysisFirst = false
            };
            var response = _hydrateSeismic.Execute(request);
            if (response.Success)
            {
                _statusLabel.Text = "Datos sísmicos obtenidos.";
                MessageBox.Show($"Datos sísmicos extraídos. ID: {response.SeismicSourceId}",
                    "Obtener Sísmico", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Error: {response.ErrorMessage}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuObtenerDiseno_Click(object sender, EventArgs e)
        {
            if (!EnsureActiveProject()) return;
            if (!_sapAdapter.IsConnected)
            {
                MessageBox.Show("Conecte a SAP2000 primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var request = new HydrateDesignSourceRequest
            {
                ProjectId = _activeProjectId!.Value,
                Label = $"Diseno-{DateTime.Now:yyyyMMdd-HHmm}",
                RunDesignFirst = false
            };
            var response = _hydrateDesign.Execute(request);
            _statusLabel.Text = response.Success ? "Datos de diseño obtenidos." : $"Error: {response.ErrorMessage}";
        }

        private bool EnsureActiveProject()
        {
            if (!_activeProjectId.HasValue)
            {
                MessageBox.Show("Cree o abra un proyecto primero (Archivo → Nuevo Proyecto).",
                    "Sin proyecto activo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // ─── Definir menu ───────────────────────────────────────────────────────
        private void menuGrupos_Click(object sender, EventArgs e)
        {
            using var dlg = new Forms.GroupPrefixDialog(_groupPrefixes);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _groupPrefixes = dlg.ResultConfiguration;
                _statusLabel.Text = $"Prefijos actualizados: Vigas={_groupPrefixes.BeamPrefix}, Columnas={_groupPrefixes.ColumnPrefix}";
            }
        }

        private void menuVigas_Click(object sender, EventArgs e)
        {
            var frm = new Forms.ElementListForm(_sapAdapter, ElementType.Beam, _groupPrefixes.BeamPrefix, _floorCount);
            frm.Show(this);
        }

        private void menuColumnas_Click(object sender, EventArgs e)
        {
            var frm = new Forms.ElementListForm(_sapAdapter, ElementType.Column, _groupPrefixes.ColumnPrefix, _floorCount);
            frm.Show(this);
        }

        private void menuMuros_Click(object sender, EventArgs e)
        {
            var frm = new Forms.ElementListForm(_sapAdapter, ElementType.ShearWall, _groupPrefixes.ShearWallPrefix, _floorCount);
            frm.Show(this);
        }

        private void menuLosas_Click(object sender, EventArgs e)
        {
            var frm = new Forms.ElementListForm(_sapAdapter, ElementType.Slab, _groupPrefixes.SlabPrefix, _floorCount);
            frm.Show(this);
        }

        // ─── Análisis menu ──────────────────────────────────────────────────────
        private void menuResultadosSismicos_Click(object sender, EventArgs e)
        {
            var frm = new Forms.SeismicResultsForm(_sapAdapter);
            frm.Show(this);
        }
    }
}
