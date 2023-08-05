

using Contracts.Models;
using System;
using System.Data.SQLite;
using static IronPython.Modules._ast;
using System.Diagnostics.Metrics;

namespace Publisher.Services
{
    public class SqLiteRepo : ISqLiteRepo
    {
        private readonly SQLiteConnection sqlite_conn;
        public SqLiteRepo()
        {
            sqlite_conn = CreateConnection();
            CreateTable();
            //InsertData();
            //ReadData();
        }
        static SQLiteConnection CreateConnection()
        {

            SQLiteConnection sqlite_conn;
            sqlite_conn = new SQLiteConnection("Data Source=database.db; Version = 3; New = True; Compress = True; ");
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {

            }
            return sqlite_conn;
        }

        static void CreateTable()
        {
            using (SQLiteConnection connection = CreateConnection())
            {
                //connection.Open();

                string createTableQuery = "CREATE TABLE IF NOT EXISTS Joystics (ID INTEGER PRIMARY KEY AUTOINCREMENT, Time TEXT, Axis_1 TEXT, Axis_2 TEXT, Button_1 TEXT, Button_2 TEXT)";
                using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void InsertJoystic(Joystic joystic)
        {

            string insertQuery = "INSERT INTO Joystics (Time, Axis_1, Axis_2, Button_1, Button_2) VALUES (@Time, @Axis_1, @Axis_2, @Button_1, @Button_2)";
            using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, this.sqlite_conn))
            {
                cmd.Parameters.AddWithValue("@Time", joystic.time);
                cmd.Parameters.AddWithValue("@Axis_1", joystic.axis_1);
                cmd.Parameters.AddWithValue("@Axis_2", joystic.axis_2);
                cmd.Parameters.AddWithValue("@Button_1", joystic.button_1);
                cmd.Parameters.AddWithValue("@Button_2", joystic.button_2);

                cmd.ExecuteNonQuery();
            }
        }
        public List<Joystic> GetAllJoystics()
        {
            List<Joystic> joystics = new List<Joystic>();

            string selectQuery = "SELECT * FROM Joystics";
            using (SQLiteCommand cmd = new SQLiteCommand(selectQuery, this.sqlite_conn))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Joystic joystic = new Joystic()
                        {
                            id = Convert.ToInt32(reader["ID"]),
                            time = reader["Time"].ToString(),
                            axis_1 = reader["Axis_1"].ToString(),
                            axis_2 = reader["Axis_2"].ToString(),
                            button_1 = reader["Button_1"].ToString(),
                            button_2 = reader["Button_2"].ToString()
                        };

                        joystics.Add(joystic);
                    }
                }
            }

            return joystics;
        }

        public void InsertDataGet()
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = "INSERT INTO SampleTable(Col1, Col2) VALUES('Test Text ', 1); ";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_cmd.CommandText = "INSERT INTO SampleTable(Col1, Col2) VALUES('Test1 Text1 ', 2); ";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_cmd.CommandText = "INSERT INTO SampleTable(Col1, Col2) VALUES('Test2 Text2 ', 3); ";
            sqlite_cmd.ExecuteNonQuery();
            sqlite_cmd.CommandText = "INSERT INTO SampleTable1(Col1, Col2) VALUES('Test3 Text3 ', 3); ";
            sqlite_cmd.ExecuteNonQuery();

        }

        public void ReadDataById(int id)
        {

            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM SampleTable where";

            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                string myreader = sqlite_datareader.GetString(0);
                Console.WriteLine(myreader);
            }
            sqlite_conn.Close();
        }

        public void InsertAllJoystics(IList<Joystic> joystics)
        {
            try
            {
                // SQL query to insert data into the 'Joystics' table
                string insertQuery = "INSERT INTO Joystics (Time, Axis_1, Axis_2, Button_1, Button_2) VALUES (@Time, @Axis_1, @Axis_2, @Button_1, @Button_2)";

                using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, this.sqlite_conn))
                {
                    // Add parameters outside the loop for SQL query parameterization
                    cmd.Parameters.Add(new SQLiteParameter("@Time"));
                    cmd.Parameters.Add(new SQLiteParameter("@Axis_1"));
                    cmd.Parameters.Add(new SQLiteParameter("@Axis_2"));
                    cmd.Parameters.Add(new SQLiteParameter("@Button_1"));
                    cmd.Parameters.Add(new SQLiteParameter("@Button_2"));

                    // Execute the command multiple times in a single transaction
                    using (var transaction = this.sqlite_conn.BeginTransaction())
                    {
                        foreach (Joystic joystic in joystics)
                        {
                            // Set parameter values inside the loop for each Joystic object
                            cmd.Parameters["@Time"].Value = joystic.time;
                            cmd.Parameters["@Axis_1"].Value = joystic.axis_1;
                            cmd.Parameters["@Axis_2"].Value = joystic.axis_2;
                            cmd.Parameters["@Button_1"].Value = joystic.button_1;
                            cmd.Parameters["@Button_2"].Value = joystic.button_2;

                            // Execute the SQL command to insert data into the table
                            cmd.ExecuteNonQuery();
                        }

                        // Commit the transaction to persist changes in the database
                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
            }
        }
        public Joystic GetJoysticById(int id)
        {
            string selectQuery = "SELECT * FROM Joystics WHERE ID = @Id";
            using (SQLiteCommand cmd = new SQLiteCommand(selectQuery, this.sqlite_conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Joystic joystic = new Joystic()
                        {
                            id = Convert.ToInt32(reader["ID"]),
                            time = reader["Time"].ToString(),
                            axis_1 = reader["Axis_1"].ToString(),
                            axis_2 = reader["Axis_2"].ToString(),
                            button_1 = reader["Button_1"].ToString(),
                            button_2 = reader["Button_2"].ToString()
                        };

                        return joystic;
                    }
                }
            }

            return null;
        }
        public void ClearAllJoystics()
        {
            try
            {
                string deleteQuery = "DELETE FROM Joystics";
                using (SQLiteCommand cmd = new SQLiteCommand(deleteQuery, this.sqlite_conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}