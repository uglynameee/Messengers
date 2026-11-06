namespace LibrarySystem
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label heading;
        private System.Windows.Forms.TabControl mainTabs;
        private System.Windows.Forms.TabPage versionOneTab;
        private System.Windows.Forms.TabPage versionTwoTab;
        private System.Windows.Forms.TabPage experimentTab;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button editButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button runExperimentButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.heading = new System.Windows.Forms.Label();
            this.mainTabs = new System.Windows.Forms.TabControl();
            this.versionOneTab = new System.Windows.Forms.TabPage();
            this.versionTwoTab = new System.Windows.Forms.TabPage();
            this.experimentTab = new System.Windows.Forms.TabPage();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.grid = new System.Windows.Forms.DataGridView();
            this.statusLabel = new System.Windows.Forms.Label();
            this.addButton = new System.Windows.Forms.Button();
            this.editButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.runExperimentButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            // heading
            this.heading.AutoSize = true;
            this.heading.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.heading.Location = new System.Drawing.Point(24, 18);
            this.heading.Name = "heading";
            this.heading.Size = new System.Drawing.Size(280, 32);
            this.heading.Text = "Система учета книг";
            // tabs
            this.mainTabs.Controls.Add(this.versionOneTab);
            this.mainTabs.Controls.Add(this.versionTwoTab);
            this.mainTabs.Controls.Add(this.experimentTab);
            this.mainTabs.Location = new System.Drawing.Point(27, 115);
            this.mainTabs.Name = "mainTabs";
            this.mainTabs.SelectedIndex = 0;
            this.mainTabs.Size = new System.Drawing.Size(910, 430);
            this.mainTabs.SelectedIndexChanged += new System.EventHandler(this.TableChanged);
            this.versionOneTab.Location = new System.Drawing.Point(4, 24);
            this.versionOneTab.Name = "versionOneTab";
            this.versionOneTab.Padding = new System.Windows.Forms.Padding(3);
            this.versionOneTab.Size = new System.Drawing.Size(902, 402);
            this.versionOneTab.Tag = 0;
            this.versionOneTab.Text = "Версия 1 · базовая";
            this.versionOneTab.UseVisualStyleBackColor = true;
            this.versionTwoTab.Location = new System.Drawing.Point(4, 24);
            this.versionTwoTab.Name = "versionTwoTab";
            this.versionTwoTab.Padding = new System.Windows.Forms.Padding(3);
            this.versionTwoTab.Size = new System.Drawing.Size(902, 402);
            this.versionTwoTab.Tag = 1;
            this.versionTwoTab.Text = "Версия 2 · с индексом";
            this.versionTwoTab.UseVisualStyleBackColor = true;
            this.experimentTab.Location = new System.Drawing.Point(4, 24);
            this.experimentTab.Name = "experimentTab";
            this.experimentTab.Padding = new System.Windows.Forms.Padding(3);
            this.experimentTab.Size = new System.Drawing.Size(902, 402);
            this.experimentTab.Tag = 2;
            this.experimentTab.Text = "Вычислительный эксперимент";
            this.experimentTab.UseVisualStyleBackColor = true;
            // search
            this.searchBox.Location = new System.Drawing.Point(27, 72);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(210, 23);
            this.searchBox.TextChanged += new System.EventHandler(this.SearchChanged);
            // buttons
            this.addButton.Location = new System.Drawing.Point(250, 67);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(90, 34);
            this.addButton.Text = "Добавить";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.AddClicked);
            this.editButton.Location = new System.Drawing.Point(348, 67);
            this.editButton.Name = "editButton";
            this.editButton.Size = new System.Drawing.Size(90, 34);
            this.editButton.Text = "Изменить";
            this.editButton.UseVisualStyleBackColor = true;
            this.editButton.Click += new System.EventHandler(this.EditClicked);
            this.deleteButton.Location = new System.Drawing.Point(446, 67);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(90, 34);
            this.deleteButton.Text = "Удалить";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.DeleteClicked);
            this.refreshButton.Location = new System.Drawing.Point(544, 67);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(90, 34);
            this.refreshButton.Text = "Обновить";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.RefreshClicked);
            this.runExperimentButton.Location = new System.Drawing.Point(642, 67);
            this.runExperimentButton.Name = "runExperimentButton";
            this.runExperimentButton.Size = new System.Drawing.Size(220, 34);
            this.runExperimentButton.Text = "Запустить эксперимент";
            this.runExperimentButton.UseVisualStyleBackColor = true;
            this.runExperimentButton.Visible = false;
            this.runExperimentButton.Click += new System.EventHandler(this.RunExperimentClicked);
            // grid
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.BackgroundColor = System.Drawing.Color.White;
            this.grid.Location = new System.Drawing.Point(3, 3);
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.RowHeadersVisible = false;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid.Size = new System.Drawing.Size(896, 396);
            this.grid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DoubleClicked);
            this.versionOneTab.Controls.Add(this.grid);
            // status
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(27, 559);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 15);
            // form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(964, 601);
            this.Controls.Add(this.heading);
            this.Controls.Add(this.searchBox);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.editButton);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.runExperimentButton);
            this.Controls.Add(this.mainTabs);
            this.Controls.Add(this.statusLabel);
            this.MinimumSize = new System.Drawing.Size(980, 640);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Библиотека | Версия 2";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.mainTabs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
