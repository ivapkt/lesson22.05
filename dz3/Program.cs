using Microsoft.Data.Sqlite;

namespace ContactsReport
{
    internal class Program
    {
        // Путь к файлу БД — рядом с приложением
        private static readonly string DbPath = Path.Combine(AppContext.BaseDirectory, "contacts.db");

        // Строка подключения хранится отдельно, как требуется в задании
        private static readonly string ConnectionString = $"Data Source={DbPath}";

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=== Мини-отчёт по контактам ===");
            Console.WriteLine($"Файл базы данных: {DbPath}");
            Console.WriteLine();

            // 1. Создаём таблицу, если её нет
            CreateTableIfNotExists();

            // 2. Заполняем тестовыми данными, только если таблица пустая
            SeedDataIfEmpty();

            // 3. Выводим отчёты (всё через ExecuteScalar)
            PrintReports();

            Console.WriteLine();
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        // Создание таблицы
        private static void CreateTableIfNotExists()
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS Contacts (
                    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
                    FullName   TEXT NOT NULL,
                    Phone      TEXT,
                    Email      TEXT,
                    Department TEXT
                );";

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        // Заполнение тестовыми данными
        private static void SeedDataIfEmpty()
        {
            // Проверим количество строк (ExecuteScalar)
            int count = ExecuteScalarInt("SELECT COUNT(*) FROM Contacts;");
            if (count > 0)
            {
                return;
            }

            const string insertSql = @"
                INSERT INTO Contacts (FullName, Phone, Email, Department) VALUES
                ('Анна Соколова',   '+7-900-100-10-01', 'anna@example.com',   'IT'),
                ('Борис Иванов',    '+7-900-100-10-02', 'boris@example.com',  'IT'),
                ('Виктор Петров',   NULL,                'viktor@example.com', 'HR'),
                ('Галина Кузнецова','+7-900-100-10-04', NULL,                 'HR'),
                ('Дмитрий Орлов',   '+7-900-100-10-05', 'dmitry@example.com', 'Sales'),
                ('Елена Новикова',  NULL,                'elena@example.com',  'Sales');";

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = insertSql;
            command.ExecuteNonQuery();
        }

        // Вывод отчётов
        private static void PrintReports()
        {
            int total = ExecuteScalarInt(
                "SELECT COUNT(*) FROM Contacts;");

            int withEmail = ExecuteScalarInt(
                "SELECT COUNT(*) FROM Contacts WHERE Email IS NOT NULL AND Email <> '';");

            int withoutPhone = ExecuteScalarInt(
                "SELECT COUNT(*) FROM Contacts WHERE Phone IS NULL OR Phone = '';");

            int itCount = ExecuteScalarInt(
                "SELECT COUNT(*) FROM Contacts WHERE Department = 'IT';");

            int departments = ExecuteScalarInt(
                "SELECT COUNT(DISTINCT Department) FROM Contacts WHERE Department IS NOT NULL;");

            int maxId = ExecuteScalarInt(
                "SELECT MAX(Id) FROM Contacts;");

            string firstByAlphabet = ExecuteScalarString(
                "SELECT FullName FROM Contacts ORDER BY FullName ASC LIMIT 1;");

            Console.WriteLine($"Всего контактов: {total}");
            Console.WriteLine($"Контактов с e-mail: {withEmail}");
            Console.WriteLine($"Контактов без телефона: {withoutPhone}");
            Console.WriteLine($"Контактов отдела IT: {itCount}");
            Console.WriteLine($"Различных отделов: {departments}");
            Console.WriteLine($"Максимальный Id: {maxId}");
            Console.WriteLine($"Первый по алфавиту: {firstByAlphabet}");
        }

        // Метод-помощник: ExecuteScalar int
        private static int ExecuteScalarInt(string sql)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;

            object? result = command.ExecuteScalar();

            if (result == null || result == DBNull.Value)
                return 0;

            return Convert.ToInt32(result);
        }

        // Метод-помощник: ExecuteScalar string
        private static string ExecuteScalarString(string sql)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;

            object? result = command.ExecuteScalar();

            if (result == null || result == DBNull.Value)
                return string.Empty;

            return result.ToString() ?? string.Empty;
        }
    }
}