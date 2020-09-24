using System;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;
using System.Diagnostics;
using System.Data.SQLite;
using System.Data.Common;

namespace Currency
{
    class DataBase
    {
        private string dbFileName;
        private SQLiteConnection connection;
        private SQLiteCommand command;
        private string tableName;

        public DataBase()
        {
            this.dbFileName = "data.sqlite";
            this.connection = new SQLiteConnection($"Data Source={dbFileName};Version=3;");
            this.command = new SQLiteCommand();
            this.tableName = "Currency";
            connection.Open();
            command.Connection = connection;
            CreateDataBase();
        }

        public string Select(string date, string charCode)
        {
            string ret = null;
            try
            {
                //I thought that Select request would be enough
                string sqlQuery = $"SELECT DISTINCT value FROM {tableName} WHERE (date = '{date}' AND charcode = '{charCode}')";
                command = new SQLiteCommand(sqlQuery, connection);
                var data = command.ExecuteReader();
                while (data.Read())
                {
                    ret += data.GetString(0);
                }
                return ret;

            }
            catch (SQLiteException ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public void CreateDataBase()
        {
            if (!File.Exists(dbFileName))
            {
                SQLiteConnection.CreateFile(dbFileName);
            }
            try
            {
                command.CommandText = $"CREATE TABLE IF NOT EXISTS {tableName} (date TEXT, value TEXT, charcode TEXT, CONSTRAINT Cur UNIQUE  (date,value,charcode))";
                SQLiteFunction.RegisterFunction(typeof(string));
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine($"Table {tableName} didn't create");
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public void Insert(string value, string charCode, string date)
        {
            try
            {
                command.CommandText = $"INSERT OR IGNORE INTO {tableName} (date, charcode, value) VALUES ('{date}', '{charCode}','{value}')";
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Data didn't insert");
                Console.WriteLine("Error: " + ex.Message);
            }

        }

        public void DropTable()
        {
            command.Connection = connection;
            command.CommandText = $"DROP TABLE {tableName}";
            command.ExecuteNonQuery();
        }
    }
}
