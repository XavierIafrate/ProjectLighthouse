using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class MySQLHelper
    {
        private static readonly string ConnectionString = "server=localhost; user id=root;database=test;Password=Lighthouse12!";

        public static void test()
        {
            MySqlConnection conn = new();

            conn.ConnectionString = ConnectionString;
            conn.Open();

            MySqlCommand command = new("INSERT INTO test_table(test_str) VALUES (@test_str)", conn);

            command.Parameters.AddWithValue("@test_str", "hello, sql!");
            command.ExecuteNonQuery();

            command = new("SELECT * FROM test_table", conn);

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Debug.WriteLine(reader["test_str"]);
                }
            }

            conn.Close();
        }
    }
}
