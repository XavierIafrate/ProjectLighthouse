using ProjectLighthouse.Model.Programs;
using ProjectLighthouse.View.Programs;
using static ProjectLighthouse.Model.Programs.NcProgram;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ProjectLighthouse.ViewModel.Core
{
    public static class LumenManager
    {
        static Monaco MonacoWindow;

        //public static void Initialise()
        //{
        //    MonacoWindow = new();
        //}

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



            program.ProgramContent = GetProgramFromFile(path);

            program.ProgramContent.DollarOneCode = $"Program {program.Name}{Environment.NewLine}{Environment.NewLine}{program.ProgramContent.DollarOneCode}";

            bool monacoWasLoadedPrior = MonacoWindow != null;
            MonacoWindow ??= new();

            if(MonacoWindow.Programs.Contains(program))
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

        public static void DisposeWindow()
        {
            MonacoWindow = null;
        }
    }
}
