using Newtonsoft.Json;
using ProjectLighthouse.Model.Programs;
using ProjectLighthouse.View.Programs;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static ProjectLighthouse.Model.Programs.NcProgram;

namespace ProjectLighthouse.ViewModel.Core
{
    public static class LumenManager
    {
        //private static SerialPort mscomm1;

        public static List<string> Themes { get; set; } = new();
        public static List<string> ThemeNames { get; set; } = new();

        public static readonly Dictionary<string, string> builtInThemes = new()
        {
            { "Visual Studio", "vs" },
            { "Visual Studio Dark", "vs-dark" },
            { "VS High Contrast", "hc-black" },
            { "VS High Contrast Light", "hc-light" }
        };

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
        private static List<Monaco> SingleProgramWindows = new();
        public static List<NcProgramCommit> Commits;


        private static string MonacoTitle = "Lumen (preview)";


        private static void DownloadProgram()
        {
            //    frmSerialComms.serialDataItem serialDataItem1 = new frmSerialComms.serialDataItem();
            //    this.selectedMachine = selectedMachineID;
            //    this.selectedMachineSettings = mSettings.getMachineSettings(this.selectedMachine);
            //    frmSerialComms.serialDataItem serialDataItem2;
            //    if (!this.setupCommPort(this.selectedMachine))
            //    {
            //        serialDataItem2 = serialDataItem1;
            //    }
            //    else
            //    {
            //        this.timRecProgram.Enabled = true;
            //        try
            //        {
            //            this.mscomm1.Open();
            //        }
            //        catch (Exception ex)
            //        {
            //            ProjectData.SetProjectError(ex);
            //            this.resetRecTimersAndComPort(true);
            //            int num = (int)Interaction.MsgBox((object)mSettings.getLanguageText(nameof(frmSerialComms), "errCommPortOpen"), MsgBoxStyle.Exclamation);
            //            serialDataItem2 = serialDataItem1;
            //            ProjectData.ClearProjectError();
            //            goto label_14;
            //        }
            //        try
            //        {
            //            if (this.receiveMsg == null)
            //                this.receiveMsg = new frmReceiveMsg();
            //            this.receiveMsg.Owner = (Form)startup.MDIForm1;
            //            this.receiveMsg.Show();
            //            while (!this.dataReceiveComplete)
            //            {
            //                if (this.receiveMsg.recCancelled)
            //                {
            //                    serialDataItem2 = serialDataItem1;
            //                    goto label_14;
            //                }
            //                else
            //                    Application.DoEvents();
            //            }
            //            serialDataItem1.successful = true;
            //            serialDataItem1.program = this.recProg;
            //            serialDataItem2 = serialDataItem1;
            //        }
            //        catch (Exception ex)
            //        {
            //            ProjectData.SetProjectError(ex);
            //            this.resetRecTimersAndComPort(true);
            //            int num = (int)Interaction.MsgBox((object)(mSettings.getLanguageText(nameof(frmSerialComms), "errDownloadProgram") + "    " + Conversion.ErrorToString()), MsgBoxStyle.Exclamation);
            //            serialDataItem2 = serialDataItem1;
            //            ProjectData.ClearProjectError();
            //        }
            //    }
            //label_14:
            //    return serialDataItem2;
        }

        private static void SetupCommPort()
        {
            //bool flag = true;
            //this.mscomm1.Encoding = startup.encoding;
            //try
            //{
            //    mSettings.machineSettings machineSettings = mSettings.getMachineSettings(machineNo);
            //    this.mscomm1.BaudRate = Convert.ToInt32(machineSettings.baudRate);
            //    string parity1 = machineSettings.parity;
            //    Parity parity2;
            //    if (Operators.CompareString(parity1, "N", false) != 0)
            //    {
            //        if (Operators.CompareString(parity1, "E", false) != 0)
            //        {
            //            if (Operators.CompareString(parity1, "O", false) == 0)
            //                parity2 = Parity.Odd;
            //        }
            //        else
            //            parity2 = Parity.Even;
            //    }
            //    else
            //        parity2 = Parity.None;
            //    this.mscomm1.Parity = parity2;
            //    this.mscomm1.DataBits = Convert.ToInt32(machineSettings.dataBits);
            //    StopBits stopBits;
            //    switch (machineSettings.stopBits)
            //    {
            //        case 0:
            //            stopBits = StopBits.None;
            //            break;
            //        case 1:
            //            stopBits = StopBits.One;
            //            break;
            //        case 2:
            //            stopBits = StopBits.Two;
            //            break;
            //    }
            //    this.mscomm1.StopBits = stopBits;
            //    string handshaking = machineSettings.handshaking;
            //    Handshake handshake;
            //    if (Operators.CompareString(handshaking, "0", false) != 0)
            //    {
            //        if (Operators.CompareString(handshaking, "1", false) != 0)
            //        {
            //            if (Operators.CompareString(handshaking, "2", false) != 0)
            //            {
            //                if (Operators.CompareString(handshaking, "3", false) == 0)
            //                    handshake = Handshake.RequestToSendXOnXOff;
            //            }
            //            else
            //                handshake = Handshake.RequestToSend;
            //        }
            //        else
            //            handshake = Handshake.XOnXOff;
            //    }
            //    else
            //        handshake = Handshake.None;
            //    this.mscomm1.Handshake = handshake;
            //    this.mscomm1.PortName = "COM" + Conversions.ToString(machineSettings.comPort);
            //    this.mscomm1.RtsEnable = false;
            //    this.mscomm1.DtrEnable = true;
            //}
            //catch (Exception ex)
            //{
            //    ProjectData.SetProjectError(ex);
            //    flag = false;
            //    int num = (int)Interaction.MsgBox((object)(mSettings.getLanguageText(nameof(frmSerialComms), "errSetupCommPort") + "  " + Conversion.ErrorToString()), MsgBoxStyle.Exclamation);
            //    ProjectData.ClearProjectError();
            //}
            //return flag;
        }

        public static void Initialise()
        {
            Themes = Directory.GetFiles(Path.Join(AppDomain.CurrentDomain.BaseDirectory, @"Monaco\themes")).ToList();

            foreach (string key in builtInThemes.Keys)
            {
                ThemeNames.Add(key);
            }

            for (int i = 0; i < Themes.Count; i++)
            {
                string f = Path.GetFileNameWithoutExtension(Themes[i]);
                if (f == "themelist" || f == "Brilliance Dull" || f == "Brilliance Black" || f == "Upstream Sunburst")
                {
                    continue;
                }
                ThemeNames.Add(f);
            }

            SelectedThemeName = "Visual Studio";
        }

        //public static void Close()
        //{
        //    if (MonacoWindow.Programs.Count == 0)
        //    {
        //        MonacoWindow.Close();
        //    }
        //}
        public static Task<NcProgram> LoadProgram(NcProgram program)
        {
            string path = program.Path;

#if DEBUG
            if (!File.Exists(path))
            {
                path = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        @"View\Programs\12.PRG");
            }
#endif

            try
            {
                program.ProgramContent = GetProgramFromFile(path);
            }
            catch
            {
                throw;
            }

            return Task.FromResult(program);
        }


        public static void Open(NcProgram program)
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

            bool monacoWasLoadedPrior = MonacoWindow != null;
            MonacoWindow ??= new() { Title = MonacoTitle };
            if (!monacoWasLoadedPrior)
            {
                Commits = DatabaseHelper.Read<NcProgramCommit>();
            }

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

            if (!programData.Contains("$0"))
            {
                throw new InvalidDataException("Missing Spindle: $0");
            }
            if (!programData.Contains("$1"))
            {
                throw new InvalidDataException("Missing Spindle: $1");
            }
            if (!programData.Contains("$2"))
            {
                throw new InvalidDataException("Missing Spindle: $2");
            }

            string dollarOneCode = programData.Substring(programData.IndexOf("$1") + 2, programData.IndexOf("$2") - programData.IndexOf("$1") - 2).Trim();
            string dollarTwoCode = programData.Substring(programData.IndexOf("$2") + 2, programData.IndexOf("$0") - programData.IndexOf("$2") - 2).Trim();

            dollarOneCode = Unpack(dollarOneCode);
            dollarTwoCode = Unpack(dollarTwoCode);


            MonacoProgram prog = new()
            {
                Header = programData[..programData.IndexOf("$1")].Trim(),
                DollarOneCode = dollarOneCode,
                DollarTwoCode = dollarTwoCode,
                DollarZeroCode = programData.Substring(programData.IndexOf("$0") + 2, programData.Length - programData.IndexOf("$0") - 2).Trim(),
                OriginalDollarOneCode = "",
                OriginalDollarTwoCode = ""
            };

            return prog;
        }

        private static string Unpack(string dollarOneCode)
        {
            string[] lines = dollarOneCode.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                line = Regex.Replace(line, @"(?<![A-Z|\! ]{1})[A-Z]{1}(\d+|\#\d+|\-)", " $0");
                //line = Regex.Replace(line, @"(?<![\s]{1})[A-Z]{1}(\[)", " $0");
                //line = Regex.Replace(line, @"(?>![\s]{1})[A-Z]{1}(\])", "$0 ");
                line = line.Trim();
                lines[i] = line;
            }

            string cleanedCode = string.Join('\n', lines);
            cleanedCode = cleanedCode.Trim();
            return cleanedCode;
        }

        public static void PostCommit(MonacoProgram program, NcProgramCommit commit)
        {
            commit.CommittedAt = DateTime.Now;
            commit.CommittedBy = App.CurrentUser.UserName;

            commit.FileName = Guid.NewGuid().ToString()[..8];
            while (File.Exists(commit.Url))
            {
                commit.FileName = Guid.NewGuid().ToString()[..8];
            }

            try
            {
                File.WriteAllText(commit.Url, program.Pack());
            }
            catch
            {
                throw;
            }

            try
            {
                DatabaseHelper.Insert(commit, throwErrs: true);
            }
            catch
            {
                throw;
            }

            Commits.Add(commit);
        }

        public static MonacoProgram LoadCommit(MonacoProgram target, NcProgramCommit? preceedingCommit)
        {
            MonacoProgram targetProgram = target;

            if (preceedingCommit is null) return targetProgram;

            MonacoProgram preceedingProgram;
            try
            {
                preceedingProgram = GetProgramFromFile(preceedingCommit.Url);
            }
            catch
            {
                throw;
            }

            targetProgram.OriginalDollarOneCode = preceedingProgram.DollarOneCode;
            targetProgram.OriginalDollarTwoCode = preceedingProgram.DollarTwoCode;

            return targetProgram;
        }

        public static MonacoProgram LoadCommit(NcProgramCommit target, NcProgramCommit? preceedingCommit)
        {
            MonacoProgram targetProgram;

            try
            {
                targetProgram = GetProgramFromFile(target.Url);
            }
            catch
            {
                throw;
            }

            if (preceedingCommit is null) return targetProgram;

            MonacoProgram preceedingProgram;
            try
            {
                preceedingProgram = GetProgramFromFile(preceedingCommit.Url);
            }
            catch
            {
                throw;
            }

            targetProgram.OriginalDollarOneCode = preceedingProgram.DollarOneCode;
            targetProgram.OriginalDollarTwoCode = preceedingProgram.DollarTwoCode;

            return targetProgram;
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
            if (builtInThemes.TryGetValue(name, out string value))
            {
                return value;
            }

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
