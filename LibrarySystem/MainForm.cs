using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace LibrarySystem
{
    public partial class MainForm : Form
    {
        private readonly string _userName;
        private DataTable _books;

        public MainForm(string userName)
        {
            _userName = userName;
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            TableChanged(this, EventArgs.Empty);
        }

        private int CurrentVersion
        {
            get { return mainTabs.SelectedIndex; }
        }

        private void TableChanged(object sender, EventArgs e)
        {
            if (mainTabs.SelectedTab == null) return;
            grid.Parent = mainTabs.SelectedTab;
            grid.Dock = DockStyle.Fill;
            grid.BringToFront();
            var experiment = CurrentVersion == 2;
            searchBox.Visible = !experiment;
            addButton.Visible = !experiment;
            editButton.Visible = !experiment;
            deleteButton.Visible = !experiment;
            runExperimentButton.Visible = experiment;
            if (experiment)
            {
                _books = null;
                grid.DataSource = null;
                statusLabel.Text = "Вычислительный эксперимент | библиотекарь: " + _userName;
            }
            else if (CurrentVersion == 0) Reload();
            else ReloadIndexed();
        }

        private void Reload()
        {
            try
            {
                _books = AccessDatabase.GetBooks("", false);
                ApplySearch();
            }
            catch (Exception ex) { Error(ex); }
        }

        private void ApplySearch()
        {
            if (_books == null || CurrentVersion == 2) return;
            var query = searchBox.Text.Trim().Replace("'", "''").Replace("%", "[%]").Replace("*", "[*]");
            _books.DefaultView.RowFilter = "";
            if (query.Length > 0)
            _books.DefaultView.RowFilter = "[Название] LIKE '*" + query + "*' OR [Автор] LIKE '*" + query + "*'";
            grid.DataSource = _books.DefaultView;
            TranslateHeaders();
            statusLabel.Text = grid.Rows.Count + " записей | библиотекарь: " + _userName + " | " + AccessDatabase.DatabasePathForDisplay;
        }

        private void ReloadIndexed()
        {
            try
            {
                grid.DataSource = AccessDatabase.GetBooks(searchBox.Text, true);
                TranslateHeaders();
                statusLabel.Text = grid.Rows.Count + " записей | поиск по индексу | библиотекарь: " + _userName;
            }
            catch (Exception ex) { Error(ex); }
        }

        private void TranslateHeaders()
        {
            if (grid.Columns.Contains("Код"))
            {
                grid.Columns["Код"].Visible = false;
                grid.Columns["Код"].HeaderText = "№";
            }
            if (grid.Columns.Contains("Название")) grid.Columns["Название"].HeaderText = "Название";
            if (grid.Columns.Contains("Автор")) grid.Columns["Автор"].HeaderText = "Автор";
            if (grid.Columns.Contains("ГодИздания")) grid.Columns["ГодИздания"].HeaderText = "Год";
            if (grid.Columns.Contains("Жанр")) grid.Columns["Жанр"].HeaderText = "Жанр";
            if (grid.Columns.Contains("ВНаличии")) grid.Columns["ВНаличии"].HeaderText = "В наличии";
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void SearchChanged(object sender, EventArgs e)
        {
            if (CurrentVersion == 1) ReloadIndexed();
            else ApplySearch();
        }

        private void RefreshClicked(object sender, EventArgs e)
        {
            if (CurrentVersion == 1) ReloadIndexed();
            else if (CurrentVersion == 0) Reload();
        }

        private void AddClicked(object sender, EventArgs e) { EditRecord(null); }

        private void EditClicked(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null) return;
            EditRecord(grid.CurrentRow);
        }

        private void DoubleClicked(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && grid.CurrentRow != null) EditRecord(grid.CurrentRow);
        }

        private void EditRecord(DataGridViewRow selected)
        {
            var view = selected == null ? null : selected.DataBoundItem as DataRowView;
            try
            {
                using (var editor = new BookEditorForm(view == null ? null : view.Row))
                {
                    if (editor.ShowDialog(this) != DialogResult.OK) return;
                    if (view == null) AccessDatabase.SaveBook(0, editor.TitleValue, editor.AuthorValue, editor.YearValue, editor.GenreValue, editor.AvailableValue);
                    else AccessDatabase.SaveBook(Convert.ToInt32(view["Код"]), editor.TitleValue, editor.AuthorValue, editor.YearValue, editor.GenreValue, editor.AvailableValue);
                    if (CurrentVersion == 1) ReloadIndexed(); else Reload();
                }
            }
            catch (Exception ex) { Error(ex); }
        }

        private void DeleteClicked(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null) return;
            var view = grid.CurrentRow.DataBoundItem as DataRowView;
            if (view == null) return;
            if (MessageBox.Show("Удалить выбранную книгу?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                AccessDatabase.DeleteBook(Convert.ToInt32(view["Код"]));
                if (CurrentVersion == 1) ReloadIndexed(); else Reload();
            }
            catch (Exception ex) { Error(ex); }
        }

        private void RunExperimentClicked(object sender, EventArgs e)
        {
            runExperimentButton.Enabled = false;
            statusLabel.Text = "Выполняется эксперимент...";
            try
            {
                var result = AccessDatabase.RunExperiment();
                grid.DataSource = result;
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                statusLabel.Text = "Замеры для 1 000, 5 000 и 10 000 записей | библиотекарь: " + _userName;
            }
            catch (Exception ex) { Error(ex); }
            finally { runExperimentButton.Enabled = true; }
        }

        private static void Error(Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка базы данных", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    internal sealed class BookEditorForm : Form
    {
        private readonly DataRow _book;
        private readonly TextBox _titleBox = new TextBox();
        private readonly TextBox _authorBox = new TextBox();
        private readonly TextBox _yearBox = new TextBox();
        private readonly TextBox _genreBox = new TextBox();
        private readonly CheckBox _availableBox = new CheckBox();

        public string TitleValue { get; private set; }
        public string AuthorValue { get; private set; }
        public int? YearValue { get; private set; }
        public string GenreValue { get; private set; }
        public bool AvailableValue { get; private set; }

        public BookEditorForm(DataRow book)
        {
            _book = book;
            Text = book == null ? "Добавить книгу" : "Изменить книгу";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 520;
            MinimumSize = new Size(480, 340);
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            Controls.Add(layout);
            AddField(layout, "Название", _titleBox);
            AddField(layout, "Автор", _authorBox);
            AddField(layout, "Год издания", _yearBox);
            AddField(layout, "Жанр", _genreBox);
            _availableBox.Text = "Книга в наличии";
            _availableBox.Checked = true;
            layout.Controls.Add(new Label { AutoSize = true, Anchor = AnchorStyles.Left, Text = "Статус", Margin = new Padding(0, 8, 12, 8) });
            layout.Controls.Add(_availableBox);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 10, 0, 0) };
            var saveButton = new Button { Text = "Сохранить", AutoSize = true, DialogResult = DialogResult.None };
            var cancelButton = new Button { Text = "Отмена", AutoSize = true, DialogResult = DialogResult.Cancel };
            saveButton.Click += SaveClicked;
            buttons.Controls.Add(saveButton);
            buttons.Controls.Add(cancelButton);
            layout.Controls.Add(buttons);
            layout.SetColumnSpan(buttons, 2);
            AcceptButton = saveButton;
            CancelButton = cancelButton;
            if (_book != null) LoadBook();
        }

        private static void AddField(TableLayoutPanel layout, string caption, Control input)
        {
            var label = new Label { Text = caption, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 12, 8) };
            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(0, 4, 0, 4);
            layout.Controls.Add(label);
            layout.Controls.Add(input);
        }

        private void LoadBook()
        {
            _titleBox.Text = Convert.ToString(_book["Название"]);
            _authorBox.Text = Convert.ToString(_book["Автор"]);
            _yearBox.Text = _book["ГодИздания"] == DBNull.Value ? "" : Convert.ToString(_book["ГодИздания"]);
            _genreBox.Text = _book["Жанр"] == DBNull.Value ? "" : Convert.ToString(_book["Жанр"]);
            _availableBox.Checked = Convert.ToBoolean(_book["ВНаличии"]);
        }

        private void SaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_titleBox.Text) || string.IsNullOrWhiteSpace(_authorBox.Text))
            {
                MessageBox.Show("Заполните название и автора книги.", "Проверьте данные", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int year;
            if (_yearBox.Text.Trim().Length > 0 && (!int.TryParse(_yearBox.Text.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out year) || year < 0 || year > DateTime.Now.Year))
            {
                MessageBox.Show("Введите корректный год издания.", "Проверьте данные", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _yearBox.Focus();
                return;
            }
            TitleValue = _titleBox.Text.Trim();
            AuthorValue = _authorBox.Text.Trim();
            YearValue = _yearBox.Text.Trim().Length == 0 ? (int?)null : int.Parse(_yearBox.Text.Trim());
            GenreValue = _genreBox.Text.Trim();
            AvailableValue = _availableBox.Checked;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
