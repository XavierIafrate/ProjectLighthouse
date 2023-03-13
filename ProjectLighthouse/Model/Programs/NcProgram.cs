using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Programs
{
    public class NcProgram : BaseObject, IObjectWithValidation
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        private string name;
        [Unique]
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        [Unique]
        public string FilePath { get; set; }
        public string? Description { get; set; }

        public bool Inactive { get; set; }
        public string? SchedulingProgramName { get; set; }

        private string materials;
        public string Materials
        {
            get { return materials; }
            set 
            { 
                materials = value;
                OnPropertyChanged();
            }
        }

        private string machines;
        public string Machines
        {
            get { return machines; }
            set 
            { 
                machines = value;
                OnPropertyChanged();
            }
        }

        private string tags = "";
        public string Tags
        {
            get { return tags; }
            set
            {
                tags = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        public string SearchableTags
        {
            get
            {
                return Tags.Replace(" ", "").ToLowerInvariant();
            }
        }

        public List<string> TagsList
        {
            get
            {
                if (Tags == "") return new();
                return Tags.Split(";").OrderBy(x => x).ToList();
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? Id.ToString("0") : Name;
        }

        public void ValidateAll()
        {
            ValidateProperty(nameof(Name));
            ValidateProperty(nameof(Tags));
            ValidateProperty(nameof(TagsList));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(Name))
            {
                ClearErrors(propertyName);

                if (!ValidationHelper.StringIsNumbers(Name))
                {
                    AddError(propertyName, "Name must only contain numeric characters");
                }

                return;
            }
            else if (propertyName == nameof(Tags) || propertyName == nameof(TagsList))
            {
                ClearErrors(propertyName);

                List<string> tags = TagsList.Distinct().ToList();
                tags.ForEach(x => x = x.ToLower());

                if (tags.Count != TagsList.Count)
                {
                    AddError(propertyName, "Tags contains duplicates");
                }

                if (tags.Count > 10)
                {
                    AddError(propertyName, "Maximum 10 tags");
                }

                if (tags.Any(x => x.Length > 16))
                {
                    AddError(propertyName, "Maximum tag length is 16 characters");
                }

                if (tags.Any(x => x.Trim() != x))
                {
                    AddError(propertyName, "One or more tags has leading or trailing whitespace");
                }

                if (tags.Any(x => string.IsNullOrWhiteSpace(x)))
                {
                    AddError(propertyName, "One or more tags is empty");
                }

                return;
            }

            throw new System.NotImplementedException();
        }
    }
}
