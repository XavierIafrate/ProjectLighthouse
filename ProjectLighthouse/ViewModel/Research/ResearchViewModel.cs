using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Research;
using ProjectLighthouse.View.Research;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Commands.Research;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using ViewModel.Commands.Research;

namespace ProjectLighthouse.ViewModel.Research
{
    public class ResearchViewModel : BaseViewModel
    {
        #region Data
        public List<ResearchProject> Projects { get; set; }
        public List<ResearchArchetype> Archetypes { get; set; }
        public List<Note> Notes { get; set; }
        public List<ResearchPurchase> Purchases { get; set; }
        public Array Stages { get; set; }
        public List<User> Designers { get; set; }
        public List<Attachment> Attachments { get; set; }

        #endregion

        #region Full Properties
        private string searchString;
        public string SearchString
        {
            get { return searchString; }
            set { searchString = value; OnPropertyChanged(); Filter(); }
        }


        private string newMessage;

        public string NewMessage
        {
            get { return newMessage; }
            set { newMessage = value; OnPropertyChanged(); }
        }


        private List<ResearchProject> filteredProjects;
        public List<ResearchProject> FilteredProjects
        {
            get { return filteredProjects; }
            set { filteredProjects = value; OnPropertyChanged(); }
        }

        private ResearchProject selectedProject;
        public ResearchProject SelectedProject
        {
            get { return selectedProject; }
            set
            {
                selectedProject = value;
                ProjectShowing = value is not null;
                OnPropertyChanged();
            }
        }

        private List<Note> filteredNotes;
        public List<Note> FilteredNotes
        {
            get { return filteredNotes; }
            set { filteredNotes = value; OnPropertyChanged(); }
        }


        private bool projectShowing;
        public bool ProjectShowing
        {
            get { return projectShowing; }
            set { projectShowing = value; OnPropertyChanged(); }
        }

        private ResearchArchetype? selectedArchetype;

        public ResearchArchetype? SelectedArchetype
        {
            get { return selectedArchetype; }
            set { selectedArchetype = value; OnPropertyChanged(); }
        }

        private ResearchPurchase? selectedPurchase;

        public ResearchPurchase? SelectedPurchase
        {
            get { return selectedPurchase; }
            set { selectedPurchase = value; OnPropertyChanged(); }
        }

        private Attachment? selectedAttachment;

        public Attachment? SelectedAttachment
        {
            get { return selectedAttachment; }
            set { selectedAttachment = value; OnPropertyChanged(); }
        }

        private bool canManage;

        public bool CanManage
        {
            get { return canManage; }
            set { canManage = value; OnPropertyChanged(); }
        }



        #endregion

        #region Commands
        public SendMessageCommand SendMessageCmd { get; set; }
        public AddProjectCommand AddProjectCmd { get; set; }
        public OpenRootDirectoryCommand OpenRootDirectoryCmd { get; set; }
        public NewProjectArchetypeCommand NewProjectArchetypeCmd { get; set; }
        public NewPurchaseCommand NewPurchaseCmd { get; set; }
        public AddAttachmentCommand AddAttachmentCmd { get; set; }
        public RemoveAttachmentCommand RemoveAttachmentCmd { get; set; }
        public RemoveArchetypeCommand RemoveArchetypeCmd { get; set; }
        public RemovePurchaseCommand RemovePurchaseCmd { get; set; }
        public SaveProjectCommand SaveProjectCmd { get; set; }

        #endregion

        // TODO network config
        public const string projectRoot = @"\\groupfile01\Marketing\Product Development";

        public ResearchViewModel()
        {
            InitialiseCommands();
            LoadData();
            Filter();
        }

        private void InitialiseCommands()
        {
            AddProjectCmd = new(this);
            SendMessageCmd = new(this);
            OpenRootDirectoryCmd = new(this);
            NewProjectArchetypeCmd = new(this);
            NewPurchaseCmd = new(this);
            AddAttachmentCmd = new(this);
            RemoveAttachmentCmd = new(this);
            RemoveArchetypeCmd = new(this);
            RemovePurchaseCmd = new(this);
            SaveProjectCmd = new(this);
        }

        private void LoadData()
        {
            CanManage = App.CurrentUser.HasPermission(PermissionType.ManageProjects);

            Designers = App.NotificationsManager.users
                .Where(x => x.HasPermission(PermissionType.ManageProjects) || x.HasPermission(PermissionType.ModifyProjects))
                .ToList();

            Projects = DatabaseHelper.Read<ResearchProject>().ToList();
            Archetypes = DatabaseHelper.Read<ResearchArchetype>().ToList();
            Notes = DatabaseHelper.Read<Note>().ToList();
            Purchases = DatabaseHelper.Read<ResearchPurchase>().ToList();
            Attachments = DatabaseHelper.Read<Attachment>().ToList();

            for (int i = 0; i < Projects.Count; i++)
            {
                Projects[i].Notes = Notes
                    .Where(x => x.DocumentReference == $"dev{Projects[i].Id}")
                    .ToList();

                Projects[i].Archetypes = Archetypes
                    .Where(x => x.ProjectId == Projects[i].Id)
                    .ToList();

                Projects[i].Purchases = Purchases
                    .Where(x => x.ProjectId == Projects[i].Id)
                    .ToList();

                Projects[i].Attachments = Attachments
                    .Where(x => x.DocumentReference == $"dev{Projects[i].Id}")
                    .ToList();
            }

            Stages = Enum.GetValues(typeof(ResearchStage));
        }


        private void Filter()
        {
            List<ResearchProject> projects = new();
            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                string userRequest = searchString.Replace(" ", "").ToLowerInvariant();
                projects = Projects
                    .Where(x => x.ProjectName.ToLowerInvariant().Replace(" ", "").Contains(userRequest) || x.ProjectCode.Contains(userRequest))
                    .ToList();
            }
            else
            {
                projects = Projects;
            }

            FilteredProjects = projects
                .OrderBy(x => x.Id)
                .ToList();


            if (FilteredProjects.Count > 0)
            {
                SelectedProject = FilteredProjects.First();
            }
        }

        public void SendMessage()
        {
            Note newNote = new()
            {
                Message = NewMessage,
                OriginalMessage = NewMessage,
                DateSent = DateTime.Now.ToString("s"),
                DocumentReference = $"dev{SelectedProject.Id:0}",
                SentBy = App.CurrentUser.UserName,
            };

            _ = DatabaseHelper.Insert(newNote);

            // TODO notifications

            Notes.Add(newNote);
            Projects.Find(x => x.Id == SelectedProject.Id)!.Notes.Add(newNote);
            int id = SelectedProject.Id;

            Filter();

            SelectedProject = null;
            SelectedProject = FilteredProjects.Find(x => x.Id == id);

            NewMessage = "";
        }

        public void CreateProject()
        {
            NewProjectWindow Window = new()
            {
                Owner = App.MainViewModel.MainWindow
            };

            Window.ShowDialog();

            if (!Window.SaveExit)
            {
                return;
            }

            int id = DatabaseHelper.InsertAndReturnId(Window.NewProject);


            if (id == 0)
            {
                MessageBox.Show("Failed to insert to database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Directory.Exists($@"{projectRoot}\@template"))
            {
                Window.NewProject.RootDirectory = $"{Window.NewProject.ProjectCode} - {Window.NewProject.ProjectName}";
                DatabaseHelper.Update(Window.NewProject);
                CreateProjectFolder($@"{projectRoot}\@template", $@"{projectRoot}\{Window.NewProject.RootDirectory}");
            }

            Projects.Add(Window.NewProject);

            Filter();

            if (FilteredProjects.Any(x => x.Id == id))
            {
                SelectedProject = FilteredProjects.Find(x => x.Id == id);
                OnPropertyChanged(nameof(SelectedProject));
            }
        }

        private void CreateProjectFolder(string templateDirectory, string newDirectory)
        {
            string[] allDirectories = Directory.GetDirectories(templateDirectory, "*", SearchOption.AllDirectories);

            foreach (string dir in allDirectories)
            {
                string dirToCreate = dir.Replace(templateDirectory, newDirectory);
                Directory.CreateDirectory(dirToCreate);
            }

            string[] allFiles = Directory.GetFiles(templateDirectory, "*.*", SearchOption.AllDirectories);
            foreach (string newPath in allFiles)
            {
                File.Copy(newPath, newPath.Replace(templateDirectory, newDirectory));
            }
        }

        public void AddArchetype()
        {
            AddNewArchetypeWindow window = new(SelectedProject) { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();

            if (!window.SaveExit) return;

            ResearchArchetype newArchetype = window.NewArchetype;

            if (!DatabaseHelper.Insert(newArchetype))
            {
                MessageBox.Show("Failed to insert to database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Archetypes.Add(newArchetype);

            string[] projectFolders = Directory.GetDirectories($@"{projectRoot}\{SelectedProject.RootDirectory}\", "*", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < projectFolders.Length; i++)
            {
                string folder = projectFolders[i];

                if (Directory.Exists($@"{folder}\@archetype"))
                {
                    CopyAndRenameFolder($@"{folder}\@archetype", $@"{folder}\{newArchetype.Name}", archetype: newArchetype.Name);
                }
            }

            Projects.Find(x => x.Id == newArchetype.ProjectId)!.Archetypes.Add(newArchetype);

            Filter();

            if (FilteredProjects.Any(x => x.Id == newArchetype.ProjectId))
            {
                SelectedProject = FilteredProjects.Find(x => x.Id == newArchetype.ProjectId);
                OnPropertyChanged(nameof(SelectedProject));
            }
        }

        public void AddAttachment()
        {
            throw new NotImplementedException();
        }

        public void RemoveAttachment()
        {
            throw new NotImplementedException();
        }


        public void RemovePurchase()
        {
            throw new NotImplementedException();
        }

        public void RemoveArchetype()
        {
            throw new NotImplementedException();
        }

        public void SaveProject()
        {
            if (!DatabaseHelper.Update(SelectedProject))
            {
                MessageBox.Show("Failed to update the database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private static void CopyAndRenameFolder(string from, string to, string? archetype = null)
        {
            string[] allDirectories = Directory.GetDirectories(from, "*", SearchOption.AllDirectories);

            foreach (string dir in allDirectories)
            {
                string dirToCreate = dir.Replace(from, to);
                Directory.CreateDirectory(dirToCreate);
            }

            string[] allFiles = Directory.GetFiles(from, "*.*", SearchOption.AllDirectories);
            foreach (string newPath in allFiles)
            {
                string fileToWrite = newPath.Replace(from, to);

                if (Path.GetFileNameWithoutExtension(fileToWrite) == "archetype" && archetype is not null)
                {
                    string path = new FileInfo(fileToWrite).DirectoryName;
                    string newFileName = Path.GetFileName(fileToWrite).Replace("archetype", archetype);

                    fileToWrite = Path.Join(path, newFileName);
                }

                if (!File.Exists(fileToWrite))
                {
                    File.Copy(newPath, fileToWrite);
                }
            }
        }

        public void OpenRoot()
        {
            if (string.IsNullOrWhiteSpace(SelectedProject.RootDirectory))
            {
                if (!DirectoryFound(SelectedProject.ProjectCode))
                {
                    MessageBox.Show($"{SelectedProject.ProjectCode} could not be found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                OnPropertyChanged(nameof(SelectedProject));
            }


            string targetFolder = Path.Join(projectRoot, SelectedProject.RootDirectory);

            if (!Directory.Exists(targetFolder))
            {
                MessageBox.Show($"{SelectedProject.RootDirectory} could not be found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Process.Start(new ProcessStartInfo()
            {
                FileName = targetFolder,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private bool DirectoryFound(string code)
        {
            List<string> subdirectories = Directory.GetDirectories(projectRoot).ToList();
            subdirectories = subdirectories
                .Where(x => Path.GetFileName(x).ToUpper().StartsWith(code.ToUpper()))
                .ToList();

            if (subdirectories.Count > 0)
            {
                string dir = Path.GetFileName(subdirectories.First());
                SelectedProject.RootDirectory = dir;
                Projects.Find(x => x.Id == SelectedProject.Id)!.RootDirectory = dir;
                DatabaseHelper.Update(SelectedProject);
                return true;
            }

            return false;
        }

        public void AddPurchaseToProject()
        {
            AddResearchPurchaseWindow window = new(SelectedProject)
            {
                Owner = App.MainViewModel.MainWindow,
            };

            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }

            ResearchPurchase newPurchase = window.NewPurchase;

            if (!DatabaseHelper.Insert(newPurchase))
            {
                MessageBox.Show("Failed to insert to database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Projects.Find(x => x.Id == newPurchase.ProjectId)!.Purchases.Add(newPurchase);

            Filter();

            if (FilteredProjects.Any(x => x.Id == newPurchase.ProjectId))
            {
                SelectedProject = FilteredProjects.Find(x => x.Id == newPurchase.ProjectId);
                OnPropertyChanged(nameof(SelectedProject));
            }
        }
    }
}

