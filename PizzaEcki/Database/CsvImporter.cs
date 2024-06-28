using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace PizzaEcki.Database
{
    public class CsvImporter
    {
        private readonly SqliteConnection _sqliteConnection;

        public CsvImporter(SqliteConnection sqliteConnection)
        {
            _sqliteConnection = sqliteConnection;
        }

        public bool IsTableEmpty(string tableName)
        {
            _sqliteConnection.Open();
            string sql = $"SELECT COUNT(*) FROM {tableName}";
            using (SqliteCommand command = new SqliteCommand(sql, _sqliteConnection))
            {
                long count = (long)command.ExecuteScalar();
                return count == 0;
            }
        }

        public void ImportCSVData(string csvFilePath, string tableName)
        {
            if (!File.Exists(csvFilePath))
            {
                Console.WriteLine($"CSV file not found at {csvFilePath}");
                return;
            }

            using (var reader = new StreamReader(csvFilePath))
            {
                var firstLine = reader.ReadLine(); // Read the header line
                if (string.IsNullOrWhiteSpace(firstLine))
                {
                    Console.WriteLine("CSV file is empty or invalid.");
                    return;
                }

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if (values.Length < 4) // Assuming 4 columns in the CSV
                    {
                        Console.WriteLine("CSV file has invalid data.");
                        continue;
                    }
                    var id = int.Parse(values[0]);
                    var strasse = values[1];
                    var ortsteil = values[2];
                    var plz = int.Parse(values[3]);
                    var vorwahl = int.Parse(values[4]);

                    InsertData(tableName, strasse, ortsteil, plz, vorwahl);
                }
            }
        }

        private void InsertData(string tableName, string strasse, string ortsteil, int plz, int vorwahl)
        {
            string sql = $"INSERT INTO {tableName} (Strasse, Ortsteil, PLZ, Vorwahl) VALUES (@strasse, @ortsteil, @plz, @vorwahl)";
            using (var command = new SqliteCommand(sql, _sqliteConnection))
            {
                command.Parameters.AddWithValue("@strasse", strasse);
                command.Parameters.AddWithValue("@ortsteil", ortsteil);
                command.Parameters.AddWithValue("@plz", plz);
                command.Parameters.AddWithValue("@vorwahl", vorwahl);
                command.ExecuteNonQuery();
            }
        }
    }
}
