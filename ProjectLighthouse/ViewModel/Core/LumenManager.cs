using DocumentFormat.OpenXml.Drawing.Diagrams;
using Newtonsoft.Json;
using ProjectLighthouse.Model.Programs;
using ProjectLighthouse.View.Programs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static ProjectLighthouse.Model.Programs.NcProgram;

namespace ProjectLighthouse.ViewModel.Core
{
    public static class LumenManager
    {
        public static List<string> Themes { get; set; } = new();
        public static List<string> ThemeNames { get; set; } = new();

        private static string selectedThemeName;
        public static string SelectedThemeName
        {
            get { return selectedThemeName; }
            set
            {
                selectedThemeName = value;
                SelectedThemeData = LoadThemeData(selectedThemeName);
            }
        }

        public static string SelectedThemeData { get; set; }

        private static Monaco MonacoWindow;

        private static List<Monaco> SingleProgramWindows { get; set; } = new();


        private static string MonacoTitle = "Lumen (preview)";


        public static void Initialise()
        {
            Themes = Directory.GetFiles(Path.Join(AppDomain.CurrentDomain.BaseDirectory, @"Monaco\themes")).ToList();

            for (int i = 0; i < Themes.Count; i++)
            {
                string f = Path.GetFileNameWithoutExtension(Themes[i]);
                if (f == "themelist")
                {
                    continue;
                }
                ThemeNames.Add(f);
            }

            SelectedThemeName = "Oceanic Next";
        }

        //public static void Close()
        //{
        //    if (MonacoWindow.Programs.Count == 0)
        //    {
        //        MonacoWindow.Close();
        //    }
        //}

        public static void Open(NcProgram program)
        {
            string path = program.Path ?? Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        @"View\Programs\12.PRG");


            try
            {
                program.ProgramContent = GetProgramFromFile(path);
            }
            catch (Exception ex)
            {
                NotificationManager.NotifyHandledException(ex);
                return;
            }

            program.ProgramContent.DollarOneCode = $"Program {program.Name}{Environment.NewLine}{Environment.NewLine}{program.ProgramContent.DollarOneCode}";
            
            for(int i = 0; i < SingleProgramWindows.Count; i++)
            {
                if (SingleProgramWindows[i].SelectedProgram.Name == program.Name)
                {
                    SingleProgramWindows[i].Show();
                    SingleProgramWindows[i].Activate();
                    return;
                }
            }

            bool monacoWasLoadedPrior = MonacoWindow != null;
            MonacoWindow ??= new() { Title = MonacoTitle };

            if (MonacoWindow.Programs.Contains(program))
            {
                MonacoWindow.SelectedProgram = program;
            }
            else
            {
                MonacoWindow.Programs.Add(program);
                if (monacoWasLoadedPrior)
                {
                    MonacoWindow.SelectedProgram = program;
                }
            }

            MonacoWindow.Show();
            MonacoWindow.Activate();
        }

        public static void OpenInSingleWindow(NcProgram program)
        {
            for (int i = 0; i < SingleProgramWindows.Count; i++)
            {
                if (SingleProgramWindows[i].SelectedProgram.Name == program.Name)
                {
                    SingleProgramWindows[i].Show();
                    SingleProgramWindows[i].Activate();
                    return;
                }
            }

            string path = program.Path ?? Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        @"View\Programs\12.PRG");

            try
            {
                program.ProgramContent = GetProgramFromFile(path);
            }
            catch (Exception ex)
            {
                NotificationManager.NotifyHandledException(ex);
                return;
            }

            // check if in any other window
            Monaco newWindow = new() { Title = $"{MonacoTitle} | Program {program.Name}" };
            newWindow.Programs.Add(program);
            newWindow.HideMenu();

            SingleProgramWindows.Add(newWindow);

            newWindow.Show();
            newWindow.Activate();

            MonacoWindow.Programs.Remove(program);
            if (MonacoWindow.Programs.Count > 0)
            {
                MonacoWindow.SelectedProgram = MonacoWindow.Programs.First();   
            }
        }

        public static void Close(NcProgram program)
        {
            MonacoWindow.Programs.Remove(program);
        }

        private static MonacoProgram GetProgramFromFile(string path)
        {
            string programData = File.ReadAllText(path);
            if (!programData.Contains("$0") || !programData.Contains("$1") || !programData.Contains("$2"))
            {
                throw new InvalidDataException("spindle missing");
            }

            MonacoProgram prog = new()
            {
                Header = programData[..programData.IndexOf("$1")].Trim(),
                DollarOneCode = programData.Substring(programData.IndexOf("$1") + 2, programData.IndexOf("$2") - programData.IndexOf("$1") - 2).Trim(),
                DollarTwoCode = programData.Substring(programData.IndexOf("$2") + 2, programData.IndexOf("$0") - programData.IndexOf("$2") - 2).Trim(),
                DollarZeroCode = programData.Substring(programData.IndexOf("$0") + 2, programData.Length - programData.IndexOf("$0") - 2).Trim()
            };

            prog.OriginalDollarOneCode = prog.DollarOneCode;
            prog.OriginalDollarTwoCode = prog.DollarTwoCode;

            return prog;
        }

        public static async Task<string> ExecuteScriptFunctionAsync(Microsoft.Web.WebView2.Wpf.WebView2 webView2, string functionName, params object[] parameters)
        {
            string script = functionName + "(";
            for (int i = 0; i < parameters.Length; i++)
            {
                script += JsonConvert.SerializeObject(parameters[i]);
                if (i < parameters.Length - 1)
                {
                    script += ", ";
                }
            }
            script += ");";
            return await webView2.ExecuteScriptAsync(script);
        }

        public static void SetTheme(string name)
        {
            try
            {
                SelectedThemeName = name;
            }
            catch (Exception ex)
            {
                NotificationManager.NotifyHandledException(ex);
                return;
            }

            MonacoWindow.ApplyTheme(SelectedThemeData);

            for (int i = 0; i < SingleProgramWindows.Count; i++)
            {
                SingleProgramWindows[i].ApplyTheme(SelectedThemeData);
            }
        }

        static string LoadThemeData(string name)
        {
            string path = LumenManager.Themes.Find(x => Path.GetFileNameWithoutExtension(x) == name)
                 ?? throw new ArgumentException($"Could not find theme: '{name}'");

            string data = File.ReadAllText(path);
            return data;
        }


        public static void DisposeWindow(Monaco window)
        {
            if (window != MonacoWindow)
            {
                SingleProgramWindows.Remove(window);
            }
            else if (SingleProgramWindows.Count > 0)
            {
                MonacoWindow = SingleProgramWindows.First();
                SingleProgramWindows.RemoveAt(0);
                MonacoWindow.ShowMenu();
                MonacoWindow.Title = MonacoTitle;
            }
            else
            {
                MonacoWindow = null;    
            }
        }
    }
}
