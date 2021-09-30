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

        private static readonly string DEBUG_dbFile = @"manufactureDB_debug.db3";

        private static readonly string dbFile = @"manufactureDB.db3";

        public static string GetDatabaseFile()
        {
            if (Debugger.IsAttached)
            {
                return Environment.UserName == "xavier"
                ? @"C:\Users\xavie\Desktop\manufactureDB.db3"
                : Path.Join(App.ROOT_PATH ?? @"\\groupfile01\Sales\Production\Administration\Manufacture Records\Lighthouse", DEBUG_dbFile);
            }
            else
            {
                return Environment.UserName == "xavier"
                ? @"C:\Users\xavie\Desktop\manufactureDB.db3"
                : Path.Join(App.ROOT_PATH ?? @"\\groupfile01\Sales\Production\Administration\Manufacture Records\Lighthouse", dbFile);
            } 
        }

        public static bool Insert<T>(T item)
        {
            bool result = false;

            using (SQLiteConnection conn = new(GetDatabaseFile()))
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

            using (SQLiteConnection conn = new(GetDatabaseFile()))
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

            using (SQLiteConnection conn = new(GetDatabaseFile()))
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
            try
            {
                using (SQLiteConnection conn = new(GetDatabaseFile()))
                {
                    conn.CreateTable<T>();
                    items = conn.Table<T>().ToList();
                }

                return items;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
