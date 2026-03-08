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

            // Cargas (placeholder)
            var menuCargas = new ToolStripMenuItem("Cargas");
            menuCargas.DropDownItems.Add(new ToolStripMenuItem("Configurar Cargas... (próximamente)"));

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
            var itemObtenerSismico = new ToolStripMenuItem("Obtener Datos &Sísmicos", null, menuObtenerSismico_Click);
            var itemObtenerDiseno = new ToolStripMenuItem("Obtener Datos de &Diseño", null, menuObtenerDiseno_Click);
            menuObtener.DropDownItems.Add(itemObtenerSismico);
            menuObtener.DropDownItems.Add(itemObtenerDiseno);

            // Análisis (placeholder)
            var menuAnalisis = new ToolStripMenuItem("&Análisis");
            menuAnalisis.DropDownItems.Add(new ToolStripMenuItem("Ver Resultados Sísmicos... (próximamente)"));
            menuAnalisis.DropDownItems.Add(new ToolStripMenuItem("Ver Derivas... (próximamente)"));
            menuAnalisis.DropDownItems.Add(new ToolStripMenuItem("Ver Modos... (próximamente)"));

            // Definir (placeholder)
            var menuDefinir = new ToolStripMenuItem("&Definir");
            menuDefinir.DropDownItems.Add(new ToolStripMenuItem("Elementos Estructurales... (próximamente)"));
            menuDefinir.DropDownItems.Add(new ToolStripMenuItem("Combinaciones de Carga... (próximamente)"));

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
                Text = "Comenzar:\n" +
                       "  1. Archivo → Nuevo Proyecto  (crear un proyecto)\n" +
                       "  2. Conectar → Conectar a SAP2000  (abrir SAP2000)\n" +
                       "  3. Archivo → Abrir Modelo SAP  (cargar un modelo .sdb)\n" +
                       "  4. Sismicidad → Parámetros Sísmicos  (configurar NEC/E030)\n" +
                       "  5. Run → Run Análisis  (ejecutar análisis)\n" +
                       "  6. Obtener → Obtener Datos Sísmicos  (extraer resultados)",
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
