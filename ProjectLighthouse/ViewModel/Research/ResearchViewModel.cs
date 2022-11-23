using Model.Research;
using ProjectLighthouse;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

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


        public ResearchViewModel()
        {
            LoadData();
            Filter();

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

            SendMessageCmd = new(this);

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
    }
}
