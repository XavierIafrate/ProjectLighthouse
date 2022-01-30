using SQLite;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class DatabaseHelper
    {
        public static string GetDatabaseFile()
        {
            return Environment.UserName == "xavier"
                               ? @"C:\Users\xavie\Documents\lighthouse_test\manufactureDB.db3"
                               : @"\\groupfile01\Sales\Production\Administration\Manufacture Records\Lighthouse\manufactureDB.db3";
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
                    MessageBox.Show($"Lighthouse encountered an error trying to add to the database.\nError message: {sqle.Message}", "Insert Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show($"Lighthouse encountered an error trying to update the database.\nError message: {ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Lighthouse encountered an error trying to read the database.\nError message: {ex.Message}", "Read Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
