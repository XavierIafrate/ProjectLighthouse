using Microsoft.Win32;
using Model.Research;
using ProjectLighthouse;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Products;
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
using Windows.Storage.Pickers;

namespace ViewModel.Research
{
    public class ResearchViewModel : BaseViewModel
    {
        #region Data
        public List<ResearchProject> Projects { get; set; }
        public List<ResearchArchetype> Archetypes { get; set; }
        public List<Note> Notes { get; set; }
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


            Projects.Add(Window.NewProject);

            Filter();

            if (FilteredProjects.Any(x => x.Id == id))
            {
                SelectedProject = FilteredProjects.Find(x => x.Id == id);
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

            if(subdirectories.Count > 0)
            {
                SelectedProject.RootDirectory = Path.GetFileName(subdirectories.First());
                DatabaseHelper.Update(SelectedProject);
                return true;
            }

            return false;
        }
    }
}

