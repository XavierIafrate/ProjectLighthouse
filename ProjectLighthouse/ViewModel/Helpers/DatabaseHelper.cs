using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class DatabaseHelper
    {
        private static readonly string dbFile = @"manufactureDB.db3";

        public static bool Insert<T>(T item)
        {
            bool result = false;

            Debug.WriteLine(string.Format("dbFile: {0}", Path.Join(App.ROOT_PATH ?? Environment.SpecialFolder.Desktop.ToString(), dbFile)));

            using (SQLiteConnection conn = new SQLiteConnection(Path.Join(App.ROOT_PATH ?? Environment.SpecialFolder.Desktop.ToString(), dbFile)))
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

            Debug.WriteLine(string.Format("dbFile: {0}", Path.Join(App.ROOT_PATH ?? Environment.SpecialFolder.Desktop.ToString(), dbFile)));

            using (SQLiteConnection conn = new SQLiteConnection(Path.Join(App.ROOT_PATH ?? Environment.SpecialFolder.Desktop.ToString(), dbFile)))
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

            Debug.WriteLine(string.Format("dbFile: {0}", Path.Join(App.ROOT_PATH ?? Environment.SpecialFolder.Desktop.ToString(), dbFile)));

            using (SQLiteConnection conn = new SQLiteConnection(Path.Join(App.ROOT_PATH ?? Environment.SpecialFolder.Desktop.ToString(), dbFile)))
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

            Debug.WriteLine(string.Format("dbFile: {0}", Path.Join(App.ROOT_PATH ?? Environment.SpecialFolder.Desktop.ToString(), dbFile)));

            using (SQLiteConnection conn = new(Path.Join(App.ROOT_PATH ?? Environment.SpecialFolder.Desktop.ToString(), dbFile)))
            {
                conn.CreateTable<T>();
                items = conn.Table<T>().ToList();
            }
            return items;
        }
    }
}
