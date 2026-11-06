using System.Data.SQLite;

namespace DataWebService.Database
{
    internal static class DBConnection
    {
        private static string connectionString = "";

        public const string TimeFormat = "yyyy-MM-dd HH:mm:ss";

        public static void Configure(string dbPath)
        {
            connectionString = $"Data Source={dbPath};Version=3;Foreign Keys=True"; // foreign keys config 
        }

        public static SQLiteConnection Open()
        {
            if (connectionString == "")
            {
                throw new InvalidOperationException("DBConnection.Configure must be called at startup.");
            }
            var conn = new SQLiteConnection(connectionString);
            conn.Open();
            return conn;
        }
    }
}