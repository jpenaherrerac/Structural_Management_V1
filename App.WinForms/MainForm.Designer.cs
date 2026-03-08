using System.Windows.Forms;

namespace App.WinForms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Text = "Structural Management V1";
            this.MinimumSize = new System.Drawing.Size(1024, 700);
            this.Size = new System.Drawing.Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // ── Menu bar ────────────────────────────────────────────────────────
            var menuBar = new MenuStrip();
            this.MainMenuStrip = menuBar;

            // Archivo
            var menuArchivo = new ToolStripMenuItem("&Archivo");
            var itemNuevo = new ToolStripMenuItem("&Nuevo Proyecto...", null, menuNuevoProyecto_Click);
            var itemAbrir = new ToolStripMenuItem("&Abrir Modelo SAP...", null, menuAbrir_Click);
            var itemGuardar = new ToolStripMenuItem("&Guardar Modelo", null, menuGuardar_Click);
            var itemSalir = new ToolStripMenuItem("&Salir", null, menuSalir_Click);
            menuArchivo.DropDownItems.Add(itemNuevo);
            menuArchivo.DropDownItems.Add(itemAbrir);
            menuArchivo.DropDownItems.Add(itemGuardar);
            menuArchivo.DropDownItems.Add(new ToolStripSeparator());
            menuArchivo.DropDownItems.Add(itemSalir);

            // Conectar
            var menuConectar = new ToolStripMenuItem("&Conectar");
            var itemConectar = new ToolStripMenuItem("Conectar a SAP2000...", null, menuConectarSAP_Click);
            var itemDesconectar = new ToolStripMenuItem("Desconectar SAP2000", null, menuDesconectarSAP_Click);
            var itemCambiarSesion = new ToolStripMenuItem("Cambiar sesión SAP2000...", null, menuCambiarSesionSAP_Click);
            menuConectar.DropDownItems.Add(itemConectar);
            menuConectar.DropDownItems.Add(itemDesconectar);
            menuConectar.DropDownItems.Add(new ToolStripSeparator());
            menuConectar.DropDownItems.Add(itemCambiarSesion);

            // Cargas
            var menuCargas = new ToolStripMenuItem("Cargas");
            var itemConfigurarCargas = new ToolStripMenuItem("&Configurar Cargas...", null, menuConfigurarCargas_Click);
            menuCargas.DropDownItems.Add(itemConfigurarCargas);

            // Sismicidad
            var menuSismicidad = new ToolStripMenuItem("&Sismicidad");
            var itemSismicidad = new ToolStripMenuItem("Parámetros Sísmicos...", null, menuSismicidad_Click);
            menuSismicidad.DropDownItems.Add(itemSismicidad);

            // Run
            var menuRun = new ToolStripMenuItem("&Run");
            var itemRunAnalysis = new ToolStripMenuItem("Run &Análisis", null, menuRunAnalysis_Click);
            var itemRunDesign = new ToolStripMenuItem("Run &Diseño", null, menuRunDesign_Click);
            menuRun.DropDownItems.Add(itemRunAnalysis);
            menuRun.DropDownItems.Add(itemRunDesign);

            // Obtener
            var menuObtener = new ToolStripMenuItem("&Obtener");
            var itemObtenerSismico = new ToolStripMenuItem("Obtener Datos &Sísmicos", null, menuObtenerSismico_Advanced_Click);
            var itemObtenerDiseno = new ToolStripMenuItem("Obtener Datos de &Diseño", null, menuObtenerDiseno_Click);
            menuObtener.DropDownItems.Add(itemObtenerSismico);
            menuObtener.DropDownItems.Add(itemObtenerDiseno);

            // Análisis
            var menuAnalisis = new ToolStripMenuItem("&Análisis");
            var itemResultadosSismicos = new ToolStripMenuItem("Ver Resultados &Sísmicos...", null, menuResultadosSismicos_Click);
            menuAnalisis.DropDownItems.Add(itemResultadosSismicos);

            // Definir
            var menuDefinir = new ToolStripMenuItem("&Definir");
            var itemGrupos = new ToolStripMenuItem("&Grupos (Prefijos)...", null, menuGrupos_Click);
            menuDefinir.DropDownItems.Add(itemGrupos);
            menuDefinir.DropDownItems.Add(new ToolStripSeparator());
            var itemVigas = new ToolStripMenuItem("&Vigas...", null, menuVigas_Click);
            var itemColumnas = new ToolStripMenuItem("&Columnas...", null, menuColumnas_Click);
            var itemMuros = new ToolStripMenuItem("&Muros de Corte...", null, menuMuros_Click);
            var itemLosas = new ToolStripMenuItem("&Losas...", null, menuLosas_Click);
            menuDefinir.DropDownItems.Add(itemVigas);
            menuDefinir.DropDownItems.Add(itemColumnas);
            menuDefinir.DropDownItems.Add(itemMuros);
            menuDefinir.DropDownItems.Add(itemLosas);

            menuBar.Items.Add(menuArchivo);
            menuBar.Items.Add(menuConectar);
            menuBar.Items.Add(menuCargas);
            menuBar.Items.Add(menuSismicidad);
            menuBar.Items.Add(menuRun);
            menuBar.Items.Add(menuObtener);
            menuBar.Items.Add(menuAnalisis);
            menuBar.Items.Add(menuDefinir);

            // ── Status bar ───────────────────────────────────────────────────────
            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Listo.")
            {
                Spring = true,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            _sapStatusLabel = new ToolStripStatusLabel("SAP2000: Desconectado")
            {
                ForeColor = System.Drawing.Color.Red
            };
            _statusStrip.Items.Add(_statusLabel);
            _statusStrip.Items.Add(new ToolStripSeparator());
            _iterationLabel = new ToolStripStatusLabel("Iteración: 1 | Inicio")
            {
                ForeColor = System.Drawing.Color.FromArgb(60, 90, 140)
            };
            _statusStrip.Items.Add(_iterationLabel);
            _statusStrip.Items.Add(new ToolStripSeparator());
            _statusStrip.Items.Add(_sapStatusLabel);

            // ── Panel de bienvenida ──────────────────────────────────────────────
            var welcomePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(240, 244, 248)
            };

            var lblTitle = new Label
            {
                Text = "Structural Management V1",
                Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(30, 60, 100),
                AutoSize = true,
                Location = new System.Drawing.Point(40, 80)
            };

            var lblSubtitle = new Label
            {
                Text = "Gestión de proyectos de ingeniería estructural integrada con SAP2000",
                Font = new System.Drawing.Font("Segoe UI", 11F),
                ForeColor = System.Drawing.Color.FromArgb(80, 100, 130),
                AutoSize = true,
                Location = new System.Drawing.Point(40, 130)
            };

            var lblInstructions = new Label
            {
                Text = "Flujo de trabajo iterativo:\n" +
                       "  1. Archivo → Nuevo Proyecto  (crear un proyecto)\n" +
                       "  2. Conectar → Conectar a SAP2000  (abrir SAP2000)\n" +
                       "  3. Archivo → Abrir Modelo SAP  (cargar un modelo .sdb)\n" +
                       "  4. Sismicidad → Parámetros Sísmicos  (configurar E030)\n" +
                       "  5. Cargas → Configurar Cargas  (aplicar al modelo SAP2000)\n" +
                       "  6. Run → Run Análisis  (ejecutar análisis)\n" +
                       "  7. Obtener → Obtener Datos Sísmicos  (extraer resultados)\n" +
                       "  8. Análisis → Ver Resultados Sísmicos  (evaluar criterios)\n" +
                       "  9. Ajustar parámetros y repetir desde paso 4 si es necesario",
                Font = new System.Drawing.Font("Segoe UI", 10F),
                ForeColor = System.Drawing.Color.FromArgb(50, 70, 90),
                Location = new System.Drawing.Point(40, 200),
                Size = new System.Drawing.Size(700, 200)
            };

            welcomePanel.Controls.Add(lblTitle);
            welcomePanel.Controls.Add(lblSubtitle);
            welcomePanel.Controls.Add(lblInstructions);

            // ── Assemble form ────────────────────────────────────────────────────
            this.Controls.Add(welcomePanel);
            this.Controls.Add(_statusStrip);
            this.Controls.Add(menuBar);
        }
    }
}
