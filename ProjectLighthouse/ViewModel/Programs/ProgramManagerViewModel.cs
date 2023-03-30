using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Programs;
using ProjectLighthouse.View.Programs;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Commands.Programs;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using ViewModel.Commands.Programs;

namespace ProjectLighthouse.ViewModel.Programs
{
    public class ProgramManagerViewModel : BaseViewModel, IDisposable
    {
        public List<NcProgram> Programs;

        private List<NcProgram> filteredPrograms;
        public List<NcProgram> FilteredPrograms
        {
            get { return filteredPrograms; }
            set
            {
                filteredPrograms = value;
                OnPropertyChanged();
            }
        }

        public List<NcProgramCommit> ProgramCommits;

        private ObservableCollection<NcProgramCommit> filteredCommits = new();
        public ObservableCollection<NcProgramCommit> FilteredCommits
        {
            get { return filteredCommits; }
            set
            {
                filteredCommits = value;
                OnPropertyChanged();
            }
        }



        public List<MaterialInfo> Materials;
        public List<Lathe> Machines;
        public List<ProductGroup> Archetypes { get; set; }
        public List<Product> Products { get; set; }

        private List<Product>? usedFor;
        public List<Product>? UsedFor
        {
            get { return usedFor; }
            set
            {
                if (value is List<Product> list)
                {
                    if (list.Count == 0) value = null;
                }
                usedFor = value;
                OnPropertyChanged();
            }
        }

        private List<MaterialInfo> constrainedMaterials;
        public List<MaterialInfo> ConstrainedMaterials
        {
            get { return constrainedMaterials; }
            set
            {
                constrainedMaterials = value;
                OnPropertyChanged();
            }
        }


        List<Note> Notes;

        private List<Note> filteredNotes;
        public List<Note> FilteredNotes
        {
            get { return filteredNotes; }
            set
            {
                filteredNotes = value;
                OnPropertyChanged();
            }
        }

        private string searchString = "";
        public string SearchString
        {
            get { return searchString; }
            set
            {
                searchString = value;
                Search();
                OnPropertyChanged();
            }
        }

        private NcProgram selectedProgram;
        public NcProgram SelectedProgram
        {
            get { return selectedProgram; }
            set
            {
                selectedProgram = value;
                LoadProgram();
                OnPropertyChanged();
            }
        }

        private string newMessage;
        public string NewMessage
        {
            get { return newMessage; }
            set
            {
                newMessage = value;
                OnPropertyChanged();
            }
        }

        private bool lumenButtonEnabled = true;

        public bool LumenButtonEnabled
        {
            get { return lumenButtonEnabled; }
            set { lumenButtonEnabled = value; OnPropertyChanged(); }
        }


        public SendMessageCommand SendMessageCmd { get; set; }
        public EditProgramCommand EditProgramCmd { get; set; }
        public OpenProgramCommand OpenProgramCmd { get; set; }
        public OpenCommitCommand OpenCommitCmd { get; set; }

        private System.Timers.Timer timer;

        public ProgramManagerViewModel()
        {
            timer = new(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            LoadData();
            Search();
        }


        private CancellationTokenSource cancellationTokenSource = new();
        private void CheckProgramStatuses()
        {
            cancellationTokenSource = new();

            Task.Run(() => CheckPrograms(cancellationTokenSource.Token), cancellationTokenSource.Token);
        }


        private async Task CheckPrograms(CancellationToken ct)
        {
            Thread.Sleep(500);
            List<NcProgram> programs = new(Programs);

            List<string> filesInDir = Directory.GetFiles(NcProgram.BaseProgramPath).ToList();

            foreach (NcProgram program in programs)
            {
                if (ct.IsCancellationRequested)
                {
                    Debug.WriteLine("Cancellation Requested");
                    ct.ThrowIfCancellationRequested();
                    return;
                }

                string path = filesInDir.Find(x => x == program.Path);

                bool exists = path is not null;
                DateTime? date;

                if (path is not null)
                {
                    date = File.GetLastWriteTime(path);
                }
                else
                {
                    date = null;
                }

                Thread.Sleep(50); // dramatic effect
                NcProgram p = Programs.Find(x => x.Name == program.Name);
                if (p is null) continue;

                p.FileExists = exists;
                p.FileLastModified = date;

                NcProgram? uiProgram = FilteredPrograms.Find(x => x.Id == program.Id);
                if (uiProgram is not null)
                {
                    uiProgram.FileExists = exists;
                    uiProgram.FileLastModified = date;
                }
            }


            foreach (NcProgram program in FilteredPrograms)
            {
                program.FileExists = Programs.Find(x => x.Id == program.Id)?.FileExists;
            }
        }


        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (FilteredCommits is null) return;
            if (FilteredCommits.Count == 0) return;

            FilteredCommits[0].CommittedAt = DateTime.Now;
        }

        void LoadData()
        {
            SendMessageCmd = new(this);
            EditProgramCmd = new(this);
            OpenProgramCmd = new(this);
            OpenCommitCmd = new(this);

            Programs = DatabaseHelper.Read<NcProgram>()
                .OrderBy(x => x.Name.Length)
                .ThenBy(x => x.Name)
                .ToList();
            Programs.ForEach(x => x.ValidateAll());

            ProgramCommits = DatabaseHelper.Read<NcProgramCommit>();

            Notes = DatabaseHelper.Read<Note>()
                .Where(x => x.DocumentReference.StartsWith("p"))
                .ToList();

            Archetypes = DatabaseHelper.Read<ProductGroup>()
                .OrderBy(x => x.Name)
                .ToList();
            Products = DatabaseHelper.Read<Product>()
                .OrderBy(x => x.Name)
                .ToList();

            Materials = DatabaseHelper.Read<MaterialInfo>();
            Machines = DatabaseHelper.Read<Lathe>();
        }

        public void EditProgram()
        {
            if (SelectedProgram is null) return;

            AddProgramWindow window = new(Products, Archetypes, Materials, Machines, SelectedProgram) { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }

            int selectedProgramId = SelectedProgram.Id;

            LoadData();
            Search();

            SelectedProgram = FilteredPrograms.Find(x => x.Id == selectedProgramId);
        }

        public async void OpenProgram()
        {
            if (SelectedProgram is null) return;
            if (SelectedProgram.Path is null) return;

            //LumenButtonEnabled = false;
            NcProgram p;
            try
            {
                p = await LumenManager.LoadProgram(SelectedProgram);
            }
            catch (Exception ex)
            {
                NotificationManager.NotifyHandledException(ex);
                return;
            }

            LumenManager.Open(p);

            LumenButtonEnabled = true;
        }

        public void AddProgram()
        {
            AddProgramWindow window = new(Products, Archetypes, Materials, Machines) { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }

            int selectedProgramId = window.Program.Id;

            LoadData();
            Search();

            SelectedProgram = FilteredPrograms.Find(x => x.Id == selectedProgramId);
        }

        void Search()
        {
            cancellationTokenSource.Cancel();

            if (string.IsNullOrWhiteSpace(SearchString))
            {
                FilteredPrograms = new(Programs);
                if (FilteredPrograms.Count > 0)
                {
                    SelectedProgram = filteredPrograms[0];
                }
                else
                {
                    SelectedProgram = null;
                }

                CheckProgramStatuses();
                return;
            }

            string tagSearch = SearchString.Replace(" ", "").ToLowerInvariant();

            FilteredPrograms = Programs
                .Where(x =>
                x.Name.ToLowerInvariant().Contains(SearchString.ToLowerInvariant()) ||
                x.SearchableTags.Contains(tagSearch))
                .ToList();

            List<int> products = Products.Where(x => x.Name.ToLowerInvariant().Contains(tagSearch)).Select(x => x.Id).ToList();
            FilteredPrograms.AddRange(Programs.Where(x => products.Any(y => x.ProductStringIds.Contains(y.ToString("0")))));

            List<int> groups = Archetypes.Where(x => x.Name.ToLowerInvariant().Contains(tagSearch)).Select(x => x.Id).ToList();
            FilteredPrograms.AddRange(Programs.Where(x => groups.Any(y => x.GroupStringIds.Contains(y.ToString("0")))));

            FilteredPrograms = FilteredPrograms.Distinct().OrderBy(x => x.Name.Length).ThenBy(x => x.Name).ToList();

            if (FilteredPrograms.Count > 0)
            {
                SelectedProgram = FilteredPrograms[0];
            }
            else
            {
                SelectedProgram = null;
            }

            CheckProgramStatuses();
        }

        public void SendMessage()
        {
            if (SelectedProgram is null) return;

            Note newNote = new()
            {
                Message = NewMessage,
                OriginalMessage = NewMessage,
                DateSent = DateTime.Now.ToString("s"),
                DocumentReference = $"p{SelectedProgram.Id:0}",
                SentBy = App.CurrentUser.UserName,
            };

            try
            {
                DatabaseHelper.Insert(newNote, throwErrs: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while inserting to the database:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Notes.Add(newNote);
            LoadProgram();

            NewMessage = "";
        }

        private void LoadProgram()
        {
            if (SelectedProgram == null)
            {
                FilteredNotes = null;
                FilteredCommits = new();
                return;
            }

            List<NcProgramCommit> commits = ProgramCommits
                .Where(x => x.ProgramId == SelectedProgram.Id)
                .OrderByDescending(x => x.CommittedAt)
                .ToList();
            FilteredCommits = new();
            commits.ForEach(x => FilteredCommits.Add(x));
            OnPropertyChanged(nameof(FilteredCommits));

            FilteredNotes = Notes
                .Where(x => x.DocumentReference == $"p{SelectedProgram.Id}")
                .ToList();

            if (SelectedProgram.Groups is null)
            {
                UsedFor = new();
                ConstrainedMaterials = new();
            }

            List<string> productStringIds = SelectedProgram.ProductStringIds;
            List<Product> targetedProducts = new();
            for (int i = 0; i < productStringIds.Count; i++)
            {
                if (!int.TryParse(productStringIds[i], out int productId))
                {
                    MessageBox.Show($"Failed to convert '{productStringIds[i]}' to integer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }
                Product? p = Products.Find(x => x.Id == productId);

                if (p is null)
                {
                    MessageBox.Show($"Failed to find group with ID '{productId}'", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                targetedProducts.Add(p);
            }

            List<string> groupStringIds = SelectedProgram.GroupStringIds;
            List<ProductGroup> targetedGroups = new();

            for (int i = 0; i < groupStringIds.Count; i++)
            {
                if (!int.TryParse(groupStringIds[i], out int groupId))
                {
                    MessageBox.Show($"Failed to convert '{groupStringIds[i]}' to integer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }
                ProductGroup? g = Archetypes.Find(x => x.Id == groupId);

                if (g is null)
                {
                    MessageBox.Show($"Failed to find group with ID '{groupId}'", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                targetedGroups.Add(g);
            }

            for (int i = 0; i < targetedProducts.Count; i++)
            {
                targetedProducts[i].Archetypes = targetedGroups.Where(x => x.ProductId == targetedProducts[i].Id).ToList();
            }

            List<string> materialStringIds = SelectedProgram.MaterialsList;
            List<MaterialInfo> targetedMaterials = new();

            for (int i = 0; i < materialStringIds.Count; i++)
            {
                if (!int.TryParse(materialStringIds[i], out int materialId))
                {
                    MessageBox.Show($"Failed to convert '{materialStringIds[i]}' to integer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }
                MaterialInfo? m = Materials.Find(x => x.Id == materialId);

                if (m is null)
                {
                    MessageBox.Show($"Failed to find material with ID '{materialId}'", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                targetedMaterials.Add(m);
            }

            UsedFor = targetedProducts.OrderBy(x => x.Name).ToList();
            ConstrainedMaterials = targetedMaterials.OrderBy(x => x.ToString()).ToList();
        }

        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            //LumenManager.Close();
        }

        public void OpenCommit(NcProgramCommit commit)
        {
            MessageBox.Show(commit.FileName);
        }
    }
}

