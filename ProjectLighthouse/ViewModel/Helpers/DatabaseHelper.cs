using ProjectLighthouse.Model.Analytics;
using ProjectLighthouse.Model.Core;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class DatabaseHelper
    {

        public static string DatabasePath { get; set; }

        public static bool ExecuteCommand(string query)
        {
            using SQLiteConnection conn = GetConnection();
            try
            {
                int result = conn.Execute(query);
            }
            catch(Exception err)
            {
                throw err;
            }
            return true;
        }

        public static List<MachineStatistics> QueryMachineHistory(DateTime date)
        {
            using SQLiteConnection conn = GetConnection();
            return conn.Query<MachineStatistics>(
                query: $"SELECT * FROM {nameof(MachineStatistics)} WHERE {nameof(MachineStatistics.DataTime)} > ? ORDER BY {nameof(MachineStatistics.DataTime)}",
                args: date.Ticks)
                .ToList();
        }

        public static bool Insert<T>(T item, bool throwErrs = false, SQLiteConnection? conn = null)
        {
            bool result = false;
            conn ??= GetConnection();

            using (conn)
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
                catch (Exception ex)
                {
                    if (throwErrs)
                    {
                        throw;
                    }
                    Debug.WriteLine(ex.Message);
                    result = false;
                }
            }
            return result;
        }

        public static int InsertAndReturnId<T>(T item, SQLiteConnection? conn = null) where T : IAutoIncrementPrimaryKey
        {
            conn ??= GetConnection();

            using (conn)
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

        public static bool Update<T>(T item, bool throwErrs = false, SQLiteConnection? conn = null)
        {
            bool result = false;

            conn ??= GetConnection();

            using (conn)
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
                    if (throwErrs)
                    {
                        throw;
                    }
                    MessageBox.Show($"Lighthouse encountered an error trying to update the database.\nError message: {ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return result;
        }

        public static bool Delete<T>(T item, SQLiteConnection? conn = null)
        {
            bool result = false;

            conn ??= GetConnection();

            using (conn)
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

        public static List<T> Read<T>(bool throwErrs = false, SQLiteConnection? conn = null) where T : new()
        {
            List<T> items;
            try
            {
                conn ??= GetConnection();

                using (conn)
                {
                    conn.CreateTable<T>();
                    items = conn.Table<T>().ToList();
                }

                return items;
            }
            catch
            {
                if (throwErrs) { throw; }

                return null;
            }
        }

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(DatabasePath);
        }
    }
}
