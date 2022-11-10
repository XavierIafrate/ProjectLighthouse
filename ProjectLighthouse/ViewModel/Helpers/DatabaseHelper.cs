using ProjectLighthouse.Model.Core;
using SQLite;
using System.Collections.Generic;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class DatabaseHelper
    {

        public static string DatabasePath { get; set; }

        public static bool ExecuteCommand<T>(string query)
        {
            using (SQLiteConnection conn = new(DatabasePath))
            {
                var test = conn.Execute(query);

            }

            return true;
        }

        public static bool Insert<T>(T item)
        {
            bool result = false;

            using (SQLiteConnection conn = new(DatabasePath))
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
                catch
                {
                    result = false;
                }
            }
            return result;
        }

        public static int InsertAndReturnId<T>(T item) where T : IAutoIncrementPrimaryKey
        {
            using (SQLiteConnection conn = new(DatabasePath))
            {
                conn.CreateTable<T>();
                try
                {
                    int rows = conn.Insert(item);
                }
                catch (SQLiteException sqle)
                {
                    MessageBox.Show($"Lighthouse encountered an error trying to add to the database.\nError message: {sqle.Message}", "Insert Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0;
                }
            }
            return item.Id;
        }

        public static bool Update<T>(T item)
        {
            bool result = false;

            using (SQLiteConnection conn = new(DatabasePath))
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

            using (SQLiteConnection conn = new(DatabasePath))
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
                using (SQLiteConnection conn = new(DatabasePath))
            {
                conn.CreateTable<T>();
                items = conn.Table<T>().ToList();
            }

            return items;
            }
            catch
            {
                return null;
            }
        }
    }
}
