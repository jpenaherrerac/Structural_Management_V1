using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Arbol_de_Cargas.Geometry;

namespace Arbol_de_Cargas.SAP
{
    /// <summary>
    /// Servicio UI-friendly para ejecutar la exportación completa a SAP2000 (crear modelos + correr análisis),
    /// replicando el flujo legacy del botón "Exportar a SAP2000" pero reutilizable desde menú "Run".
    /// </summary>
    public sealed class RunSapExportService
    {
        private readonly SapStaHost.StaComRunner _sapRunner;
        private readonly SapProcessor _sapProcessor;
        private readonly DataGridView _transformGrid;
        private readonly Action _switchToLogTab;

        public RunSapExportService(
        SapStaHost.StaComRunner sapRunner,
        SapProcessor sapProcessor,
        DataGridView transformGrid,
        Action switchToLogTab)
        {
            _sapRunner = sapRunner;
            _sapProcessor = sapProcessor;
            _transformGrid = transformGrid;
            _switchToLogTab = switchToLogTab;
        }

        public void Run(
        Control uiOwner,
        Dictionary<string, List<ExcelToCsv.Bloque>> resultado,
        Dictionary<string, List<TowerModel>> generatedTowers)
        {
            if (generatedTowers == null || generatedTowers.Count == 0)
            {
                MessageBox.Show(uiOwner,
                "No hay torres generadas para exportar.\nPrimero calcule las torres.",
                "No hay torres",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
                return;
            }

            if (_sapRunner == null || _sapProcessor == null)
            {
                MessageBox.Show(uiOwner,
                "SAP2000 no está disponible.\nVerifique la conexión con SAP2000.",
                "SAP2000 no disponible",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
                return;
            }

            if (_transformGrid == null)
            {
                MessageBox.Show(uiOwner,
                "Grid de transformaciones no está disponible.\nEsto es un error interno.",
                "Error interno",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
                return;
            }

            if (resultado == null || resultado.Count == 0)
            {
                MessageBox.Show(uiOwner,
                "No hay datos en memoria para exportar.",
                "Sin datos",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
                return;
            }

            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Seleccione la carpeta donde guardar los modelos SAP2000";
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog(uiOwner) != DialogResult.OK)
                    return;

                string outputDirectory = folderDialog.SelectedPath;

                _switchToLogTab?.Invoke();
                Application.DoEvents();

                var form = uiOwner as Form ?? uiOwner?.FindForm();
                TabControl tabControl = null;
                if (form != null)
                {
                    tabControl = form.Controls.OfType<TabControl>().FirstOrDefault();
                }

                Action restoreUi = () =>
                {
                    if (form == null) return;
                    try
                    {
                        SetControlsEnabledRecursive(form, true);
                        if (tabControl != null) tabControl.Enabled = true;
                    }
                    catch { }
                };

                // Deshabilitar edición en UI durante procesamiento (pero mantener navegación entre tabs)
                if (form != null)
                {
                    try { SetControlsEnabledRecursive(form, false); } catch { }
                    try { if (tabControl != null) tabControl.Enabled = true; } catch { }
                }

                try
                {
                    // Ocultar SAP2000 al iniciar exportación
                    _sapRunner.Invoke(() =>
                    {
                        try { _sapProcessor.HideSAP2000(); } catch { }
                    });

                    SapProcessor.Log("========================================");
                    SapProcessor.Log("INICIANDO EXPORTACIÓN A SAP2000");
                    SapProcessor.Log("SAP2000 oculto durante procesamiento...");
                    SapProcessor.Log("========================================");

                    Action<string> logCallback = SapProcessor.Log;

                    var modelBuilder = new SapModelBuilder(_sapRunner, _sapProcessor, _transformGrid, logCallback);

                    logCallback($"Carpeta de salida: {outputDirectory}");
                    logCallback(string.Empty);
                    logCallback("[RunSapExportService] Iniciando exportación (reconstrucción por bloque).");

                    modelBuilder.CreateModelsForWorkspace(resultado, generatedTowers, outputDirectory);

                    // Refrescar Resultados en MainForm si existe
                    try
                    {
                        if (form != null)
                        {
                            form.GetType().GetMethod(
                            "RefreshResultadosTab",
                            System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.Public |
                            System.Reflection.BindingFlags.NonPublic)
                            ?.Invoke(form, new object[] { modelBuilder });
                        }
                    }
                    catch { }

                    restoreUi();

                    MessageBox.Show(uiOwner,
                    "Exportación completada.\n\nVer tab 'Log' para detalles completos.",
                    "Exportación SAP2000",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    restoreUi();

                    MessageBox.Show(uiOwner,
                    "Error durante la exportación:\n\n" + ex.Message + "\n\nVer tab 'Log' para más detalles.",
                    "Error de exportación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                    SapProcessor.Log("[ERROR CRÍTICO] " + ex.Message);
                }
                finally
                {
                    try
                    {
                        _sapRunner.Invoke(() =>
                        {
                            try { _sapProcessor.ShowSAP2000(); } catch { }
                        });
                    }
                    catch { }

                    SapProcessor.Log(string.Empty);
                    SapProcessor.Log("========================================");
                    SapProcessor.Log("EXPORTACIÓN FINALIZADA");
                    SapProcessor.Log("SAP2000 restaurado y visible");
                    SapProcessor.Log("========================================");

                    restoreUi();
                }
            }
        }

        private static void SetControlsEnabledRecursive(Control parent, bool enabled)
        {
            if (parent == null) return;

            foreach (Control c in parent.Controls)
            {
                // Mantener navegación disponible
                if (c is TabControl)
                {
                    SetControlsEnabledRecursive(c, enabled);
                    continue;
                }

                // No tocar el textbox multilínea (log)
                if (c is TextBox tb && tb.Multiline)
                {
                    // leave as-is
                }
                else
                {
                    try { c.Enabled = enabled; } catch { }
                }

                if (c.HasChildren)
                    SetControlsEnabledRecursive(c, enabled);
            }
        }
    }
}