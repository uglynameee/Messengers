using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Security.Cryptography;

namespace LibrarySystem
{
    internal static class AccessDatabase
    {
        private const string DatabaseFileName = "Microsoft Access База данных.accdb";
        private static readonly string DatabasePath = FindDatabasePath();

        public static string DatabasePathForDisplay
        {
            get { return DatabasePath; }
        }

        private static string ConnectionString
        {
            get { return "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + DatabasePath + ";Persist Security Info=False;"; }
        }

        private static string FindDatabasePath()
        {
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, DatabaseFileName);
                if (File.Exists(candidate)) return candidate;
                directory = directory.Parent;
            }
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DatabaseFileName);
        }

        public static void Initialize()
        {
            if (!File.Exists(DatabasePath)) CreateDatabaseFile();
            using (var connection = Open())
            {
                if (!TableExists(connection, "Библиотекари"))
                    Execute(connection, "CREATE TABLE [Библиотекари] ([Код] AUTOINCREMENT PRIMARY KEY, [Логин] TEXT(50) NOT NULL, [Соль] TEXT(100) NOT NULL, [ХешПароля] TEXT(100) NOT NULL)");
                if (!TableExists(connection, "Книги"))
                    Execute(connection, "CREATE TABLE [Книги] ([Код] AUTOINCREMENT PRIMARY KEY, [Название] TEXT(255) NOT NULL, [Автор] TEXT(150) NOT NULL, [ГодИздания] INTEGER, [Жанр] TEXT(100), [ВНаличии] YESNO NOT NULL)");
                if (!IndexExists(connection, "IX_Книги_Название")) Execute(connection, "CREATE INDEX [IX_Книги_Название] ON [Книги] ([Название])");
                if (!IndexExists(connection, "IX_Книги_Автор")) Execute(connection, "CREATE INDEX [IX_Книги_Автор] ON [Книги] ([Автор])");
                if (!TableExists(connection, "ТестовыеЗаписи"))
                    Execute(connection, "CREATE TABLE [ТестовыеЗаписи] ([Код] AUTOINCREMENT PRIMARY KEY, [КлючПоиска] TEXT(100) NOT NULL)");
                if (!IndexExists(connection, "IX_ТестовыеЗаписи_Ключ"))
                    Execute(connection, "CREATE INDEX [IX_ТестовыеЗаписи_Ключ] ON [ТестовыеЗаписи] ([КлючПоиска])");
                SeedLibrarian(connection);
                SeedBooks(connection);
            }
        }

        private static bool TableExists(OleDbConnection connection, string tableName)
        {
            var tables = connection.GetSchema("Tables");
            foreach (DataRow row in tables.Rows)
                if (string.Equals(Convert.ToString(row["TABLE_NAME"]), tableName, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static bool IndexExists(OleDbConnection connection, string indexName)
        {
            try
            {
                var indexes = connection.GetSchema("Indexes");
                foreach (DataRow row in indexes.Rows)
                    if (string.Equals(Convert.ToString(row["INDEX_NAME"]), indexName, StringComparison.OrdinalIgnoreCase)) return true;
            }
            catch (ArgumentException) { }
            return false;
        }

        private static void CreateDatabaseFile()
        {
            var catalogType = Type.GetTypeFromProgID("ADOX.Catalog");
            if (catalogType == null)
                throw new InvalidOperationException("Компонент ADOX не найден. Создайте пустую базу " + DatabaseFileName + " в Microsoft Access рядом с программой.");
            dynamic catalog = Activator.CreateInstance(catalogType);
            try { catalog.Create(ConnectionString); }
            finally
            {
                if (catalog != null && System.Runtime.InteropServices.Marshal.IsComObject(catalog))
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(catalog);
            }
        }

        public static OleDbConnection Open()
        {
            var connection = new OleDbConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        private static void Execute(OleDbConnection connection, string sql)
        {
            try
            {
                using (var command = new OleDbCommand(sql, connection)) command.ExecuteNonQuery();
            }
            catch (OleDbException ex)
            {
                if (!ex.Message.Contains("already exists") && !ex.Message.Contains("уже существует")) throw;
            }
        }

        private static void SeedLibrarian(OleDbConnection connection)
        {
            using (var command = new OleDbCommand("SELECT COUNT(*) FROM [Библиотекари]", connection))
            {
                if (Convert.ToInt32(command.ExecuteScalar()) > 0) return;
            }
            var salt = PasswordHasher.NewSalt();
            using (var command = new OleDbCommand("INSERT INTO [Библиотекари] ([Логин], [Соль], [ХешПароля]) VALUES (?, ?, ?)", connection))
            {
                command.Parameters.AddWithValue("@user", "librarian");
                command.Parameters.AddWithValue("@salt", salt);
                command.Parameters.AddWithValue("@hash", PasswordHasher.Hash("Library123!", salt));
                command.ExecuteNonQuery();
            }
        }

        private static void SeedBooks(OleDbConnection connection)
        {
            using (var command = new OleDbCommand("SELECT COUNT(*) FROM [Книги]", connection))
                if (Convert.ToInt32(command.ExecuteScalar()) > 0) return;

            var books = new[]
            {
                new object[] { "Мастер и Маргарита", "Михаил Булгаков", 1967, "Роман", true },
                new object[] { "Преступление и наказание", "Федор Достоевский", 1866, "Роман", true },
                new object[] { "Война и мир", "Лев Толстой", 1869, "Роман", false },
                new object[] { "Три товарища", "Эрих Мария Ремарк", 1936, "Проза", true },
                new object[] { "Цветы для Элджернона", "Дэниел Киз", 1966, "Фантастика", true }
            };
            foreach (var book in books)
            {
                using (var command = new OleDbCommand("INSERT INTO [Книги] ([Название], [Автор], [ГодИздания], [Жанр], [ВНаличии]) VALUES (?, ?, ?, ?, ?)", connection))
                {
                    foreach (var value in book) command.Parameters.AddWithValue("@p", value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static bool Authenticate(string userName, string password)
        {
            using (var connection = Open())
            using (var command = new OleDbCommand("SELECT [Соль], [ХешПароля] FROM [Библиотекари] WHERE [Логин] = ?", connection))
            {
                command.Parameters.AddWithValue("@user", userName);
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return false;
                    var actual = PasswordHasher.Hash(password, Convert.ToString(reader["Соль"]));
                    var expected = Convert.FromBase64String(Convert.ToString(reader["ХешПароля"]));
                    var supplied = Convert.FromBase64String(actual);
                    if (expected.Length != supplied.Length) return false;
                    var difference = 0;
                    for (var i = 0; i < expected.Length; i++) difference |= expected[i] ^ supplied[i];
                    return difference == 0;
                }
            }
        }

        public static DataTable GetBooks(string search, bool indexed)
        {
            using (var connection = Open())
            using (var command = connection.CreateCommand())
            {
                var text = (search ?? "").Trim();
                if (indexed && text.Length > 0)
                {
                    command.CommandText = "SELECT [Код], [Название], [Автор], [ГодИздания], [Жанр], [ВНаличии] FROM [Книги] WHERE [Название] LIKE ? OR [Автор] LIKE ? ORDER BY [Название]";
                    command.Parameters.AddWithValue("@title", text + "%");
                    command.Parameters.AddWithValue("@author", text + "%");
                }
                else
                {
                    command.CommandText = "SELECT [Код], [Название], [Автор], [ГодИздания], [Жанр], [ВНаличии] FROM [Книги] ORDER BY [Название]";
                }
                var table = new DataTable();
                using (var adapter = new OleDbDataAdapter(command)) adapter.Fill(table);
                if (!indexed && text.Length > 0)
                {
                    var view = new DataView(table);
                    view.RowFilter = "Название LIKE '%" + EscapeFilter(text) + "%' OR Автор LIKE '%" + EscapeFilter(text) + "%'";
                    return view.ToTable();
                }
                return table;
            }
        }

        private static string EscapeFilter(string value)
        {
            return value.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]");
        }

        public static void SaveBook(int id, string title, string author, int? year, string genre, bool available)
        {
            using (var connection = Open())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = id == 0
                    ? "INSERT INTO [Книги] ([Название], [Автор], [ГодИздания], [Жанр], [ВНаличии]) VALUES (?, ?, ?, ?, ?)"
                    : "UPDATE [Книги] SET [Название]=?, [Автор]=?, [ГодИздания]=?, [Жанр]=?, [ВНаличии]=? WHERE [Код]=?";
                command.Parameters.AddWithValue("@title", title);
                command.Parameters.AddWithValue("@author", author);
                command.Parameters.AddWithValue("@year", year.HasValue ? (object)year.Value : DBNull.Value);
                command.Parameters.AddWithValue("@genre", genre ?? "");
                command.Parameters.AddWithValue("@available", available);
                if (id != 0) command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
        }

        public static void DeleteBook(int id)
        {
            using (var connection = Open())
            using (var command = new OleDbCommand("DELETE FROM [Книги] WHERE [Код] = ?", connection))
            {
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
        }

        public static DataTable RunExperiment()
        {
            var result = new DataTable();
            result.Columns.Add("Записей", typeof(int));
            result.Columns.Add("Полный перебор, мс", typeof(double));
            result.Columns.Add("По индексу, мс", typeof(double));
            var sizes = new[] { 1000, 5000, 10000 };
            using (var connection = Open())
            {
                Execute(connection, "DELETE FROM [ТестовыеЗаписи]");
                var inserted = 0;
                foreach (var size in sizes)
                {
                    InsertBenchmarkRows(connection, inserted, size - inserted);
                    inserted = size;
                    var target = "КНИГА-" + (size - 1).ToString("D6");
                    var scan = MeasureScan(connection, target);
                    var indexed = MeasureIndexed(connection, target);
                    result.Rows.Add(size, Math.Round(scan, 3), Math.Round(indexed, 3));
                }
                Execute(connection, "DELETE FROM [ТестовыеЗаписи]");
            }
            return result;
        }

        private static void InsertBenchmarkRows(OleDbConnection connection, int start, int count)
        {
            using (var transaction = connection.BeginTransaction())
            using (var command = new OleDbCommand("INSERT INTO [ТестовыеЗаписи] ([КлючПоиска]) VALUES (?)", connection, transaction))
            {
                var parameter = command.Parameters.Add("@key", OleDbType.VarWChar, 100);
                for (var i = start; i < start + count; i++)
                {
                    parameter.Value = "КНИГА-" + i.ToString("D6");
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
        }

        private static double MeasureScan(OleDbConnection connection, string target)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            using (var command = new OleDbCommand("SELECT [КлючПоиска] FROM [ТестовыеЗаписи]", connection))
            using (var reader = command.ExecuteReader())
                while (reader.Read()) if (string.Equals(Convert.ToString(reader[0]), target, StringComparison.OrdinalIgnoreCase)) { }
            timer.Stop();
            return timer.Elapsed.TotalMilliseconds;
        }

        private static double MeasureIndexed(OleDbConnection connection, string target)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            using (var command = new OleDbCommand("SELECT [КлючПоиска] FROM [ТестовыеЗаписи] WHERE [КлючПоиска] = ?", connection))
            {
                command.Parameters.AddWithValue("@key", target);
                command.ExecuteScalar();
            }
            timer.Stop();
            return timer.Elapsed.TotalMilliseconds;
        }
    }
}
