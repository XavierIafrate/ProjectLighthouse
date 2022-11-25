using DocumentFormat.OpenXml.InkML;
using Model.Research;
using ProjectLighthouse;
using ProjectLighthouse.Model.Core;
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

namespace ViewModel.Research
{
    public class ResearchViewModel : BaseViewModel
    {
        #region Data
        public List<ResearchProject> Projects { get; set; }
        public List<ResearchArchetype> Archetypes { get; set; }
        public List<Note> Notes { get; set; }
        public Array Stages { get; set; }

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

        #endregion

        public SendMessageCommand SendMessageCmd { get; set; }
        public AddProjectCommand AddProjectCmd { get; set; }
        public OpenRootDirectoryCommand OpenRootDirectoryCmd { get; set; }

        public NewProjectArchetypeCommand NewProjectArchetypeCmd { get; set; }

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
        }

        private void LoadData()
        {
            Projects = DatabaseHelper.Read<ResearchProject>().ToList();
            Archetypes = DatabaseHelper.Read<ResearchArchetype>().ToList();
            Notes = DatabaseHelper.Read<Note>().ToList();

            for (int i = 0; i < Projects.Count; i++)
            {
                Projects[i].Notes = Notes
                    .Where(x => x.DocumentReference == $"dev{Projects[i].Id}")
                    .ToList();

                Projects[i].Archetypes = Archetypes
                    .Where(x => x.ProjectId == Projects[i].Id)
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
                    .Where(x => x.ProjectName.Contains(userRequest) || x.ProjectCode.Contains(userRequest))
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

            //List<string> otherUsers = FilteredNotes.Select(x => x.SentBy).ToList();
            //otherUsers.AddRange(App.NotificationsManager.users.Where(x => x.HasPermission(PermissionType.ApproveRequest)).Select(x => x.UserName));
            //otherUsers.Add(App.NotificationsManager.users.Find(x => x.GetFullName() == SelectedRequest.RaisedBy).UserName);

            //otherUsers = otherUsers.Where(x => x != App.CurrentUser.UserName).Distinct().ToList();

            //for (int i = 0; i < otherUsers.Count; i++)
            //{
            //    Notification newNotification = new(otherUsers[i], App.CurrentUser.UserName, $"Comment: Request #{SelectedRequest.Id:0}", $"{App.CurrentUser.FirstName} left a comment: {NewMessage}");
            //    _ = DatabaseHelper.Insert(newNotification);
            //}

            Notes.Add(newNote);
            Projects.Find(x => x.Id == SelectedProject.Id)!.Notes.Add(newNote);
            int id = SelectedProject.Id;

            Filter();

            SelectedProject = null;
            SelectedProject = FilteredProjects.Find(x => x.Id == id);
            //LoadRequestCard(SelectedRequest);

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
            ResearchArchetype newArchetype = new()
            {
                Name = "New_archetype",
                ProjectId = SelectedProject.Id,
            };

            string[] projectFolders = Directory.GetDirectories($@"{projectRoot}\{SelectedProject.RootDirectory}\", "*", SearchOption.TopDirectoryOnly);

            for(int i = 0; i < projectFolders.Length; i++) 
            { 
                string folder = projectFolders[i];

                if (Directory.Exists($@"{folder}\@archetype"))
                {
                    CopyAndRenameFolder($@"{folder}\@archetype", $@"{folder}\{newArchetype.Name}");
                }
            }
        }

        private static void CopyAndRenameFolder(string from, string to)
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
                if (!File.Exists(newPath.Replace(from, to)))
                {
                    File.Copy(newPath, newPath.Replace(from, to));
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
                SelectedProject.RootDirectory = Path.GetFileName(subdirectories.First());
                DatabaseHelper.Update(SelectedProject);
                return true;
            }

            return false;
        }
    }
}

