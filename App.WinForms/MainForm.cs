using System;
using System.Collections.Generic;
using System.Windows.Forms;
using App.Application.Interfaces;
using App.Application.UseCases;
using App.Domain.Entities.Elements;
using App.Domain.Entities.Loads;
using App.Domain.Entities.Seismic;
using App.Domain.Enums;

namespace App.WinForms
{
    public partial class MainForm : Form
    {
        private readonly ISapAdapter _sapAdapter;
        private readonly CreateProjectUseCase _createProject;
        private readonly HydrateSeismicSourceUseCase _hydrateSeismic;
        private readonly HydrateDesignSourceUseCase _hydrateDesign;
        private readonly ApplySeismicConfigurationUseCase _applyConfig;

        private Guid? _activeProjectId;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;
        private ToolStripStatusLabel _sapStatusLabel;
        private ToolStripStatusLabel _iterationLabel;

        // Shared state for group prefixes (editable via Definir → Grupos)
        private GroupPrefixConfiguration _groupPrefixes = new GroupPrefixConfiguration();
        private int _floorCount = 5;

        // ── Iterative seismic workflow state ─────────────────────────────────
        /// <summary>Current seismic parameter values (from SeismicityForm).</summary>
        private Dictionary<string, double> _currentSeismicValues = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        /// <summary>Current seismic configuration for the active project.</summary>
        private SeismicConfiguration _seismicConfig;
        /// <summary>Current iteration number (1-based).</summary>
        private int _iterationNumber = 1;
        /// <summary>Most recent iteration record.</summary>
        private AnalysisIterationRecord _lastIteration;
        /// <summary>Most recent structural output snapshot from hydration.</summary>
        private StructureOutputSnapshot _lastSnapshot;

        public MainForm(
            ISapAdapter sapAdapter,
            CreateProjectUseCase createProject,
            HydrateSeismicSourceUseCase hydrateSeismic,
            HydrateDesignSourceUseCase hydrateDesign,
            ApplySeismicConfigurationUseCase applyConfig)
        {
            _sapAdapter = sapAdapter ?? throw new ArgumentNullException(nameof(sapAdapter));
            _createProject = createProject ?? throw new ArgumentNullException(nameof(createProject));
            _hydrateSeismic = hydrateSeismic ?? throw new ArgumentNullException(nameof(hydrateSeismic));
            _hydrateDesign = hydrateDesign ?? throw new ArgumentNullException(nameof(hydrateDesign));
            _applyConfig = applyConfig ?? throw new ArgumentNullException(nameof(applyConfig));
            InitializeComponent();
            UpdateSapStatus();
            UpdateIterationLabel();
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
            // Capture the seismic values after the dialog closes
            var vals = frm.GetCurrentValues();
            if (vals != null && vals.Count > 0)
            {
                _currentSeismicValues = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                foreach (var kv in vals) _currentSeismicValues[kv.Key] = kv.Value;
                _statusLabel.Text = $"Parámetros sísmicos actualizados (R_x={GetVal("R_x", 0):G4}, R_y={GetVal("R_y", 0):G4}).";
            }
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

        // ─── Cargas menu ────────────────────────────────────────────────────────
        private void menuConfigurarCargas_Click(object sender, EventArgs e)
        {
            if (!EnsureActiveProject()) return;
            if (!_sapAdapter.IsConnected)
            {
                MessageBox.Show("Conecte a SAP2000 primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_currentSeismicValues.Count == 0)
            {
                MessageBox.Show(
                    "Configure los parámetros sísmicos primero.\n(Sismicidad → Parámetros Sísmicos)",
                    "Parámetros pendientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Build or update SeismicConfiguration
            if (_seismicConfig == null)
                _seismicConfig = new SeismicConfiguration(_activeProjectId!.Value);

            _seismicConfig.ZoneFactor = GetVal("Z", 0.45);
            _seismicConfig.SoilAmplificationFactor = GetVal("S", 1.0);
            _seismicConfig.ImportanceFactor = GetVal("U", 1.0);
            _seismicConfig.ReductionFactor = GetVal("Ro_x", 6.0);
            _seismicConfig.Tp = GetVal("TP", 0.4);
            _seismicConfig.Tl = GetVal("TL", 2.5);

            // Build the default load configuration
            var loadConfig = DefaultLoadConfigurationBuilder.Build(
                _activeProjectId!.Value,
                _seismicConfig,
                _currentSeismicValues);

            var request = new ApplySeismicConfigurationRequest
            {
                ProjectId = _activeProjectId!.Value,
                LoadConfig = loadConfig,
                NumberOfStories = _floorCount,
                StoryHeights = null,
                IterationNumber = _iterationNumber
            };

            Cursor = Cursors.WaitCursor;
            var response = _applyConfig.Execute(request);
            Cursor = Cursors.Default;

            if (response.Success)
            {
                _lastIteration = response.Iteration;
                _statusLabel.Text = $"Iteración {_iterationNumber}: Cargas aplicadas a SAP2000. " +
                                    $"({response.CommandsExecuted.Count} comandos)";
                UpdateIterationLabel();
                MessageBox.Show(
                    $"Configuración aplicada exitosamente.\n\n" +
                    $"Iteración: {_iterationNumber}\n" +
                    $"Comandos ejecutados: {response.CommandsExecuted.Count}\n\n" +
                    "Siguiente paso: Run → Run Análisis",
                    "Cargas Aplicadas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Error al aplicar configuración:\n{response.ErrorMessage}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── Helpers ────────────────────────────────────────────────────────────
        private double GetVal(string key, double fallback)
        {
            return _currentSeismicValues.TryGetValue(key, out var v) && v > 0 ? v : fallback;
        }

        private void UpdateIterationLabel()
        {
            if (_iterationLabel != null)
            {
                string phase = _lastIteration?.CurrentPhase.ToString() ?? "Inicio";
                _iterationLabel.Text = $"Iteración: {_iterationNumber} | {phase}";
            }
        }

        // ─── Obtener menu (override to capture snapshot + advance iteration) ───
        private void menuObtenerSismico_Advanced_Click(object sender, EventArgs e)
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
                Label = $"Sismico-Iter{_iterationNumber}-{DateTime.Now:yyyyMMdd-HHmm}",
                SeismicLoadCaseX = "Sdx",
                SeismicLoadCaseY = "Sdy",
                RunAnalysisFirst = false
            };
            var response = _hydrateSeismic.Execute(request);
            if (response.Success)
            {
                _lastSnapshot = response.Snapshot;

                // Advance iteration tracking
                if (_lastIteration != null)
                    _lastIteration.AdvanceTo(IterationPhase.ResultsExtracted);

                // Capture results into iteration record
                if (_lastIteration != null && _lastSnapshot?.GlobalSummary != null)
                {
                    _lastIteration.FundamentalPeriodX = _lastSnapshot.GlobalSummary.FundamentalPeriodX;
                    _lastIteration.FundamentalPeriodY = _lastSnapshot.GlobalSummary.FundamentalPeriodY;
                    _lastIteration.AnalysisConverged = true;
                }

                if (_lastIteration != null && _lastSnapshot?.DriftData != null &&
                    _lastSnapshot.DriftData.Results.Count > 0)
                {
                    _lastIteration.MaxDriftX = _lastSnapshot.DriftData.GetMaxInelasticDriftX();
                    _lastIteration.MaxDriftY = _lastSnapshot.DriftData.GetMaxInelasticDriftY();
                }

                UpdateIterationLabel();
                _statusLabel.Text = $"Iteración {_iterationNumber}: Datos sísmicos extraídos.";
                MessageBox.Show(
                    $"Datos sísmicos extraídos (Iteración {_iterationNumber}).\n" +
                    $"ID: {response.SeismicSourceId}\n\n" +
                    "Revise los resultados en Análisis → Ver Resultados Sísmicos.\n" +
                    "Si necesita ajustar parámetros, vuelva a Sismicidad → Parámetros Sísmicos\n" +
                    "y luego Cargas → Configurar Cargas para re-aplicar.",
                    "Obtener Sísmico", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Error: {response.ErrorMessage}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Advances to next iteration after the user adjusts parameters.
        /// Called when re-applying configuration after parameter update.
        /// </summary>
        private void AdvanceIteration()
        {
            if (_lastIteration != null)
                _lastIteration.MarkCompleted();

            _iterationNumber++;
            UpdateIterationLabel();
        }
    }
}
