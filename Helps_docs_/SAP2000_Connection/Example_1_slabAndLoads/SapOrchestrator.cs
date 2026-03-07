using System;
using App.Domain.SapSnapshotDomain.Entities;

namespace App.Infrastructure.Sap2000
{
    /// <summary>
    /// Orquesta una corrida completa contra SAP2000:
    /// - crea un runner STA (requerido por COM)
    /// - conecta/inicializa SAP2000
    /// - ejecuta un snapshot (sin lógica de negocio)
    /// - opcionalmente guarda el modelo
    /// - libera COM
    ///
    /// Este orquestador debe permanecer thin: coordina pasos y manejo de errores.
    /// </summary>
    public sealed class SapOrchestrator
    {
        private readonly SapSnapshotExecutor _executor;

        public SapOrchestrator(SapSnapshotExecutor executor)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        }

        /// <summary>
        /// Ejecuta el snapshot en una sesión de SAP2000.
        /// </summary>
        /// <param name="snapshot">Snapshot completo a aplicar.</param>
        /// <param name="saveAsPath">Ruta donde guardar (opcional).</param>
        /// <param name="visible">Si SAP2000 debe mostrarse.</param>
        public SapRunResult Run(SapBuildSnapshot snapshot, string saveAsPath = null, bool visible = false)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

            using (var runner = SapStaHost.CreateRunner())
            {
                var proc = new SapProcessor();

                try
                {
                    runner.Invoke(() =>
                    {
                        proc.ConnectAndInit(visible: visible);
                        _executor.Execute(proc, snapshot);

#if SAP2000_AVAILABLE
                        if (!string.IsNullOrWhiteSpace(saveAsPath))
                        {
                            proc.SapModel.File.Save(saveAsPath);
                        }
#endif
                    });

                    return SapRunResult.Success(proc.ConnectedProgId);
                }
                catch (Exception ex)
                {
                    return SapRunResult.Failure(proc.ConnectedProgId, ex);
                }
                finally
                {
                    try
                    {
                        runner.Invoke(proc.ReleaseCom);
                    }
                    catch
                    {
                        // best-effort cleanup
                    }
                }
            }
        }
    }

    public sealed class SapRunResult
    {
        private SapRunResult(bool ok, string connectedProgId, Exception error)
        {
            Ok = ok;
            ConnectedProgId = connectedProgId;
            Error = error;
        }

        public bool Ok { get; }
        public string ConnectedProgId { get; }
        public Exception Error { get; }

        public static SapRunResult Success(string connectedProgId) => new SapRunResult(true, connectedProgId, null);
        public static SapRunResult Failure(string connectedProgId, Exception error) => new SapRunResult(false, connectedProgId, error);
    }
}
