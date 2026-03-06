using System;
using System.Windows.Forms;
using App.Application.UseCases;

namespace App.WinForms.Forms
{
    public class NewProjectDialog : Form
    {
        private readonly CreateProjectUseCase _useCase;
        private TextBox _txtName, _txtCode, _txtDescription, _txtClient, _txtLocation;
        private TextBox _txtStoreys;
        private ComboBox _cmbDesignCode, _cmbStructuralSystem, _cmbBuildingUse;
        private Button _btnCreate, _btnCancel;

        public Guid CreatedProjectId { get; private set; }
        public string ProjectName { get; private set; }

        public NewProjectDialog(CreateProjectUseCase useCase)
        {
            _useCase = useCase ?? throw new ArgumentNullException(nameof(useCase));
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Nuevo Proyecto";
            Size = new System.Drawing.Size(480, 520);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                ColumnCount = 2,
                RowCount = 10
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;
            AddRow(layout, row++, "Nombre *:", _txtName = new TextBox { Dock = DockStyle.Fill });
            AddRow(layout, row++, "Código *:", _txtCode = new TextBox { Dock = DockStyle.Fill });
            AddRow(layout, row++, "Cliente:", _txtClient = new TextBox { Dock = DockStyle.Fill });
            AddRow(layout, row++, "Ubicación:", _txtLocation = new TextBox { Dock = DockStyle.Fill });
            AddRow(layout, row++, "Número de pisos:", _txtStoreys = new TextBox { Dock = DockStyle.Fill, Text = "5" });

            _cmbDesignCode = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbDesignCode.Items.AddRange(new object[] { "ACI 318 / NEC-SE-HM", "ACI 318 / E060", "Eurocódigo 2", "Otro" });
            _cmbDesignCode.SelectedIndex = 0;
            AddRow(layout, row++, "Código de diseño:", _cmbDesignCode);

            _cmbStructuralSystem = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbStructuralSystem.Items.AddRange(new object[] { "Muros estructurales", "Pórticos de concreto", "Dual (muros + pórticos)", "Pórticos metálicos", "Otro" });
            _cmbStructuralSystem.SelectedIndex = 0;
            AddRow(layout, row++, "Sistema estructural:", _cmbStructuralSystem);

            _cmbBuildingUse = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbBuildingUse.Items.AddRange(new object[] { "Vivienda", "Comercial", "Hospitales / Esencial", "Industrial", "Educativo" });
            _cmbBuildingUse.SelectedIndex = 0;
            AddRow(layout, row++, "Uso del edificio:", _cmbBuildingUse);

            _txtDescription = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 60 };
            AddRow(layout, row++, "Descripción:", _txtDescription);

            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 8, 0, 0)
            };
            _btnCancel = new Button { Text = "Cancelar", Width = 90, DialogResult = DialogResult.Cancel };
            _btnCreate = new Button { Text = "Crear", Width = 90 };
            _btnCreate.Click += BtnCreate_Click;
            btnPanel.Controls.Add(_btnCancel);
            btnPanel.Controls.Add(_btnCreate);
            layout.Controls.Add(btnPanel, 0, row);
            layout.SetColumnSpan(btnPanel, 2);

            this.Controls.Add(layout);
            this.AcceptButton = _btnCreate;
            this.CancelButton = _btnCancel;
        }

        private void AddRow(TableLayoutPanel panel, int row, string label, Control control)
        {
            panel.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleRight }, 0, row);
            panel.Controls.Add(control, 1, row);
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtName.Text))
            {
                MessageBox.Show("Ingrese el nombre del proyecto.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtName.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(_txtCode.Text))
            {
                MessageBox.Show("Ingrese el código del proyecto.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtCode.Focus();
                return;
            }

            int.TryParse(_txtStoreys.Text, out int storeys);

            var request = new CreateProjectRequest
            {
                Name = _txtName.Text.Trim(),
                Code = _txtCode.Text.Trim().ToUpperInvariant(),
                Description = _txtDescription.Text.Trim(),
                Client = _txtClient.Text.Trim(),
                Location = _txtLocation.Text.Trim(),
                DesignCode = _cmbDesignCode.SelectedItem?.ToString(),
                StructuralSystem = _cmbStructuralSystem.SelectedItem?.ToString(),
                BuildingUse = _cmbBuildingUse.SelectedItem?.ToString(),
                NumberOfStoreys = storeys
            };

            var response = _useCase.Execute(request);
            if (response.Success)
            {
                CreatedProjectId = response.ProjectId;
                ProjectName = request.Name;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(response.ErrorMessage, "Error al crear proyecto",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
