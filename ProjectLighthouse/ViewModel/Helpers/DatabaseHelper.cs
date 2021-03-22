using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class DatabaseHelper
    {
        private static string dbFile = "C:\\Users\\xavie\\Desktop\\manufactureDB.db3";

        public static bool Insert<T>(T item)
        {
            bool result = false;

            using (SQLiteConnection conn = new SQLiteConnection(dbFile))
            {
                conn.CreateTable<T>();
                try
                {
                    int rows = conn.Insert(item);
                    if (rows > 0)
                    {
                        result = true;
                    }
                }
                catch (SQLiteException sqle)
                {
                    MessageBox.Show(sqle.Message.ToString());
                    result = false;
                }

            }

            return result;
        }

        public static bool Update<T>(T item)
        {
            bool result = false;

            using (SQLiteConnection conn = new SQLiteConnection(dbFile))
            {
                try
                {
                    conn.CreateTable<T>();
                    int rows = conn.Update(item);
                    if (rows > 0)
                    {
                        result = true;
                    }
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
            }

            return result;
        }

        public static bool Delete<T>(T item)
        {
            bool result = false;

            using (SQLiteConnection conn = new SQLiteConnection(dbFile))
            {
                conn.CreateTable<T>();
                int rows = conn.Delete(item);
                if (rows > 0)
                {
                    result = true;
                }
            }

            return result;
        }

        public static List<T> Read<T>() where T : new()
        {
            List<T> items;

            using (SQLiteConnection conn = new SQLiteConnection(dbFile))
            {
                conn.CreateTable<T>();
                items = conn.Table<T>().ToList();
            }

            return items;
        }
    }
}
